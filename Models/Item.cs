using System;

namespace SaleBillSystem.NET.Models
{
    public class Item
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public decimal DefaultRate { get; set; }
        public decimal Charges { get; set; }  // Now represents charge per sub-quantity unit (per bag, per box etc.)
        public int CompanyID { get; set; }
        public string SubQuantity { get; set; } = "";  // Sub-quantity unit type (e.g., "bag", "box")
    }
}