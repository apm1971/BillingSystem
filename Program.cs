using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Forms;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET
{
    internal static class Program
    {
        // Application-wide constants
        public const string APP_NAME = "Sale Bill System";
        
        // Application-wide state
        public static User CurrentUser { get; set; }
        public static Company ActiveCompany { get; set; }
        
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
                    // Create necessary tables if they don't exist
                    DatabaseManager.CreateTablesIfNeeded();
                    
                    // Show login screen
                    using (var loginForm = new LoginForm())
                    {
                        if (loginForm.ShowDialog() == DialogResult.OK)
                        {
                            // Authentication successful, set current user
                            CurrentUser = loginForm.AuthenticatedUser;
                            
                            // Get active company
                            ActiveCompany = CompanyService.GetActiveCompany();
                            
                            // If no companies exist, prompt to create one
                            if (ActiveCompany == null)
                            {
                                MessageBox.Show(
                                    "Welcome to Sale Bill System!\n\nNo companies found. Please create a company to continue.",
                                    APP_NAME,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                                
                                using (var companyForm = new CompanyForm())
                                {
                                    if (companyForm.ShowDialog() == DialogResult.OK)
                                    {
                                        // Refresh active company
                                        ActiveCompany = CompanyService.GetActiveCompany();
                                        
                                        // If still no active company, show company list to select one
                                        if (ActiveCompany == null)
                                        {
                                            MessageBox.Show(
                                                "Please select an active company to continue.",
                                                APP_NAME,
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Information);
                                                
                                            using (var companyListForm = new CompanyListForm())
                                            {
                                                if (companyListForm.ShowDialog() == DialogResult.OK)
                                                {
                                                    // Refresh active company again
                                                    ActiveCompany = CompanyService.GetActiveCompany();
                                                }
                                            }
                                            
                                            // If still no active company, exit application
                                            if (ActiveCompany == null)
                                            {
                                                MessageBox.Show(
                                                    "No active company selected. The application will now exit.",
                                                    APP_NAME,
                                                    MessageBoxButtons.OK,
                                                    MessageBoxIcon.Warning);
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // User cancelled company creation, exit application
                                        return;
                                    }
                                }
                            }
                            
                            // Start the main form only if an active company exists
                            if (ActiveCompany != null)
                            {
                                Application.Run(new MainForm());
                            }
                            else
                            {
                                MessageBox.Show(
                                    "No active company selected. The application will now exit.",
                                    APP_NAME,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                            }
                        }
                    }
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