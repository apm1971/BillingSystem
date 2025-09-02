using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Text.Json;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Services
{
    /// <summary>
    /// Service for managing payment report data storage and retrieval
    /// </summary>
    public static class PaymentReportService
    {
        /// <summary>
        /// Creates the PaymentReports table if it doesn't exist
        /// </summary>
        public static void EnsurePaymentReportsTableExists()
        {
            string createTableSql = @"
                CREATE TABLE PaymentReports (
                    ReportID AUTOINCREMENT PRIMARY KEY,
                    PaymentID INTEGER NOT NULL,
                    PaymentDate DATETIME NOT NULL,
                    PartyID INTEGER,
                    BrokerID INTEGER,
                    PartyName TEXT(255),
                    BrokerName TEXT(255),
                    TotalAmount CURRENCY,
                    PaymentMethod TEXT(50),
                    ReportData MEMO NOT NULL,
                    CreatedDate DATETIME NOT NULL
                )";

            using (var connection = DatabaseManager.GetConnection())
            {
                connection.Open();
                
                // Check if table exists
                try
                {
                    var checkTableSql = "SELECT COUNT(*) FROM PaymentReports";
                    using (var checkCmd = new OleDbCommand(checkTableSql, connection))
                    {
                        checkCmd.ExecuteScalar();
                        // Table exists if no exception thrown
                    }
                }
                catch
                {
                    // Table doesn't exist, create it
                    using (var createCmd = new OleDbCommand(createTableSql, connection))
                    {
                        createCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Saves payment report data to the database
        /// </summary>
        public static void SavePaymentReport(int paymentId, PaymentReportData reportData)
        {
            EnsurePaymentReportsTableExists();
            
            reportData.PaymentID = paymentId;
            
            string jsonData = JsonSerializer.Serialize(reportData, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            string insertSql = @"
                INSERT INTO PaymentReports 
                (PaymentID, PaymentDate, PartyID, BrokerID, PartyName, BrokerName, TotalAmount, PaymentMethod, ReportData, CreatedDate)
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

            using (var connection = DatabaseManager.GetConnection())
            {
                connection.Open();
                using (var command = new OleDbCommand(insertSql, connection))
                {
                    command.Parameters.AddWithValue("@PaymentID", paymentId);
                    command.Parameters.AddWithValue("@PaymentDate", reportData.PaymentDate);
                    command.Parameters.AddWithValue("@PartyID", reportData.PartyID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@BrokerID", reportData.BrokerID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@PartyName", reportData.PartyName ?? "");
                    command.Parameters.AddWithValue("@BrokerName", reportData.BrokerName ?? "");
                    command.Parameters.AddWithValue("@TotalAmount", reportData.TotalPaymentAmount);
                    command.Parameters.AddWithValue("@PaymentMethod", reportData.PaymentMethod ?? "");
                    command.Parameters.AddWithValue("@ReportData", jsonData);
                    command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Retrieves payment report data from the database
        /// </summary>
        public static PaymentReportData GetPaymentReport(int paymentId)
        {
            EnsurePaymentReportsTableExists();

            string selectSql = "SELECT ReportData FROM PaymentReports WHERE PaymentID = ?";

            using (var connection = DatabaseManager.GetConnection())
            {
                connection.Open();
                using (var command = new OleDbCommand(selectSql, connection))
                {
                    command.Parameters.AddWithValue("@PaymentID", paymentId);
                    
                    var result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        string jsonData = result.ToString();
                        return JsonSerializer.Deserialize<PaymentReportData>(jsonData);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets filtered list of payment reports for the search form
        /// </summary>
        public static List<PaymentReportSummary> GetPaymentReports(DateTime? fromDate = null, DateTime? toDate = null, string partyName = null, string brokerName = null)
        {
            EnsurePaymentReportsTableExists();

            var reports = new List<PaymentReportSummary>();
            var conditions = new List<string>();
            var parameters = new List<object>();

            string sql = @"
                SELECT PaymentID, PaymentDate, PartyName, BrokerName, TotalAmount, PaymentMethod
                FROM PaymentReports";

            // Add date filter
            if (fromDate.HasValue)
            {
                conditions.Add("PaymentDate >= ?");
                parameters.Add(fromDate.Value);
            }

            if (toDate.HasValue)
            {
                conditions.Add("PaymentDate <= ?");
                parameters.Add(toDate.Value);
            }

            // Add party filter
            if (!string.IsNullOrEmpty(partyName) && partyName != "All")
            {
                conditions.Add("PartyName = ?");
                parameters.Add(partyName);
            }

            // Add broker filter
            if (!string.IsNullOrEmpty(brokerName) && brokerName != "All")
            {
                conditions.Add("BrokerName = ?");
                parameters.Add(brokerName);
            }

            if (conditions.Count > 0)
            {
                sql += " WHERE " + string.Join(" AND ", conditions);
            }

            sql += " ORDER BY PaymentDate DESC, PaymentID DESC";

            using (var connection = DatabaseManager.GetConnection())
            {
                connection.Open();
                using (var command = new OleDbCommand(sql, connection))
                {
                    for (int i = 0; i < parameters.Count; i++)
                    {
                        command.Parameters.AddWithValue($"@param{i}", parameters[i]);
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reports.Add(new PaymentReportSummary
                            {
                                PaymentID = reader.GetInt32(reader.GetOrdinal("PaymentID")),
                                PaymentDate = reader.GetDateTime(reader.GetOrdinal("PaymentDate")),
                                PartyName = reader.IsDBNull(reader.GetOrdinal("PartyName")) ? "" : reader.GetString(reader.GetOrdinal("PartyName")),
                                BrokerName = reader.IsDBNull(reader.GetOrdinal("BrokerName")) ? "" : reader.GetString(reader.GetOrdinal("BrokerName")),
                                TotalAmount = reader.IsDBNull(reader.GetOrdinal("TotalAmount")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                                PaymentMethod = reader.IsDBNull(reader.GetOrdinal("PaymentMethod")) ? "" : reader.GetString(reader.GetOrdinal("PaymentMethod"))
                            });
                        }
                    }
                }
            }

            return reports;
        }

        /// <summary>
        /// Gets distinct party names for filter dropdown
        /// </summary>
        public static List<string> GetDistinctParties()
        {
            EnsurePaymentReportsTableExists();

            var parties = new List<string>();
            string sql = "SELECT DISTINCT PartyName FROM PaymentReports WHERE PartyName IS NOT NULL AND PartyName <> '' ORDER BY PartyName";

            using (var connection = DatabaseManager.GetConnection())
            {
                connection.Open();
                using (var command = new OleDbCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        parties.Add(reader.GetString(0));
                    }
                }
            }

            return parties;
        }

        /// <summary>
        /// Gets distinct broker names for filter dropdown
        /// </summary>
        public static List<string> GetDistinctBrokers()
        {
            EnsurePaymentReportsTableExists();

            var brokers = new List<string>();
            string sql = "SELECT DISTINCT BrokerName FROM PaymentReports WHERE BrokerName IS NOT NULL AND BrokerName <> '' ORDER BY BrokerName";

            using (var connection = DatabaseManager.GetConnection())
            {
                connection.Open();
                using (var command = new OleDbCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        brokers.Add(reader.GetString(0));
                    }
                }
            }

            return brokers;
        }
    }

    /// <summary>
    /// Summary data for payment reports list
    /// </summary>
    public class PaymentReportSummary
    {
        public int PaymentID { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PartyName { get; set; } = "";
        public string BrokerName { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = "";
    }
}