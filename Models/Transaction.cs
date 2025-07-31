using System;
namespace SaleBillSystem.NET.Models
{
    public class Transaction
    {
        public int TransactionID { get; set; }
        public int PartyID { get; set; }

        /// <summary>
        /// The ID of the bill this transaction applies to. Can be null for on-account payments.
        /// </summary>
        public int? BillID { get; set; }
        public int? PaymentID { get; set; }
        public System.DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public string Description { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string Reference { get; set; }
        
        /// <summary>
        /// The ID of the user who created the transaction. Can be null.
        /// </summary>
        public int? UserID { get; set; }
        
        public int CompanyID { get; set; }
    }
}