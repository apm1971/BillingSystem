using System;
using System.Collections.Generic;

namespace SaleBillSystem.NET.Models
{
    public class Bill
    {
        public int BillID { get; set; }
        public string BillNo { get; set; }
        public System.DateTime BillDate { get; set; }
        public int PartyID { get; set; }
        public string PartyName { get; set; } = string.Empty;
        
        /// <summary>
        /// The ID of the broker for this specific bill. Can be null.
        /// </summary>
        public int? BrokerID { get; set; }
        
        /// <summary>
        /// The broker's name, stored at the time of the sale for historical record.
        /// </summary>
        public string BrokerName { get; set; }
        
        public decimal OriginalAmount { get; set; }
        public decimal AdditionalCharges { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public int CompanyID { get; set; }
        public decimal TotalAmount => OriginalAmount + AdditionalCharges;
        /// <summary>
        /// A list of all line items included in this bill.
        /// </summary>
        public List<BillItem> BillItems { get; set; }

        public Bill()
        {
            // Always initialize the list to prevent errors
            BillItems = new List<BillItem>();
        }
    }
}