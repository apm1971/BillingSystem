using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Data
{
    public static class AdvanceUtilizationService
    {
        /// <summary>
        /// Adds a new advance utilization record to track advance payment usage
        /// </summary>
        public static int AddUtilization(AdvanceUtilization utilization, OleDbConnection conn = null, OleDbTransaction transaction = null)
        {
            try
            {
                bool shouldCloseConnection = conn == null;
                if (conn == null)
                {
                    // Initialize advance payment tables if they don't exist
                    DatabaseManager.InitializeAdvancePaymentSystem();
                    conn = DatabaseManager.GetConnection();
                    conn.Open();
                }

                string sql = @"
                    INSERT INTO AdvanceUtilization (AdvanceID, PaymentID, AmountUsed, UtilizedDate, PartyID, BrokerID, CompanyID, CreatedDate)
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?)";

                var parameters = new OleDbParameter[]
                {
                    new OleDbParameter("AdvanceID", OleDbType.Integer) { Value = utilization.AdvanceID },
                    new OleDbParameter("PaymentID", OleDbType.Integer) { Value = utilization.PaymentID > 0 ? utilization.PaymentID : (object)DBNull.Value },
                    new OleDbParameter("AmountUsed", OleDbType.Currency) { Value = utilization.AmountUsed },
                    new OleDbParameter("UtilizedDate", OleDbType.Date) { Value = utilization.UtilizedDate },
                    new OleDbParameter("PartyID", OleDbType.Integer) { Value = utilization.PartyID ?? (object)DBNull.Value },
                    new OleDbParameter("BrokerID", OleDbType.Integer) { Value = utilization.BrokerID ?? (object)DBNull.Value },
                    new OleDbParameter("CompanyID", OleDbType.Integer) { Value = utilization.CompanyID },
                    new OleDbParameter("CreatedDate", OleDbType.Date) { Value = DateTime.Now }
                };

                int rowsAffected;
                int utilizationId = 0; 
                using (var cmd = new OleDbCommand(sql, conn))
                {
                    if (transaction != null)
                    {
                        cmd.Transaction = transaction;
                    }
                    foreach (var p in parameters) cmd.Parameters.Add(p);
                    cmd.ExecuteNonQuery();
                    string getUtilizationIdSql = "SELECT @@IDENTITY";
                    using (var getIdCmd = new OleDbCommand(getUtilizationIdSql, conn))
                    {
                        if (transaction != null)
                        {
                            getIdCmd.Transaction = transaction;
                        }
                        utilizationId = Convert.ToInt32(getIdCmd.ExecuteScalar());
                    }
                }

                if (shouldCloseConnection)
                {
                    conn.Close();
                }

                return utilizationId;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding advance utilization: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets all utilization records for a specific advance payment
        /// </summary>
        public static List<AdvanceUtilization> GetUtilizationsByAdvance(int advanceId, int companyId = 1)
        {
            var utilizations = new List<AdvanceUtilization>();
            
            try
            {
                string sql = @"
                    SELECT au.UtilizationID, au.AdvanceID, au.PaymentID, au.AmountUsed, au.UtilizedDate, 
                           au.PartyID, au.BrokerID, au.CompanyID, au.CreatedDate,
                           pm.PartyName, bm.BrokerName, 
                           CASE 
                               WHEN au.PaymentID IS NOT NULL AND au.PaymentID > 0 THEN 
                                   (SELECT Reference FROM AdvancePayments WHERE AdvanceID = au.PaymentID)
                               ELSE 'N/A'
                           END as PaymentReference,
                           ap.Reference as OriginalAdvanceReference, ap.Amount as OriginalAdvanceAmount, ap.PaymentDate as OriginalAdvanceDate
                    FROM (((AdvanceUtilization au
                    LEFT JOIN PartyMaster pm ON au.PartyID = pm.PartyID)
                    LEFT JOIN BrokerMaster bm ON au.BrokerID = bm.BrokerID)
                    LEFT JOIN AdvancePayments ap ON au.AdvanceID = ap.AdvanceID)
                    WHERE au.AdvanceID = ? AND au.CompanyID = ?
                    ORDER BY au.UtilizedDate DESC";

                var parameters = new OleDbParameter[]
                {
                    new OleDbParameter("AdvanceID", OleDbType.Integer) { Value = advanceId },
                    new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyId }
                };

                DataTable dt = DatabaseManager.ExecuteQuery(sql, parameters);
                
                foreach (DataRow row in dt.Rows)
                {
                    utilizations.Add(MapRowToUtilization(row));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting utilizations by advance: {ex.Message}");
                throw;
            }
            
            return utilizations;
        }

        /// <summary>
        /// Gets all utilization records for a specific payment
        /// </summary>
        public static List<AdvanceUtilization> GetUtilizationsByPayment(int paymentId, int companyId = 1)
        {
            var utilizations = new List<AdvanceUtilization>();
            
            try
            {
                string sql = @"
                    SELECT au.UtilizationID, au.AdvanceID, au.PaymentID, au.AmountUsed, au.UtilizedDate, 
                           au.PartyID, au.BrokerID, au.CompanyID, au.CreatedDate,
                           pm.PartyName, bm.BrokerName, 
                           CASE 
                               WHEN au.PaymentID IS NOT NULL AND au.PaymentID > 0 THEN 
                                   (SELECT Reference FROM AdvancePayments WHERE AdvanceID = au.PaymentID)
                               ELSE 'N/A'
                           END as PaymentReference,
                           ap.Reference as OriginalAdvanceReference, ap.Amount as OriginalAdvanceAmount, ap.PaymentDate as OriginalAdvanceDate
                    FROM (((AdvanceUtilization au
                    LEFT JOIN PartyMaster pm ON au.PartyID = pm.PartyID)
                    LEFT JOIN BrokerMaster bm ON au.BrokerID = bm.BrokerID)
                    LEFT JOIN AdvancePayments ap ON au.AdvanceID = ap.AdvanceID)
                    WHERE au.PaymentID = ? AND au.CompanyID = ?
                    ORDER BY au.UtilizedDate DESC";

                var parameters = new OleDbParameter[]
                {
                    new OleDbParameter("PaymentID", OleDbType.Integer) { Value = paymentId },
                    new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyId }
                };

                DataTable dt = DatabaseManager.ExecuteQuery(sql, parameters);
                
                foreach (DataRow row in dt.Rows)
                {
                    utilizations.Add(MapRowToUtilization(row));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting utilizations by payment: {ex.Message}");
                throw;
            }
            
            return utilizations;
        }

        /// <summary>
        /// Gets the total utilized amount for a specific advance payment
        /// </summary>
        public static decimal GetTotalUtilizedAmount(int advanceId, int companyId = 1)
        {
            try
            {
                string sql = @"
                    SELECT IIF(SUM(AmountUsed) IS NULL, 0, SUM(AmountUsed)) 
                    FROM AdvanceUtilization 
                    WHERE AdvanceID = ? AND CompanyID = ?";

                var parameters = new OleDbParameter[]
                {
                    new OleDbParameter("AdvanceID", OleDbType.Integer) { Value = advanceId },
                    new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyId }
                };

                object result = DatabaseManager.ExecuteScalar(sql, parameters);
                return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0m;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting total utilized amount: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets all utilization records for a party/broker combination
        /// </summary>
        public static List<AdvanceUtilization> GetUtilizationsByPartyBroker(int? partyId = null, int? brokerId = null, int companyId = 1)
        {
            var utilizations = new List<AdvanceUtilization>();
            
            try
            {
                string sql = @"
                    SELECT au.UtilizationID, au.AdvanceID, au.PaymentID, au.AmountUsed, au.UtilizedDate, 
                           au.PartyID, au.BrokerID, au.CompanyID, au.CreatedDate,
                           pm.PartyName, bm.BrokerName, 
                           CASE 
                               WHEN au.PaymentID IS NOT NULL AND au.PaymentID > 0 THEN 
                                   (SELECT Reference FROM AdvancePayments WHERE AdvanceID = au.PaymentID)
                               ELSE 'N/A'
                           END as PaymentReference,
                           ap.Reference as OriginalAdvanceReference, ap.Amount as OriginalAdvanceAmount, ap.PaymentDate as OriginalAdvanceDate
                    FROM (((AdvanceUtilization au
                    LEFT JOIN PartyMaster pm ON au.PartyID = pm.PartyID)
                    LEFT JOIN BrokerMaster bm ON au.BrokerID = bm.BrokerID)
                    LEFT JOIN AdvancePayments ap ON au.AdvanceID = ap.AdvanceID)
                    WHERE au.CompanyID = ?";

                var parametersList = new List<OleDbParameter>
                {
                    new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyId }
                };

                if (partyId.HasValue)
                {
                    sql += " AND au.PartyID = ?";
                    parametersList.Add(new OleDbParameter("PartyID", OleDbType.Integer) { Value = partyId.Value });
                }

                if (brokerId.HasValue)
                {
                    sql += " AND au.BrokerID = ?";
                    parametersList.Add(new OleDbParameter("BrokerID", OleDbType.Integer) { Value = brokerId.Value });
                }

                sql += " ORDER BY au.UtilizedDate DESC";

                DataTable dt = DatabaseManager.ExecuteQuery(sql, parametersList.ToArray());
                
                foreach (DataRow row in dt.Rows)
                {
                    utilizations.Add(MapRowToUtilization(row));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting utilizations by party/broker: {ex.Message}");
                throw;
            }
            
            return utilizations;
        }

        /// <summary>
        /// Helper method to map DataRow to AdvanceUtilization object
        /// </summary>
        private static AdvanceUtilization MapRowToUtilization(DataRow row)
        {
            return new AdvanceUtilization
            {
                UtilizationID = Convert.ToInt32(row["UtilizationID"]),
                AdvanceID = Convert.ToInt32(row["AdvanceID"]),
                PaymentID = Convert.ToInt32(row["PaymentID"]),
                AmountUsed = Convert.ToDecimal(row["AmountUsed"]),
                UtilizedDate = Convert.ToDateTime(row["UtilizedDate"]),
                PartyID = row["PartyID"] != DBNull.Value ? Convert.ToInt32(row["PartyID"]) : null,
                BrokerID = row["BrokerID"] != DBNull.Value ? Convert.ToInt32(row["BrokerID"]) : null,
                CompanyID = Convert.ToInt32(row["CompanyID"]),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                PartyName = row["PartyName"]?.ToString() ?? string.Empty,
                BrokerName = row["BrokerName"]?.ToString() ?? string.Empty,
                PaymentReference = row["PaymentReference"]?.ToString() ?? string.Empty,
                OriginalAdvanceReference = row["OriginalAdvanceReference"]?.ToString() ?? string.Empty,
                OriginalAdvanceAmount = row["OriginalAdvanceAmount"] != DBNull.Value ? Convert.ToDecimal(row["OriginalAdvanceAmount"]) : null,
                OriginalAdvanceDate = row["OriginalAdvanceDate"] != DBNull.Value ? Convert.ToDateTime(row["OriginalAdvanceDate"]) : null
            };
        }

        /// <summary>
        /// Deletes utilization records for a specific payment (used for rollback scenarios)
        /// </summary>
        public static bool DeleteUtilizationsByPayment(int paymentId, int companyId = 1)
        {
            try
            {
                string sql = "DELETE FROM AdvanceUtilization WHERE PaymentID = ? AND CompanyID = ?";
                var parameters = new OleDbParameter[]
                {
                    new OleDbParameter("PaymentID", OleDbType.Integer) { Value = paymentId },
                    new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyId }
                };

                int rowsAffected = DatabaseManager.ExecuteNonQuery(sql, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting utilizations by payment: {ex.Message}");
                throw;
            }
        }
    }
}