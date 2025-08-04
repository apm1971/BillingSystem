using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb; // Changed from SQLite
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Data
{
    public class BrokerService
    {
        // Get all brokers
        public static List<Broker> GetAllBrokers()
        {
            List<Broker> brokers = new List<Broker>();
            
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            string sql = "SELECT * FROM BrokerMaster WHERE CompanyID = ? ORDER BY BrokerName";
            OleDbParameter param = new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID };
            
            DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
            
            foreach (DataRow row in dt.Rows)
            {
                brokers.Add(MapRowToBroker(row));
            }
            
            return brokers;
        }
        
        // Get broker by ID
        public static Broker? GetBrokerByID(int brokerID)
        {
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            string sql = "SELECT * FROM BrokerMaster WHERE BrokerID = ? AND CompanyID = ?";
            OleDbParameter[] parameters = {
                new OleDbParameter("BrokerID", OleDbType.Integer) { Value = brokerID },
                new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID }
            };
            
            DataTable dt = DatabaseManager.ExecuteQuery(sql, parameters);
            
            if (dt.Rows.Count > 0)
            {
                return MapRowToBroker(dt.Rows[0]);
            }
            
            return null;
        }
        
        // Search brokers
        public static List<Broker> SearchBrokers(string searchText)
        {
            List<Broker> brokers = new List<Broker>();
            
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            string sql = "SELECT * FROM BrokerMaster WHERE (BrokerName LIKE ? OR Phone LIKE ?) AND CompanyID = ? ORDER BY BrokerName";
            
            string param = "%" + searchText + "%";
            DataTable dt = DatabaseManager.ExecuteQuery(sql, 
                new OleDbParameter("BrokerName", OleDbType.VarChar) { Value = param },
                new OleDbParameter("Phone", OleDbType.VarChar) { Value = param },
                new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID });
                
            foreach (DataRow row in dt.Rows)
            {
                brokers.Add(MapRowToBroker(row));
            }
            
            return brokers;
        }
        
        // Add a new broker
        public static bool AddBroker(Broker broker)
        {
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            string sql = @"INSERT INTO BrokerMaster (BrokerName, Phone, CompanyID, InterestDays, InterestRate, DiscountDays, DiscountRate, BrokerageRate) 
                         VALUES (?, ?, ?, ?, ?, ?, ?, ?)";
            
            OleDbParameter[] parameters = {
                new OleDbParameter("BrokerName", OleDbType.VarChar) { Value = broker.BrokerName },
                new OleDbParameter("Phone", OleDbType.VarChar) { Value = broker.Phone },
                new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID },
                new OleDbParameter("InterestDays", OleDbType.Integer) { Value = broker.InterestDays },
                new OleDbParameter("InterestRate", OleDbType.Decimal) { Value = broker.InterestRate },
                new OleDbParameter("DiscountDays", OleDbType.Integer) { Value = broker.DiscountDays },
                new OleDbParameter("DiscountRate", OleDbType.Decimal) { Value = broker.DiscountRate },
                new OleDbParameter("BrokerageRate", OleDbType.Decimal) { Value = broker.BrokerageRate }
            };
            
            int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
            
            return result > 0;
        }
        
        // Update an existing broker
        public static bool UpdateBroker(Broker broker)
        {
            try
            {
                // Get the active company ID
                int companyID = Program.ActiveCompany?.CompanyID ?? 0;
                
                string sql = @"UPDATE BrokerMaster SET 
                             BrokerName = ?, Phone = ?, InterestDays = ?, InterestRate = ?, DiscountDays = ?, DiscountRate = ?, BrokerageRate = ? 
                             WHERE BrokerID = ? AND CompanyID = ?";
                
                OleDbParameter[] parameters = {
                    new OleDbParameter("BrokerName", OleDbType.VarChar) { Value = broker.BrokerName },
                    new OleDbParameter("Phone", OleDbType.VarChar) { Value = broker.Phone },
                    new OleDbParameter("InterestDays", OleDbType.Integer) { Value = broker.InterestDays },
                    new OleDbParameter("InterestRate", OleDbType.Decimal) { Value = broker.InterestRate },
                    new OleDbParameter("DiscountDays", OleDbType.Integer) { Value = broker.DiscountDays },
                    new OleDbParameter("DiscountRate", OleDbType.Decimal) { Value = broker.DiscountRate },
                    new OleDbParameter("BrokerageRate", OleDbType.Decimal) { Value = broker.BrokerageRate },
                    new OleDbParameter("BrokerID", OleDbType.Integer) { Value = broker.BrokerID },
                    new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID }
                };
                
                int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
                
                return result > 0;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error updating broker: {ex.Message}", "Database Error", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }
        
        // Delete a broker
        public static bool DeleteBroker(int brokerID)
        {
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            // Check if broker exists in bills
            string checkBillsSql = "SELECT COUNT(*) FROM BillMaster WHERE BrokerID = ? AND CompanyID = ?";
            OleDbParameter[] checkBillsParams = {
                new OleDbParameter("BrokerID", OleDbType.Integer) { Value = brokerID },
                new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID }
            };
            
            int billCount = Convert.ToInt32(DatabaseManager.ExecuteScalar(checkBillsSql, checkBillsParams));
            
            // Check if broker exists in parties
            string checkPartiesSql = "SELECT COUNT(*) FROM PartyMaster WHERE BrokerID = ? AND CompanyID = ?";
            OleDbParameter[] checkPartiesParams = {
                new OleDbParameter("BrokerID", OleDbType.Integer) { Value = brokerID },
                new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID }
            };
            
            int partyCount = Convert.ToInt32(DatabaseManager.ExecuteScalar(checkPartiesSql, checkPartiesParams));
            
            if (billCount > 0 || partyCount > 0)
            {
                // Broker has bills or parties, cannot delete
                return false;
            }
            
            string sql = "DELETE FROM BrokerMaster WHERE BrokerID = ? AND CompanyID = ?";
            OleDbParameter[] parameters = {
                new OleDbParameter("BrokerID", OleDbType.Integer) { Value = brokerID },
                new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID }
            };
            
            int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
            
            return result > 0;
        }
        
        // Check if broker exists with the same name
        public static bool BrokerExists(string brokerName, int? excludeBrokerID = null)
        {
            // Get the active company ID
            int companyID = Program.ActiveCompany?.CompanyID ?? 0;
            
            string sql = "SELECT COUNT(*) FROM BrokerMaster WHERE BrokerName = ? AND CompanyID = ?";
            List<OleDbParameter> paramsList = new List<OleDbParameter> {
                new OleDbParameter("BrokerName", OleDbType.VarChar) { Value = brokerName },
                new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID }
            };
            
            if (excludeBrokerID.HasValue)
            {
                sql += " AND BrokerID <> ?";
                paramsList.Add(new OleDbParameter("BrokerID", OleDbType.Integer) { Value = excludeBrokerID.Value });
            }
            
            OleDbParameter[] parameters = paramsList.ToArray();
            object result = DatabaseManager.ExecuteScalar(sql, parameters);
            
            return Convert.ToInt32(result) > 0;
        }
        
        // Map DataRow to Broker object
        private static Broker MapRowToBroker(DataRow row)
        {
            return new Broker
            {
                BrokerID = Convert.ToInt32(row["BrokerID"]),
                BrokerName = row["BrokerName"].ToString(),
                Phone = row["Phone"].ToString(),
                CompanyID = row["CompanyID"] != DBNull.Value ? Convert.ToInt32(row["CompanyID"]) : 0,
                InterestDays = row["InterestDays"] != DBNull.Value ? Convert.ToInt32(row["InterestDays"]) : 0,
                InterestRate = row["InterestRate"] != DBNull.Value ? Convert.ToDecimal(row["InterestRate"]) : 0,
                DiscountDays = row["DiscountDays"] != DBNull.Value ? Convert.ToInt32(row["DiscountDays"]) : 0,
                DiscountRate = row["DiscountRate"] != DBNull.Value ? Convert.ToDecimal(row["DiscountRate"]) : 0,
                BrokerageRate = row["BrokerageRate"] != DBNull.Value ? Convert.ToDecimal(row["BrokerageRate"]) : 0
            };
        }
    }
} 