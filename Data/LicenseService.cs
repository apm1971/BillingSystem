using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Data
{
    public class LicenseService
    {
        private const string LICENSE_FILE = "license.dat";
        private const string ENCRYPTION_KEY = "YourSecretKey123!@#"; // Change this to your own secret key

        public static string GetHardwareId()
        {
            // Get CPU ID
            string cpuId = "";
            try
            {
                ManagementClass mc = new ManagementClass("win32_processor");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    cpuId = mo.Properties["processorID"].Value.ToString();
                    break;
                }
            }
            catch { }

            // Get HDD Serial
            string hddSerial = "";
            try
            {
                ManagementClass mc = new ManagementClass("Win32_LogicalDisk");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if (mo.Properties["DeviceID"].Value.ToString() == "C:")
                    {
                        hddSerial = mo.Properties["VolumeSerialNumber"].Value.ToString();
                        break;
                    }
                }
            }
            catch { }

            // Combine and hash hardware info
            string combined = $"{cpuId}|{hddSerial}";
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                return BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 16);
            }
        }

        public static bool ValidateLicense()
        {
            try
            {
                string licensePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LICENSE_FILE);
                if (!File.Exists(licensePath))
                    return false;

                string encryptedData = File.ReadAllText(licensePath);
                string decryptedData = Decrypt(encryptedData);

                string[] parts = decryptedData.Split('|');
                if (parts.Length != 5)
                    return false;

                License license = new License
                {
                    LicenseKey = parts[0],
                    HardwareId = parts[1],
                    ExpiryDate = DateTime.Parse(parts[2]),
                    CompanyName = parts[3],
                    Email = parts[4]
                };

                // Validate hardware ID
                if (license.HardwareId != GetHardwareId())
                    return false;

                // Validate expiry
                if (license.ExpiryDate < DateTime.Now)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool SaveLicense(License license)
        {
            try
            {
                string data = $"{license.LicenseKey}|{license.HardwareId}|{license.ExpiryDate:yyyy-MM-dd}|{license.CompanyName}|{license.Email}";
                string encrypted = Encrypt(data);
                string licensePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LICENSE_FILE);
                File.WriteAllText(licensePath, encrypted);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string Encrypt(string clearText)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(ENCRYPTION_KEY, 
                    new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        private static string Decrypt(string cipherText)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(ENCRYPTION_KEY,
                    new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    return Encoding.Unicode.GetString(ms.ToArray());
                }
            }
        }
    }
} 