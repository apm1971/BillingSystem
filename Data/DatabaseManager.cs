using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace SaleBillSystem.NET.Data
{
    public class DatabaseManager
    {
        // Database constants
        private const string DB_FILENAME = "SaleSystem.db";

        // Database connection string
        private static string _connectionString;

        // Initialize the database manager
        public static bool Initialize()
        {
            try
            {
                string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", DB_FILENAME);
                
                // Check if database directory exists, if not create it
                string dbDir = Path.GetDirectoryName(dbPath);
                if (!Directory.Exists(dbDir))
                {
                    Directory.CreateDirectory(dbDir);
                }
                
                // Check if database exists, if not create it
                if (!File.Exists(dbPath))
                {
                    if (!CreateDatabase(dbPath))
                    {
                        return false;
                    }
                }
                
                // Set connection string
                _connectionString = $"Data Source={dbPath};Version=3;";
                
                // Test connection
                using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                }
                
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error initializing database: {ex.Message}", "Database Error", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }

        // Create database with required tables
        private static bool CreateDatabase(string dbPath)
        {
            try
            {
                // Create a new SQLite database
                SQLiteConnection.CreateFile(dbPath);
                
                // Connect to the new database
                using (SQLiteConnection conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    conn.Open();
                    
                    // Create PartyMaster table
                    ExecuteNonQuery(conn, @"CREATE TABLE PartyMaster (
                        PartyID INTEGER PRIMARY KEY AUTOINCREMENT,
                        PartyName TEXT,
                        Address TEXT,
                        City TEXT,
                        Phone TEXT,
                        Email TEXT,
                        GSTIN TEXT,
                        CreditLimit REAL,
                        OutstandingAmount REAL
                    )");
                    
                    // Create ItemMaster table
                    ExecuteNonQuery(conn, @"CREATE TABLE ItemMaster (
                        ItemID INTEGER PRIMARY KEY AUTOINCREMENT,
                        ItemCode TEXT,
                        ItemName TEXT,
                        Unit TEXT,
                        Rate REAL,
                        GST REAL,
                        StockQuantity REAL
                    )");
                    
                    // Create BillMaster table
                    ExecuteNonQuery(conn, @"CREATE TABLE BillMaster (
                        BillID INTEGER PRIMARY KEY AUTOINCREMENT,
                        BillNo TEXT,
                        BillDate TEXT,
                        PartyID INTEGER,
                        PartyName TEXT,
                        TotalAmount REAL,
                        TotalGST REAL,
                        NetAmount REAL
                    )");
                    
                    // Create BillDetails table
                    ExecuteNonQuery(conn, @"CREATE TABLE BillDetails (
                        BillDetailID INTEGER PRIMARY KEY AUTOINCREMENT,
                        BillID INTEGER,
                        ItemID INTEGER,
                        ItemName TEXT,
                        Quantity REAL,
                        Rate REAL,
                        Amount REAL,
                        GSTPct REAL,
                        GSTAmount REAL,
                        TotalAmount REAL
                    )");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error creating database: {ex.Message}", "Database Error", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }
        
        // Execute a non-query SQL command
        private static int ExecuteNonQuery(SQLiteConnection connection, string sql, params SQLiteParameter[] parameters)
        {
            using (SQLiteCommand cmd = new SQLiteCommand(sql, connection))
            {
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }
                return cmd.ExecuteNonQuery();
            }
        }

        // Get a database connection
        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(_connectionString);
        }
        
        // Execute a query and return a DataTable
        public static DataTable ExecuteQuery(string sql, params SQLiteParameter[] parameters)
        {
            DataTable dt = new DataTable();
            
            using (SQLiteConnection conn = GetConnection())
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            
            return dt;
        }
        
        // Execute a non-query SQL command
        public static int ExecuteNonQuery(string sql, params SQLiteParameter[] parameters)
        {
            using (SQLiteConnection conn = GetConnection())
            {
                conn.Open();
                return ExecuteNonQuery(conn, sql, parameters);
            }
        }
        
        // Execute a scalar query
        public static object ExecuteScalar(string sql, params SQLiteParameter[] parameters)
        {
            using (SQLiteConnection conn = GetConnection())
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    
                    return cmd.ExecuteScalar();
                }
            }
        }
        
        // Get next bill number
        public static string GetNextBillNumber()
        {
            try
            {
                string sql = "SELECT MAX(CAST(BillNo AS INTEGER)) FROM BillMaster";
                object result = ExecuteScalar(sql);
                
                int lastBillNo = 0;
                if (result != null && result != DBNull.Value)
                {
                    int.TryParse(result.ToString(), out lastBillNo);
                }
                
                return (lastBillNo + 1).ToString("00000");
            }
            catch (Exception)
            {
                return "00001";
            }
        }
        
        // Begin a transaction
        public static SQLiteTransaction BeginTransaction()
        {
            SQLiteConnection conn = GetConnection();
            conn.Open();
            return conn.BeginTransaction();
        }
    }
} 