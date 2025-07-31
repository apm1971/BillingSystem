using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb; // Changed from SQLite
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Data
{
    public class PartyService
    {
        // Get all parties
        public static List<Party> GetAllParties()
        {
            List<Party> parties = new List<Party>();
            
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            string sql = @"SELECT p.*, b.BrokerName 
                          FROM PartyMaster p 
                          LEFT JOIN BrokerMaster b ON p.BrokerID = b.BrokerID AND p.CompanyID = b.CompanyID 
                          WHERE p.CompanyID = ? 
                          ORDER BY p.PartyName";
            OleDbParameter param = new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID };
            
            DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
            
            foreach (DataRow row in dt.Rows)
            {
                parties.Add(MapRowToParty(row));
            }
            
            return parties;
        }
        
        // Get party by ID
        public static Party? GetPartyByID(int partyID)
        {
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            string sql = @"SELECT p.*, b.BrokerName 
                          FROM PartyMaster p 
                          LEFT JOIN BrokerMaster b ON p.BrokerID = b.BrokerID AND p.CompanyID = b.CompanyID 
                          WHERE p.PartyID = ? AND p.CompanyID = ?";
            OleDbParameter[] parameters = {
                new OleDbParameter("PartyID", OleDbType.Integer) { Value = partyID },
                new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID }
            };
            
            DataTable dt = DatabaseManager.ExecuteQuery(sql, parameters);
            
            if (dt.Rows.Count > 0)
            {
                return MapRowToParty(dt.Rows[0]);
            }
            
            return null;
        }
        
        // Check if party exists by name
        public static bool PartyExists(string partyName, int? excludePartyID = null)
        {
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            string sql;
            List<OleDbParameter> paramsList = new List<OleDbParameter>();
            
            if (excludePartyID.HasValue)
            {
                // Check for parties with the same name but different ID
                sql = "SELECT COUNT(*) FROM PartyMaster WHERE PartyName = ? AND PartyID <> ? AND CompanyID = ?";
                paramsList.Add(new OleDbParameter("PartyName", OleDbType.VarChar) { Value = partyName });
                paramsList.Add(new OleDbParameter("PartyID", OleDbType.Integer) { Value = excludePartyID.Value });
                paramsList.Add(new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID });
            }
            else
            {
                // Check for any party with this name
                sql = "SELECT COUNT(*) FROM PartyMaster WHERE PartyName = ? AND CompanyID = ?";
                paramsList.Add(new OleDbParameter("PartyName", OleDbType.VarChar) { Value = partyName });
                paramsList.Add(new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID });
            }
            
            int count = Convert.ToInt32(DatabaseManager.ExecuteScalar(sql, paramsList.ToArray()));
            
            return count > 0;
        }
        
        // Search parties
        public static List<Party> SearchParties(string searchText)
        {
            List<Party> parties = new List<Party>();
            
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            string sql = @"SELECT p.*, b.BrokerName 
                          FROM PartyMaster p 
                          LEFT JOIN BrokerMaster b ON p.BrokerID = b.BrokerID AND p.CompanyID = b.CompanyID 
                          WHERE (p.PartyName LIKE ? OR p.Phone LIKE ?) AND p.CompanyID = ? 
                          ORDER BY p.PartyName";
            
            string param = "%" + searchText + "%";
            DataTable dt = DatabaseManager.ExecuteQuery(sql, 
                new OleDbParameter("PartyName", OleDbType.VarChar) { Value = param },
                new OleDbParameter("Phone", OleDbType.VarChar) { Value = param },
                new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID });
                
            foreach (DataRow row in dt.Rows)
            {
                parties.Add(MapRowToParty(row));
            }
            
            return parties;
        }
        
                 // Add a new party
         public static bool AddParty(Party party)
         {
             // Get the active company ID
             int companyID = Program.ActiveCompany?.CompanyID ?? 0;
             
             string sql = @"INSERT INTO PartyMaster 
                 (PartyName, Address, Phone, BrokerID, CompanyID) 
                 VALUES (?, ?, ?, ?, ?)";
             
             OleDbParameter[] parameters = {
                 new OleDbParameter("PartyName", party.PartyName),
                 new OleDbParameter("Address", party.Address),
                 new OleDbParameter("Phone", party.Phone),
                 party.BrokerID.HasValue ? new OleDbParameter("BrokerID", party.BrokerID) : new OleDbParameter("BrokerID", DBNull.Value),
                 new OleDbParameter("CompanyID", companyID)
             };
             
             int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
             
             return result > 0;
         }
        
                 // Update an existing party
         public static bool UpdateParty(Party party)
         {
             try
             {
                 // Get the active company ID
                 int companyID = Program.ActiveCompany?.CompanyID ?? 0;
                 
                 // Simple update without complex transaction
                 string sql = @"UPDATE PartyMaster SET 
                     PartyName = ?, Address = ?, Phone = ?, BrokerID = ? 
                     WHERE PartyID = ? AND CompanyID = ?";
                 
                 OleDbParameter[] parameters = {
                     new OleDbParameter("PartyName", party.PartyName),
                     new OleDbParameter("Address", party.Address),
                     new OleDbParameter("Phone", party.Phone),
                     party.BrokerID.HasValue ? new OleDbParameter("BrokerID", party.BrokerID) : new OleDbParameter("BrokerID", DBNull.Value),
                     new OleDbParameter("PartyID", party.PartyID),
                     new OleDbParameter("CompanyID", companyID)
                 };
                 
                 int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
                 
                 return result > 0;
             }
             catch (Exception ex)
             {
                 System.Windows.Forms.MessageBox.Show($"Error updating party: {ex.Message}", "Database Error", 
                     System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                 return false;
             }
         }
        
        // Delete a party
        public static bool DeleteParty(int partyID)
        {
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            // Check if party exists in bills
            string checkSql = "SELECT COUNT(*) FROM BillMaster WHERE PartyID = ? AND CompanyID = ?";
            OleDbParameter[] checkParams = {
                new OleDbParameter("PartyID", OleDbType.Integer) { Value = partyID },
                new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID }
            };
            
            int billCount = Convert.ToInt32(DatabaseManager.ExecuteScalar(checkSql, checkParams));
            
            if (billCount > 0)
            {
                // Party has bills, cannot delete
                return false;
            }
            
            // Safe to delete
            string sql = "DELETE FROM PartyMaster WHERE PartyID = ? AND CompanyID = ?";
            OleDbParameter[] parameters = {
                new OleDbParameter("PartyID", OleDbType.Integer) { Value = partyID },
                new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID }
            };
            
            int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
            
            return result > 0;
        }
        
        // Map DataRow to Party object
        private static Party MapRowToParty(DataRow row)
        {
            return new Party
            {
                PartyID = Convert.ToInt32(row["PartyID"]),
                PartyName = row["PartyName"].ToString(),
                Address = row["Address"].ToString(),
                Phone = row["Phone"].ToString(),
                BrokerID = row["BrokerID"] != DBNull.Value ? Convert.ToInt32(row["BrokerID"]) : (int?)null,
                BrokerName = row["BrokerName"] != DBNull.Value ? row["BrokerName"].ToString() : string.Empty,
                CompanyID = row["CompanyID"] != DBNull.Value ? Convert.ToInt32(row["CompanyID"]) : 0
            };
        }
    }
} 