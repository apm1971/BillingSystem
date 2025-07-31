using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb; // Changed from SQLite
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Data
{
    public class ItemService
    {
        // Get all items
        public static List<Item> GetAllItems()
        {
            List<Item> items = new List<Item>();
            
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            string sql = "SELECT * FROM ItemMaster WHERE CompanyID = ? ORDER BY ItemName";
            OleDbParameter param = new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID };
            
            DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
            
            foreach (DataRow row in dt.Rows)
            {
                Item item = new Item
                {
                    ItemID = Convert.ToInt32(row["ItemID"]),
                    ItemName = row["ItemName"].ToString(),
                    Unit = row["Unit"].ToString(),
                    DefaultRate = Convert.ToDecimal(row["DefaultRate"]),
                    Charges = Convert.ToDecimal(row["Charges"]),
                    CompanyID = Convert.ToInt32(row["CompanyID"])
                };
                
                items.Add(item);
            }
            
            return items;
        }
        
        // Get item by ID
        public static Item? GetItemByID(int itemID)
        {
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            string sql = "SELECT * FROM ItemMaster WHERE ItemID = ? AND CompanyID = ?";
            OleDbParameter[] parameters = {
                new OleDbParameter("ItemID", OleDbType.Integer) { Value = itemID },
                new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID }
            };
            
            DataTable dt = DatabaseManager.ExecuteQuery(sql, parameters);
            
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                
                Item item = new Item
                {
                    ItemID = Convert.ToInt32(row["ItemID"]),
                    ItemName = row["ItemName"].ToString(),
                    Unit = row["Unit"].ToString(),
                    DefaultRate = Convert.ToDecimal(row["DefaultRate"]),
                    Charges = Convert.ToDecimal(row["Charges"]),
                    CompanyID = Convert.ToInt32(row["CompanyID"])
                };
                
                return item;
            }
            
            return null;
        }
        
        // Search items
        public static List<Item> SearchItems(string searchText)
        {
            List<Item> items = new List<Item>();
            
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            string sql = "SELECT * FROM ItemMaster WHERE ItemName LIKE ? AND CompanyID = ? ORDER BY ItemName";
            
            string param = "%" + searchText + "%";
            DataTable dt = DatabaseManager.ExecuteQuery(sql, 
                new OleDbParameter("ItemName", OleDbType.VarChar) { Value = param },
                new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID });
                
            foreach (DataRow row in dt.Rows)
            {
                items.Add(MapRowToItem(row));
            }
            
            return items;
        }
        
        // Add a new item
        public static bool AddItem(Item item)
        {
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            string sql = @"INSERT INTO ItemMaster (ItemName, Unit, DefaultRate, Charges, CompanyID) 
                         VALUES (?, ?, ?, ?, ?)";
            
            OleDbParameter[] parameters = {
                new OleDbParameter("ItemName", OleDbType.VarChar) { Value = item.ItemName },
                new OleDbParameter("Unit", OleDbType.VarChar) { Value = item.Unit },
                new OleDbParameter("DefaultRate", OleDbType.Decimal) { Value = item.DefaultRate },
                new OleDbParameter("Charges", OleDbType.Decimal) { Value = item.Charges },
                new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID }
            };
            
            int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
            
            return result > 0;
        }
        
        // Update an existing item
        public static bool UpdateItem(Item item)
        {
            try
            {
                // Get the active company ID
                int companyID = Program.ActiveCompany?.CompanyID ?? 0;
                
                string sql = @"UPDATE ItemMaster SET 
                             ItemName = ?, Unit = ?, DefaultRate = ?, Charges = ? 
                             WHERE ItemID = ? AND CompanyID = ?";
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("ItemName", OleDbType.VarChar) { Value = item.ItemName },
                    new OleDbParameter("Unit", OleDbType.VarChar) { Value = item.Unit },
                    new OleDbParameter("DefaultRate", OleDbType.Decimal) { Value = item.DefaultRate },
                    new OleDbParameter("Charges", OleDbType.Decimal) { Value = item.Charges },
                    new OleDbParameter("ItemID", OleDbType.Integer) { Value = item.ItemID },
                    new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID }
                };
                
                int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
                
                return result > 0;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error updating item: {ex.Message}", "Database Error", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }
        
        // Delete an item
        public static bool DeleteItem(int itemID)
        {
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            // Check if item exists in bills
            string checkSql = @"SELECT COUNT(*) FROM BillDetails bd 
                               INNER JOIN BillMaster bm ON bd.BillID = bm.BillID 
                               WHERE bd.ItemID = ? AND bm.CompanyID = ?";
            OleDbParameter[] checkParams = {
                new OleDbParameter("ItemID", OleDbType.Integer) { Value = itemID },
                new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID }
            };
            
            int billCount = Convert.ToInt32(DatabaseManager.ExecuteScalar(checkSql, checkParams));
            
            if (billCount > 0)
            {
                // Item has bills, cannot delete
                return false;
            }
            
            string sql = "DELETE FROM ItemMaster WHERE ItemID = ? AND CompanyID = ?";
            OleDbParameter[] parameters = {
                new OleDbParameter("ItemID", OleDbType.Integer) { Value = itemID },
                new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID }
            };
            
            int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
            
            return result > 0;
        }
        
        // Map DataRow to Item object
        private static Item MapRowToItem(DataRow row)
        {
            return new Item
            {
                ItemID = Convert.ToInt32(row["ItemID"]),
                ItemName = row["ItemName"].ToString(),
                Unit = row["Unit"].ToString(),
                DefaultRate = Convert.ToDecimal(row["DefaultRate"]),
                Charges = Convert.ToDecimal(row["Charges"]),
                CompanyID = Convert.ToInt32(row["CompanyID"])
            };
        }
    }
} 