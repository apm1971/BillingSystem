using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Data
{
    public class ItemService
    {
        // Get all items
        public static List<Item> GetAllItems()
        {
            List<Item> items = new List<Item>();
            
            string sql = "SELECT * FROM ItemMaster ORDER BY ItemName";
            DataTable dt = DatabaseManager.ExecuteQuery(sql);
            
            foreach (DataRow row in dt.Rows)
            {
                Item item = new Item
                {
                    ItemID = Convert.ToInt32(row["ItemID"]),
                    ItemCode = row["ItemCode"].ToString(),
                    ItemName = row["ItemName"].ToString(),
                    Unit = row["Unit"].ToString(),
                    Rate = Convert.ToDouble(row["Rate"]),
                    GST = Convert.ToDouble(row["GST"]),
                    StockQuantity = Convert.ToDouble(row["StockQuantity"])
                };
                
                items.Add(item);
            }
            
            return items;
        }
        
        // Get item by ID
        public static Item? GetItemByID(int itemID)
        {
            string sql = "SELECT * FROM ItemMaster WHERE ItemID = @ItemID";
            SQLiteParameter param = new SQLiteParameter("@ItemID", itemID);
            
            DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
            
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                
                Item item = new Item
                {
                    ItemID = Convert.ToInt32(row["ItemID"]),
                    ItemCode = row["ItemCode"].ToString(),
                    ItemName = row["ItemName"].ToString(),
                    Unit = row["Unit"].ToString(),
                    Rate = Convert.ToDouble(row["Rate"]),
                    GST = Convert.ToDouble(row["GST"]),
                    StockQuantity = Convert.ToDouble(row["StockQuantity"])
                };
                
                return item;
            }
            
            return null;
        }
        
        // Search items
        public static List<Item> SearchItems(string searchText)
        {
            List<Item> items = new List<Item>();
            
            string sql = "SELECT * FROM ItemMaster WHERE ItemName LIKE ? OR ItemCode LIKE ? ORDER BY ItemName";
            
            string param = "%" + searchText + "%";
            DataTable dt = DatabaseManager.ExecuteQuery(sql, 
                new SQLiteParameter("@ItemName", param),
                new SQLiteParameter("@ItemCode", param));
                
            foreach (DataRow row in dt.Rows)
            {
                items.Add(MapRowToItem(row));
            }
            
            return items;
        }
        
        // Add a new item
        public static bool AddItem(Item item)
        {
            string sql = @"INSERT INTO ItemMaster (
                ItemCode, ItemName, Unit, Rate, GST, StockQuantity
            ) VALUES (
                @ItemCode, @ItemName, @Unit, @Rate, @GST, @StockQuantity
            )";
            
            SQLiteParameter[] parameters = {
                new SQLiteParameter("@ItemCode", item.ItemCode),
                new SQLiteParameter("@ItemName", item.ItemName),
                new SQLiteParameter("@Unit", item.Unit),
                new SQLiteParameter("@Rate", item.Rate),
                new SQLiteParameter("@GST", item.GST),
                new SQLiteParameter("@StockQuantity", item.StockQuantity)
            };
            
            int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
            
            return result > 0;
        }
        
        // Update an existing item
        public static bool UpdateItem(Item item)
        {
            string sql = @"UPDATE ItemMaster SET 
                ItemCode = @ItemCode,
                ItemName = @ItemName,
                Unit = @Unit,
                Rate = @Rate,
                GST = @GST,
                StockQuantity = @StockQuantity
            WHERE ItemID = @ItemID";
            
            SQLiteParameter[] parameters = {
                new SQLiteParameter("@ItemCode", item.ItemCode),
                new SQLiteParameter("@ItemName", item.ItemName),
                new SQLiteParameter("@Unit", item.Unit),
                new SQLiteParameter("@Rate", item.Rate),
                new SQLiteParameter("@GST", item.GST),
                new SQLiteParameter("@StockQuantity", item.StockQuantity),
                new SQLiteParameter("@ItemID", item.ItemID)
            };
            
            int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
            
            return result > 0;
        }
        
        // Delete an item
        public static bool DeleteItem(int itemID)
        {
            string sql = "DELETE FROM ItemMaster WHERE ItemID = @ItemID";
            SQLiteParameter param = new SQLiteParameter("@ItemID", itemID);
            
            int result = DatabaseManager.ExecuteNonQuery(sql, param);
            
            return result > 0;
        }
        
        // Check if item exists with the same code
        public static bool ItemCodeExists(string itemCode, int? excludeItemID = null)
        {
            string sql = "SELECT COUNT(*) FROM ItemMaster WHERE ItemCode = @ItemCode";
            SQLiteParameter[] parameters = { new SQLiteParameter("@ItemCode", itemCode) };
            
            if (excludeItemID.HasValue)
            {
                sql += " AND ItemID <> @ItemID";
                parameters = new SQLiteParameter[] {
                    new SQLiteParameter("@ItemCode", itemCode),
                    new SQLiteParameter("@ItemID", excludeItemID.Value)
                };
            }
            
            object result = DatabaseManager.ExecuteScalar(sql, parameters);
            
            return Convert.ToInt32(result) > 0;
        }
        
        // Map DataRow to Item object
        private static Item MapRowToItem(DataRow row)
        {
            return new Item
            {
                ItemID = Convert.ToInt32(row["ItemID"]),
                ItemCode = row["ItemCode"].ToString(),
                ItemName = row["ItemName"].ToString(),
                Unit = row["Unit"].ToString(),
                Rate = Convert.ToDouble(row["Rate"]),
                GST = Convert.ToDouble(row["GST"]),
                StockQuantity = Convert.ToDouble(row["StockQuantity"])
            };
        }
    }
} 