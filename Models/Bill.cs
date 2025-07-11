using System;
using System.Collections.Generic;

namespace SaleBillSystem.NET.Models
{
    public class Bill
    {
        public int BillID { get; set; }
        public string BillNo { get; set; } = string.Empty;
        public DateTime BillDate { get; set; } = DateTime.Today;
        public DateTime DueDate { get; set; } = DateTime.Today;
        public int PartyID { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public int? BrokerID { get; set; }
        public string BrokerName { get; set; } = string.Empty;
        public double TotalAmount { get; set; }
        public double TotalCharges { get; set; }
        public double NetAmount { get; set; }
        public double PaidAmount { get; set; }
        public double BalanceAmount => NetAmount - PaidAmount;
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

        // Calculate due date based on bill date and credit days
        public void CalculateDueDate(int creditDays)
        {
            DueDate = BillDate.AddDays(creditDays);
        }

        // Get days remaining until due date
        public int DaysUntilDue => (DueDate - DateTime.Today).Days;

        // Check if bill is overdue
        public bool IsOverdue => DateTime.Today > DueDate;

        // Get days overdue
        public int DaysOverdue => IsOverdue ? (DateTime.Today - DueDate).Days : 0;

        // Payment status text for display
        public string PaymentStatusText
        {
            get
            {
                if (BalanceAmount <= 0.01)
                    return "Paid";
                else if (IsOverdue)
                    return $"Overdue ({DaysOverdue} days)";
                else if (DaysUntilDue > 0)
                    return $"Due in {DaysUntilDue} days";
                else
                    return "Due Today";
            }
        }

        // Item count for display
        public int ItemCount => BillItems.Count;
    }
} 