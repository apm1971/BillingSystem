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
            
                // Start the application with the main form
                Application.Run(new MainForm());
            }
        }
    }
}