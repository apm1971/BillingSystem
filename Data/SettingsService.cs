using System;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;

namespace SaleBillSystem.NET.Data
{
    /// <summary>
    /// Manages reading and writing application settings to the database.
    /// </summary>
    public static class SettingsService
    {
        #region == Generic Core Methods ==

        /// <summary>
        /// Gets a setting's value by its key.
        /// </summary>
        /// <param name="key">The unique key of the setting.</param>
        /// <param name="defaultValue">The value to return if the key is not found.</param>
        /// <returns>The setting value as a string.</returns>
        public static string GetSetting(string key, string defaultValue = "")
        {
            try
            {
                string sql = "SELECT SettingValue FROM Settings WHERE SettingKey = ?";
                var param = new OleDbParameter("SettingKey", key);
                
                object result = DatabaseManager.ExecuteScalar(sql, param);
                
                return result?.ToString() ?? defaultValue;
            }
            catch (Exception)
            {
                // In case of error (e.g., table not ready), return the safe default.
                return defaultValue;
            }
        }

        /// <summary>
        /// Sets a setting's value. Creates the setting if it doesn't exist, otherwise updates it.
        /// </summary>
        /// <param name="key">The unique key of the setting.</param>
        /// <param name="value">The value to save.</param>
        /// <param name="description">An optional description of the setting.</param>
        public static bool SetSetting(string key, string value, string description = "")
        {
            try
            {
                string checkSql = "SELECT COUNT(*) FROM Settings WHERE SettingKey = ?";
                var checkParam = new OleDbParameter("SettingKey", key);
                int count = Convert.ToInt32(DatabaseManager.ExecuteScalar(checkSql, checkParam));
                
                if (count > 0)
                {
                    // Update existing setting
                    string updateSql = "UPDATE Settings SET SettingValue = ?, Description = ? WHERE SettingKey = ?";
                    var updateParams = new OleDbParameter[]
                    {
                        new OleDbParameter("SettingValue", value),
                        new OleDbParameter("Description", description),
                        new OleDbParameter("SettingKey", key)
                    };
                    DatabaseManager.ExecuteNonQuery(updateSql, updateParams);
                }
                else
                {
                    // Insert new setting
                    string insertSql = "INSERT INTO Settings (SettingKey, SettingValue, Description) VALUES (?, ?, ?)";
                    var insertParams = new OleDbParameter[]
                    {
                        new OleDbParameter("SettingKey", key),
                        new OleDbParameter("SettingValue", value),
                        new OleDbParameter("Description", description)
                    };
                    DatabaseManager.ExecuteNonQuery(insertSql, insertParams);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving setting '{key}': {ex.Message}", "Database Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        
        #endregion

        #region == Specific Setting Helpers ==

        public static int GetDefaultCreditDays()
        {
            string value = GetSetting("DefaultCreditDays", "30");
            return int.TryParse(value, out int days) ? days : 30;
        }

        public static bool SetDefaultCreditDays(int days)
        {
            return SetSetting("DefaultCreditDays", days.ToString(), "Default credit days to show on payment screen");
        }

        public static int GetDefaultInterestDays()
        {
            string value = GetSetting("DefaultInterestDays", "30");
            return int.TryParse(value, out int days) ? days : 30;
        }

        public static bool SetDefaultInterestDays(int days)
        {
            return SetSetting("DefaultInterestDays", days.ToString(), "Default interest days to show on payment screen");
        }

        public static int GetDefaultDiscountDays()
        {
            string value = GetSetting("DefaultDiscountDays", "10");
            return int.TryParse(value, out int days) ? days : 10;
        }

        public static bool SetDefaultDiscountDays(int days)
        {
            return SetSetting("DefaultDiscountDays", days.ToString(), "Default discount days to show on payment screen");
        }

        public static decimal GetDefaultInterestRate()
        {
            string value = GetSetting("DefaultInterestRate", "18.0");
            return decimal.TryParse(value, out decimal rate) ? rate : 18.0m;
        }

        public static bool SetDefaultInterestRate(decimal rate)
        {
            return SetSetting("DefaultInterestRate", rate.ToString("F2"), "Default annual interest rate (%) to show on payment screen");
        }

        public static decimal GetDefaultDiscountRate()
        {
            string value = GetSetting("DefaultDiscountRate", "1.0");
            return decimal.TryParse(value, out decimal rate) ? rate : 1.0m;
        }

        public static bool SetDefaultDiscountRate(decimal rate)
        {
            return SetSetting("DefaultDiscountRate", rate.ToString("F2"), "Default discount rate (%) for early payments to show on payment screen");
        }

        public static decimal GetDefaultBrokerageRate()
        {
            string value = GetSetting("DefaultBrokerageRate", "0.0");
            return decimal.TryParse(value, out decimal rate) ? rate : 0.0m;
        }

        public static bool SetDefaultBrokerageRate(decimal rate)
        {
            return SetSetting("DefaultBrokerageRate", rate.ToString("F2"), "Default brokerage rate (%) to show on payment screen");
        }

        #endregion
    }
}