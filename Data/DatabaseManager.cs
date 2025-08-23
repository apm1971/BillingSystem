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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding advance columns to PaymentMaster: {ex.Message}");
                throw;
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
                
                // Add advance columns to PaymentMaster table
                AddAdvanceColumnsToPaymentMaster();
                
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
                        System.Windows.Forms.MessageBox.Show(
                            "Cheque amount columns already exist in AdvancePayments table.",
                            "Database Update Complete",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Information);
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
    }
} 