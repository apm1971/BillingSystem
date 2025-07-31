using System;

namespace SaleBillSystem.NET.Models
{
    public class Item
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public decimal DefaultRate { get; set; }
        public decimal Charges { get; set; }
        public int CompanyID { get; set; }
    }
}