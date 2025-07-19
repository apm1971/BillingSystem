using System;
using System.Data;
using System.Data.OleDb; // Changed from SQLite to OleDb
using System.IO;
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

        // Set a custom database path
        public static bool SetDatabasePath(string newPath)
        {
            try
            {
                if (string.IsNullOrEmpty(newPath))
                {
                    return false;
                }

                // Validate the new database path
                if (!File.Exists(newPath))
                {
                    // If file doesn't exist, check if we can create it
                    string dir = Path.GetDirectoryName(newPath);
                    if (!Directory.Exists(dir))
                    {
                        try
                        {
                            Directory.CreateDirectory(dir);
                        }
                        catch
                        {
                            return false;
                        }
                    }
                    
                    // Try to create a new database at the specified path
                    if (!CreateDatabase(newPath))
                    {
                        return false;
                    }
                }
                else
                {
                    // Test if the file is a valid Access database
                    try
                    {
                        if (!ValidateAndLoadExistingDatabase(newPath))
                        {
                            System.Windows.Forms.MessageBox.Show("The selected file is not a valid database or is missing required tables.", 
                                "Invalid Database", System.Windows.Forms.MessageBoxButtons.OK, 
                                System.Windows.Forms.MessageBoxIcon.Error);
                            return false;
                        }
                    }
                    catch
                    {
                        System.Windows.Forms.MessageBox.Show("The selected file is not a valid Access database.", 
                            "Invalid Database", System.Windows.Forms.MessageBoxButtons.OK, 
                            System.Windows.Forms.MessageBoxIcon.Error);
                        return false;
                    }
                }

                // Store the new path in settings
                using (OleDbConnection conn = GetConnection())
                {
                    conn.Open();
                    
                    // Check if the setting already exists
                    using (OleDbCommand cmd = new OleDbCommand("SELECT COUNT(*) FROM Settings WHERE SettingKey = 'DatabasePath'", conn))
                    {
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        
                        if (count > 0)
                        {
                            // Update existing setting
                            using (OleDbCommand updateCmd = new OleDbCommand(
                                "UPDATE Settings SET SettingValue = ?, Description = ? WHERE SettingKey = 'DatabasePath'", conn))
                            {
                                updateCmd.Parameters.AddWithValue("SettingValue", newPath);
                                updateCmd.Parameters.AddWithValue("Description", "Custom database path");
                                updateCmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // Insert new setting
                            using (OleDbCommand insertCmd = new OleDbCommand(
                                "INSERT INTO Settings (SettingKey, SettingValue, Description) VALUES (?, ?, ?)", conn))
                            {
                                insertCmd.Parameters.AddWithValue("SettingKey", "DatabasePath");
                                insertCmd.Parameters.AddWithValue("SettingValue", newPath);
                                insertCmd.Parameters.AddWithValue("Description", "Custom database path");
                                insertCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
                
                CustomDatabasePath = newPath;
                
                // Show message to restart application
                System.Windows.Forms.MessageBox.Show(
                    "Database path has been changed. The application will now restart to apply the changes.", 
                    "Database Path Changed", 
                    System.Windows.Forms.MessageBoxButtons.OK, 
                    System.Windows.Forms.MessageBoxIcon.Information);
                
                // Restart the application
                System.Windows.Forms.Application.Restart();
                Environment.Exit(0);
                
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error setting database path: {ex.Message}", 
                    "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }

        // Validate and load an existing database
        private static bool ValidateAndLoadExistingDatabase(string dbPath)
        {
            try
            {
                string testConn = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Persist Security Info=False;";
                using (OleDbConnection conn = new OleDbConnection(testConn))
                {
                    conn.Open();
                    
                    // Check if essential tables exist
                    var requiredTables = new[] { "Settings", "ItemMaster", "PartyMaster", "BillMaster", "BrokerMaster" };
                    bool allTablesExist = true;
                    
                    foreach (var tableName in requiredTables)
                    {
                        var tableInfo = GetSchema(conn, "Tables", new string[] { null, null, tableName });
                        if (tableInfo.Rows.Count == 0)
                        {
                            allTablesExist = false;
                            break;
                        }
                    }
                    
                    if (!allTablesExist)
                    {
                        // Ask user if they want to initialize the database with required tables
                        if (System.Windows.Forms.MessageBox.Show(
                            "The selected database is missing some required tables. Would you like to initialize it?",
                            "Initialize Database",
                            System.Windows.Forms.MessageBoxButtons.YesNo,
                            System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                        {
                            // Create required tables
                            CreateTablesIfNeeded(conn);
                            return true;
                        }
                        return false;
                    }
                    
                    return true;
                }
            }
            catch
            {
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
                    
                    // Create BrokerMaster table
                    ExecuteNonQuery(conn, @"CREATE TABLE BrokerMaster (
                        BrokerID COUNTER PRIMARY KEY,
                        BrokerName TEXT(255),
                        Phone TEXT(50),
                        Email TEXT(100)
                    )");

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
                        BrokerName TEXT(255)
                    )");

                    // Create ItemMaster table
                    ExecuteNonQuery(conn, @"CREATE TABLE ItemMaster (
                        ItemID COUNTER PRIMARY KEY,
                        ItemCode TEXT(50),
                        ItemName TEXT(255),
                        Unit TEXT(50),
                        Rate CURRENCY,
                        Charges CURRENCY,
                        StockQuantity DOUBLE
                    )");

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
                        Notes MEMO
                    )");

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

                    // Create PaymentMaster table
                    ExecuteNonQuery(conn, @"CREATE TABLE PaymentMaster (
                        PaymentID COUNTER PRIMARY KEY,
                        PaymentDate DATETIME,
                        PaymentAmount CURRENCY,
                        PaymentMethod TEXT(50),
                        Reference TEXT(100),
                        Notes MEMO
                    )");

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
                        ('CompanyName', 'Your Company Name', 'Company name for reports')");
                    ExecuteNonQuery(conn, @"INSERT INTO Settings (SettingKey, SettingValue, Description) VALUES 
                        ('CompanyAddress', 'Your Company Address', 'Company address for reports')");
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
        private static void UpgradeDatabase(OleDbConnection conn)
        {
            try
            {
                // Check for BrokerMaster table
                var tableInfo = GetSchema(conn, "Tables", new string[] { null, null, "BrokerMaster" });

                // Add other database upgrade checks here if needed
                
                // Check if UserMaster table exists
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
                
                // Check if CompanyMaster table exists
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

                // Add CompanyID fields to existing tables
                AddCompanyIDToTables(conn);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error upgrading database: {ex.Message}", "Database Error",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

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
                    System.Windows.Forms.MessageBox.Show("Added CompanyID field to PartyMaster table.", "Database Upgrade",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                
                // Check if ItemMaster table has CompanyID field
                columns = conn.GetSchema("Columns", new string[] { null, null, "ItemMaster", "CompanyID" });
                hasCompanyID = columns.Rows.Count > 0;
                
                if (!hasCompanyID)
                {
                    // Add CompanyID field to ItemMaster
                    ExecuteNonQuery(conn, "ALTER TABLE ItemMaster ADD COLUMN CompanyID INTEGER DEFAULT 0");
                    System.Windows.Forms.MessageBox.Show("Added CompanyID field to ItemMaster table.", "Database Upgrade",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                
                // Check if BillMaster table has CompanyID field
                columns = conn.GetSchema("Columns", new string[] { null, null, "BillMaster", "CompanyID" });
                hasCompanyID = columns.Rows.Count > 0;
                
                if (!hasCompanyID)
                {
                    // Add CompanyID field to BillMaster
                    ExecuteNonQuery(conn, "ALTER TABLE BillMaster ADD COLUMN CompanyID INTEGER DEFAULT 0");
                    System.Windows.Forms.MessageBox.Show("Added CompanyID field to BillMaster table.", "Database Upgrade",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                
                // Check if PaymentMaster table has CompanyID field
                columns = conn.GetSchema("Columns", new string[] { null, null, "PaymentMaster", "CompanyID" });
                hasCompanyID = columns.Rows.Count > 0;
                
                if (!hasCompanyID)
                {
                    // Add CompanyID field to PaymentMaster
                    ExecuteNonQuery(conn, "ALTER TABLE PaymentMaster ADD COLUMN CompanyID INTEGER DEFAULT 0");
                    System.Windows.Forms.MessageBox.Show("Added CompanyID field to PaymentMaster table.", "Database Upgrade",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                
                // Check if BrokerMaster table has CompanyID field
                columns = conn.GetSchema("Columns", new string[] { null, null, "BrokerMaster", "CompanyID" });
                hasCompanyID = columns.Rows.Count > 0;
                
                if (!hasCompanyID)
                {
                    // Add CompanyID field to BrokerMaster
                    ExecuteNonQuery(conn, "ALTER TABLE BrokerMaster ADD COLUMN CompanyID INTEGER DEFAULT 0");
                    System.Windows.Forms.MessageBox.Show("Added CompanyID field to BrokerMaster table.", "Database Upgrade",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error adding CompanyID fields: {ex.Message}", "Database Error",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
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
                        ('CompanyName', 'Your Company Name', 'Company name for reports')");
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
            OleDbConnection conn = GetConnection();
            conn.Open();
            return conn.BeginTransaction();
        }
    }
} 