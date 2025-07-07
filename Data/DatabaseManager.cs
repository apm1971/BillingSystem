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
                
                // Test connection and upgrade database if needed
                using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    UpgradeDatabase(connection);
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
                    
                    // Create BrokerMaster table
                    ExecuteNonQuery(conn, @"CREATE TABLE BrokerMaster (
                        BrokerID INTEGER PRIMARY KEY AUTOINCREMENT,
                        BrokerName TEXT,
                        Phone TEXT,
                        Email TEXT
                    )");

                    // Create PartyMaster table
                    ExecuteNonQuery(conn, @"CREATE TABLE PartyMaster (
                        PartyID INTEGER PRIMARY KEY AUTOINCREMENT,
                        PartyName TEXT,
                        Address TEXT,
                        City TEXT,
                        Phone TEXT,
                        Email TEXT,
                        CreditLimit REAL,
                        CreditDays INTEGER,
                        OutstandingAmount REAL,
                        BrokerID INTEGER,
                        BrokerName TEXT
                    )");
                    
                    // Create ItemMaster table
                    ExecuteNonQuery(conn, @"CREATE TABLE ItemMaster (
                        ItemID INTEGER PRIMARY KEY AUTOINCREMENT,
                        ItemCode TEXT,
                        ItemName TEXT,
                        Unit TEXT,
                        Rate REAL,
                        Charges REAL,
                        StockQuantity REAL
                    )");
                    
                    // Create BillMaster table
                    ExecuteNonQuery(conn, @"CREATE TABLE BillMaster (
                        BillID INTEGER PRIMARY KEY AUTOINCREMENT,
                        BillNo TEXT,
                        BillDate TEXT,
                        DueDate TEXT,
                        PartyID INTEGER,
                        PartyName TEXT,
                        BrokerID INTEGER,
                        BrokerName TEXT,
                        TotalAmount REAL,
                        TotalCharges REAL,
                        NetAmount REAL,
                        PaidAmount REAL DEFAULT 0
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
                        Charges REAL,
                        TotalAmount REAL
                    )");

                    // Create PaymentMaster table
                    ExecuteNonQuery(conn, @"CREATE TABLE PaymentMaster (
                        PaymentID INTEGER PRIMARY KEY AUTOINCREMENT,
                        PaymentDate TEXT,
                        PaymentAmount REAL,
                        PaymentMethod TEXT,
                        Reference TEXT,
                        Notes TEXT
                    )");

                    // Create PaymentDetails table
                    ExecuteNonQuery(conn, @"CREATE TABLE PaymentDetails (
                        PaymentDetailID INTEGER PRIMARY KEY AUTOINCREMENT,
                        PaymentID INTEGER,
                        BillID INTEGER,
                        PreviousPaid REAL,
                        BalanceBefore REAL,
                        AllocatedAmount REAL,
                        BalanceAfter REAL
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

        // Upgrade database schema if needed
        private static void UpgradeDatabase(SQLiteConnection conn)
        {
            try
            {
                // Check if BrokerMaster table exists
                var tableInfo = ExecuteQuery(conn, "SELECT name FROM sqlite_master WHERE type='table' AND name='BrokerMaster'");
                if (tableInfo.Rows.Count == 0)
                {
                    // Create BrokerMaster table
                    ExecuteNonQuery(conn, @"CREATE TABLE BrokerMaster (
                        BrokerID INTEGER PRIMARY KEY AUTOINCREMENT,
                        BrokerName TEXT,
                        Phone TEXT,
                        Email TEXT
                    )");
                }

                // Check and add broker columns to PartyMaster
                var partyTableInfo = ExecuteQuery(conn, "PRAGMA table_info(PartyMaster)");
                bool brokerIdExistsInParty = false;
                bool brokerNameExistsInParty = false;
                foreach (DataRow row in partyTableInfo.Rows)
                {
                    string columnName = row["name"].ToString();
                    if (columnName == "BrokerID") brokerIdExistsInParty = true;
                    if (columnName == "BrokerName") brokerNameExistsInParty = true;
                }

                if (!brokerIdExistsInParty)
                {
                    ExecuteNonQuery(conn, "ALTER TABLE PartyMaster ADD COLUMN BrokerID INTEGER");
                }
                if (!brokerNameExistsInParty)
                {
                    ExecuteNonQuery(conn, "ALTER TABLE PartyMaster ADD COLUMN BrokerName TEXT");
                }

                // Check and add columns to BillMaster
                var billTableInfo = ExecuteQuery(conn, "PRAGMA table_info(BillMaster)");
                bool dueDateExists = false;
                bool brokerIdExistsInBill = false;
                bool brokerNameExistsInBill = false;
                foreach (DataRow row in billTableInfo.Rows)
                {
                    string columnName = row["name"].ToString();
                    if (columnName == "DueDate") dueDateExists = true;
                    if (columnName == "BrokerID") brokerIdExistsInBill = true;
                    if (columnName == "BrokerName") brokerNameExistsInBill = true;
                }

                // Add DueDate column if it doesn't exist
                if (!dueDateExists)
                {
                    ExecuteNonQuery(conn, "ALTER TABLE BillMaster ADD COLUMN DueDate TEXT");
                    
                    // Update existing bills with calculated due dates
                    ExecuteNonQuery(conn, @"
                        UPDATE BillMaster 
                        SET DueDate = date(BillDate, '+30 days') 
                        WHERE DueDate IS NULL OR DueDate = ''");
                }

                // Add broker columns to BillMaster
                if (!brokerIdExistsInBill)
                {
                    ExecuteNonQuery(conn, "ALTER TABLE BillMaster ADD COLUMN BrokerID INTEGER");
                }
                if (!brokerNameExistsInBill)
                {
                    ExecuteNonQuery(conn, "ALTER TABLE BillMaster ADD COLUMN BrokerName TEXT");
                }

                // Check and add PaidAmount column to BillMaster
                bool paidAmountExists = false;
                foreach (DataRow row in billTableInfo.Rows)
                {
                    if (row["name"].ToString() == "PaidAmount")
                    {
                        paidAmountExists = true;
                        break;
                    }
                }

                if (!paidAmountExists)
                {
                    ExecuteNonQuery(conn, "ALTER TABLE BillMaster ADD COLUMN PaidAmount REAL DEFAULT 0");
                }

                // Check and create PaymentMaster table
                var paymentMasterInfo = ExecuteQuery(conn, "SELECT name FROM sqlite_master WHERE type='table' AND name='PaymentMaster'");
                if (paymentMasterInfo.Rows.Count == 0)
                {
                    ExecuteNonQuery(conn, @"CREATE TABLE PaymentMaster (
                        PaymentID INTEGER PRIMARY KEY AUTOINCREMENT,
                        PaymentDate TEXT,
                        PaymentAmount REAL,
                        PaymentMethod TEXT,
                        Reference TEXT,
                        Notes TEXT
                    )");
                }

                // Check and create PaymentDetails table
                var paymentDetailsInfo = ExecuteQuery(conn, "SELECT name FROM sqlite_master WHERE type='table' AND name='PaymentDetails'");
                if (paymentDetailsInfo.Rows.Count == 0)
                {
                    ExecuteNonQuery(conn, @"CREATE TABLE PaymentDetails (
                        PaymentDetailID INTEGER PRIMARY KEY AUTOINCREMENT,
                        PaymentID INTEGER,
                        BillID INTEGER,
                        PreviousPaid REAL,
                        BalanceBefore REAL,
                        AllocatedAmount REAL,
                        BalanceAfter REAL
                    )");
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error upgrading database: {ex.Message}", "Database Upgrade Error", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
            }
        }

        // Execute a query and return a DataTable (for connection-specific queries)
        private static DataTable ExecuteQuery(SQLiteConnection conn, string sql, params SQLiteParameter[] parameters)
        {
            DataTable dt = new DataTable();
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
            return dt;
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