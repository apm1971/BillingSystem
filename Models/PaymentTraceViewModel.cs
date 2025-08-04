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
        public string TransactionType { get; set; } = string.Empty;
        
        // Calculated properties
        public decimal AppliedAmount
        {
            get
            {
                switch (TransactionType)
                {
                    case "Payment":
                        return CreditAmount;
                    case "Interest":
                        return DebitAmount;
                    case "Discount":
                        return -CreditAmount; // Negative for discount
                    case "Brokerage":
                        return -CreditAmount; // Negative for brokerage (similar to discount)
                    default:
                        return CreditAmount > 0 ? CreditAmount : DebitAmount;
                }
            }
        }
    }
} 