using System;

namespace SaleBillSystem.NET.Models
{
    public class Company
    {
        public int CompanyID { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string PrintName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public DateTime FinancialYearStart { get; set; }
        public DateTime FinancialYearEnd { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        // Helper properties
        public string FinancialYearDisplay => $"{FinancialYearStart:dd/MM/yyyy} to {FinancialYearEnd:dd/MM/yyyy}";
        public bool IsDateWithinFinancialYear(DateTime date) => date >= FinancialYearStart && date <= FinancialYearEnd;
        public string FullAddress => string.IsNullOrWhiteSpace(City) ? Address : $"{Address}, {City}";
    }
} 