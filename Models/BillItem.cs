using System;

namespace SaleBillSystem.NET.Models
{
    public class BillItem
    {
        public int BillDetailID { get; set; }
        public int BillID { get; set; }
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public double Quantity { get; set; }
        public double Rate { get; set; }
        public double Amount { get; set; }
        public double GSTPct { get; set; }
        public double GSTAmount { get; set; }
        public double TotalAmount { get; set; }

        // Calculate values based on quantity and rate
        public void Calculate()
        {
            Amount = Quantity * Rate;
            GSTAmount = Amount * (GSTPct / 100);
            TotalAmount = Amount + GSTAmount;
        }
    }
} 