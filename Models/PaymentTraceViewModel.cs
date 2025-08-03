using System;

namespace SaleBillSystem.NET.Models
{
    public class PaymentTraceViewModel
    {
        public int TransactionID { get; set; }
        public int? BillID { get; set; }
        public string BillNo { get; set; } = string.Empty;
        public DateTime? BillDate { get; set; }
        public decimal BillAmount { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        
        // Calculated properties
        public decimal AppliedAmount => CreditAmount > 0 ? CreditAmount : DebitAmount;
        public string TransactionType => CreditAmount > 0 ? "Payment" : "Bill";
    }
} 