using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace SaleBillSystem.NET.Utils
{
    /// <summary>
    /// Manages application settings stored in a local JSON file.
    /// Settings are stored outside the database to allow path configuration before database access.
    /// </summary>
    public static class SettingsManager
    {
        private static readonly string SettingsFileName = "appsettings.json";
        private static readonly string SettingsFilePath;
        private static Dictionary<string, string> _settings;

        static SettingsManager()
        {
            // Store settings in the application directory
            SettingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFileName);
            LoadSettings();
        }

        /// <summary>
        /// Loads settings from the JSON file
        /// </summary>
        private static void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    _settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                }
                else
                {
                    _settings = new Dictionary<string, string>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
                _settings = new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Saves settings to the JSON file
        /// </summary>
        private static void SaveSettings()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_settings, options);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
                throw new Exception($"Could not save settings: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a setting value by key
        /// </summary>
        public static string GetSetting(string key, string defaultValue = null)
        {
            if (_settings == null)
                LoadSettings();

            return _settings.TryGetValue(key, out string value) ? value : defaultValue;
        }

        /// <summary>
        /// Sets a setting value
        /// </summary>
        public static void SetSetting(string key, string value)
        {
            if (_settings == null)
                LoadSettings();

            _settings[key] = value;
            SaveSettings();
        }

        /// <summary>
        /// Removes a setting
        /// </summary>
        public static void RemoveSetting(string key)
        {
            if (_settings == null)
                LoadSettings();

            if (_settings.ContainsKey(key))
            {
                _settings.Remove(key);
                SaveSettings();
            }
        }

        /// <summary>
        /// Gets the configured database path, or null if using default
        /// </summary>
        public static string DatabasePath
        {
            get => GetSetting("DatabasePath");
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    RemoveSetting("DatabasePath");
                else
                    SetSetting("DatabasePath", value);
            }
        }

        /// <summary>
        /// Gets the default database path in the application directory
        /// </summary>
        public static string DefaultDatabasePath
        {
            get => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "SaleSystem.accdb");
        }

        /// <summary>
        /// Gets the current active database path (custom or default)
        /// </summary>
        public static string ActiveDatabasePath
        {
            get
            {
                string customPath = DatabasePath;
                if (!string.IsNullOrEmpty(customPath) && File.Exists(customPath))
                    return customPath;
                return DefaultDatabasePath;
            }
        }

        /// <summary>
        /// Checks if a custom database path is configured
        /// </summary>
        public static bool HasCustomDatabasePath
        {
            get => !string.IsNullOrEmpty(DatabasePath);
        }

        /// <summary>
        /// Gets the path to the settings file
        /// </summary>
        public static string GetSettingsFilePath()
        {
            return SettingsFilePath;
        }
    }
}
