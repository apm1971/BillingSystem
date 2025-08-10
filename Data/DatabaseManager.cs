using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb; // Changed from SQLite to OleDb
using System.IO;
using System.Linq;
// Remove ADOX reference

namespace SaleBillSystem.NET.Data
{
    public class DatabaseManager
    {
        // Database constants
        private const string DB_FILENAME = "SaleSystem.accdb"; // Changed from .db to .accdb
        
        // Static property for custom database path
        public static string CustomDatabasePath { get; private set; }

        // Database connection string
        private static string _connectionString;

        // Initialize the database manager
        public static bool Initialize()
        {
            try
            {
                // Default database path in application directory
                string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", DB_FILENAME);
                
                // Check if a custom path is stored in settings
                if (File.Exists(dbPath))
                {
                    // Temporarily connect to the default database to check for custom path setting
                    string tempConnectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Persist Security Info=False;";
                    using (OleDbConnection tempConn = new OleDbConnection(tempConnectionString))
                    {
                        try
                        {
                            tempConn.Open();
                            
                            // Check if Settings table exists
                            DataTable tables = tempConn.GetSchema("Tables", new string[] { null, null, "Settings" });
                            if (tables.Rows.Count > 0)
                            {
                                // Check for custom database path setting
                                using (OleDbCommand cmd = new OleDbCommand("SELECT SettingValue FROM Settings WHERE SettingKey = 'DatabasePath'", tempConn))
                                {
                                    object result = cmd.ExecuteScalar();
                                    if (result != null && result != DBNull.Value)
                                    {
                                        string customPath = result.ToString();
                                        if (!string.IsNullOrEmpty(customPath) && File.Exists(customPath))
                                        {
                                            dbPath = customPath;
                                            CustomDatabasePath = customPath;
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // If we can't read the setting, continue with default path
                        }
                    }
                }
                
                // If custom path is set but file doesn't exist, revert to default
                if (CustomDatabasePath != null && !File.Exists(CustomDatabasePath))
                {
                    CustomDatabasePath = null;
                    dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", DB_FILENAME);
                }
                
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
                
                // Set connection string for Access
                _connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Persist Security Info=False;";
                
                // Test connection and upgrade database if needed
                using (OleDbConnection connection = new OleDbConnection(_connectionString))
                {
                    connection.Open();
                    // UpgradeDatabase(connection);
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
                // First, try using a template file if it exists
                string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "template.accdb");
                
                if (File.Exists(templatePath))
                {
                    // Copy the template database to the target location
                    File.Copy(templatePath, dbPath);
                }
                else
                {
                    // Template doesn't exist, create manually using the connection string
                    // Use Microsoft Access directly - this will require ACE to be installed
                    string connString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Engine Type=5";
                    
                    using (OleDbConnection tempConn = new OleDbConnection(connString))
                    {
                        // This will create an empty database
                        System.Windows.Forms.MessageBox.Show(
                            "Creating a new database. This might take a moment.",
                            "Creating Database",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Information);
                        
                        try 
                        {
                            // Using reflection to access ADOX Catalog
                            var catalogType = Type.GetTypeFromProgID("ADOX.Catalog");
                            if (catalogType == null) 
                            {
                                throw new InvalidOperationException("ADOX.Catalog not found. Make sure Microsoft Access or the Microsoft Access Database Engine is installed.");
                            }
                            
                            dynamic catalog = Activator.CreateInstance(catalogType);
                            catalog.Create(connString);
                            
                            System.Windows.Forms.MessageBox.Show(
                                "Database created successfully.",
                                "Database Created",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Information);
                        }
                        catch (Exception ex) 
                        {
                            throw new Exception($"Error creating database with ADOX: {ex.Message}", ex);
                        }
                    }
                }
                
                // Connect to the new database and create tables
                string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Persist Security Info=False;";
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    ExecuteNonQuery(conn, @"CREATE TABLE CompanyMaster (
                            CompanyID COUNTER PRIMARY KEY,
                            CompanyName TEXT(255) NOT NULL,
                            Address MEMO,
                            Phone TEXT(50)
                    )");
                    ExecuteNonQuery(conn, @"CREATE TABLE UserMaster (
                            UserID COUNTER PRIMARY KEY,
                            Username TEXT(50) UNIQUE NOT NULL,
                            PasswordHash TEXT(255) NOT NULL,
                            DisplayName TEXT(100),
                            IsAdmin BIT 
                    )");
                    // Create BrokerMaster table
                    ExecuteNonQuery(conn, @"CREATE TABLE BrokerMaster (
                        BrokerID COUNTER PRIMARY KEY,
                        BrokerName TEXT(255) NOT NULL,
                        Phone TEXT(50),
                        CompanyID INTEGER,
                        InterestDays INTEGER,
                        InterestRate DECIMAL(10, 2),
                        DiscountDays INTEGER,
                        DiscountRate DECIMAL(10, 2),
                        BrokerageRate DECIMAL(10, 2)
                    )");

                    // Create PartyMaster table
                    ExecuteNonQuery(conn, @"CREATE TABLE PartyMaster (
                        PartyID COUNTER PRIMARY KEY,
                        PartyName TEXT(255) NOT NULL,
                        Address MEMO,
                        Phone TEXT(50),
                        CompanyID INTEGER,
                        BrokerID INTEGER
                    )");

                    // Create ItemMaster table
                    ExecuteNonQuery(conn, @"CREATE TABLE ItemMaster (
                        ItemID COUNTER PRIMARY KEY,
                        ItemName TEXT(255) NOT NULL,
                        Unit TEXT(50),
                        DefaultRate CURRENCY DEFAULT 0,
                        Charges CURRENCY DEFAULT 0,
                        CompanyID INTEGER
                    )");

                    // Create BillMaster table
                    ExecuteNonQuery(conn, @"CREATE TABLE BillMaster (
                        BillID COUNTER PRIMARY KEY,
                        BillNo TEXT(50) NOT NULL,
                        BillDate DATETIME NOT NULL,
                        PartyID INTEGER NOT NULL,
                        BrokerID INTEGER,
                        BrokerName TEXT(255),
                        OriginalAmount CURRENCY,
                        AdditionalCharges CURRENCY DEFAULT 0,
                        ChequeAmountFirm1 CURRENCY DEFAULT 0,
                        ChequeAmountFirm2 CURRENCY DEFAULT 0,
                        Status TEXT(20) DEFAULT 'Unpaid',
                        Notes MEMO,
                        CompanyID INTEGER
                    )");

                    // Create BillDetails table
                    ExecuteNonQuery(conn, @"CREATE TABLE BillDetails (
                        BillDetailID COUNTER PRIMARY KEY,
                        BillID INTEGER NOT NULL,
                        ItemID INTEGER NOT NULL,
                        ItemName TEXT(255),
                        Quantity DOUBLE NOT NULL,
                        Rate CURRENCY NOT NULL,
                        Amount CURRENCY NOT NULL,
                        Charges CURRENCY DEFAULT 0,
                        TotalAmount CURRENCY DEFAULT 0,
                        CompanyID INTEGER
                    )");

                    ExecuteNonQuery(conn, @"CREATE TABLE PaymentMaster (
                        PaymentID COUNTER PRIMARY KEY,
                        PartyID INTEGER NOT NULL,
                        BrokerID INTEGER,
                        PaymentDate DATETIME NOT NULL,
                        TotalAmountPaid CURRENCY NOT NULL,
                        PaymentMethod TEXT(50),
                        Reference TEXT(100),
                        CompanyID INTEGER,
                        ChequeAmountFirm1 CURRENCY DEFAULT 0,
                        ChequeAmountFirm2 CURRENCY DEFAULT 0
                    )");
                    // Create PaymentDetails table
                    ExecuteNonQuery(conn, @"CREATE TABLE TransactionLedger (
                        TransactionID COUNTER PRIMARY KEY,
                        PartyID INTEGER NOT NULL,
                        BillID INTEGER,
                        PaymentID INTEGER,
                        TransactionDate DATETIME NOT NULL,
                        TransactionType TEXT(50) NOT NULL,
                        Description MEMO,
                        DebitAmount CURRENCY DEFAULT 0,
                        CreditAmount CURRENCY DEFAULT 0,
                        PaymentMethod TEXT(50),
                        Reference TEXT(100),
                        UserID INTEGER,
                        CompanyID INTEGER
                    )");

                    // Create Settings table
                    ExecuteNonQuery(conn, @"CREATE TABLE Settings (
                        SettingKey TEXT(100) PRIMARY KEY,
                        SettingValue TEXT(255),
                        Description TEXT(255)
                    )");

                    // Insert default settings
                    ExecuteNonQuery(conn, @"INSERT INTO Settings (SettingKey, SettingValue, Description) VALUES ('DefaultInterestDays', '30', 'Default interest days to show on payment screen')");
                    ExecuteNonQuery(conn, @"INSERT INTO Settings (SettingKey, SettingValue, Description) VALUES ('DefaultInterestRate', '18.0', 'Default annual interest rate (%) to show on payment screen')");
                    ExecuteNonQuery(conn, @"INSERT INTO Settings (SettingKey, SettingValue, Description) VALUES ('DefaultDiscountDays', '10.0', 'Default discount days for early payments to show on payment screen')");
                    ExecuteNonQuery(conn, @"INSERT INTO Settings (SettingKey, SettingValue, Description) VALUES ('DefaultDiscountRate', '1.0', 'Default discount rate (%) for early payments to show on payment screen')");
                    ExecuteNonQuery(conn, @"INSERT INTO Settings (SettingKey, SettingValue, Description) VALUES ('DefaultBrokerageRate', '0.0', 'Default brokerage rate (%) for early payments to show on payment screen')");
                    ExecuteNonQuery(conn, @"INSERT INTO UserMaster (Username, PasswordHash, DisplayName, IsAdmin) VALUES ('admin', 'admin', 'Admin', 1)");
                    ExecuteNonQuery(conn, @"INSERT INTO CompanyMaster (CompanyName, Address, Phone) VALUES ('Your Company Name', 'Your Company Address', 'Your Company Phone')");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                // If file creation failed but the file exists, delete it
                try {
                    if (File.Exists(dbPath))
                        File.Delete(dbPath);
                } catch { /* Ignore cleanup errors */ }
                
                System.Windows.Forms.MessageBox.Show(
                    $"Error creating database: {ex.Message}\n\n" +
                    "Please make sure Microsoft Access or the Microsoft Access Database Engine is installed.",
                    "Database Error", 
                    System.Windows.Forms.MessageBoxButtons.OK, 
                    System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }

        // Upgrade database schema if needed

        // Add CompanyID fields to relevant tables
        private static void AddCompanyIDToTables(OleDbConnection conn)
        {
            try
            {
                // Check if PartyMaster table has CompanyID field
                bool hasCompanyID = false;
                DataTable columns = conn.GetSchema("Columns", new string[] { null, null, "PartyMaster", "CompanyID" });
                hasCompanyID = columns.Rows.Count > 0;
                
                if (!hasCompanyID)
                {
                    // Add CompanyID field to PartyMaster
                    ExecuteNonQuery(conn, "ALTER TABLE PartyMaster ADD COLUMN CompanyID INTEGER DEFAULT 0");
                    // System.Windows.Forms.MessageBox.Show("Added CompanyID field to PartyMaster table.", "Database Upgrade",
                    //     System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                
                // Check if ItemMaster table has CompanyID field
                columns = conn.GetSchema("Columns", new string[] { null, null, "ItemMaster", "CompanyID" });
                hasCompanyID = columns.Rows.Count > 0;
                
                if (!hasCompanyID)
                {
                    // Add CompanyID field to ItemMaster
                    ExecuteNonQuery(conn, "ALTER TABLE ItemMaster ADD COLUMN CompanyID INTEGER DEFAULT 0");
                    // System.Windows.Forms.MessageBox.Show("Added CompanyID field to ItemMaster table.", "Database Upgrade",
                    //     System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                
                // Check if BillMaster table has CompanyID field
                columns = conn.GetSchema("Columns", new string[] { null, null, "BillMaster", "CompanyID" });
                hasCompanyID = columns.Rows.Count > 0;
                
                if (!hasCompanyID)
                {
                    // Add CompanyID field to BillMaster
                    ExecuteNonQuery(conn, "ALTER TABLE BillMaster ADD COLUMN CompanyID INTEGER DEFAULT 0");
                    // System.Windows.Forms.MessageBox.Show("Added CompanyID field to BillMaster table.", "Database Upgrade",
                    //     System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                
                // Check if PaymentMaster table has CompanyID field
                columns = conn.GetSchema("Columns", new string[] { null, null, "PaymentMaster", "CompanyID" });
                hasCompanyID = columns.Rows.Count > 0;
                
                if (!hasCompanyID)
                {
                    // Add CompanyID field to PaymentMaster
                    ExecuteNonQuery(conn, "ALTER TABLE PaymentMaster ADD COLUMN CompanyID INTEGER DEFAULT 0");
                    // System.Windows.Forms.MessageBox.Show("Added CompanyID field to PaymentMaster table.", "Database Upgrade",
                    //     System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                
                // Check if BrokerMaster table has CompanyID field
                columns = conn.GetSchema("Columns", new string[] { null, null, "BrokerMaster", "CompanyID" });
                hasCompanyID = columns.Rows.Count > 0;
                
                if (!hasCompanyID)
                {
                    // Add CompanyID field to BrokerMaster
                    ExecuteNonQuery(conn, "ALTER TABLE BrokerMaster ADD COLUMN CompanyID INTEGER DEFAULT 0");
                    // System.Windows.Forms.MessageBox.Show("Added CompanyID field to BrokerMaster table.", "Database Upgrade",
                    //     System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error adding CompanyID fields: {ex.Message}", "Database Error",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        // Update existing database schema to add missing columns
        public static void UpdateDatabaseSchema(OleDbConnection existingConn = null)
        {
            OleDbConnection conn = existingConn ?? GetConnection();
            bool shouldCloseConn = existingConn == null;
            
            try
            {
                if (shouldCloseConn)
                {
                    conn.Open();
                }
                
                // Check if BillDetails table has Charges column
                try
                {
                    using (var cmd = new OleDbCommand("SELECT TOP 1 Charges FROM BillDetails", conn))
                    {
                        cmd.ExecuteScalar();
                    }
                }
                catch
                {
                    // Column doesn't exist, add it
                    try
                    {
                        using (var cmd = new OleDbCommand("ALTER TABLE BillDetails ADD COLUMN Charges CURRENCY DEFAULT 0", conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error adding Charges column: {ex.Message}");
                    }
                }
                
                // Check if BillDetails table has TotalAmount column
                try
                {
                    using (var cmd = new OleDbCommand("SELECT TOP 1 TotalAmount FROM BillDetails", conn))
                    {
                        cmd.ExecuteScalar();
                    }
                }
                catch
                {
                    // Column doesn't exist, add it
                    try
                    {
                        using (var cmd = new OleDbCommand("ALTER TABLE BillDetails ADD COLUMN TotalAmount CURRENCY DEFAULT 0", conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error adding TotalAmount column: {ex.Message}");
                    }
                }
                
                // Note: Fixed issue where OriginalAmount wasn't including item charges
                // New bills will now correctly include item charges in OriginalAmount
                // Existing bills may need manual correction if they have item charges
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating database schema: {ex.Message}");
            }
            finally
            {
                if (shouldCloseConn && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        // Create all necessary tables if they don't exist
        public static void CreateTablesIfNeeded(OleDbConnection existingConn = null)
        {
            OleDbConnection conn = existingConn ?? GetConnection();
            bool shouldCloseConn = existingConn == null;
            
            try
            {
                if (shouldCloseConn)
                {
                    conn.Open();
                }
                
                // Check and create UserMaster table
                var userTableInfo = GetSchema(conn, "Tables", new string[] { null, null, "UserMaster" });
                if (userTableInfo.Rows.Count == 0)
                {
                    // Create UserMaster table
                    ExecuteNonQuery(conn, @"CREATE TABLE UserMaster (
                        UserID COUNTER PRIMARY KEY,
                        Username TEXT(50) UNIQUE,
                        PasswordHash TEXT(255),
                        DisplayName TEXT(100),
                        IsAdmin BIT,
                        IsActive BIT,
                        CreatedOn DATETIME,
                        LastLogin DATETIME
                    )");
                }
                
                // Check and create CompanyMaster table
                var companyTableInfo = GetSchema(conn, "Tables", new string[] { null, null, "CompanyMaster" });
                if (companyTableInfo.Rows.Count == 0)
                {
                    // Create CompanyMaster table
                    ExecuteNonQuery(conn, @"CREATE TABLE CompanyMaster (
                        CompanyID COUNTER PRIMARY KEY,
                        CompanyName TEXT(255),
                        PrintName TEXT(255),
                        Address TEXT(255),
                        City TEXT(100),
                        FinancialYearStart DATETIME,
                        FinancialYearEnd DATETIME,
                        IsActive BIT,
                        CreatedOn DATETIME
                    )");
                }
                
                // Check and create BrokerMaster table
                var brokerTableInfo = GetSchema(conn, "Tables", new string[] { null, null, "BrokerMaster" });
                if (brokerTableInfo.Rows.Count == 0)
                {
                    // Create BrokerMaster table
                    ExecuteNonQuery(conn, @"CREATE TABLE BrokerMaster (
                        BrokerID COUNTER PRIMARY KEY,
                        BrokerName TEXT(255),
                        Phone TEXT(50),
                        Email TEXT(100),
                        CompanyID INTEGER
                    )");
                }
                
                // Check and create PartyMaster table
                var partyTableInfo = GetSchema(conn, "Tables", new string[] { null, null, "PartyMaster" });
                if (partyTableInfo.Rows.Count == 0)
                {
                    // Create PartyMaster table
                    ExecuteNonQuery(conn, @"CREATE TABLE PartyMaster (
                        PartyID COUNTER PRIMARY KEY,
                        PartyName TEXT(255),
                        Address TEXT(255),
                        City TEXT(100),
                        Phone TEXT(50),
                        Email TEXT(100),
                        GSTNo TEXT(50),
                        PAN TEXT(50),
                        OpeningBalance CURRENCY,
                        OpeningBalanceDate DATETIME,
                        CreditDays INTEGER,
                        BrokerID INTEGER,
                        BrokerName TEXT(255),
                        CompanyID INTEGER
                    )");
                }
                
                // Check and create ItemMaster table
                var itemTableInfo = GetSchema(conn, "Tables", new string[] { null, null, "ItemMaster" });
                if (itemTableInfo.Rows.Count == 0)
                {
                    // Create ItemMaster table
                    ExecuteNonQuery(conn, @"CREATE TABLE ItemMaster (
                        ItemID COUNTER PRIMARY KEY,
                        ItemName TEXT(255),
                        Unit TEXT(50),
                        Rate CURRENCY,
                        Charges CURRENCY,
                        StockQuantity DOUBLE,
                        CompanyID INTEGER
                    )");
                }
                
                // Check and create BillMaster table
                var billTableInfo = GetSchema(conn, "Tables", new string[] { null, null, "BillMaster" });
                if (billTableInfo.Rows.Count == 0)
                {
                    // Create BillMaster table
                    ExecuteNonQuery(conn, @"CREATE TABLE BillMaster (
                        BillID COUNTER PRIMARY KEY,
                        BillNo TEXT(50),
                        BillDate DATETIME,
                        DueDate DATETIME,
                        PartyID INTEGER,
                        PartyName TEXT(255),
                        BrokerID INTEGER,
                        BrokerName TEXT(255),
                        TotalAmount CURRENCY,
                        TotalCharges CURRENCY,
                        NetAmount CURRENCY,
                        ChequeAmountFirm1 CURRENCY DEFAULT 0,
                        ChequeAmountFirm2 CURRENCY DEFAULT 0,
                        Notes MEMO,
                        CompanyID INTEGER
                    )");
                }
                
                // Check and create BillDetails table
                var billDetailsTableInfo = GetSchema(conn, "Tables", new string[] { null, null, "BillDetails" });
                if (billDetailsTableInfo.Rows.Count == 0)
                {
                    // Create BillDetails table
                    ExecuteNonQuery(conn, @"CREATE TABLE BillDetails (
                        BillDetailID COUNTER PRIMARY KEY,
                        BillID INTEGER,
                        ItemID INTEGER,
                        ItemName TEXT(255),
                        Quantity DOUBLE,
                        Rate CURRENCY,
                        Amount CURRENCY,
                        Charges CURRENCY,
                        TotalAmount CURRENCY
                    )");
                }
                
                // Check and create PaymentMaster table
                var paymentTableInfo = GetSchema(conn, "Tables", new string[] { null, null, "PaymentMaster" });
                if (paymentTableInfo.Rows.Count == 0)
                {
                    // Create PaymentMaster table
                    ExecuteNonQuery(conn, @"CREATE TABLE PaymentMaster (
                        PaymentID COUNTER PRIMARY KEY,
                        PaymentDate DATETIME,
                        PaymentAmount CURRENCY,
                        PaymentMethod TEXT(50),
                        Reference TEXT(100),
                        Notes MEMO,
                        CompanyID INTEGER
                    )");
                }
                
                // Check and create PaymentDetails table
                var paymentDetailsTableInfo = GetSchema(conn, "Tables", new string[] { null, null, "PaymentDetails" });
                if (paymentDetailsTableInfo.Rows.Count == 0)
                {
                    // Create PaymentDetails table
                    ExecuteNonQuery(conn, @"CREATE TABLE PaymentDetails (
                        PaymentDetailID COUNTER PRIMARY KEY,
                        PaymentID INTEGER,
                        BillID INTEGER,
                        PreviousPaid CURRENCY,
                        BalanceBefore CURRENCY,
                        AllocatedAmount CURRENCY,
                        BalanceAfter CURRENCY
                    )");
                }

                // Check and create Settings table
                var settingsTableInfo = GetSchema(conn, "Tables", new string[] { null, null, "Settings" });
                if (settingsTableInfo.Rows.Count == 0)
                {
                    // Create Settings table
                    ExecuteNonQuery(conn, @"CREATE TABLE Settings (
                        SettingID COUNTER PRIMARY KEY,
                        SettingKey TEXT(100) UNIQUE,
                        SettingValue TEXT(255),
                        Description TEXT(255)
                    )");

                    // Insert default settings
                    ExecuteNonQuery(conn, @"INSERT INTO Settings (SettingKey, SettingValue, Description) VALUES 
                        ('InterestRate', '12.0', 'Annual interest rate percentage for overdue bills')");
                    ExecuteNonQuery(conn, @"INSERT INTO Settings (SettingKey, SettingValue, Description) VALUES 
                        ('DiscountRate', '1.0', 'Discount rate percentage for early payment')");
                    ExecuteNonQuery(conn, @"INSERT INTO Settings (SettingKey, SettingValue, Description) VALUES 
                        ('DefaultCreditDays', 'Your Company Name', 'Company name for reports')");
                    ExecuteNonQuery(conn, @"INSERT INTO Settings (SettingKey, SettingValue, Description) VALUES 
                        ('CompanyAddress', 'Your Company Address', 'Company address for reports')");
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error creating database tables: {ex.Message}", "Database Error",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            finally
            {
                if (shouldCloseConn && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        // Get database schema information
        private static DataTable GetSchema(OleDbConnection conn, string collectionName, string[] restrictionValues)
        {
            return conn.GetSchema(collectionName, restrictionValues);
        }

        // Execute a query and return a DataTable (for connection-specific queries)
        private static DataTable ExecuteQuery(OleDbConnection conn, string sql, params OleDbParameter[] parameters)
        {
            DataTable dt = new DataTable();
            using (OleDbCommand cmd = new OleDbCommand(sql, conn))
            {
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }
                
                using (OleDbDataAdapter adapter = new OleDbDataAdapter(cmd))
                {
                    adapter.Fill(dt);
                }
            }
            return dt;
        }
        
        // Execute a non-query SQL command
        private static int ExecuteNonQuery(OleDbConnection connection, string sql, params OleDbParameter[] parameters)
        {
            using (OleDbCommand cmd = new OleDbCommand(sql, connection))
            {
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                // Check if the connection has an active transaction and use it
                if (connection.State == ConnectionState.Open && 
                    typeof(OleDbConnection).GetProperty("InTransaction", 
                        System.Reflection.BindingFlags.Instance | 
                        System.Reflection.BindingFlags.NonPublic)?.GetValue(connection) != null)
                {
                    // Get the current transaction
                    OleDbTransaction transaction = null;
                    try
                    {
                        // Try to get the current transaction
                        transaction = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Catalogs, null) == null ? 
                            null : connection.BeginTransaction();
                    }
                    catch
                    {
                        // Ignore errors, we'll proceed without a transaction if needed
                    }

                    if (transaction != null)
                    {
                        cmd.Transaction = transaction;
                    }
                }

                return cmd.ExecuteNonQuery();
            }
        }

        // Get a database connection
        public static OleDbConnection GetConnection()
        {
            return new OleDbConnection(_connectionString);
        }
        
        // Execute a query and return a DataTable
        public static DataTable ExecuteQuery(string sql, params OleDbParameter[] parameters)
        {
            DataTable dt = new DataTable();
            
            using (OleDbConnection conn = GetConnection())
            {
                conn.Open();
                using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    
                    try
                    {
                        using (OleDbDataAdapter adapter = new OleDbDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show($"Database query error: {ex.Message}", "Database Error", 
                            System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        throw;
                    }
                }
            }
            
            return dt;
        }
        
        // Execute a non-query SQL command
        public static int ExecuteNonQuery(string sql, params OleDbParameter[] parameters)
        {
            using (OleDbConnection conn = GetConnection())
            {
                conn.Open();
                using (OleDbTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        using (OleDbCommand cmd = new OleDbCommand(sql, conn, transaction))
                        {
                            if (parameters != null)
                            {
                                cmd.Parameters.AddRange(parameters);
                            }
                            
                            int result = cmd.ExecuteNonQuery();
                            transaction.Commit();
                            return result;
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        
        // Execute a scalar query
        public static object ExecuteScalar(string sql, params OleDbParameter[] parameters)
        {
            using (OleDbConnection conn = GetConnection())
            {
                conn.Open();
                using (OleDbTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        using (OleDbCommand cmd = new OleDbCommand(sql, conn, transaction))
                        {
                            if (parameters != null)
                            {
                                cmd.Parameters.AddRange(parameters);
                            }
                            
                            object result = cmd.ExecuteScalar();
                            transaction.Commit();
                            return result;
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Windows.Forms.MessageBox.Show($"Database error: {ex.Message}", "Database Error", 
                            System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        throw;
                    }
                }
            }
        }
        
        // Get next bill number
        public static string GetNextBillNumber()
        {
            try
            {
                string sql = "SELECT MAX(Val(BillNo)) FROM BillMaster";
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
        public static OleDbTransaction BeginTransaction()
        {
            OleDbConnection connection = GetConnection();
            return connection.BeginTransaction();
        }

        /// <summary>
        /// Creates a backup of the current database
        /// </summary>
        /// <param name="backupPath">Optional custom backup path. If null, uses default backup directory.</param>
        /// <returns>True if backup was successful, false otherwise</returns>
        public static bool BackupDatabase(string backupPath = null)
        {
            try
            {
                // Get current database path
                string currentDbPath = CustomDatabasePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", DB_FILENAME);
                
                if (!File.Exists(currentDbPath))
                {
                    throw new FileNotFoundException("Current database file not found.", currentDbPath);
                }

                // Determine backup path
                if (string.IsNullOrEmpty(backupPath))
                {
                    // Create backup directory in user's documents folder
                    string backupDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "SaleBillSystem",
                        "Backups"
                    );
                    
                    if (!Directory.Exists(backupDir))
                    {
                        Directory.CreateDirectory(backupDir);
                    }

                    // Generate backup filename with timestamp
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string backupFileName = $"SaleSystem_Backup_{timestamp}.accdb";
                    backupPath = Path.Combine(backupDir, backupFileName);
                }

                // Ensure backup directory exists
                string backupDirPath = Path.GetDirectoryName(backupPath);
                if (!Directory.Exists(backupDirPath))
                {
                    Directory.CreateDirectory(backupDirPath);
                }

                // Close any existing connections to the database
                // This is important for Access databases to ensure no locks
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // Copy the database file
                File.Copy(currentDbPath, backupPath, true);

                // Verify the backup was created successfully
                if (!File.Exists(backupPath))
                {
                    throw new Exception("Backup file was not created successfully.");
                }

                // Test the backup by trying to open it
                string backupConnectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={backupPath};Persist Security Info=False;";
                using (OleDbConnection testConn = new OleDbConnection(backupConnectionString))
                {
                    testConn.Open();
                    // If we can open the connection, the backup is valid
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Backup failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a list of available backups
        /// </summary>
        /// <returns>List of backup file paths</returns>
        public static List<string> GetAvailableBackups()
        {
            List<string> backups = new List<string>();
            
            try
            {
                string backupDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "SaleBillSystem",
                    "Backups"
                );

                if (Directory.Exists(backupDir))
                {
                    string[] backupFiles = Directory.GetFiles(backupDir, "SaleSystem_Backup_*.accdb");
                    backups.AddRange(backupFiles.OrderByDescending(f => File.GetLastWriteTime(f)));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting backup list: {ex.Message}", ex);
            }

            return backups;
        }

        /// <summary>
        /// Restores a database from a backup file
        /// </summary>
        /// <param name="backupPath">Path to the backup file</param>
        /// <returns>True if restore was successful, false otherwise</returns>
        public static bool RestoreDatabase(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                {
                    throw new FileNotFoundException("Backup file not found.", backupPath);
                }

                // Get current database path
                string currentDbPath = CustomDatabasePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", DB_FILENAME);

                // Test the backup file first
                string backupConnectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={backupPath};Persist Security Info=False;";
                using (OleDbConnection testConn = new OleDbConnection(backupConnectionString))
                {
                    testConn.Open();
                    // If we can open the connection, the backup is valid
                }

                // Close any existing connections
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // Create a backup of the current database before restoring
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string preRestoreBackup = currentDbPath.Replace(".accdb", $"_BeforeRestore_{timestamp}.accdb");
                
                if (File.Exists(currentDbPath))
                {
                    File.Copy(currentDbPath, preRestoreBackup, true);
                }

                // Copy the backup to the current database location
                File.Copy(backupPath, currentDbPath, true);

                // Update the connection string to use the restored database
                _connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={currentDbPath};Persist Security Info=False;";

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Restore failed: {ex.Message}", ex);
            }
        }
    }
} 