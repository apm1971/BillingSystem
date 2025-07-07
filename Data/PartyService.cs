using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Data
{
    public class PartyService
    {
        // Get all parties
        public static List<Party> GetAllParties()
        {
            List<Party> parties = new List<Party>();
            
            string sql = "SELECT * FROM PartyMaster ORDER BY PartyName";
            DataTable dt = DatabaseManager.ExecuteQuery(sql);
            
            foreach (DataRow row in dt.Rows)
            {
                parties.Add(MapRowToParty(row));
            }
            
            return parties;
        }
        
        // Get party by ID
        public static Party? GetPartyByID(int partyID)
        {
            string sql = "SELECT * FROM PartyMaster WHERE PartyID = @PartyID";
            SQLiteParameter param = new SQLiteParameter("@PartyID", partyID);
            
            DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
            
            if (dt.Rows.Count > 0)
            {
                return MapRowToParty(dt.Rows[0]);
            }
            
            return null;
        }
        
        // Search parties
        public static List<Party> SearchParties(string searchText)
        {
            List<Party> parties = new List<Party>();
            
            string sql = "SELECT * FROM PartyMaster WHERE PartyName LIKE ? OR City LIKE ? OR Phone LIKE ? ORDER BY PartyName";
            
            string param = "%" + searchText + "%";
            DataTable dt = DatabaseManager.ExecuteQuery(sql, 
                new SQLiteParameter("@PartyName", param),
                new SQLiteParameter("@City", param),
                new SQLiteParameter("@Phone", param));
                
            foreach (DataRow row in dt.Rows)
            {
                parties.Add(MapRowToParty(row));
            }
            
            return parties;
        }
        
        // Add a new party
        public static bool AddParty(Party party)
        {
            string sql = @"INSERT INTO PartyMaster (
                PartyName, Address, City, Phone, Email, CreditLimit, CreditDays, OutstandingAmount, BrokerID, BrokerName
            ) VALUES (
                @PartyName, @Address, @City, @Phone, @Email, @CreditLimit, @CreditDays, @OutstandingAmount, @BrokerID, @BrokerName
            )";
            
            SQLiteParameter[] parameters = {
                new SQLiteParameter("@PartyName", party.PartyName),
                new SQLiteParameter("@Address", party.Address),
                new SQLiteParameter("@City", party.City),
                new SQLiteParameter("@Phone", party.Phone),
                new SQLiteParameter("@Email", party.Email),
                new SQLiteParameter("@CreditLimit", party.CreditLimit),
                new SQLiteParameter("@CreditDays", party.CreditDays),
                new SQLiteParameter("@OutstandingAmount", party.OutstandingAmount),
                new SQLiteParameter("@BrokerID", party.BrokerID.HasValue ? (object)party.BrokerID.Value : DBNull.Value),
                new SQLiteParameter("@BrokerName", party.BrokerName ?? string.Empty)
            };
            
            int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
            
            return result > 0;
        }
        
        // Update an existing party
        public static bool UpdateParty(Party party)
        {
            string sql = @"UPDATE PartyMaster SET 
                PartyName = @PartyName,
                Address = @Address,
                City = @City,
                Phone = @Phone,
                Email = @Email,
                CreditLimit = @CreditLimit,
                CreditDays = @CreditDays,
                OutstandingAmount = @OutstandingAmount,
                BrokerID = @BrokerID,
                BrokerName = @BrokerName
            WHERE PartyID = @PartyID";
            
            SQLiteParameter[] parameters = {
                new SQLiteParameter("@PartyName", party.PartyName),
                new SQLiteParameter("@Address", party.Address),
                new SQLiteParameter("@City", party.City),
                new SQLiteParameter("@Phone", party.Phone),
                new SQLiteParameter("@Email", party.Email),
                new SQLiteParameter("@CreditLimit", party.CreditLimit),
                new SQLiteParameter("@CreditDays", party.CreditDays),
                new SQLiteParameter("@OutstandingAmount", party.OutstandingAmount),
                new SQLiteParameter("@BrokerID", party.BrokerID.HasValue ? (object)party.BrokerID.Value : DBNull.Value),
                new SQLiteParameter("@BrokerName", party.BrokerName ?? string.Empty),
                new SQLiteParameter("@PartyID", party.PartyID)
            };
            
            int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
            
            return result > 0;
        }
        
        // Delete a party
        public static bool DeleteParty(int partyID)
        {
            string sql = "DELETE FROM PartyMaster WHERE PartyID = @PartyID";
            SQLiteParameter param = new SQLiteParameter("@PartyID", partyID);
            
            int result = DatabaseManager.ExecuteNonQuery(sql, param);
            
            return result > 0;
        }
        
        // Check if party exists with the same name
        public static bool PartyExists(string partyName, int? excludePartyID = null)
        {
            string sql = "SELECT COUNT(*) FROM PartyMaster WHERE PartyName = @PartyName";
            SQLiteParameter[] parameters = { new SQLiteParameter("@PartyName", partyName) };
            
            if (excludePartyID.HasValue)
            {
                sql += " AND PartyID <> @PartyID";
                parameters = new SQLiteParameter[] {
                    new SQLiteParameter("@PartyName", partyName),
                    new SQLiteParameter("@PartyID", excludePartyID.Value)
                };
            }
            
            object result = DatabaseManager.ExecuteScalar(sql, parameters);
            
            return Convert.ToInt32(result) > 0;
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
                CreditLimit = Convert.ToDouble(row["CreditLimit"]),
                CreditDays = Convert.ToInt32(row["CreditDays"]),
                OutstandingAmount = Convert.ToDouble(row["OutstandingAmount"]),
                BrokerID = row["BrokerID"] != DBNull.Value ? Convert.ToInt32(row["BrokerID"]) : null,
                BrokerName = row["BrokerName"]?.ToString() ?? string.Empty
            };
        }
    }
} 