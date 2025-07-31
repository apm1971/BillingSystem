using System;

namespace SaleBillSystem.NET.Models
{
    /// <summary>
    /// Represents a transaction for display in the Bill Ledger, including a running balance.
    /// </summary>
    public class TransactionViewModel
    {
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public string Description { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal RunningBalance { get; set; }
    }
}