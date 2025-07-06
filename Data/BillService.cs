using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Data
{
    public class BillService
    {
        // Get all bills
        public static List<Bill> GetAllBills()
        {
            List<Bill> bills = new List<Bill>();
            
            string sql = "SELECT * FROM BillMaster ORDER BY BillDate DESC";
            DataTable dt = DatabaseManager.ExecuteQuery(sql);
            
            foreach (DataRow row in dt.Rows)
            {
                Bill bill = new Bill
                {
                    BillID = Convert.ToInt32(row["BillID"]),
                    BillNo = row["BillNo"].ToString(),
                    BillDate = Convert.ToDateTime(row["BillDate"]),
                    PartyID = Convert.ToInt32(row["PartyID"]),
                    PartyName = row["PartyName"].ToString(),
                    TotalAmount = Convert.ToDouble(row["TotalAmount"]),
                    TotalGST = Convert.ToDouble(row["TotalGST"]),
                    NetAmount = Convert.ToDouble(row["NetAmount"])
                };
                
                // Get bill details
                bill.BillItems = GetBillDetails(bill.BillID);
                
                bills.Add(bill);
            }
            
            return bills;
        }
        
        // Get bill details
        private static List<BillItem> GetBillDetails(int billID)
        {
            List<BillItem> billItems = new List<BillItem>();
            
            string sql = "SELECT * FROM BillDetails WHERE BillID = @BillID";
            SQLiteParameter param = new SQLiteParameter("@BillID", billID);
            
            DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
            
            foreach (DataRow row in dt.Rows)
            {
                BillItem item = new BillItem
                {
                    BillDetailID = Convert.ToInt32(row["BillDetailID"]),
                    BillID = billID,
                    ItemID = Convert.ToInt32(row["ItemID"]),
                    ItemName = row["ItemName"].ToString(),
                    Quantity = Convert.ToDouble(row["Quantity"]),
                    Rate = Convert.ToDouble(row["Rate"]),
                    Amount = Convert.ToDouble(row["Amount"]),
                    GSTPct = Convert.ToDouble(row["GSTPct"]),
                    GSTAmount = Convert.ToDouble(row["GSTAmount"]),
                    TotalAmount = Convert.ToDouble(row["TotalAmount"])
                };
                
                billItems.Add(item);
            }
            
            return billItems;
        }
        
        // Get bill by ID
        public static Bill? GetBillByID(int billID)
        {
            string sql = "SELECT * FROM BillMaster WHERE BillID = @BillID";
            SQLiteParameter param = new SQLiteParameter("@BillID", billID);
            
            DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
            
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                
                Bill bill = new Bill
                {
                    BillID = Convert.ToInt32(row["BillID"]),
                    BillNo = row["BillNo"].ToString(),
                    BillDate = Convert.ToDateTime(row["BillDate"]),
                    PartyID = Convert.ToInt32(row["PartyID"]),
                    PartyName = row["PartyName"].ToString(),
                    TotalAmount = Convert.ToDouble(row["TotalAmount"]),
                    TotalGST = Convert.ToDouble(row["TotalGST"]),
                    NetAmount = Convert.ToDouble(row["NetAmount"])
                };
                
                // Get bill details
                bill.BillItems = GetBillDetails(bill.BillID);
                
                return bill;
            }
            
            return null;
        }
        
        // Save bill (add or update)
        public static bool SaveBill(Bill bill)
        {
            using (SQLiteConnection conn = DatabaseManager.GetConnection())
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        if (bill.BillID == 0)
                        {
                            // Add new bill
                            string sql = @"INSERT INTO BillMaster (
                                BillNo, BillDate, PartyID, PartyName, TotalAmount, TotalGST, NetAmount
                            ) VALUES (
                                @BillNo, @BillDate, @PartyID, @PartyName, @TotalAmount, @TotalGST, @NetAmount
                            )";
                            
                            SQLiteParameter[] parameters = {
                                new SQLiteParameter("@BillNo", bill.BillNo),
                                new SQLiteParameter("@BillDate", bill.BillDate),
                                new SQLiteParameter("@PartyID", bill.PartyID),
                                new SQLiteParameter("@PartyName", bill.PartyName),
                                new SQLiteParameter("@TotalAmount", bill.TotalAmount),
                                new SQLiteParameter("@TotalGST", bill.TotalGST),
                                new SQLiteParameter("@NetAmount", bill.NetAmount)
                            };
                            
                            ExecuteNonQuery(conn, sql, parameters);
                            
                            // Get the new bill ID
                            sql = "SELECT last_insert_rowid()";
                            object result = ExecuteScalar(conn, sql);
                            bill.BillID = Convert.ToInt32(result);
                        }
                        else
                        {
                            // Update existing bill
                            string sql = @"UPDATE BillMaster SET 
                                BillNo = @BillNo,
                                BillDate = @BillDate,
                                PartyID = @PartyID,
                                PartyName = @PartyName,
                                TotalAmount = @TotalAmount,
                                TotalGST = @TotalGST,
                                NetAmount = @NetAmount
                            WHERE BillID = @BillID";
                            
                            SQLiteParameter[] parameters = {
                                new SQLiteParameter("@BillNo", bill.BillNo),
                                new SQLiteParameter("@BillDate", bill.BillDate),
                                new SQLiteParameter("@PartyID", bill.PartyID),
                                new SQLiteParameter("@PartyName", bill.PartyName),
                                new SQLiteParameter("@TotalAmount", bill.TotalAmount),
                                new SQLiteParameter("@TotalGST", bill.TotalGST),
                                new SQLiteParameter("@NetAmount", bill.NetAmount),
                                new SQLiteParameter("@BillID", bill.BillID)
                            };
                            
                            ExecuteNonQuery(conn, sql, parameters);
                            
                            // Delete existing bill details
                            sql = "DELETE FROM BillDetails WHERE BillID = @BillID";
                            ExecuteNonQuery(conn, sql, new SQLiteParameter("@BillID", bill.BillID));
                        }
                        
                        // Add bill details
                        foreach (BillItem item in bill.BillItems)
                        {
                            string sql = @"INSERT INTO BillDetails (
                                BillID, ItemID, ItemName, Quantity, Rate, Amount, GSTPct, GSTAmount, TotalAmount
                            ) VALUES (
                                @BillID, @ItemID, @ItemName, @Quantity, @Rate, @Amount, @GSTPct, @GSTAmount, @TotalAmount
                            )";
                            
                            SQLiteParameter[] parameters = {
                                new SQLiteParameter("@BillID", bill.BillID),
                                new SQLiteParameter("@ItemID", item.ItemID),
                                new SQLiteParameter("@ItemName", item.ItemName),
                                new SQLiteParameter("@Quantity", item.Quantity),
                                new SQLiteParameter("@Rate", item.Rate),
                                new SQLiteParameter("@Amount", item.Amount),
                                new SQLiteParameter("@GSTPct", item.GSTPct),
                                new SQLiteParameter("@GSTAmount", item.GSTAmount),
                                new SQLiteParameter("@TotalAmount", item.TotalAmount)
                            };
                            
                            ExecuteNonQuery(conn, sql, parameters);
                        }
                        
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Windows.Forms.MessageBox.Show($"Error saving bill: {ex.Message}", "Database Error",
                            System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
        }
        
        // Delete bill
        public static bool DeleteBill(int billID)
        {
            using (SQLiteConnection conn = DatabaseManager.GetConnection())
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Delete bill details
                        string sql = "DELETE FROM BillDetails WHERE BillID = @BillID";
                        ExecuteNonQuery(conn, sql, new SQLiteParameter("@BillID", billID));
                        
                        // Delete bill master
                        sql = "DELETE FROM BillMaster WHERE BillID = @BillID";
                        ExecuteNonQuery(conn, sql, new SQLiteParameter("@BillID", billID));
                        
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Windows.Forms.MessageBox.Show($"Error deleting bill: {ex.Message}", "Database Error",
                            System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
        }
        
        // Execute non-query with transaction
        private static int ExecuteNonQuery(SQLiteConnection conn, string sql, params SQLiteParameter[] parameters)
        {
            using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
            {
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }
                return cmd.ExecuteNonQuery();
            }
        }
        
        // Execute scalar with transaction
        private static object ExecuteScalar(SQLiteConnection conn, string sql, params SQLiteParameter[] parameters)
        {
            using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
            {
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }
                return cmd.ExecuteScalar();
            }
        }
    }
} 