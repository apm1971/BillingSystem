using System;
using System.Windows.Forms;
using SaleBillSystem.NET.Forms;
using SaleBillSystem.NET.Models;
using SaleBillSystem.NET.Data;

namespace SaleBillSystem.NET
{
    static class Program
    {
        public const string APP_NAME = "Sale Bill System";
        public static User CurrentUser { get; set; }
        public static Company ActiveCompany { get; set; }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Check license
            if (!LicenseService.ValidateLicense())
            {
                using (var licenseForm = new LicenseForm())
                {
                    if (licenseForm.ShowDialog() != DialogResult.OK)
                    {
                        MessageBox.Show("Application requires a valid license to run.", 
                            "License Required", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }

                    // Validate license again after activation
                    if (!LicenseService.ValidateLicense())
                    {
                        MessageBox.Show("Invalid license. Application will now exit.", 
                            "License Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }

            // Initialize database
            if (!DatabaseManager.Initialize())
            {
                MessageBox.Show("Failed to initialize database. Application will now exit.", 
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Show login form
            using (var loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
            }

            Application.Run(new MainForm());
        }
    }
}