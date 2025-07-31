using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;
using SaleBillSystem.NET.Models; // Assuming your Company model is here

namespace SaleBillSystem.NET.Data
{
    /// <summary>
    /// Provides data services for the CompanyMaster table.
    /// </summary>
    public static class CompanyService
    {
        /// <summary>
        /// Gets a list of all companies from the database.
        /// </summary>
        public static List<Company> GetAllCompanies()
        {
            var companies = new List<Company>();
            string sql = "SELECT * FROM CompanyMaster ORDER BY CompanyName";

            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(sql);
                foreach (DataRow row in dt.Rows)
                {
                    companies.Add(MapRowToCompany(row));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading companies: {ex.Message}", "Database Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return companies;
        }

        /// <summary>
        /// Gets a single company by its unique ID.
        /// </summary>
        public static Company GetCompanyByID(int companyId)
        {
            string sql = "SELECT * FROM CompanyMaster WHERE CompanyID = ?";
            var param = new OleDbParameter("CompanyID", companyId);

            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
                if (dt.Rows.Count > 0)
                {
                    return MapRowToCompany(dt.Rows[0]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting company by ID: {ex.Message}", "Database Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null; // Return null if not found
        }

        /// <summary>
        /// Saves a company to the database (either creates a new one or updates an existing one).
        /// </summary>
        public static bool SaveCompany(Company company)
        {
            try
            {
                if (company.CompanyID == 0) // New company
                {
                    string sql = @"INSERT INTO CompanyMaster (CompanyName, Address, Phone) 
                                 VALUES (?, ?, ?)";
                    var parameters = new OleDbParameter[]
                    {
                        new OleDbParameter("CompanyName", company.CompanyName),
                        new OleDbParameter("Address", company.Address ?? (object)DBNull.Value),
                        new OleDbParameter("Phone", company.Phone ?? (object)DBNull.Value)
                    };
                    DatabaseManager.ExecuteNonQuery(sql, parameters);
                }
                else // Existing company
                {
                    string sql = @"UPDATE CompanyMaster 
                                 SET CompanyName = ?, Address = ?, Phone = ?
                                 WHERE CompanyID = ?";
                    var parameters = new OleDbParameter[]
                    {
                        new OleDbParameter("CompanyName", company.CompanyName),
                        new OleDbParameter("Address", company.Address ?? (object)DBNull.Value),
                        new OleDbParameter("Phone", company.Phone ?? (object)DBNull.Value),
                        new OleDbParameter("CompanyID", company.CompanyID)
                    };
                    DatabaseManager.ExecuteNonQuery(sql, parameters);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving company: {ex.Message}", "Database Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Deletes a company from the database.
        /// </summary>
        public static bool DeleteCompany(int companyId)
        {
            // WARNING: You should also delete all related data (parties, bills, etc.) 
            // or handle this with database constraints if your DB supports it.
            try
            {
                string sql = "DELETE FROM CompanyMaster WHERE CompanyID = ?";
                var param = new OleDbParameter("CompanyID", companyId);
                DatabaseManager.ExecuteNonQuery(sql, param);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting company: {ex.Message}", "Database Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Helper method to map a DataRow to a Company object.
        /// </summary>
        private static Company MapRowToCompany(DataRow row)
        {
            return new Company
            {
                CompanyID = Convert.ToInt32(row["CompanyID"]),
                CompanyName = row["CompanyName"].ToString(),
                Address = row["Address"].ToString(),
                Phone = row["Phone"].ToString()
            };
        }
    }
}