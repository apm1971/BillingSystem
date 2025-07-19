using System;
using System.Security.Cryptography;
using System.Text;

namespace SaleBillSystem.NET.Models
{
    public class License
    {
        public string LicenseKey { get; set; }
        public string HardwareId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsValid { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
    }
} 