using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Data
{
    public class CompanyService
    {
        // Get all companies
        public static List<Company> GetAllCompanies()
        {
            List<Company> companies = new List<Company>();
            
            string sql = "SELECT * FROM CompanyMaster ORDER BY CompanyName";
            
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(sql);
                
                foreach (DataRow row in dt.Rows)
                {
                    Company company = new Company
                    {
                        CompanyID = Convert.ToInt32(row["CompanyID"]),
                        CompanyName = row["CompanyName"].ToString(),
                        PrintName = row["PrintName"].ToString(),
                        Address = row["Address"].ToString(),
                        City = row["City"].ToString(),
                        FinancialYearStart = Convert.ToDateTime(row["FinancialYearStart"]),
                        FinancialYearEnd = Convert.ToDateTime(row["FinancialYearEnd"]),
                        IsActive = Convert.ToBoolean(row["IsActive"]),
                        CreatedOn = Convert.ToDateTime(row["CreatedOn"])
                    };
                    
                    companies.Add(company);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error loading companies: {ex.Message}", "Database Error",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            
            return companies;
        }
        
        // Get active company
        public static Company GetActiveCompany()
        {
            string sql = "SELECT * FROM CompanyMaster WHERE IsActive = True";
            
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(sql);
                
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    
                    return new Company
                    {
                        CompanyID = Convert.ToInt32(row["CompanyID"]),
                        CompanyName = row["CompanyName"].ToString(),
                        PrintName = row["PrintName"].ToString(),
                        Address = row["Address"].ToString(),
                        City = row["City"].ToString(),
                        FinancialYearStart = Convert.ToDateTime(row["FinancialYearStart"]),
                        FinancialYearEnd = Convert.ToDateTime(row["FinancialYearEnd"]),
                        IsActive = Convert.ToBoolean(row["IsActive"]),
                        CreatedOn = Convert.ToDateTime(row["CreatedOn"])
                    };
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error getting active company: {ex.Message}", "Database Error",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            
            // If no active company found, return null
            return null;
        }
        
        // Get company by ID
        public static Company GetCompanyByID(int companyID)
        {
            string sql = "SELECT * FROM CompanyMaster WHERE CompanyID = ?";
            OleDbParameter param = new OleDbParameter("CompanyID", OleDbType.Integer) { Value = companyID };
            
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
                
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    
                    return new Company
                    {
                        CompanyID = Convert.ToInt32(row["CompanyID"]),
                        CompanyName = row["CompanyName"].ToString(),
                        PrintName = row["PrintName"].ToString(),
                        Address = row["Address"].ToString(),
                        City = row["City"].ToString(),
                        FinancialYearStart = Convert.ToDateTime(row["FinancialYearStart"]),
                        FinancialYearEnd = Convert.ToDateTime(row["FinancialYearEnd"]),
                        IsActive = Convert.ToBoolean(row["IsActive"]),
                        CreatedOn = Convert.ToDateTime(row["CreatedOn"])
                    };
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error getting company: {ex.Message}", "Database Error",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            
            return null;
        }
        
        // Save company (Create or Update)
        public static bool SaveCompany(Company company)
        {
            using (OleDbConnection conn = DatabaseManager.GetConnection())
            {
                conn.Open();
                OleDbTransaction transaction = conn.BeginTransaction();
                
                try
                {
                    // Check if company already exists
                    if (company.CompanyID == 0)
                    {
                        // First, deactivate all existing companies if this one will be active
                        if (company.IsActive)
                        {
                            string deactivateAllSql = "UPDATE CompanyMaster SET IsActive = False";
                            using (OleDbCommand cmd = new OleDbCommand(deactivateAllSql, conn))
                            {
                                cmd.Transaction = transaction;
                                cmd.ExecuteNonQuery();
                            }
                        }
                        
                        // Insert new company
                        string insertSql = @"
                            INSERT INTO CompanyMaster 
                            (CompanyName, PrintName, Address, City, FinancialYearStart, FinancialYearEnd, IsActive, CreatedOn) 
                            VALUES (?, ?, ?, ?, ?, ?, ?, ?)";
                            
                        using (OleDbCommand cmd = new OleDbCommand(insertSql, conn))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("CompanyName", company.CompanyName);
                            cmd.Parameters.AddWithValue("PrintName", company.PrintName);
                            cmd.Parameters.AddWithValue("Address", company.Address);
                            cmd.Parameters.AddWithValue("City", company.City);
                            
                            // Fix: Use explicit parameter types for DateTime in Access
                            OleDbParameter fyStartParam = new OleDbParameter("FinancialYearStart", OleDbType.Date);
                            fyStartParam.Value = company.FinancialYearStart;
                            cmd.Parameters.Add(fyStartParam);
                            
                            OleDbParameter fyEndParam = new OleDbParameter("FinancialYearEnd", OleDbType.Date);
                            fyEndParam.Value = company.FinancialYearEnd;
                            cmd.Parameters.Add(fyEndParam);
                            
                            // Always set IsActive to true for the first company
                            bool isActive = company.IsActive;
                            if (GetAllCompanies().Count == 0)
                            {
                                isActive = true;
                            }
                            cmd.Parameters.AddWithValue("IsActive", isActive);
                            
                            OleDbParameter createdOnParam = new OleDbParameter("CreatedOn", OleDbType.Date);
                            createdOnParam.Value = DateTime.Now;
                            cmd.Parameters.Add(createdOnParam);
                            
                            cmd.ExecuteNonQuery();
                        }
                        
                        // We've already deactivated other companies before insertion
                    }
                    else
                    {
                        // Update existing company (only allowed fields)
                        string updateSql = @"
                            UPDATE CompanyMaster 
                            SET CompanyName = ?, PrintName = ?, Address = ?, City = ?, FinancialYearEnd = ?, IsActive = ? 
                            WHERE CompanyID = ?";
                            
                        using (OleDbCommand cmd = new OleDbCommand(updateSql, conn))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("CompanyName", company.CompanyName);
                            cmd.Parameters.AddWithValue("PrintName", company.PrintName);
                            cmd.Parameters.AddWithValue("Address", company.Address);
                            cmd.Parameters.AddWithValue("City", company.City);
                            
                            // Fix: Use explicit parameter type for DateTime in Access
                            OleDbParameter fyEndParam = new OleDbParameter("FinancialYearEnd", OleDbType.Date);
                            fyEndParam.Value = company.FinancialYearEnd;
                            cmd.Parameters.Add(fyEndParam);
                            
                            cmd.Parameters.AddWithValue("IsActive", company.IsActive);
                            cmd.Parameters.AddWithValue("CompanyID", company.CompanyID);
                            
                            cmd.ExecuteNonQuery();
                        }
                        
                        // If this is the active company, deactivate others
                        if (company.IsActive)
                        {
                            string deactivateSql = "UPDATE CompanyMaster SET IsActive = False WHERE CompanyID <> ?";
                            using (OleDbCommand cmd = new OleDbCommand(deactivateSql, conn))
                            {
                                cmd.Transaction = transaction;
                                cmd.Parameters.AddWithValue("CompanyID", company.CompanyID);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    System.Windows.Forms.MessageBox.Show($"Error saving company: {ex.Message}", "Database Error",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return false;
                }
            }
        }
        
        // Check if date is within financial year
        public static bool IsDateWithinFinancialYear(DateTime date)
        {
            Company activeCompany = GetActiveCompany();
            if (activeCompany == null)
                return true; // No company restriction
                
            return activeCompany.IsDateWithinFinancialYear(date);
        }
        
        // Get financial year boundaries
        public static (DateTime startDate, DateTime endDate) GetFinancialYearBoundaries()
        {
            Company activeCompany = GetActiveCompany();
            if (activeCompany == null)
            {
                // Default financial year if no company is set
                int year = DateTime.Now.Month >= 4 ? DateTime.Now.Year : DateTime.Now.Year - 1;
                return (new DateTime(year, 4, 1), new DateTime(year + 1, 3, 31));
            }
                
            return (activeCompany.FinancialYearStart, activeCompany.FinancialYearEnd);
        }
        
        // Generate default financial year dates
        public static (DateTime startDate, DateTime endDate) GenerateDefaultFinancialYear()
        {
            int currentYear = DateTime.Now.Year;
            int startYear, endYear;
            
            if (DateTime.Now.Month >= 4)
            {
                // We're after April 1st, so use current year as start
                startYear = currentYear;
                endYear = currentYear + 1;
            }
            else
            {
                // We're before April 1st, so use previous year as start
                startYear = currentYear - 1;
                endYear = currentYear;
            }
            
            DateTime startDate = new DateTime(startYear, 4, 1);
            DateTime endDate = new DateTime(endYear, 3, 31);
            
            return (startDate, endDate);
        }
    }
} 