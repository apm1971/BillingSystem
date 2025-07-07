using System;

namespace SaleBillSystem.NET.Models
{
    public class Broker
    {
        public int BrokerID { get; set; }
        public string BrokerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string ContactInfo => $"Phone: {Phone}, Email: {Email}";
        
        // Display name for combo boxes
        public override string ToString() => BrokerName;
    }
} 