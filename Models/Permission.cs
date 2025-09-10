using System;

namespace SaleBillSystem.NET.Models
{
    /// <summary>
    /// Defines permission levels for different operations
    /// </summary>
    public enum PermissionLevel
    {
        None = 0,
        Read = 1,
        Create = 2,
        Update = 3,
        Delete = 4,
        Admin = 5
    }

    /// <summary>
    /// Defines different modules/areas of the application
    /// </summary>
    public enum ModuleType
    {
        Masters,    // Company, Broker, Party, Item masters
        Bills,      // Sale bills
        Payments,   // Payment entries
        Reports,    // All reports
        Users,      // User management
        Settings    // System settings
    }

    /// <summary>
    /// Represents user permissions for a specific module
    /// </summary>
    public class Permission
    {
        public int PermissionID { get; set; }
        public int UserID { get; set; }
        public ModuleType Module { get; set; }
        public PermissionLevel Level { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedByUserID { get; set; }
    }
}