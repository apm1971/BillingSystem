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
                string sql = @"
                    INSERT INTO TransactionLedger 
                    (PartyID, BillID, PaymentID, TransactionDate, TransactionType, Description, DebitAmount, CreditAmount, PaymentMethod, Reference, UserID, CompanyID)
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                var parameters = new OleDbParameter[]
                {
                    new OleDbParameter("PartyID", transaction.PartyID),
                    new OleDbParameter("BillID", transaction.BillID ?? (object)DBNull.Value),
                    new OleDbParameter("PaymentID", transaction.PaymentID ?? (object)DBNull.Value),
                    new OleDbParameter("TransactionDate", transaction.TransactionDate),
                    new OleDbParameter("TransactionType", transaction.TransactionType),
                    new OleDbParameter("Description", transaction.Description ?? (object)DBNull.Value),
                    new OleDbParameter("DebitAmount", transaction.DebitAmount),
                    new OleDbParameter("CreditAmount", transaction.CreditAmount),
                    new OleDbParameter("PaymentMethod", transaction.PaymentMethod ?? (object)DBNull.Value),
                    new OleDbParameter("Reference", transaction.Reference ?? (object)DBNull.Value),
                    new OleDbParameter("UserID", transaction.UserID ?? (object)DBNull.Value),
                    new OleDbParameter("CompanyID", transaction.CompanyID)
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
        public static (decimal interest, decimal discount, decimal finalAmountDue) CalculateFinalSettlement(
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
                .Where(t => t.TransactionType == "Payment" && t.TransactionDate.Date <= discountDueDate.Date)
                .Sum(t => t.CreditAmount);
            
            totalAmountEligibleForDiscount += pastEarlyPayments;

            // **THE FIX**: If the final payment itself is being made on time, the remaining balance is also eligible for a discount.
            if (paymentDate.Date <= discountDueDate.Date)
            {
                totalAmountEligibleForDiscount += currentBalance;
            }
            
            earnedDiscount = totalAmountEligibleForDiscount * (discountRate / 100m);

            // 2. Calculate Interest
            if (paymentDate.Date > effectiveDueDate.Date)
            {
                // Logic to calculate interest on overdue balances
                decimal interestBearingPrincipal = bill.TotalAmount - pastEarlyPayments;
                DateTime interestStartDate = effectiveDueDate;

                // Loop through late payments to calculate interest in stages
                var latePayments = transactions
                    .Where(t => t.TransactionType == "Payment" && t.TransactionDate.Date > effectiveDueDate.Date)
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
            
            return (Math.Round(accruedInterest, 2), Math.Round(earnedDiscount, 2), Math.Round(finalAmountDue, 2));
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
            return bill.TotalAmount * (brokerageRate / 100m);
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
        return sum;
    }
   }
}

