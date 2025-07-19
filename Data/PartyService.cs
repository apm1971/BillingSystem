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
            
            string sql = "SELECT * FROM PartyMaster WHERE CompanyID = ? ORDER BY PartyName";
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
            
            string sql = "SELECT * FROM PartyMaster WHERE PartyID = ? AND CompanyID = ?";
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
            
            string sql = "SELECT * FROM PartyMaster WHERE (PartyName LIKE ? OR City LIKE ? OR Phone LIKE ?) AND CompanyID = ? ORDER BY PartyName";
            
            string param = "%" + searchText + "%";
            DataTable dt = DatabaseManager.ExecuteQuery(sql, 
                new OleDbParameter("PartyName", OleDbType.VarChar) { Value = param },
                new OleDbParameter("City", OleDbType.VarChar) { Value = param },
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
                (PartyName, Address, City, Phone, Email, CreditDays, BrokerID, BrokerName, CompanyID) 
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)";
            
            OleDbParameter[] parameters = {
                new OleDbParameter("PartyName", party.PartyName),
                new OleDbParameter("Address", party.Address),
                new OleDbParameter("City", party.City),
                new OleDbParameter("Phone", party.Phone),
                new OleDbParameter("Email", party.Email),
                // new OleDbParameter("GSTNo", party.GSTNo),
                // new OleDbParameter("PAN", party.PAN),
                // new OleDbParameter("OpeningBalance", party.OpeningBalance),
                // new OleDbParameter("OpeningBalanceDate", party.OpeningBalanceDate),
                new OleDbParameter("CreditDays", party.CreditDays),
                party.BrokerID.HasValue ? new OleDbParameter("BrokerID", party.BrokerID) : new OleDbParameter("BrokerID", DBNull.Value),
                new OleDbParameter("BrokerName", party.BrokerName ?? string.Empty),
                new OleDbParameter("CompanyID", companyID)
            };
            
            int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
            
            return result > 0;
        }
        
        // Update an existing party
        public static bool UpdateParty(Party party)
        {
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            string sql = @"UPDATE PartyMaster SET 
                PartyName = ?, Address = ?, City = ?, Phone = ?, Email = ?, 
                CreditDays = ?, BrokerID = ?, BrokerName = ?, CompanyID = ? 
                WHERE PartyID = ?";
            
            OleDbParameter[] parameters = {
                new OleDbParameter("PartyName", party.PartyName),
                new OleDbParameter("Address", party.Address),
                new OleDbParameter("City", party.City),
                new OleDbParameter("Phone", party.Phone),
                new OleDbParameter("Email", party.Email),
                // new OleDbParameter("GSTNo", party.GSTNo),
                // new OleDbParameter("PAN", party.PAN),
                // new OleDbParameter("OpeningBalance", party.OpeningBalance),
                // new OleDbParameter("OpeningBalanceDate", party.OpeningBalanceDate),
                new OleDbParameter("CreditDays", party.CreditDays),
                party.BrokerID.HasValue ? new OleDbParameter("BrokerID", party.BrokerID) : new OleDbParameter("BrokerID", DBNull.Value),
                new OleDbParameter("BrokerName", party.BrokerName ?? string.Empty),
                new OleDbParameter("CompanyID", companyID),
                new OleDbParameter("PartyID", party.PartyID)
            };
            
            int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
            
            return result > 0;
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
                City = row["City"].ToString(),
                Phone = row["Phone"].ToString(),
                Email = row["Email"].ToString(),
                // GSTNo = row["GSTNo"] != DBNull.Value ? row["GSTNo"].ToString() : string.Empty,
                // PAN = row["PAN"] != DBNull.Value ? row["PAN"].ToString() : string.Empty,
                // OpeningBalance = row["OpeningBalance"] != DBNull.Value ? Convert.ToDouble(row["OpeningBalance"]) : 0,
                // OpeningBalanceDate = row["OpeningBalanceDate"] != DBNull.Value ? Convert.ToDateTime(row["OpeningBalanceDate"]) : DateTime.Today,
                CreditDays = row["CreditDays"] != DBNull.Value ? Convert.ToInt32(row["CreditDays"]) : 0,
                BrokerID = row["BrokerID"] != DBNull.Value ? Convert.ToInt32(row["BrokerID"]) : (int?)null,
                BrokerName = row["BrokerName"] != DBNull.Value ? row["BrokerName"].ToString() : string.Empty,
                CompanyID = row["CompanyID"] != DBNull.Value ? Convert.ToInt32(row["CompanyID"]) : 0
            };
        }
    }
} 