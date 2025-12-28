using System;
using System.Collections.Generic;

namespace SaleBillSystem.NET.Models
{
    public class GodownTransaction
    {
        public int TransactionID { get; set; }
        public string TransactionNo { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } // "Inward", "Outward", "Transfer"
        public int? FromGodownID { get; set; }
        public string FromGodownName { get; set; }
        public int? ToGodownID { get; set; }
        public string ToGodownName { get; set; }
        public double TotalQuantity { get; set; }
        public string ReferenceNo { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        
        public List<GodownTransactionDetail> Details { get; set; }
        
        public GodownTransaction()
        {
            Details = new List<GodownTransactionDetail>();
        }
    }
}

