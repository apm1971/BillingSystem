using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;
using SaleBillSystem.NET.Models; // Assuming your Bill and BillItem models are here

namespace SaleBillSystem.NET.Data
{
    /// <summary>
    /// Provides data services for creating, reading, updating, and deleting bills.
    /// </summary>
    public static class BillService
    {
        /// <summary>
        /// Saves a bill and its details. Handles both creating new bills and updating existing ones.
        /// This is a transactional operation.
        /// </summary>
        public static bool SaveBill(Bill bill, out int savedBillId)
        {
            savedBillId = bill.BillID;
            using (var conn = DatabaseManager.GetConnection())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();

                try
                {
                    // Step 1: Save the BillMaster record
                    if (bill.BillID == 0) // Create new bill
                    {
                        string insertSql = @"
                            INSERT INTO BillMaster (BillNo, BillDate, PartyID, BrokerID, BrokerName, OriginalAmount, AdditionalCharges, Status, Notes, CompanyID) 
                            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
                        
                        var parameters = new OleDbParameter[]
                        {
                            new OleDbParameter("BillNo", bill.BillNo),
                            new OleDbParameter("BillDate", bill.BillDate),
                            new OleDbParameter("PartyID", bill.PartyID),
                            new OleDbParameter("BrokerID", bill.BrokerID ?? (object)DBNull.Value),
                            new OleDbParameter("BrokerName", bill.BrokerName ?? (object)DBNull.Value),
                            new OleDbParameter("OriginalAmount", bill.OriginalAmount),
                            new OleDbParameter("AdditionalCharges", bill.AdditionalCharges),
                            new OleDbParameter("Status", bill.Status),
                            new OleDbParameter("Notes", bill.Notes ?? (object)DBNull.Value),
                            new OleDbParameter("CompanyID", bill.CompanyID)
                        };
                        ExecuteNonQuery(conn, transaction, insertSql, parameters);

                        // Get the ID of the new bill
                        savedBillId = Convert.ToInt32(ExecuteScalar(conn, transaction, "SELECT @@Identity"));
                    }
                    else // Update existing bill
                    {
                        string updateSql = @"
                            UPDATE BillMaster SET BillNo = ?, BillDate = ?, PartyID = ?, BrokerID = ?, BrokerName = ?, 
                            OriginalAmount = ?, AdditionalCharges = ?, Status = ?, Notes = ?
                            WHERE BillID = ?";
                        
                        var parameters = new OleDbParameter[]
                        {
                            new OleDbParameter("BillNo", bill.BillNo),
                            new OleDbParameter("BillDate", bill.BillDate),
                            new OleDbParameter("PartyID", bill.PartyID),
                            new OleDbParameter("BrokerID", bill.BrokerID ?? (object)DBNull.Value),
                            new OleDbParameter("BrokerName", bill.BrokerName ?? (object)DBNull.Value),
                            new OleDbParameter("OriginalAmount", bill.OriginalAmount),
                            new OleDbParameter("AdditionalCharges", bill.AdditionalCharges),
                            new OleDbParameter("Status", bill.Status),
                            new OleDbParameter("Notes", bill.Notes ?? (object)DBNull.Value),
                            new OleDbParameter("BillID", bill.BillID)
                        };
                        ExecuteNonQuery(conn, transaction, updateSql, parameters);

                        // Delete old bill details before inserting new ones
                        ExecuteNonQuery(conn, transaction, "DELETE FROM BillDetails WHERE BillID = ?", new OleDbParameter("BillID", bill.BillID));
                    }

                    // Step 2: Insert the BillDetails records
                    foreach (var item in bill.BillItems)
                    {
                        // Check if the new columns exist in the database
                        bool hasChargesColumn = false;
                        bool hasTotalAmountColumn = false;
                        
                        try
                        {
                            ExecuteScalar(conn, transaction, "SELECT TOP 1 Charges FROM BillDetails");
                            hasChargesColumn = true;
                        }
                        catch { /* Column doesn't exist */ }
                        
                        try
                        {
                            ExecuteScalar(conn, transaction, "SELECT TOP 1 TotalAmount FROM BillDetails");
                            hasTotalAmountColumn = true;
                        }
                        catch { /* Column doesn't exist */ }
                        
                        string detailSql;
                        OleDbParameter[] itemParams;
                        
                        if (hasChargesColumn && hasTotalAmountColumn)
                        {
                            // Use new schema with all columns
                            detailSql = @"
                                INSERT INTO BillDetails (BillID, ItemID, ItemName, Quantity, Rate, Amount, Charges, TotalAmount, CompanyID) 
                                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)";
                            itemParams = new OleDbParameter[]
                            {
                                new OleDbParameter("BillID", savedBillId),
                                new OleDbParameter("ItemID", item.ItemID),
                                new OleDbParameter("ItemName", item.ItemName),
                                new OleDbParameter("Quantity", item.Quantity),
                                new OleDbParameter("Rate", item.Rate),
                                new OleDbParameter("Amount", item.Amount),
                                new OleDbParameter("Charges", item.Charges),
                                new OleDbParameter("TotalAmount", item.TotalAmount),
                                new OleDbParameter("CompanyID", bill.CompanyID)
                            };
                        }
                        else
                        {
                            // Use old schema without Charges and TotalAmount columns
                            detailSql = @"
                                INSERT INTO BillDetails (BillID, ItemID, ItemName, Quantity, Rate, Amount, CompanyID) 
                                VALUES (?, ?, ?, ?, ?, ?, ?)";
                            itemParams = new OleDbParameter[]
                            {
                                new OleDbParameter("BillID", savedBillId),
                                new OleDbParameter("ItemID", item.ItemID),
                                new OleDbParameter("ItemName", item.ItemName),
                                new OleDbParameter("Quantity", item.Quantity),
                                new OleDbParameter("Rate", item.Rate),
                                new OleDbParameter("Amount", item.Amount),
                                new OleDbParameter("CompanyID", bill.CompanyID)
                            };
                        }
                        
                        ExecuteNonQuery(conn, transaction, detailSql, itemParams);
                    }

                    // Step 3: Create or Update the corresponding ledger entry for the bill itself
                    decimal totalBillAmount = bill.OriginalAmount + bill.AdditionalCharges;
                    string ledgerCheckSql = "SELECT COUNT(*) FROM TransactionLedger WHERE BillID = ? AND TransactionType = 'Bill'";
                    int ledgerEntryCount = Convert.ToInt32(ExecuteScalar(conn, transaction, ledgerCheckSql, new OleDbParameter("BillID", savedBillId)));

                    if (ledgerEntryCount > 0)
                    {
                        // Update existing ledger entry
                        string ledgerUpdateSql = "UPDATE TransactionLedger SET DebitAmount = ?, TransactionDate = ?, PartyID = ? WHERE BillID = ? AND TransactionType = 'Bill'";
                        var ledgerParams = new OleDbParameter[]
                        {
                            new OleDbParameter("DebitAmount", totalBillAmount),
                            new OleDbParameter("TransactionDate", bill.BillDate),
                            new OleDbParameter("PartyID", bill.PartyID),
                            new OleDbParameter("BillID", savedBillId)
                        };
                        ExecuteNonQuery(conn, transaction, ledgerUpdateSql, ledgerParams);
                    }
                    else
                    {
                        // Insert new ledger entry
                        string ledgerInsertSql = @"
                            INSERT INTO TransactionLedger (PartyID, BillID, TransactionDate, TransactionType, Description, DebitAmount, CompanyID)
                            VALUES (?, ?, ?, 'Bill', ?, ?, ?)";
                        var ledgerParams = new OleDbParameter[]
                        {
                            new OleDbParameter("PartyID", bill.PartyID),
                            new OleDbParameter("BillID", savedBillId),
                            new OleDbParameter("TransactionDate", bill.BillDate),
                            new OleDbParameter("Description", $"Bill No: {bill.BillNo}"),
                            new OleDbParameter("DebitAmount", totalBillAmount),
                            new OleDbParameter("CompanyID", bill.CompanyID)
                        };
                        ExecuteNonQuery(conn, transaction, ledgerInsertSql, ledgerParams);
                    }
                    
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"Error saving bill: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets a single bill by its ID, including its line items.
        /// </summary>
        public static Bill GetBillByID(int billId)
        {
            string sql = "SELECT * FROM BillMaster WHERE BillID = ?";
            var param = new OleDbParameter("BillID", billId);
            DataTable dt = DatabaseManager.ExecuteQuery(sql, param);

            if (dt.Rows.Count > 0)
            {
                var bill = MapRowToBill(dt.Rows[0]);
                bill.BillItems = GetBillDetails(bill.BillID);
                return bill;
            }
            return null;
        }

        /// <summary>
        /// Gets all bills for a specific party.
        /// </summary>
        public static List<Bill> GetAllBillsForParty(int partyId)
        {
            var bills = new List<Bill>();
            string sql = "SELECT * FROM BillMaster WHERE PartyID = ? ORDER BY BillDate DESC";
            var param = new OleDbParameter("PartyID", partyId);

            DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
            foreach (DataRow row in dt.Rows)
            {
                bills.Add(MapRowToBill(row));
            }
            return bills;
        }

        /// <summary>
        /// Gets all bills with their related party information.
        /// </summary>
        public static List<Bill> GetAllBills()
        {
            var bills = new List<Bill>();
            string sql = @"
                SELECT b.*, p.PartyName 
                FROM BillMaster b 
                LEFT JOIN PartyMaster p ON b.PartyID = p.PartyID 
                ORDER BY b.BillDate DESC";

            DataTable dt = DatabaseManager.ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                var bill = MapRowToBill(row);
                // Set the party name from the joined query
                bill.PartyName = row["PartyName"]?.ToString() ?? "";
                bills.Add(bill);
            }
            return bills;
        }

        /// <summary>
        /// Checks if a party has any bills linked to it.
        /// </summary>
        /// <param name="partyId">The ID of the party to check</param>
        /// <returns>True if the party has bills, false otherwise</returns>
        public static bool HasBillsForParty(int partyId)
        {
            try
            {
                string sql = "SELECT COUNT(*) FROM BillMaster WHERE PartyID = ?";
                var param = new OleDbParameter("PartyID", partyId);
                object result = DatabaseManager.ExecuteScalar(sql, param);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Calculates the current outstanding balance for a specific bill by querying the ledger.
        /// </summary>
        public static decimal GetBillBalance(int billId)
        {
            string sql = "SELECT SUM(DebitAmount) - SUM(CreditAmount) FROM TransactionLedger WHERE BillID = ?";
            var param = new OleDbParameter("BillID", billId);
            object result = DatabaseManager.ExecuteScalar(sql, param);
            return result != DBNull.Value ? Convert.ToDecimal(result) : 0m;
        }

        /// <summary>
        /// Deletes a bill and all its related data (details and ledger entries).
        /// </summary>
        public static bool DeleteBill(int billId)
        {
            // Check if any payments or adjustments have been made against this bill.
            string checkSql = "SELECT COUNT(*) FROM TransactionLedger WHERE BillID = ? AND TransactionType <> 'Bill'";
            var checkParam = new OleDbParameter("BillID", billId);
            int transactionCount = Convert.ToInt32(DatabaseManager.ExecuteScalar(checkSql, checkParam));

            if (transactionCount > 0)
            {
                MessageBox.Show("Cannot delete this bill because payments or adjustments have already been recorded against it.", 
                    "Deletion Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            using (var conn = DatabaseManager.GetConnection())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                try
                {
                    // Create separate parameter objects for each query to avoid reuse issues
                    var param1 = new OleDbParameter("BillID", billId);
                    var param2 = new OleDbParameter("BillID", billId);
                    var param3 = new OleDbParameter("BillID", billId);
                    
                    // Delete from all three tables
                    ExecuteNonQuery(conn, transaction, "DELETE FROM TransactionLedger WHERE BillID = ?", param1);
                    ExecuteNonQuery(conn, transaction, "DELETE FROM BillDetails WHERE BillID = ?", param2);
                    ExecuteNonQuery(conn, transaction, "DELETE FROM BillMaster WHERE BillID = ?", param3);

                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"Error deleting bill: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        /// <summary>
        /// Generates a new bill number based on the current financial year and sequence
        /// </summary>
        public static string GenerateNewBillNumber()
        {
            try
            {
                // Get the current financial year
                int currentYear = DateTime.Now.Year;
                int financialYear = DateTime.Now.Month >= 4 ? currentYear : currentYear - 1;
                string yearSuffix = financialYear.ToString().Substring(2); // Last 2 digits
                
                // Get the active company ID
                int companyID = Program.ActiveCompany?.CompanyID ?? 1;
                
                // Find the highest bill number for this financial year and company
                string sql = @"SELECT MAX(BillNo) FROM BillMaster 
                              WHERE BillNo LIKE ? AND CompanyID = ?";
                
                string pattern = $"BILL-{yearSuffix}-%";
                var parameters = new OleDbParameter[]
                {
                    new OleDbParameter("Pattern", pattern),
                    new OleDbParameter("CompanyID", companyID)
                };
                
                object result = DatabaseManager.ExecuteScalar(sql, parameters);
                
                int nextNumber = 1;
                if (result != null && result != DBNull.Value)
                {
                    string lastBillNo = result.ToString();
                    // Extract the number part from the last bill number
                    if (lastBillNo.Contains("-"))
                    {
                        string numberPart = lastBillNo.Split('-').Last();
                        if (int.TryParse(numberPart, out int lastNumber))
                        {
                            nextNumber = lastNumber + 1;
                        }
                    }
                }
                
                return $"BILL-{yearSuffix}-{nextNumber:D4}";
            }
            catch (Exception ex)
            {
                // Fallback to a simple timestamp-based number
                return $"BILL-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmmss}";
            }
        }

        #region == Helper Methods ==

        private static List<BillItem> GetBillDetails(int billId)
        {
            var items = new List<BillItem>();
            string sql = "SELECT * FROM BillDetails WHERE BillID = ?";
            var param = new OleDbParameter("BillID", billId);
            DataTable dt = DatabaseManager.ExecuteQuery(sql, param);

            foreach (DataRow row in dt.Rows)
            {
                var billItem = new BillItem
                {
                    BillDetailID = Convert.ToInt32(row["BillDetailID"]),
                    BillID = Convert.ToInt32(row["BillID"]),
                    ItemID = Convert.ToInt32(row["ItemID"]),
                    ItemName = row["ItemName"].ToString(),
                    Quantity = Convert.ToDouble(row["Quantity"]),
                    Rate = Convert.ToDecimal(row["Rate"]),
                    Amount = Convert.ToDecimal(row["Amount"]),
                    CompanyID = Convert.ToInt32(row["CompanyID"])
                };
                
                // Handle Charges field (may not exist in older database schemas)
                if (dt.Columns.Contains("Charges"))
                {
                    billItem.Charges = Convert.ToDecimal(row["Charges"] ?? 0);
                }
                else
                {
                    billItem.Charges = 0;
                }
                
                // Handle TotalAmount field (may not exist in older database schemas)
                if (dt.Columns.Contains("TotalAmount"))
                {
                    billItem.TotalAmount = Convert.ToDecimal(row["TotalAmount"] ?? 0);
                }
                else
                {
                    // Calculate TotalAmount if not in database
                    billItem.TotalAmount = billItem.Amount + billItem.Charges;
                }
                
                items.Add(billItem);
            }
            return items;
        }
        public static void UpdateStatus(int billId, string status)
        {
            string sql = "UPDATE BillMaster SET Status = ? WHERE BillID = ?";
            var parameters = new OleDbParameter[]
            {
                new OleDbParameter("Status", status),
                new OleDbParameter("BillID", billId)
            };
            DatabaseManager.ExecuteNonQuery(sql, parameters);
        }
        private static Bill MapRowToBill(DataRow row)
        {
            return new Bill
            {
                BillID = Convert.ToInt32(row["BillID"]),
                BillNo = row["BillNo"].ToString(),
                BillDate = Convert.ToDateTime(row["BillDate"]),
                PartyID = Convert.ToInt32(row["PartyID"]),
                BrokerID = row["BrokerID"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["BrokerID"]),
                BrokerName = row["BrokerName"].ToString(),
                OriginalAmount = Convert.ToDecimal(row["OriginalAmount"]),
                AdditionalCharges = Convert.ToDecimal(row["AdditionalCharges"]),
                Status = row["Status"].ToString(),
                Notes = row["Notes"].ToString(),
                CompanyID = Convert.ToInt32(row["CompanyID"])
            };
        }

        private static int ExecuteNonQuery(IDbConnection conn, IDbTransaction transaction, string sql, params IDbDataParameter[] parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.Transaction = transaction;
                foreach (var p in parameters) cmd.Parameters.Add(p);
                return cmd.ExecuteNonQuery();
            }
        }

        private static object ExecuteScalar(IDbConnection conn, IDbTransaction transaction, string sql, params IDbDataParameter[] parameters)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.Transaction = transaction;
                foreach (var p in parameters) cmd.Parameters.Add(p);
                return cmd.ExecuteScalar();
            }
        }

        #endregion
    }
}