using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Data
{
    /// <summary>
    /// Manages user permissions and access control
    /// </summary>
    public static class PermissionManager
    {
        /// <summary>
        /// Current logged-in user
        /// </summary>
        public static User CurrentUser { get; set; }

        /// <summary>
        /// Checks if the current user has permission to perform an operation
        /// </summary>
        /// <param name="module">The module being accessed</param>
        /// <param name="requiredLevel">The minimum permission level required</param>
        /// <returns>True if user has permission, false otherwise</returns>
        public static bool HasPermission(ModuleType module, PermissionLevel requiredLevel)
        {
            if (CurrentUser == null)
            {
                ShowAccessDeniedMessage("No user logged in.");
                return false;
            }

            // Admin users have full access to everything
            if (CurrentUser.IsAdmin)
            {
                return true;
            }

            // For non-admin users, check specific permissions
            // For now, we'll implement basic rules:
            // - Non-admin users can Read, Create, Update but NOT Delete
            // - Only Admin can Delete anything
            if (requiredLevel == PermissionLevel.Delete)
            {
                ShowAccessDeniedMessage($"Only administrators can delete {module.ToString().ToLower()}.");
                return false;
            }

            // Non-admin users can perform other operations
            return true;
        }

        /// <summary>
        /// Checks if current user can delete items from a specific module
        /// </summary>
        /// <param name="module">The module to check</param>
        /// <returns>True if user can delete, false otherwise</returns>
        public static bool CanDelete(ModuleType module)
        {
            return HasPermission(module, PermissionLevel.Delete);
        }

        /// <summary>
        /// Checks if current user can create items in a specific module
        /// </summary>
        /// <param name="module">The module to check</param>
        /// <returns>True if user can create, false otherwise</returns>
        public static bool CanCreate(ModuleType module)
        {
            return HasPermission(module, PermissionLevel.Create);
        }

        /// <summary>
        /// Checks if current user can update items in a specific module
        /// </summary>
        /// <param name="module">The module to check</param>
        /// <returns>True if user can update, false otherwise</returns>
        public static bool CanUpdate(ModuleType module)
        {
            return HasPermission(module, PermissionLevel.Update);
        }

        /// <summary>
        /// Checks if current user can read items from a specific module
        /// </summary>
        /// <param name="module">The module to check</param>
        /// <returns>True if user can read, false otherwise</returns>
        public static bool CanRead(ModuleType module)
        {
            return HasPermission(module, PermissionLevel.Read);
        }

        /// <summary>
        /// Shows a standardized access denied message
        /// </summary>
        /// <param name="message">Custom message to display</param>
        private static void ShowAccessDeniedMessage(string message)
        {
            MessageBox.Show(
                $"Access Denied: {message}\n\nContact your administrator if you need additional permissions.",
                "Access Denied",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Initializes permissions for the current user session
        /// </summary>
        /// <param name="user">The logged-in user</param>
        public static void InitializeUserSession(User user)
        {
            CurrentUser = user;
        }

        /// <summary>
        /// Clears the current user session
        /// </summary>
        public static void ClearUserSession()
        {
            CurrentUser = null;
        }

        /// <summary>
        /// Gets a user-friendly permission level description
        /// </summary>
        /// <param name="level">The permission level</param>
        /// <returns>Description of the permission level</returns>
        public static string GetPermissionDescription(PermissionLevel level)
        {
            switch (level)
            {
                case PermissionLevel.None:
                    return "No Access";
                case PermissionLevel.Read:
                    return "View Only";
                case PermissionLevel.Create:
                    return "View and Create";
                case PermissionLevel.Update:
                    return "View, Create and Edit";
                case PermissionLevel.Delete:
                    return "Full Access (including Delete)";
                case PermissionLevel.Admin:
                    return "Administrator";
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// Validates if a delete operation can proceed and shows appropriate message if not
        /// </summary>
        /// <param name="module">The module being accessed</param>
        /// <param name="itemName">Name of the item being deleted (for better error messages)</param>
        /// <returns>True if delete can proceed, false otherwise</returns>
        public static bool ValidateDeleteOperation(ModuleType module, string itemName = "item")
        {
            if (!CanDelete(module))
            {
                ShowAccessDeniedMessage($"You do not have permission to delete {itemName} from {module.ToString().ToLower()}.");
                return false;
            }
            return true;
        }
    }
}