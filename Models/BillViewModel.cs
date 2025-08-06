using System;

namespace SaleBillSystem.NET.Models
{
    public class BillViewModel
    {
        public int BillID { get; set; }
        public string BillNo { get; set; }
        public string PartyName { get; set; }
        public string BrokerName { get; set; } = string.Empty;
        public DateTime BillDate { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal AdditionalCharges { get; set; }
        public decimal TotalAmount => OriginalAmount + AdditionalCharges;
        public decimal BalanceDue { get; set; }

        // Add this new property
        public decimal PaymentAllocation { get; set; }
        
        /// <summary>
        /// Cheque amount from Firm1
        /// </summary>
        public decimal ChequeAmountFirm1 { get; set; }
        
        /// <summary>
        /// Cheque amount from Firm2
        /// </summary>
        public decimal ChequeAmountFirm2 { get; set; }
    }
}