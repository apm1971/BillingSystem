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
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Temporarily bypass the login screen for development
            ShowMainApplicationUI();

            // To re-enable login later, uncomment the line below and comment out the line above
            // ShowLoginScreen();
        }

        #region Login Flow Management

        // private void ShowLoginScreen()
        // {
        //     mainMenuStrip.Visible = false;

        //     // The loginControl is already on the form from the Designer
        //     loginControl.Visible = true;
        //     loginControl.Dock = DockStyle.Fill;
        //     loginControl.LoginSuccess += OnLoginSuccess;
        // }

        private void OnLoginSuccess(object sender, User authenticatedUser)
        {

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

            mastersMenu.DropDownItems.Add(partyMasterItem);
            mastersMenu.DropDownItems.Add(itemMasterItem);
            mastersMenu.DropDownItems.Add(brokerMasterItem);

            // === BILLS MENU ===
            var billsMenu = new ToolStripMenuItem("&Bills");
            billsMenu.Font = menuFont;

            var newBillItem = new ToolStripMenuItem("&New Bill");
            newBillItem.Font = menuFont;
            newBillItem.Click += (s, e) => { ShowControl(new SaleBillUserControl()); };

            var billledgerItem = new ToolStripMenuItem("&Bill Ledger");
            billledgerItem.Font = menuFont;
            billledgerItem.Click += (s, e) => { ShowControl(new BillLedgerControl()); };
            var billListItem = new ToolStripMenuItem("&Bill List");
            billListItem.Font = menuFont;
            billListItem.Click += (s, e) => { ShowControl(new BillListUserControl()); };

            billsMenu.DropDownItems.Add(newBillItem);
            billsMenu.DropDownItems.Add(billledgerItem);
            billsMenu.DropDownItems.Add(billListItem);

            // === PAYMENTS MENU ===
            var paymentsMenu = new ToolStripMenuItem("&Payments");
            paymentsMenu.Font = menuFont;

            var paymentEntryItem = new ToolStripMenuItem("&Settlement Entry");
            paymentEntryItem.Font = menuFont;
            paymentEntryItem.Click += (s, e) => { ShowControl(new PaymentEntryControl()); };

            // var paymentListItem = new ToolStripMenuItem("&Payment List");
            // paymentListItem.Font = menuFont;
            // paymentListItem.Click += (s, e) => { ShowControl(new PaymentListControl()); };

            var advancePaymentItem = new ToolStripMenuItem("&Payment Entry");
            advancePaymentItem.Font = menuFont;
            advancePaymentItem.Click += (s, e) => { ShowControl(new AdvancePaymentEntryControl()); };

            paymentsMenu.DropDownItems.Add(paymentEntryItem);
            // paymentsMenu.DropDownItems.Add(paymentListItem);
            paymentsMenu.DropDownItems.Add(advancePaymentItem);

            // === UTILITIES MENU ===
            var utilitiesMenu = new ToolStripMenuItem("&Utilities");
            utilitiesMenu.Font = menuFont;

            var migrateItem = new ToolStripMenuItem("&Migrate Database");
            migrateItem.Font = menuFont;
            migrateItem.Click += (s, e) => { RunDatabaseMigration(); };

            utilitiesMenu.DropDownItems.Add(migrateItem);

            // Add all top-level menus to the main menu strip in the correct order
            mainMenuStrip.Items.Add(mastersMenu);
            mainMenuStrip.Items.Add(billsMenu);
            mainMenuStrip.Items.Add(paymentsMenu);
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