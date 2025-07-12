using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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
            
            string sql = "SELECT * FROM PaymentMaster ORDER BY PaymentDate DESC";
            DataTable dt = DatabaseManager.ExecuteQuery(sql);
            
            foreach (DataRow row in dt.Rows)
            {
                Payment payment = new Payment
                {
                    PaymentID = Convert.ToInt32(row["PaymentID"]),
                    PaymentDate = Convert.ToDateTime(row["PaymentDate"]),
                    PaymentAmount = Convert.ToDouble(row["PaymentAmount"]),
                    PaymentMethod = row["PaymentMethod"].ToString(),
                    Reference = row["Reference"].ToString(),
                    Notes = row["Notes"].ToString()
                };
                
                // Get payment details
                payment.PaymentDetails = GetPaymentDetails(payment.PaymentID);
                
                payments.Add(payment);
            }
            
            return payments;
        }
        
        // Get payment details
        private static List<PaymentDetail> GetPaymentDetails(int paymentID)
        {
            List<PaymentDetail> paymentDetails = new List<PaymentDetail>();
            
            string sql = "SELECT * FROM PaymentDetails WHERE PaymentID = @PaymentID";
            SQLiteParameter param = new SQLiteParameter("@PaymentID", paymentID);
            
            DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
            
            foreach (DataRow row in dt.Rows)
            {
                PaymentDetail detail = new PaymentDetail
                {
                    PaymentDetailID = Convert.ToInt32(row["PaymentDetailID"]),
                    PaymentID = paymentID,
                    BillID = Convert.ToInt32(row["BillID"]),
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
            string sql = "SELECT * FROM PaymentMaster WHERE PaymentID = @PaymentID";
            SQLiteParameter param = new SQLiteParameter("@PaymentID", paymentID);
            
            DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
            
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
                    Notes = row["Notes"].ToString()
                };
                
                // Get payment details
                payment.PaymentDetails = GetPaymentDetails(payment.PaymentID);
                
                return payment;
            }
            
            return null;
        }
        
        // Get outstanding bills by party
        public static List<Bill> GetOutstandingBillsByParty(int partyID)
        {
            List<Bill> bills = new List<Bill>();
            
            string sql = @"SELECT b.*, COALESCE(SUM(pd.AllocatedAmount), 0) as PaidAmount
                FROM BillMaster b
                LEFT JOIN PaymentDetails pd ON b.BillID = pd.BillID
                WHERE b.PartyID = @PartyID
                GROUP BY b.BillID
                HAVING (b.NetAmount - COALESCE(SUM(pd.AllocatedAmount), 0)) > 0.01
                ORDER BY b.BillDate";
                
            SQLiteParameter param = new SQLiteParameter("@PartyID", partyID);
            DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
            
            foreach (DataRow row in dt.Rows)
            {
                Bill bill = MapRowToBill(row);
                bills.Add(bill);
            }
            
            return bills;
        }
        
        // Get outstanding bills by broker
        public static List<Bill> GetOutstandingBillsByBroker(int brokerID)
        {
            List<Bill> bills = new List<Bill>();
            
            string sql = @"SELECT b.*, COALESCE(SUM(pd.AllocatedAmount), 0) as PaidAmount
                FROM BillMaster b
                LEFT JOIN PaymentDetails pd ON b.BillID = pd.BillID
                WHERE b.BrokerID = @BrokerID
                GROUP BY b.BillID
                HAVING (b.NetAmount - COALESCE(SUM(pd.AllocatedAmount), 0)) > 0.01
                ORDER BY b.BillDate";
                
            SQLiteParameter param = new SQLiteParameter("@BrokerID", brokerID);
            DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
            
            foreach (DataRow row in dt.Rows)
            {
                Bill bill = MapRowToBill(row);
                bills.Add(bill);
            }
            
            return bills;
        }
        
        // Get all outstanding bills
        public static List<Bill> GetAllOutstandingBills()
        {
            List<Bill> bills = new List<Bill>();
            
            string sql = @"SELECT b.*, COALESCE(SUM(pd.AllocatedAmount), 0) as PaidAmount
                FROM BillMaster b
                LEFT JOIN PaymentDetails pd ON b.BillID = pd.BillID
                GROUP BY b.BillID
                HAVING (b.NetAmount - COALESCE(SUM(pd.AllocatedAmount), 0)) > 0.01
                ORDER BY b.BillDate";
                
            DataTable dt = DatabaseManager.ExecuteQuery(sql);
            
            foreach (DataRow row in dt.Rows)
            {
                Bill bill = MapRowToBill(row);
                bills.Add(bill);
            }
            
            return bills;
        }
        
        // Save payment (add or update)
        public static bool SavePayment(Payment payment)
        {
            using (SQLiteConnection conn = DatabaseManager.GetConnection())
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        if (payment.PaymentID == 0)
                        {
                            // Add new payment
                            string sql = @"INSERT INTO PaymentMaster (
                                PaymentDate, PaymentAmount, PaymentMethod, Reference, Notes
                            ) VALUES (
                                @PaymentDate, @PaymentAmount, @PaymentMethod, @Reference, @Notes
                            )";
                            
                            using (SQLiteCommand cmd = new SQLiteCommand(sql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@PaymentDate", payment.PaymentDate);
                                cmd.Parameters.AddWithValue("@PaymentAmount", payment.PaymentAmount);
                                cmd.Parameters.AddWithValue("@PaymentMethod", payment.PaymentMethod);
                                cmd.Parameters.AddWithValue("@Reference", payment.Reference ?? string.Empty);
                                cmd.Parameters.AddWithValue("@Notes", payment.Notes ?? string.Empty);
                                
                                cmd.ExecuteNonQuery();
                            }
                            
                            // Get the new payment ID
                            sql = "SELECT last_insert_rowid()";
                            using (SQLiteCommand cmd = new SQLiteCommand(sql, conn, transaction))
                            {
                                object result = cmd.ExecuteScalar();
                                payment.PaymentID = Convert.ToInt32(result);
                            }
                        }
                        else
                        {
                            // Update existing payment
                            string sql = @"UPDATE PaymentMaster SET 
                                PaymentDate = @PaymentDate,
                                PaymentAmount = @PaymentAmount,
                                PaymentMethod = @PaymentMethod,
                                Reference = @Reference,
                                Notes = @Notes
                            WHERE PaymentID = @PaymentID";
                            
                            using (SQLiteCommand cmd = new SQLiteCommand(sql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@PaymentDate", payment.PaymentDate);
                                cmd.Parameters.AddWithValue("@PaymentAmount", payment.PaymentAmount);
                                cmd.Parameters.AddWithValue("@PaymentMethod", payment.PaymentMethod);
                                cmd.Parameters.AddWithValue("@Reference", payment.Reference ?? string.Empty);
                                cmd.Parameters.AddWithValue("@Notes", payment.Notes ?? string.Empty);
                                cmd.Parameters.AddWithValue("@PaymentID", payment.PaymentID);
                                
                                cmd.ExecuteNonQuery();
                            }
                            
                            // Delete existing payment details
                            string deleteSql = "DELETE FROM PaymentDetails WHERE PaymentID = @PaymentID";
                            using (SQLiteCommand cmd = new SQLiteCommand(deleteSql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@PaymentID", payment.PaymentID);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        
                        // Save payment details
                        foreach (var detail in payment.PaymentDetails)
                        {
                            string detailSql = @"INSERT INTO PaymentDetails (
                                PaymentID, BillID, PreviousPaid, BalanceBefore, AllocatedAmount, BalanceAfter
                            ) VALUES (
                                @PaymentID, @BillID, @PreviousPaid, @BalanceBefore, @AllocatedAmount, @BalanceAfter
                            )";
                            
                            using (SQLiteCommand cmd = new SQLiteCommand(detailSql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@PaymentID", payment.PaymentID);
                                cmd.Parameters.AddWithValue("@BillID", detail.BillID);
                                cmd.Parameters.AddWithValue("@PreviousPaid", detail.PreviousPaid);
                                cmd.Parameters.AddWithValue("@BalanceBefore", detail.BalanceBefore);
                                cmd.Parameters.AddWithValue("@AllocatedAmount", detail.AllocatedAmount);
                                cmd.Parameters.AddWithValue("@BalanceAfter", detail.BalanceAfter);
                                
                                cmd.ExecuteNonQuery();
                            }
                        }
                        
                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        
        // Delete payment
        public static bool DeletePayment(int paymentID)
        {
            using (SQLiteConnection conn = DatabaseManager.GetConnection())
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Delete payment details first
                        string deleteDetailsSql = "DELETE FROM PaymentDetails WHERE PaymentID = @PaymentID";
                        using (SQLiteCommand cmd = new SQLiteCommand(deleteDetailsSql, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@PaymentID", paymentID);
                            cmd.ExecuteNonQuery();
                        }
                        
                        // Delete payment master
                        string deletePaymentSql = "DELETE FROM PaymentMaster WHERE PaymentID = @PaymentID";
                        using (SQLiteCommand cmd = new SQLiteCommand(deletePaymentSql, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@PaymentID", paymentID);
                            cmd.ExecuteNonQuery();
                        }
                        
                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        
        // Update bill paid amounts after payment changes
        public static bool UpdateBillPaidAmounts()
        {
            try
            {
                string sql = @"UPDATE BillMaster 
                    SET PaidAmount = (
                        SELECT COALESCE(SUM(AllocatedAmount), 0) 
                        FROM PaymentDetails 
                        WHERE PaymentDetails.BillID = BillMaster.BillID
                    )";
                
                int result = DatabaseManager.ExecuteNonQuery(sql);
                return result >= 0;
            }
            catch
            {
                return false;
            }
        }
        
        // Helper method to map DataRow to Bill
        private static Bill MapRowToBill(DataRow row)
        {
            var bill = new Bill
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
                PaidAmount = row.Table.Columns.Contains("PaidAmount") ? Convert.ToDouble(row["PaidAmount"]) : 0
            };
            
            return bill;
        }
    }
} 