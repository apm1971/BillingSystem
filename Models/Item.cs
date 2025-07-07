using System;

namespace SaleBillSystem.NET.Models
{
    public class Item
    {
        public int ItemID { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public double Rate { get; set; }
        public double Charges { get; set; }
        public double StockQuantity { get; set; }
    }
} 