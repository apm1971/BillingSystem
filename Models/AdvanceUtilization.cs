using System;

namespace SaleBillSystem.NET.Models
{
    public class AdvanceUtilization
    {
        public int UtilizationID { get; set; }
        public int AdvanceID { get; set; }
        public int PaymentID { get; set; }
        public decimal AmountUsed { get; set; }
        public DateTime UtilizedDate { get; set; }
        public int? PartyID { get; set; }
        public string? PartyName { get; set; }
        public int? BrokerID { get; set; }
        public string? BrokerName { get; set; }
        public int CompanyID { get; set; }
        public DateTime CreatedDate { get; set; }
        // Navigation/Reference properties for convenience
        public string? PaymentReference { get; set; }
        public string? OriginalAdvanceReference { get; set; }
        public decimal? OriginalAdvanceAmount { get; set; }
        public DateTime? OriginalAdvanceDate { get; set; }
    }
}