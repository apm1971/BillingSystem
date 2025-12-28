using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Data
{
    public class GodownService
    {
        public static List<Godown> GetAllGodowns()
        {
            List<Godown> godowns = new List<Godown>();
            
            string sql = "SELECT * FROM GodownMaster ORDER BY GodownName";
            DataTable dt = DatabaseManager.ExecuteQuery(sql);
            
            foreach (DataRow row in dt.Rows)
            {
                godowns.Add(MapRowToGodown(row));
            }
            
            return godowns;
        }
        
        public static Godown? GetGodownByID(int godownID)
        {
            string sql = "SELECT * FROM GodownMaster WHERE GodownID = ?";
            OleDbParameter[] parameters = {
                new OleDbParameter("GodownID", OleDbType.Integer) { Value = godownID }
            };
            
            DataTable dt = DatabaseManager.ExecuteQuery(sql, parameters);
            
            if (dt.Rows.Count > 0)
            {
                return MapRowToGodown(dt.Rows[0]);
            }
            
            return null;
        }
        
        public static bool AddGodown(Godown godown)
        {
            string sql = @"INSERT INTO GodownMaster (GodownName, GodownShortName) 
                         VALUES (?, ?)";
            
            OleDbParameter[] parameters = {
                new OleDbParameter("GodownName", OleDbType.VarChar) { Value = godown.GodownName },
                new OleDbParameter("GodownShortName", OleDbType.VarChar) { Value = (object)godown.GodownShortName ?? DBNull.Value }
            };
            
            int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
            return result > 0;
        }
        
        public static bool UpdateGodown(Godown godown)
        {
            try
            {
                string sql = @"UPDATE GodownMaster SET 
                             GodownName = ?, GodownShortName = ? 
                             WHERE GodownID = ?";
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("GodownName", OleDbType.VarChar) { Value = godown.GodownName },
                    new OleDbParameter("GodownShortName", OleDbType.VarChar) { Value = (object)godown.GodownShortName ?? DBNull.Value },
                    new OleDbParameter("GodownID", OleDbType.Integer) { Value = godown.GodownID }
                };
                
                int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
                return result > 0;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error updating godown: {ex.Message}", "Database Error", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }
        
        public static bool DeleteGodown(int godownID)
        {
            // Check if godown has transactions
            string checkSql = @"SELECT COUNT(*) FROM GodownTransactionMaster 
                               WHERE FromGodownID = ? OR ToGodownID = ?";
            OleDbParameter[] checkParams = {
                new OleDbParameter("GodownID1", OleDbType.Integer) { Value = godownID },
                new OleDbParameter("GodownID2", OleDbType.Integer) { Value = godownID }
            };
            
            int transactionCount = Convert.ToInt32(DatabaseManager.ExecuteScalar(checkSql, checkParams));
            
            if (transactionCount > 0)
            {
                return false; // Cannot delete godown with transactions
            }
            
            // Check if godown has opening stock
            string checkOpeningStockSql = "SELECT COUNT(*) FROM GodownOpeningStock WHERE GodownID = ?";
            OleDbParameter[] checkOpeningStockParams = {
                new OleDbParameter("GodownID", OleDbType.Integer) { Value = godownID }
            };
            
            int openingStockCount = Convert.ToInt32(DatabaseManager.ExecuteScalar(checkOpeningStockSql, checkOpeningStockParams));
            
            if (openingStockCount > 0)
            {
                return false; // Cannot delete godown with opening stock
            }
            
            string sql = "DELETE FROM GodownMaster WHERE GodownID = ?";
            OleDbParameter[] parameters = {
                new OleDbParameter("GodownID", OleDbType.Integer) { Value = godownID }
            };
            
            int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
            return result > 0;
        }
        
        public static bool GodownExists(string godownName, int? excludeGodownID = null)
        {
            string sql = "SELECT COUNT(*) FROM GodownMaster WHERE GodownName = ?";
            List<OleDbParameter> parameters = new List<OleDbParameter>
            {
                new OleDbParameter("GodownName", OleDbType.VarChar) { Value = godownName.Trim() }
            };
            
            if (excludeGodownID.HasValue)
            {
                sql += " AND GodownID <> ?";
                parameters.Add(new OleDbParameter("GodownID", OleDbType.Integer) { Value = excludeGodownID.Value });
            }
            
            int count = Convert.ToInt32(DatabaseManager.ExecuteScalar(sql, parameters.ToArray()));
            return count > 0;
        }
        
        private static Godown MapRowToGodown(DataRow row)
        {
            return new Godown
            {
                GodownID = Convert.ToInt32(row["GodownID"]),
                GodownName = row["GodownName"].ToString(),
                GodownShortName = row["GodownShortName"] != DBNull.Value ? row["GodownShortName"].ToString() : ""
            };
        }
    }
}

