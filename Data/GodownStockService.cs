using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Data
{
    public class GodownStockService
    {
        /// <summary>
        /// Get current stock - optimized to use only 2-3 queries instead of N×M×3
        /// Now includes breakdown by transaction type (Direct vs Transfer)
        /// </summary>
        /// <param name="godownID">Filter by specific godown (optional)</param>
        /// <param name="godownItemID">Filter by specific item (optional)</param>
        /// <param name="asOnDate">Filter transactions up to this date (optional, defaults to all)</param>
        public static List<GodownStockViewModel> GetCurrentStock(int? godownID = null, int? godownItemID = null, DateTime? asOnDate = null)
        {
            var stockEntries = new Dictionary<string, GodownStockViewModel>();

            try
            {
                using (var conn = DatabaseManager.GetConnection())
                {
                    conn.Open();

                    // Query 1: Get opening stock entries with godown and item names
                    // Opening stock is always included (it's the starting balance)
                    string openingSql = @"SELECT os.GodownID, os.GodownItemID, g.GodownName, g.GodownShortName,
                                         i.ItemName, SUM(os.Quantity) as OpeningStock
                                         FROM (GodownOpeningStock os
                                         INNER JOIN GodownMaster g ON os.GodownID = g.GodownID)
                                         INNER JOIN GodownItemMaster i ON os.GodownItemID = i.GodownItemID
                                         WHERE 1=1";

                    var openingParams = new List<OleDbParameter>();
                    if (godownID.HasValue)
                    {
                        openingSql += " AND os.GodownID = ?";
                        openingParams.Add(new OleDbParameter("@GodownID", OleDbType.Integer) { Value = godownID.Value });
                    }
                    if (godownItemID.HasValue)
                    {
                        openingSql += " AND os.GodownItemID = ?";
                        openingParams.Add(new OleDbParameter("@GodownItemID", OleDbType.Integer) { Value = godownItemID.Value });
                    }
                    // Note: Opening stock is always included regardless of date - it's the starting balance
                    openingSql += " GROUP BY os.GodownID, os.GodownItemID, g.GodownName, g.GodownShortName, i.ItemName";

                    using (var cmd = new OleDbCommand(openingSql, conn))
                    {
                        foreach (var p in openingParams) cmd.Parameters.Add(p);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string key = $"{reader["GodownID"]}_{reader["GodownItemID"]}";
                                stockEntries[key] = new GodownStockViewModel
                                {
                                    GodownID = Convert.ToInt32(reader["GodownID"]),
                                    GodownItemID = Convert.ToInt32(reader["GodownItemID"]),
                                    GodownName = reader["GodownName"]?.ToString() ?? "",
                                    GodownShortName = reader["GodownShortName"]?.ToString() ?? "",
                                    ItemName = reader["ItemName"]?.ToString() ?? "",
                                    OpeningStock = reader["OpeningStock"] != DBNull.Value ? Convert.ToDouble(reader["OpeningStock"]) : 0
                                };
                            }
                        }
                    }

                    // Query 2: Get ledger entries with breakdown by transaction type
                    // This query separates Direct Inward/Outward from Transfer In/Out
                    string ledgerSql = @"SELECT gl.GodownID, gl.GodownItemID, g.GodownName, g.GodownShortName,
                                        i.ItemName, gl.TransactionType,
                                        SUM(gl.InwardQty) as TotalInward, SUM(gl.OutwardQty) as TotalOutward
                                        FROM (GodownLedger gl
                                        INNER JOIN GodownMaster g ON gl.GodownID = g.GodownID)
                                        INNER JOIN GodownItemMaster i ON gl.GodownItemID = i.GodownItemID
                                        WHERE 1=1";

                    var ledgerParams = new List<OleDbParameter>();
                    if (godownID.HasValue)
                    {
                        ledgerSql += " AND gl.GodownID = ?";
                        ledgerParams.Add(new OleDbParameter("@GodownID", OleDbType.Integer) { Value = godownID.Value });
                    }
                    if (godownItemID.HasValue)
                    {
                        ledgerSql += " AND gl.GodownItemID = ?";
                        ledgerParams.Add(new OleDbParameter("@GodownItemID", OleDbType.Integer) { Value = godownItemID.Value });
                    }
                    // Filter ledger entries by date if provided
                    if (asOnDate.HasValue)
                    {
                        ledgerSql += " AND gl.TransactionDate <= ?";
                        ledgerParams.Add(new OleDbParameter("@AsOnDate", OleDbType.Date) { Value = asOnDate.Value.Date });
                    }
                    ledgerSql += " GROUP BY gl.GodownID, gl.GodownItemID, g.GodownName, g.GodownShortName, i.ItemName, gl.TransactionType";

                    using (var cmd = new OleDbCommand(ledgerSql, conn))
                    {
                        foreach (var p in ledgerParams) cmd.Parameters.Add(p);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string key = $"{reader["GodownID"]}_{reader["GodownItemID"]}";
                                if (!stockEntries.ContainsKey(key))
                                {
                                    stockEntries[key] = new GodownStockViewModel
                                    {
                                        GodownID = Convert.ToInt32(reader["GodownID"]),
                                        GodownItemID = Convert.ToInt32(reader["GodownItemID"]),
                                        GodownName = reader["GodownName"]?.ToString() ?? "",
                                        GodownShortName = reader["GodownShortName"]?.ToString() ?? "",
                                        ItemName = reader["ItemName"]?.ToString() ?? ""
                                    };
                                }

                                string transactionType = reader["TransactionType"]?.ToString() ?? "";
                                double inwardQty = reader["TotalInward"] != DBNull.Value ? Convert.ToDouble(reader["TotalInward"]) : 0;
                                double outwardQty = reader["TotalOutward"] != DBNull.Value ? Convert.ToDouble(reader["TotalOutward"]) : 0;

                                // Categorize based on transaction type
                                if (transactionType == "Inward")
                                {
                                    // Direct inward transaction
                                    stockEntries[key].DirectInward += inwardQty;
                                }
                                else if (transactionType == "Outward")
                                {
                                    // Direct outward transaction
                                    stockEntries[key].DirectOutward += outwardQty;
                                }
                                else if (transactionType == "Transfer")
                                {
                                    // Transfer: InwardQty means stock came IN (Transfer In)
                                    //           OutwardQty means stock went OUT (Transfer Out)
                                    stockEntries[key].TransferIn += inwardQty;
                                    stockEntries[key].TransferOut += outwardQty;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetCurrentStock: {ex.Message}");
                // If the optimized approach fails, fall back to individual queries
                return GetCurrentStockLegacy(godownID, godownItemID, asOnDate);
            }

            // Calculate current stock and build result list
            var stockList = new List<GodownStockViewModel>();
            foreach (var entry in stockEntries.Values)
            {
                // TotalInward and TotalOutward are now computed properties in the model
                entry.CurrentStock = entry.OpeningStock + entry.TotalInward - entry.TotalOutward;
                if (entry.OpeningStock > 0 || entry.TotalInward > 0 || entry.TotalOutward > 0 || entry.CurrentStock != 0)
                {
                    stockList.Add(entry);
                }
            }

            // Sort by godown name then item name
            stockList.Sort((a, b) =>
            {
                int cmp = string.Compare(a.GodownName, b.GodownName, StringComparison.OrdinalIgnoreCase);
                if (cmp != 0) return cmp;
                return string.Compare(a.ItemName, b.ItemName, StringComparison.OrdinalIgnoreCase);
            });

            return stockList;
        }

        /// <summary>
        /// Legacy method using individual queries - slower but guaranteed to work
        /// Now includes breakdown by transaction type (Direct vs Transfer)
        /// </summary>
        private static List<GodownStockViewModel> GetCurrentStockLegacy(int? godownID = null, int? godownItemID = null, DateTime? asOnDate = null)
        {
            List<GodownStockViewModel> stockList = new List<GodownStockViewModel>();

            // Get all godowns
            List<Godown> godowns = GodownService.GetAllGodowns();
            if (godownID.HasValue)
            {
                var godown = godowns.Find(g => g.GodownID == godownID.Value);
                if (godown != null)
                {
                    godowns = new List<Godown> { godown };
                }
                else
                {
                    return stockList;
                }
            }

            // Get all godown items
            List<GodownItem> items = GodownItemService.GetAllGodownItems();
            if (godownItemID.HasValue)
            {
                var item = items.Find(i => i.GodownItemID == godownItemID.Value);
                if (item != null)
                {
                    items = new List<GodownItem> { item };
                }
                else
                {
                    return stockList;
                }
            }

            // Calculate stock for each godown-item combination
            foreach (var godown in godowns)
            {
                foreach (var item in items)
                {
                    double openingStock = GetOpeningStock(godown.GodownID, item.GodownItemID, asOnDate);
                    double directInward = GetInwardByType(godown.GodownID, item.GodownItemID, "Inward", asOnDate);
                    double transferIn = GetInwardByType(godown.GodownID, item.GodownItemID, "Transfer", asOnDate);
                    double directOutward = GetOutwardByType(godown.GodownID, item.GodownItemID, "Outward", asOnDate);
                    double transferOut = GetOutwardByType(godown.GodownID, item.GodownItemID, "Transfer", asOnDate);

                    double totalInward = directInward + transferIn;
                    double totalOutward = directOutward + transferOut;
                    double currentStock = openingStock + totalInward - totalOutward;

                    // Only add items that have some stock activity
                    if (openingStock > 0 || totalInward > 0 || totalOutward > 0 || currentStock > 0)
                    {
                        stockList.Add(new GodownStockViewModel
                        {
                            GodownID = godown.GodownID,
                            GodownName = godown.GodownName,
                            GodownShortName = godown.GodownShortName,
                            GodownItemID = item.GodownItemID,
                            ItemName = item.ItemName,
                            OpeningStock = openingStock,
                            DirectInward = directInward,
                            TransferIn = transferIn,
                            DirectOutward = directOutward,
                            TransferOut = transferOut,
                            CurrentStock = currentStock
                        });
                    }
                }
            }

            return stockList;
        }

        public static double GetOpeningStock(int godownID, int godownItemID, DateTime? asOnDate = null)
        {
            // Opening stock is always included regardless of date - it's the starting balance
            string sql = @"SELECT SUM(Quantity) FROM GodownOpeningStock 
                          WHERE GodownID = ? AND GodownItemID = ?";
            var parameters = new List<OleDbParameter>
            {
                new OleDbParameter("@GodownID", OleDbType.Integer) { Value = godownID },
                new OleDbParameter("@GodownItemID", OleDbType.Integer) { Value = godownItemID }
            };

            object result = DatabaseManager.ExecuteScalar(sql, parameters.ToArray());
            if (result != null && result != DBNull.Value)
            {
                return Convert.ToDouble(result);
            }
            return 0;
        }

        public static double GetTotalInward(int godownID, int godownItemID, DateTime? asOnDate = null)
        {
            string sql = @"SELECT SUM(InwardQty) FROM GodownLedger 
                          WHERE GodownID = ? AND GodownItemID = ?";
            var parameters = new List<OleDbParameter>
            {
                new OleDbParameter("@GodownID", OleDbType.Integer) { Value = godownID },
                new OleDbParameter("@GodownItemID", OleDbType.Integer) { Value = godownItemID }
            };

            if (asOnDate.HasValue)
            {
                sql += " AND TransactionDate <= ?";
                parameters.Add(new OleDbParameter("@AsOnDate", OleDbType.Date) { Value = asOnDate.Value.Date });
            }

            object result = DatabaseManager.ExecuteScalar(sql, parameters.ToArray());
            if (result != null && result != DBNull.Value)
            {
                return Convert.ToDouble(result);
            }
            return 0;
        }

        public static double GetTotalOutward(int godownID, int godownItemID, DateTime? asOnDate = null)
        {
            string sql = @"SELECT SUM(OutwardQty) FROM GodownLedger
                          WHERE GodownID = ? AND GodownItemID = ?";
            var parameters = new List<OleDbParameter>
            {
                new OleDbParameter("@GodownID", OleDbType.Integer) { Value = godownID },
                new OleDbParameter("@GodownItemID", OleDbType.Integer) { Value = godownItemID }
            };

            if (asOnDate.HasValue)
            {
                sql += " AND TransactionDate <= ?";
                parameters.Add(new OleDbParameter("@AsOnDate", OleDbType.Date) { Value = asOnDate.Value.Date });
            }

            object result = DatabaseManager.ExecuteScalar(sql, parameters.ToArray());
            if (result != null && result != DBNull.Value)
            {
                return Convert.ToDouble(result);
            }
            return 0;
        }

        /// <summary>
        /// Get inward quantity filtered by transaction type
        /// </summary>
        public static double GetInwardByType(int godownID, int godownItemID, string transactionType, DateTime? asOnDate = null)
        {
            string sql = @"SELECT SUM(InwardQty) FROM GodownLedger
                          WHERE GodownID = ? AND GodownItemID = ? AND TransactionType = ?";
            var parameters = new List<OleDbParameter>
            {
                new OleDbParameter("@GodownID", OleDbType.Integer) { Value = godownID },
                new OleDbParameter("@GodownItemID", OleDbType.Integer) { Value = godownItemID },
                new OleDbParameter("@TransactionType", OleDbType.VarChar) { Value = transactionType }
            };

            if (asOnDate.HasValue)
            {
                sql += " AND TransactionDate <= ?";
                parameters.Add(new OleDbParameter("@AsOnDate", OleDbType.Date) { Value = asOnDate.Value.Date });
            }

            object result = DatabaseManager.ExecuteScalar(sql, parameters.ToArray());
            if (result != null && result != DBNull.Value)
            {
                return Convert.ToDouble(result);
            }
            return 0;
        }

        /// <summary>
        /// Get outward quantity filtered by transaction type
        /// </summary>
        public static double GetOutwardByType(int godownID, int godownItemID, string transactionType, DateTime? asOnDate = null)
        {
            string sql = @"SELECT SUM(OutwardQty) FROM GodownLedger
                          WHERE GodownID = ? AND GodownItemID = ? AND TransactionType = ?";
            var parameters = new List<OleDbParameter>
            {
                new OleDbParameter("@GodownID", OleDbType.Integer) { Value = godownID },
                new OleDbParameter("@GodownItemID", OleDbType.Integer) { Value = godownItemID },
                new OleDbParameter("@TransactionType", OleDbType.VarChar) { Value = transactionType }
            };

            if (asOnDate.HasValue)
            {
                sql += " AND TransactionDate <= ?";
                parameters.Add(new OleDbParameter("@AsOnDate", OleDbType.Date) { Value = asOnDate.Value.Date });
            }

            object result = DatabaseManager.ExecuteScalar(sql, parameters.ToArray());
            if (result != null && result != DBNull.Value)
            {
                return Convert.ToDouble(result);
            }
            return 0;
        }
    }
}
