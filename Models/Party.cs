using System;

namespace SaleBillSystem.NET.Models
{
    public class Party
    {
        public int PartyID { get; set; }
        public string PartyName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public double CreditLimit { get; set; }
        public int CreditDays { get; set; }
        public double OutstandingAmount { get; set; }
        public int? BrokerID { get; set; }
        public string BrokerName { get; set; } = string.Empty;

        public string FullAddress => $"{Address}, {City}";
        public string ContactInfo => $"Phone: {Phone}, Email: {Email}";
        public string BrokerInfo => string.IsNullOrEmpty(BrokerName) ? "No Broker" : $"Broker: {BrokerName}";
    }
} 