using System;
using System.Data;
using System.Data.SQLite;

namespace SaleBillSystem.NET.Data
{
    public class SettingsService
    {
        // Get setting value by key
        public static string GetSetting(string key, string defaultValue = "")
        {
            try
            {
                string sql = "SELECT SettingValue FROM Settings WHERE SettingKey = @SettingKey";
                SQLiteParameter param = new SQLiteParameter("@SettingKey", key);
                
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
                string sql = @"INSERT OR REPLACE INTO Settings (SettingKey, SettingValue, Description) 
                              VALUES (@SettingKey, @SettingValue, @Description)";
                
                SQLiteParameter[] parameters = {
                    new SQLiteParameter("@SettingKey", key),
                    new SQLiteParameter("@SettingValue", value),
                    new SQLiteParameter("@Description", description)
                };
                
                int result = DatabaseManager.ExecuteNonQuery(sql, parameters);
                return result > 0;
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