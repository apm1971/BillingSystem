using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Data
{
    public class GodownTransactionService
    {
        public static string GenerateTransactionNo(string transactionType)
        {
            string prefix = transactionType.Substring(0, 1).ToUpper(); // I, O, or T
            string datePrefix = DateTime.Now.ToString("yyyyMMdd");
            
            // Get the last transaction number for today
            string sql = @"SELECT MAX(TransactionNo) FROM GodownTransactionMaster 
                          WHERE TransactionNo LIKE ?";
            string pattern = $"{prefix}{datePrefix}%";
            OleDbParameter[] parameters = {
                new OleDbParameter("Pattern", OleDbType.VarChar) { Value = pattern }
            };
            
            object result = DatabaseManager.ExecuteScalar(sql, parameters);
            
            if (result != null && result != DBNull.Value)
            {
                string lastNo = result.ToString();
                if (!string.IsNullOrEmpty(lastNo) && lastNo.Length > 9)
                {
                    int sequence = Convert.ToInt32(lastNo.Substring(9));
                    return $"{prefix}{datePrefix}{sequence + 1:D4}";
                }
            }
            
            return $"{prefix}{datePrefix}0001";
        }
        
        public static double GetStockBalance(int godownID, int godownItemID, OleDbConnection conn = null, OleDbTransaction trans = null)
        {
            // Get opening stock
            double openingStock = 0;
            string openingStockSql = @"SELECT SUM(Quantity) FROM GodownOpeningStock 
                                      WHERE GodownID = ? AND GodownItemID = ?";
            OleDbParameter[] openingStockParams = {
                new OleDbParameter("GodownID", OleDbType.Integer) { Value = godownID },
                new OleDbParameter("GodownItemID", OleDbType.Integer) { Value = godownItemID }
            };
            
            object openingStockResult;
            if (conn != null && trans != null)
            {
                using (var cmd = new OleDbCommand(openingStockSql, conn, trans))
                {
                    cmd.Parameters.AddRange(openingStockParams);
                    openingStockResult = cmd.ExecuteScalar();
                }
            }
            else
            {
                openingStockResult = DatabaseManager.ExecuteScalar(openingStockSql, openingStockParams);
            }
            
            if (openingStockResult != null && openingStockResult != DBNull.Value)
            {
                openingStock = Convert.ToDouble(openingStockResult);
            }
            
            // Get total inward
            double totalInward = 0;
            string inwardSql = @"SELECT SUM(InwardQty) FROM GodownLedger 
                                WHERE GodownID = ? AND GodownItemID = ?";
            OleDbParameter[] inwardParams = {
                new OleDbParameter("GodownID", OleDbType.Integer) { Value = godownID },
                new OleDbParameter("GodownItemID", OleDbType.Integer) { Value = godownItemID }
            };
            
            object inwardResult;
            if (conn != null && trans != null)
            {
                using (var cmd = new OleDbCommand(inwardSql, conn, trans))
                {
                    cmd.Parameters.AddRange(inwardParams);
                    inwardResult = cmd.ExecuteScalar();
                }
            }
            else
            {
                inwardResult = DatabaseManager.ExecuteScalar(inwardSql, inwardParams);
            }
            
            if (inwardResult != null && inwardResult != DBNull.Value)
            {
                totalInward = Convert.ToDouble(inwardResult);
            }
            
            // Get total outward
            double totalOutward = 0;
            string outwardSql = @"SELECT SUM(OutwardQty) FROM GodownLedger 
                                  WHERE GodownID = ? AND GodownItemID = ?";
            OleDbParameter[] outwardParams = {
                new OleDbParameter("GodownID", OleDbType.Integer) { Value = godownID },
                new OleDbParameter("GodownItemID", OleDbType.Integer) { Value = godownItemID }
            };
            
            object outwardResult;
            if (conn != null && trans != null)
            {
                using (var cmd = new OleDbCommand(outwardSql, conn, trans))
                {
                    cmd.Parameters.AddRange(outwardParams);
                    outwardResult = cmd.ExecuteScalar();
                }
            }
            else
            {
                outwardResult = DatabaseManager.ExecuteScalar(outwardSql, outwardParams);
            }
            
            if (outwardResult != null && outwardResult != DBNull.Value)
            {
                totalOutward = Convert.ToDouble(outwardResult);
            }
            
            return openingStock + totalInward - totalOutward;
        }
        
        public static bool CreateTransaction(GodownTransaction transaction)
        {
            using (var conn = DatabaseManager.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // Validate stock for outward and transfer transactions
                        if (transaction.TransactionType == "Outward" || transaction.TransactionType == "Transfer")
                        {
                            int sourceGodownID = transaction.TransactionType == "Transfer" 
                                ? transaction.FromGodownID.Value 
                                : transaction.ToGodownID.Value;
                            
                            foreach (var detail in transaction.Details)
                            {
                                double availableStock = GetStockBalance(sourceGodownID, detail.GodownItemID, conn, trans);
                                if (availableStock < detail.Quantity)
                                {
                                    throw new Exception($"Insufficient stock for item '{detail.ItemName}'. Available: {availableStock}, Required: {detail.Quantity}");
                                }
                            }
                        }
                        
                        // Insert transaction master
                        string masterSql = @"INSERT INTO GodownTransactionMaster 
                                           (TransactionNo, TransactionDate, TransactionType, FromGodownID, ToGodownID, 
                                            TotalQuantity, ReferenceNo, CreatedDate, CreatedBy)
                                           VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)";
                        
                        int transactionID;
                        using (var masterCmd = new OleDbCommand(masterSql, conn, trans))
                        {
                            masterCmd.Parameters.Add(new OleDbParameter("TransactionNo", OleDbType.VarChar) { Value = transaction.TransactionNo });
                            masterCmd.Parameters.Add(new OleDbParameter("TransactionDate", OleDbType.Date) { Value = transaction.TransactionDate });
                            masterCmd.Parameters.Add(new OleDbParameter("TransactionType", OleDbType.VarChar) { Value = transaction.TransactionType });
                            masterCmd.Parameters.Add(new OleDbParameter("FromGodownID", OleDbType.Integer) { Value = (object)transaction.FromGodownID ?? DBNull.Value });
                            masterCmd.Parameters.Add(new OleDbParameter("ToGodownID", OleDbType.Integer) { Value = (object)transaction.ToGodownID ?? DBNull.Value });
                            masterCmd.Parameters.Add(new OleDbParameter("TotalQuantity", OleDbType.Double) { Value = transaction.TotalQuantity });
                            masterCmd.Parameters.Add(new OleDbParameter("ReferenceNo", OleDbType.VarChar) { Value = (object)transaction.ReferenceNo ?? DBNull.Value });
                            masterCmd.Parameters.Add(new OleDbParameter("CreatedDate", OleDbType.Date) { Value = transaction.CreatedDate });
                            masterCmd.Parameters.Add(new OleDbParameter("CreatedBy", OleDbType.Integer) { Value = (object)transaction.CreatedBy ?? DBNull.Value });
                            
                            masterCmd.ExecuteNonQuery();
                            
                            // Get the inserted transaction ID
                            using (var getIdCmd = new OleDbCommand("SELECT @@IDENTITY", conn, trans))
                            {
                                transactionID = Convert.ToInt32(getIdCmd.ExecuteScalar());
                            }
                        }
                        
                        // Insert transaction details
                        string detailSql = @"INSERT INTO GodownTransactionDetails 
                                           (TransactionID, GodownItemID, Quantity)
                                           VALUES (?, ?, ?)";
                        
                        foreach (var detail in transaction.Details)
                        {
                            using (var detailCmd = new OleDbCommand(detailSql, conn, trans))
                            {
                                detailCmd.Parameters.Add(new OleDbParameter("TransactionID", OleDbType.Integer) { Value = transactionID });
                                detailCmd.Parameters.Add(new OleDbParameter("GodownItemID", OleDbType.Integer) { Value = detail.GodownItemID });
                                detailCmd.Parameters.Add(new OleDbParameter("Quantity", OleDbType.Double) { Value = detail.Quantity });
                                detailCmd.ExecuteNonQuery();
                            }
                        }
                        
                        // Update ledger
                        UpdateLedger(transaction, transactionID, conn, trans);
                        
                        trans.Commit();
                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }
        
        private static void UpdateLedger(GodownTransaction transaction, int transactionID, OleDbConnection conn, OleDbTransaction trans)
        {
            string ledgerSql = @"INSERT INTO GodownLedger 
                               (GodownID, GodownItemID, TransactionID, TransactionDate, TransactionType, 
                                InwardQty, OutwardQty, BalanceQty)
                               VALUES (?, ?, ?, ?, ?, ?, ?, ?)";
            
            foreach (var detail in transaction.Details)
            {
                if (transaction.TransactionType == "Inward")
                {
                    // Inward: Add to ToGodownID
                    double currentBalance = GetStockBalance(transaction.ToGodownID.Value, detail.GodownItemID, conn, trans);
                    double newBalance = currentBalance + detail.Quantity;
                    
                    using (var ledgerCmd = new OleDbCommand(ledgerSql, conn, trans))
                    {
                        ledgerCmd.Parameters.Add(new OleDbParameter("GodownID", OleDbType.Integer) { Value = transaction.ToGodownID.Value });
                        ledgerCmd.Parameters.Add(new OleDbParameter("GodownItemID", OleDbType.Integer) { Value = detail.GodownItemID });
                        ledgerCmd.Parameters.Add(new OleDbParameter("TransactionID", OleDbType.Integer) { Value = transactionID });
                        ledgerCmd.Parameters.Add(new OleDbParameter("TransactionDate", OleDbType.Date) { Value = transaction.TransactionDate });
                        ledgerCmd.Parameters.Add(new OleDbParameter("TransactionType", OleDbType.VarChar) { Value = transaction.TransactionType });
                        ledgerCmd.Parameters.Add(new OleDbParameter("InwardQty", OleDbType.Double) { Value = detail.Quantity });
                        ledgerCmd.Parameters.Add(new OleDbParameter("OutwardQty", OleDbType.Double) { Value = 0 });
                        ledgerCmd.Parameters.Add(new OleDbParameter("BalanceQty", OleDbType.Double) { Value = newBalance });
                        ledgerCmd.ExecuteNonQuery();
                    }
                }
                else if (transaction.TransactionType == "Outward")
                {
                    // Outward: Deduct from ToGodownID (which is the source godown for outward)
                    double currentBalance = GetStockBalance(transaction.ToGodownID.Value, detail.GodownItemID, conn, trans);
                    double newBalance = currentBalance - detail.Quantity;
                    
                    using (var ledgerCmd = new OleDbCommand(ledgerSql, conn, trans))
                    {
                        ledgerCmd.Parameters.Add(new OleDbParameter("GodownID", OleDbType.Integer) { Value = transaction.ToGodownID.Value });
                        ledgerCmd.Parameters.Add(new OleDbParameter("GodownItemID", OleDbType.Integer) { Value = detail.GodownItemID });
                        ledgerCmd.Parameters.Add(new OleDbParameter("TransactionID", OleDbType.Integer) { Value = transactionID });
                        ledgerCmd.Parameters.Add(new OleDbParameter("TransactionDate", OleDbType.Date) { Value = transaction.TransactionDate });
                        ledgerCmd.Parameters.Add(new OleDbParameter("TransactionType", OleDbType.VarChar) { Value = transaction.TransactionType });
                        ledgerCmd.Parameters.Add(new OleDbParameter("InwardQty", OleDbType.Double) { Value = 0 });
                        ledgerCmd.Parameters.Add(new OleDbParameter("OutwardQty", OleDbType.Double) { Value = detail.Quantity });
                        ledgerCmd.Parameters.Add(new OleDbParameter("BalanceQty", OleDbType.Double) { Value = newBalance });
                        ledgerCmd.ExecuteNonQuery();
                    }
                }
                else if (transaction.TransactionType == "Transfer")
                {
                    // Transfer: Deduct from FromGodownID, Add to ToGodownID
                    // Deduct from source
                    double sourceBalance = GetStockBalance(transaction.FromGodownID.Value, detail.GodownItemID, conn, trans);
                    double newSourceBalance = sourceBalance - detail.Quantity;
                    
                    using (var ledgerCmd = new OleDbCommand(ledgerSql, conn, trans))
                    {
                        ledgerCmd.Parameters.Add(new OleDbParameter("GodownID", OleDbType.Integer) { Value = transaction.FromGodownID.Value });
                        ledgerCmd.Parameters.Add(new OleDbParameter("GodownItemID", OleDbType.Integer) { Value = detail.GodownItemID });
                        ledgerCmd.Parameters.Add(new OleDbParameter("TransactionID", OleDbType.Integer) { Value = transactionID });
                        ledgerCmd.Parameters.Add(new OleDbParameter("TransactionDate", OleDbType.Date) { Value = transaction.TransactionDate });
                        ledgerCmd.Parameters.Add(new OleDbParameter("TransactionType", OleDbType.VarChar) { Value = transaction.TransactionType });
                        ledgerCmd.Parameters.Add(new OleDbParameter("InwardQty", OleDbType.Double) { Value = 0 });
                        ledgerCmd.Parameters.Add(new OleDbParameter("OutwardQty", OleDbType.Double) { Value = detail.Quantity });
                        ledgerCmd.Parameters.Add(new OleDbParameter("BalanceQty", OleDbType.Double) { Value = newSourceBalance });
                        ledgerCmd.ExecuteNonQuery();
                    }
                    
                    // Add to destination
                    double destBalance = GetStockBalance(transaction.ToGodownID.Value, detail.GodownItemID, conn, trans);
                    double newDestBalance = destBalance + detail.Quantity;
                    
                    using (var ledgerCmd = new OleDbCommand(ledgerSql, conn, trans))
                    {
                        ledgerCmd.Parameters.Add(new OleDbParameter("GodownID", OleDbType.Integer) { Value = transaction.ToGodownID.Value });
                        ledgerCmd.Parameters.Add(new OleDbParameter("GodownItemID", OleDbType.Integer) { Value = detail.GodownItemID });
                        ledgerCmd.Parameters.Add(new OleDbParameter("TransactionID", OleDbType.Integer) { Value = transactionID });
                        ledgerCmd.Parameters.Add(new OleDbParameter("TransactionDate", OleDbType.Date) { Value = transaction.TransactionDate });
                        ledgerCmd.Parameters.Add(new OleDbParameter("TransactionType", OleDbType.VarChar) { Value = transaction.TransactionType });
                        ledgerCmd.Parameters.Add(new OleDbParameter("InwardQty", OleDbType.Double) { Value = detail.Quantity });
                        ledgerCmd.Parameters.Add(new OleDbParameter("OutwardQty", OleDbType.Double) { Value = 0 });
                        ledgerCmd.Parameters.Add(new OleDbParameter("BalanceQty", OleDbType.Double) { Value = newDestBalance });
                        ledgerCmd.ExecuteNonQuery();
                    }
                }
            }
        }
        
        public static List<GodownTransaction> GetTransactions(DateTime? fromDate = null, DateTime? toDate = null, int? godownID = null)
        {
            List<GodownTransaction> transactions = new List<GodownTransaction>();
            
            string sql = @"SELECT tm.*, 
                          fg.GodownName AS FromGodownName, 
                          tg.GodownName AS ToGodownName
                          FROM ((GodownTransactionMaster tm
                          LEFT JOIN GodownMaster fg ON tm.FromGodownID = fg.GodownID)
                          LEFT JOIN GodownMaster tg ON tm.ToGodownID = tg.GodownID)
                          WHERE 1=1";
            
            List<OleDbParameter> parameters = new List<OleDbParameter>();
            
            if (fromDate.HasValue)
            {
                sql += " AND tm.TransactionDate >= ?";
                parameters.Add(new OleDbParameter("FromDate", OleDbType.Date) { Value = fromDate.Value });
            }
            
            if (toDate.HasValue)
            {
                sql += " AND tm.TransactionDate <= ?";
                parameters.Add(new OleDbParameter("ToDate", OleDbType.Date) { Value = toDate.Value });
            }
            
            if (godownID.HasValue)
            {
                sql += " AND (tm.FromGodownID = ? OR tm.ToGodownID = ?)";
                parameters.Add(new OleDbParameter("GodownID1", OleDbType.Integer) { Value = godownID.Value });
                parameters.Add(new OleDbParameter("GodownID2", OleDbType.Integer) { Value = godownID.Value });
            }
            
            sql += " ORDER BY tm.TransactionDate DESC, tm.TransactionID DESC";
            
            DataTable dt = DatabaseManager.ExecuteQuery(sql, parameters.ToArray());
            
            foreach (DataRow row in dt.Rows)
            {
                var transaction = MapRowToTransaction(row);
                
                // Load details
                string detailSql = @"SELECT td.*, gi.ItemName 
                                    FROM (GodownTransactionDetails td
                                    INNER JOIN GodownItemMaster gi ON td.GodownItemID = gi.GodownItemID)
                                    WHERE td.TransactionID = ?";
                OleDbParameter[] detailParams = {
                    new OleDbParameter("TransactionID", OleDbType.Integer) { Value = transaction.TransactionID }
                };
                
                DataTable detailDt = DatabaseManager.ExecuteQuery(detailSql, detailParams);
                foreach (DataRow detailRow in detailDt.Rows)
                {
                    transaction.Details.Add(new GodownTransactionDetail
                    {
                        DetailID = Convert.ToInt32(detailRow["DetailID"]),
                        TransactionID = transaction.TransactionID,
                        GodownItemID = Convert.ToInt32(detailRow["GodownItemID"]),
                        ItemName = detailRow["ItemName"].ToString(),
                        Quantity = Convert.ToDouble(detailRow["Quantity"])
                    });
                }
                
                transactions.Add(transaction);
            }
            
            return transactions;
        }
        
        private static GodownTransaction MapRowToTransaction(DataRow row)
        {
            return new GodownTransaction
            {
                TransactionID = Convert.ToInt32(row["TransactionID"]),
                TransactionNo = row["TransactionNo"].ToString(),
                TransactionDate = Convert.ToDateTime(row["TransactionDate"]),
                TransactionType = row["TransactionType"].ToString(),
                FromGodownID = row["FromGodownID"] != DBNull.Value ? (int?)Convert.ToInt32(row["FromGodownID"]) : null,
                FromGodownName = row["FromGodownName"] != DBNull.Value ? row["FromGodownName"].ToString() : "",
                ToGodownID = row["ToGodownID"] != DBNull.Value ? (int?)Convert.ToInt32(row["ToGodownID"]) : null,
                ToGodownName = row["ToGodownName"] != DBNull.Value ? row["ToGodownName"].ToString() : "",
                TotalQuantity = Convert.ToDouble(row["TotalQuantity"]),
                ReferenceNo = row["ReferenceNo"] != DBNull.Value ? row["ReferenceNo"].ToString() : "",
                CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                CreatedBy = row["CreatedBy"] != DBNull.Value ? (int?)Convert.ToInt32(row["CreatedBy"]) : null
            };
        }
    }
}

