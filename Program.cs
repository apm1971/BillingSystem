using System;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Forms;
using SaleBillSystem.NET.Models;

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

            // Update database schema to add any missing columns
            DatabaseManager.UpdateDatabaseSchema();

            // Start the application with the main form, which will handle the login process internally.
            Application.Run(new MainForm());
        }
    }
}