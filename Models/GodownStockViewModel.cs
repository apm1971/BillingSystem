using System;

namespace SaleBillSystem.NET.Models
{
    public class GodownStockViewModel
    {
        public int GodownID { get; set; }
        public string GodownName { get; set; }
        public string GodownShortName { get; set; }
        public int GodownItemID { get; set; }
        public string ItemName { get; set; }
        public double OpeningStock { get; set; }

        // Direct inward (non-transfer)
        public double DirectInward { get; set; }
        // Transfer in (from other godowns)
        public double TransferIn { get; set; }
        // Total inward (Direct + Transfer In)
        public double TotalInward { get { return DirectInward + TransferIn; } }

        // Direct outward (non-transfer)
        public double DirectOutward { get; set; }
        // Transfer out (to other godowns)
        public double TransferOut { get; set; }
        // Total outward (Direct + Transfer Out)
        public double TotalOutward { get { return DirectOutward + TransferOut; } }

        public double CurrentStock { get; set; }
    }
}

