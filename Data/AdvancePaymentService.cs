using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
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
        /// Gets all advance payments with party and broker names
        /// </summary>
        public static List<AdvancePayment> GetAllAdvancePayments(int companyId = 1)
        {
            var advancePayments = new List<AdvancePayment>();
            
            try
            {
                string sql = @"
                    SELECT ap.AdvanceID, ap.PartyID, ap.BrokerID, ap.PaymentDate, 
                           ap.Amount, ap.PaymentMethod, ap.Reference, ap.ChequeAmountFirm1, ap.ChequeAmountFirm2, 
                           ap.CompanyID, ap.CreatedDate, pm.PartyName, bm.BrokerName
                    FROM ((AdvancePayments ap
                    LEFT JOIN PartyMaster pm ON ap.PartyID = pm.PartyID)
                    LEFT JOIN BrokerMaster bm ON ap.BrokerID = bm.BrokerID)
                    WHERE ap.CompanyID = ?
                    ORDER BY ap.PaymentDate DESC";

                var parameters = new OleDbParameter[]
                {
                    new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyId }
                };

                DataTable dt = DatabaseManager.ExecuteQuery(sql, parameters);
                
                foreach (DataRow row in dt.Rows)
                {
                    advancePayments.Add(new AdvancePayment
                    {
                        AdvanceID = Convert.ToInt32(row["AdvanceID"]),
                        PartyID = row["PartyID"] != DBNull.Value ? Convert.ToInt32(row["PartyID"]) : null,
                        BrokerID = row["BrokerID"] != DBNull.Value ? Convert.ToInt32(row["BrokerID"]) : null,
                        PaymentDate = Convert.ToDateTime(row["PaymentDate"]),
                        Amount = Convert.ToDecimal(row["Amount"]),
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
        /// Gets advance payments filtered by party and/or broker
        /// </summary>
        public static List<AdvancePayment> GetAdvancePayments(int? partyId = null, int? brokerId = null, int companyId = 1)
        {
            var advancePayments = new List<AdvancePayment>();
            
            try
            {
                string sql = @"
                    SELECT ap.AdvanceID, ap.PartyID, ap.BrokerID, ap.PaymentDate, 
                           ap.Amount, ap.PaymentMethod, ap.Reference, ap.ChequeAmountFirm1, ap.ChequeAmountFirm2,
                           ap.CompanyID, ap.CreatedDate, pm.PartyName, bm.BrokerName
                    FROM ((AdvancePayments ap
                    LEFT JOIN PartyMaster pm ON ap.PartyID = pm.PartyID)
                    LEFT JOIN BrokerMaster bm ON ap.BrokerID = bm.BrokerID)
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

                sql += " ORDER BY ap.PaymentDate DESC";

                DataTable dt = DatabaseManager.ExecuteQuery(sql, parametersList.ToArray());
                
                foreach (DataRow row in dt.Rows)
                {
                    advancePayments.Add(new AdvancePayment
                    {
                        AdvanceID = Convert.ToInt32(row["AdvanceID"]),
                        PartyID = row["PartyID"] != DBNull.Value ? Convert.ToInt32(row["PartyID"]) : null,
                        BrokerID = row["BrokerID"] != DBNull.Value ? Convert.ToInt32(row["BrokerID"]) : null,
                        PaymentDate = Convert.ToDateTime(row["PaymentDate"]),
                        Amount = Convert.ToDecimal(row["Amount"]),
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
        /// Gets the total advance balance for a party (optionally filtered by broker)
        /// </summary>
        public static decimal GetPartyAdvanceBalance(int partyId, int? brokerId = null, int companyId = 1)
        {
            return DatabaseManager.GetPartyAdvanceBalance(partyId, brokerId, companyId);
        }

        /// <summary>
        /// Gets the total advance balance for a broker (optionally filtered by party)
        /// </summary>
        public static decimal GetBrokerAdvanceBalance(int brokerId, int? partyId = null, int companyId = 1)
        {
            return DatabaseManager.GetBrokerAdvanceBalance(brokerId, partyId, companyId);
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
    }
}