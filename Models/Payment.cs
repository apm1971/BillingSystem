using System;
using System.Collections.Generic;

namespace SaleBillSystem.NET.Models
{
    public class Payment
    {
        public int PaymentID { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Today;
        public double PaymentAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public List<PaymentDetail> PaymentDetails { get; set; } = new List<PaymentDetail>();

        // Party information properties (populated by service)
        public string PrimaryPartyName { get; set; } = string.Empty;
        public int UniquePartyCount { get; set; } = 0;
        public string PartyInfo => UniquePartyCount > 1 ? $"{PrimaryPartyName} (+{UniquePartyCount - 1} others)" : PrimaryPartyName;

        // Helper properties
        public string PaymentInfo => $"Payment #{PaymentID} - ₹{PaymentAmount:N2} on {PaymentDate:dd/MM/yyyy}";
        public string MethodInfo => string.IsNullOrEmpty(Reference) ? PaymentMethod : $"{PaymentMethod} ({Reference})";
    }

    public class PaymentDetail
    {
        public int PaymentDetailID { get; set; }
        public int PaymentID { get; set; }
        public int BillID { get; set; }
        public string BillNo { get; set; } = string.Empty;
        public string PartyName { get; set; } = string.Empty;
        public double BillAmount { get; set; }
        public double PreviousPaid { get; set; }
        public double BalanceBefore { get; set; }
        public double AllocatedAmount { get; set; }
        public double BalanceAfter { get; set; }

        // Helper properties
        public string BillInfo => $"{BillNo} - {PartyName}";
        public bool IsFullyPaid => BalanceAfter <= 0.01; // Account for floating point precision
    }

    // Enum for payment methods
    public enum PaymentMethod
    {
        Cash,
        Cheque,
        BankTransfer,
        UPI,
        Card,
        Other
    }

    // Enum for payment status
    public enum PaymentStatus
    {
        Unpaid,
        Partial,
        Paid,
        Overpaid
    }
} 