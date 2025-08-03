using System;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class MainForm : Form
    {
        // This field keeps track of the currently displayed UserControl
        private UserControl _currentControl = null;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Temporarily bypass the login screen for development
            ShowMainApplicationUI();
            
            // To re-enable login later, uncomment the line below and comment out the line above
            // ShowLoginScreen();
        }

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
            Program.CurrentUser = authenticatedUser;
            loginControl.LoginSuccess -= OnLoginSuccess;
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

            // === MASTERS MENU ===
            var mastersMenu = new ToolStripMenuItem("&Masters");
            
            var partyMasterItem = new ToolStripMenuItem("&Party Master");
            partyMasterItem.Click += (s, e) => { ShowControl(new PartyMasterUserControl()); };
            
            var itemMasterItem = new ToolStripMenuItem("&Item Master");
            itemMasterItem.Click += (s, e) => { ShowControl(new ItemMasterUserControl()); };

            var brokerMasterItem = new ToolStripMenuItem("&Broker Master");
            brokerMasterItem.Click += (s, e) => { ShowControl(new BrokerMasterUserControl()); };
            
            mastersMenu.DropDownItems.Add(partyMasterItem);
            mastersMenu.DropDownItems.Add(itemMasterItem);
            mastersMenu.DropDownItems.Add(brokerMasterItem);
            
            // === BILLS MENU ===
            var billsMenu = new ToolStripMenuItem("&Bills");
            
            var newBillItem = new ToolStripMenuItem("&New Bill");
            newBillItem.Click += (s, e) => { ShowControl(new SaleBillUserControl()); };

            var billledgerItem = new ToolStripMenuItem("&Bill Ledger");
            billledgerItem.Click += (s, e) => { ShowControl(new BillLedgerControl()); };
            var billListItem = new ToolStripMenuItem("&Bill List");
            billListItem.Click += (s, e) => { ShowControl(new BillListUserControl()); };
            
            billsMenu.DropDownItems.Add(newBillItem);
            billsMenu.DropDownItems.Add(billledgerItem);
            billsMenu.DropDownItems.Add(billListItem);
            
            // === PAYMENTS MENU ===
            var paymentsMenu = new ToolStripMenuItem("&Payments");
            
            var paymentEntryItem = new ToolStripMenuItem("&Payment Entry");
            paymentEntryItem.Click += (s, e) => { ShowControl(new PaymentEntryControl()); };
            
            var paymentListItem = new ToolStripMenuItem("&Payment List");
            paymentListItem.Click += (s, e) => { ShowControl(new PaymentListControl()); };  
            
            paymentsMenu.DropDownItems.Add(paymentEntryItem);
            paymentsMenu.DropDownItems.Add(paymentListItem);

            // Add all top-level menus to the main menu strip in the correct order
            mainMenuStrip.Items.Add(mastersMenu);
            mainMenuStrip.Items.Add(billsMenu);
            mainMenuStrip.Items.Add(paymentsMenu);
            
            // Add Transactions, Reports, etc. menus here
        }

        #endregion
    }
}