using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Data
{
    public class BrokerService
    {
        // Get all brokers
        public static List<Broker> GetAllBrokers()
        {
            List<Broker> brokers = new List<Broker>();
            
            string sql = "SELECT * FROM BrokerMaster ORDER BY BrokerName";
            DataTable dt = DatabaseManager.ExecuteQuery(sql);
            
            foreach (DataRow row in dt.Rows)
            {
                Broker broker = new Broker
                {
                    BrokerID = Convert.ToInt32(row["BrokerID"]),
                    BrokerName = row["BrokerName"].ToString(),
                    Phone = row["Phone"].ToString(),
                    Email = row["Email"].ToString()
                };
                
                brokers.Add(broker);
            }
            
            return brokers;
        }
        
        // Get broker by ID
        public static Broker? GetBrokerByID(int brokerID)
        {
            string sql = "SELECT * FROM BrokerMaster WHERE BrokerID = @BrokerID";
            SQLiteParameter param = new SQLiteParameter("@BrokerID", brokerID);
            
            DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
            
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                
                Broker broker = new Broker
                {
                    BrokerID = Convert.ToInt32(row["BrokerID"]),
                    BrokerName = row["BrokerName"].ToString(),
                    Phone = row["Phone"].ToString(),
                    Email = row["Email"].ToString()
                };
                
                return broker;
            }
            
            return null;
        }
        
        // Search brokers
        public static List<Broker> SearchBrokers(string searchText)
        {
            List<Broker> brokers = new List<Broker>();
            
            string sql = "SELECT * FROM BrokerMaster WHERE BrokerName LIKE ? OR Phone LIKE ? ORDER BY BrokerName";
            
            string param = "%" + searchText + "%";
            DataTable dt = DatabaseManager.ExecuteQuery(sql, 
                new SQLiteParameter("@BrokerName", param),
                new SQLiteParameter("@Phone", param));
                
            foreach (DataRow row in dt.Rows)
            {
                brokers.Add(MapRowToBroker(row));
            }
            
            return brokers;
        }
        
        // Add a new broker
        public static bool AddBroker(Broker broker)
        {
            string sql = @"INSERT INTO BrokerMaster (
                BrokerName, Phone, Email
            ) VALUES (
                @BrokerName, @Phone, @Email
            )";
            
            SQLiteParameter[] parameters = {
                new SQLiteParameter("@BrokerName", broker.BrokerName),
                new SQLiteParameter("@Phone", broker.Phone),
                new SQLiteParameter("@Email", broker.Email)
            };
            
            int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
            
            return result > 0;
        }
        
        // Update an existing broker
        public static bool UpdateBroker(Broker broker)
        {
            string sql = @"UPDATE BrokerMaster SET 
                BrokerName = @BrokerName,
                Phone = @Phone,
                Email = @Email
            WHERE BrokerID = @BrokerID";
            
            SQLiteParameter[] parameters = {
                new SQLiteParameter("@BrokerName", broker.BrokerName),
                new SQLiteParameter("@Phone", broker.Phone),
                new SQLiteParameter("@Email", broker.Email),
                new SQLiteParameter("@BrokerID", broker.BrokerID)
            };
            
            int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
            
            return result > 0;
        }
        
        // Delete a broker
        public static bool DeleteBroker(int brokerID)
        {
            string sql = "DELETE FROM BrokerMaster WHERE BrokerID = @BrokerID";
            SQLiteParameter param = new SQLiteParameter("@BrokerID", brokerID);
            
            int result = DatabaseManager.ExecuteNonQuery(sql, param);
            
            return result > 0;
        }
        
        // Check if broker exists with the same name
        public static bool BrokerExists(string brokerName, int? excludeBrokerID = null)
        {
            string sql = "SELECT COUNT(*) FROM BrokerMaster WHERE BrokerName = @BrokerName";
            SQLiteParameter[] parameters = { new SQLiteParameter("@BrokerName", brokerName) };
            
            if (excludeBrokerID.HasValue)
            {
                sql += " AND BrokerID <> @BrokerID";
                parameters = new SQLiteParameter[] {
                    new SQLiteParameter("@BrokerName", brokerName),
                    new SQLiteParameter("@BrokerID", excludeBrokerID.Value)
                };
            }
            
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
                Email = row["Email"].ToString()
            };
        }
    }
} 