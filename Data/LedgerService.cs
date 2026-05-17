using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Windows.Forms;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Data
{
    public static class LedgerService
    {
        public static bool AddTransaction(Transaction transaction, OleDbConnection conn, OleDbTransaction trans)
        {
            try
            {
                // Round the transaction amounts to 2 decimal places for CURRENCY data type
                transaction.DebitAmount = Math.Round(transaction.DebitAmount, 2);
                transaction.CreditAmount = Math.Round(transaction.CreditAmount, 2);
                
                string sql = @"
                    INSERT INTO TransactionLedger 
                    (PartyID, BillID, PaymentID, TransactionDate, TransactionType, Description, DebitAmount, CreditAmount, PaymentMethod, Reference, UserID, CompanyID)
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                var parameters = new OleDbParameter[]
                {
                    new OleDbParameter("PartyID", OleDbType.Integer) { Value = transaction.PartyID },
                    new OleDbParameter("BillID", OleDbType.Integer) { Value = transaction.BillID ?? (object)DBNull.Value },
                    new OleDbParameter("PaymentID", OleDbType.Integer) { Value = transaction.PaymentID ?? (object)DBNull.Value },
                    new OleDbParameter("TransactionDate", OleDbType.Date) { Value = transaction.TransactionDate },
                    new OleDbParameter("TransactionType", OleDbType.VarChar, 50) { Value = transaction.TransactionType ?? (object)DBNull.Value },
                    new OleDbParameter("Description", OleDbType.LongVarChar) { Value = transaction.Description ?? (object)DBNull.Value },
                    new OleDbParameter("DebitAmount", OleDbType.Currency) { Value = transaction.DebitAmount },
                    new OleDbParameter("CreditAmount", OleDbType.Currency) { Value = transaction.CreditAmount },
                    new OleDbParameter("PaymentMethod", OleDbType.VarChar, 50) { Value = transaction.PaymentMethod ?? (object)DBNull.Value },
                    new OleDbParameter("Reference", OleDbType.VarChar, 100) { Value = transaction.Reference ?? (object)DBNull.Value },
                    new OleDbParameter("UserID", OleDbType.Integer) { Value = transaction.UserID ?? (object)DBNull.Value },
                    new OleDbParameter("CompanyID", OleDbType.Integer) { Value = transaction.CompanyID }
                };

                using (var cmd = new OleDbCommand(sql, conn, trans))
                {
                    cmd.Parameters.AddRange(parameters);
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                 MessageBox.Show($"Error adding transaction: {ex.Message}", "Ledger Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        public static List<Transaction> GetTransactionsForBill(int billId)
        {
            var transactions = new List<Transaction>();
            string sql = "SELECT * FROM TransactionLedger WHERE BillID = ? ORDER BY TransactionDate, TransactionID";
            var param = new OleDbParameter("BillID", billId);

            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
                foreach (DataRow row in dt.Rows)
                {
                    transactions.Add(MapRowToTransaction(row));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading transactions for bill: {ex.Message}", "Database Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return transactions;
        }
        
        /// <summary>
        /// Performs the final reconciliation calculation for a bill.
        /// </summary>
        /// <param name="paymentDate">The date the final settlement is occurring.</param>
        /// <returns>A tuple containing the calculated interest, discount, and the final payable amount.</returns>
        /// 
        /// 
        /// 
        public static (decimal interest, decimal discount, decimal finalAmountDue,decimal interestBearingPrincipal) CalculateFinalSettlement(
            Bill bill,
            int interestDays, 
            decimal interestRate, 
            int discountDays,
            decimal discountRate,
            DateTime paymentDate) // Added paymentDate for accuracy
        {
            var transactions = GetTransactionsForBill(bill.BillID);
            DateTime effectiveDueDate = bill.BillDate.AddDays(interestDays);
            decimal currentBalance = BillService.GetBillBalance(bill.BillID);
            decimal earnedDiscount = 0;
            decimal accruedInterest = 0;

            // 1. Calculate Discount
            decimal totalAmountEligibleForDiscount = 0;
            DateTime discountDueDate = bill.BillDate.AddDays(discountDays);
            // Find past payments made on time for discount
            decimal pastEarlyPayments = transactions
                .Where(t => (t.TransactionType == "Payment" || t.TransactionType == "Advance") && t.TransactionDate.Date <= discountDueDate.Date)
                .Sum(t => t.CreditAmount);
            
            totalAmountEligibleForDiscount += pastEarlyPayments;

            // **THE FIX**: If the final payment itself is being made on time, the remaining balance is also eligible for a discount.
            if (paymentDate.Date <= discountDueDate.Date)
            {
                totalAmountEligibleForDiscount += currentBalance;
            }
            
            earnedDiscount = totalAmountEligibleForDiscount * (discountRate / 100m);

            // 2. Calculate Interest
            decimal interestBearingPrincipal = bill.TotalAmount - pastEarlyPayments;
            if (paymentDate.Date > effectiveDueDate.Date)
            {
                // Logic to calculate interest on overdue balances
                DateTime interestStartDate = effectiveDueDate;

                // Loop through late payments to calculate interest in stages
                var latePayments = transactions
                    .Where(t => (t.TransactionType == "Payment" || t.TransactionType == "Advance") && t.TransactionDate.Date > effectiveDueDate.Date)
                    .OrderBy(t => t.TransactionDate);

                foreach (var payment in latePayments)
                {
                    int overdueDays = (payment.TransactionDate.Date - interestStartDate.Date).Days;
                    if (overdueDays > 0)
                    {
                        accruedInterest += interestBearingPrincipal * (interestRate / 100m) * (overdueDays / 365m);
                    }
                    interestBearingPrincipal -= payment.CreditAmount;
                    interestStartDate = payment.TransactionDate;
                }

                // **THE FIX**: Calculate interest on the final remaining balance up to the actual payment date
                int finalOverdueDays = (paymentDate.Date - interestStartDate.Date).Days;
                if (finalOverdueDays > 0 && interestBearingPrincipal > 0)
                {
                    accruedInterest += interestBearingPrincipal * (interestRate / 100m) * (finalOverdueDays / 365m);
                }
            }
            
            // 3. Calculate Final Amount
            decimal finalAmountDue = currentBalance - earnedDiscount + accruedInterest;
            
            return (Math.Round(accruedInterest, 0), Math.Round(earnedDiscount, 0), Math.Round(finalAmountDue, 0), interestBearingPrincipal);
        }

        public static (decimal interest, decimal discount, decimal finalAmountDue,DateTime InterestStartDate,decimal brokerage,decimal interestBearingPrincipal) CalculateSettlementTillNow(
            Bill bill,
            int interestDays, 
            decimal interestRate, 
            int discountDays,
            decimal discountRate,
            decimal brokerageRate
) // Added paymentDate for accuracy
        {
            var transactions = GetTransactionsForBill(bill.BillID);
            DateTime effectiveDueDate = bill.BillDate.AddDays(interestDays);
            decimal currentBalance = BillService.GetBillBalance(bill.BillID) - (bill.TotalAmount*brokerageRate/100);
            decimal earnedDiscount = 0;
            decimal accruedInterest = 0;

            // 1. Calculate Discount
            decimal totalAmountEligibleForDiscount = 0;
            DateTime discountDueDate = bill.BillDate.AddDays(discountDays);
            // Find past payments made on time for discount
            decimal pastEarlyPayments = transactions
                .Where(t => (t.TransactionType == "Payment" || t.TransactionType == "Advance") && t.TransactionDate.Date <= discountDueDate.Date)
                .Sum(t => t.CreditAmount);
            
            totalAmountEligibleForDiscount += pastEarlyPayments;

           
            
            earnedDiscount = totalAmountEligibleForDiscount * (discountRate / 100m);

            // 2. Calculate Interest
            
                // Logic to calculate interest on overdue balances
                decimal interestBearingPrincipal = bill.TotalAmount - pastEarlyPayments;
                DateTime interestStartDate = effectiveDueDate;

                // Loop through late payments to calculate interest in stages
                var latePayments = transactions
                    .Where(t => (t.TransactionType == "Payment" || t.TransactionType == "Advance") && t.TransactionDate.Date > effectiveDueDate.Date)
                    .OrderBy(t => t.TransactionDate);

                foreach (var payment in latePayments)
                {
                    int overdueDays = (payment.TransactionDate.Date - interestStartDate.Date).Days;
                    if (overdueDays > 0)
                    {
                        accruedInterest += interestBearingPrincipal * (interestRate / 100m) * (overdueDays / 365m);
                    }
                    interestBearingPrincipal -= payment.CreditAmount;
                    interestStartDate = payment.TransactionDate;
                }
                
            
            // 3. Calculate Final Amount
            decimal finalAmountDue = currentBalance - earnedDiscount + accruedInterest;
            
            return (Math.Round(accruedInterest, 0), Math.Round(earnedDiscount, 0), Math.Round(finalAmountDue, 0),interestStartDate, bill.TotalAmount*brokerageRate/100,interestBearingPrincipal);
        }
        private static Transaction MapRowToTransaction(DataRow row)
        {
            return new Transaction
            {
                TransactionID = Convert.ToInt32(row["TransactionID"]),
                PartyID = Convert.ToInt32(row["PartyID"]),
                BillID = row["BillID"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["BillID"]),
                TransactionDate = Convert.ToDateTime(row["TransactionDate"]),
                TransactionType = row["TransactionType"].ToString(),
                Description = row["Description"].ToString(),
                DebitAmount = Convert.ToDecimal(row["DebitAmount"]),
                CreditAmount = Convert.ToDecimal(row["CreditAmount"]),
                PaymentMethod = row["PaymentMethod"].ToString(),
                Reference = row["Reference"].ToString(),
                UserID = row["UserID"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["UserID"]),
                CompanyID = Convert.ToInt32(row["CompanyID"])
            };
        }

        public static decimal CalculateBrokerage(Bill bill, decimal brokerageRate)
        {
            return Math.Round(bill.TotalAmount * (brokerageRate / 100m), 0);
        }

        public static decimal GetDueAmount(int billId)
    {
        string sql = "SELECT * FROM TransactionLedger WHERE BillID = ? ORDER BY TransactionDate, TransactionID";
        var param = new OleDbParameter("BillID", billId);
        decimal sum = 0;
        try
        {
            DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
            foreach (DataRow row in dt.Rows)
            {
                    sum += (Convert.ToDecimal(row["DebitAmount"]) - Convert.ToDecimal(row["CreditAmount"])) ;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading transactions for bill: {ex.Message}", "Database Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        return Math.Round(sum);
    }

/// <summary>
/// Gets balances for all bills at once using a single database query.
/// This is much more efficient than calling GetDueAmount for each bill individually.
/// </summary>
/// <returns>Dictionary with BillID as key and Balance as value</returns>
public static Dictionary<int, decimal> GetAllBillBalances()
{
    var balances = new Dictionary<int, decimal>();
    
    try
    {
        // Query that calculates the balance for each bill in a single database operation
        string sql = @"SELECT BillID, SUM(DebitAmount) - SUM(CreditAmount) AS Balance 
                      FROM TransactionLedger 
                      WHERE BillID IS NOT NULL 
                      GROUP BY BillID";
        
        DataTable dt = DatabaseManager.ExecuteQuery(sql);
        foreach (DataRow row in dt.Rows)
        {
            int billId = Convert.ToInt32(row["BillID"]);
            decimal balance = Convert.ToDecimal(row["Balance"]);
            balances[billId] = Math.Round(balance);
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error loading bill balances: {ex.Message}", "Database Error", 
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    
    return balances;
}

/// <summary>
/// Gets all transactions for a list of bill IDs in a single database query.
/// This is much more efficient than calling GetTransactionsForBill for each bill individually.
/// </summary>
/// <param name="billIds">List of bill IDs to get transactions for</param>
/// <returns>Dictionary with BillID as key and list of transactions as value</returns>
public static Dictionary<int, List<Transaction>> GetTransactionsForBills(List<int> billIds)
{
    var result = new Dictionary<int, List<Transaction>>();
    
    // Initialize empty lists for all requested bill IDs
    foreach (var billId in billIds)
    {
        result[billId] = new List<Transaction>();
    }
    
    if (billIds.Count == 0) return result;
    
    try
    {
        // Build a query with all bill IDs in an IN clause
        string billIdList = string.Join(",", billIds);
        string sql = $"SELECT * FROM TransactionLedger WHERE BillID IN ({billIdList}) ORDER BY BillID, TransactionDate, TransactionID";
        
        DataTable dt = DatabaseManager.ExecuteQuery(sql);
        foreach (DataRow row in dt.Rows)
        {
            var transaction = MapRowToTransaction(row);
            if (transaction.BillID.HasValue)
            {
                int billId = transaction.BillID.Value;
                if (result.ContainsKey(billId))
                {
                    result[billId].Add(transaction);
                }
            }
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error loading transactions for bills: {ex.Message}", "Database Error", 
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    
    return result;
}
/// <summary>
/// Gets the settlement date for a bill by finding when the balance first reached zero.
/// Returns null if the bill has not been fully settled.
/// </summary>
public static DateTime? GetSettlementDate(int billId)
{
    try
    {
        var transactions = GetTransactionsForBill(billId);
        if (transactions.Count == 0) return null;

        decimal runningBalance = 0;
        DateTime? settlementDate = null;

        foreach (var txn in transactions)
        {
            runningBalance += txn.DebitAmount - txn.CreditAmount;
            
            if (runningBalance <= 0)
            {
                // Bill became fully settled at this transaction's date
                settlementDate = txn.TransactionDate;
            }
            else
            {
                // Balance went positive again (e.g. new debit), so no longer settled
                settlementDate = null;
            }
        }

        return settlementDate;
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error getting settlement date: {ex.Message}", "Database Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
        return null;
    }
}
   }
}

