using System;

namespace SaleBillSystem.NET.Models
{
    /// <summary>
    /// Represents a single payment event (voucher) from a party.
    /// </summary>
    public class PaymentMaster
    {
        public int PaymentID { get; set; }
        public int PartyID { get; set; }
        public int? BrokerID { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal TotalAmountPaid { get; set; }
        public string PaymentMethod { get; set; }
        public string Reference { get; set; }
        public int CompanyID { get; set; }
        
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
