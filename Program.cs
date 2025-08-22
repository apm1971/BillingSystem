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
            // DatabaseManager.UpdateDatabaseSchema();

            // Update existing bill details with default values for new fields
            // try
            // {
            //     DatabaseManager.UpdateBillDetailsWithDefaultValues();
            // }
            // catch (Exception ex)
            // {
            //     // Log the error but don't stop the application from starting
            //     System.Diagnostics.Debug.WriteLine($"Warning: Could not update bill details with default values: {ex.Message}");
            // }

            // Fix CompanyID issues for existing data (runs automatically at startup)
            // try
            // {
            //     DatabaseManager.UpdateAllCompanyIDsToOne();
            // }
            // catch (Exception ex)
            // {
            //     // Log the error but don't stop the application from starting
            //     System.Diagnostics.Debug.WriteLine($"Warning: Could not update CompanyID fields: {ex.Message}");
            // }

            // Set the active company to CompanyID = 1 so all services can find the data

            try {
                DatabaseManager.InitializeAdvancePaymentSystem();
            }catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Could not initialize advance payment system: {ex.Message}");
            }
            try
            {
                // Load the existing company with ID = 1 from the database
                using (var conn = DatabaseManager.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT CompanyID, CompanyName, Address, Phone FROM CompanyMaster WHERE CompanyID = 1";
                    using (var cmd = new System.Data.OleDb.OleDbCommand(sql, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Program.ActiveCompany = new Company
                                {
                                    CompanyID = Convert.ToInt32(reader["CompanyID"]),
                                    CompanyName = reader["CompanyName"].ToString(),
                                    Address = reader["Address"].ToString(),
                                    Phone = reader["Phone"].ToString()
                                };
                            }
                            else
                            {
                                // Fallback: create default company if none exists
                                Program.ActiveCompany = new Company
                                {
                                    CompanyID = 1,
                                    CompanyName = "Default Company",
                                    Address = "Default Address",
                                    Phone = "Default Phone"
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Could not set active company: {ex.Message}");
                // Fallback: create default company if database access fails
                Program.ActiveCompany = new Company
                {
                    CompanyID = 1,
                    CompanyName = "Default Company",
                    Address = "Default Address",
                    Phone = "Default Phone"
                };
            }

            // Start the application with the main form, which will handle the login process internally.
            Application.Run(new MainForm());
        }
    }
}