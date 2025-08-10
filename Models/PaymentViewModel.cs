using System;

namespace SaleBillSystem.NET.Models
{
    /// <summary>
    /// Represents a payment for display in UI grids, including the Party's name.
    /// </summary>
    public class PaymentViewModel
    {
        public int PaymentID { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal TotalAmountPaid { get; set; }
        public string PaymentMethod { get; set; }
        public string Reference { get; set; }
        public int PartyID { get; set; }
        public string PartyName { get; set; }
        public int? BrokerID { get; set; }
        public string BrokerName { get; set; } = string.Empty;
        
        /// <summary>
        /// Cheque amount from Firm1 (only applicable when PaymentMethod is "Cheque")
        /// </summary>
        public decimal ChequeAmountFirm1 { get; set; }
        
        /// <summary>
        /// Cheque amount from Firm2 (only applicable when PaymentMethod is "Cheque")
        /// </summary>
        public decimal ChequeAmountFirm2 { get; set; }
    }
}
