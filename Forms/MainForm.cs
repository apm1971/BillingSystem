using System;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;
using System.Drawing; // Added for Font

namespace SaleBillSystem.NET.Forms
{
    public partial class MainForm : Form
    {
        // This field keeps track of the currently displayed UserControl
        private UserControl _currentControl = null;

        public MainForm()
        {
            InitializeComponent();
            this.FormClosing += MainForm_FormClosing;
            this.KeyPreview = true; // Enable keyboard shortcuts at form level
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Show login screen first
            ShowLoginScreen();
        }

        #region Keyboard Shortcuts

        /// <summary>
        /// Handles keyboard shortcuts for quick navigation
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Only process shortcuts if main menu is visible (user is logged in)
            if (!mainMenuStrip.Visible)
                return base.ProcessCmdKey(ref msg, keyData);

            switch (keyData)
            {
                case Keys.Control | Keys.B:
                    // Ctrl + B: New Bill
                    ShowControl(new SaleBillUserControl());
                    return true;

                case Keys.Control | Keys.L:
                    // Ctrl + L: Bill List
                    ShowControl(new BillListUserControl());
                    return true;

                case Keys.Control | Keys.A:
                    // Ctrl + A: Advance Payment Entry
                    ShowControl(new AdvancePaymentEntryControl());
                    return true;

                case Keys.Control | Keys.E:
                    // Ctrl + E: Settlement Entry
                    ShowControl(new PaymentEntryControl());
                    return true;

                case Keys.Control | Keys.R:
                    // Ctrl + R: Payment Reports
                    ShowControl(new PaymentReportsControl());
                    return true;

                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        #endregion

        #region Login Flow Management

        private void ShowLoginScreen()
        {
            mainMenuStrip.Visible = false;

            // The loginControl is already on the form from the Designer
            loginControl.Visible = true;
            loginControl.Dock = DockStyle.Fill;
            loginControl.LoginSuccess += OnLoginSuccess;
        }

        private void OnLoginSuccess(object sender, User authenticatedUser)
        {
            // Set the global current user
            Program.CurrentUser = authenticatedUser;
            
            // Initialize permission manager with the authenticated user
            PermissionManager.InitializeUserSession(authenticatedUser);

            // Hide the login control
            loginControl.Visible = false;

            // Once login is successful, show the main UI
            ShowMainApplicationUI();
        }

        #endregion

        #region Main UI and Control Management

        private void ShowMainApplicationUI()
        {
            mainMenuStrip.Visible = true;
            InitializeMainMenu();
            this.Text = $"{Program.APP_NAME} - Main Dashboard";
        }

        /// <summary>
        /// This is the core method for the "single-page" feel.
        /// It closes any current control and displays the new one.
        /// </summary>
        public void ShowControl(UserControl controlToShow)
        {
            // Close any control that is currently open
            CloseCurrentControl();

            // Set up the new control
            _currentControl = controlToShow;
            _currentControl.Dock = DockStyle.Fill;

            // Add the new control to the form's controls
            this.Controls.Add(_currentControl);

            // Bring the new control to the front to be visible
            _currentControl.BringToFront();

            // Subscribe to the control's close request event
            if (_currentControl is PartyMasterUserControl partyControl)
            {
                partyControl.PartySelected += (s, e) => CloseCurrentControl();
            }
            // Add similar handlers for your other controls (ItemMasterControl, etc.)
            // else if (_currentControl is ItemMasterControl itemControl) { ... }
        }

        /// <summary>
        /// Removes the currently active UserControl from the screen.
        /// </summary>
        private void CloseCurrentControl()
        {
            if (_currentControl != null)
            {
                this.Controls.Remove(_currentControl);
                _currentControl.Dispose();
                _currentControl = null;
            }
        }

        private void InitializeMainMenu()
        {
            // Clear existing items to prevent duplicates if called multiple times
            mainMenuStrip.Items.Clear();

            // Create a bold, larger font for menu items
            var menuFont = new Font("Segoe UI", 10.5f, FontStyle.Bold);

            // === MASTERS MENU ===
            var mastersMenu = new ToolStripMenuItem("&Masters");
            mastersMenu.Font = menuFont;

            var partyMasterItem = new ToolStripMenuItem("&Party Master");
            partyMasterItem.Font = menuFont;
            partyMasterItem.Click += (s, e) => { ShowControl(new PartyMasterUserControl()); };

            var itemMasterItem = new ToolStripMenuItem("&Item Master");
            itemMasterItem.Font = menuFont;
            itemMasterItem.Click += (s, e) => { ShowControl(new ItemMasterUserControl()); };

            var brokerMasterItem = new ToolStripMenuItem("&Broker Master");
            brokerMasterItem.Font = menuFont;
            brokerMasterItem.Click += (s, e) => { ShowControl(new BrokerMasterUserControl()); };

            var godownMasterItem = new ToolStripMenuItem("&Godown Master");
            godownMasterItem.Font = menuFont;
            godownMasterItem.Click += (s, e) => { ShowControl(new GodownMasterUserControl()); };

            var godownItemMasterItem = new ToolStripMenuItem("Godown &Item Master");
            godownItemMasterItem.Font = menuFont;
            godownItemMasterItem.Click += (s, e) => { ShowControl(new GodownItemMasterUserControl()); };

            mastersMenu.DropDownItems.Add(partyMasterItem);
            mastersMenu.DropDownItems.Add(itemMasterItem);
            mastersMenu.DropDownItems.Add(brokerMasterItem);
            mastersMenu.DropDownItems.Add(new ToolStripSeparator());
            mastersMenu.DropDownItems.Add(godownMasterItem);
            mastersMenu.DropDownItems.Add(godownItemMasterItem);

            // === BILLS MENU ===
            var billsMenu = new ToolStripMenuItem("&Bills");
            billsMenu.Font = menuFont;

            var newBillItem = new ToolStripMenuItem("&New Bill");
            newBillItem.Font = menuFont;
            newBillItem.ShortcutKeys = Keys.Control | Keys.B;
            newBillItem.ShowShortcutKeys = true;
            newBillItem.Click += (s, e) => { ShowControl(new SaleBillUserControl()); };

            var billledgerItem = new ToolStripMenuItem("&Bill Ledger");
            billledgerItem.Font = menuFont;
            billledgerItem.Click += (s, e) => { ShowControl(new BillLedgerControl()); };

            var billListItem = new ToolStripMenuItem("Bill &List");
            billListItem.Font = menuFont;
            billListItem.ShortcutKeys = Keys.Control | Keys.L;
            billListItem.ShowShortcutKeys = true;
            billListItem.Click += (s, e) => { ShowControl(new BillListUserControl()); };

            billsMenu.DropDownItems.Add(newBillItem);
            billsMenu.DropDownItems.Add(billledgerItem);
            billsMenu.DropDownItems.Add(billListItem);

            // === PAYMENTS MENU ===
            var paymentsMenu = new ToolStripMenuItem("&Payments");
            paymentsMenu.Font = menuFont;

            var paymentEntryItem = new ToolStripMenuItem("&Settlement Entry");
            paymentEntryItem.Font = menuFont;
            paymentEntryItem.ShortcutKeys = Keys.Control | Keys.E;
            paymentEntryItem.ShowShortcutKeys = true;
            paymentEntryItem.Click += (s, e) => { ShowControl(new PaymentEntryControl()); };

            // var paymentListItem = new ToolStripMenuItem("&Payment List");
            // paymentListItem.Font = menuFont;
            // paymentListItem.Click += (s, e) => { ShowControl(new PaymentListControl()); };

            var advancePaymentItem = new ToolStripMenuItem("&Payment Entry");
            advancePaymentItem.Font = menuFont;
            advancePaymentItem.ShortcutKeys = Keys.Control | Keys.A;
            advancePaymentItem.ShowShortcutKeys = true;
            advancePaymentItem.Click += (s, e) => { ShowControl(new AdvancePaymentEntryControl()); };

            paymentsMenu.DropDownItems.Add(paymentEntryItem);
            // paymentsMenu.DropDownItems.Add(paymentListItem);
            paymentsMenu.DropDownItems.Add(advancePaymentItem);

            // === REPORTS MENU ===
            var reportsMenu = new ToolStripMenuItem("&Reports");
            reportsMenu.Font = menuFont;

            var paymentReportsItem = new ToolStripMenuItem("&Payment Reports");
            paymentReportsItem.Font = menuFont;
            paymentReportsItem.ShortcutKeys = Keys.Control | Keys.R;
            paymentReportsItem.ShowShortcutKeys = true;
            paymentReportsItem.Click += (s, e) => { ShowControl(new PaymentReportsControl()); };

            reportsMenu.DropDownItems.Add(paymentReportsItem);

            // === GODOWN TRANSACTIONS MENU ===
            var godownMenu = new ToolStripMenuItem("&Godown");
            godownMenu.Font = menuFont;

            var openingStockItem = new ToolStripMenuItem("&Opening Stock Entry");
            openingStockItem.Font = menuFont;
            openingStockItem.Click += (s, e) => { ShowControl(new GodownOpeningStockControl()); };

            var transactionEntryItem = new ToolStripMenuItem("&Transaction Entry");
            transactionEntryItem.Font = menuFont;
            transactionEntryItem.Click += (s, e) => { ShowControl(new GodownTransactionControl()); };

            var stockReportItem = new ToolStripMenuItem("&Stock Report");
            stockReportItem.Font = menuFont;
            stockReportItem.Click += (s, e) => { ShowControl(new GodownStockReportControl()); };

            var stockLedgerItem = new ToolStripMenuItem("Stock &Ledger");
            stockLedgerItem.Font = menuFont;
            stockLedgerItem.Click += (s, e) => { ShowControl(new GodownStockLedgerControl()); };

            godownMenu.DropDownItems.Add(openingStockItem);
            godownMenu.DropDownItems.Add(transactionEntryItem);
            godownMenu.DropDownItems.Add(stockReportItem);
            godownMenu.DropDownItems.Add(stockLedgerItem);

            // === UTILITIES MENU ===
            var utilitiesMenu = new ToolStripMenuItem("&Utilities");
            utilitiesMenu.Font = menuFont;

            // User Management - Only visible to admin users
            if (Program.CurrentUser != null && Program.CurrentUser.IsAdmin)
            {
                var userManagementItem = new ToolStripMenuItem("&User Management");
                userManagementItem.Font = menuFont;
                userManagementItem.Click += (s, e) => { ShowControl(new UserManagementControl()); };
                utilitiesMenu.DropDownItems.Add(userManagementItem);

                var databasePathItem = new ToolStripMenuItem("&Database Path Settings");
                databasePathItem.Font = menuFont;
                databasePathItem.Click += (s, e) => { ShowControl(new DatabasePathSettingsControl()); };
                utilitiesMenu.DropDownItems.Add(databasePathItem);
            }

            // var migrateItem = new ToolStripMenuItem("&Migrate Database");
            // migrateItem.Font = menuFont;
            // migrateItem.Click += (s, e) => { RunDatabaseMigration(); };

            // var passwordTestItem = new ToolStripMenuItem("&Test Database Password");
            // passwordTestItem.Font = menuFont;
            // passwordTestItem.Click += (s, e) => { TestDatabasePassword(); };

            // var forcePasswordItem = new ToolStripMenuItem("&Force Set Password");
            // forcePasswordItem.Font = menuFont;
            // forcePasswordItem.Click += (s, e) => { ForceSetPassword(); };

            var backupDatabaseItem = new ToolStripMenuItem("&Backup Database");
            backupDatabaseItem.Font = menuFont;
            backupDatabaseItem.Click += (s, e) => { BackupDatabase(); };

            var restoreDatabaseItem = new ToolStripMenuItem("&Restore Database");
            restoreDatabaseItem.Font = menuFont;
            restoreDatabaseItem.Click += (s, e) => { RestoreDatabase(); };

            var databaseInfoItem = new ToolStripMenuItem("Database &Info");
            databaseInfoItem.Font = menuFont;
            databaseInfoItem.Click += (s, e) => { ShowDatabaseInfo(); };

            // License Generator removed from distributed app - use separate LicenseGeneratorTool

            // utilitiesMenu.DropDownItems.Add(migrateItem);
            utilitiesMenu.DropDownItems.Add(new ToolStripSeparator());
            utilitiesMenu.DropDownItems.Add(backupDatabaseItem);
            utilitiesMenu.DropDownItems.Add(restoreDatabaseItem);
            utilitiesMenu.DropDownItems.Add(databaseInfoItem);
            // utilitiesMenu.DropDownItems.Add(passwordTestItem);
            // utilitiesMenu.DropDownItems.Add(forcePasswordItem);

            // Add all top-level menus to the main menu strip in the correct order
            mainMenuStrip.Items.Add(mastersMenu);
            mainMenuStrip.Items.Add(billsMenu);
            mainMenuStrip.Items.Add(paymentsMenu);
            mainMenuStrip.Items.Add(reportsMenu);
            mainMenuStrip.Items.Add(godownMenu);
            mainMenuStrip.Items.Add(utilitiesMenu);

            // Add Transactions, Reports, etc. menus here
        }

        #endregion

        #region Form Closing Confirmation

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Show confirmation dialog
            DialogResult result = MessageBox.Show(
                "Are you sure you want to exit the Sale Bill System?\n\nAny unsaved data will be lost.",
                "Confirm Exit",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2); // Default to "No"

            // If user clicks "No", cancel the closing
            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        #endregion

        #region Database Password Testing

        /// <summary>
        /// Tests the database password functionality and shows detailed results
        /// </summary>
        private void TestDatabasePassword()
        {
            try
            {
                string testResults = DatabaseManager.TestDatabasePasswordFunctionality();
                
                // Create a form to show the detailed results
                var resultForm = new Form
                {
                    Text = "Database Password Test Results",
                    Size = new Size(600, 400),
                    StartPosition = FormStartPosition.CenterParent,
                    ShowIcon = false,
                    MaximizeBox = false,
                    MinimizeBox = false
                };
                
                var textBox = new TextBox
                {
                    Text = testResults,
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Both,
                    Dock = DockStyle.Fill,
                    Font = new Font("Consolas", 9f)
                };
                
                var closeButton = new Button
                {
                    Text = "Close",
                    Size = new Size(100, 30),
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                    DialogResult = DialogResult.OK
                };
                closeButton.Location = new Point(resultForm.Width - closeButton.Width - 20, resultForm.Height - closeButton.Height - 50);
                
                resultForm.Controls.Add(textBox);
                resultForm.Controls.Add(closeButton);
                resultForm.AcceptButton = closeButton;
                
                resultForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running password test: {ex.Message}", "Test Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// Forces password setting on the current database
        /// </summary>
        private void ForceSetPassword()
        {
            try
            {
                var result = MessageBox.Show(
                    "This will attempt to set the password 'salessystem' on your database.\n\n" +
                    "Are you sure you want to continue?",
                    "Force Set Password",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    bool success = DatabaseManager.ForceSetDatabasePassword();
                    
                    if (success)
                    {
                        MessageBox.Show("Password setting completed successfully!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Password setting failed. Check the error messages for details.", "Failed", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error forcing password set: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Database Backup and Restore

        /// <summary>
        /// Creates a backup of the database with user-selected location
        /// </summary>
        private void BackupDatabase()
        {
            try
            {
                // Generate default backup filename with timestamp
                string defaultFileName = $"SaleSystem_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.accdb";
                
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Title = "Save Database Backup";
                    saveDialog.Filter = "Access Database Files (*.accdb)|*.accdb|All Files (*.*)|*.*";
                    saveDialog.DefaultExt = "accdb";
                    saveDialog.FileName = defaultFileName;
                    saveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Show progress message
                        var progressForm = new Form
                        {
                            Text = "Creating Backup",
                            Size = new Size(300, 100),
                            StartPosition = FormStartPosition.CenterParent,
                            FormBorderStyle = FormBorderStyle.FixedDialog,
                            MaximizeBox = false,
                            MinimizeBox = false,
                            ShowIcon = false
                        };
                        
                        var progressLabel = new Label
                        {
                            Text = "Creating database backup...",
                            Dock = DockStyle.Fill,
                            TextAlign = ContentAlignment.MiddleCenter
                        };
                        
                        progressForm.Controls.Add(progressLabel);
                        progressForm.Show();
                        Application.DoEvents();
                        
                        try
                        {
                            bool success = DatabaseManager.CreateDatabaseBackup(saveDialog.FileName);
                            progressForm.Close();
                            
                            if (!success)
                            {
                                MessageBox.Show("Backup creation failed. Please check the error messages.", "Backup Failed", 
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                        finally
                        {
                            if (!progressForm.IsDisposed)
                                progressForm.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initiating backup: {ex.Message}", "Backup Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// Restores the database from a user-selected backup file
        /// </summary>
        private void RestoreDatabase()
        {
            try
            {
                using (OpenFileDialog openDialog = new OpenFileDialog())
                {
                    openDialog.Title = "Select Database Backup to Restore";
                    openDialog.Filter = "Access Database Files (*.accdb)|*.accdb|All Files (*.*)|*.*";
                    openDialog.DefaultExt = "accdb";
                    openDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    
                    if (openDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Show additional warning
                        var result = MessageBox.Show(
                            "IMPORTANT WARNING:\n\n" +
                            "Restoring from a backup will completely replace your current database.\n" +
                            "ALL CURRENT DATA WILL BE PERMANENTLY LOST!\n\n" +
                            "Make sure you have a backup of your current data before proceeding.\n\n" +
                            "Do you want to continue with the restore?",
                            "Final Restore Warning", 
                            MessageBoxButtons.YesNo, 
                            MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button2);
                        
                        if (result == DialogResult.Yes)
                        {
                            // Show progress message
                            var progressForm = new Form
                            {
                                Text = "Restoring Database",
                                Size = new Size(300, 100),
                                StartPosition = FormStartPosition.CenterParent,
                                FormBorderStyle = FormBorderStyle.FixedDialog,
                                MaximizeBox = false,
                                MinimizeBox = false,
                                ShowIcon = false
                            };
                            
                            var progressLabel = new Label
                            {
                                Text = "Restoring database from backup...",
                                Dock = DockStyle.Fill,
                                TextAlign = ContentAlignment.MiddleCenter
                            };
                            
                            progressForm.Controls.Add(progressLabel);
                            progressForm.Show();
                            Application.DoEvents();
                            
                            try
                            {
                                bool success = DatabaseManager.RestoreDatabaseFromBackup(openDialog.FileName);
                                progressForm.Close();
                                
                                if (success)
                                {
                                    // Ask user if they want to restart the application
                                    var restartResult = MessageBox.Show(
                                        "Database restored successfully!\n\n" +
                                        "It's recommended to restart the application to ensure all data is loaded correctly.\n\n" +
                                        "Do you want to restart the application now?",
                                        "Restart Application?", 
                                        MessageBoxButtons.YesNo, 
                                        MessageBoxIcon.Question);
                                    
                                    if (restartResult == DialogResult.Yes)
                                    {
                                        Application.Restart();
                                    }
                                }
                            }
                            finally
                            {
                                if (!progressForm.IsDisposed)
                                    progressForm.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initiating restore: {ex.Message}", "Restore Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// Shows detailed information about the current database
        /// </summary>
        private void ShowDatabaseInfo()
        {
            try
            {
                string databaseInfo = DatabaseManager.GetDatabaseInfo();
                
                // Create a form to show the database information
                var infoForm = new Form
                {
                    Text = "Database Information",
                    Size = new Size(600, 400),
                    StartPosition = FormStartPosition.CenterParent,
                    ShowIcon = false,
                    MaximizeBox = false,
                    MinimizeBox = false
                };
                
                var textBox = new TextBox
                {
                    Text = databaseInfo,
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Both,
                    Dock = DockStyle.Fill,
                    Font = new Font("Consolas", 9f)
                };
                
                var buttonPanel = new Panel
                {
                    Height = 40,
                    Dock = DockStyle.Bottom
                };
                
                var closeButton = new Button
                {
                    Text = "Close",
                    Size = new Size(100, 30),
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                    DialogResult = DialogResult.OK
                };
                closeButton.Location = new Point(buttonPanel.Width - closeButton.Width - 10, 5);
                
                var backupButton = new Button
                {
                    Text = "Create Backup",
                    Size = new Size(120, 30),
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Left
                };
                backupButton.Location = new Point(10, 5);
                backupButton.Click += (s, e) => 
                {
                    infoForm.Close();
                    BackupDatabase();
                };
                
                buttonPanel.Controls.Add(closeButton);
                buttonPanel.Controls.Add(backupButton);
                
                infoForm.Controls.Add(textBox);
                infoForm.Controls.Add(buttonPanel);
                infoForm.AcceptButton = closeButton;
                
                infoForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting database information: {ex.Message}", "Database Info Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Database Migration

        /// <summary>
        /// Runs database migration functions to update CompanyIDs and add default values
        /// </summary>
        private void RunDatabaseMigration()
        {
            try
            {
                // Show confirmation dialog
                DialogResult result = MessageBox.Show(
                    "This will run database migration functions:\n\n" +
                    "1. Update all CompanyIDs to 1\n" +
                    "2. Add default values to bill details\n" +
                    "3. Add SubQuantity column to ItemMaster\n\n" +
                    "This operation may take some time. Continue?",
                    "Database Migration",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

                if (result != DialogResult.Yes)
                    return;

                // Show progress message
                using (var progressForm = new Form())
                {
                    progressForm.Text = "Database Migration";
                    progressForm.Size = new Size(400, 150);
                    progressForm.StartPosition = FormStartPosition.CenterParent;
                    progressForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    progressForm.MaximizeBox = false;
                    progressForm.MinimizeBox = false;

                    var label = new Label
                    {
                        Text = "Running database migration...\nPlease wait...",
                        Location = new Point(20, 20),
                        Size = new Size(350, 60),
                        TextAlign = ContentAlignment.MiddleCenter
                    };

                    progressForm.Controls.Add(label);
                    progressForm.Show();
                    progressForm.Refresh();

                    // Run the migration functions
                    bool companyIdSuccess = DatabaseManager.UpdateAllCompanyIDsToOne();
                    bool billDetailsSuccess = DatabaseManager.UpdateBillDetailsWithDefaultValues();
                    bool itemMasterSuccess = DatabaseManager.AddSubQuantityToItemMaster();

                    progressForm.Close();

                    // Show results
                    string message = "Database migration completed!\n\n";
                    message += companyIdSuccess ? "✓ CompanyID migration: SUCCESS\n" : "✗ CompanyID migration: FAILED\n";
                    message += billDetailsSuccess ? "✓ Bill details update: SUCCESS\n" : "✗ Bill details update: FAILED\n";
                    message += itemMasterSuccess ? "✓ ItemMaster update: SUCCESS" : "✗ ItemMaster update: FAILED";

                    MessageBox.Show(message, "Migration Complete", 
                        MessageBoxButtons.OK, 
                        companyIdSuccess && billDetailsSuccess ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during database migration: {ex.Message}", 
                    "Migration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        // private void loginControl_Load(object sender, EventArgs e)
        // {

        // }
    }
}