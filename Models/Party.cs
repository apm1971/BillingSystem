using System;
namespace SaleBillSystem.NET.Models
{
    public class Party
    {
        public int PartyID { get; set; }
        public string PartyName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        
        /// <summary>
        /// The ID of the associated broker. Can be null if no broker is assigned.
        /// </summary>
        public int? BrokerID { get; set; }
        
        /// <summary>
        /// The name of the associated broker.
        /// </summary>
        public string BrokerName { get; set; } = string.Empty;
        
        public int CompanyID { get; set; }
    }
}