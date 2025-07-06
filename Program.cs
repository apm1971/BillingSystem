using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Forms;

namespace SaleBillSystem.NET
{
    internal static class Program
    {
        // Application-wide constants
        public const string APP_NAME = "Sale Bill System";
        
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            
            try
            {
                // Initialize database
                if (DatabaseManager.Initialize())
                {
                    // Start the main form
                    Application.Run(new MainForm());
                }
                else
                {
                    // Offer a solution if database initialization failed
                    if (MessageBox.Show(
                        "Failed to initialize the database. This is likely because the Microsoft Access Database Engine is not installed.\n\n" +
                        "Would you like to download and install the Microsoft Access Database Engine Redistributable?",
                        APP_NAME,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        // Open the download page for Microsoft Access Database Engine
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "https://www.microsoft.com/en-us/download/details.aspx?id=54920",
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", APP_NAME, 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}