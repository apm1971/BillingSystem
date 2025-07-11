using System;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;

namespace SaleBillSystem.NET.Forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            InitializeUI();
        }
        
        private void InitializeUI()
        {
            // Set form properties
            this.Text = Program.APP_NAME;
            this.Width = 1024;
            this.Height = 768;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.KeyPreview = true; // Enable form to receive key events first
            
            // Create menu
            MenuStrip mainMenu = new MenuStrip();
            this.MainMenuStrip = mainMenu;
            
            // File Menu
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("&File");
            ToolStripMenuItem fileNewBill = new ToolStripMenuItem("&New Bill");
            ToolStripMenuItem fileBillList = new ToolStripMenuItem("&Bill List");
            ToolStripMenuItem filePaymentEntry = new ToolStripMenuItem("&Payment Entry");
            ToolStripMenuItem filePaymentList = new ToolStripMenuItem("Payment &List");
            ToolStripMenuItem fileExit = new ToolStripMenuItem("E&xit");
            
            // Add keyboard shortcuts
            fileNewBill.ShortcutKeys = Keys.Control | Keys.N;
            fileBillList.ShortcutKeys = Keys.Control | Keys.B;
            filePaymentEntry.ShortcutKeys = Keys.Control | Keys.P;
            filePaymentList.ShortcutKeys = Keys.Control | Keys.L;
            fileExit.ShortcutKeys = Keys.Alt | Keys.F4;
            
            fileMenu.DropDownItems.Add(fileNewBill);
            fileMenu.DropDownItems.Add(fileBillList);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(filePaymentEntry);
            fileMenu.DropDownItems.Add(filePaymentList);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(fileExit);
            
            // Masters Menu
            ToolStripMenuItem mastersMenu = new ToolStripMenuItem("&Masters");
            ToolStripMenuItem mastersParty = new ToolStripMenuItem("&Party Master");
            ToolStripMenuItem mastersItem = new ToolStripMenuItem("&Item Master");
            ToolStripMenuItem mastersBroker = new ToolStripMenuItem("&Broker Master");
            
            // Add keyboard shortcuts for masters
            mastersParty.ShortcutKeys = Keys.Control | Keys.Shift | Keys.P;
            mastersItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.I;
            mastersBroker.ShortcutKeys = Keys.Control | Keys.Shift | Keys.B;
            
            mastersMenu.DropDownItems.Add(mastersParty);
            mastersMenu.DropDownItems.Add(mastersItem);
            mastersMenu.DropDownItems.Add(mastersBroker);
            
            // Tools Menu
            ToolStripMenuItem toolsMenu = new ToolStripMenuItem("&Tools");
            ToolStripMenuItem toolsGenerateMockData = new ToolStripMenuItem("&Generate Mock Data");
            ToolStripMenuItem toolsClearAllData = new ToolStripMenuItem("&Clear All Data");
            
            // Add keyboard shortcuts for tools
            toolsGenerateMockData.ShortcutKeys = Keys.Control | Keys.G;
            toolsClearAllData.ShortcutKeys = Keys.Control | Keys.Shift | Keys.Delete;
            
            toolsMenu.DropDownItems.Add(toolsGenerateMockData);
            toolsMenu.DropDownItems.Add(new ToolStripSeparator());
            toolsMenu.DropDownItems.Add(toolsClearAllData);
            
            // Help Menu
            ToolStripMenuItem helpMenu = new ToolStripMenuItem("&Help");
            ToolStripMenuItem helpAbout = new ToolStripMenuItem("&About");
            ToolStripMenuItem helpKeyboardShortcuts = new ToolStripMenuItem("&Keyboard Shortcuts");
            
            // Add keyboard shortcuts for help
            helpAbout.ShortcutKeys = Keys.F1;
            helpKeyboardShortcuts.ShortcutKeys = Keys.F2;
            
            helpMenu.DropDownItems.Add(helpKeyboardShortcuts);
            helpMenu.DropDownItems.Add(new ToolStripSeparator());
            helpMenu.DropDownItems.Add(helpAbout);
            
            // Add menus to menu strip
            mainMenu.Items.Add(fileMenu);
            mainMenu.Items.Add(mastersMenu);
            mainMenu.Items.Add(toolsMenu);
            mainMenu.Items.Add(helpMenu);
            
            // Add menu strip to form
            this.Controls.Add(mainMenu);
            
            // Create toolbar
            ToolStrip toolBar = new ToolStrip();
            
            // Add toolbar buttons
            ToolStripButton btnNewBill = new ToolStripButton("New Bill");
            ToolStripButton btnBillList = new ToolStripButton("Bill List");
            ToolStripButton btnPaymentEntry = new ToolStripButton("Payment Entry");
            ToolStripButton btnPartyMaster = new ToolStripButton("Party Master");
            ToolStripButton btnItemMaster = new ToolStripButton("Item Master");
            ToolStripButton btnBrokerMaster = new ToolStripButton("Broker Master");
            ToolStripButton btnAbout = new ToolStripButton("About");
            ToolStripButton btnExit = new ToolStripButton("Exit");
            
            // Add tooltips with keyboard shortcuts
            btnNewBill.ToolTipText = "Create new bill (Ctrl+N)";
            btnBillList.ToolTipText = "View bill list (Ctrl+B)";
            btnPaymentEntry.ToolTipText = "Record payments against bills (Ctrl+P)";
            btnPartyMaster.ToolTipText = "Manage parties (Ctrl+Shift+P)";
            btnItemMaster.ToolTipText = "Manage items (Ctrl+Shift+I)";
            btnBrokerMaster.ToolTipText = "Manage brokers (Ctrl+Shift+B)";
            btnAbout.ToolTipText = "About this application (F1)";
            btnExit.ToolTipText = "Exit application (Alt+F4)";
            
            // Add separators and buttons to toolbar
            toolBar.Items.Add(btnNewBill);
            toolBar.Items.Add(btnBillList);
            toolBar.Items.Add(btnPaymentEntry);
            toolBar.Items.Add(new ToolStripSeparator());
            toolBar.Items.Add(btnPartyMaster);
            toolBar.Items.Add(btnItemMaster);
            toolBar.Items.Add(btnBrokerMaster);
            toolBar.Items.Add(new ToolStripSeparator());
            toolBar.Items.Add(btnAbout);
            toolBar.Items.Add(btnExit);
            
            // Add toolbar to form
            this.Controls.Add(toolBar);
            
            // Create status bar
            StatusStrip statusBar = new StatusStrip();
            ToolStripStatusLabel statusLabel = new ToolStripStatusLabel(Program.APP_NAME);
            ToolStripStatusLabel dateLabel = new ToolStripStatusLabel(DateTime.Now.ToShortDateString());
            ToolStripStatusLabel timeLabel = new ToolStripStatusLabel(DateTime.Now.ToShortTimeString());
            
            // Set status bar properties
            statusLabel.Spring = true;
            statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            dateLabel.Alignment = ToolStripItemAlignment.Right;
            timeLabel.Alignment = ToolStripItemAlignment.Right;
            
            // Add labels to status bar
            statusBar.Items.Add(statusLabel);
            statusBar.Items.Add(dateLabel);
            statusBar.Items.Add(timeLabel);
            
            // Add status bar to form
            this.Controls.Add(statusBar);
            
            // Wire up event handlers
            fileNewBill.Click += (s, e) => NewBill();
            fileBillList.Click += (s, e) => ShowBillList();
            filePaymentEntry.Click += (s, e) => ShowPaymentEntry();
            filePaymentList.Click += (s, e) => ShowPaymentList();
            fileExit.Click += (s, e) => this.Close();
            mastersParty.Click += (s, e) => ShowPartyMaster();
            mastersItem.Click += (s, e) => ShowItemMaster();
            mastersBroker.Click += (s, e) => ShowBrokerMaster();
            toolsGenerateMockData.Click += (s, e) => GenerateMockData();
            toolsClearAllData.Click += (s, e) => ClearAllData();
            helpAbout.Click += (s, e) => ShowAbout();
            helpKeyboardShortcuts.Click += (s, e) => ShowKeyboardShortcuts();
            
            btnNewBill.Click += (s, e) => NewBill();
            btnBillList.Click += (s, e) => ShowBillList();
            btnPaymentEntry.Click += (s, e) => ShowPaymentEntry();
            btnPartyMaster.Click += (s, e) => ShowPartyMaster();
            btnItemMaster.Click += (s, e) => ShowItemMaster();
            btnBrokerMaster.Click += (s, e) => ShowBrokerMaster();
            btnAbout.Click += (s, e) => ShowAbout();
            btnExit.Click += (s, e) => this.Close();
            
            // Add KeyDown event handler for global shortcuts
            this.KeyDown += MainForm_KeyDown;
            
            // Timer for updating date/time
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
            timer.Tick += (s, e) =>
            {
                dateLabel.Text = DateTime.Now.ToShortDateString();
                timeLabel.Text = DateTime.Now.ToShortTimeString();
            };
            timer.Start();
        }
        
        private void NewBill()
        {
            var form = new SaleBillForm();
            form.ShowDialog();
        }
        
        private void ShowBillList()
        {
            var form = new BillListForm();
            form.ShowDialog();
        }
        
        private void ShowPartyMaster()
        {
            var form = new PartyMasterForm();
            form.ShowDialog();
        }
        
        private void ShowItemMaster()
        {
            var form = new ItemMasterForm();
            form.ShowDialog();
        }

        private void ShowBrokerMaster()
        {
            var form = new BrokerListForm();
            form.ShowDialog();
        }

        private void ShowPaymentEntry()
        {
            try
            {
                using (PaymentEntryForm paymentForm = new PaymentEntryForm())
                {
                    paymentForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening payment entry: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowPaymentList()
        {
            try
            {
                using (PaymentListForm paymentListForm = new PaymentListForm())
                {
                    paymentListForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening payment list: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void GenerateMockData()
        {
            MockDataGenerator.GenerateMockData();
        }
        
        private void ClearAllData()
        {
            if (MessageBox.Show(
                "Are you sure you want to clear all data?\n\n" +
                "This will permanently delete:\n" +
                "• All parties\n" +
                "• All items\n" +
                "• All bills\n\n" +
                "This action cannot be undone!",
                "Confirm Clear All Data",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                MockDataGenerator.ClearAllData();
            }
        }
        
        private void ShowAbout()
        {
            MessageBox.Show(
                $"{Program.APP_NAME} v1.0\n" +
                "© 2025 Your Company\n\n" +
                "A complete sales billing system with multi-item bills,\n" +
                "party master, broker management, and payment tracking.\n\n" +
                "Features:\n" +
                "• Party management with credit days\n" +
                "• Item management with charges\n" +
                "• Multi-item billing\n" +
                "• Broker management system\n" +
                "• Payment tracking with partial payment support\n" +
                "• Outstanding bills management\n" +
                "• Mock data generation for testing",
                "About " + Program.APP_NAME,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void ShowKeyboardShortcuts()
        {
            MessageBox.Show(
                "GLOBAL KEYBOARD SHORTCUTS:\n\n" +
                "=== MAIN MENU ===\n" +
                "Ctrl+N: New Bill\n" +
                "Ctrl+B: Bill List\n" +
                "Ctrl+P: Payment Entry\n" +
                "Ctrl+L: Payment List\n" +
                "Ctrl+Shift+P: Party Master\n" +
                "Ctrl+Shift+I: Item Master\n" +
                "Ctrl+Shift+B: Broker Master\n" +
                "Ctrl+G: Generate Mock Data\n" +
                "Alt+F4: Exit\n" +
                "F1: About\n" +
                "F2: Show Keyboard Shortcuts\n\n" +
                "=== BILL ENTRY ===\n" +
                "Ctrl+S: Save Bill\n" +
                "Escape: Cancel\n" +
                "F2: Focus on Party\n" +
                "F3: Focus on Items Grid\n" +
                "Ctrl+N: Add New Row (in grid)\n" +
                "Delete: Delete Row (in grid)\n\n" +
                "=== PAYMENT ENTRY ===\n" +
                "Ctrl+S: Save Payment\n" +
                "Ctrl+A: Auto Allocate\n" +
                "Ctrl+R: Clear Allocation\n" +
                "Alt+P: Switch to Party Filter\n" +
                "Alt+B: Switch to Broker Filter\n" +
                "F5: Refresh Bill List\n\n" +
                "=== LIST FORMS ===\n" +
                "F5: Refresh List\n" +
                "Enter/F2: Edit/View Selected\n" +
                "Delete: Delete Selected\n" +
                "Ctrl+F: Focus on Search\n" +
                "Escape: Close\n\n" +
                "=== GENERAL NAVIGATION ===\n" +
                "Tab: Next Field\n" +
                "Shift+Tab: Previous Field\n" +
                "Arrow Keys: Navigate in Grids\n" +
                "Page Up/Down: Scroll Lists\n" +
                "Home/End: First/Last Item\n" +
                "F4: Open Dropdown\n" +
                "Enter: Confirm/Next Cell\n" +
                "F1: Context Help",
                "Complete Keyboard Shortcuts Reference",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.N)
            {
                NewBill();
                e.SuppressKeyPress = true; // Suppress default behavior
            }
            else if (e.Control && e.KeyCode == Keys.B)
            {
                ShowBillList();
                e.SuppressKeyPress = true; // Suppress default behavior
            }
            else if (e.Control && e.KeyCode == Keys.P)
            {
                ShowPaymentEntry();
                e.SuppressKeyPress = true; // Suppress default behavior
            }
            else if (e.Control && e.KeyCode == Keys.L)
            {
                ShowPaymentList();
                e.SuppressKeyPress = true; // Suppress default behavior
            }
            else if (e.Alt && e.KeyCode == Keys.F4)
            {
                this.Close();
                e.SuppressKeyPress = true; // Suppress default behavior
            }
            else if (e.Control && e.KeyCode == Keys.G)
            {
                GenerateMockData();
                e.SuppressKeyPress = true; // Suppress default behavior
            }
            else if (e.Control && e.KeyCode == Keys.Shift && e.KeyCode == Keys.Delete)
            {
                ClearAllData();
                e.SuppressKeyPress = true; // Suppress default behavior
            }
            else if (e.KeyCode == Keys.F1)
            {
                ShowAbout();
                e.SuppressKeyPress = true; // Suppress default behavior
            }
            else if (e.KeyCode == Keys.F2)
            {
                ShowKeyboardShortcuts();
                e.SuppressKeyPress = true; // Suppress default behavior
            }
        }
        
        // Required by Windows Forms designer
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.ResumeLayout(false);
        }
    }
} 