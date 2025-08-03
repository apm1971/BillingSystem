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
                    INSERT INTO PaymentMaster (PartyID, PaymentDate, TotalAmountPaid, PaymentMethod, Reference, CompanyID)
                    VALUES (?, ?, ?, ?, ?, ?)";

                var parameters = new OleDbParameter[]
                {
                    new OleDbParameter("PartyID", payment.PartyID),
                    new OleDbParameter("PaymentDate", payment.PaymentDate),
                    new OleDbParameter("TotalAmountPaid", payment.TotalAmountPaid),
                    new OleDbParameter("PaymentMethod", payment.PaymentMethod ?? (object)DBNull.Value),
                    new OleDbParameter("Reference", payment.Reference ?? (object)DBNull.Value),
                    new OleDbParameter("CompanyID", payment.CompanyID)
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
            string sql = @"
                SELECT pm.PaymentID, pm.PaymentDate, pm.TotalAmountPaid, pm.PaymentMethod, pm.Reference, pm.PartyID, p.PartyName
                FROM PaymentMaster pm
                LEFT JOIN PartyMaster p ON pm.PartyID = p.PartyID
                WHERE pm.CompanyID = ?
                ORDER BY pm.PaymentDate DESC";
            
            var param = new OleDbParameter("CompanyID", companyId);

            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
                foreach (DataRow row in dt.Rows)
                {
                    payments.Add(new PaymentViewModel
                    {
                        PaymentID = Convert.ToInt32(row["PaymentID"]),
                        PaymentDate = Convert.ToDateTime(row["PaymentDate"]),
                        TotalAmountPaid = Convert.ToDecimal(row["TotalAmountPaid"]),
                        PaymentMethod = row["PaymentMethod"].ToString(),
                        Reference = row["Reference"].ToString(),
                        PartyID = Convert.ToInt32(row["PartyID"]),
                        PartyName = row["PartyName"].ToString()
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
                        tl.Reference
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
                        Reference = row["Reference"]?.ToString() ?? ""
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
            using (var conn = DatabaseManager.GetConnection())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                try
                {
                    var param = new OleDbParameter("PaymentID", paymentId);

                    // First, get the bills that were affected by this payment before deleting transactions
                    var affectedBills = GetBillsAffectedByPayment(paymentId, conn, transaction);

                    // Delete all ledger entries associated with this payment
                    using (var cmd = new OleDbCommand("DELETE FROM TransactionLedger WHERE PaymentID = ?", conn, transaction))
                    {
                        cmd.Parameters.Add(param);
                        cmd.ExecuteNonQuery();
                    }

                    // Then, delete the master payment record
                    using (var cmd = new OleDbCommand("DELETE FROM PaymentMaster WHERE PaymentID = ?", conn, transaction))
                    {
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
                    transaction.Rollback();
                    MessageBox.Show($"Error deleting payment: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
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
        /// Updates the status of bills after payment deletion using the same logic as payment entry save
        /// </summary>
        private static void UpdateBillStatusesAfterDeletion(List<int> affectedBillIds)
        {
            try
            {
                using (var conn = DatabaseManager.GetConnection())
                {
                    conn.Open();
                    var trans = conn.BeginTransaction();
                    
                    try
                    {
                        foreach (var billId in affectedBillIds)
                        {
                            // Get the bill details to calculate total amount
                            string billSql = "SELECT (OriginalAmount + AdditionalCharges) as TotalAmount FROM BillMaster WHERE BillID = ?";
                            var billParam = new OleDbParameter("BillID", billId);
                            decimal totalAmount = 0;
                            
                            using (var cmd = new OleDbCommand(billSql, conn, trans))
                            {
                                cmd.Parameters.Add(billParam);
                                var result = cmd.ExecuteScalar();
                                if (result != null && result != DBNull.Value)
                                {
                                    totalAmount = Convert.ToDecimal(result);
                                }
                            }

                            // Calculate current balance after payment deletion (ledger transactions are now deleted)
                            decimal dueAmount = LedgerService.GetDueAmount(billId);
                            
                            // Determine new status based on balance using the same logic as payment entry save
                            string newStatus;
                            if (dueAmount <= 0)
                            {
                                newStatus = "Paid";
                            }
                            else if (dueAmount >= totalAmount)
                            {
                                newStatus = "Unpaid";
                            }
                            else
                            {
                                newStatus = "Partial";
                            }
                            
                            // Update the bill status in the database
                            string updateSql = "UPDATE BillMaster SET Status = ? WHERE BillID = ?";
                            var statusParam = new OleDbParameter("Status", newStatus);
                            var billIdParam = new OleDbParameter("BillID", billId);
                            
                            using (var cmd = new OleDbCommand(updateSql, conn, trans))
                            {
                                cmd.Parameters.Add(statusParam);
                                cmd.Parameters.Add(billIdParam);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw;
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
