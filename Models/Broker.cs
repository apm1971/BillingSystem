using System;

namespace SaleBillSystem.NET.Models
{
    public class Broker
    {
        public int BrokerID { get; set; }
        public string BrokerName { get; set; }
        public string Phone { get; set; }
        public int CompanyID { get; set; }
        
        // New fields for interest and discount calculations
        public int InterestDays { get; set; }
        public decimal InterestRate { get; set; }
        public int DiscountDays { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal BrokerageRate { get; set; }
    }
}