using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using SaleBillSystem.NET.Models; // Assuming your User model is here

namespace SaleBillSystem.NET.Data
{
    /// <summary>
    /// Provides services for user management and authentication.
    /// </summary>
    public static class UserService
    {
        /// <summary>
        /// Authenticates a user based on username and password.
        /// </summary>
        /// <returns>The User object if authentication is successful; otherwise, null.</returns>
        public static User AuthenticateUser(string username, string password)
        {
            User user = GetUserByUsername(username);

            if (user != null)
            {
                string hashedPassword = (password);
                if (user.PasswordHash == hashedPassword)
                {
                    return user; // Success
                }
            }
            return null; // Failed
        }

        /// <summary>
        /// Gets a list of all users.
        /// </summary>
        public static List<User> GetAllUsers()
        {
            var users = new List<User>();
            string sql = "SELECT * FROM UserMaster ORDER BY Username";

            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(sql);
                foreach (DataRow row in dt.Rows)
                {
                    users.Add(MapRowToUser(row));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return users;
        }

        /// <summary>
        /// Gets a single user by their username.
        /// </summary>
        public static User GetUserByUsername(string username)
        {
            string sql = "SELECT * FROM UserMaster WHERE Username = ?";
            var param = new OleDbParameter("Username", username);

            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(sql, param);
                if (dt.Rows.Count > 0)
                {
                    return MapRowToUser(dt.Rows[0]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting user: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }

        /// <summary>
        /// Saves a user (creates new or updates existing).
        /// </summary>
        /// <param name="user">The user object to save.</param>
        /// <param name="plainPassword">The user's new password. If null or empty, the password is not changed.</param>
        public static bool SaveUser(User user, string plainPassword = null)
        {
            try
            {
                // Hash the password if a new one is provided
                if (!string.IsNullOrWhiteSpace(plainPassword))
                {
                    user.PasswordHash = HashPassword(plainPassword);
                }

                if (user.UserID == 0) // Create new user
                {
                    string sql = @"INSERT INTO UserMaster (Username, PasswordHash, DisplayName, IsAdmin) 
                                 VALUES (?, ?, ?, ?)";
                    var parameters = new OleDbParameter[]
                    {
                        new OleDbParameter("Username", user.Username),
                        new OleDbParameter("PasswordHash", user.PasswordHash),
                        new OleDbParameter("DisplayName", user.DisplayName ?? (object)DBNull.Value),
                        new OleDbParameter("IsAdmin", user.IsAdmin)
                    };
                    DatabaseManager.ExecuteNonQuery(sql, parameters);
                }
                else // Update existing user
                {
                    // Conditionally build the UPDATE statement
                    string sql = string.IsNullOrWhiteSpace(plainPassword)
                        ? "UPDATE UserMaster SET Username = ?, DisplayName = ?, IsAdmin = ? WHERE UserID = ?"
                        : "UPDATE UserMaster SET Username = ?, PasswordHash = ?, DisplayName = ?, IsAdmin = ? WHERE UserID = ?";
                    
                    var parameters = new List<OleDbParameter>
                    {
                        new OleDbParameter("Username", user.Username),
                    };

                    if (!string.IsNullOrWhiteSpace(plainPassword))
                    {
                        parameters.Add(new OleDbParameter("PasswordHash", user.PasswordHash));
                    }

                    parameters.AddRange(new OleDbParameter[] {
                        new OleDbParameter("DisplayName", user.DisplayName ?? (object)DBNull.Value),
                        new OleDbParameter("IsAdmin", user.IsAdmin),
                        new OleDbParameter("UserID", user.UserID)
                    });
                    
                    DatabaseManager.ExecuteNonQuery(sql, parameters.ToArray());
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving user: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Deletes a user from the database.
        /// </summary>
        public static bool DeleteUser(int userId)
        {
            try
            {
                string sql = "DELETE FROM UserMaster WHERE UserID = ?";
                var param = new OleDbParameter("UserID", userId);
                DatabaseManager.ExecuteNonQuery(sql, param);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting user: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Hashes a plain-text password using SHA256.
        /// </summary>
        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Helper method to map a DataRow to a User object.
        /// </summary>
        private static User MapRowToUser(DataRow row)
        {
            return new User
            {
                UserID = Convert.ToInt32(row["UserID"]),
                Username = row["Username"].ToString(),
                PasswordHash = row["PasswordHash"].ToString(),
                DisplayName = row["DisplayName"].ToString(),
                IsAdmin = Convert.ToBoolean(row["IsAdmin"])
            };
        }
    }
}