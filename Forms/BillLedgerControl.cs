using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace SaleBillSystem.NET.Forms
{
    /// <summary>
    /// Control for viewing bill ledger entries with party and broker information.
    /// Party and broker names are resolved using their respective services (PartyService, BrokerService)
    /// rather than database JOINs for better maintainability and separation of concerns.
    /// </summary>
    public partial class BillLedgerControl : UserControl
    {
        public event EventHandler? CloseRequested;
        
        private List<Bill> _billsForParty = new List<Bill>();
        private Bill? _selectedBill;
        private List<TransactionViewModel> _currentLedgerEntries = new List<TransactionViewModel>();
        
        // Pagination properties for bills
        private int _billPageSize = 50;
        private int _currentBillPage = 1;
        private List<Bill> _allBills = new List<Bill>();
        private Button btnPrevBillPage;
        private Button btnNextBillPage;
        private Label lblBillPageInfo;
        
        // Pagination properties for ledger entries
        private int _ledgerPageSize = 100;
        private int _currentLedgerPage = 1;
        private List<TransactionViewModel> _allLedgerEntries = new List<TransactionViewModel>();
        private Button btnPrevLedgerPage;
        private Button btnNextLedgerPage;
        private Label lblLedgerPageInfo;


        public BillLedgerControl()
        {
            InitializeComponent();
            InitializePaginationControls();
        }

        private void InitializePaginationControls()
        {
            // Create bill pagination panel
            Panel billPaginationPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                Parent = gbBills
            };

            // Create Previous Page button for bills
            btnPrevBillPage = new Button
            {
                Text = "< Previous",
                Width = 100,
                Location = new Point(10, 7),
                Enabled = false
            };
            btnPrevBillPage.Click += BtnPrevBillPage_Click;

            // Create Next Page button for bills
            btnNextBillPage = new Button
            {
                Text = "Next >",
                Width = 100,
                Location = new Point(billPaginationPanel.Width - 110, 7),
                Anchor = AnchorStyles.Right
            };
            btnNextBillPage.Click += BtnNextBillPage_Click;

            // Create page info label for bills
            lblBillPageInfo = new Label
            {
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Text = "Page 1"
            };

            // Add bill controls to panel
            billPaginationPanel.Controls.Add(btnPrevBillPage);
            billPaginationPanel.Controls.Add(btnNextBillPage);
            billPaginationPanel.Controls.Add(lblBillPageInfo);

            // Create ledger pagination panel
            Panel ledgerPaginationPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                Parent = gbLedger
            };

            // Create Previous Page button for ledger
            btnPrevLedgerPage = new Button
            {
                Text = "< Previous",
                Width = 100,
                Location = new Point(10, 7),
                Enabled = false
            };
            btnPrevLedgerPage.Click += BtnPrevLedgerPage_Click;

            // Create Next Page button for ledger
            btnNextLedgerPage = new Button
            {
                Text = "Next >",
                Width = 100,
                Location = new Point(ledgerPaginationPanel.Width - 110, 7),
                Anchor = AnchorStyles.Right
            };
            btnNextLedgerPage.Click += BtnNextLedgerPage_Click;

            // Create page info label for ledger
            lblLedgerPageInfo = new Label
            {
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Text = "Page 1"
            };

            // Add ledger controls to panel
            ledgerPaginationPanel.Controls.Add(btnPrevLedgerPage);
            ledgerPaginationPanel.Controls.Add(btnNextLedgerPage);
            ledgerPaginationPanel.Controls.Add(lblLedgerPageInfo);
        }

        private void BtnPrevBillPage_Click(object sender, EventArgs e)
        {
            if (_currentBillPage > 1)
            {
                _currentBillPage--;
                DisplayCurrentBillPage();
            }
        }

        private void BtnNextBillPage_Click(object sender, EventArgs e)
        {
            int totalPages = GetTotalBillPages();
            if (_currentBillPage < totalPages)
            {
                _currentBillPage++;
                DisplayCurrentBillPage();
            }
        }

        private int GetTotalBillPages()
        {
            if (_allBills == null) return 1;
            return (int)Math.Ceiling(_allBills.Count / (double)_billPageSize);
        }

        private void DisplayCurrentBillPage()
        {
            if (_allBills == null) return;

            int startIndex = (_currentBillPage - 1) * _billPageSize;
            var currentPageItems = _allBills.Skip(startIndex).Take(_billPageSize).ToList();

            // Update data source with just the current page of items
            dgvBills.DataSource = null;
            dgvBills.DataSource = currentPageItems;

            // Update pagination controls
            btnPrevBillPage.Enabled = _currentBillPage > 1;
            btnNextBillPage.Enabled = _currentBillPage < GetTotalBillPages();
            lblBillPageInfo.Text = $"Page {_currentBillPage} of {GetTotalBillPages()} ({_allBills.Count} bills)";
        }

        private void BtnPrevLedgerPage_Click(object sender, EventArgs e)
        {
            if (_currentLedgerPage > 1)
            {
                _currentLedgerPage--;
                DisplayCurrentLedgerPage();
            }
        }

        private void BtnNextLedgerPage_Click(object sender, EventArgs e)
        {
            int totalPages = GetTotalLedgerPages();
            if (_currentLedgerPage < totalPages)
            {
                _currentLedgerPage++;
                DisplayCurrentLedgerPage();
            }
        }

        private int GetTotalLedgerPages()
        {
            if (_allLedgerEntries == null) return 1;
            return (int)Math.Ceiling(_allLedgerEntries.Count / (double)_ledgerPageSize);
        }

        private void DisplayCurrentLedgerPage()
        {
            if (_allLedgerEntries == null) return;

            int startIndex = (_currentLedgerPage - 1) * _ledgerPageSize;
            var currentPageItems = _allLedgerEntries.Skip(startIndex).Take(_ledgerPageSize).ToList();

            // Update data source with just the current page of items
            dgvLedger.DataSource = null;
            dgvLedger.DataSource = currentPageItems;

            // Update pagination controls
            btnPrevLedgerPage.Enabled = _currentLedgerPage > 1;
            btnNextLedgerPage.Enabled = _currentLedgerPage < GetTotalLedgerPages();
            lblLedgerPageInfo.Text = $"Page {_currentLedgerPage} of {GetTotalLedgerPages()} ({_allLedgerEntries.Count} transactions)";
        }

        private void BillLedgerControl_Load(object? sender, EventArgs e)
        {
            SetupBillGrid();
            SetupLedgerGrid();
            LoadInitialData();
            SetupEventHandlers();
            ClearForm();
        }

        #region Initial Setup & Styling

        private void SetupBillGrid()
        {
            dgvBills.AutoGenerateColumns = false;
            dgvBills.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBills.MultiSelect = false;
            dgvBills.AllowUserToAddRows = false;
            dgvBills.RowHeadersVisible = false;
            dgvBills.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            dgvBills.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvBills.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvBills.DefaultCellStyle.Font = new Font("Segoe UI", 9.75F);
            dgvBills.RowTemplate.Height = 28;

            dgvBills.Columns.Clear();
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BillNo", HeaderText = "Bill No.", Width = 120 });
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BillDate", HeaderText = "Bill Date", DefaultCellStyle = new DataGridViewCellStyle { Format = "dd-MMM-yyyy" }, Width = 120 });
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PartyName", HeaderText = "Party Name", Width = 200 });
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BrokerName", HeaderText = "Broker Name", Width = 150 });
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalAmount", HeaderText = "Bill Amount", DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }, Width = 150 });
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Status", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        }

        private void SetupLedgerGrid()
        {
            dgvLedger.AutoGenerateColumns = false;
            dgvLedger.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLedger.AllowUserToAddRows = false;
            dgvLedger.RowHeadersVisible = false;
            dgvLedger.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvLedger.DefaultCellStyle.Font = new Font("Segoe UI", 9.75F);
            dgvLedger.RowTemplate.Height = 28;

            dgvLedger.Columns.Clear();
            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TransactionDate", HeaderText = "Date", DefaultCellStyle = new DataGridViewCellStyle { Format = "dd-MMM-yyyy" }, Width = 120 });
            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TransactionType", HeaderText = "Type", Width = 100 });
            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Description", HeaderText = "Description", Width = 250 });
            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DebitAmount", HeaderText = "Debit (₹)", DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }, Width = 120 });
            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CreditAmount", HeaderText = "Credit (₹)", DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }, Width = 120 });
            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RunningBalance", HeaderText = "Balance (₹)", DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9.75F, FontStyle.Bold) }, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        }

        private void LoadInitialData()
        {
            try
            {
                // Load combo box data sources directly from services
                cmbParty.DataSource = PartyService.GetAllParties();
                cmbParty.DisplayMember = "PartyName";
                cmbParty.ValueMember = "PartyID";
                
                cmbBroker.DataSource = BrokerService.GetAllBrokers();
                cmbBroker.DisplayMember = "BrokerName";
                cmbBroker.ValueMember = "BrokerID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading initial data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupEventHandlers()
        {
            cmbParty.SelectedIndexChanged += CmbParty_SelectedIndexChanged;
            cmbBroker.SelectedIndexChanged += CmbBroker_SelectedIndexChanged;
            dgvBills.SelectionChanged += DgvBills_SelectionChanged;
            dgvBills.KeyDown += DgvBills_KeyDown;
            btnPrint.Click += BtnPrint_Click;
            btnRefresh.Click += BtnRefresh_Click;
        }

        private void ClearForm()
        {
            cmbParty.SelectedIndex = -1;
            cmbBroker.SelectedIndex = -1;
            dgvBills.DataSource = null;
            dgvLedger.DataSource = null;
            gbLedger.Text = "Transaction History";
            _selectedBill = null;
        }

        #endregion

        #region Data Loading

        private void CmbParty_SelectedIndexChanged(object? sender, EventArgs e)
        {
            LoadBillsBasedOnSelection();
        }

        private void CmbBroker_SelectedIndexChanged(object? sender, EventArgs e)
        {
            LoadBillsBasedOnSelection();
        }

        private void LoadBillsBasedOnSelection()
        {
            int? partyId = cmbParty.SelectedValue as int?;
            int? brokerId = cmbBroker.SelectedValue as int?;

            if (partyId.HasValue && partyId.Value > 0)
            {
                LoadBillsForParty(partyId.Value, brokerId);
            }
            else if (brokerId.HasValue && brokerId.Value > 0)
            {
                LoadBillsByBroker(brokerId.Value);
            }
            else
            {
                LoadAllBills();
            }
        }
        
        private void LoadAllBills()
        {
            try
            {
                // Use the optimized GetAllBills that includes party names through JOIN
                var allBills = BillService.GetAllBills();
                
                // Get all bill balances in one database query instead of querying individually
                var allBillBalances = LedgerService.GetAllBillBalances();
                
                // Get all brokers once to use for lookups
                var brokers = BrokerService.GetAllBrokers();
                
                // Update status for each bill based on due amount and populate broker names
                foreach (var bill in allBills)
                {
                    // Use pre-calculated balance from the dictionary
                    decimal dueAmount = allBillBalances.ContainsKey(bill.BillID) ? allBillBalances[bill.BillID] : 0m;
                    bill.Balance = dueAmount;

                    // Look up broker name from broker ID using cached brokers list
                    if (bill.BrokerID.HasValue && bill.BrokerID.Value > 0)
                    {
                        var broker = brokers.FirstOrDefault(b => b.BrokerID == bill.BrokerID.Value);
                        bill.BrokerName = broker?.BrokerName ?? "Unknown Broker";
                    }
                    else
                    {
                        bill.BrokerName = "No Broker";
                    }

                    // Set bill status based on balance
                    SetBillStatus(bill, dueAmount);
                }
                
                _allBills = allBills; // Store all bills for pagination
                _currentBillPage = 1; // Reset to first page
                DisplayCurrentBillPage(); // Display the first page
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading all bills: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetBillStatus(Bill bill, decimal dueAmount)
        {
            // Set bill status based on balance
            if (dueAmount <= 0)
            {
                bill.Status = "Paid";
            }
            else if (dueAmount < bill.TotalAmount)
            {
                bill.Status = "Partial";
            }
            else
            {
                bill.Status = "Unpaid";
            }
        }

        private void LoadBillsForParty(int partyId, int? brokerId = null)
        {
            try
            {
                var bills = BillService.GetAllBillsForParty(partyId);
                
                // Filter by broker if specified
                if (brokerId.HasValue && brokerId.Value > 0)
                {
                    bills = bills.Where(b => b.BrokerID == brokerId.Value).ToList();
                }
                
                // Get all bill balances in one database query instead of querying individually
                var allBillBalances = LedgerService.GetAllBillBalances();
                
                // Get all brokers once to use for lookups
                var brokers = BrokerService.GetAllBrokers();
                
                // Get the party once to use for all bills
                var party = PartyService.GetPartyByID(partyId);
                string partyName = party?.PartyName ?? "Unknown Party";
                
                // Update status for each bill
                foreach (var bill in bills)
                {
                    // Set party name for all bills from the same party
                    bill.PartyName = partyName;
                    
                    // Use pre-calculated balance from the dictionary
                    decimal dueAmount = allBillBalances.ContainsKey(bill.BillID) ? allBillBalances[bill.BillID] : 0m;
                    bill.Balance = dueAmount;

                    // Look up broker name from broker ID using cached brokers list
                    if (bill.BrokerID.HasValue && bill.BrokerID.Value > 0)
                    {
                        var broker = brokers.FirstOrDefault(b => b.BrokerID == bill.BrokerID.Value);
                        bill.BrokerName = broker?.BrokerName ?? "Unknown Broker";
                    }
                    else
                    {
                        bill.BrokerName = "No Broker";
                    }

                    // Set bill status based on balance
                    SetBillStatus(bill, dueAmount);
                }
                
                _allBills = bills; // Store all bills for pagination
                _currentBillPage = 1; // Reset to first page
                DisplayCurrentBillPage(); // Display the first page
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bills: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadBillsByBroker(int brokerId)
        {
            try
            {
                // Get all bills with joined party names
                var allBills = BillService.GetAllBills();
                
                // Filter by broker
                _allBills = allBills.Where(b => b.BrokerID == brokerId).ToList();
                
                // Get all bill balances in one database query instead of querying individually
                var allBillBalances = LedgerService.GetAllBillBalances();
                
                // Get the broker once
                var broker = BrokerService.GetBrokerByID(brokerId);
                string brokerName = broker?.BrokerName ?? "Unknown Broker";
                
                // Update status for each bill
                foreach (var bill in _allBills)
                {
                    // Set broker name for all bills
                    bill.BrokerName = brokerName;
                    
                    // Use pre-calculated balance from the dictionary
                    decimal dueAmount = allBillBalances.ContainsKey(bill.BillID) ? allBillBalances[bill.BillID] : 0m;
                    bill.Balance = dueAmount;
                    
                    // Set bill status based on balance
                    SetBillStatus(bill, dueAmount);
                }
                
                _currentBillPage = 1; // Reset to first page
                DisplayCurrentBillPage(); // Display the first page
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bills by broker: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvBills_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvBills.SelectedRows.Count > 0 && dgvBills.SelectedRows[0].DataBoundItem is Bill selectedBill)
            {
                _selectedBill = selectedBill;
                LoadLedgerForBill(selectedBill);
            }
        }

        private void LoadLedgerForBill(Bill bill)
        {
            try
            {
                _currentLedgerPage = 1; // Reset to first page
                _selectedBill = bill;
                
                // Load all transactions for this bill in a single query
                var transactions = LedgerService.GetTransactionsForBill(bill.BillID);
                _allLedgerEntries = new List<TransactionViewModel>(); // Clear existing entries
                decimal runningBalance = 0;

                // Convert transactions to view models with running balance
                foreach (var tx in transactions)
                {
                    runningBalance += tx.DebitAmount - tx.CreditAmount;
                    _allLedgerEntries.Add(new TransactionViewModel
                    {
                        TransactionDate = tx.TransactionDate,
                        TransactionType = tx.TransactionType,
                        Description = tx.Description,
                        DebitAmount = tx.DebitAmount,
                        CreditAmount = tx.CreditAmount,
                        RunningBalance = Math.Round(runningBalance)
                    });
                }

                // Display the transactions with pagination
                DisplayCurrentLedgerPage();
                gbLedger.Text = $"Transaction History for Bill: {bill.BillNo}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading ledger: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion
        
        #region Printing and Exporting

        private void BtnPrint_Click(object? sender, EventArgs e)
        {
            if (_selectedBill == null || !_allLedgerEntries.Any())
            {
                MessageBox.Show("Please select a bill with transactions to print.", "No Bill Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string htmlContent = GenerateHtmlReport(_selectedBill, _allLedgerEntries);
                string tempFilePath = Path.Combine(Path.GetTempPath(), $"Bill_{_selectedBill.BillNo}.html");
                File.WriteAllText(tempFilePath, htmlContent);

                // Open the file in the default web browser
                Process.Start(new ProcessStartInfo(tempFilePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not generate or open the report: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            LoadBillsBasedOnSelection();
        }

        private string GenerateHtmlReport(Bill bill, List<TransactionViewModel> ledgerEntries)
        {
            var party = PartyService.GetPartyByID(bill.PartyID);
            var company = Program.ActiveCompany; // Assuming you have this in Program.cs

            var sb = new StringBuilder();

            // --- HTML and CSS Styling ---
            sb.AppendLine("<html><head><title>Bill Statement</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: 'Segoe UI', sans-serif; margin: 20px; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
            sb.AppendLine("th, td { border: 1px solid #ccc; padding: 8px; text-align: left; }");
            sb.AppendLine("th { background-color: #f2f2f2; }");
            sb.AppendLine(".header { display: flex; justify-content: space-between; border-bottom: 2px solid #333; padding-bottom: 10px; }");
            sb.AppendLine(".header-left, .header-right { width: 48%; }");
            sb.AppendLine(".text-right { text-align: right; }");
            sb.AppendLine(".total-row { font-weight: bold; background-color: #f8f8f8; }");
            sb.AppendLine("h1, h2 { margin: 0; }");
            sb.AppendLine("</style></head><body>");

            // --- Report Header ---
            sb.AppendLine($"<h1>Statement for Bill No: {bill.BillNo}</h1>");
            sb.AppendLine($"<p>Bill Date: {bill.BillDate:dd-MMM-yyyy}</p>");
            if (!string.IsNullOrEmpty(bill.BrokerName))
            {
                sb.AppendLine($"<p>Broker: {bill.BrokerName}</p>");
            }
            sb.AppendLine("<div class='header'>");
            sb.AppendLine("<div class='header-left'>");
            sb.AppendLine($"<h2>{company?.CompanyName ?? "Your Company"}</h2>");
            sb.AppendLine($"<p>{company?.Address?.Replace("\n", "<br>")}</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class='header-right'>");
            sb.AppendLine($"<h2>To: {party?.PartyName}</h2>");
            sb.AppendLine($"<p>{party?.Address?.Replace("\n", "<br>")}</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            // --- Ledger Table ---
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Date</th><th>Type</th><th>Description</th><th class='text-right'>Debit (₹)</th><th class='text-right'>Credit (₹)</th><th class='text-right'>Balance (₹)</th></tr>");
            
            foreach (var entry in ledgerEntries)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{entry.TransactionDate:dd-MMM-yyyy}</td>");
                sb.AppendLine($"<td>{entry.TransactionType}</td>");
                sb.AppendLine($"<td>{entry.Description}</td>");
                sb.AppendLine($"<td class='text-right'>{Math.Round(entry.DebitAmount):N0}</td>");
                sb.AppendLine($"<td class='text-right'>{Math.Round(entry.CreditAmount):N0}</td>");
                sb.AppendLine($"<td class='text-right'>{Math.Round(entry.RunningBalance):N0}</td>");
                sb.AppendLine("</tr>");
            }

            // --- Totals Row ---
            var totalDebit = ledgerEntries.Sum(e => e.DebitAmount);
            var totalCredit = ledgerEntries.Sum(e => e.CreditAmount);
            var finalBalance = ledgerEntries.LastOrDefault()?.RunningBalance ?? 0;

            sb.AppendLine("<tr class='total-row'>");
            sb.AppendLine("<td colspan='3'><strong>Totals</strong></td>");
            sb.AppendLine($"<td class='text-right'><strong>{Math.Round(totalDebit):N0}</strong></td>");
            sb.AppendLine($"<td class='text-right'><strong>{Math.Round(totalCredit):N0}</strong></td>");
            sb.AppendLine($"<td class='text-right'><strong>{Math.Round(finalBalance):N0}</strong></td>");
            sb.AppendLine("</tr>");

            sb.AppendLine("</table>");
            sb.AppendLine("</body></html>");

            return sb.ToString();
        }

        #endregion

        #region Keyboard Navigation

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                CloseRequested?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void DgvBills_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Prevent the "ding" sound
                dgvLedger.Focus();
            }
            else if (e.KeyCode == Keys.F5)
            {
                e.SuppressKeyPress = true;
                BtnRefresh_Click(sender, e);
            }
        }

        #endregion
    }
}