using System;

namespace SaleBillSystem.NET.Models
{
    public class GodownTransactionDetail
    {
        public int DetailID { get; set; }
        public int TransactionID { get; set; }
        public int GodownItemID { get; set; }
        public string ItemName { get; set; }
        public double Quantity { get; set; }
    }
}

