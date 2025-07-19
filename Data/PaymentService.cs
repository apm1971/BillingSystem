using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb; // Changed from SQLite
using System.Linq;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Data
{
    public class PaymentService
    {
        // Calculate interest and discount for a bill based on payment date
        public static (double interestAmount, double discountAmount, double netPayableAmount) CalculateInterestAndDiscount(Bill bill, DateTime paymentDate)
        {
            // Get global rates from settings
            double interestRate = SettingsService.GetInterestRate();
            double discountRate = SettingsService.GetDiscountRate();
            
            double interestAmount = 0;
            double discountAmount = 0;
            
            // Calculate days difference from payment date to due date
            int daysDifference = (paymentDate - bill.DueDate).Days;
            
            if (daysDifference > 0)
            {
                // Payment is after due date - calculate interest
                // Interest = NetAmount * (InterestRate / 100) * (DaysOverdue / 365)
                interestAmount = bill.NetAmount * (interestRate / 100) * (daysDifference / 365.0);
            }
            else if (daysDifference < 0)
            {
                // Payment is before due date - calculate discount
                // Discount = NetAmount * (DiscountRate / 100)
                discountAmount = bill.NetAmount * (discountRate / 100);
            }
            
            // Calculate net payable amount
            double netPayableAmount = bill.NetAmount + interestAmount - discountAmount;
            
            return (Math.Round(interestAmount, 2), Math.Round(discountAmount, 2), Math.Round(netPayableAmount, 2));
        }
        
        // Get all payments
        public static List<Payment> GetAllPayments()
        {
            List<Payment> payments = new List<Payment>();
            
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            string sql = "SELECT * FROM PaymentMaster WHERE CompanyID = ? ORDER BY PaymentDate DESC";
            OleDbParameter param = new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID };
            
            DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
            
            foreach (DataRow row in dt.Rows)
            {
                Payment payment = new Payment
                {
                    PaymentID = Convert.ToInt32(row["PaymentID"]),
                    PaymentDate = Convert.ToDateTime(row["PaymentDate"]),
                    PaymentAmount = Convert.ToDouble(row["PaymentAmount"]),
                    PaymentMethod = row["PaymentMethod"].ToString(),
                    Reference = row["Reference"].ToString(),
                    Notes = row["Notes"].ToString(),
                    CompanyID = Convert.ToInt32(row["CompanyID"])
                };
                
                // Get payment details
                payment.PaymentDetails = GetPaymentDetails(payment.PaymentID);
                
                // Extract party information from payment details
                PopulatePartyInformation(payment);
                
                payments.Add(payment);
            }
            
            return payments;
        }
        
        // Populate party and broker information for a payment
        private static void PopulatePartyInformation(Payment payment)
        {
            if (payment.PaymentDetails.Count == 0)
                return;
                
            // Get all bills associated with this payment
            var billIds = payment.PaymentDetails.Select(pd => pd.BillID).ToList();
            var partyNames = new HashSet<string>();
            string primaryPartyName = string.Empty;
            
            // For each payment detail, get bill information
            foreach (var detail in payment.PaymentDetails)
            {
                // Load bill and party information
                var bill = BillService.GetBillByID(detail.BillID);
                if (bill != null)
                {
                    // Set bill no and party name on payment detail
                    detail.BillNo = bill.BillNo;
                    detail.PartyName = bill.PartyName;
                    
                    // Add to unique party names
                    if (!string.IsNullOrEmpty(bill.PartyName))
                    {
                        partyNames.Add(bill.PartyName);
                        
                        // Use first bill's party name as primary
                        if (string.IsNullOrEmpty(primaryPartyName))
                        {
                            primaryPartyName = bill.PartyName;
                        }
                    }
                }
            }
            
            // Set party information on payment
            payment.PrimaryPartyName = primaryPartyName;
            payment.UniquePartyCount = partyNames.Count;
        }
        
        // Get payment details
        private static List<PaymentDetail> GetPaymentDetails(int paymentID)
        {
            List<PaymentDetail> paymentDetails = new List<PaymentDetail>();
            
            string sql = "SELECT pd.*, b.BillNo, b.PartyName FROM PaymentDetails pd " +
                         "LEFT JOIN BillMaster b ON pd.BillID = b.BillID " +
                         "WHERE pd.PaymentID = ?";
            OleDbParameter param = new OleDbParameter("PaymentID", paymentID);
            
            DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
            
            foreach (DataRow row in dt.Rows)
            {
                PaymentDetail detail = new PaymentDetail
                {
                    PaymentDetailID = Convert.ToInt32(row["PaymentDetailID"]),
                    PaymentID = paymentID,
                    BillID = Convert.ToInt32(row["BillID"]),
                    BillNo = row["BillNo"].ToString(),
                    PartyName = row["PartyName"].ToString(),
                    PreviousPaid = Convert.ToDouble(row["PreviousPaid"]),
                    BalanceBefore = Convert.ToDouble(row["BalanceBefore"]),
                    AllocatedAmount = Convert.ToDouble(row["AllocatedAmount"]),
                    BalanceAfter = Convert.ToDouble(row["BalanceAfter"])
                };
                
                paymentDetails.Add(detail);
            }
            
            return paymentDetails;
        }
        
        // Get payment by ID
        public static Payment? GetPaymentByID(int paymentID)
        {
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            string sql = "SELECT * FROM PaymentMaster WHERE PaymentID = ? AND CompanyID = ?";
            OleDbParameter[] parameters = {
                new OleDbParameter("PaymentID", OleDbType.Integer) { Value = paymentID },
                new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID }
            };
            
            DataTable dt = DatabaseManager.ExecuteQuery(sql, parameters);
            
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                
                Payment payment = new Payment
                {
                    PaymentID = Convert.ToInt32(row["PaymentID"]),
                    PaymentDate = Convert.ToDateTime(row["PaymentDate"]),
                    PaymentAmount = Convert.ToDouble(row["PaymentAmount"]),
                    PaymentMethod = row["PaymentMethod"].ToString(),
                    Reference = row["Reference"].ToString(),
                    Notes = row["Notes"].ToString(),
                    CompanyID = Convert.ToInt32(row["CompanyID"])
                };
                
                // Get payment details
                payment.PaymentDetails = GetPaymentDetails(payment.PaymentID);
                
                // Extract party information from payment details
                PopulatePartyInformation(payment);
                
                return payment;
            }
            
            return null;
        }
        
        // Get outstanding bills by party
        public static List<Bill> GetOutstandingBillsByParty(int partyID)
        {
            List<Bill> bills = new List<Bill>();
            
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            // Modified SQL query to be compatible with MS Access - listing all columns explicitly
            string sql = @"SELECT b.BillID, b.BillNo, b.BillDate, b.DueDate, b.PartyID, b.PartyName, 
                    b.BrokerID, b.BrokerName, b.TotalAmount, b.TotalCharges, b.NetAmount, b.Notes,
                    IIF(SUM(pd.AllocatedAmount) IS NULL, 0, SUM(pd.AllocatedAmount)) as PaidAmount
                FROM BillMaster b
                LEFT JOIN PaymentDetails pd ON b.BillID = pd.BillID
                WHERE b.PartyID = ? AND b.CompanyID = ?
                GROUP BY b.BillID, b.BillNo, b.BillDate, b.DueDate, b.PartyID, b.PartyName, 
                    b.BrokerID, b.BrokerName, b.TotalAmount, b.TotalCharges, b.NetAmount, b.Notes
                HAVING (b.NetAmount - IIF(SUM(pd.AllocatedAmount) IS NULL, 0, SUM(pd.AllocatedAmount))) > 0.01
                ORDER BY b.BillDate";
                
            // Explicitly set parameter type for MS Access
            OleDbParameter[] parameters = {
                new OleDbParameter("PartyID", OleDbType.Integer) { Value = partyID },
                new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID }
            };
            
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(sql, parameters);
                
                foreach (DataRow row in dt.Rows)
                {
                    Bill bill = MapRowToBill(row);
                    bills.Add(bill);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error loading bills: {ex.Message}\n\nSQL: {sql}", "Database Error", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            
            return bills;
        }
        
        // Get outstanding bills by broker
        public static List<Bill> GetOutstandingBillsByBroker(int brokerID)
        {
            List<Bill> bills = new List<Bill>();
            
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            // Modified SQL query to be compatible with MS Access - listing all columns explicitly
            string sql = @"SELECT b.BillID, b.BillNo, b.BillDate, b.DueDate, b.PartyID, b.PartyName, 
                    b.BrokerID, b.BrokerName, b.TotalAmount, b.TotalCharges, b.NetAmount, b.Notes,
                    IIF(SUM(pd.AllocatedAmount) IS NULL, 0, SUM(pd.AllocatedAmount)) as PaidAmount
                FROM BillMaster b
                LEFT JOIN PaymentDetails pd ON b.BillID = pd.BillID
                WHERE b.BrokerID = ? AND b.CompanyID = ?
                GROUP BY b.BillID, b.BillNo, b.BillDate, b.DueDate, b.PartyID, b.PartyName, 
                    b.BrokerID, b.BrokerName, b.TotalAmount, b.TotalCharges, b.NetAmount, b.Notes
                HAVING (b.NetAmount - IIF(SUM(pd.AllocatedAmount) IS NULL, 0, SUM(pd.AllocatedAmount))) > 0.01
                ORDER BY b.BillDate";
                
            // Explicitly set parameter type for MS Access
            OleDbParameter[] parameters = {
                new OleDbParameter("BrokerID", OleDbType.Integer) { Value = brokerID },
                new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID }
            };
            
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(sql, parameters);
                
                foreach (DataRow row in dt.Rows)
                {
                    Bill bill = MapRowToBill(row);
                    bills.Add(bill);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error loading bills: {ex.Message}\n\nSQL: {sql}", "Database Error", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            
            return bills;
        }
        
        // Get all outstanding bills
        public static List<Bill> GetAllOutstandingBills()
        {
            List<Bill> bills = new List<Bill>();
            
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            // Modified SQL query to be compatible with MS Access - listing all columns explicitly
            string sql = @"SELECT b.BillID, b.BillNo, b.BillDate, b.DueDate, b.PartyID, b.PartyName, 
                    b.BrokerID, b.BrokerName, b.TotalAmount, b.TotalCharges, b.NetAmount, b.Notes,
                    IIF(SUM(pd.AllocatedAmount) IS NULL, 0, SUM(pd.AllocatedAmount)) as PaidAmount
                FROM BillMaster b
                LEFT JOIN PaymentDetails pd ON b.BillID = pd.BillID
                WHERE b.CompanyID = ?
                GROUP BY b.BillID, b.BillNo, b.BillDate, b.DueDate, b.PartyID, b.PartyName, 
                    b.BrokerID, b.BrokerName, b.TotalAmount, b.TotalCharges, b.NetAmount, b.Notes
                ORDER BY b.BillDate";
            
            OleDbParameter param = new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID };
            
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
                
                foreach (DataRow row in dt.Rows)
                {
                    Bill bill = MapRowToBill(row);
                    
                    // Calculate interest and discount for bills with payments
                    if (bill.PaidAmount > 0)
                    {
                        // Get payment details to determine when payment was made
                        string paymentSql = @"SELECT TOP 1 pm.PaymentDate 
                                            FROM PaymentDetails pd 
                                            INNER JOIN PaymentMaster pm ON pd.PaymentID = pm.PaymentID
                                            WHERE pd.BillID = ?
                                            ORDER BY pm.PaymentDate DESC";
                        
                        OleDbParameter billIdParam = new OleDbParameter("BillID", OleDbType.Integer) { Value = bill.BillID };
                        object result = DatabaseManager.ExecuteScalar(paymentSql, billIdParam);
                        
                        if (result != null && result != DBNull.Value)
                        {
                            DateTime paymentDate = Convert.ToDateTime(result);
                            var (interest, discount, _) = CalculateInterestAndDiscount(bill, paymentDate);
                            bill.InterestAmount = interest;
                            bill.DiscountAmount = discount;
                        }
                    }
                    
                    bills.Add(bill);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error loading bills: {ex.Message}\n\nSQL: {sql}", "Database Error", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            
            return bills;
        }
        
        // Save payment (add or update)
        public static bool SavePayment(Payment payment)
        {
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            using (OleDbConnection conn = DatabaseManager.GetConnection())
            {
                conn.Open();
                OleDbTransaction transaction = conn.BeginTransaction();
                
                try
                {
                    if (payment.PaymentID == 0)
                    {
                        // Add new payment
                        string sql = @"INSERT INTO PaymentMaster (
                            PaymentDate, PaymentAmount, PaymentMethod, Reference, Notes, CompanyID
                        ) VALUES (?, ?, ?, ?, ?, ?)";
                        
                        using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("PaymentDate", payment.PaymentDate);
                            cmd.Parameters.AddWithValue("PaymentAmount", payment.PaymentAmount);
                            cmd.Parameters.AddWithValue("PaymentMethod", payment.PaymentMethod);
                            cmd.Parameters.AddWithValue("Reference", payment.Reference ?? string.Empty);
                            cmd.Parameters.AddWithValue("Notes", payment.Notes ?? string.Empty);
                            cmd.Parameters.AddWithValue("CompanyID", companyID);
                            
                            cmd.ExecuteNonQuery();
                        }
                        
                        // Get the new payment ID
                        sql = "SELECT @@Identity";
                        using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                        {
                            cmd.Transaction = transaction;
                            object result = cmd.ExecuteScalar();
                            payment.PaymentID = Convert.ToInt32(result);
                        }
                    }
                    else
                    {
                        // Update existing payment
                        string sql = @"UPDATE PaymentMaster SET 
                            PaymentDate = ?,
                            PaymentAmount = ?,
                            PaymentMethod = ?,
                            Reference = ?,
                            Notes = ?,
                            CompanyID = ?
                        WHERE PaymentID = ?";
                        
                        using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("PaymentDate", payment.PaymentDate);
                            cmd.Parameters.AddWithValue("PaymentAmount", payment.PaymentAmount);
                            cmd.Parameters.AddWithValue("PaymentMethod", payment.PaymentMethod);
                            cmd.Parameters.AddWithValue("Reference", payment.Reference ?? string.Empty);
                            cmd.Parameters.AddWithValue("Notes", payment.Notes ?? string.Empty);
                            cmd.Parameters.AddWithValue("CompanyID", companyID);
                            cmd.Parameters.AddWithValue("PaymentID", payment.PaymentID);
                            
                            cmd.ExecuteNonQuery();
                        }
                        
                        // Delete existing payment details
                        string deleteSql = "DELETE FROM PaymentDetails WHERE PaymentID = ?";
                        using (OleDbCommand cmd = new OleDbCommand(deleteSql, conn))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("PaymentID", payment.PaymentID);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    
                    // Add payment details
                    foreach (var detail in payment.PaymentDetails)
                    {
                        string detailSql = @"INSERT INTO PaymentDetails (
                            PaymentID, BillID, PreviousPaid, BalanceBefore, AllocatedAmount, BalanceAfter
                        ) VALUES (?, ?, ?, ?, ?, ?)";
                        
                        using (OleDbCommand cmd = new OleDbCommand(detailSql, conn))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("PaymentID", payment.PaymentID);
                            cmd.Parameters.AddWithValue("BillID", detail.BillID);
                            cmd.Parameters.AddWithValue("PreviousPaid", detail.PreviousPaid);
                            cmd.Parameters.AddWithValue("BalanceBefore", detail.BalanceBefore);
                            cmd.Parameters.AddWithValue("AllocatedAmount", detail.AllocatedAmount);
                            cmd.Parameters.AddWithValue("BalanceAfter", detail.BalanceAfter);
                            
                            cmd.ExecuteNonQuery();
                        }
                    }
                    
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    System.Windows.Forms.MessageBox.Show($"Error saving payment: {ex.Message}", "Payment Error", 
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return false;
                }
            }
        }
        
        // Delete payment
        public static bool DeletePayment(int paymentID)
        {
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            using (OleDbConnection conn = DatabaseManager.GetConnection())
            {
                conn.Open();
                OleDbTransaction transaction = conn.BeginTransaction();
                
                try
                {
                    // First verify this payment belongs to the active company
                    string checkSql = "SELECT COUNT(*) FROM PaymentMaster WHERE PaymentID = ? AND CompanyID = ?";
                    using (OleDbCommand cmd = new OleDbCommand(checkSql, conn))
                    {
                        cmd.Transaction = transaction;
                        cmd.Parameters.AddWithValue("PaymentID", paymentID);
                        cmd.Parameters.AddWithValue("CompanyID", companyID);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        
                        if (count == 0)
                        {
                            // Payment doesn't belong to this company
                            transaction.Rollback();
                            return false;
                        }
                    }
                    
                    // Delete payment details first
                    string deleteDetailsSql = "DELETE FROM PaymentDetails WHERE PaymentID = ?";
                    using (OleDbCommand cmd = new OleDbCommand(deleteDetailsSql, conn))
                    {
                        cmd.Transaction = transaction;
                        cmd.Parameters.AddWithValue("PaymentID", paymentID);
                        cmd.ExecuteNonQuery();
                    }
                    
                    // Delete payment master
                    string deletePaymentSql = "DELETE FROM PaymentMaster WHERE PaymentID = ? AND CompanyID = ?";
                    using (OleDbCommand cmd = new OleDbCommand(deletePaymentSql, conn))
                    {
                        cmd.Transaction = transaction;
                        cmd.Parameters.AddWithValue("PaymentID", paymentID);
                        cmd.Parameters.AddWithValue("CompanyID", companyID);
                        cmd.ExecuteNonQuery();
                    }
                    
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    System.Windows.Forms.MessageBox.Show($"Error deleting payment: {ex.Message}", "Payment Error", 
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return false;
                }
            }
        }
        
        // Update bill paid amounts
        public static bool UpdateBillPaidAmounts()
        {
            try
            {
                // Get the active company ID
                int companyID = Program.ActiveCompany?.CompanyID ?? 0;
                
                // This is a more complex operation in Access because we can't UPDATE with a subquery
                // We'd need to do this in multiple steps or by using ADO.NET directly
                
                string sql = @"SELECT b.BillID, IIF(SUM(pd.AllocatedAmount) IS NULL, 0, SUM(pd.AllocatedAmount)) AS PaidAmount 
                              FROM BillMaster b 
                              LEFT JOIN PaymentDetails pd ON b.BillID = pd.BillID 
                              WHERE b.CompanyID = ?
                              GROUP BY b.BillID";
                
                OleDbParameter param = new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID };
                DataTable billPayments = DatabaseManager.ExecuteQuery(sql, param);
                
                // Now we'd need to update each bill with its paid amount
                // This is just a placeholder - the actual implementation would be complex in Access
                
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error updating bill paid amounts: {ex.Message}", "Update Error", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }
        
        // Map DataRow to Bill object
        private static Bill MapRowToBill(DataRow row)
        {
            double paidAmount = row.Table.Columns.Contains("PaidAmount") ? 
                Convert.ToDouble(row["PaidAmount"]) : 0;
            
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
                PaidAmount = paidAmount,
                Notes = row["Notes"]?.ToString() ?? string.Empty
            };
            
            return bill;
        }
    }
} 