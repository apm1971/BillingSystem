using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Data
{
    public static class AdvancePaymentService
    {
        /// <summary>
        /// Adds a new advance payment to the database
        /// </summary>
        public static bool AddAdvancePayment(AdvancePayment advancePayment)
        {
            try
            {
                // Initialize advance payment tables if they don't exist
                DatabaseManager.InitializeAdvancePaymentSystem();

                string sql = @"
                    INSERT INTO AdvancePayments (PartyID, BrokerID, PaymentDate, Amount, PaymentMethod, Reference, ChequeAmountFirm1, ChequeAmountFirm2, CompanyID, CreatedDate)
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                var parameters = new OleDbParameter[]
                {
                    new OleDbParameter("PartyID", OleDbType.Integer) { Value = advancePayment.PartyID ?? (object)DBNull.Value },
                    new OleDbParameter("BrokerID", OleDbType.Integer) { Value = advancePayment.BrokerID ?? (object)DBNull.Value },
                    new OleDbParameter("PaymentDate", OleDbType.Date) { Value = advancePayment.PaymentDate },
                    new OleDbParameter("Amount", OleDbType.Currency) { Value = advancePayment.Amount },
                    new OleDbParameter("PaymentMethod", OleDbType.VarChar, 50) { Value = advancePayment.PaymentMethod ?? (object)DBNull.Value },
                    new OleDbParameter("Reference", OleDbType.VarChar, 255) { Value = advancePayment.Reference ?? (object)DBNull.Value },
                    new OleDbParameter("ChequeAmountFirm1", OleDbType.Currency) { Value = advancePayment.ChequeAmountFirm1 },
                    new OleDbParameter("ChequeAmountFirm2", OleDbType.Currency) { Value = advancePayment.ChequeAmountFirm2 },
                    new OleDbParameter("CompanyID", OleDbType.Integer) { Value = advancePayment.CompanyID },
                    new OleDbParameter("CreatedDate", OleDbType.Date) { Value = DateTime.Now }
                };

                int rowsAffected = DatabaseManager.ExecuteNonQuery(sql, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding advance payment: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets all advance payments with party and broker names - OPTIMIZED to avoid N+1 queries
        /// </summary>
        public static List<AdvancePayment> GetAllAdvancePayments(int companyId = 1)
        {
            var advancePayments = new List<AdvancePayment>();
            
            try
            {
                // OPTIMIZED: Single query with LEFT JOIN to get utilization amounts
                string sql = @"
                    SELECT ap.AdvanceID, ap.PartyID, ap.BrokerID, ap.PaymentDate, 
                           ap.Amount, ap.PaymentMethod, ap.Reference, ap.ChequeAmountFirm1, ap.ChequeAmountFirm2, 
                           ap.CompanyID, ap.CreatedDate, pm.PartyName, bm.BrokerName,
                           IIF(SUM(au.AmountUsed) IS NULL, 0, SUM(au.AmountUsed)) AS TotalUtilized
                    FROM (((AdvancePayments ap
                    LEFT JOIN PartyMaster pm ON ap.PartyID = pm.PartyID)
                    LEFT JOIN BrokerMaster bm ON ap.BrokerID = bm.BrokerID)
                    LEFT JOIN AdvanceUtilization au ON ap.AdvanceID = au.AdvanceID AND au.CompanyID = ap.CompanyID)
                    WHERE ap.CompanyID = ?
                    GROUP BY ap.AdvanceID, ap.PartyID, ap.BrokerID, ap.PaymentDate, 
                             ap.Amount, ap.PaymentMethod, ap.Reference, ap.ChequeAmountFirm1, ap.ChequeAmountFirm2, 
                             ap.CompanyID, ap.CreatedDate, pm.PartyName, bm.BrokerName
                    ORDER BY ap.PaymentDate DESC";

                var parameters = new OleDbParameter[]
                {
                    new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyId }
                };

                DataTable dt = DatabaseManager.ExecuteQuery(sql, parameters);
                
                foreach (DataRow row in dt.Rows)
                {
                    int advanceId = Convert.ToInt32(row["AdvanceID"]);
                    decimal originalAmount = Convert.ToDecimal(row["Amount"]);
                    decimal utilizedAmount = Convert.ToDecimal(row["TotalUtilized"]); // OPTIMIZED: From single query
                    
                    advancePayments.Add(new AdvancePayment
                    {
                        AdvanceID = advanceId,
                        PartyID = row["PartyID"] != DBNull.Value ? Convert.ToInt32(row["PartyID"]) : null,
                        BrokerID = row["BrokerID"] != DBNull.Value ? Convert.ToInt32(row["BrokerID"]) : null,
                        PaymentDate = Convert.ToDateTime(row["PaymentDate"]),
                        Amount = Math.Max(0, originalAmount - utilizedAmount), // Show available amount
                        OriginalAmount = originalAmount, // Store original for reference
                        UtilizedAmount = utilizedAmount, // Show utilized amount
                        PaymentMethod = row["PaymentMethod"]?.ToString() ?? string.Empty,
                        Reference = row["Reference"]?.ToString() ?? string.Empty,
                        ChequeAmountFirm1 = row["ChequeAmountFirm1"] != DBNull.Value ? Convert.ToDecimal(row["ChequeAmountFirm1"]) : 0m,
                        ChequeAmountFirm2 = row["ChequeAmountFirm2"] != DBNull.Value ? Convert.ToDecimal(row["ChequeAmountFirm2"]) : 0m,
                        CompanyID = Convert.ToInt32(row["CompanyID"]),
                        CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                        PartyName = row["PartyName"]?.ToString() ?? string.Empty,
                        BrokerName = row["BrokerName"]?.ToString() ?? string.Empty
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting advance payments: {ex.Message}");
                throw;
            }
            
            return advancePayments;
        }

        /// <summary>
        /// Gets advance payments filtered by party and/or broker - OPTIMIZED to avoid N+1 queries
        /// </summary>
        public static List<AdvancePayment> GetAdvancePayments(int? partyId = null, int? brokerId = null, int companyId = 1)
        {
            var advancePayments = new List<AdvancePayment>();
            
            try
            {
                // OPTIMIZED: Single query with LEFT JOIN to get utilization amounts
                string sql = @"
                    SELECT ap.AdvanceID, ap.PartyID, ap.BrokerID, ap.PaymentDate, 
                           ap.Amount, ap.PaymentMethod, ap.Reference, ap.ChequeAmountFirm1, ap.ChequeAmountFirm2,
                           ap.CompanyID, ap.CreatedDate, pm.PartyName, bm.BrokerName,
                           IIF(SUM(au.AmountUsed) IS NULL, 0, SUM(au.AmountUsed)) AS TotalUtilized
                    FROM (((AdvancePayments ap
                    LEFT JOIN PartyMaster pm ON ap.PartyID = pm.PartyID)
                    LEFT JOIN BrokerMaster bm ON ap.BrokerID = bm.BrokerID)
                    LEFT JOIN AdvanceUtilization au ON ap.AdvanceID = au.AdvanceID AND au.CompanyID = ap.CompanyID)
                    WHERE ap.CompanyID = ?";

                var parametersList = new List<OleDbParameter>
                {
                    new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyId }
                };

                if (partyId.HasValue)
                {
                    sql += " AND ap.PartyID = ?";
                    parametersList.Add(new OleDbParameter("PartyID", OleDbType.Integer) { Value = partyId.Value });
                }

                if (brokerId.HasValue)
                {
                    sql += " AND ap.BrokerID = ?";
                    parametersList.Add(new OleDbParameter("BrokerID", OleDbType.Integer) { Value = brokerId.Value });
                }

                sql += @"
                    GROUP BY ap.AdvanceID, ap.PartyID, ap.BrokerID, ap.PaymentDate, 
                             ap.Amount, ap.PaymentMethod, ap.Reference, ap.ChequeAmountFirm1, ap.ChequeAmountFirm2,
                             ap.CompanyID, ap.CreatedDate, pm.PartyName, bm.BrokerName
                    ORDER BY ap.PaymentDate DESC";

                DataTable dt = DatabaseManager.ExecuteQuery(sql, parametersList.ToArray());
                
                foreach (DataRow row in dt.Rows)
                {
                    int advanceId = Convert.ToInt32(row["AdvanceID"]);
                    decimal originalAmount = Convert.ToDecimal(row["Amount"]);
                    decimal utilizedAmount = Convert.ToDecimal(row["TotalUtilized"]); // OPTIMIZED: From single query
                    
                    advancePayments.Add(new AdvancePayment
                    {
                        AdvanceID = advanceId,
                        PartyID = row["PartyID"] != DBNull.Value ? Convert.ToInt32(row["PartyID"]) : null,
                        BrokerID = row["BrokerID"] != DBNull.Value ? Convert.ToInt32(row["BrokerID"]) : null,
                        PaymentDate = Convert.ToDateTime(row["PaymentDate"]),
                        Amount = Math.Max(0, originalAmount - utilizedAmount), // Show available amount
                        OriginalAmount = originalAmount, // Store original for reference
                        UtilizedAmount = utilizedAmount, // Show utilized amount
                        PaymentMethod = row["PaymentMethod"]?.ToString() ?? string.Empty,
                        Reference = row["Reference"]?.ToString() ?? string.Empty,
                        ChequeAmountFirm1 = row["ChequeAmountFirm1"] != DBNull.Value ? Convert.ToDecimal(row["ChequeAmountFirm1"]) : 0m,
                        ChequeAmountFirm2 = row["ChequeAmountFirm2"] != DBNull.Value ? Convert.ToDecimal(row["ChequeAmountFirm2"]) : 0m,
                        CompanyID = Convert.ToInt32(row["CompanyID"]),
                        CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                        PartyName = row["PartyName"]?.ToString() ?? string.Empty,
                        BrokerName = row["BrokerName"]?.ToString() ?? string.Empty
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting filtered advance payments: {ex.Message}");
                throw;
            }
            
            return advancePayments;
        }

        /// <summary>
        /// Gets the total available advance balance for a party (optionally filtered by broker)
        /// Uses utilization tracking to show only available amounts
        /// </summary>
        public static decimal GetPartyAdvanceBalance(int partyId, int? brokerId = null, int companyId = 1)
        {
            var availableAdvances = GetAvailableAdvancePayments(partyId, brokerId, companyId);
            return availableAdvances.Sum(a => a.Amount);
        }

        /// <summary>
        /// Gets the total available advance balance for a broker (optionally filtered by party)
        /// Uses utilization tracking to show only available amounts
        /// </summary>
        public static decimal GetBrokerAdvanceBalance(int brokerId, int? partyId = null, int companyId = 1)
        {
            var availableAdvances = GetAvailableAdvancePayments(partyId, brokerId, companyId);
            return availableAdvances.Sum(a => a.Amount);
        }

        /// <summary>
        /// Deletes an advance payment
        /// </summary>
        public static bool DeleteAdvancePayment(int advanceId)
        {
            try
            {
                string sql = "DELETE FROM AdvancePayments WHERE AdvanceID = ?";
                var parameters = new OleDbParameter[]
                {
                    new OleDbParameter("AdvanceID", OleDbType.Integer) { Value = advanceId }
                };

                int rowsAffected = DatabaseManager.ExecuteNonQuery(sql, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting advance payment: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets the available amount for a specific advance payment (Original Amount - Utilized Amount)
        /// </summary>
        public static decimal GetAvailableAdvanceAmount(int advanceId, int companyId = 1)
        {
            try
            {
                // Get original advance amount
                string originalSql = "SELECT Amount FROM AdvancePayments WHERE AdvanceID = ? AND CompanyID = ?";
                var originalParams = new OleDbParameter[]
                {
                    new OleDbParameter("AdvanceID", OleDbType.Integer) { Value = advanceId },
                    new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyId }
                };

                object originalResult = DatabaseManager.ExecuteScalar(originalSql, originalParams);
                if (originalResult == null || originalResult == DBNull.Value)
                {
                    return 0m; // Advance not found
                }

                decimal originalAmount = Convert.ToDecimal(originalResult);
                decimal utilizedAmount = AdvanceUtilizationService.GetTotalUtilizedAmount(advanceId, companyId);
                
                return Math.Max(0, originalAmount - utilizedAmount);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting available advance amount: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets all advance payments with their available amounts (excludes fully utilized advances) - OPTIMIZED
        /// </summary>
        public static List<AdvancePayment> GetAvailableAdvancePayments(int? partyId = null, int? brokerId = null, int companyId = 1)
        {
            // OPTIMIZED: Use the already optimized GetAdvancePayments which includes utilization data
            var allAdvances = GetAdvancePayments(partyId, brokerId, companyId);
            var availableAdvances = new List<AdvancePayment>();

            // OPTIMIZED: No additional database calls needed - utilization already calculated
            foreach (var advance in allAdvances)
            {
                if (advance.Amount > 0) // Available amount already calculated in GetAdvancePayments
                {
                    availableAdvances.Add(advance);
                }
            }

            return availableAdvances;
        }
    }
}