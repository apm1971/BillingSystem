using System;

namespace SaleBillSystem.NET.Models
{
    public class BillViewModel
    {
        public int BillID { get; set; }
        public string BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal AdditionalCharges { get; set; }
        public decimal TotalAmount => OriginalAmount + AdditionalCharges;
        public decimal BalanceDue { get; set; }

        // Add this new property
        public decimal PaymentAllocation { get; set; } 
    }
}