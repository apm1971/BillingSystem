using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb; // Changed from SQLite to OleDb
using System.IO;
using System.Linq;
using SaleBillSystem.NET.Models;
// Remove ADOX reference

namespace SaleBillSystem.NET.Data
{
    public class DatabaseManager
    {
        // Database constants
        private const string DB_FILENAME = "SaleSystem.accdb"; // Changed from .db to .accdb
        private const string DB_PASSWORD = "salessystem"; // Database password
        
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
                    string tempConnectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={DB_PASSWORD};Persist Security Info=False;";
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
                
                // Set connection string for Access with password
                _connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={DB_PASSWORD};Persist Security Info=False;";
                
                // Test connection and upgrade database if needed
                using (OleDbConnection connection = new OleDbConnection(_connectionString))
                {
                    try
                    {
                        connection.Open();
                        // UpgradeDatabase(connection);
                    }
                    catch (Exception connEx)
                    {
                        // If connection fails due to password, try to set password on existing database
                        if (connEx.Message.Contains("password") || connEx.Message.Contains("3031"))
                        {
                            System.Windows.Forms.MessageBox.Show(
                                "Existing database found without password protection. Setting password now...", 
                                "Database Security Update", 
                                System.Windows.Forms.MessageBoxButtons.OK, 
                                System.Windows.Forms.MessageBoxIcon.Information);
                            
                            if (SetPasswordOnExistingDatabase(dbPath))
                            {
                                // Try connecting again with password
                                connection.Open();
                                // UpgradeDatabase(connection);
                            }
                            else
                            {
                                throw connEx;
                            }
                        }
                        else
                        {
                            throw connEx;
                        }
                    }
                }
                
                // Create default user if it doesn't exist
                CreateDefaultUserIfNotExists();
                
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error initializing database: {ex.Message}", "Database Error", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Creates a default user 'user' with password 'user123' if it doesn't already exist
        /// </summary>
        private static void CreateDefaultUserIfNotExists()
        {
            try
            {
                // Check if default user already exists
                var existingUser = UserService.GetUserByUsername("user");
                if (existingUser == null)
                {
                    // Create default user
                    var defaultUser = new User
                    {
                        Username = "user",
                        DisplayName = "Default User",
                        IsAdmin = false
                    };
                    
                    // Save user with password 'user123'
                    UserService.SaveUser(defaultUser, "user123");
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Warning: Could not create default user: {ex.Message}", 
                    "User Creation Warning", 
                    System.Windows.Forms.MessageBoxButtons.OK, 
                    System.Windows.Forms.MessageBoxIcon.Warning);
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
                    
                    // Ensure the copied template has the correct password
                    if (!SetPasswordOnExistingDatabase(dbPath))
                    {
                        System.Windows.Forms.MessageBox.Show(
                            "Warning: Could not set password on template database copy.\nThe database may not be password protected.",
                            "Password Warning",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    // Template doesn't exist, create manually using ADOX
                        System.Windows.Forms.MessageBox.Show(
                        "Creating a new password-protected database. This might take a moment.",
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
                        
                        // Create database with password from the start
                        string connStringWithPassword = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Engine Type=5;Jet OLEDB:Database Password={DB_PASSWORD}";
                        catalog.Create(connStringWithPassword);
                        
                        // Ensure password is properly set
                            if (catalog.ActiveConnection != null)
                            {
                                try
                                {
                                // This ensures the password is properly applied
                                    catalog.ActiveConnection.Execute($"ALTER DATABASE PASSWORD [{DB_PASSWORD}] NULL", 0);
                                }
                                catch (Exception passwordEx)
                                {
                                System.Diagnostics.Debug.WriteLine($"Note: Password already set during creation: {passwordEx.Message}");
                                }
                            catalog.ActiveConnection.Close();
                                catalog.ActiveConnection = null;
                            }
                            
                            System.Windows.Forms.MessageBox.Show(
                            $"New database created successfully with password protection.\n\nPassword: {DB_PASSWORD}\n\nThis database can only be opened with this password, even in Microsoft Access.",
                            "Database Created Successfully",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Information);
                        }
                        catch (Exception ex) 
                        {
                        throw new Exception($"Error creating password-protected database with ADOX: {ex.Message}", ex);
                    }
                }
                
                // Connect to the new database and create tables
                string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={DB_PASSWORD};Persist Security Info=False;";
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
                        PartyID INTEGER,
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
                
                // Check if ItemMaster table has SubQuantity column
                try
                {
                    using (var cmd = new OleDbCommand("SELECT TOP 1 SubQuantity FROM ItemMaster", conn))
                    {
                        cmd.ExecuteScalar();
                    }
                }
                catch
                {
                    // Column doesn't exist, add it
                    try
                    {
                        using (var cmd = new OleDbCommand("ALTER TABLE ItemMaster ADD COLUMN SubQuantity TEXT(50)", conn))
                        {
                            cmd.ExecuteNonQuery();
                            System.Windows.Forms.MessageBox.Show(
                                "Added SubQuantity field to ItemMaster table. This represents the sub-quantity unit type (e.g., bag, box).",
                                "Database Update",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error adding SubQuantity column: {ex.Message}");
                    }
                }
                
                // Check if BillDetails table has SubQuantity column
                try
                {
                    using (var cmd = new OleDbCommand("SELECT TOP 1 SubQuantity FROM BillDetails", conn))
                    {
                        cmd.ExecuteScalar();
                    }
                }
                catch
                {
                    // Column doesn't exist, add it
                    try
                    {
                        using (var cmd = new OleDbCommand("ALTER TABLE BillDetails ADD COLUMN SubQuantity DOUBLE DEFAULT 0", conn))
                        {
                            cmd.ExecuteNonQuery();
                            System.Windows.Forms.MessageBox.Show(
                                "Added SubQuantity field to BillDetails table. This represents the number of sub-quantity units.",
                                "Database Update",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error adding SubQuantity column to BillDetails: {ex.Message}");
                    }
                }

                // Check if BillDetails table has SubQuantityUnit column
                try
                {
                    using (var cmd = new OleDbCommand("SELECT TOP 1 SubQuantityUnit FROM BillDetails", conn))
                    {
                        cmd.ExecuteScalar();
                    }
                }
                catch
                {
                    // Column doesn't exist, add it
                    try
                    {
                        using (var cmd = new OleDbCommand("ALTER TABLE BillDetails ADD COLUMN SubQuantityUnit TEXT(50)", conn))
                        {
                            cmd.ExecuteNonQuery();
                            System.Windows.Forms.MessageBox.Show(
                                "Added SubQuantityUnit field to BillDetails table. This represents the sub-quantity unit description (e.g., bag, box).",
                                "Database Update",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error adding SubQuantityUnit column to BillDetails: {ex.Message}");
                    }
                }

                // Check if BillDetails table has TotalCharges column
                try
                {
                    using (var cmd = new OleDbCommand("SELECT TOP 1 TotalCharges FROM BillDetails", conn))
                    {
                        cmd.ExecuteScalar();
                    }
                }
                catch
                {
                    // Column doesn't exist, add it
                    try
                    {
                        using (var cmd = new OleDbCommand("ALTER TABLE BillDetails ADD COLUMN TotalCharges CURRENCY DEFAULT 0", conn))
                        {
                            cmd.ExecuteNonQuery();
                            System.Windows.Forms.MessageBox.Show(
                                "Added TotalCharges field to BillDetails table. This represents the total charges (SubQuantity * Charges).",
                                "Database Update",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error adding TotalCharges column to BillDetails: {ex.Message}");
                    }
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

        /// <summary>
        /// Gets a list of available backups
        /// </summary>
        /// <returns>List of backup file paths</returns>


        /// <summary>
        /// Restores a database from a backup file
        /// </summary>
        /// <param name="backupPath">Path to the backup file</param>
        /// <returns>True if restore was successful, false otherwise</returns>

        /// <summary>
        /// Updates all existing records in various tables to set CompanyID = 1.
        /// This is useful for migrating older data that doesn't have proper CompanyID values.
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        public static bool UpdateAllCompanyIDsToOne()
        {
            try
            {
                using (OleDbConnection conn = GetConnection())
                {
                    conn.Open();
                    
                    // List of tables to update
                    var tablesToUpdate = new[]
                    {
                        "BrokerMaster",
                        "PartyMaster", 
                        "ItemMaster",
                        "BillMaster",
                        "BillDetails",
                        "PaymentMaster",
                        "TransactionLedger"
                    };
                    
                    int totalRowsUpdated = 0;
                    
                    foreach (string tableName in tablesToUpdate)
                    {
                        try
                        {
                            // Check if table exists and has CompanyID column
                            DataTable columns = conn.GetSchema("Columns", new string[] { null, null, tableName, "CompanyID" });
                            if (columns.Rows.Count == 0)
                            {
                                // Table doesn't have CompanyID column, skip it
                                continue;
                            }
                            
                            // Update CompanyID to 1 where it's 0 or NULL
                            string updateSql = $"UPDATE {tableName} SET CompanyID = 1 WHERE CompanyID = 0 OR CompanyID IS NULL";
                            using (OleDbCommand cmd = new OleDbCommand(updateSql, conn))
                            {
                                int rowsAffected = cmd.ExecuteNonQuery();
                                totalRowsUpdated += rowsAffected;
                                
                                // Log the update
                                System.Diagnostics.Debug.WriteLine($"Updated {rowsAffected} rows in {tableName}");
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log error but continue with other tables
                            System.Diagnostics.Debug.WriteLine($"Error updating {tableName}: {ex.Message}");
                        }
                    }
                    
                    System.Windows.Forms.MessageBox.Show(
                        $"Successfully updated CompanyID fields in {tablesToUpdate.Length} tables.\nTotal rows updated: {totalRowsUpdated}", 
                        "CompanyID Update Complete", 
                        System.Windows.Forms.MessageBoxButtons.OK, 
                        System.Windows.Forms.MessageBoxIcon.Information);
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Error updating CompanyID fields: {ex.Message}", 
                    "Database Error", 
                    System.Windows.Forms.MessageBoxButtons.OK, 
                    System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Gets a summary of CompanyID values across all tables for diagnostic purposes.
        /// </summary>
        /// <returns>Dictionary with table names and their CompanyID distribution</returns>
        public static Dictionary<string, Dictionary<int, int>> GetCompanyIDSummary()
        {
            var summary = new Dictionary<string, Dictionary<int, int>>();
            
            try
            {
                using (OleDbConnection conn = GetConnection())
                {
                    conn.Open();
                    
                    var tablesToCheck = new[]
                    {
                        "BrokerMaster",
                        "PartyMaster", 
                        "ItemMaster",
                        "BillMaster",
                        "BillDetails",
                        "PaymentMaster",
                        "TransactionLedger"
                    };
                    
                    foreach (string tableName in tablesToCheck)
                    {
                        try
                        {
                            // Check if table exists and has CompanyID column
                            DataTable columns = conn.GetSchema("Columns", new string[] { null, null, tableName, "CompanyID" });
                            if (columns.Rows.Count == 0)
                            {
                                continue;
                            }
                            
                            // Get CompanyID distribution
                            string countSql = $"SELECT CompanyID, COUNT(*) as Count FROM {tableName} GROUP BY CompanyID";
                            using (OleDbCommand cmd = new OleDbCommand(countSql, conn))
                            {
                                using (OleDbDataReader reader = cmd.ExecuteReader())
                                {
                                    var tableSummary = new Dictionary<int, int>();
                                    
                                    while (reader.Read())
                                    {
                                        int companyId = Convert.ToInt32(reader["CompanyID"]);
                                        int count = Convert.ToInt32(reader["Count"]);
                                        tableSummary[companyId] = count;
                                    }
                                    
                                    summary[tableName] = tableSummary;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error checking {tableName}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Error getting CompanyID summary: {ex.Message}", 
                    "Database Error", 
                    System.Windows.Forms.MessageBoxButtons.OK, 
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
            
            return summary;
        }

        /// <summary>
        /// Updates existing bill details to set default values for newly added fields:
        /// - SubQuantity = 1
        /// - SubQuantityUnit = 'BOX'
        /// - TotalCharges = Charges
        /// First creates the required columns if they don't exist.
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        public static bool UpdateBillDetailsWithDefaultValues()
        {
            try
            {
                using (OleDbConnection conn = GetConnection())
                {
                    conn.Open();
                    
                    // First, check if the required columns exist and create them if they don't
                    var columns = conn.GetSchema("Columns", new string[] { null, null, "BillDetails" });
                    bool hasSubQuantity = columns.AsEnumerable().Any(row => row["COLUMN_NAME"].ToString() == "SubQuantity");
                    bool hasSubQuantityUnit = columns.AsEnumerable().Any(row => row["COLUMN_NAME"].ToString() == "SubQuantityUnit");
                    bool hasTotalCharges = columns.AsEnumerable().Any(row => row["COLUMN_NAME"].ToString() == "TotalCharges");
                    bool hasCharges = columns.AsEnumerable().Any(row => row["COLUMN_NAME"].ToString() == "Charges");
                    
                    // Create missing columns
                    if (!hasSubQuantity)
                    {
                        ExecuteNonQuery(conn, "ALTER TABLE BillDetails ADD COLUMN SubQuantity DOUBLE DEFAULT 1");
                        System.Diagnostics.Debug.WriteLine("Added SubQuantity column to BillDetails table.");
                    }
                    
                    if (!hasSubQuantityUnit)
                    {
                        ExecuteNonQuery(conn, "ALTER TABLE BillDetails ADD COLUMN SubQuantityUnit TEXT(50) DEFAULT 'BOX'");
                        System.Diagnostics.Debug.WriteLine("Added SubQuantityUnit column to BillDetails table.");
                    }
                    
                    if (!hasTotalCharges)
                    {
                        ExecuteNonQuery(conn, "ALTER TABLE BillDetails ADD COLUMN TotalCharges CURRENCY DEFAULT 0");
                        System.Diagnostics.Debug.WriteLine("Added TotalCharges column to BillDetails table.");
                    }
                    
                    // Update all bill details with default values
                    string updateSql = @"
                        UPDATE BillDetails 
                        SET SubQuantity = 1,
                            SubQuantityUnit = 'BOX',
                            TotalCharges = IIF(Charges IS NULL, 0, Charges)
                        WHERE SubQuantity IS NULL 
                           OR SubQuantityUnit IS NULL 
                           OR TotalCharges IS NULL";
                    
                    using (OleDbCommand cmd = new OleDbCommand(updateSql, conn))
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        
                        System.Diagnostics.Debug.WriteLine($"Updated {rowsAffected} bill detail records with default values.");
                        
                        if (rowsAffected > 0)
                        {
                            System.Windows.Forms.MessageBox.Show(
                                $"Successfully updated {rowsAffected} bill detail records with default values:\n" +
                                "• SubQuantity = 1\n" +
                                "• SubQuantityUnit = 'BOX'\n" +
                                "• TotalCharges = Charges",
                                "Bill Details Update Complete",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Information);
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show(
                                "No bill detail records needed updating. All records already have the required values.",
                                "Bill Details Update Complete",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Information);
                        }
                        
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Error updating bill details with default values: {ex.Message}",
                    "Database Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
                
                System.Diagnostics.Debug.WriteLine($"Error updating bill details: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Adds SubQuantity column to ItemMaster table with default value 'BOX' if it doesn't exist
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        public static bool AddSubQuantityToItemMaster()
        {
            try
            {
                using (OleDbConnection conn = GetConnection())
                {
                    conn.Open();
                    
                    // Check if SubQuantity column already exists
                    var columns = conn.GetSchema("Columns", new string[] { null, null, "ItemMaster" });
                    bool hasSubQuantity = columns.AsEnumerable().Any(row => row["COLUMN_NAME"].ToString() == "SubQuantity");
                    
                    if (!hasSubQuantity)
                    {
                        // Add SubQuantity column with default value 'BOX'
                        ExecuteNonQuery(conn, "ALTER TABLE ItemMaster ADD COLUMN SubQuantity TEXT(50) DEFAULT 'BOX'");
                        
                        // Update existing records to set SubQuantity = 'BOX'
                        string updateSql = "UPDATE ItemMaster SET SubQuantity = 'BOX' WHERE SubQuantity IS NULL";
                        using (OleDbCommand cmd = new OleDbCommand(updateSql, conn))
                        {
                            int rowsAffected = cmd.ExecuteNonQuery();
                            
                            System.Diagnostics.Debug.WriteLine($"Added SubQuantity column to ItemMaster table and updated {rowsAffected} existing records.");
                            
                            System.Windows.Forms.MessageBox.Show(
                                $"Successfully added SubQuantity column to ItemMaster table:\n" +
                                "• Column Type: TEXT(50)\n" +
                                "• Default Value: 'BOX'\n" +
                                "• Updated {rowsAffected} existing records",
                                "ItemMaster Update Complete",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Information);
                        }
                        
                        return true;
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show(
                            "SubQuantity column already exists in ItemMaster table.",
                            "ItemMaster Update Complete",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Information);
                        
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Error adding SubQuantity column to ItemMaster: {ex.Message}",
                    "Database Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
                
                System.Diagnostics.Debug.WriteLine($"Error adding SubQuantity to ItemMaster: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Creates the AdvancePayments table if it doesn't exist
        /// </summary>
        public static void CreateAdvancePaymentsTable()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    
                    // Check if table already exists using Access-specific method
                    bool tableExists = false;
                    try
                    {
                        string checkTableSql = "SELECT COUNT(*) FROM AdvancePayments";
                        using (var checkCmd = new OleDbCommand(checkTableSql, conn))
                        {
                            checkCmd.ExecuteScalar();
                            tableExists = true;
                        }
                    }
                    catch
                    {
                        tableExists = false;
                    }
                    
                    if (tableExists)
                    {
                        Console.WriteLine("AdvancePayments table already exists.");
                        return;
                    }
                    
                    // Create the AdvancePayments table with flexible schema
                    string createTableSql = @"
                        CREATE TABLE AdvancePayments (
                            AdvanceID COUNTER PRIMARY KEY,
                            PartyID LONG NULL,
                            BrokerID LONG NULL,
                            PaymentDate DATETIME NOT NULL,
                            Amount CURRENCY NOT NULL,
                            PaymentMethod TEXT(50),
                            Reference TEXT(255),
                            CompanyID LONG NOT NULL,
                            CreatedDate DATETIME DEFAULT NOW(),
                            CONSTRAINT CheckPartyOrBroker CHECK (PartyID IS NOT NULL OR BrokerID IS NOT NULL)
                        )";
                    
                    using (var createCmd = new OleDbCommand(createTableSql, conn))
                    {
                        createCmd.ExecuteNonQuery();
                        Console.WriteLine("AdvancePayments table created successfully.");
                    }
                    
                    // Create indexes for better performance
                    try
                    {
                        string createIndexesSql = @"
                            CREATE INDEX idx_AdvancePayments_PartyID ON AdvancePayments(PartyID);
                            CREATE INDEX idx_AdvancePayments_BrokerID ON AdvancePayments(BrokerID);
                            CREATE INDEX idx_AdvancePayments_PaymentDate ON AdvancePayments(PaymentDate);
                            CREATE INDEX idx_AdvancePayments_CompanyID ON AdvancePayments(CompanyID);
                            CREATE INDEX idx_AdvancePayments_PartyBroker ON AdvancePayments(PartyID, BrokerID);";
                        
                        using (var indexCmd = new OleDbCommand(createIndexesSql, conn))
                        {
                            indexCmd.ExecuteNonQuery();
                            Console.WriteLine("AdvancePayments table indexes created successfully.");
                        }
                    }
                    catch (Exception indexEx)
                    {
                        Console.WriteLine($"Warning: Could not create indexes: {indexEx.Message}");
                        // Continue without indexes - table creation is more important
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating AdvancePayments table: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Creates the AdvanceUtilization table for tracking advance payment usage
        /// </summary>
        public static void CreateAdvanceUtilizationTable()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    
                    // Check if table already exists
                    bool tableExists = false;
                    try
                    {
                        string checkTableSql = "SELECT COUNT(*) FROM AdvanceUtilization";
                        using (var checkCmd = new OleDbCommand(checkTableSql, conn))
                        {
                            checkCmd.ExecuteScalar();
                            tableExists = true;
                        }
                    }
                    catch
                    {
                        tableExists = false;
                    }
                    
                    if (tableExists)
                    {
                        Console.WriteLine("AdvanceUtilization table already exists.");
                        return;
                    }
                    
                    // Create the AdvanceUtilization table
                    string createTableSql = @"
                        CREATE TABLE AdvanceUtilization (
                            UtilizationID COUNTER PRIMARY KEY,
                            AdvanceID LONG NOT NULL,
                            PaymentID LONG NULL,
                            AmountUsed CURRENCY NOT NULL,
                            UtilizedDate DATETIME NOT NULL,
                            PartyID LONG NULL,
                            BrokerID LONG NULL,
                            CompanyID LONG NOT NULL,
                            CreatedDate DATETIME DEFAULT NOW(),
                            CONSTRAINT FK_AdvanceUtilization_Advance FOREIGN KEY (AdvanceID) REFERENCES AdvancePayments(AdvanceID),
                            CONSTRAINT CheckPartyOrBrokerUtilization CHECK (PartyID IS NOT NULL OR BrokerID IS NOT NULL)
                        )";
                    
                    using (var createCmd = new OleDbCommand(createTableSql, conn))
                    {
                        createCmd.ExecuteNonQuery();
                        Console.WriteLine("AdvanceUtilization table created successfully.");
                    }
                    
                    // Create indexes for better performance
                    try
                    {
                        string createIndexesSql = @"
                            CREATE INDEX idx_AdvanceUtilization_AdvanceID ON AdvanceUtilization(AdvanceID);
                            CREATE INDEX idx_AdvanceUtilization_PaymentID ON AdvanceUtilization(PaymentID);
                            CREATE INDEX idx_AdvanceUtilization_PartyID ON AdvanceUtilization(PartyID);
                            CREATE INDEX idx_AdvanceUtilization_BrokerID ON AdvanceUtilization(BrokerID);
                            CREATE INDEX idx_AdvanceUtilization_UtilizedDate ON AdvanceUtilization(UtilizedDate);
                            CREATE INDEX idx_AdvanceUtilization_CompanyID ON AdvanceUtilization(CompanyID);";
                        
                        using (var indexCmd = new OleDbCommand(createIndexesSql, conn))
                        {
                            indexCmd.ExecuteNonQuery();
                            Console.WriteLine("AdvanceUtilization table indexes created successfully.");
                        }
                    }
                    catch (Exception indexEx)
                    {
                        Console.WriteLine($"Warning: Could not create AdvanceUtilization indexes: {indexEx.Message}");
                        // Continue without indexes - table creation is more important
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating AdvanceUtilization table: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Adds the IsAdvancePayment and AdvanceAmount columns to PaymentMaster table if they don't exist
        /// </summary>
        public static void AddAdvanceColumnsToPaymentMaster()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    
                    // Check if IsAdvancePayment column exists using Access-specific method
                    bool column1Exists = false;
                    try
                    {
                        string checkColumn1Sql = "SELECT IsAdvancePayment FROM PaymentMaster WHERE 1=0";
                        using (var checkCmd = new OleDbCommand(checkColumn1Sql, conn))
                        {
                            checkCmd.ExecuteScalar();
                            column1Exists = true;
                        }
                    }
                    catch
                    {
                        column1Exists = false;
                    }
                    
                    if (!column1Exists)
                    {
                        // Add IsAdvancePayment column
                        string addColumn1Sql = "ALTER TABLE PaymentMaster ADD COLUMN IsAdvancePayment BIT DEFAULT 0";
                        using (var addCmd = new OleDbCommand(addColumn1Sql, conn))
                        {
                            addCmd.ExecuteNonQuery();
                            Console.WriteLine("IsAdvancePayment column added to PaymentMaster table.");
                        }
                    }
                    
                    // Check if AdvanceAmount column exists
                    bool column2Exists = false;
                    try
                    {
                        string checkColumn2Sql = "SELECT AdvanceAmount FROM PaymentMaster WHERE 1=0";
                        using (var checkCmd = new OleDbCommand(checkColumn2Sql, conn))
                        {
                            checkCmd.ExecuteScalar();
                            column2Exists = true;
                        }
                    }
                    catch
                    {
                        column2Exists = false;
                    }
                    
                    if (!column2Exists)
                    {
                        // Add AdvanceAmount column
                        string addColumn2Sql = "ALTER TABLE PaymentMaster ADD COLUMN AdvanceAmount CURRENCY DEFAULT 0";
                        using (var addCmd = new OleDbCommand(addColumn2Sql, conn))
                        {
                            addCmd.ExecuteNonQuery();
                            Console.WriteLine("AdvanceAmount column added to PaymentMaster table.");
                        }
                    }
                    bool column3Exists = false;
                    try
                    {
                        string checkColumn3Sql = "SELECT AdvanceUsed FROM PaymentMaster WHERE 1=0";
                        using (var checkCmd = new OleDbCommand(checkColumn3Sql, conn))
                        {
                            checkCmd.ExecuteScalar();
                            column3Exists = true;
                        }
                    }
                    catch
                    {
                        column3Exists = false;
                    }
                    if (!column3Exists)
                    {
                        // Add AdvanceUsed column
                        string addColumn3Sql = "ALTER TABLE PaymentMaster ADD COLUMN AdvanceUsed CURRENCY DEFAULT 0";
                        using (var addCmd = new OleDbCommand(addColumn3Sql, conn))
                        {
                            addCmd.ExecuteNonQuery();
                            Console.WriteLine("AdvanceUsed column added to PaymentMaster table.");
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding advance columns to PaymentMaster: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Sets password on an existing database that doesn't have one
        /// </summary>
        /// <param name="dbPath">Path to the database file</param>
        /// <returns>True if password was set successfully, false otherwise</returns>
        public static bool SetPasswordOnExistingDatabase(string dbPath)
        {
            try
            {
                // First check if database already has our password
                try
                {
                    string connectionStringWithPassword = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={DB_PASSWORD};Persist Security Info=False;";
                    using (OleDbConnection passwordConn = new OleDbConnection(connectionStringWithPassword))
                    {
                        passwordConn.Open();
                        passwordConn.Close();
                        // If this succeeds, database already has our password
                        System.Windows.Forms.MessageBox.Show($"Database already has the correct password: {DB_PASSWORD}", 
                            "Database Password Already Set", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                        return true;
                    }
                }
                catch
                {
                    // Database doesn't have our password, continue to set it
                }
                
                // Try to connect without password to see if database has no password
                string connectionStringWithoutPassword = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Persist Security Info=False;";
                
                using (OleDbConnection testConn = new OleDbConnection(connectionStringWithoutPassword))
                {
                    try
                    {
                        testConn.Open();
                        // If we can open without password, it means database has no password
                        testConn.Close();
                        
                        // Now set the password using ADOX
                        var catalogType = Type.GetTypeFromProgID("ADOX.Catalog");
                        if (catalogType == null)
                        {
                            System.Windows.Forms.MessageBox.Show("ADOX.Catalog not found. Cannot set database password.\nPlease ensure Microsoft Access Database Engine is installed.", "Error", 
                                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                            return false;
                        }
                        
                        dynamic catalog = Activator.CreateInstance(catalogType);
                        catalog.ActiveConnection = connectionStringWithoutPassword;
                        
                        // Set the password (from NULL to our password)
                        catalog.ActiveConnection.Execute($"ALTER DATABASE PASSWORD [{DB_PASSWORD}] NULL", 0);
                        
                        catalog.ActiveConnection.Close();
                        catalog.ActiveConnection = null;
                        
                        System.Windows.Forms.MessageBox.Show($"Password has been successfully set on the existing database.\n\nPassword: {DB_PASSWORD}\n\nThis database is now password protected and can only be opened with this password, even in Microsoft Access.", 
                            "Database Password Set Successfully", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                        
                        return true;
                    }
                    catch (Exception innerEx)
                    {
                        // If connection fails, database might already have a different password
                        if (innerEx.Message.Contains("password") || innerEx.Message.Contains("3031") || innerEx.Message.Contains("Could not decrypt file"))
                        {
                            // Try common passwords or ask user
                            var result = System.Windows.Forms.MessageBox.Show(
                                $"Database appears to have a password, but it's not '{DB_PASSWORD}'.\n\n" +
                                "Would you like to try changing it from a common old password?\n\n" +
                                "Click Yes to try 'SaleSystem@2024' (old password)\n" +
                                "Click No to skip password setting for now",
                                "Database Password Issue", 
                                System.Windows.Forms.MessageBoxButtons.YesNo, 
                                System.Windows.Forms.MessageBoxIcon.Question);
                            
                            if (result == System.Windows.Forms.DialogResult.Yes)
                            {
                                return TryChangePasswordFromOld(dbPath, "SaleSystem@2024");
                            }
                            else
                            {
                                System.Windows.Forms.MessageBox.Show(
                                    "Password setting skipped. The database will continue to use its existing password.\n" +
                                    "You may need to manually change the password in Microsoft Access if needed.",
                                    "Password Setting Skipped", 
                                    System.Windows.Forms.MessageBoxButtons.OK, 
                                    System.Windows.Forms.MessageBoxIcon.Warning);
                                return false;
                            }
                        }
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error setting database password: {ex.Message}\n\nThe application will continue to work, but the database may not be password protected.", "Database Error", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }
        
        /// <summary>
        /// Attempts to change password from an old password to the new one
        /// </summary>
        /// <param name="dbPath">Path to the database file</param>
        /// <param name="oldPassword">The old password to try</param>
        /// <returns>True if password was changed successfully, false otherwise</returns>
        private static bool TryChangePasswordFromOld(string dbPath, string oldPassword)
        {
            try
            {
                // Try to connect with old password
                string connectionStringWithOldPassword = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={oldPassword};Persist Security Info=False;";
                
                using (OleDbConnection oldConn = new OleDbConnection(connectionStringWithOldPassword))
                {
                    oldConn.Open();
                    oldConn.Close();
                    
                    // If we can connect with old password, change it to new password
                    var catalogType = Type.GetTypeFromProgID("ADOX.Catalog");
                    if (catalogType == null)
                    {
                        System.Windows.Forms.MessageBox.Show("ADOX.Catalog not found. Cannot change database password.", "Error", 
                            System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        return false;
                    }
                    
                    dynamic catalog = Activator.CreateInstance(catalogType);
                    catalog.ActiveConnection = connectionStringWithOldPassword;
                    
                    // Change password from old to new
                    catalog.ActiveConnection.Execute($"ALTER DATABASE PASSWORD [{DB_PASSWORD}] [{oldPassword}]", 0);
                    
                    catalog.ActiveConnection.Close();
                    catalog.ActiveConnection = null;
                    
                    System.Windows.Forms.MessageBox.Show($"Password has been successfully changed from '{oldPassword}' to '{DB_PASSWORD}'.\n\nThe database is now using the new password.", 
                        "Database Password Changed Successfully", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Could not change password from '{oldPassword}': {ex.Message}", "Password Change Failed", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                return false;
            }
        }

        /// <summary>
        /// Gets information about the current database password status
        /// </summary>
        /// <returns>String describing the password status</returns>
        public static string GetDatabasePasswordStatus()
        {
            try
            {
                string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", DB_FILENAME);
                if (CustomDatabasePath != null)
                {
                    dbPath = CustomDatabasePath;
                }
                
                if (!File.Exists(dbPath))
                {
                    return "Database file does not exist.";
                }
                
                // Try to connect with our password
                            try
                            {
                                string connectionStringWithPassword = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={DB_PASSWORD};Persist Security Info=False;";
                                using (OleDbConnection passwordConn = new OleDbConnection(connectionStringWithPassword))
                                {
                                    passwordConn.Open();
                                    passwordConn.Close();
                        return $"Database is password protected with the correct password: '{DB_PASSWORD}'";
                                }
                            }
                            catch
                            {
                    // Try to connect without password
                    try
                    {
                        string connectionStringWithoutPassword = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Persist Security Info=False;";
                        using (OleDbConnection testConn = new OleDbConnection(connectionStringWithoutPassword))
                        {
                            testConn.Open();
                            testConn.Close();
                            return "Database exists but has no password protection.";
                        }
                    }
                    catch
                    {
                        return $"Database exists but has a different password (not '{DB_PASSWORD}').";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error checking database password status: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Forces password setting on the current database (for manual troubleshooting)
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        public static bool ForceSetDatabasePassword()
        {
            try
            {
                string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", DB_FILENAME);
                if (CustomDatabasePath != null)
                {
                    dbPath = CustomDatabasePath;
                }
                
                if (!File.Exists(dbPath))
                {
                    System.Windows.Forms.MessageBox.Show("Database file does not exist.", "Error", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
                
                return SetPasswordOnExistingDatabase(dbPath);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error forcing password set: {ex.Message}", "Error", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }
        
        /// <summary>
        /// Tests the database password functionality (for debugging purposes)
        /// </summary>
        /// <returns>Test results as a string</returns>
        public static string TestDatabasePasswordFunctionality()
        {
            var results = new System.Text.StringBuilder();
            results.AppendLine("=== Database Password Functionality Test ===");
            results.AppendLine($"Expected Password: '{DB_PASSWORD}'");
            results.AppendLine();
            
            try
            {
                string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", DB_FILENAME);
                if (CustomDatabasePath != null)
                {
                    dbPath = CustomDatabasePath;
                }
                
                results.AppendLine($"Database Path: {dbPath}");
                results.AppendLine($"Database Exists: {File.Exists(dbPath)}");
                
                if (File.Exists(dbPath))
                {
                    results.AppendLine();
                    results.AppendLine("Password Status: " + GetDatabasePasswordStatus());
                    
                    // Test connection with password
                    try
                    {
                        string connectionStringWithPassword = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={DB_PASSWORD};Persist Security Info=False;";
                        using (OleDbConnection passwordConn = new OleDbConnection(connectionStringWithPassword))
                        {
                            passwordConn.Open();
                            results.AppendLine("✓ Connection with password: SUCCESS");
                            passwordConn.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        results.AppendLine($"✗ Connection with password: FAILED - {ex.Message}");
                    }
                    
                    // Test connection without password
                    try
                    {
                        string connectionStringWithoutPassword = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Persist Security Info=False;";
                        using (OleDbConnection testConn = new OleDbConnection(connectionStringWithoutPassword))
                        {
                            testConn.Open();
                            results.AppendLine("⚠ Connection without password: SUCCESS (Database not protected!)");
                            testConn.Close();
                        }
                    }
                    catch
                    {
                        results.AppendLine("✓ Connection without password: FAILED (Database is protected)");
                    }
                }
                else
                {
                    results.AppendLine("Database file does not exist - will be created on first run.");
                }
            }
            catch (Exception ex)
            {
                results.AppendLine($"Test Error: {ex.Message}");
            }
            
            results.AppendLine();
            results.AppendLine("=== Test Complete ===");
            return results.ToString();
        }

        /// <summary>
        /// Creates a backup of the current database to a specified location
        /// </summary>
        /// <param name="backupPath">Full path where the backup should be saved (including filename)</param>
        /// <returns>True if backup was successful, false otherwise</returns>
        public static bool CreateDatabaseBackup(string backupPath)
        {
            try
            {
                string currentDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", DB_FILENAME);
                if (CustomDatabasePath != null)
                {
                    currentDbPath = CustomDatabasePath;
                }
                
                if (!File.Exists(currentDbPath))
                {
                    System.Windows.Forms.MessageBox.Show("Database file not found. Cannot create backup.", "Backup Error", 
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return false;
                }
                
                // Ensure backup directory exists
                string backupDir = Path.GetDirectoryName(backupPath);
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }
                
                // Copy the database file to backup location
                File.Copy(currentDbPath, backupPath, true);
                
                // Verify the backup was created successfully
                if (File.Exists(backupPath))
                {
                    FileInfo originalFile = new FileInfo(currentDbPath);
                    FileInfo backupFile = new FileInfo(backupPath);
                    
                    if (originalFile.Length == backupFile.Length)
                    {
                        System.Windows.Forms.MessageBox.Show(
                            $"Database backup created successfully!\n\n" +
                            $"Backup Location: {backupPath}\n" +
                            $"Backup Size: {FormatFileSize(backupFile.Length)}\n" +
                            $"Backup Date: {backupFile.CreationTime:yyyy-MM-dd HH:mm:ss}",
                            "Backup Successful", 
                            System.Windows.Forms.MessageBoxButtons.OK, 
                            System.Windows.Forms.MessageBoxIcon.Information);
                        return true;
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Backup file size doesn't match original. Backup may be corrupted.", "Backup Warning", 
                            System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                        return false;
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Backup file was not created. Unknown error occurred.", "Backup Error", 
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Error creating database backup: {ex.Message}\n\n" +
                    "Please ensure:\n" +
                    "• The backup location is accessible\n" +
                    "• You have write permissions to the backup folder\n" +
                    "• The database is not currently being used by another process",
                    "Backup Error", 
                    System.Windows.Forms.MessageBoxButtons.OK, 
                    System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }
        
        /// <summary>
        /// Restores a database from a backup file
        /// </summary>
        /// <param name="backupPath">Path to the backup file to restore from</param>
        /// <returns>True if restore was successful, false otherwise</returns>
        public static bool RestoreDatabaseFromBackup(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                {
                    System.Windows.Forms.MessageBox.Show("Backup file not found.", "Restore Error", 
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return false;
                }
                
                string currentDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", DB_FILENAME);
                if (CustomDatabasePath != null)
                {
                    currentDbPath = CustomDatabasePath;
                }
                
                // Confirm with user before restoring
                var result = System.Windows.Forms.MessageBox.Show(
                    $"This will replace your current database with the backup.\n\n" +
                    $"Current Database: {currentDbPath}\n" +
                    $"Backup File: {backupPath}\n\n" +
                    "ALL CURRENT DATA WILL BE LOST!\n\n" +
                    "Are you sure you want to continue?",
                    "Confirm Database Restore", 
                    System.Windows.Forms.MessageBoxButtons.YesNo, 
                    System.Windows.Forms.MessageBoxIcon.Warning,
                    System.Windows.Forms.MessageBoxDefaultButton.Button2);
                
                if (result != System.Windows.Forms.DialogResult.Yes)
                {
                    return false;
                }
                
                // Create a backup of current database before restoring
                string tempBackup = currentDbPath + ".temp_backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                try
                {
                    File.Copy(currentDbPath, tempBackup, true);
                }
                catch
                {
                    // Continue even if temp backup fails
                }
                
                // Copy backup file to current database location
                File.Copy(backupPath, currentDbPath, true);
                
                // Verify the restore was successful by testing database connection
                try
                {
                    string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={currentDbPath};Jet OLEDB:Database Password={DB_PASSWORD};Persist Security Info=False;";
                    using (OleDbConnection testConn = new OleDbConnection(connectionString))
                    {
                        testConn.Open();
                        testConn.Close();
                    }
                    
                    // Clean up temp backup if restore was successful
                    try
                    {
                        if (File.Exists(tempBackup))
                            File.Delete(tempBackup);
                    }
                    catch { }
                    
                    System.Windows.Forms.MessageBox.Show(
                        $"Database restored successfully from backup!\n\n" +
                        $"Restored from: {backupPath}\n" +
                        $"Please restart the application to ensure all data is loaded correctly.",
                        "Restore Successful", 
                        System.Windows.Forms.MessageBoxButtons.OK, 
                        System.Windows.Forms.MessageBoxIcon.Information);
                    
                    return true;
                }
                catch (Exception testEx)
                {
                    // Restore failed, try to restore from temp backup
                    try
                    {
                        if (File.Exists(tempBackup))
                        {
                            File.Copy(tempBackup, currentDbPath, true);
                            File.Delete(tempBackup);
                        }
                    }
                    catch { }
                    
                    System.Windows.Forms.MessageBox.Show(
                        $"Database restore failed. The backup file may be corrupted or incompatible.\n\n" +
                        $"Error: {testEx.Message}\n\n" +
                        "Your original database has been restored.",
                        "Restore Failed", 
                        System.Windows.Forms.MessageBoxButtons.OK, 
                        System.Windows.Forms.MessageBoxIcon.Error);
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Error restoring database: {ex.Message}",
                    "Restore Error", 
                    System.Windows.Forms.MessageBoxButtons.OK, 
                    System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }
        
        /// <summary>
        /// Formats file size in human-readable format
        /// </summary>
        /// <param name="bytes">File size in bytes</param>
        /// <returns>Formatted file size string</returns>
        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
        
        /// <summary>
        /// Gets information about the current database
        /// </summary>
        /// <returns>Database information string</returns>
        public static string GetDatabaseInfo()
        {
            try
            {
                string currentDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", DB_FILENAME);
                if (CustomDatabasePath != null)
                {
                    currentDbPath = CustomDatabasePath;
                }
                
                if (!File.Exists(currentDbPath))
                {
                    return "Database file not found.";
                }
                
                FileInfo dbFile = new FileInfo(currentDbPath);
                
                var info = new System.Text.StringBuilder();
                info.AppendLine("=== Database Information ===");
                info.AppendLine($"Database Path: {currentDbPath}");
                info.AppendLine($"File Size: {FormatFileSize(dbFile.Length)}");
                info.AppendLine($"Created: {dbFile.CreationTime:yyyy-MM-dd HH:mm:ss}");
                info.AppendLine($"Last Modified: {dbFile.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
                info.AppendLine($"Password Protected: {(GetDatabasePasswordStatus().Contains("correct password") ? "Yes" : "No")}");
                info.AppendLine();
                
                // Get table count
                try
                {
                    using (var conn = GetConnection())
                    {
                        conn.Open();
                        var tables = conn.GetSchema("Tables");
                        int userTableCount = 0;
                        foreach (System.Data.DataRow row in tables.Rows)
                        {
                            string tableName = row["TABLE_NAME"].ToString();
                            string tableType = row["TABLE_TYPE"].ToString();
                            if (tableType == "TABLE" && !tableName.StartsWith("MSys"))
                            {
                                userTableCount++;
                            }
                        }
                        info.AppendLine($"User Tables: {userTableCount}");
                    }
                }
                catch (Exception ex)
                {
                    info.AppendLine($"Table Count: Error - {ex.Message}");
                }
                
                return info.ToString();
            }
            catch (Exception ex)
            {
                return $"Error getting database info: {ex.Message}";
            }
        }

        /// <summary>
        /// Initializes all advance payment related database structures
        /// </summary>
        public static void InitializeAdvancePaymentSystem()
        {
            try
            {
                Console.WriteLine("Initializing advance payment system...");
                
                // Create the AdvancePayments table
                CreateAdvancePaymentsTable();
                
                // Create the AdvanceUtilization table
                CreateAdvanceUtilizationTable();
                
                // Add advance columns to PaymentMaster table
                AddAdvanceColumnsToPaymentMaster();
                 
                 // Update existing database constraints if needed
                 UpdateAdvanceUtilizationTable();
                
                Console.WriteLine("Advance payment system initialized successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing advance payment system: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets the total advance amount for a party (including broker-specific advances)
        /// </summary>
        public static decimal GetPartyAdvanceBalance(int partyId, int? brokerId = null, int companyId = 1)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    
                    string sql;
                    if (brokerId.HasValue)
                    {
                        // Get advance balance for specific party + broker combination
                        sql = @"
                            SELECT IIF(SUM(Amount) IS NULL, 0, SUM(Amount)) 
                            FROM AdvancePayments 
                            WHERE PartyID = ? AND BrokerID = ? AND CompanyID = ?";
                    }
                    else
                    {
                        // Get total advance balance for party (including no-broker advances)
                        sql = @"
                            SELECT IIF(SUM(Amount) IS NULL, 0, SUM(Amount)) 
                            FROM AdvancePayments 
                            WHERE PartyID = ? AND CompanyID = ?";
                    }
                    
                    using (var cmd = new OleDbCommand(sql, conn))
                    {
                        cmd.Parameters.Add(new OleDbParameter("PartyID", partyId));
                        if (brokerId.HasValue)
                        {
                            cmd.Parameters.Add(new OleDbParameter("BrokerID", brokerId.Value));
                        }
                        cmd.Parameters.Add(new OleDbParameter("CompanyID", companyId));
                        
                        object result = cmd.ExecuteScalar();
                        return result != null ? Convert.ToDecimal(result) : 0m;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting party advance balance: {ex.Message}");
                return 0m;
            }
        }

        /// <summary>
        /// Gets the total advance amount for a broker (including party-specific advances)
        /// </summary>
        public static decimal GetBrokerAdvanceBalance(int brokerId, int? partyId = null, int companyId = 1)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    
                    string sql;
                    if (partyId.HasValue)
                    {
                        // Get advance balance for specific broker + party combination
                        sql = @"
                            SELECT IIF(SUM(Amount) IS NULL, 0, SUM(Amount)) 
                            FROM AdvancePayments 
                            WHERE BrokerID = ? AND PartyID = ? AND CompanyID = ?";
                    }
                    else
                    {
                        // Get total advance balance for broker (including no-party advances)
                        sql = @"
                            SELECT IIF(SUM(Amount) IS NULL, 0, SUM(Amount)) 
                            FROM AdvancePayments 
                            WHERE BrokerID = ? AND CompanyID = ?";
                    }
                    
                    using (var cmd = new OleDbCommand(sql, conn))
                    {
                        cmd.Parameters.Add(new OleDbParameter("BrokerID", brokerId));
                        if (partyId.HasValue)
                        {
                            cmd.Parameters.Add(new OleDbParameter("PartyID", partyId.Value));
                        }
                        cmd.Parameters.Add(new OleDbParameter("CompanyID", companyId));
                        
                        object result = cmd.ExecuteScalar();
                        return result != null ? Convert.ToDecimal(result) : 0m;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting broker advance balance: {ex.Message}");
                return 0m;
            }
        }

        /// <summary>
        /// Gets all advance payments with flexible filtering
        /// </summary>
        // public static List<AdvancePayment> GetAdvancePayments(int? partyId = null, int? brokerId = null, int companyId = 1)
        // {
        //     var advancePayments = new List<AdvancePayment>();
            
        //     try
        //     {
        //         using (var conn = GetConnection())
        //         {
        //             conn.Open();
                    
        //             string sql = @"
        //                 SELECT AdvanceID, PartyID, BrokerID, PaymentDate, Amount, 
        //                        PaymentMethod, Reference, CompanyID, CreatedDate
        //                 FROM AdvancePayments 
        //                 WHERE CompanyID = ?";
                    
        //             var parameters = new List<OleDbParameter>
        //             {
        //                 new OleDbParameter("CompanyID", companyId)
        //             };
                    
        //             if (partyId.HasValue)
        //             {
        //                 sql += " AND PartyID = ?";
        //                 parameters.Add(new OleDbParameter("PartyID", partyId.Value));
        //             }
                    
        //             if (brokerId.HasValue)
        //             {
        //                 sql += " AND BrokerID = ?";
        //                 parameters.Add(new OleDbParameter("BrokerID", brokerId.Value));
        //             }
                    
        //             sql += " ORDER BY PaymentDate DESC";
                    
        //             using (var cmd = new OleDbCommand(sql, conn))
        //             {
        //                 foreach (var param in parameters)
        //                 {
        //                     cmd.Parameters.Add(param);
        //                 }
                        
        //                 using (var reader = cmd.ExecuteReader())
        //                 {
        //                     while (reader.Read())
        //                     {
        //                         advancePayments.Add(new AdvancePayment
        //                         {
        //                             AdvanceID = Convert.ToInt32(reader["AdvanceID"]),
        //                             PartyID = reader["PartyID"] != DBNull.Value ? Convert.ToInt32(reader["PartyID"]) : null,
        //                             BrokerID = reader["BrokerID"] != DBNull.Value ? Convert.ToInt32(reader["BrokerID"]) : null,
        //                             PaymentDate = Convert.ToDateTime(reader["PaymentDate"]),
        //                             Amount = Convert.ToDecimal(reader["Amount"]),
        //                             PaymentMethod = reader["PaymentMethod"]?.ToString() ?? "",
        //                             Reference = reader["Reference"]?.ToString() ?? "",
        //                             CompanyID = Convert.ToInt32(reader["CompanyID"]),
        //                             CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
        //                         });
        //                     }
        //                 }
        //             }
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Error getting advance payments: {ex.Message}");
        //     }
            
        //     return advancePayments;
        // }

        /// <summary>
        /// Gets advance payment summary for reporting
        /// </summary>
        public static Dictionary<string, decimal> GetAdvancePaymentSummary(int companyId = 1)
        {
            var summary = new Dictionary<string, decimal>();
            
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    
                    // Get total advances by type
                    string sql = @"
                        SELECT 
                            CASE 
                                WHEN PartyID IS NOT NULL AND BrokerID IS NOT NULL THEN 'Party+Broker'
                                WHEN PartyID IS NOT NULL THEN 'Party Only'
                                WHEN BrokerID IS NOT NULL THEN 'Broker Only'
                                ELSE 'Unknown'
                            END AS AdvanceType,
                            IIF(SUM(Amount) IS NULL, 0, SUM(Amount)) AS TotalAmount
                        FROM AdvancePayments 
                        WHERE CompanyID = ?
                        GROUP BY 
                            CASE 
                                WHEN PartyID IS NOT NULL AND BrokerID IS NOT NULL THEN 'Party+Broker'
                                WHEN PartyID IS NOT NULL THEN 'Party Only'
                                WHEN BrokerID IS NOT NULL THEN 'Broker Only'
                                ELSE 'Unknown'
                            END";
                    
                    using (var cmd = new OleDbCommand(sql, conn))
                    {
                        cmd.Parameters.Add(new OleDbParameter("CompanyID", companyId));
                        
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string advanceType = reader["AdvanceType"].ToString();
                                decimal amount = Convert.ToDecimal(reader["TotalAmount"]);
                                summary[advanceType] = amount;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting advance payment summary: {ex.Message}");
            }
            
            return summary;
        }

        /// <summary>
        /// Adds the ChequeAmountFirm1 and ChequeAmountFirm2 columns to AdvancePayments table if they don't exist
        /// </summary>
        public static bool AddChequeAmountColumnsToAdvancePayments()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    
                    bool changesNeeded = false;
                    
                    // Check if ChequeAmountFirm1 column exists
                    bool firm1Exists = false;
                    try
                    {
                        string checkColumn1Sql = "SELECT ChequeAmountFirm1 FROM AdvancePayments WHERE 1=0";
                        using (var checkCmd = new OleDbCommand(checkColumn1Sql, conn))
                        {
                            checkCmd.ExecuteScalar();
                            firm1Exists = true;
                        }
                    }
                    catch
                    {
                        firm1Exists = false;
                    }
                    
                    if (!firm1Exists)
                    {
                        // Add ChequeAmountFirm1 column
                        string addColumn1Sql = "ALTER TABLE AdvancePayments ADD COLUMN ChequeAmountFirm1 CURRENCY DEFAULT 0";
                        using (var addCmd = new OleDbCommand(addColumn1Sql, conn))
                        {
                            addCmd.ExecuteNonQuery();
                            Console.WriteLine("ChequeAmountFirm1 column added to AdvancePayments table.");
                            changesNeeded = true;
                        }
                    }
                    
                    // Check if ChequeAmountFirm2 column exists
                    bool firm2Exists = false;
                    try
                    {
                        string checkColumn2Sql = "SELECT ChequeAmountFirm2 FROM AdvancePayments WHERE 1=0";
                        using (var checkCmd = new OleDbCommand(checkColumn2Sql, conn))
                        {
                            checkCmd.ExecuteScalar();
                            firm2Exists = true;
                        }
                    }
                    catch
                    {
                        firm2Exists = false;
                    }
                    
                    if (!firm2Exists)
                    {
                        // Add ChequeAmountFirm2 column
                        string addColumn2Sql = "ALTER TABLE AdvancePayments ADD COLUMN ChequeAmountFirm2 CURRENCY DEFAULT 0";
                        using (var addCmd = new OleDbCommand(addColumn2Sql, conn))
                        {
                            addCmd.ExecuteNonQuery();
                            Console.WriteLine("ChequeAmountFirm2 column added to AdvancePayments table.");
                            changesNeeded = true;
                        }
                    }
                    
                    if (changesNeeded)
                    {
                        System.Windows.Forms.MessageBox.Show(
                            "Successfully added cheque amount fields to AdvancePayments table:\n" +
                            "• ChequeAmountFirm1 (CURRENCY)\n" +
                            "• ChequeAmountFirm2 (CURRENCY)\n\n" +
                            "These fields will be used when payment method is 'Cheque' to track firm-specific amounts.",
                            "Database Update Complete",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Information);
                    }
                    else
                    {
                        // System.Windows.Forms.MessageBox.Show(
                        //     "Cheque amount columns already exist in AdvancePayments table.",
                        //     "Database Update Complete",
                        //     System.Windows.Forms.MessageBoxButtons.OK,
                        //     System.Windows.Forms.MessageBoxIcon.Information);
                    }
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Error adding cheque amount columns to AdvancePayments: {ex.Message}",
                    "Database Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
                
                Console.WriteLine($"Error adding cheque columns to AdvancePayments: {ex.Message}");
                return false;
            }
        }

        public static void changePartyIdToNullable()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    
                    string sql = "ALTER TABLE PaymentMaster ALTER COLUMN PartyID INT NULL";
                    using (var cmd = new OleDbCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("PartyID column in PaymentMaster table changed to nullable.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error changing PartyID column to nullable: {ex.Message}");
                throw;
            }
        }

        public static void UpdateAdvanceUtilizationTable()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    
                    // Check if AdvanceUtilization table exists
                    bool tableExists = false;
                    try
                    {
                        string checkTableSql = "SELECT COUNT(*) FROM AdvanceUtilization";
                        using (var checkCmd = new OleDbCommand(checkTableSql, conn))
                        {
                            checkCmd.ExecuteScalar();
                            tableExists = true;
                        }
                    }
                    catch
                    {
                        tableExists = false;
                    }
                    
                    if (tableExists)
                    {
                        // Table exists, try to make PaymentID nullable
                        try
                        {
                            string sql = "ALTER TABLE AdvanceUtilization ALTER COLUMN PaymentID INT NULL";
                            using (var cmd = new OleDbCommand(sql, conn))
                            {
                                cmd.ExecuteNonQuery();
                                Console.WriteLine("PaymentID column in AdvanceUtilization table changed to nullable.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Warning: Could not make PaymentID nullable: {ex.Message}");
                            
                            // If that fails, try to drop and recreate the table without the constraint
                            try
                            {
                                Console.WriteLine("Attempting to recreate AdvanceUtilization table without PaymentMaster constraint...");
                                
                                // Drop the existing table
                                string dropSql = "DROP TABLE AdvanceUtilization";
                                using (var dropCmd = new OleDbCommand(dropSql, conn))
                                {
                                    dropCmd.ExecuteNonQuery();
                                    Console.WriteLine("Dropped existing AdvanceUtilization table.");
                                }
                                
                                // Recreate without the problematic constraint
                                string createTableSql = @"
                                    CREATE TABLE AdvanceUtilization (
                                        UtilizationID COUNTER PRIMARY KEY,
                                        AdvanceID LONG NOT NULL,
                                        PaymentID LONG NULL,
                                        AmountUsed CURRENCY NOT NULL,
                                        UtilizedDate DATETIME NOT NULL,
                                        PartyID LONG NULL,
                                        BrokerID LONG NULL,
                                        CompanyID LONG NOT NULL,
                                        CreatedDate DATETIME DEFAULT NOW(),
                                        CONSTRAINT FK_AdvanceUtilization_Advance FOREIGN KEY (AdvanceID) REFERENCES AdvancePayments(AdvanceID),
                                        CONSTRAINT CheckPartyOrBrokerUtilization CHECK (PartyID IS NOT NULL OR BrokerID IS NOT NULL)
                                    )";
                                
                                using (var createCmd = new OleDbCommand(createTableSql, conn))
                                {
                                    createCmd.ExecuteNonQuery();
                                    Console.WriteLine("Recreated AdvanceUtilization table without PaymentMaster constraint.");
                                }
                            }
                            catch (Exception recreateEx)
                            {
                                Console.WriteLine($"Error recreating table: {recreateEx.Message}");
                                // Continue - this is not critical for basic functionality
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating AdvanceUtilization table: {ex.Message}");
                // Don't throw - this is not critical for basic functionality
            }
        }
    }
} 

