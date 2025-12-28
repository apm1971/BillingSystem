using System;

namespace SaleBillSystem.NET.Models
{
    public class GodownOpeningStock
    {
        public int OpeningStockID { get; set; }
        public int GodownID { get; set; }
        public string GodownName { get; set; }
        public int GodownItemID { get; set; }
        public string ItemName { get; set; }
        public double Quantity { get; set; }
        public DateTime AsOnDate { get; set; }
    }
}

