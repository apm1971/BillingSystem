using System;

namespace SaleBillSystem.NET.Models
{
    public class AdvancePayment
    {
        public int AdvanceID { get; set; }
        public int? PartyID { get; set; }
        public string? PartyName { get; set; }
        public int? BrokerID { get; set; }
        public string? BrokerName { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public decimal ChequeAmountFirm1 { get; set; } = 0;
        public decimal ChequeAmountFirm2 { get; set; } = 0;
        public int CompanyID { get; set; }
        public DateTime CreatedDate { get; set; }
        
        // Utilization tracking properties
        public decimal OriginalAmount { get; set; } = 0; // Store original amount for reference
        public decimal UtilizedAmount { get; set; } = 0; // Total amount used
        public decimal AvailableAmount => Math.Max(0, Amount - UtilizedAmount); // Calculated available amount
    }
}