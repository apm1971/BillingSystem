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
                // First, get the payment details
                string paymentSql = "SELECT PaymentDate, PaymentMethod, Reference FROM PaymentMaster WHERE PaymentID = ?";
                var paymentParam = new OleDbParameter("PaymentID", paymentId);
                DataTable paymentDt = DatabaseManager.ExecuteQuery(paymentSql, paymentParam);
                
                if (paymentDt.Rows.Count == 0)
                {
                    MessageBox.Show($"Payment with ID {paymentId} not found.", "Payment Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return paymentTrace;
                }
                
                var paymentRow = paymentDt.Rows[0];
                DateTime paymentDate = Convert.ToDateTime(paymentRow["PaymentDate"]);
                string paymentMethod = paymentRow["PaymentMethod"]?.ToString() ?? "";
                string paymentReference = paymentRow["Reference"]?.ToString() ?? "";
                
                // Now find transactions that match this payment
                string sql = @"
                    SELECT 
                        tl.TransactionID,
                        tl.BillID,
                        b.BillNo,
                        b.BillDate,
                        b.TotalAmount as BillAmount,
                        tl.DebitAmount,
                        tl.CreditAmount,
                        tl.TransactionDate,
                        tl.Description,
                        tl.PaymentMethod,
                        tl.Reference
                    FROM TransactionLedger tl
                    LEFT JOIN BillMaster b ON tl.BillID = b.BillID
                    WHERE tl.TransactionType = 'Payment' 
                    AND tl.TransactionDate = ?
                    AND tl.PaymentMethod = ?
                    ORDER BY tl.TransactionDate, tl.TransactionID";
                
                var dateParam = new OleDbParameter("PaymentDate", paymentDate);
                var methodParam = new OleDbParameter("PaymentMethod", paymentMethod);

                DataTable dt = DatabaseManager.ExecuteQuery(sql, dateParam, methodParam);
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

                    // First, delete all ledger entries associated with this payment
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

                    transaction.Commit();
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
    }
}
