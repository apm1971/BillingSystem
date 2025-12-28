/*
 * ============================================
 * LICENSE GENERATOR TOOL - FOR DEVELOPER ONLY
 * ============================================
 * 
 * DO NOT DISTRIBUTE THIS FILE!
 * Keep this tool only on YOUR computer.
 * 
 * How to Run:
 * -----------
 * Option 1: Copy this file to a new Console App project and run it
 * Option 2: Use C# Interactive in Visual Studio (View > Other Windows > C# Interactive)
 * Option 3: Use dotnet-script: dotnet tool install -g dotnet-script
 *           Then run: dotnet script LicenseGeneratorTool.cs
 * 
 * ============================================
 */

using System;
using System.Security.Cryptography;
using System.Text;

class LicenseGeneratorTool
{
    // IMPORTANT: This MUST match the SECRET_KEY in your main application's LicenseManager.cs
    private const string SECRET_KEY = "SaleBill@2025#SecretKey!xyz$PROTECTED";

    static void Main(string[] args)
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║       LICENSE KEY GENERATOR - Sale Bill System             ║");
        Console.WriteLine("║            FOR DEVELOPER USE ONLY                          ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        while (true)
        {
            Console.WriteLine("─────────────────────────────────────────────────────────────");
            Console.Write("Enter Customer's Hardware ID (or 'exit' to quit): ");
            string hardwareId = Console.ReadLine()?.Trim() ?? "";

            if (hardwareId.ToLower() == "exit")
            {
                Console.WriteLine("Goodbye!");
                break;
            }

            if (string.IsNullOrWhiteSpace(hardwareId))
            {
                Console.WriteLine("Error: Hardware ID cannot be empty.\n");
                continue;
            }

            // Validate Hardware ID format
            string cleanHwid = hardwareId.Replace("-", "").Replace(" ", "");
            if (cleanHwid.Length != 16)
            {
                Console.WriteLine("Warning: Hardware ID should be 16 characters (XXXX-XXXX-XXXX-XXXX)");
                Console.Write("Continue anyway? (y/n): ");
                if (Console.ReadLine()?.ToLower() != "y")
                {
                    continue;
                }
            }

            Console.Write("Enter Customer/Company Name: ");
            string customerName = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(customerName))
            {
                Console.WriteLine("Error: Customer name cannot be empty.\n");
                continue;
            }

            // Generate license key
            string licenseKey = GenerateLicenseKey(hardwareId, customerName);

            Console.WriteLine();
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                  LICENSE KEY GENERATED                     ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║  Customer: {customerName.PadRight(46)}║");
            Console.WriteLine($"║  Hardware ID: {hardwareId.PadRight(43)}║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║  LICENSE KEY: {licenseKey.PadRight(43)}║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("Send this license key to your customer!");
            Console.WriteLine();
        }
    }

    static string GenerateLicenseKey(string hardwareId, string customerName)
    {
        // Remove formatting from hardware ID
        string cleanHwid = hardwareId.Replace("-", "").Replace(" ", "").ToUpper();
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

    static string FormatLicenseKey(string key)
    {
        if (key.Length != 24) return key;
        return $"{key.Substring(0, 6)}-{key.Substring(6, 6)}-{key.Substring(12, 6)}-{key.Substring(18, 6)}";
    }
}

