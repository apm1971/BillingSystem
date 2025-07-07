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

        // Payment status properties
        public PaymentStatus PaymentStatus
        {
            get
            {
                if (PaidAmount <= 0.01)
                    return PaymentStatus.Unpaid;
                else if (PaidAmount >= NetAmount - 0.01)
                    return PaidAmount > NetAmount + 0.01 ? PaymentStatus.Overpaid : PaymentStatus.Paid;
                else
                    return PaymentStatus.Partial;
            }
        }

        public string PaymentStatusText
        {
            get
            {
                return PaymentStatus switch
                {
                    PaymentStatus.Unpaid => "Unpaid",
                    PaymentStatus.Partial => "Partial",
                    PaymentStatus.Paid => "Paid",
                    PaymentStatus.Overpaid => "Overpaid",
                    _ => "Unknown"
                };
            }
        }

        public string PaymentSummary => $"Paid: ₹{PaidAmount:N2} | Balance: ₹{BalanceAmount:N2}";
    }
} 