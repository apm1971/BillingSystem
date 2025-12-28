using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Data
{
    public class GodownItemService
    {
        public static List<GodownItem> GetAllGodownItems()
        {
            List<GodownItem> items = new List<GodownItem>();
            
            string sql = "SELECT * FROM GodownItemMaster ORDER BY ItemName";
            DataTable dt = DatabaseManager.ExecuteQuery(sql);
            
            foreach (DataRow row in dt.Rows)
            {
                items.Add(MapRowToGodownItem(row));
            }
            
            return items;
        }
        
        public static GodownItem? GetGodownItemByID(int godownItemID)
        {
            string sql = "SELECT * FROM GodownItemMaster WHERE GodownItemID = ?";
            OleDbParameter[] parameters = {
                new OleDbParameter("GodownItemID", OleDbType.Integer) { Value = godownItemID }
            };
            
            DataTable dt = DatabaseManager.ExecuteQuery(sql, parameters);
            
            if (dt.Rows.Count > 0)
            {
                return MapRowToGodownItem(dt.Rows[0]);
            }
            
            return null;
        }
        
        public static bool AddGodownItem(GodownItem item)
        {
            string sql = @"INSERT INTO GodownItemMaster (ItemName) 
                         VALUES (?)";
            
            OleDbParameter[] parameters = {
                new OleDbParameter("ItemName", OleDbType.VarChar) { Value = item.ItemName }
            };
            
            int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
            return result > 0;
        }
        
        public static bool UpdateGodownItem(GodownItem item)
        {
            try
            {
                string sql = @"UPDATE GodownItemMaster SET 
                             ItemName = ? 
                             WHERE GodownItemID = ?";
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("ItemName", OleDbType.VarChar) { Value = item.ItemName },
                    new OleDbParameter("GodownItemID", OleDbType.Integer) { Value = item.GodownItemID }
                };
                
                int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
                return result > 0;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error updating godown item: {ex.Message}", "Database Error", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }
        
        public static bool DeleteGodownItem(int godownItemID)
        {
            // Check if item exists in transactions
            string checkSql = @"SELECT COUNT(*) FROM GodownTransactionDetails 
                               WHERE GodownItemID = ?";
            OleDbParameter[] checkParams = {
                new OleDbParameter("GodownItemID", OleDbType.Integer) { Value = godownItemID }
            };
            
            int transactionCount = Convert.ToInt32(DatabaseManager.ExecuteScalar(checkSql, checkParams));
            
            if (transactionCount > 0)
            {
                return false; // Cannot delete item with transactions
            }
            
            // Check if item exists in opening stock
            string checkOpeningStockSql = "SELECT COUNT(*) FROM GodownOpeningStock WHERE GodownItemID = ?";
            OleDbParameter[] checkOpeningStockParams = {
                new OleDbParameter("GodownItemID", OleDbType.Integer) { Value = godownItemID }
            };
            
            int openingStockCount = Convert.ToInt32(DatabaseManager.ExecuteScalar(checkOpeningStockSql, checkOpeningStockParams));
            
            if (openingStockCount > 0)
            {
                return false; // Cannot delete item with opening stock
            }
            
            string sql = "DELETE FROM GodownItemMaster WHERE GodownItemID = ?";
            OleDbParameter[] parameters = {
                new OleDbParameter("GodownItemID", OleDbType.Integer) { Value = godownItemID }
            };
            
            int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
            return result > 0;
        }
        
        public static bool GodownItemExists(string itemName, int? excludeGodownItemID = null)
        {
            string sql = "SELECT COUNT(*) FROM GodownItemMaster WHERE ItemName = ?";
            List<OleDbParameter> parameters = new List<OleDbParameter>
            {
                new OleDbParameter("ItemName", OleDbType.VarChar) { Value = itemName.Trim() }
            };
            
            if (excludeGodownItemID.HasValue)
            {
                sql += " AND GodownItemID <> ?";
                parameters.Add(new OleDbParameter("GodownItemID", OleDbType.Integer) { Value = excludeGodownItemID.Value });
            }
            
            int count = Convert.ToInt32(DatabaseManager.ExecuteScalar(sql, parameters.ToArray()));
            return count > 0;
        }
        
        private static GodownItem MapRowToGodownItem(DataRow row)
        {
            return new GodownItem
            {
                GodownItemID = Convert.ToInt32(row["GodownItemID"]),
                ItemName = row["ItemName"].ToString()
            };
        }
    }
}

