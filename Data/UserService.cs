using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Security.Cryptography;
using System.Text;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Data
{
    public class UserService
    {
        // Get all users
        public static List<User> GetAllUsers()
        {
            List<User> users = new List<User>();
            
            string sql = "SELECT * FROM UserMaster ORDER BY Username";
            
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(sql);
                
                foreach (DataRow row in dt.Rows)
                {
                    User user = new User
                    {
                        UserID = Convert.ToInt32(row["UserID"]),
                        Username = row["Username"].ToString(),
                        PasswordHash = row["PasswordHash"].ToString(),
                        DisplayName = row["DisplayName"].ToString(),
                        IsAdmin = Convert.ToBoolean(row["IsAdmin"]),
                        IsActive = Convert.ToBoolean(row["IsActive"]),
                        CreatedOn = Convert.ToDateTime(row["CreatedOn"]),
                        LastLogin = row["LastLogin"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["LastLogin"]) : null
                    };
                    
                    users.Add(user);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error loading users: {ex.Message}", "Database Error",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            
            return users;
        }
        
        // Get user by username
        public static User GetUserByUsername(string username)
        {
            string sql = "SELECT * FROM UserMaster WHERE Username = ?";
            OleDbParameter param = new OleDbParameter("Username", OleDbType.VarChar) { Value = username };
            
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
                
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    
                    return new User
                    {
                        UserID = Convert.ToInt32(row["UserID"]),
                        Username = row["Username"].ToString(),
                        PasswordHash = row["PasswordHash"].ToString(),
                        DisplayName = row["DisplayName"].ToString(),
                        IsAdmin = Convert.ToBoolean(row["IsAdmin"]),
                        IsActive = Convert.ToBoolean(row["IsActive"]),
                        CreatedOn = Convert.ToDateTime(row["CreatedOn"]),
                        LastLogin = row["LastLogin"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["LastLogin"]) : null
                    };
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error getting user: {ex.Message}", "Database Error",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            
            return null;
        }
        
        // Authenticate user
        public static User AuthenticateUser(string username, string password)
        {
            User user = GetUserByUsername(username);
            
            if (user != null && user.IsActive)
            {
                string hashedPassword = HashPassword(password);
                
                if (user.PasswordHash == hashedPassword)
                {
                    // Update last login time
                    UpdateLastLogin(user.UserID);
                    return user;
                }
            }
            
            return null;
        }
        
        // Update last login time
        private static void UpdateLastLogin(int userID)
        {
            string sql = "UPDATE UserMaster SET LastLogin = ? WHERE UserID = ?";
            
            try
            {
                using (OleDbConnection conn = DatabaseManager.GetConnection())
                {
                    conn.Open();
                    
                    using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                    {
                        // Fix: Use explicit parameter type for DateTime in Access
                        OleDbParameter dateParam = new OleDbParameter("LastLogin", OleDbType.Date);
                        dateParam.Value = DateTime.Now;
                        cmd.Parameters.Add(dateParam);
                        
                        cmd.Parameters.AddWithValue("UserID", userID);
                        
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error updating last login: {ex.Message}", "Database Error",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
        
        // Save user (Create or Update)
        public static bool SaveUser(User user, string plainPassword = null)
        {
            using (OleDbConnection conn = DatabaseManager.GetConnection())
            {
                conn.Open();
                OleDbTransaction transaction = conn.BeginTransaction();
                
                try
                {
                    // Hash password if provided
                    if (!string.IsNullOrEmpty(plainPassword))
                    {
                        user.PasswordHash = HashPassword(plainPassword);
                    }
                    
                    if (user.UserID == 0)
                    {
                        // Create new user
                        string insertSql = @"
                            INSERT INTO UserMaster 
                            (Username, PasswordHash, DisplayName, IsAdmin, IsActive, CreatedOn) 
                            VALUES (?, ?, ?, ?, ?, ?)";
                            
                        using (OleDbCommand cmd = new OleDbCommand(insertSql, conn))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("Username", user.Username);
                            cmd.Parameters.AddWithValue("PasswordHash", user.PasswordHash);
                            cmd.Parameters.AddWithValue("DisplayName", user.DisplayName);
                            cmd.Parameters.AddWithValue("IsAdmin", user.IsAdmin);
                            cmd.Parameters.AddWithValue("IsActive", user.IsActive);
                            
                            // Fix: Use explicit parameter type for DateTime in Access
                            OleDbParameter dateParam = new OleDbParameter("CreatedOn", OleDbType.Date);
                            dateParam.Value = DateTime.Now;
                            cmd.Parameters.Add(dateParam);
                            
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // Update existing user
                        string updateSql;
                        
                        if (!string.IsNullOrEmpty(plainPassword))
                        {
                            // Update with new password
                            updateSql = @"
                                UPDATE UserMaster SET
                                Username = ?, PasswordHash = ?, DisplayName = ?, IsAdmin = ?, IsActive = ?
                                WHERE UserID = ?";
                                
                            using (OleDbCommand cmd = new OleDbCommand(updateSql, conn))
                            {
                                cmd.Transaction = transaction;
                                cmd.Parameters.AddWithValue("Username", user.Username);
                                cmd.Parameters.AddWithValue("PasswordHash", user.PasswordHash);
                                cmd.Parameters.AddWithValue("DisplayName", user.DisplayName);
                                cmd.Parameters.AddWithValue("IsAdmin", user.IsAdmin);
                                cmd.Parameters.AddWithValue("IsActive", user.IsActive);
                                cmd.Parameters.AddWithValue("UserID", user.UserID);
                                
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // Update without changing password
                            updateSql = @"
                                UPDATE UserMaster SET
                                Username = ?, DisplayName = ?, IsAdmin = ?, IsActive = ?
                                WHERE UserID = ?";
                                
                            using (OleDbCommand cmd = new OleDbCommand(updateSql, conn))
                            {
                                cmd.Transaction = transaction;
                                cmd.Parameters.AddWithValue("Username", user.Username);
                                cmd.Parameters.AddWithValue("DisplayName", user.DisplayName);
                                cmd.Parameters.AddWithValue("IsAdmin", user.IsAdmin);
                                cmd.Parameters.AddWithValue("IsActive", user.IsActive);
                                cmd.Parameters.AddWithValue("UserID", user.UserID);
                                
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
                    System.Windows.Forms.MessageBox.Show($"Error saving user: {ex.Message}", "Database Error",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return false;
                }
            }
        }
        
        // Hash password using SHA256
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                
                return builder.ToString();
            }
        }
        
        // Create default admin user if no users exist
        public static void EnsureAdminUserExists()
        {
            try
            {
                string countSql = "SELECT COUNT(*) FROM UserMaster";
                int userCount = Convert.ToInt32(DatabaseManager.ExecuteScalar(countSql));
                
                if (userCount == 0)
                {
                    // Create default admin user
                    User adminUser = new User
                    {
                        Username = "admin",
                        DisplayName = "Administrator",
                        IsAdmin = true,
                        IsActive = true,
                        CreatedOn = DateTime.Now
                    };
                    
                    // Use direct SQL to avoid potential parameter type issues
                    using (OleDbConnection conn = DatabaseManager.GetConnection())
                    {
                        conn.Open();
                        using (OleDbTransaction transaction = conn.BeginTransaction())
                        {
                            try
                            {
                                string insertSql = @"
                                    INSERT INTO UserMaster 
                                    (Username, PasswordHash, DisplayName, IsAdmin, IsActive, CreatedOn) 
                                    VALUES (?, ?, ?, ?, ?, ?)";
                                    
                                using (OleDbCommand cmd = new OleDbCommand(insertSql, conn))
                                {
                                    cmd.Transaction = transaction;
                                    cmd.Parameters.AddWithValue("Username", adminUser.Username);
                                    cmd.Parameters.AddWithValue("PasswordHash", HashPassword("admin"));
                                    cmd.Parameters.AddWithValue("DisplayName", adminUser.DisplayName);
                                    cmd.Parameters.AddWithValue("IsAdmin", adminUser.IsAdmin);
                                    cmd.Parameters.AddWithValue("IsActive", adminUser.IsActive);
                                    
                                    // Fix: Use explicit parameter type for DateTime in Access
                                    OleDbParameter dateParam = new OleDbParameter("CreatedOn", OleDbType.Date);
                                    dateParam.Value = DateTime.Now;
                                    cmd.Parameters.Add(dateParam);
                                    
                                    cmd.ExecuteNonQuery();
                                }
                                
                                transaction.Commit();
                                
                                System.Windows.Forms.MessageBox.Show(
                                    "Default admin user created.\nUsername: admin\nPassword: admin\n\nPlease change the password after logging in.",
                                    "Initial Setup",
                                    System.Windows.Forms.MessageBoxButtons.OK,
                                    System.Windows.Forms.MessageBoxIcon.Information);
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                throw new Exception($"Failed to create admin user: {ex.Message}", ex);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error checking for admin user: {ex.Message}", "Database Error",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }
} 