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
            
            mastersMenu.DropDownItems.Add(mastersParty);
            mastersMenu.DropDownItems.Add(mastersItem);
            mastersMenu.DropDownItems.Add(mastersBroker);
            
            // Tools Menu
            ToolStripMenuItem toolsMenu = new ToolStripMenuItem("&Tools");
            ToolStripMenuItem toolsGenerateMockData = new ToolStripMenuItem("&Generate Mock Data");
            ToolStripMenuItem toolsClearAllData = new ToolStripMenuItem("&Clear All Data");
            
            toolsMenu.DropDownItems.Add(toolsGenerateMockData);
            toolsMenu.DropDownItems.Add(new ToolStripSeparator());
            toolsMenu.DropDownItems.Add(toolsClearAllData);
            
            // Help Menu
            ToolStripMenuItem helpMenu = new ToolStripMenuItem("&Help");
            ToolStripMenuItem helpAbout = new ToolStripMenuItem("&About");
            
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
            
            // Add tooltips
            btnNewBill.ToolTipText = "Create new bill";
            btnBillList.ToolTipText = "View bill list";
            btnPaymentEntry.ToolTipText = "Record payments against bills";
            btnPartyMaster.ToolTipText = "Manage parties";
            btnItemMaster.ToolTipText = "Manage items";
            btnBrokerMaster.ToolTipText = "Manage brokers";
            btnAbout.ToolTipText = "About this application";
            btnExit.ToolTipText = "Exit application";
            
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
            
            btnNewBill.Click += (s, e) => NewBill();
            btnBillList.Click += (s, e) => ShowBillList();
            btnPaymentEntry.Click += (s, e) => ShowPaymentEntry();
            btnPartyMaster.Click += (s, e) => ShowPartyMaster();
            btnItemMaster.Click += (s, e) => ShowItemMaster();
            btnBrokerMaster.Click += (s, e) => ShowBrokerMaster();
            btnAbout.Click += (s, e) => ShowAbout();
            btnExit.Click += (s, e) => this.Close();
            
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