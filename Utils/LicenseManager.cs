using System;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace SaleBillSystem.NET.Utils
{
    public static class LicenseManager
    {
        // IMPORTANT: Change this secret key to something unique and keep it private!
        private const string SECRET_KEY = "SaleBill@2025#SecretKey!xyz$PROTECTED";

        private static readonly string LicenseFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SaleBillSystem");

        private static readonly string LicenseFile = Path.Combine(LicenseFolder, "license.dat");

        /// <summary>
        /// Gets unique hardware fingerprint of this computer
        /// Customer needs to send this to you for activation
        /// </summary>
        public static string GetHardwareId()
        {
            try
            {
                string cpuId = GetCpuId();
                string diskId = GetDiskSerialNumber();
                string motherboardId = GetMotherboardId();

                string combined = $"{cpuId}-{diskId}-{motherboardId}";

                using (var sha = SHA256.Create())
                {
                    var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(combined));
                    // Take first 16 chars and format nicely
                    string hashStr = BitConverter.ToString(hash).Replace("-", "");
                    return FormatHardwareId(hashStr.Substring(0, 16));
                }
            }
            catch
            {
                return "ERROR-HWID-GEN";
            }
        }

        /// <summary>
        /// Formats hardware ID as XXXX-XXXX-XXXX-XXXX for easy reading
        /// </summary>
        private static string FormatHardwareId(string id)
        {
            if (id.Length != 16) return id;
            return $"{id.Substring(0, 4)}-{id.Substring(4, 4)}-{id.Substring(8, 4)}-{id.Substring(12, 4)}";
        }

        /// <summary>
        /// YOU use this to generate license for a customer
        /// Run this on YOUR computer, not on customer's
        /// </summary>
        public static string GenerateLicenseKey(string hardwareId, string customerName)
        {
            // Remove formatting from hardware ID
            string cleanHwid = hardwareId.Replace("-", "").ToUpper();
            string cleanName = customerName.Trim().ToUpper();

            string data = $"{cleanHwid}|{cleanName}|{SECRET_KEY}";

            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(data));
                string hashStr = BitConverter.ToString(hash).Replace("-", "");
                // Return formatted license key
                return FormatLicenseKey(hashStr.Substring(0, 24));
            }
        }

        /// <summary>
        /// Formats license key as XXXXXX-XXXXXX-XXXXXX-XXXXXX
        /// </summary>
        private static string FormatLicenseKey(string key)
        {
            if (key.Length != 24) return key;
            return $"{key.Substring(0, 6)}-{key.Substring(6, 6)}-{key.Substring(12, 6)}-{key.Substring(18, 6)}";
        }

        /// <summary>
        /// Validates if the software is licensed on this computer
        /// </summary>
        public static LicenseStatus ValidateLicense()
        {
            if (!File.Exists(LicenseFile))
            {
                return new LicenseStatus
                {
                    IsValid = false,
                    Message = "Software is not activated. Please enter your license key."
                };
            }

            try
            {
                string[] lines = File.ReadAllLines(LicenseFile);
                if (lines.Length < 2)
                {
                    return new LicenseStatus
                    {
                        IsValid = false,
                        Message = "Invalid license file. Please re-activate."
                    };
                }

                string storedName = Decrypt(lines[0]);
                string storedKey = Decrypt(lines[1]);

                // Generate expected key for this hardware
                string expectedKey = GenerateLicenseKey(GetHardwareId(), storedName);

                if (storedKey.Replace("-", "").ToUpper() == expectedKey.Replace("-", "").ToUpper())
                {
                    return new LicenseStatus
                    {
                        IsValid = true,
                        CustomerName = storedName,
                        Message = $"Licensed to: {storedName}"
                    };
                }
                else
                {
                    return new LicenseStatus
                    {
                        IsValid = false,
                        Message = "License is not valid for this computer. Hardware may have changed."
                    };
                }
            }
            catch (Exception ex)
            {
                return new LicenseStatus
                {
                    IsValid = false,
                    Message = $"Error validating license: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Activates the software with provided license key
        /// </summary>
        public static ActivationResult ActivateLicense(string customerName, string licenseKey)
        {
            if (string.IsNullOrWhiteSpace(customerName))
            {
                return new ActivationResult { Success = false, Message = "Please enter customer name." };
            }

            if (string.IsNullOrWhiteSpace(licenseKey))
            {
                return new ActivationResult { Success = false, Message = "Please enter license key." };
            }

            // Generate expected key for this hardware + customer name
            string expectedKey = GenerateLicenseKey(GetHardwareId(), customerName);

            // Compare keys (ignore formatting)
            string cleanInput = licenseKey.Replace("-", "").Replace(" ", "").ToUpper();
            string cleanExpected = expectedKey.Replace("-", "").ToUpper();

            if (cleanInput == cleanExpected)
            {
                try
                {
                    // Save license file
                    Directory.CreateDirectory(LicenseFolder);
                    File.WriteAllLines(LicenseFile, new[]
                    {
                        Encrypt(customerName.Trim()),
                        Encrypt(licenseKey.Trim())
                    });

                    return new ActivationResult
                    {
                        Success = true,
                        Message = "Software activated successfully! Thank you."
                    };
                }
                catch (Exception ex)
                {
                    return new ActivationResult
                    {
                        Success = false,
                        Message = $"Error saving license: {ex.Message}"
                    };
                }
            }
            else
            {
                return new ActivationResult
                {
                    Success = false,
                    Message = "Invalid license key. Please check and try again."
                };
            }
        }

        /// <summary>
        /// Removes the license file (for testing or deactivation)
        /// </summary>
        public static void RemoveLicense()
        {
            try
            {
                if (File.Exists(LicenseFile))
                {
                    File.Delete(LicenseFile);
                }
            }
            catch { }
        }

        /// <summary>
        /// Simple encryption for license file
        /// </summary>
        private static string Encrypt(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            // Simple XOR with key + Base64
            byte[] keyBytes = Encoding.UTF8.GetBytes(SECRET_KEY);
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(bytes[i] ^ keyBytes[i % keyBytes.Length]);
            }
            return Convert.ToBase64String(bytes);
        }

        private static string Decrypt(string encrypted)
        {
            byte[] bytes = Convert.FromBase64String(encrypted);
            // Simple XOR with key (same as encrypt since XOR is reversible)
            byte[] keyBytes = Encoding.UTF8.GetBytes(SECRET_KEY);
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(bytes[i] ^ keyBytes[i % keyBytes.Length]);
            }
            return Encoding.UTF8.GetString(bytes);
        }

        #region Hardware Detection

        private static string GetCpuId()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    return obj["ProcessorId"]?.ToString() ?? "";
                }
            }
            catch { }
            return "CPU000";
        }

        private static string GetDiskSerialNumber()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive WHERE Index=0");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string serial = obj["SerialNumber"]?.ToString()?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(serial)) return serial;
                }
            }
            catch { }
            return "DISK000";
        }

        private static string GetMotherboardId()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string serial = obj["SerialNumber"]?.ToString()?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(serial) && serial != "To be filled by O.E.M.")
                        return serial;
                }
            }
            catch { }
            return "MB000";
        }

        #endregion
    }

    public class LicenseStatus
    {
        public bool IsValid { get; set; }
        public string CustomerName { get; set; } = "";
        public string Message { get; set; } = "";
    }

    public class ActivationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}

