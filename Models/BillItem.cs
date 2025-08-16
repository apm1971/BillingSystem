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
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal Charges { get; set; }
        public decimal SubQuantity { get; set; }  // Number of sub-quantity units (numeric value)
        public string SubQuantityUnit { get; set; } = "";  // Sub-quantity unit description (from ItemMaster)
        public decimal TotalCharges { get; set; }  // Total charges (SubQuantity * Charges)
        public decimal TotalAmount { get; set; }
        public int CompanyID { get; set; }
    }
}