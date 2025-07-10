using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Data
{
    public class PaymentService
    {
        // Get all payments
        public static List<Payment> GetAllPayments()
        {
            List<Payment> payments = new List<Payment>();
            
            string sql = @"SELECT p.*, 
                    COUNT(pd.PaymentDetailID) as DetailCount,
                    SUM(pd.AllocatedAmount) as TotalAllocated
                FROM PaymentMaster p
                LEFT JOIN PaymentDetails pd ON p.PaymentID = pd.PaymentID
                GROUP BY p.PaymentID
                ORDER BY p.PaymentDate DESC, p.PaymentID DESC";
                
            DataTable dt = DatabaseManager.ExecuteQuery(sql);
            
            foreach (DataRow row in dt.Rows)
            {
                Payment payment = new Payment
                {
                    PaymentID = Convert.ToInt32(row["PaymentID"]),
                    PaymentDate = Convert.ToDateTime(row["PaymentDate"]),
                    PaymentAmount = Convert.ToDouble(row["PaymentAmount"]),
                    PaymentMethod = row["PaymentMethod"].ToString(),
                    Reference = row["Reference"].ToString() ?? string.Empty,
                    Notes = row["Notes"].ToString() ?? string.Empty
                };
                
                // Load payment details
                payment.PaymentDetails = GetPaymentDetails(payment.PaymentID);
                
                payments.Add(payment);
            }
            
            return payments;
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
                    Reference = row["Reference"].ToString() ?? string.Empty,
                    Notes = row["Notes"].ToString() ?? string.Empty
                };
                
                // Load payment details
                payment.PaymentDetails = GetPaymentDetails(payment.PaymentID);
                
                return payment;
            }
            
            return null;
        }
        
        // Get payment details for a payment
        public static List<PaymentDetail> GetPaymentDetails(int paymentID)
        {
            List<PaymentDetail> details = new List<PaymentDetail>();
            
            string sql = @"SELECT pd.*, b.BillNo, b.PartyName, b.NetAmount as BillAmount
                FROM PaymentDetails pd
                INNER JOIN BillMaster b ON pd.BillID = b.BillID
                WHERE pd.PaymentID = @PaymentID
                ORDER BY pd.PaymentDetailID";
                
            SQLiteParameter param = new SQLiteParameter("@PaymentID", paymentID);
            DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
            
            foreach (DataRow row in dt.Rows)
            {
                PaymentDetail detail = new PaymentDetail
                {
                    PaymentDetailID = Convert.ToInt32(row["PaymentDetailID"]),
                    PaymentID = Convert.ToInt32(row["PaymentID"]),
                    BillID = Convert.ToInt32(row["BillID"]),
                    BillNo = row["BillNo"].ToString(),
                    PartyName = row["PartyName"].ToString(),
                    BillAmount = Convert.ToDouble(row["BillAmount"]),
                    PreviousPaid = Convert.ToDouble(row["PreviousPaid"]),
                    BalanceBefore = Convert.ToDouble(row["BalanceBefore"]),
                    AllocatedAmount = Convert.ToDouble(row["AllocatedAmount"]),
                    BalanceAfter = Convert.ToDouble(row["BalanceAfter"])
                };
                
                details.Add(detail);
            }
            
            return details;
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
            try
            {
                using (var conn = DatabaseManager.GetConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
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
                                
                                var cmd = new SQLiteCommand(sql, conn, transaction);
                                cmd.Parameters.AddWithValue("@PaymentDate", payment.PaymentDate);
                                cmd.Parameters.AddWithValue("@PaymentAmount", payment.PaymentAmount);
                                cmd.Parameters.AddWithValue("@PaymentMethod", payment.PaymentMethod);
                                cmd.Parameters.AddWithValue("@Reference", payment.Reference ?? string.Empty);
                                cmd.Parameters.AddWithValue("@Notes", payment.Notes ?? string.Empty);
                                
                                cmd.ExecuteNonQuery();
                                
                                // Get the new payment ID
                                payment.PaymentID = Convert.ToInt32(conn.LastInsertRowId);
                            }
                            
                            // Delete existing payment details
                            string deleteSql = "DELETE FROM PaymentDetails WHERE PaymentID = @PaymentID";
                            var deleteCmd = new SQLiteCommand(deleteSql, conn, transaction);
                            deleteCmd.Parameters.AddWithValue("@PaymentID", payment.PaymentID);
                            deleteCmd.ExecuteNonQuery();
                            
                            // Add payment details
                            foreach (var detail in payment.PaymentDetails)
                            {
                                string detailSql = @"INSERT INTO PaymentDetails (
                                    PaymentID, BillID, PreviousPaid, BalanceBefore, AllocatedAmount, BalanceAfter
                                ) VALUES (
                                    @PaymentID, @BillID, @PreviousPaid, @BalanceBefore, @AllocatedAmount, @BalanceAfter
                                )";
                                
                                var detailCmd = new SQLiteCommand(detailSql, conn, transaction);
                                detailCmd.Parameters.AddWithValue("@PaymentID", payment.PaymentID);
                                detailCmd.Parameters.AddWithValue("@BillID", detail.BillID);
                                detailCmd.Parameters.AddWithValue("@PreviousPaid", detail.PreviousPaid);
                                detailCmd.Parameters.AddWithValue("@BalanceBefore", detail.BalanceBefore);
                                detailCmd.Parameters.AddWithValue("@AllocatedAmount", detail.AllocatedAmount);
                                detailCmd.Parameters.AddWithValue("@BalanceAfter", detail.BalanceAfter);
                                
                                detailCmd.ExecuteNonQuery();
                            }
                            
                            // Update PaidAmount in BillMaster for each bill
                            foreach (var detail in payment.PaymentDetails)
                            {
                                // Calculate total paid for this bill
                                string totalPaidSql = @"SELECT COALESCE(SUM(AllocatedAmount), 0) 
                                    FROM PaymentDetails 
                                    WHERE BillID = @BillID";
                                
                                var totalPaidCmd = new SQLiteCommand(totalPaidSql, conn, transaction);
                                totalPaidCmd.Parameters.AddWithValue("@BillID", detail.BillID);
                                double totalPaid = Convert.ToDouble(totalPaidCmd.ExecuteScalar());
                                
                                // Update BillMaster with new PaidAmount
                                string updateBillSql = @"UPDATE BillMaster 
                                    SET PaidAmount = @PaidAmount 
                                    WHERE BillID = @BillID";
                                
                                var updateBillCmd = new SQLiteCommand(updateBillSql, conn, transaction);
                                updateBillCmd.Parameters.AddWithValue("@PaidAmount", totalPaid);
                                updateBillCmd.Parameters.AddWithValue("@BillID", detail.BillID);
                                updateBillCmd.ExecuteNonQuery();
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
            catch (Exception ex)
            {
                throw new Exception($"Error saving payment: {ex.Message}");
            }
        }
        
        // Delete payment
        public static bool DeletePayment(int paymentID)
        {
            try
            {
                using (var conn = DatabaseManager.GetConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // First, get all bills affected by this payment
                            string getBillsSql = "SELECT DISTINCT BillID FROM PaymentDetails WHERE PaymentID = @PaymentID";
                            var getBillsCmd = new SQLiteCommand(getBillsSql, conn, transaction);
                            getBillsCmd.Parameters.AddWithValue("@PaymentID", paymentID);
                            
                            var affectedBills = new List<int>();
                            using (var reader = getBillsCmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    affectedBills.Add(reader.GetInt32(0));
                                }
                            }
                            
                            // Delete payment details
                            string deleteDetailsSql = "DELETE FROM PaymentDetails WHERE PaymentID = @PaymentID";
                            var deleteDetailsCmd = new SQLiteCommand(deleteDetailsSql, conn, transaction);
                            deleteDetailsCmd.Parameters.AddWithValue("@PaymentID", paymentID);
                            deleteDetailsCmd.ExecuteNonQuery();
                            
                            // Delete payment
                            string deletePaymentSql = "DELETE FROM PaymentMaster WHERE PaymentID = @PaymentID";
                            var deletePaymentCmd = new SQLiteCommand(deletePaymentSql, conn, transaction);
                            deletePaymentCmd.Parameters.AddWithValue("@PaymentID", paymentID);
                            int result = deletePaymentCmd.ExecuteNonQuery();
                            
                            // Update PaidAmount for all affected bills
                            foreach (int billID in affectedBills)
                            {
                                // Recalculate total paid for this bill
                                string totalPaidSql = @"SELECT COALESCE(SUM(AllocatedAmount), 0) 
                                    FROM PaymentDetails 
                                    WHERE BillID = @BillID";
                                
                                var totalPaidCmd = new SQLiteCommand(totalPaidSql, conn, transaction);
                                totalPaidCmd.Parameters.AddWithValue("@BillID", billID);
                                double totalPaid = Convert.ToDouble(totalPaidCmd.ExecuteScalar());
                                
                                // Update BillMaster with new PaidAmount
                                string updateBillSql = @"UPDATE BillMaster 
                                    SET PaidAmount = @PaidAmount 
                                    WHERE BillID = @BillID";
                                
                                var updateBillCmd = new SQLiteCommand(updateBillSql, conn, transaction);
                                updateBillCmd.Parameters.AddWithValue("@PaidAmount", totalPaid);
                                updateBillCmd.Parameters.AddWithValue("@BillID", billID);
                                updateBillCmd.ExecuteNonQuery();
                            }
                            
                            transaction.Commit();
                            return result > 0;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
        }
        
        // Helper method to map DataRow to Bill
        private static Bill MapRowToBill(DataRow row)
        {
            return new Bill
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
        }
    }
} 