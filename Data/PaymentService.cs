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
