using System;

namespace SaleBillSystem.NET.Models
{
    public class GodownLedgerViewModel
    {
        public int LedgerID { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionNo { get; set; }
        public string TransactionType { get; set; }
        public int GodownID { get; set; }
        public string GodownName { get; set; }
        public int GodownItemID { get; set; }
        public string ItemName { get; set; }
        public double InwardQty { get; set; }
        public double OutwardQty { get; set; }
        public double BalanceQty { get; set; }
        public string FromGodown { get; set; }
        public string ToGodown { get; set; }
        public string Remarks { get; set; }

        // For display formatting
        public string TransactionDateFormatted => TransactionDate.ToString("dd-MM-yyyy");
        public string InwardDisplay => InwardQty > 0 ? InwardQty.ToString("N2") : "-";
        public string OutwardDisplay => OutwardQty > 0 ? OutwardQty.ToString("N2") : "-";
    }
}
