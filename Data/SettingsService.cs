using System;
using System.Data;
using System.Data.OleDb; // Changed from SQLite to OleDb

namespace SaleBillSystem.NET.Data
{
    public class SettingsService
    {
        // Get setting value by key
        public static string GetSetting(string key, string defaultValue = "")
        {
            try
            {
                string sql = "SELECT SettingValue FROM Settings WHERE SettingKey = ?";
                OleDbParameter param = new OleDbParameter("SettingKey", key);
                
                object result = DatabaseManager.ExecuteScalar(sql, param);
                
                return result?.ToString() ?? defaultValue;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        // Set setting value
        public static bool SetSetting(string key, string value, string description = "")
        {
            try
            {
                // Check if setting exists
                string checkSql = "SELECT COUNT(*) FROM Settings WHERE SettingKey = ?";
                OleDbParameter checkParam = new OleDbParameter("SettingKey", key);
                
                int count = Convert.ToInt32(DatabaseManager.ExecuteScalar(checkSql, checkParam));
                
                if (count > 0)
                {
                    // Update existing setting
                    string updateSql = "UPDATE Settings SET SettingValue = ?, Description = ? WHERE SettingKey = ?";
                    OleDbParameter[] updateParams = {
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
                    OleDbParameter[] insertParams = {
                        new OleDbParameter("SettingKey", key),
                        new OleDbParameter("SettingValue", value),
                        new OleDbParameter("Description", description)
                    };
                    
                    DatabaseManager.ExecuteNonQuery(insertSql, insertParams);
                }
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Get interest rate
        public static double GetInterestRate()
        {
            string value = GetSetting("InterestRate", "12.0");
            if (double.TryParse(value, out double rate))
                return rate;
            return 12.0; // Default 12%
        }

        // Set interest rate
        public static bool SetInterestRate(double rate)
        {
            return SetSetting("InterestRate", rate.ToString(), "Annual interest rate percentage for overdue bills");
        }

        // Get discount rate
        public static double GetDiscountRate()
        {
            string value = GetSetting("DiscountRate", "1.0");
            if (double.TryParse(value, out double rate))
                return rate;
            return 1.0; // Default 1%
        }

        // Set discount rate
        public static bool SetDiscountRate(double rate)
        {
            return SetSetting("DiscountRate", rate.ToString(), "Discount rate percentage for early payment");
        }

        // Get company name
        public static string GetCompanyName()
        {
            return GetSetting("CompanyName", "Your Company Name");
        }

        // Set company name
        public static bool SetCompanyName(string name)
        {
            return SetSetting("CompanyName", name, "Company name for reports");
        }

        // Get company address
        public static string GetCompanyAddress()
        {
            return GetSetting("CompanyAddress", "Your Company Address");
        }

        // Set company address
        public static bool SetCompanyAddress(string address)
        {
            return SetSetting("CompanyAddress", address, "Company address for reports");
        }

        // Get database path
        public static string GetDatabasePath()
        {
            return GetSetting("DatabasePath", "");
        }

        // Set database path
        public static bool SetDatabasePath(string path)
        {
            return DatabaseManager.SetDatabasePath(path);
        }

        // Get all settings
        public static DataTable GetAllSettings()
        {
            try
            {
                string sql = "SELECT * FROM Settings ORDER BY SettingKey";
                return DatabaseManager.ExecuteQuery(sql);
            }
            catch (Exception)
            {
                return new DataTable();
            }
        }
    }
} 