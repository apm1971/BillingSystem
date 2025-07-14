using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb; // Changed from SQLite
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Data
{
    public class BillService
    {
        // Get all bills
        public static List<Bill> GetAllBills()
        {
            List<Bill> bills = new List<Bill>();
            
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            // Explicitly list all columns for MS Access GROUP BY
            string sql = @"SELECT b.BillID, b.BillNo, b.BillDate, b.DueDate, b.PartyID, b.PartyName, 
                    b.BrokerID, b.BrokerName, b.TotalAmount, b.TotalCharges, b.NetAmount, b.Notes,
                    IIF(SUM(pd.AllocatedAmount) IS NULL, 0, SUM(pd.AllocatedAmount)) as PaidAmount
                FROM BillMaster b
                LEFT JOIN PaymentDetails pd ON b.BillID = pd.BillID
                WHERE b.CompanyID = ?
                GROUP BY b.BillID, b.BillNo, b.BillDate, b.DueDate, b.PartyID, b.PartyName, 
                    b.BrokerID, b.BrokerName, b.TotalAmount, b.TotalCharges, b.NetAmount, b.Notes
                ORDER BY b.BillDate DESC";
            
            OleDbParameter param = new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID };
            
            DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
            
            foreach (DataRow row in dt.Rows)
            {
                Bill bill = new Bill
                {
                    BillID = Convert.ToInt32(row["BillID"]),
                    BillNo = row["BillNo"].ToString(),
                    BillDate = Convert.ToDateTime(row["BillDate"]),
                    DueDate = row["DueDate"] != DBNull.Value ? Convert.ToDateTime(row["DueDate"]) : Convert.ToDateTime(row["BillDate"]).AddDays(30),
                    PartyID = Convert.ToInt32(row["PartyID"]),
                    PartyName = row["PartyName"].ToString(),
                    BrokerID = row["BrokerID"] != DBNull.Value ? Convert.ToInt32(row["BrokerID"]) : null,
                    BrokerName = row["BrokerName"]?.ToString() ?? string.Empty,
                    TotalAmount = Convert.ToDouble(row["TotalAmount"]),
                    TotalCharges = Convert.ToDouble(row["TotalCharges"]),
                    NetAmount = Convert.ToDouble(row["NetAmount"]),
                    PaidAmount = Convert.ToDouble(row["PaidAmount"]),
                    Notes = row["Notes"]?.ToString() ?? string.Empty
                };
                
                // Get bill details
                bill.BillItems = GetBillDetails(bill.BillID);
                
                bills.Add(bill);
            }
            
            return bills;
        }
        
        // Generate a new bill number based on the current date and last bill number
        public static string GenerateNewBillNumber()
        {
            string prefix = DateTime.Now.ToString("yyMM");
            int nextNumber = 1;
            
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            // Get the last bill number with the same prefix
            string sql = "SELECT MAX(BillNo) FROM BillMaster WHERE BillNo LIKE ? AND CompanyID = ?";
            OleDbParameter[] parameters = {
                new OleDbParameter("BillNo", OleDbType.VarChar) { Value = prefix + "%" },
                new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID }
            };
            
            object result = DatabaseManager.ExecuteScalar(sql, parameters);
            
            if (result != null && result != DBNull.Value)
            {
                string lastBillNo = result.ToString();
                if (lastBillNo.Length >= 4 + 3) // prefix (4 chars) + number (at least 3 digits)
                {
                    string numberPart = lastBillNo.Substring(4);
                    if (int.TryParse(numberPart, out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }
            }
            
            // Format: YYMM + 3-digit sequential number
            return $"{prefix}{nextNumber:000}";
        }
        
        // Get bill details
        private static List<BillItem> GetBillDetails(int billID)
        {
            List<BillItem> billItems = new List<BillItem>();
            
            string sql = "SELECT * FROM BillDetails WHERE BillID = ?";
            OleDbParameter param = new OleDbParameter("BillID", OleDbType.Integer) { Value = billID };
            
            DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
            
            foreach (DataRow row in dt.Rows)
            {
                BillItem billItem = new BillItem
                {
                    BillDetailID = Convert.ToInt32(row["BillDetailID"]),
                    BillID = Convert.ToInt32(row["BillID"]),
                    ItemID = Convert.ToInt32(row["ItemID"]),
                    ItemName = row["ItemName"].ToString(),
                    Quantity = Convert.ToDouble(row["Quantity"]),
                    Rate = Convert.ToDouble(row["Rate"]),
                    Amount = Convert.ToDouble(row["Amount"]),
                    Charges = Convert.ToDouble(row["Charges"]),
                    TotalAmount = Convert.ToDouble(row["TotalAmount"])
                };
                
                billItems.Add(billItem);
            }
            
            return billItems;
        }
        
        // Get bill by ID
        public static Bill? GetBillByID(int billID)
        {
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            // Explicitly list all columns for MS Access GROUP BY
            string sql = @"SELECT b.BillID, b.BillNo, b.BillDate, b.DueDate, b.PartyID, b.PartyName, 
                    b.BrokerID, b.BrokerName, b.TotalAmount, b.TotalCharges, b.NetAmount, b.Notes,
                    IIF(SUM(pd.AllocatedAmount) IS NULL, 0, SUM(pd.AllocatedAmount)) as PaidAmount
                FROM BillMaster b
                LEFT JOIN PaymentDetails pd ON b.BillID = pd.BillID
                WHERE b.BillID = ? AND b.CompanyID = ?
                GROUP BY b.BillID, b.BillNo, b.BillDate, b.DueDate, b.PartyID, b.PartyName, 
                    b.BrokerID, b.BrokerName, b.TotalAmount, b.TotalCharges, b.NetAmount, b.Notes";
            
            OleDbParameter[] parameters = {
                new OleDbParameter("BillID", OleDbType.Integer) { Value = billID },
                new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID }
            };
            
            DataTable dt = DatabaseManager.ExecuteQuery(sql, parameters);
            
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                
                Bill bill = new Bill
                {
                    BillID = Convert.ToInt32(row["BillID"]),
                    BillNo = row["BillNo"].ToString(),
                    BillDate = Convert.ToDateTime(row["BillDate"]),
                    DueDate = row["DueDate"] != DBNull.Value ? Convert.ToDateTime(row["DueDate"]) : Convert.ToDateTime(row["BillDate"]).AddDays(30),
                    PartyID = Convert.ToInt32(row["PartyID"]),
                    PartyName = row["PartyName"].ToString(),
                    BrokerID = row["BrokerID"] != DBNull.Value ? Convert.ToInt32(row["BrokerID"]) : null,
                    BrokerName = row["BrokerName"]?.ToString() ?? string.Empty,
                    TotalAmount = Convert.ToDouble(row["TotalAmount"]),
                    TotalCharges = Convert.ToDouble(row["TotalCharges"]),
                    NetAmount = Convert.ToDouble(row["NetAmount"]),
                    PaidAmount = Convert.ToDouble(row["PaidAmount"]),
                    Notes = row["Notes"]?.ToString() ?? string.Empty
                };
                
                // Get bill details
                bill.BillItems = GetBillDetails(bill.BillID);
                
                return bill;
            }
            
            return null;
        }
        
        // Search bills
        public static List<Bill> SearchBills(string searchText)
        {
            List<Bill> bills = new List<Bill>();
            
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            // Explicitly list all columns for MS Access GROUP BY
            string sql = @"SELECT b.BillID, b.BillNo, b.BillDate, b.DueDate, b.PartyID, b.PartyName, 
                    b.BrokerID, b.BrokerName, b.TotalAmount, b.TotalCharges, b.NetAmount, b.Notes,
                    IIF(SUM(pd.AllocatedAmount) IS NULL, 0, SUM(pd.AllocatedAmount)) as PaidAmount
                FROM BillMaster b
                LEFT JOIN PaymentDetails pd ON b.BillID = pd.BillID
                WHERE (b.BillNo LIKE ? OR b.PartyName LIKE ? OR b.BrokerName LIKE ?)
                AND b.CompanyID = ?
                GROUP BY b.BillID, b.BillNo, b.BillDate, b.DueDate, b.PartyID, b.PartyName, 
                    b.BrokerID, b.BrokerName, b.TotalAmount, b.TotalCharges, b.NetAmount, b.Notes
                ORDER BY b.BillDate DESC";
            
            string param = "%" + searchText + "%";
            DataTable dt = DatabaseManager.ExecuteQuery(sql, 
                new OleDbParameter("BillNo", OleDbType.VarChar) { Value = param },
                new OleDbParameter("PartyName", OleDbType.VarChar) { Value = param },
                new OleDbParameter("BrokerName", OleDbType.VarChar) { Value = param },
                new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID });
                
            foreach (DataRow row in dt.Rows)
            {
                Bill bill = new Bill
                {
                    BillID = Convert.ToInt32(row["BillID"]),
                    BillNo = row["BillNo"].ToString(),
                    BillDate = Convert.ToDateTime(row["BillDate"]),
                    DueDate = row["DueDate"] != DBNull.Value ? Convert.ToDateTime(row["DueDate"]) : Convert.ToDateTime(row["BillDate"]).AddDays(30),
                    PartyID = Convert.ToInt32(row["PartyID"]),
                    PartyName = row["PartyName"].ToString(),
                    BrokerID = row["BrokerID"] != DBNull.Value ? Convert.ToInt32(row["BrokerID"]) : null,
                    BrokerName = row["BrokerName"]?.ToString() ?? string.Empty,
                    TotalAmount = Convert.ToDouble(row["TotalAmount"]),
                    TotalCharges = Convert.ToDouble(row["TotalCharges"]),
                    NetAmount = Convert.ToDouble(row["NetAmount"]),
                    PaidAmount = Convert.ToDouble(row["PaidAmount"]),
                    Notes = row["Notes"]?.ToString() ?? string.Empty
                };
                
                // Get bill details
                bill.BillItems = GetBillDetails(bill.BillID);
                
                bills.Add(bill);
            }
            
            return bills;
        }
        
        // Private helper methods for transaction support
        private static int ExecuteNonQuery(OleDbConnection conn, OleDbTransaction transaction, string sql, params OleDbParameter[] parameters)
        {
            using (OleDbCommand cmd = new OleDbCommand(sql, conn, transaction))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                    
                return cmd.ExecuteNonQuery();
            }
        }
        
        private static object ExecuteScalar(OleDbConnection conn, OleDbTransaction transaction, string sql, params OleDbParameter[] parameters)
        {
            using (OleDbCommand cmd = new OleDbCommand(sql, conn, transaction))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                    
                return cmd.ExecuteScalar();
            }
        }
        
        // Add a new bill
        public static bool SaveBill(Bill bill, out int billID)
        {
            billID = 0;
            
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            using (OleDbConnection conn = DatabaseManager.GetConnection())
            {
                conn.Open();
                OleDbTransaction transaction = conn.BeginTransaction();
                
                try
                {
                    // Check if bill already exists
                    if (bill.BillID == 0)
                    {
                        // Insert new bill
                        string insertSql = @"
                            INSERT INTO BillMaster 
                            (BillNo, BillDate, DueDate, PartyID, PartyName, BrokerID, BrokerName, 
                            TotalAmount, TotalCharges, NetAmount, Notes, CompanyID) 
                            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
                            
                        using (OleDbCommand cmd = new OleDbCommand(insertSql, conn))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("BillNo", bill.BillNo);
                            
                            OleDbParameter billDateParam = new OleDbParameter("BillDate", OleDbType.Date);
                            billDateParam.Value = bill.BillDate;
                            cmd.Parameters.Add(billDateParam);
                            
                            OleDbParameter dueDateParam = new OleDbParameter("DueDate", OleDbType.Date);
                            dueDateParam.Value = bill.DueDate;
                            cmd.Parameters.Add(dueDateParam);
                            
                            cmd.Parameters.AddWithValue("PartyID", bill.PartyID);
                            cmd.Parameters.AddWithValue("PartyName", bill.PartyName);
                            
                            if (bill.BrokerID.HasValue)
                            {
                                cmd.Parameters.AddWithValue("BrokerID", bill.BrokerID.Value);
                                cmd.Parameters.AddWithValue("BrokerName", bill.BrokerName);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("BrokerID", DBNull.Value);
                                cmd.Parameters.AddWithValue("BrokerName", DBNull.Value);
                            }
                            
                            cmd.Parameters.AddWithValue("TotalAmount", bill.TotalAmount);
                            cmd.Parameters.AddWithValue("TotalCharges", bill.TotalCharges);
                            cmd.Parameters.AddWithValue("NetAmount", bill.NetAmount);
                            cmd.Parameters.AddWithValue("Notes", bill.Notes ?? string.Empty);
                            cmd.Parameters.AddWithValue("CompanyID", companyID);
                            
                            cmd.ExecuteNonQuery();
                        }
                        
                        // Get the newly inserted bill ID
                        string getIdSql = "SELECT @@Identity";
                        billID = Convert.ToInt32(ExecuteScalar(conn, transaction, getIdSql));
                        bill.BillID = billID;
                    }
                    else
                    {
                        // Update existing bill
                        string updateSql = @"
                            UPDATE BillMaster 
                            SET BillNo = ?, BillDate = ?, DueDate = ?, PartyID = ?, PartyName = ?, 
                                BrokerID = ?, BrokerName = ?, TotalAmount = ?, TotalCharges = ?, 
                                NetAmount = ?, Notes = ?, CompanyID = ?
                            WHERE BillID = ?";
                            
                        using (OleDbCommand cmd = new OleDbCommand(updateSql, conn))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("BillNo", bill.BillNo);
                            
                            OleDbParameter billDateParam = new OleDbParameter("BillDate", OleDbType.Date);
                            billDateParam.Value = bill.BillDate;
                            cmd.Parameters.Add(billDateParam);
                            
                            OleDbParameter dueDateParam = new OleDbParameter("DueDate", OleDbType.Date);
                            dueDateParam.Value = bill.DueDate;
                            cmd.Parameters.Add(dueDateParam);
                            
                            cmd.Parameters.AddWithValue("PartyID", bill.PartyID);
                            cmd.Parameters.AddWithValue("PartyName", bill.PartyName);
                            
                            if (bill.BrokerID.HasValue)
                            {
                                cmd.Parameters.AddWithValue("BrokerID", bill.BrokerID.Value);
                                cmd.Parameters.AddWithValue("BrokerName", bill.BrokerName);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("BrokerID", DBNull.Value);
                                cmd.Parameters.AddWithValue("BrokerName", DBNull.Value);
                            }
                            
                            cmd.Parameters.AddWithValue("TotalAmount", bill.TotalAmount);
                            cmd.Parameters.AddWithValue("TotalCharges", bill.TotalCharges);
                            cmd.Parameters.AddWithValue("NetAmount", bill.NetAmount);
                            cmd.Parameters.AddWithValue("Notes", bill.Notes ?? string.Empty);
                            cmd.Parameters.AddWithValue("CompanyID", companyID);
                            cmd.Parameters.AddWithValue("BillID", bill.BillID);
                            
                            cmd.ExecuteNonQuery();
                        }
                        
                        // Delete existing bill details
                        string deleteBillDetailsSql = "DELETE FROM BillDetails WHERE BillID = ?";
                        ExecuteNonQuery(conn, transaction, deleteBillDetailsSql, 
                            new OleDbParameter("BillID", OleDbType.Integer) { Value = billID });
                    }
                    
                    // Insert bill details
                    foreach (BillItem item in bill.BillItems)
                    {
                        string insertBillDetailsSql = @"
                            INSERT INTO BillDetails (
                                BillID, ItemID, ItemName, Quantity, Rate, Amount, Charges, TotalAmount
                            ) VALUES (?, ?, ?, ?, ?, ?, ?, ?)";
                        
                        OleDbParameter[] itemParams = {
                            new OleDbParameter("BillID", OleDbType.Integer) { Value = billID },
                            new OleDbParameter("ItemID", OleDbType.Integer) { Value = item.ItemID },
                            new OleDbParameter("ItemName", OleDbType.VarChar) { Value = item.ItemName },
                            new OleDbParameter("Quantity", OleDbType.Double) { Value = item.Quantity },
                            new OleDbParameter("Rate", OleDbType.Currency) { Value = item.Rate },
                            new OleDbParameter("Amount", OleDbType.Currency) { Value = item.Amount },
                            new OleDbParameter("Charges", OleDbType.Currency) { Value = item.Charges },
                            new OleDbParameter("TotalAmount", OleDbType.Currency) { Value = item.TotalAmount }
                        };
                        
                        ExecuteNonQuery(conn, transaction, insertBillDetailsSql, itemParams);
                    }
                    
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    System.Windows.Forms.MessageBox.Show($"Error saving bill: {ex.Message}", "Bill Error", 
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return false;
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                        conn.Close();
                }
            }
        }
        
        // Delete a bill
        public static bool DeleteBill(int billID)
        {
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            // Check if bill has payment records
            string checkSql = @"SELECT COUNT(*) FROM PaymentDetails pd 
                               INNER JOIN BillMaster bm ON pd.BillID = bm.BillID 
                               WHERE pd.BillID = ? AND bm.CompanyID = ?";
            OleDbParameter[] checkParams = {
                new OleDbParameter("BillID", OleDbType.Integer) { Value = billID },
                new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID }
            };
            
            int paymentCount = Convert.ToInt32(DatabaseManager.ExecuteScalar(checkSql, checkParams));
            
            if (paymentCount > 0)
            {
                // Bill has payment records, cannot delete
                return false;
            }
            
            using (OleDbConnection conn = DatabaseManager.GetConnection())
            {
                conn.Open();
                OleDbTransaction transaction = conn.BeginTransaction();
                
                try
                {
                    // Delete bill details
                    string deleteBillDetailsSql = "DELETE FROM BillDetails WHERE BillID = ?";
                    ExecuteNonQuery(conn, transaction, deleteBillDetailsSql, 
                        new OleDbParameter("BillID", OleDbType.Integer) { Value = billID });
                    
                    // Delete bill master
                    string deleteBillMasterSql = "DELETE FROM BillMaster WHERE BillID = ? AND CompanyID = ?";
                    ExecuteNonQuery(conn, transaction, deleteBillMasterSql, 
                        new OleDbParameter("BillID", OleDbType.Integer) { Value = billID },
                        new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID });
                    
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }
    }
}