using System;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using System.Drawing;

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
            this.Text = Program.APP_NAME + " - Main Menu";
            this.Width = 1200;
            this.Height = 800;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.KeyPreview = true; // Enable form to receive key events first
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(240, 240, 240);
            
            // Create menu
            MenuStrip mainMenu = new MenuStrip();
            this.MainMenuStrip = mainMenu;
            mainMenu.BackColor = Color.FromArgb(45, 45, 48);
            mainMenu.ForeColor = Color.White;
            mainMenu.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            
            // === TRANSACTIONS MENU ===
            ToolStripMenuItem transactionsMenu = new ToolStripMenuItem("&Transactions");
            transactionsMenu.ForeColor = Color.White;
            
            ToolStripMenuItem transSales = new ToolStripMenuItem("&Sales");
            ToolStripMenuItem transSalesEntry = new ToolStripMenuItem("Sales &Entry");
            ToolStripMenuItem transSalesList = new ToolStripMenuItem("Sales &List");
            transSalesEntry.ShortcutKeys = Keys.F9;
            transSalesList.ShortcutKeys = Keys.Control | Keys.F9;
            
            ToolStripMenuItem transPayments = new ToolStripMenuItem("&Payments");
            ToolStripMenuItem transPaymentEntry = new ToolStripMenuItem("Payment &Entry");
            ToolStripMenuItem transPaymentList = new ToolStripMenuItem("Payment &List");
            transPaymentEntry.ShortcutKeys = Keys.F5;
            transPaymentList.ShortcutKeys = Keys.Control | Keys.F5;
            
            // Add sales submenu
            transSales.DropDownItems.Add(transSalesEntry);
            transSales.DropDownItems.Add(transSalesList);
            
            // Add payments submenu
            transPayments.DropDownItems.Add(transPaymentEntry);
            transPayments.DropDownItems.Add(transPaymentList);
            
            // Add to transactions menu
            transactionsMenu.DropDownItems.Add(transSales);
            transactionsMenu.DropDownItems.Add(new ToolStripSeparator());
            transactionsMenu.DropDownItems.Add(transPayments);
            
            // === MASTERS MENU ===
            ToolStripMenuItem mastersMenu = new ToolStripMenuItem("&Masters");
            mastersMenu.ForeColor = Color.White;
            
            ToolStripMenuItem mastersParty = new ToolStripMenuItem("&Party Master");
            ToolStripMenuItem mastersItem = new ToolStripMenuItem("&Item Master");
            ToolStripMenuItem mastersBroker = new ToolStripMenuItem("&Broker Master");
            
            // Add keyboard shortcuts for masters
            mastersParty.ShortcutKeys = Keys.Alt | Keys.F1;
            mastersItem.ShortcutKeys = Keys.Alt | Keys.F2;
            mastersBroker.ShortcutKeys = Keys.Alt | Keys.F3;
            
            mastersMenu.DropDownItems.Add(mastersParty);
            mastersMenu.DropDownItems.Add(mastersItem);
            mastersMenu.DropDownItems.Add(mastersBroker);
            
            // === REPORTS MENU ===
            ToolStripMenuItem reportsMenu = new ToolStripMenuItem("&Reports");
            reportsMenu.ForeColor = Color.White;
            
            ToolStripMenuItem reportsOutstanding = new ToolStripMenuItem("&Outstanding Reports");
            ToolStripMenuItem reportsPayment = new ToolStripMenuItem("&Payment Reports");
            ToolStripMenuItem reportsSales = new ToolStripMenuItem("&Sales Reports");
            
            reportsMenu.DropDownItems.Add(reportsOutstanding);
            reportsMenu.DropDownItems.Add(reportsPayment);
            reportsMenu.DropDownItems.Add(reportsSales);
            
            // === UTILITIES MENU ===
            ToolStripMenuItem utilitiesMenu = new ToolStripMenuItem("&Utilities");
            utilitiesMenu.ForeColor = Color.White;
            
            ToolStripMenuItem utilitiesBackup = new ToolStripMenuItem("&Backup Data");
            ToolStripMenuItem utilitiesRestore = new ToolStripMenuItem("&Restore Data");
            ToolStripMenuItem utilitiesGenerateMockData = new ToolStripMenuItem("&Generate Mock Data");
            ToolStripMenuItem utilitiesClearAllData = new ToolStripMenuItem("&Clear All Data");
            
            // Add keyboard shortcuts for utilities
            utilitiesGenerateMockData.ShortcutKeys = Keys.Control | Keys.G;
            utilitiesClearAllData.ShortcutKeys = Keys.Control | Keys.Shift | Keys.Delete;
            
            utilitiesMenu.DropDownItems.Add(utilitiesBackup);
            utilitiesMenu.DropDownItems.Add(utilitiesRestore);
            utilitiesMenu.DropDownItems.Add(new ToolStripSeparator());
            utilitiesMenu.DropDownItems.Add(utilitiesGenerateMockData);
            utilitiesMenu.DropDownItems.Add(utilitiesClearAllData);
            
            // === HELP MENU ===
            ToolStripMenuItem helpMenu = new ToolStripMenuItem("&Help");
            helpMenu.ForeColor = Color.White;
            
            ToolStripMenuItem helpKeyboardShortcuts = new ToolStripMenuItem("&Keyboard Shortcuts");
            ToolStripMenuItem helpAbout = new ToolStripMenuItem("&About");
            
            // Add keyboard shortcuts for help
            helpKeyboardShortcuts.ShortcutKeys = Keys.F1;
            helpAbout.ShortcutKeys = Keys.Control | Keys.F1;
            
            helpMenu.DropDownItems.Add(helpKeyboardShortcuts);
            helpMenu.DropDownItems.Add(new ToolStripSeparator());
            helpMenu.DropDownItems.Add(helpAbout);
            
            // Add menus to menu strip
            mainMenu.Items.Add(transactionsMenu);
            mainMenu.Items.Add(mastersMenu);
            mainMenu.Items.Add(reportsMenu);
            mainMenu.Items.Add(utilitiesMenu);
            mainMenu.Items.Add(helpMenu);
            
            // Add menu strip to form
            this.Controls.Add(mainMenu);
            
            // Create main dashboard panel
            CreateDashboardPanel();
            
            // Create status bar
            CreateStatusBar();
            
            // Wire up event handlers
            transSalesEntry.Click += (s, e) => NewBill();
            transSalesList.Click += (s, e) => ShowBillList();
            transPaymentEntry.Click += (s, e) => ShowPaymentEntry();
            transPaymentList.Click += (s, e) => ShowPaymentList();
            mastersParty.Click += (s, e) => ShowPartyMaster();
            mastersItem.Click += (s, e) => ShowItemMaster();
            mastersBroker.Click += (s, e) => ShowBrokerMaster();
            utilitiesGenerateMockData.Click += (s, e) => GenerateMockData();
            utilitiesClearAllData.Click += (s, e) => ClearAllData();
            helpAbout.Click += (s, e) => ShowAbout();
            helpKeyboardShortcuts.Click += (s, e) => ShowKeyboardShortcuts();
            
            // Add KeyDown event handler for global shortcuts
            this.KeyDown += MainForm_KeyDown;
            
            // Timer for updating date/time
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
            timer.Tick += (s, e) => UpdateStatusBar();
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
                "ACCOUNTING SOFTWARE KEYBOARD SHORTCUTS\n\n" +
                "=== TRANSACTIONS ===\n" +
                "F9: New Sales Entry\n" +
                "Ctrl+F9: Sales List\n" +
                "F5: Payment Entry\n" +
                "Ctrl+F5: Payment List\n\n" +
                "=== MASTERS ===\n" +
                "Alt+F1: Party Master\n" +
                "Alt+F2: Item Master\n" +
                "Alt+F3: Broker Master\n\n" +
                "=== UTILITIES ===\n" +
                "Ctrl+G: Generate Mock Data\n" +
                "Ctrl+Shift+Delete: Clear All Data\n\n" +
                "=== HELP & SYSTEM ===\n" +
                "F1: Show Keyboard Shortcuts\n" +
                "Ctrl+F1: About\n" +
                "Alt+F4: Exit Application\n\n" +
                "=== FORM NAVIGATION ===\n" +
                "Tab: Next Field\n" +
                "Shift+Tab: Previous Field\n" +
                "Enter: Confirm/Next Cell\n" +
                "Escape: Cancel/Close\n" +
                "F4: Open Dropdown\n" +
                "Arrow Keys: Navigate in Grids\n" +
                "Page Up/Down: Scroll Lists\n" +
                "Home/End: First/Last Item\n" +
                "Delete: Delete Selected Item\n" +
                "F2: Edit Selected Item\n" +
                "F5: Refresh (in lists)\n" +
                "Ctrl+F: Find/Search\n\n" +
                "=== BILL ENTRY ===\n" +
                "Ctrl+S: Save Bill\n" +
                "F2: Focus on Party\n" +
                "F3: Focus on Items Grid\n" +
                "Ctrl+N: Add New Row\n\n" +
                "=== PAYMENT ENTRY ===\n" +
                "Ctrl+S: Save Payment\n" +
                "Ctrl+A: Auto Allocate\n" +
                "Ctrl+R: Clear Allocation\n" +
                "Alt+P: Party Filter\n" +
                "Alt+B: Broker Filter",
                "Complete Keyboard Shortcuts Reference",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F9)
            {
                // F9: New Sales Entry
                NewBill();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.F9)
            {
                // Ctrl+F9: Sales List
                ShowBillList();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                // F5: Payment Entry
                ShowPaymentEntry();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.F5)
            {
                // Ctrl+F5: Payment List
                ShowPaymentList();
                e.SuppressKeyPress = true;
            }
            else if (e.Alt && e.KeyCode == Keys.F1)
            {
                // Alt+F1: Party Master
                ShowPartyMaster();
                e.SuppressKeyPress = true;
            }
            else if (e.Alt && e.KeyCode == Keys.F2)
            {
                // Alt+F2: Item Master
                ShowItemMaster();
                e.SuppressKeyPress = true;
            }
            else if (e.Alt && e.KeyCode == Keys.F3)
            {
                // Alt+F3: Broker Master
                ShowBrokerMaster();
                e.SuppressKeyPress = true;
            }
            else if (e.Alt && e.KeyCode == Keys.F4)
            {
                // Alt+F4: Exit
                this.Close();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.G)
            {
                // Ctrl+G: Generate Mock Data
                GenerateMockData();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.Shift && e.KeyCode == Keys.Delete)
            {
                // Ctrl+Shift+Delete: Clear All Data
                ClearAllData();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F1)
            {
                // F1: Keyboard Shortcuts
                ShowKeyboardShortcuts();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.F1)
            {
                // Ctrl+F1: About
                ShowAbout();
                e.SuppressKeyPress = true;
            }
        }
        
        private void CreateDashboardPanel()
        {
            // Create main dashboard panel
            Panel dashboardPanel = new Panel();
            dashboardPanel.Dock = DockStyle.Fill;
            dashboardPanel.BackColor = Color.FromArgb(250, 250, 250);
            dashboardPanel.Padding = new Padding(20);
            
            // Create title label
            Label titleLabel = new Label();
            titleLabel.Text = Program.APP_NAME;
            titleLabel.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(45, 45, 48);
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(50, 50);
            
            // Create subtitle label
            Label subtitleLabel = new Label();
            subtitleLabel.Text = "Complete Billing & Accounting Solution";
            subtitleLabel.Font = new Font("Segoe UI", 12F, FontStyle.Regular);
            subtitleLabel.ForeColor = Color.FromArgb(100, 100, 100);
            subtitleLabel.AutoSize = true;
            subtitleLabel.Location = new Point(50, 90);
            
            // Create quick access panel
            Panel quickAccessPanel = CreateQuickAccessPanel();
            quickAccessPanel.Location = new Point(50, 150);
            
            // Create recent activities panel
            Panel recentPanel = CreateRecentActivitiesPanel();
            recentPanel.Location = new Point(450, 150);
            
            // Add controls to dashboard
            dashboardPanel.Controls.Add(titleLabel);
            dashboardPanel.Controls.Add(subtitleLabel);
            dashboardPanel.Controls.Add(quickAccessPanel);
            dashboardPanel.Controls.Add(recentPanel);
            
            // Add dashboard to form
            this.Controls.Add(dashboardPanel);
        }
        
        private Panel CreateQuickAccessPanel()
        {
            Panel panel = new Panel();
            panel.Size = new Size(350, 400);
            panel.BackColor = Color.White;
            panel.BorderStyle = BorderStyle.FixedSingle;
            
            // Title
            Label titleLabel = new Label();
            titleLabel.Text = "Quick Access";
            titleLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(45, 45, 48);
            titleLabel.Location = new Point(15, 15);
            titleLabel.AutoSize = true;
            
            // Create buttons
            Button btnNewBill = CreateQuickAccessButton("New Sales Entry", "F9", 50);
            Button btnPayment = CreateQuickAccessButton("Payment Entry", "F5", 90);
            Button btnBillList = CreateQuickAccessButton("Sales List", "Ctrl+F9", 130);
            Button btnPaymentList = CreateQuickAccessButton("Payment List", "Ctrl+F5", 170);
            Button btnPartyMaster = CreateQuickAccessButton("Party Master", "Alt+F1", 210);
            Button btnItemMaster = CreateQuickAccessButton("Item Master", "Alt+F2", 250);
            
            // Wire up events
            btnNewBill.Click += (s, e) => NewBill();
            btnPayment.Click += (s, e) => ShowPaymentEntry();
            btnBillList.Click += (s, e) => ShowBillList();
            btnPaymentList.Click += (s, e) => ShowPaymentList();
            btnPartyMaster.Click += (s, e) => ShowPartyMaster();
            btnItemMaster.Click += (s, e) => ShowItemMaster();
            
            // Add controls
            panel.Controls.Add(titleLabel);
            panel.Controls.Add(btnNewBill);
            panel.Controls.Add(btnPayment);
            panel.Controls.Add(btnBillList);
            panel.Controls.Add(btnPaymentList);
            panel.Controls.Add(btnPartyMaster);
            panel.Controls.Add(btnItemMaster);
            
            return panel;
        }
        
        private Button CreateQuickAccessButton(string text, string shortcut, int top)
        {
            Button btn = new Button();
            btn.Text = $"{text} ({shortcut})";
            btn.Size = new Size(320, 30);
            btn.Location = new Point(15, top);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btn.BackColor = Color.FromArgb(245, 245, 245);
            btn.ForeColor = Color.FromArgb(45, 45, 48);
            btn.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Cursor = Cursors.Hand;
            
            // Hover effects
            btn.MouseEnter += (s, e) => {
                btn.BackColor = Color.FromArgb(230, 230, 230);
                btn.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
            };
            btn.MouseLeave += (s, e) => {
                btn.BackColor = Color.FromArgb(245, 245, 245);
                btn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            };
            
            return btn;
        }
        
        private Panel CreateRecentActivitiesPanel()
        {
            Panel panel = new Panel();
            panel.Size = new Size(350, 400);
            panel.BackColor = Color.White;
            panel.BorderStyle = BorderStyle.FixedSingle;
            
            // Title
            Label titleLabel = new Label();
            titleLabel.Text = "Recent Activities";
            titleLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(45, 45, 48);
            titleLabel.Location = new Point(15, 15);
            titleLabel.AutoSize = true;
            
            // Placeholder content
            Label placeholderLabel = new Label();
            placeholderLabel.Text = "No recent activities to display.\nStart by creating a new bill or payment.";
            placeholderLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            placeholderLabel.ForeColor = Color.FromArgb(150, 150, 150);
            placeholderLabel.Location = new Point(15, 60);
            placeholderLabel.Size = new Size(320, 40);
            
            panel.Controls.Add(titleLabel);
            panel.Controls.Add(placeholderLabel);
            
            return panel;
        }
        
        private void CreateStatusBar()
        {
            StatusStrip statusBar = new StatusStrip();
            statusBar.BackColor = Color.FromArgb(45, 45, 48);
            statusBar.ForeColor = Color.White;
            
            ToolStripStatusLabel statusLabel = new ToolStripStatusLabel(Program.APP_NAME);
            ToolStripStatusLabel userLabel = new ToolStripStatusLabel("User: Administrator");
            ToolStripStatusLabel dateLabel = new ToolStripStatusLabel(DateTime.Now.ToString("dd/MM/yyyy"));
            ToolStripStatusLabel timeLabel = new ToolStripStatusLabel(DateTime.Now.ToString("HH:mm:ss"));
            
            // Set status bar properties
            statusLabel.Spring = true;
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            statusLabel.ForeColor = Color.White;
            userLabel.ForeColor = Color.White;
            dateLabel.ForeColor = Color.White;
            timeLabel.ForeColor = Color.White;
            
            // Add labels to status bar
            statusBar.Items.Add(statusLabel);
            statusBar.Items.Add(userLabel);
            statusBar.Items.Add(dateLabel);
            statusBar.Items.Add(timeLabel);
            
            // Store references for updating
            this.statusDateLabel = dateLabel;
            this.statusTimeLabel = timeLabel;
            
            // Add status bar to form
            this.Controls.Add(statusBar);
        }
        
        private ToolStripStatusLabel statusDateLabel;
        private ToolStripStatusLabel statusTimeLabel;
        
        private void UpdateStatusBar()
        {
            if (statusDateLabel != null && statusTimeLabel != null)
            {
                statusDateLabel.Text = DateTime.Now.ToString("dd/MM/yyyy");
                statusTimeLabel.Text = DateTime.Now.ToString("HH:mm:ss");
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