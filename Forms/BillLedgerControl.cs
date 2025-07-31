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
    public partial class BillLedgerControl : UserControl
    {
        public event EventHandler? CloseRequested;
        
        private List<Party> _parties = new List<Party>();
        private List<Bill> _billsForParty = new List<Bill>();
        private Bill? _selectedBill;
        private List<TransactionViewModel> _currentLedgerEntries = new List<TransactionViewModel>();


        public BillLedgerControl()
        {
            InitializeComponent();
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
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalAmount", HeaderText = "Bill Amount", DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }, Width = 150 });
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
            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DebitAmount", HeaderText = "Debit (₹)", DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }, Width = 120 });
            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CreditAmount", HeaderText = "Credit (₹)", DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }, Width = 120 });
            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RunningBalance", HeaderText = "Balance (₹)", DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9.75F, FontStyle.Bold) }, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        }

        private void LoadInitialData()
        {
            try
            {
                _parties = PartyService.GetAllParties();
                cmbParty.DataSource = _parties;
                cmbParty.DisplayMember = "PartyName";
                cmbParty.ValueMember = "PartyID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading parties: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupEventHandlers()
        {
            cmbParty.SelectedIndexChanged += CmbParty_SelectedIndexChanged;
            dgvBills.SelectionChanged += DgvBills_SelectionChanged;
            dgvBills.KeyDown += DgvBills_KeyDown;
            btnPrint.Click += BtnPrint_Click; // Event for the new Print button
        }

        private void ClearForm()
        {
            cmbParty.SelectedIndex = -1;
            dgvBills.DataSource = null;
            dgvLedger.DataSource = null;
            gbLedger.Text = "Transaction History";
            _selectedBill = null;
        }

        #endregion

        #region Data Loading

        private void CmbParty_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbParty.SelectedValue is int partyId && partyId > 0)
            {
                LoadBillsForParty(partyId);
            }
            else
            {
                dgvBills.DataSource = null;
                dgvLedger.DataSource = null;
            }
        }

        private void LoadBillsForParty(int partyId)
        {
            try
            {
                _billsForParty = BillService.GetAllBillsForParty(partyId);
                dgvBills.DataSource = _billsForParty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bills: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                var transactions = LedgerService.GetTransactionsForBill(bill.BillID);
                _currentLedgerEntries = new List<TransactionViewModel>();
                decimal runningBalance = 0;

                foreach (var tx in transactions)
                {
                    runningBalance += tx.DebitAmount - tx.CreditAmount;
                    _currentLedgerEntries.Add(new TransactionViewModel
                    {
                        TransactionDate = tx.TransactionDate,
                        TransactionType = tx.TransactionType,
                        Description = tx.Description,
                        DebitAmount = tx.DebitAmount,
                        CreditAmount = tx.CreditAmount,
                        RunningBalance = runningBalance
                    });
                }

                dgvLedger.DataSource = _currentLedgerEntries;
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
            if (_selectedBill == null || !_currentLedgerEntries.Any())
            {
                MessageBox.Show("Please select a bill with transactions to print.", "No Bill Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string htmlContent = GenerateHtmlReport(_selectedBill, _currentLedgerEntries);
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
                sb.AppendLine($"<td class='text-right'>{entry.DebitAmount:N2}</td>");
                sb.AppendLine($"<td class='text-right'>{entry.CreditAmount:N2}</td>");
                sb.AppendLine($"<td class='text-right'>{entry.RunningBalance:N2}</td>");
                sb.AppendLine("</tr>");
            }

            // --- Totals Row ---
            var totalDebit = ledgerEntries.Sum(e => e.DebitAmount);
            var totalCredit = ledgerEntries.Sum(e => e.CreditAmount);
            var finalBalance = ledgerEntries.LastOrDefault()?.RunningBalance ?? 0;

            sb.AppendLine("<tr class='total-row'>");
            sb.AppendLine("<td colspan='3'><strong>Totals</strong></td>");
            sb.AppendLine($"<td class='text-right'><strong>{totalDebit:N2}</strong></td>");
            sb.AppendLine($"<td class='text-right'><strong>{totalCredit:N2}</strong></td>");
            sb.AppendLine($"<td class='text-right'><strong>{finalBalance:N2}</strong></td>");
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
        }

        #endregion
    }
}