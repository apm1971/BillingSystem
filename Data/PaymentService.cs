using System;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Data
{
    /// <summary>
    /// Manages saving and retrieving master payment records (vouchers).
    /// </summary>
    public static class PaymentService
    {
        /// <summary>
        /// Saves a new PaymentMaster record within an existing database transaction and returns the new ID.
        /// </summary>
        public static int SavePaymentMaster(PaymentMaster payment, OleDbConnection conn, OleDbTransaction trans)
        {
            try
            {
                string sql = @"
                    INSERT INTO PaymentMaster (PartyID, BrokerID, PaymentDate, TotalAmountPaid, PaymentMethod, Reference, CompanyID, ChequeAmountFirm1, ChequeAmountFirm2, AdvanceUsed, AdvanceAmount, IsAdvancePayment)
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                var parameters = new OleDbParameter[]
                {
                    new OleDbParameter("PartyID", payment.PartyID),
                    new OleDbParameter("BrokerID", payment.BrokerID ?? (object)DBNull.Value),
                    new OleDbParameter("PaymentDate", payment.PaymentDate),
                    new OleDbParameter("TotalAmountPaid", payment.TotalAmountPaid),
                    new OleDbParameter("PaymentMethod", payment.PaymentMethod ?? (object)DBNull.Value),
                    new OleDbParameter("Reference", payment.Reference ?? (object)DBNull.Value),
                    new OleDbParameter("CompanyID", payment.CompanyID),
                    new OleDbParameter("ChequeAmountFirm1", payment.ChequeAmountFirm1),
                    new OleDbParameter("ChequeAmountFirm2", payment.ChequeAmountFirm2),
                    new OleDbParameter("AdvanceUsed", payment.AdvanceUsed),
                    new OleDbParameter("AdvanceAmount", payment.AdvanceAmount),
                    new OleDbParameter("IsAdvancePayment", payment.IsAdvancePayment)
                };

                using (var cmd = new OleDbCommand(sql, conn, trans))
                {
                    cmd.Parameters.AddRange(parameters);
                    cmd.ExecuteNonQuery();
                }

                // Get the ID of the newly inserted payment record
                using (var cmd = new OleDbCommand("SELECT @@Identity", conn, trans))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving payment master record: {ex.Message}", "Payment Service Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Re-throw the exception to ensure the transaction is rolled back
                throw;
            }
        }

        // In Data/PaymentService.cs

/// <summary>
/// Gets a list of all payments with their associated party names for display.
/// </summary>
     public static List<PaymentViewModel> GetAllPaymentsForDisplay(int companyId)
{
    var payments = new List<PaymentViewModel>();
    // Simple query without JOINs to avoid Access syntax issues
    string sql = @"
        SELECT PaymentID, PaymentDate, TotalAmountPaid, PaymentMethod, Reference, PartyID, BrokerID, ChequeAmountFirm1, ChequeAmountFirm2
        FROM PaymentMaster
        WHERE CompanyID = ?
        ORDER BY PaymentDate DESC";
    
    var param = new OleDbParameter("CompanyID", companyId);
    try
    {
        DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
        foreach (DataRow row in dt.Rows)
        {
            // Get party name separately


            int partyId = Convert.ToInt32(row["PartyID"]);
            string partyName = PartyService.GetPartyByID(partyId)?.PartyName ?? string.Empty;
            // You'll need to create a method to get party name by ID
            // partyName = PartyService.GetPartyNameById(partyId);
            
            // For now, we'll leave it empty or you can add the party lookup
            // Get broker name separately
            string brokerName = string.Empty;
            if (row["BrokerID"] != DBNull.Value)
            {
                var broker = BrokerService.GetBrokerByID(Convert.ToInt32(row["BrokerID"]));
                brokerName = broker?.BrokerName ?? string.Empty;
            }
            
            payments.Add(new PaymentViewModel
            {
                PaymentID = Convert.ToInt32(row["PaymentID"]),
                PaymentDate = Convert.ToDateTime(row["PaymentDate"]),
                TotalAmountPaid = Convert.ToDecimal(row["TotalAmountPaid"]),
                PaymentMethod = row["PaymentMethod"]?.ToString() ?? string.Empty,
                Reference = row["Reference"]?.ToString() ?? string.Empty,
                PartyID = partyId,
                PartyName = partyName, // You'll need to implement party lookup
                BrokerID = row["BrokerID"] != DBNull.Value ? Convert.ToInt32(row["BrokerID"]) : (int?)null,
                BrokerName = brokerName,
                ChequeAmountFirm1 = row["ChequeAmountFirm1"] != DBNull.Value ? Convert.ToDecimal(row["ChequeAmountFirm1"]) : 0,
                ChequeAmountFirm2 = row["ChequeAmountFirm2"] != DBNull.Value ? Convert.ToDecimal(row["ChequeAmountFirm2"]) : 0
            });
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error loading payments: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    return payments;
}

/// <summary>
/// Gets a list of payments within the specified date range.
/// </summary>
public static List<PaymentViewModel> GetPaymentsInDateRange(int companyId, DateTime fromDate, DateTime toDate)
{
    var payments = new List<PaymentViewModel>();
    
    // Adjust toDate to include the entire day
    toDate = toDate.Date.AddDays(1).AddSeconds(-1);
    
    string sql = @"
        SELECT PaymentID, PaymentDate, TotalAmountPaid, PaymentMethod, Reference, PartyID, BrokerID, ChequeAmountFirm1, ChequeAmountFirm2
        FROM PaymentMaster
        WHERE CompanyID = ? AND PaymentDate >= ? AND PaymentDate <= ?
        ORDER BY PaymentDate DESC";
    
    var parameters = new OleDbParameter[] {
        new OleDbParameter("CompanyID", companyId),
        new OleDbParameter("FromDate", fromDate.Date),
        new OleDbParameter("ToDate", toDate)
    };
    
    try
    {
        DataTable dt = DatabaseManager.ExecuteQuery(sql, parameters);
        foreach (DataRow row in dt.Rows)
        {
            int partyId = Convert.ToInt32(row["PartyID"]);
            string partyName = PartyService.GetPartyByID(partyId)?.PartyName ?? string.Empty;
            
            string brokerName = string.Empty;
            if (row["BrokerID"] != DBNull.Value)
            {
                var broker = BrokerService.GetBrokerByID(Convert.ToInt32(row["BrokerID"]));
                brokerName = broker?.BrokerName ?? string.Empty;
            }
            
            payments.Add(new PaymentViewModel
            {
                PaymentID = Convert.ToInt32(row["PaymentID"]),
                PaymentDate = Convert.ToDateTime(row["PaymentDate"]),
                TotalAmountPaid = Convert.ToDecimal(row["TotalAmountPaid"]),
                PaymentMethod = row["PaymentMethod"]?.ToString() ?? string.Empty,
                Reference = row["Reference"]?.ToString() ?? string.Empty,
                PartyID = partyId,
                PartyName = partyName,
                BrokerID = row["BrokerID"] != DBNull.Value ? Convert.ToInt32(row["BrokerID"]) : (int?)null,
                BrokerName = brokerName,
                ChequeAmountFirm1 = row["ChequeAmountFirm1"] != DBNull.Value ? Convert.ToDecimal(row["ChequeAmountFirm1"]) : 0,
                ChequeAmountFirm2 = row["ChequeAmountFirm2"] != DBNull.Value ? Convert.ToDecimal(row["ChequeAmountFirm2"]) : 0
            });
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error loading payments: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    return payments;
}

public static PaymentViewModel? GetPaymentById(int paymentId)
{
    try
    {
        string sql = @"
            SELECT PaymentID, PaymentDate, TotalAmountPaid, PaymentMethod, Reference, PartyID, BrokerID, ChequeAmountFirm1, ChequeAmountFirm2
            FROM PaymentMaster
            WHERE PaymentID = ?";
        
        var param = new OleDbParameter("PaymentID", paymentId);
        DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
        
        if (dt.Rows.Count > 0)
        {
            var row = dt.Rows[0];
            
            // Get party name separately
            int partyId = Convert.ToInt32(row["PartyID"]);
            string partyName = PartyService.GetPartyByID(partyId)?.PartyName ?? string.Empty;
            
            // Get broker name separately
            string brokerName = string.Empty;
            if (row["BrokerID"] != DBNull.Value)
            {
                var broker = BrokerService.GetBrokerByID(Convert.ToInt32(row["BrokerID"]));
                brokerName = broker?.BrokerName ?? string.Empty;
            }
            
            return new PaymentViewModel
            {
                PaymentID = Convert.ToInt32(row["PaymentID"]),
                PaymentDate = Convert.ToDateTime(row["PaymentDate"]),
                TotalAmountPaid = Convert.ToDecimal(row["TotalAmountPaid"]),
                PaymentMethod = row["PaymentMethod"]?.ToString() ?? string.Empty,
                Reference = row["Reference"]?.ToString() ?? string.Empty,
                PartyID = partyId,
                PartyName = partyName,
                BrokerID = row["BrokerID"] != DBNull.Value ? Convert.ToInt32(row["BrokerID"]) : (int?)null,
                BrokerName = brokerName,
                ChequeAmountFirm1 = row["ChequeAmountFirm1"] != DBNull.Value ? Convert.ToDecimal(row["ChequeAmountFirm1"]) : 0,
                ChequeAmountFirm2 = row["ChequeAmountFirm2"] != DBNull.Value ? Convert.ToDecimal(row["ChequeAmountFirm2"]) : 0
            };
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error loading payment: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    return null;
}
        /// <summary>
        /// Gets the payment trace showing which bills a payment was applied to
        /// </summary>
        public static List<PaymentTraceViewModel> GetPaymentTrace(int paymentId)
        {
            var paymentTrace = new List<PaymentTraceViewModel>();
            
            try
            {
                // First, check if the payment exists
                string checkPaymentSql = "SELECT COUNT(*) FROM PaymentMaster WHERE PaymentID = ?";
                var checkParam = new OleDbParameter("PaymentID", paymentId);
                int paymentCount = Convert.ToInt32(DatabaseManager.ExecuteScalar(checkPaymentSql, checkParam));
                
                if (paymentCount == 0)
                {
                    MessageBox.Show($"Payment with ID {paymentId} not found.", "Payment Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return paymentTrace;
                }
                
                // Check if there are any transactions for this payment
                string checkTransactionsSql = "SELECT COUNT(*) FROM TransactionLedger WHERE PaymentID = ?";
                var checkTransParam = new OleDbParameter("PaymentID", paymentId);
                int transactionCount = Convert.ToInt32(DatabaseManager.ExecuteScalar(checkTransactionsSql, checkTransParam));
                
                if (transactionCount == 0)
                {
                    MessageBox.Show($"No transactions found for payment ID {paymentId}. This payment may not have been properly linked to transactions.", "No Transactions Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return paymentTrace;
                }
                
                // Direct lookup using PaymentID column in TransactionLedger
                string sql = @"
                    SELECT 
                        tl.TransactionID,
                        tl.BillID,
                        b.BillNo,
                        b.BillDate,
                        (b.OriginalAmount + b.AdditionalCharges) as BillAmount,
                        tl.DebitAmount,
                        tl.CreditAmount,
                        tl.TransactionDate,
                        tl.Description,
                        tl.PaymentMethod,
                        tl.Reference,
                        tl.TransactionType
                    FROM TransactionLedger tl
                    LEFT JOIN BillMaster b ON tl.BillID = b.BillID
                    WHERE tl.PaymentID = ?
                    ORDER BY tl.TransactionDate, tl.TransactionID";
                
                var param = new OleDbParameter("PaymentID", paymentId);

                DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
                foreach (DataRow row in dt.Rows)
                {
                    paymentTrace.Add(new PaymentTraceViewModel
                    {
                        TransactionID = Convert.ToInt32(row["TransactionID"]),
                        BillID = row["BillID"] != DBNull.Value ? Convert.ToInt32(row["BillID"]) : (int?)null,
                        BillNo = row["BillNo"]?.ToString() ?? "N/A",
                        BillDate = row["BillDate"] != DBNull.Value ? Convert.ToDateTime(row["BillDate"]) : (DateTime?)null,
                        BillAmount = row["BillAmount"] != DBNull.Value ? Convert.ToDecimal(row["BillAmount"]) : 0,
                        DebitAmount = Convert.ToDecimal(row["DebitAmount"]),
                        CreditAmount = Convert.ToDecimal(row["CreditAmount"]),
                        TransactionDate = Convert.ToDateTime(row["TransactionDate"]),
                        Description = row["Description"]?.ToString() ?? "",
                        PaymentMethod = row["PaymentMethod"]?.ToString() ?? "",
                        Reference = row["Reference"]?.ToString() ?? "",
                        TransactionType = row["TransactionType"]?.ToString() ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading payment trace: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return paymentTrace;
        }

        public static bool DeletePayment(int paymentId)
        {
            try
            {
                using (var conn = DatabaseManager.GetConnection())
                {
                    conn.Open();
                    
                    // Set command timeout to prevent hanging
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            var param = new OleDbParameter("PaymentID", paymentId);

                            // First, get the bills that were affected by this payment before deleting transactions
                            var affectedBills = GetBillsAffectedByPayment(paymentId, conn, transaction);

                            // Delete all ledger entries associated with this payment
                            using (var cmd = new OleDbCommand("DELETE FROM TransactionLedger WHERE PaymentID = ?", conn, transaction))
                            {
                                cmd.CommandTimeout = 30; // Set timeout to 30 seconds
                                cmd.Parameters.Add(param);
                                cmd.ExecuteNonQuery();
                            }

                            // Then, delete the master payment record
                            using (var cmd = new OleDbCommand("DELETE FROM PaymentMaster WHERE PaymentID = ?", conn, transaction))
                            {
                                cmd.CommandTimeout = 30; // Set timeout to 30 seconds
                                // Re-add the parameter as it was used in the previous command
                                cmd.Parameters.Add(new OleDbParameter("PaymentID", paymentId));
                                cmd.ExecuteNonQuery();
                            }

                            // Commit the deletion transaction
                            transaction.Commit();
                            
                            // Now update bill statuses in a new transaction
                            UpdateBillStatusesAfterDeletion(affectedBills);
                            
                            return true;
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                transaction.Rollback();
                            }
                            catch
                            {
                                // Ignore rollback errors
                            }
                            throw; // Re-throw to be caught by outer try-catch
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting payment: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Gets the list of bills that were affected by a specific payment
        /// </summary>
        private static List<int> GetBillsAffectedByPayment(int paymentId, OleDbConnection conn, OleDbTransaction transaction)
        {
            var affectedBills = new List<int>();
            string sql = "SELECT DISTINCT BillID FROM TransactionLedger WHERE PaymentID = ? AND BillID IS NOT NULL";
            var param = new OleDbParameter("PaymentID", paymentId);

            using (var cmd = new OleDbCommand(sql, conn, transaction))
            {
                cmd.Parameters.Add(param);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        affectedBills.Add(Convert.ToInt32(reader["BillID"]));
                    }
                }
            }
            return affectedBills;
        }

        /// <summary>
        /// Updates the status of bills after payment deletion using the same logic as payment entry save - OPTIMIZED
        /// </summary>
        private static void UpdateBillStatusesAfterDeletion(List<int> affectedBillIds)
        {
            if (affectedBillIds == null || !affectedBillIds.Any())
                return;
                
            try
            {
                using (var conn = DatabaseManager.GetConnection())
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // Batch process bill status updates for better performance
                            string billIdList = string.Join(",", affectedBillIds);
                            
                            // Get all bill details in a single query
                            string billSql = $"SELECT BillID, (OriginalAmount + AdditionalCharges) as TotalAmount FROM BillMaster WHERE BillID IN ({billIdList})";
                            var billData = new Dictionary<int, decimal>();
                            
                            using (var cmd = new OleDbCommand(billSql, conn, trans))
                            {
                                cmd.CommandTimeout = 30;
                                using (var reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        int billId = Convert.ToInt32(reader["BillID"]);
                                        decimal totalAmount = reader["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(reader["TotalAmount"]) : 0;
                                        billData[billId] = totalAmount;
                                    }
                                }
                            }
                            
                            // OPTIMIZATION: Get all due amounts in one query instead of individual calls
                            var currentBalances = LedgerService.GetAllBillBalances();
                            
                            foreach (var billId in affectedBillIds)
                            {
                                if (!billData.ContainsKey(billId))
                                    continue;
                                    
                                decimal totalAmount = billData[billId];

                                // Use pre-calculated balance from the batch query
                                decimal dueAmount = currentBalances.ContainsKey(billId) ? currentBalances[billId] : 0;
                                
                                // Determine new status based on balance using the same logic as payment entry save
                                string newStatus;
                                if (dueAmount <= 0)
                                {
                                    newStatus = "Paid";
                                }
                                else if (Math.Round(dueAmount) >= Math.Round(totalAmount))
                                {
                                    newStatus = "Unpaid";
                                }
                                else
                                {
                                    newStatus = "Partial";
                                }
                                
                                // Update the bill status in the database
                                string updateSql = "UPDATE BillMaster SET Status = ? WHERE BillID = ?";
                                using (var cmd = new OleDbCommand(updateSql, conn, trans))
                                {
                                    cmd.CommandTimeout = 30;
                                    cmd.Parameters.Add(new OleDbParameter("Status", newStatus));
                                    cmd.Parameters.Add(new OleDbParameter("BillID", billId));
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                trans.Rollback();
                            }
                            catch
                            {
                                // Ignore rollback errors
                            }
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't throw - we don't want to rollback the payment deletion if status update fails
                System.Diagnostics.Debug.WriteLine($"Error updating bill statuses after payment deletion: {ex.Message}");
            }
        }
    }
}
