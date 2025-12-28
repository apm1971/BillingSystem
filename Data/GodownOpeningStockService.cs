using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Data
{
    public class GodownOpeningStockService
    {
        public static List<GodownOpeningStock> GetOpeningStockByGodown(int godownID)
        {
            List<GodownOpeningStock> stocks = new List<GodownOpeningStock>();
            
            string sql = @"SELECT os.*, g.GodownName, gi.ItemName 
                          FROM ((GodownOpeningStock os
                          INNER JOIN GodownMaster g ON os.GodownID = g.GodownID)
                          INNER JOIN GodownItemMaster gi ON os.GodownItemID = gi.GodownItemID)
                          WHERE os.GodownID = ?
                          ORDER BY gi.ItemName";
            
            OleDbParameter[] parameters = {
                new OleDbParameter("GodownID", OleDbType.Integer) { Value = godownID }
            };
            
            DataTable dt = DatabaseManager.ExecuteQuery(sql, parameters);
            
            foreach (DataRow row in dt.Rows)
            {
                stocks.Add(MapRowToOpeningStock(row));
            }
            
            return stocks;
        }
        
        public static GodownOpeningStock? GetOpeningStock(int godownID, int godownItemID)
        {
            string sql = @"SELECT os.*, g.GodownName, gi.ItemName 
                          FROM ((GodownOpeningStock os
                          INNER JOIN GodownMaster g ON os.GodownID = g.GodownID)
                          INNER JOIN GodownItemMaster gi ON os.GodownItemID = gi.GodownItemID)
                          WHERE os.GodownID = ? AND os.GodownItemID = ?";
            
            OleDbParameter[] parameters = {
                new OleDbParameter("GodownID", OleDbType.Integer) { Value = godownID },
                new OleDbParameter("GodownItemID", OleDbType.Integer) { Value = godownItemID }
            };
            
            DataTable dt = DatabaseManager.ExecuteQuery(sql, parameters);
            
            if (dt.Rows.Count > 0)
            {
                return MapRowToOpeningStock(dt.Rows[0]);
            }
            
            return null;
        }
        
        public static bool AddOpeningStock(GodownOpeningStock stock)
        {
            string sql = @"INSERT INTO GodownOpeningStock (GodownID, GodownItemID, Quantity, AsOnDate) 
                         VALUES (?, ?, ?, ?)";
            
            OleDbParameter[] parameters = {
                new OleDbParameter("GodownID", OleDbType.Integer) { Value = stock.GodownID },
                new OleDbParameter("GodownItemID", OleDbType.Integer) { Value = stock.GodownItemID },
                new OleDbParameter("Quantity", OleDbType.Double) { Value = stock.Quantity },
                new OleDbParameter("AsOnDate", OleDbType.Date) { Value = stock.AsOnDate }
            };
            
            int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
            return result > 0;
        }
        
        public static bool UpdateOpeningStock(GodownOpeningStock stock)
        {
            try
            {
                string sql = @"UPDATE GodownOpeningStock SET 
                             Quantity = ?, AsOnDate = ? 
                             WHERE OpeningStockID = ?";
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("Quantity", OleDbType.Double) { Value = stock.Quantity },
                    new OleDbParameter("AsOnDate", OleDbType.Date) { Value = stock.AsOnDate },
                    new OleDbParameter("OpeningStockID", OleDbType.Integer) { Value = stock.OpeningStockID }
                };
                
                int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
                return result > 0;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error updating opening stock: {ex.Message}", "Database Error", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }
        
        public static bool DeleteOpeningStock(int openingStockID)
        {
            string sql = "DELETE FROM GodownOpeningStock WHERE OpeningStockID = ?";
            OleDbParameter[] parameters = {
                new OleDbParameter("OpeningStockID", OleDbType.Integer) { Value = openingStockID }
            };
            
            int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
            return result > 0;
        }
        
        public static bool SaveOpeningStockBatch(int godownID, List<GodownOpeningStock> stocks, DateTime asOnDate)
        {
            using (var conn = DatabaseManager.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Delete existing opening stock for this godown
                        string deleteSql = "DELETE FROM GodownOpeningStock WHERE GodownID = ?";
                        using (var deleteCmd = new OleDbCommand(deleteSql, conn, transaction))
                        {
                            deleteCmd.Parameters.Add(new OleDbParameter("GodownID", OleDbType.Integer) { Value = godownID });
                            deleteCmd.ExecuteNonQuery();
                        }
                        
                        // Insert new opening stock
                        string insertSql = @"INSERT INTO GodownOpeningStock (GodownID, GodownItemID, Quantity, AsOnDate) 
                                            VALUES (?, ?, ?, ?)";
                        
                        foreach (var stock in stocks)
                        {
                            if (stock.Quantity > 0)
                            {
                                using (var insertCmd = new OleDbCommand(insertSql, conn, transaction))
                                {
                                    insertCmd.Parameters.Add(new OleDbParameter("GodownID", OleDbType.Integer) { Value = godownID });
                                    insertCmd.Parameters.Add(new OleDbParameter("GodownItemID", OleDbType.Integer) { Value = stock.GodownItemID });
                                    insertCmd.Parameters.Add(new OleDbParameter("Quantity", OleDbType.Double) { Value = stock.Quantity });
                                    insertCmd.Parameters.Add(new OleDbParameter("AsOnDate", OleDbType.Date) { Value = asOnDate });
                                    insertCmd.ExecuteNonQuery();
                                }
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
        
        private static GodownOpeningStock MapRowToOpeningStock(DataRow row)
        {
            return new GodownOpeningStock
            {
                OpeningStockID = Convert.ToInt32(row["OpeningStockID"]),
                GodownID = Convert.ToInt32(row["GodownID"]),
                GodownName = row["GodownName"].ToString(),
                GodownItemID = Convert.ToInt32(row["GodownItemID"]),
                ItemName = row["ItemName"].ToString(),
                Quantity = Convert.ToDouble(row["Quantity"]),
                AsOnDate = Convert.ToDateTime(row["AsOnDate"])
            };
        }
    }
}

