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
                    // Opening stock is always included - it's the initial/base stock (no date filter)
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
                    // No date filter on opening stock - it's always included as the base/initial stock
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
                    // Opening stock is always included (no date filter) - it's the initial/base stock
                    double openingStock = GetOpeningStock(godown.GodownID, item.GodownItemID, null);
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
            // Opening stock is always included - it's the initial/base stock
            // The asOnDate parameter is kept for backward compatibility but typically pass null
            string sql = @"SELECT SUM(Quantity) FROM GodownOpeningStock
                          WHERE GodownID = ? AND GodownItemID = ?";
            var parameters = new List<OleDbParameter>
            {
                new OleDbParameter("@GodownID", OleDbType.Integer) { Value = godownID },
                new OleDbParameter("@GodownItemID", OleDbType.Integer) { Value = godownItemID }
            };

            // Note: Opening stock should typically NOT be filtered by date
            // It represents the initial/base stock that is always included

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
        /// Get total inward BEFORE a specific date (using < instead of <=)
        /// This handles transactions that have time components correctly
        /// </summary>
        public static double GetTotalInwardBeforeDate(int godownID, int godownItemID, DateTime beforeDate)
        {
            string sql = @"SELECT SUM(InwardQty) FROM GodownLedger
                          WHERE GodownID = ? AND GodownItemID = ? AND TransactionDate < ?";
            var parameters = new OleDbParameter[]
            {
                new OleDbParameter("@GodownID", OleDbType.Integer) { Value = godownID },
                new OleDbParameter("@GodownItemID", OleDbType.Integer) { Value = godownItemID },
                new OleDbParameter("@BeforeDate", OleDbType.Date) { Value = beforeDate.Date }
            };

            object result = DatabaseManager.ExecuteScalar(sql, parameters);
            if (result != null && result != DBNull.Value)
            {
                return Convert.ToDouble(result);
            }
            return 0;
        }

        /// <summary>
        /// Get total outward BEFORE a specific date (using < instead of <=)
        /// This handles transactions that have time components correctly
        /// </summary>
        public static double GetTotalOutwardBeforeDate(int godownID, int godownItemID, DateTime beforeDate)
        {
            string sql = @"SELECT SUM(OutwardQty) FROM GodownLedger
                          WHERE GodownID = ? AND GodownItemID = ? AND TransactionDate < ?";
            var parameters = new OleDbParameter[]
            {
                new OleDbParameter("@GodownID", OleDbType.Integer) { Value = godownID },
                new OleDbParameter("@GodownItemID", OleDbType.Integer) { Value = godownItemID },
                new OleDbParameter("@BeforeDate", OleDbType.Date) { Value = beforeDate.Date }
            };

            object result = DatabaseManager.ExecuteScalar(sql, parameters);
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

        /// <summary>
        /// Get ledger entries for detailed stock ledger report
        /// </summary>
        public static List<GodownLedgerViewModel> GetLedgerEntries(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int? godownID = null,
            int? godownItemID = null)
        {
            var ledgerEntries = new List<GodownLedgerViewModel>();

            try
            {
                string sql = @"SELECT gl.LedgerID, gl.TransactionDate, gl.TransactionType,
                              gl.GodownID, gl.GodownItemID, gl.InwardQty, gl.OutwardQty, gl.BalanceQty,
                              gl.TransactionID, g.GodownName, i.ItemName,
                              tm.TransactionNo, tm.FromGodownID, tm.ToGodownID,
                              fg.GodownName AS FromGodownName, tg.GodownName AS ToGodownName
                              FROM ((((GodownLedger gl
                              INNER JOIN GodownMaster g ON gl.GodownID = g.GodownID)
                              INNER JOIN GodownItemMaster i ON gl.GodownItemID = i.GodownItemID)
                              LEFT JOIN GodownTransactionMaster tm ON gl.TransactionID = tm.TransactionID)
                              LEFT JOIN GodownMaster fg ON tm.FromGodownID = fg.GodownID)
                              LEFT JOIN GodownMaster tg ON tm.ToGodownID = tg.GodownID
                              WHERE 1=1";

                var parameters = new List<OleDbParameter>();

                if (fromDate.HasValue)
                {
                    sql += " AND gl.TransactionDate >= ?";
                    parameters.Add(new OleDbParameter("@FromDate", OleDbType.Date) { Value = fromDate.Value.Date });
                }

                if (toDate.HasValue)
                {
                    sql += " AND gl.TransactionDate <= ?";
                    parameters.Add(new OleDbParameter("@ToDate", OleDbType.Date) { Value = toDate.Value.Date });
                }

                if (godownID.HasValue)
                {
                    sql += " AND gl.GodownID = ?";
                    parameters.Add(new OleDbParameter("@GodownID", OleDbType.Integer) { Value = godownID.Value });
                }

                if (godownItemID.HasValue)
                {
                    sql += " AND gl.GodownItemID = ?";
                    parameters.Add(new OleDbParameter("@GodownItemID", OleDbType.Integer) { Value = godownItemID.Value });
                }

                sql += " ORDER BY gl.GodownID, gl.GodownItemID, gl.TransactionDate, gl.LedgerID";

                DataTable dt = DatabaseManager.ExecuteQuery(sql, parameters.ToArray());

                foreach (DataRow row in dt.Rows)
                {
                    string transactionType = row["TransactionType"]?.ToString() ?? "";
                    string fromGodown = row["FromGodownName"] != DBNull.Value ? row["FromGodownName"].ToString() : "";
                    string toGodown = row["ToGodownName"] != DBNull.Value ? row["ToGodownName"].ToString() : "";

                    // Build remarks based on transaction type
                    string remarks = "";
                    if (transactionType == "Transfer")
                    {
                        double inwardQty = row["InwardQty"] != DBNull.Value ? Convert.ToDouble(row["InwardQty"]) : 0;
                        if (inwardQty > 0)
                        {
                            remarks = $"Transfer from {fromGodown}";
                        }
                        else
                        {
                            remarks = $"Transfer to {toGodown}";
                        }
                    }
                    else if (transactionType == "Inward")
                    {
                        remarks = "Direct Inward";
                    }
                    else if (transactionType == "Outward")
                    {
                        remarks = "Direct Outward";
                    }

                    ledgerEntries.Add(new GodownLedgerViewModel
                    {
                        LedgerID = Convert.ToInt32(row["LedgerID"]),
                        TransactionDate = Convert.ToDateTime(row["TransactionDate"]),
                        TransactionNo = row["TransactionNo"] != DBNull.Value ? row["TransactionNo"].ToString() : "",
                        TransactionType = transactionType,
                        GodownID = Convert.ToInt32(row["GodownID"]),
                        GodownName = row["GodownName"]?.ToString() ?? "",
                        GodownItemID = Convert.ToInt32(row["GodownItemID"]),
                        ItemName = row["ItemName"]?.ToString() ?? "",
                        InwardQty = row["InwardQty"] != DBNull.Value ? Convert.ToDouble(row["InwardQty"]) : 0,
                        OutwardQty = row["OutwardQty"] != DBNull.Value ? Convert.ToDouble(row["OutwardQty"]) : 0,
                        BalanceQty = row["BalanceQty"] != DBNull.Value ? Convert.ToDouble(row["BalanceQty"]) : 0,
                        FromGodown = fromGodown,
                        ToGodown = toGodown,
                        Remarks = remarks
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetLedgerEntries: {ex.Message}");
            }

            return ledgerEntries;
        }

        /// <summary>
        /// Get ledger entries with running balance calculated
        /// Adds an "Opening" row at the top for each godown+item showing balance before fromDate
        /// Opening balance includes: Opening Stock + Inward (before fromDate) - Outward (before fromDate)
        /// OPTIMIZED: Uses bulk queries instead of per-combination queries for better performance
        /// </summary>
        public static List<GodownLedgerViewModel> GetLedgerEntriesWithRunningBalance(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int? godownID = null,
            int? godownItemID = null)
        {
            var entries = GetLedgerEntries(fromDate, toDate, godownID, godownItemID);

            // OPTIMIZED: Get all opening balances in bulk queries instead of per-combination
            var openingBalances = GetAllOpeningBalancesBulk(godownID, godownItemID, fromDate);

            // Create result list with opening rows first
            var result = new List<GodownLedgerViewModel>();

            // Add opening balance rows for each godown+item combination
            foreach (var kvp in openingBalances.OrderBy(x => x.Value.GodownName).ThenBy(x => x.Value.ItemName))
            {
                var opening = kvp.Value;
                result.Add(new GodownLedgerViewModel
                {
                    LedgerID = 0,
                    TransactionDate = fromDate ?? DateTime.Today,
                    TransactionNo = "",
                    TransactionType = "Opening",
                    GodownID = opening.GodownID,
                    GodownName = opening.GodownName,
                    GodownItemID = opening.GodownItemID,
                    ItemName = opening.ItemName,
                    InwardQty = 0,
                    OutwardQty = 0,
                    BalanceQty = opening.Balance,
                    FromGodown = "",
                    ToGodown = "",
                    Remarks = "Opening Balance"
                });
            }

            // Now add the actual entries with running balance
            var balanceTracker = new Dictionary<string, double>();
            foreach (var kvp in openingBalances)
            {
                balanceTracker[kvp.Key] = kvp.Value.Balance;
            }

            foreach (var entry in entries)
            {
                string key = $"{entry.GodownID}_{entry.GodownItemID}";

                // Calculate running balance
                balanceTracker[key] = balanceTracker[key] + entry.InwardQty - entry.OutwardQty;
                entry.BalanceQty = balanceTracker[key];

                result.Add(entry);
            }

            return result;
        }

        /// <summary>
        /// Calculate opening balance for a godown+item combination
        /// Opening balance = Opening Stock (always included) + Inward (before fromDate) - Outward (before fromDate)
        /// </summary>
        private static double CalculateOpeningBalance(int godownID, int godownItemID, DateTime? fromDate)
        {
            // Opening stock is always included - it's the initial/base stock (no date filter)
            double openingStock = GetOpeningStock(godownID, godownItemID, null);

            if (fromDate.HasValue)
            {
                // Get all inward transactions BEFORE the from date (using < instead of <= to handle time component)
                double inwardBeforeDate = GetTotalInwardBeforeDate(godownID, godownItemID, fromDate.Value);
                // Get all outward transactions BEFORE the from date (using < instead of <= to handle time component)
                double outwardBeforeDate = GetTotalOutwardBeforeDate(godownID, godownItemID, fromDate.Value);

                return openingStock + inwardBeforeDate - outwardBeforeDate;
            }
            else
            {
                // No from date filter - just opening stock
                return openingStock;
            }
        }

        /// <summary>
        /// OPTIMIZED: Get all opening balances in bulk using only 3 queries instead of N*3 queries
        /// Returns a dictionary with key = "GodownID_GodownItemID" and value = (GodownID, GodownItemID, GodownName, ItemName, Balance)
        /// </summary>
        private static Dictionary<string, (int GodownID, int GodownItemID, string GodownName, string ItemName, double Balance)> GetAllOpeningBalancesBulk(
            int? godownID = null, int? godownItemID = null, DateTime? fromDate = null)
        {
            var result = new Dictionary<string, (int GodownID, int GodownItemID, string GodownName, string ItemName, double Balance)>();

            try
            {
                // Step 1: Get all opening stock in one query
                var openingStocks = new Dictionary<string, (int GodownID, int GodownItemID, string GodownName, string ItemName, double Qty)>();

                string openingSql = @"SELECT os.GodownID, os.GodownItemID, g.GodownName, i.ItemName, SUM(os.Quantity) as TotalQty
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
                openingSql += " GROUP BY os.GodownID, os.GodownItemID, g.GodownName, i.ItemName";

                DataTable dtOpening = DatabaseManager.ExecuteQuery(openingSql, openingParams.ToArray());
                foreach (DataRow row in dtOpening.Rows)
                {
                    int gid = Convert.ToInt32(row["GodownID"]);
                    int iid = Convert.ToInt32(row["GodownItemID"]);
                    string key = $"{gid}_{iid}";
                    double qty = row["TotalQty"] != DBNull.Value ? Convert.ToDouble(row["TotalQty"]) : 0;
                    openingStocks[key] = (gid, iid, row["GodownName"]?.ToString() ?? "", row["ItemName"]?.ToString() ?? "", qty);
                }

                // Step 2: Get all inward/outward before fromDate in one query
                var priorTransactions = new Dictionary<string, (double Inward, double Outward)>();

                if (fromDate.HasValue)
                {
                    string ledgerSql = @"SELECT gl.GodownID, gl.GodownItemID, g.GodownName, i.ItemName,
                                        SUM(gl.InwardQty) as TotalInward, SUM(gl.OutwardQty) as TotalOutward
                                        FROM (GodownLedger gl
                                        INNER JOIN GodownMaster g ON gl.GodownID = g.GodownID)
                                        INNER JOIN GodownItemMaster i ON gl.GodownItemID = i.GodownItemID
                                        WHERE gl.TransactionDate < ?";
                    var ledgerParams = new List<OleDbParameter>();
                    ledgerParams.Add(new OleDbParameter("@BeforeDate", OleDbType.Date) { Value = fromDate.Value.Date });

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
                    ledgerSql += " GROUP BY gl.GodownID, gl.GodownItemID, g.GodownName, i.ItemName";

                    DataTable dtLedger = DatabaseManager.ExecuteQuery(ledgerSql, ledgerParams.ToArray());
                    foreach (DataRow row in dtLedger.Rows)
                    {
                        int gid = Convert.ToInt32(row["GodownID"]);
                        int iid = Convert.ToInt32(row["GodownItemID"]);
                        string key = $"{gid}_{iid}";
                        double inward = row["TotalInward"] != DBNull.Value ? Convert.ToDouble(row["TotalInward"]) : 0;
                        double outward = row["TotalOutward"] != DBNull.Value ? Convert.ToDouble(row["TotalOutward"]) : 0;
                        priorTransactions[key] = (inward, outward);

                        // Also add to openingStocks if not already there (items with transactions but no opening stock)
                        if (!openingStocks.ContainsKey(key))
                        {
                            openingStocks[key] = (gid, iid, row["GodownName"]?.ToString() ?? "", row["ItemName"]?.ToString() ?? "", 0);
                        }
                    }
                }

                // Step 3: Also get combinations from ledger that are within the date range (for items with no prior transactions or opening stock)
                string allLedgerSql = @"SELECT DISTINCT gl.GodownID, gl.GodownItemID, g.GodownName, i.ItemName
                                       FROM (GodownLedger gl
                                       INNER JOIN GodownMaster g ON gl.GodownID = g.GodownID)
                                       INNER JOIN GodownItemMaster i ON gl.GodownItemID = i.GodownItemID
                                       WHERE 1=1";
                var allLedgerParams = new List<OleDbParameter>();

                if (godownID.HasValue)
                {
                    allLedgerSql += " AND gl.GodownID = ?";
                    allLedgerParams.Add(new OleDbParameter("@GodownID", OleDbType.Integer) { Value = godownID.Value });
                }
                if (godownItemID.HasValue)
                {
                    allLedgerSql += " AND gl.GodownItemID = ?";
                    allLedgerParams.Add(new OleDbParameter("@GodownItemID", OleDbType.Integer) { Value = godownItemID.Value });
                }

                DataTable dtAllLedger = DatabaseManager.ExecuteQuery(allLedgerSql, allLedgerParams.ToArray());
                foreach (DataRow row in dtAllLedger.Rows)
                {
                    int gid = Convert.ToInt32(row["GodownID"]);
                    int iid = Convert.ToInt32(row["GodownItemID"]);
                    string key = $"{gid}_{iid}";
                    if (!openingStocks.ContainsKey(key))
                    {
                        openingStocks[key] = (gid, iid, row["GodownName"]?.ToString() ?? "", row["ItemName"]?.ToString() ?? "", 0);
                    }
                }

                // Step 4: Calculate final opening balances
                foreach (var kvp in openingStocks)
                {
                    string key = kvp.Key;
                    var stock = kvp.Value;
                    double openingQty = stock.Qty;
                    double inward = 0, outward = 0;

                    if (priorTransactions.ContainsKey(key))
                    {
                        inward = priorTransactions[key].Inward;
                        outward = priorTransactions[key].Outward;
                    }

                    double balance = openingQty + inward - outward;
                    result[key] = (stock.GodownID, stock.GodownItemID, stock.GodownName, stock.ItemName, balance);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetAllOpeningBalancesBulk: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Get all godown+item combinations from BOTH opening stock table AND ledger table
        /// This ensures items with only transactions (no opening stock) also get an opening row
        /// </summary>
        private static List<(int GodownID, int GodownItemID, string GodownName, string ItemName)> GetAllGodownItemCombinations(
            int? godownID = null, int? godownItemID = null)
        {
            var combinations = new Dictionary<string, (int GodownID, int GodownItemID, string GodownName, string ItemName)>();

            try
            {
                // First, get combinations from opening stock table
                string openingSql = @"SELECT DISTINCT os.GodownID, os.GodownItemID, g.GodownName, i.ItemName
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

                DataTable dtOpening = DatabaseManager.ExecuteQuery(openingSql, openingParams.ToArray());

                foreach (DataRow row in dtOpening.Rows)
                {
                    int gid = Convert.ToInt32(row["GodownID"]);
                    int iid = Convert.ToInt32(row["GodownItemID"]);
                    string key = $"{gid}_{iid}";

                    if (!combinations.ContainsKey(key))
                    {
                        combinations[key] = (gid, iid, row["GodownName"]?.ToString() ?? "", row["ItemName"]?.ToString() ?? "");
                    }
                }

                // Second, get combinations from ledger table (items that have transactions but may not have opening stock)
                string ledgerSql = @"SELECT DISTINCT gl.GodownID, gl.GodownItemID, g.GodownName, i.ItemName
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

                DataTable dtLedger = DatabaseManager.ExecuteQuery(ledgerSql, ledgerParams.ToArray());

                foreach (DataRow row in dtLedger.Rows)
                {
                    int gid = Convert.ToInt32(row["GodownID"]);
                    int iid = Convert.ToInt32(row["GodownItemID"]);
                    string key = $"{gid}_{iid}";

                    if (!combinations.ContainsKey(key))
                    {
                        combinations[key] = (gid, iid, row["GodownName"]?.ToString() ?? "", row["ItemName"]?.ToString() ?? "");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetAllGodownItemCombinations: {ex.Message}");
            }

            // Return sorted by godown name, then item name
            return combinations.Values
                .OrderBy(x => x.GodownName)
                .ThenBy(x => x.ItemName)
                .ToList();
        }
    }
}
