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
    }
}