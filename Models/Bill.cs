using System;
using System.Collections.Generic;

namespace SaleBillSystem.NET.Models
{
    public class Bill
    {
        public int BillID { get; set; }
        public string BillNo { get; set; } = string.Empty;
        public DateTime BillDate { get; set; } = DateTime.Today;
        public int PartyID { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public double TotalAmount { get; set; }
        public double TotalCharges { get; set; }
        public double NetAmount { get; set; }
        public List<BillItem> BillItems { get; set; } = new List<BillItem>();

        // Calculate bill totals from items
        public void CalculateTotals()
        {
            TotalAmount = 0;
            TotalCharges = 0;
            NetAmount = 0;

            foreach (var item in BillItems)
            {
                TotalAmount += item.Amount;
                TotalCharges += item.Charges;
                NetAmount += item.TotalAmount;
            }
        }
    }
} 