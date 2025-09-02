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
            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();
                    
                    // Simple test - if this works, table exists
                    var checkTableSql = "SELECT COUNT(*) FROM PaymentReports";
                    using (var checkCmd = new OleDbCommand(checkTableSql, connection))
                    {
                        checkCmd.ExecuteScalar();
                        System.Diagnostics.Debug.WriteLine("PaymentReports table exists and is accessible");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Table check failed: {ex.Message}");
                
                // Only try to create if table doesn't exist
                try
                {
                    string createTableSql = @"
                        CREATE TABLE PaymentReports (
                            ReportID COUNTER PRIMARY KEY,
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
                        using (var createCmd = new OleDbCommand(createTableSql, connection))
                        {
                            createCmd.ExecuteNonQuery();
                            System.Diagnostics.Debug.WriteLine("PaymentReports table created successfully");
                        }
                    }
                }
                catch (Exception createEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to create table: {createEx.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Saves payment report data to the database
        /// </summary>
        public static bool SavePaymentReport(int paymentId, PaymentReportData reportData)
        {
            try
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
                        // Use proper parameter types for OleDb
                        command.Parameters.Add("@PaymentID", OleDbType.Integer).Value = paymentId;
                        command.Parameters.Add("@PaymentDate", OleDbType.Date).Value = reportData.PaymentDate;
                        command.Parameters.Add("@PartyID", OleDbType.Integer).Value = reportData.PartyID ?? (object)DBNull.Value;
                        command.Parameters.Add("@BrokerID", OleDbType.Integer).Value = reportData.BrokerID ?? (object)DBNull.Value;
                        command.Parameters.Add("@PartyName", OleDbType.VarChar, 255).Value = (reportData.PartyName ?? "").Trim();
                        command.Parameters.Add("@BrokerName", OleDbType.VarChar, 255).Value = (reportData.BrokerName ?? "").Trim();
                        command.Parameters.Add("@TotalAmount", OleDbType.Currency).Value = Math.Round(reportData.TotalPaymentAmount, 2);
                        command.Parameters.Add("@PaymentMethod", OleDbType.VarChar, 50).Value = (reportData.PaymentMethod ?? "").Trim();
                        command.Parameters.Add("@ReportData", OleDbType.LongVarChar).Value = jsonData;
                        command.Parameters.Add("@CreatedDate", OleDbType.Date).Value = DateTime.Now;
                        
                        int rowsAffected = command.ExecuteNonQuery();
                        
                        if (rowsAffected > 0)
                        {
                            System.Windows.Forms.MessageBox.Show(
                                "Payment report saved successfully!",
                                "Save Complete",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Information);
                            return true;
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show(
                                "No data was saved. Please try again.",
                                "Save Failed",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Warning);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Error saving payment report: {ex.Message}",
                    "Database Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
                
                System.Diagnostics.Debug.WriteLine($"SavePaymentReport Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
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
        public static List<PaymentReportSummary> GetPaymentReports(DateTime? fromDate = null, DateTime? toDate = null, int? partyId = null, int? brokerId = null)
        {
            EnsurePaymentReportsTableExists();

            var reports = new List<PaymentReportSummary>();
            var conditions = new List<string>();
            var parameters = new List<object>();

            string sql = @"
                SELECT PaymentID, PaymentDate, PartyName, BrokerName, TotalAmount, PaymentMethod
                FROM PaymentReports";

            // Debug: Log filter parameters
            System.Diagnostics.Debug.WriteLine($"GetPaymentReports called with: partyId={partyId}, brokerId={brokerId}, fromDate={fromDate}, toDate={toDate}");

            // Add date filter - include the entire toDate (end of day)
            if (fromDate.HasValue)
            {
                conditions.Add("PaymentDate >= ?");
                parameters.Add(fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                conditions.Add("PaymentDate <= ?");
                parameters.Add(toDate.Value.Date.AddDays(1).AddTicks(-1)); // End of day
            }

            // Add party filter - filter by PartyID
            if (partyId.HasValue && partyId.Value > 0)
            {
                conditions.Add("PartyID = ?");
                parameters.Add(partyId.Value);
            }

            // Add broker filter - filter by BrokerID
            if (brokerId.HasValue && brokerId.Value > 0)
            {
                conditions.Add("BrokerID = ?");
                parameters.Add(brokerId.Value);
            }

            if (conditions.Count > 0)
            {
                sql += " WHERE " + string.Join(" AND ", conditions);
            }

            sql += " ORDER BY PaymentDate DESC, PaymentID DESC";

            // Debug: Log the final SQL and parameters
            System.Diagnostics.Debug.WriteLine($"Final SQL: {sql}");
            System.Diagnostics.Debug.WriteLine($"Parameters: {string.Join(", ", parameters)}");
            System.Diagnostics.Debug.WriteLine($"Number of conditions: {conditions.Count}");

            using (var connection = DatabaseManager.GetConnection())
            {
                connection.Open();
                using (var command = new OleDbCommand(sql, connection))
                {
                    // Fix parameter binding - use proper OleDb parameter types for queries
                    for (int i = 0; i < parameters.Count; i++)
                    {
                        var param = parameters[i];
                        if (param is DateTime)
                        {
                            command.Parameters.Add($"@param{i}", OleDbType.Date).Value = param;
                        }
                        else if (param is int)
                        {
                            command.Parameters.Add($"@param{i}", OleDbType.Integer).Value = param;
                        }
                        else if (param is string)
                        {
                            command.Parameters.Add($"@param{i}", OleDbType.VarChar, 255).Value = param;
                        }
                        else
                        {
                            command.Parameters.AddWithValue($"@param{i}", param);
                        }
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var report = new PaymentReportSummary
                            {
                                PaymentID = reader.GetInt32(reader.GetOrdinal("PaymentID")),
                                PaymentDate = reader.GetDateTime(reader.GetOrdinal("PaymentDate")),
                                PartyName = reader.IsDBNull(reader.GetOrdinal("PartyName")) ? "" : reader.GetString(reader.GetOrdinal("PartyName")),
                                BrokerName = reader.IsDBNull(reader.GetOrdinal("BrokerName")) ? "" : reader.GetString(reader.GetOrdinal("BrokerName")),
                                TotalAmount = reader.IsDBNull(reader.GetOrdinal("TotalAmount")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                                PaymentMethod = reader.IsDBNull(reader.GetOrdinal("PaymentMethod")) ? "" : reader.GetString(reader.GetOrdinal("PaymentMethod"))
                            };
                            
                            // Debug: Log each report found
                            System.Diagnostics.Debug.WriteLine($"Found report: PaymentID={report.PaymentID}, PartyName='{report.PartyName}', BrokerName='{report.BrokerName}'");
                            reports.Add(report);
                        }
                    }
                }
            }
            
            // Debug: Log final result count
            System.Diagnostics.Debug.WriteLine($"Total reports found: {reports.Count}");

            return reports;
        }

        /// <summary>
        /// Gets distinct party names for filter dropdown
        /// </summary>
        public static List<string> GetDistinctParties()
        {
            try
            {
                // EnsurePaymentReportsTableExists(); // Skip this since table exists

                var parties = new List<string>();
                
                // First try to get count to see if table has data
                string countSql = "SELECT COUNT(*) FROM PaymentReports";
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();
                    using (var countCmd = new OleDbCommand(countSql, connection))
                    {
                        int totalRecords = Convert.ToInt32(countCmd.ExecuteScalar());
                        System.Diagnostics.Debug.WriteLine($"Total PaymentReports records: {totalRecords}");
                        
                        if (totalRecords == 0)
                        {
                            System.Diagnostics.Debug.WriteLine("No payment reports found, returning empty party list");
                            return parties;
                        }
                    }
                    
                    // Now get distinct parties
                    string sql = "SELECT DISTINCT PartyName FROM PaymentReports WHERE PartyName IS NOT NULL AND PartyName <> ''";
                    using (var command = new OleDbCommand(sql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                string partyName = reader.GetValue(0)?.ToString()?.Trim();
                                if (!string.IsNullOrEmpty(partyName))
                                {
                                    parties.Add(partyName);
                                    System.Diagnostics.Debug.WriteLine($"Found party: '{partyName}'");
                                }
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Total distinct parties found: {parties.Count}");
                return parties;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetDistinctParties: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<string>(); // Return empty list on error
            }
        }

        /// <summary>
        /// Gets distinct broker names for filter dropdown
        /// </summary>
        public static List<string> GetDistinctBrokers()
        {
            try
            {
                // EnsurePaymentReportsTableExists(); // Skip this since table exists

                var brokers = new List<string>();
                
                // First try to get count to see if table has data
                string countSql = "SELECT COUNT(*) FROM PaymentReports";
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();
                    using (var countCmd = new OleDbCommand(countSql, connection))
                    {
                        int totalRecords = Convert.ToInt32(countCmd.ExecuteScalar());
                        System.Diagnostics.Debug.WriteLine($"Total PaymentReports records for brokers: {totalRecords}");
                        
                        if (totalRecords == 0)
                        {
                            System.Diagnostics.Debug.WriteLine("No payment reports found, returning empty broker list");
                            return brokers;
                        }
                    }
                    
                    // Now get distinct brokers
                    string sql = "SELECT DISTINCT BrokerName FROM PaymentReports WHERE BrokerName IS NOT NULL AND BrokerName <> ''";
                    using (var command = new OleDbCommand(sql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                string brokerName = reader.GetValue(0)?.ToString()?.Trim();
                                if (!string.IsNullOrEmpty(brokerName))
                                {
                                    brokers.Add(brokerName);
                                    System.Diagnostics.Debug.WriteLine($"Found broker: '{brokerName}'");
                                }
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Total distinct brokers found: {brokers.Count}");
                return brokers;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetDistinctBrokers: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<string>(); // Return empty list on error
            }
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