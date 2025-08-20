using System;
using System.Collections.Generic;
using System.Data;
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
    public partial class BillListUserControl : UserControl
    {
        private List<Bill> _allBills;
        private List<Party> _parties;
        private List<Broker> _brokers;
        private DataGridViewColumn _sortedColumn;
        private SortOrder _sortOrder = SortOrder.None;
        
        // Pagination properties
        private int _pageSize = 100;
        private int _currentPage = 1;
        private List<Bill> _filteredBills;
        private Button btnPrevPage;
        private Button btnNextPage;
        private Label lblPageInfo;

        public BillListUserControl()
        {
            InitializeComponent();
            InitializePaginationControls();
        }

        private void InitializePaginationControls()
        {
            // Create pagination panel at the bottom
            Panel paginationPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40
            };

            // Create Previous Page button
            btnPrevPage = new Button
            {
                Text = "< Previous",
                Width = 100,
                Location = new Point(10, 7),
                Enabled = false
            };
            btnPrevPage.Click += BtnPrevPage_Click;

            // Create Next Page button
            btnNextPage = new Button
            {
                Text = "Next >",
                Width = 100,
                Location = new Point(paginationPanel.Width - 110, 7),
                Anchor = AnchorStyles.Right
            };
            btnNextPage.Click += BtnNextPage_Click;

            // Create page info label
            lblPageInfo = new Label
            {
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Text = "Page 1"
            };

            // Add controls to panel
            paginationPanel.Controls.Add(btnPrevPage);
            paginationPanel.Controls.Add(btnNextPage);
            paginationPanel.Controls.Add(lblPageInfo);

            // Add panel to form
            this.Controls.Add(paginationPanel);
        }

        private void BtnPrevPage_Click(object sender, EventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                DisplayCurrentPage();
            }
        }

        private void BtnNextPage_Click(object sender, EventArgs e)
        {
            int totalPages = GetTotalPages();
            if (_currentPage < totalPages)
            {
                _currentPage++;
                DisplayCurrentPage();
            }
        }

        private int GetTotalPages()
        {
            if (_filteredBills == null) return 1;
            return (int)Math.Ceiling(_filteredBills.Count / (double)_pageSize);
        }

        private void DisplayCurrentPage()
        {
            if (_filteredBills == null) return;

            int startIndex = (_currentPage - 1) * _pageSize;
            var currentPageItems = _filteredBills.Skip(startIndex).Take(_pageSize).ToList();

            // Update data source with just the current page of items
            dgvBills.DataSource = null;
            dgvBills.DataSource = currentPageItems;

            // Update pagination controls
            btnPrevPage.Enabled = _currentPage > 1;
            btnNextPage.Enabled = _currentPage < GetTotalPages();
            lblPageInfo.Text = $"Page {_currentPage} of {GetTotalPages()} ({_filteredBills.Count} items)";
        }

        private void BillListUserControl_Load(object sender, EventArgs e)
        {
            LoadData();
            SetupForm();
        }

        private void LoadData()
        {
            try
            {
                // Load all bills with their related party information
                _allBills = BillService.GetAllBills();
                _parties = PartyService.GetAllParties();
                _brokers = BrokerService.GetAllBrokers();

                // Get all bill balances in one database query instead of querying individually
                var allBillBalances = LedgerService.GetAllBillBalances();

                // Update status for each bill based on due amount and populate broker names from broker ID
                foreach (var bill in _allBills)
                {
                    // Use pre-calculated balance from the dictionary
                    decimal dueAmount = allBillBalances.ContainsKey(bill.BillID) ? allBillBalances[bill.BillID] : 0m;
                    bill.Balance = dueAmount;

                    // Look up broker name from broker ID
                    if (bill.BrokerID.HasValue && bill.BrokerID.Value > 0)
                    {
                        var broker = _brokers.FirstOrDefault(b => b.BrokerID == bill.BrokerID.Value);
                        bill.BrokerName = broker?.BrokerName ?? "Unknown Broker";
                    }
                    else
                    {
                        bill.BrokerName = "No Broker";
                    }

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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupForm()
        {
            // Setup DataGridView
            dgvBills.AutoGenerateColumns = false;
            dgvBills.AllowUserToAddRows = false;
            dgvBills.AllowUserToDeleteRows = false;
            dgvBills.ReadOnly = true;
            dgvBills.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBills.MultiSelect = false;
            dgvBills.BackgroundColor = Color.White;
            dgvBills.BorderStyle = BorderStyle.Fixed3D;
            dgvBills.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(64, 64, 64);
            dgvBills.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvBills.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            dgvBills.EnableHeadersVisualStyles = false;
            dgvBills.GridColor = Color.LightGray;
            dgvBills.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);

            // Setup columns
            dgvBills.Columns.Clear();
            dgvBills.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "BillNo", HeaderText = "Bill No", DataPropertyName = "BillNo", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "BillDate", HeaderText = "Bill Date", DataPropertyName = "BillDate", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" } },
                new DataGridViewTextBoxColumn { Name = "PartyName", HeaderText = "Party", DataPropertyName = "PartyName", Width = 200 },
                new DataGridViewTextBoxColumn { Name = "BrokerName", HeaderText = "Broker", DataPropertyName = "BrokerName", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "OriginalAmount", HeaderText = "Amount", DataPropertyName = "OriginalAmount", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
                new DataGridViewTextBoxColumn { Name = "AdditionalCharges", HeaderText = "Charges", DataPropertyName = "AdditionalCharges", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
                new DataGridViewTextBoxColumn { Name = "ChequeAmountFirm1", HeaderText = "Cheque Firm1", DataPropertyName = "ChequeAmountFirm1", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, BackColor = Color.LightCyan } },
                new DataGridViewTextBoxColumn { Name = "ChequeAmountFirm2", HeaderText = "Cheque Firm2", DataPropertyName = "ChequeAmountFirm2", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, BackColor = Color.LightCyan } },
                new DataGridViewTextBoxColumn { Name = "Balance", HeaderText = "Balance", DataPropertyName = "Balance", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold) } },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
            });



            // Setup buttons and event handlers
            btnNewBill.Click += BtnNewBill_Click;
            btnEditBill.Click += BtnEditBill_Click;
            btnViewDetails.Click += BtnViewDetails_Click;
            btnDeleteBill.Click += BtnDeleteBill_Click;
            btnRefresh.Click += BtnRefresh_Click;
            btnPrint.Click += BtnPrint_Click;

            txtSearch.TextChanged += TxtSearch_TextChanged;
            dgvBills.CellDoubleClick += DgvBills_CellDoubleClick;
            this.KeyDown += BillListUserControl_KeyDown;

            // --- NEW: Add event handler for column header click for sorting ---
            dgvBills.ColumnHeaderMouseClick += DgvBills_ColumnHeaderMouseClick;

            // Add custom cell formatting for balance styling
            dgvBills.CellFormatting += DgvBills_CellFormatting;

            // Initialize date pickers
            dtpStartDate.Value = DateTime.Now.AddMonths(-1);
            dtpEndDate.Value = DateTime.Now;

            // Add event handlers for date pickers to update list dynamically
            dtpStartDate.ValueChanged += DatePicker_ValueChanged;
            dtpEndDate.ValueChanged += DatePicker_ValueChanged;

            // Setup status filter
            cmbStatus.SelectedIndex = 0; // Select "All" by default
            cmbStatus.SelectedIndexChanged += CmbStatus_SelectedIndexChanged;

            // Add tooltip for print button
            var toolTip = new ToolTip();
            toolTip.SetToolTip(btnPrint, "Print Bill List Report (Ctrl+P)");

            RefreshGrid();
        }

        private void CmbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            // When status selection changes, update the filter
            ApplyFilters();
        }

        private void DatePicker_ValueChanged(object sender, EventArgs e)
        {
            // When dates change, automatically update the filter
            ApplyFilters();
        }

        private void RefreshGrid()
        {
            // Apply filters, which now handles pagination as well
            ApplyFilters();
        }

        private void DgvBills_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Handle balance column styling
            if (e.ColumnIndex == dgvBills.Columns["Balance"].Index && e.Value != null)
            {
                if (decimal.TryParse(e.Value.ToString(), out decimal balance))
                {
                    if (balance > 0)
                    {
                        // Red color for unpaid amounts
                        e.CellStyle.ForeColor = Color.Red;
                        e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                    }
                    else if (balance == 0)
                    {
                        // Green color for paid amounts
                        e.CellStyle.ForeColor = Color.Green;
                        e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                    }
                    else
                    {
                        // Blue color for overpaid amounts (credit)
                        e.CellStyle.ForeColor = Color.Blue;
                        e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                    }
                }
            }

            // Handle status column styling
            if (e.ColumnIndex == dgvBills.Columns["Status"].Index && e.Value != null)
            {
                string status = e.Value.ToString();

                switch (status)
                {
                    case "Paid":
                        e.CellStyle.ForeColor = Color.Green;
                        e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                        break;
                    case "Partial":
                        e.CellStyle.ForeColor = Color.Blue;
                        e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                        break;
                    case "Unpaid":
                        e.CellStyle.ForeColor = Color.Red;
                        e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                        break;
                }
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            string searchText = txtSearch.Text.ToLower().Trim();
            
            // Use more efficient LINQ query that creates a single filtered list
            _filteredBills = _allBills
                .Where(b => 
                    // Date filter
                    b.BillDate >= dtpStartDate.Value.Date && 
                    b.BillDate <= dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1) && 
                    // Status filter
                    (cmbStatus.SelectedItem.ToString() == "All" || b.Status == cmbStatus.SelectedItem.ToString()) &&
                    // Text search
                    (string.IsNullOrWhiteSpace(searchText) || 
                     b.BillNo.ToLower().Contains(searchText) || 
                     b.PartyName.ToLower().Contains(searchText) || 
                     (b.BrokerName?.ToLower().Contains(searchText) ?? false))
                )
                .ToList();

            // Reset to first page and display
            _currentPage = 1;
            DisplayCurrentPage();

            // Calculate and display totals using all filtered bills
            UpdateTotals(_filteredBills);
        }

        private void UpdateTotals(List<Bill> bills)
        {
            decimal totalAmount = bills.Sum(b => b.OriginalAmount);
            decimal totalCharges = bills.Sum(b => b.AdditionalCharges);
            decimal netAmount = totalAmount + totalCharges;
            decimal totalBalance = bills.Sum(b => b.Balance);
            decimal totalChequeFirm1 = bills.Sum(b => b.ChequeAmountFirm1);
            decimal totalChequeFirm2 = bills.Sum(b => b.ChequeAmountFirm2);

            // Format with commas for thousand separators and always show 2 decimal places
            // Note: lblTotalAmountValue and lblTotalChargesValue don't exist in the designer
            // Using available labels for display
            lblNetAmountValue.Text = string.Format("₹{0:N2}", netAmount);
            lblTotalBalanceValue.Text = string.Format("₹{0:N2}", totalBalance);
            lblChequeFirm1Value.Text = string.Format("₹{0:N2}", totalChequeFirm1);
            lblChequeFirm2Value.Text = string.Format("₹{0:N2}", totalChequeFirm2);

            // Set color for balance total
            if (totalBalance > 0)
            {
                lblTotalBalanceValue.ForeColor = Color.Red;
            }
            else if (totalBalance == 0)
            {
                lblTotalBalanceValue.ForeColor = Color.Green;
            }
            else
            {
                lblTotalBalanceValue.ForeColor = Color.Blue;
            }
        }

        #region Sorting Logic

        private void DgvBills_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var column = dgvBills.Columns[e.ColumnIndex];

            // Determine the new sort order
            if (_sortedColumn == column)
            {
                _sortOrder = (_sortOrder == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                _sortOrder = SortOrder.Ascending;
                if (_sortedColumn != null)
                {
                    _sortedColumn.HeaderCell.SortGlyphDirection = SortOrder.None;
                }
            }

            _sortedColumn = column;

            // Sort the filtered data based on the column's DataPropertyName
            switch (column.DataPropertyName)
            {
                case "BillNo":
                    _filteredBills = (_sortOrder == SortOrder.Ascending) ? _filteredBills.OrderBy(b => b.BillNo).ToList() : _filteredBills.OrderByDescending(b => b.BillNo).ToList();
                    break;
                case "BillDate":
                    _filteredBills = (_sortOrder == SortOrder.Ascending) ? _filteredBills.OrderBy(b => b.BillDate).ToList() : _filteredBills.OrderByDescending(b => b.BillDate).ToList();
                    break;
                case "PartyName":
                    _filteredBills = (_sortOrder == SortOrder.Ascending) ? _filteredBills.OrderBy(b => b.PartyName).ToList() : _filteredBills.OrderByDescending(b => b.PartyName).ToList();
                    break;
                case "BrokerName":
                    _filteredBills = (_sortOrder == SortOrder.Ascending) ? _filteredBills.OrderBy(b => b.BrokerName).ToList() : _filteredBills.OrderByDescending(b => b.BrokerName).ToList();
                    break;
                case "OriginalAmount":
                    _filteredBills = (_sortOrder == SortOrder.Ascending) ? _filteredBills.OrderBy(b => b.OriginalAmount).ToList() : _filteredBills.OrderByDescending(b => b.OriginalAmount).ToList();
                    break;
                case "Balance":
                    _filteredBills = (_sortOrder == SortOrder.Ascending) ? _filteredBills.OrderBy(b => b.Balance).ToList() : _filteredBills.OrderByDescending(b => b.Balance).ToList();
                    break;
                case "Status":
                    _filteredBills = (_sortOrder == SortOrder.Ascending) ? _filteredBills.OrderBy(b => b.Status).ToList() : _filteredBills.OrderByDescending(b => b.Status).ToList();
                    break;
            }

            // Update the sort glyph on the header cell
            column.HeaderCell.SortGlyphDirection = _sortOrder;
            
            // Reset to first page when sorting changes
            _currentPage = 1;
            DisplayCurrentPage();
        }

        #endregion

        #region Button and Event Handlers

        private void BtnNewBill_Click(object sender, EventArgs e)
        {
            try
            {
                var newBillControl = new SaleBillUserControl();
                newBillControl.BillSaved += (s, args) =>
                {
                    LoadData();
                    RefreshGrid();
                };

                var parentForm = this.FindForm() as MainForm;
                parentForm?.ShowControl(newBillControl);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening new bill: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnEditBill_Click(object sender, EventArgs e) => EditSelectedBill();
        private void BtnViewDetails_Click(object sender, EventArgs e) => ViewSelectedBillDetails();
        private void BtnDeleteBill_Click(object sender, EventArgs e) => DeleteSelectedBill();
        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
            RefreshGrid();
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            if (_filteredBills == null || !_filteredBills.Any())
            {
                MessageBox.Show("No bills to print. Please refresh the data first.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string htmlContent = GenerateHtmlReport(_filteredBills);
                string tempFilePath = Path.Combine(Path.GetTempPath(), $"BillListReport_{DateTime.Now:yyyyMMdd_HHmmss}.html");
                File.WriteAllText(tempFilePath, htmlContent);

                // Open the file in the default web browser
                Process.Start(new ProcessStartInfo(tempFilePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not generate or open the report: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvBills_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                EditSelectedBill();
            }
        }

        private void EditSelectedBill()
        {
            if (dgvBills.CurrentRow?.DataBoundItem is Bill selectedBill)
            {
                // Check if bill can be edited based on status
                if (selectedBill.Status == "Paid" || selectedBill.Status == "Partial")
                {
                    MessageBox.Show(
                        $"Cannot edit bill '{selectedBill.BillNo}' because it has a status of '{selectedBill.Status}'.\n\nOnly unpaid bills can be edited.",
                        "Edit Not Allowed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    var editBillControl = new SaleBillUserControl(selectedBill);
                    editBillControl.BillSaved += (s, args) =>
                    {
                        LoadData();
                        RefreshGrid();
                    };

                    var parentForm = this.FindForm() as MainForm;
                    parentForm?.ShowControl(editBillControl);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening bill for editing: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ViewSelectedBillDetails()
        {
            if (dgvBills.CurrentRow?.DataBoundItem is Bill selectedBill)
            {
                try
                {
                    // Get the bill details using BillService.GetBillDetails
                    var billItems = BillService.GetBillDetails(selectedBill.BillID);

                    // Build the details message
                    var details = new System.Text.StringBuilder();
                    details.AppendLine($"Bill No: {selectedBill.BillNo}");
                    details.AppendLine($"Bill Date: {selectedBill.BillDate:dd/MM/yyyy}");
                    details.AppendLine($"Party: {selectedBill.PartyName}");
                    details.AppendLine($"Broker: {selectedBill.BrokerName}");
                    details.AppendLine($"Status: {selectedBill.Status}");
                    details.AppendLine();
                    details.AppendLine("Items:");
                    details.AppendLine("----------------------------------------");

                    decimal totalItemAmount = 0;
                    decimal totalItemCharges = 0;

                    foreach (var item in billItems)
                    {
                        details.AppendLine($"• {item.ItemName}");
                        details.AppendLine($"  Quantity: {item.Quantity} × Rate: ₹{item.Rate:N2} = ₹{item.Amount:N2}");
                        if (item.Charges > 0)
                        {
                            details.AppendLine($"  Charges: ₹{item.Charges:N2}");
                        }
                        details.AppendLine($"  Total: ₹{item.TotalAmount:N2}");
                        details.AppendLine();

                        totalItemAmount += item.Amount;
                        totalItemCharges += item.Charges;
                    }

                    details.AppendLine("----------------------------------------");
                    details.AppendLine($"Item Total: ₹{totalItemAmount:N2}");
                    details.AppendLine($"Item Charges: ₹{totalItemCharges:N2}");
                    details.AppendLine($"Additional Charges: ₹{selectedBill.AdditionalCharges:N2}");
                    details.AppendLine($"NET AMOUNT: ₹{selectedBill.TotalAmount:N2}");
                    details.AppendLine($"Balance: ₹{selectedBill.Balance:N2}");

                    if (!string.IsNullOrEmpty(selectedBill.Notes))
                    {
                        details.AppendLine();
                        details.AppendLine($"Notes: {selectedBill.Notes}");
                    }

                    MessageBox.Show(details.ToString(), $"Bill Details - {selectedBill.BillNo}",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading bill details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void DeleteSelectedBill()
        {
            if (dgvBills.CurrentRow?.DataBoundItem is Bill selectedBill)
            {
                if (MessageBox.Show($"Are you sure you want to delete bill {selectedBill.BillNo}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        if (BillService.DeleteBill(selectedBill.BillID))
                        {
                            LoadData();
                            RefreshGrid();
                            MessageBox.Show("Bill deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete bill.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting bill: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BillListUserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.N)
            {
                BtnNewBill_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.P)
            {
                BtnPrint_Click(sender, e);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                BtnRefresh_Click(sender, e);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedBill();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                ViewSelectedBillDetails();
                e.Handled = true;
            }
        }

        #endregion

        private void dgvBills_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void lblChequeFirm2_Click(object sender, EventArgs e)
        {

        }

        private void lblTotalBalance_Click(object sender, EventArgs e)
        {

        }

        #region Printing and Exporting

        private string GenerateHtmlReport(List<Bill> bills)
        {
            var company = Program.ActiveCompany; // Assuming you have this in Program.cs
            var sb = new StringBuilder();

            // Get filter details for the report header
            string statusFilter = cmbStatus.SelectedItem?.ToString() ?? "All";
            string dateRange = $"From: {dtpStartDate.Value:dd/MM/yyyy} To: {dtpEndDate.Value:dd/MM/yyyy}";
            string searchFilter = !string.IsNullOrWhiteSpace(txtSearch.Text) ? $"Search: {txtSearch.Text}" : "";

            // --- HTML and CSS Styling ---
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><title>Bill List Report</title>");
            sb.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            sb.AppendLine("<style>");
            sb.AppendLine("@page { size: A4; margin: 15mm; }");
            sb.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; margin: 20px; font-size: 10pt; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
            sb.AppendLine("th, td { border: 1px solid #ccc; padding: 6px; text-align: left; font-size: 9pt; }");
            sb.AppendLine("th { background-color: #f2f2f2; font-weight: bold; }");
            sb.AppendLine(".header { display: flex; justify-content: space-between; border-bottom: 2px solid #333; padding-bottom: 10px; margin-bottom: 20px; }");
            sb.AppendLine(".header-left, .header-right { width: 48%; }");
            sb.AppendLine(".text-right { text-align: right; }");
            sb.AppendLine(".total-row { font-weight: bold; background-color: #f8f8f8; }");
            sb.AppendLine("h1, h2 { margin: 0; color: #333; }");
            sb.AppendLine(".filter-info { background-color: #f0f8ff; padding: 10px; border-radius: 5px; margin: 10px 0; border: 1px solid #ddd; }");
            sb.AppendLine(".summary-box { background-color: #f5f5f5; padding: 10px; border-radius: 5px; margin: 10px 0; border: 1px solid #ddd; }");
            sb.AppendLine(".status-paid { color: #28a745; font-weight: bold; }");
            sb.AppendLine(".status-partial { color: #007bff; font-weight: bold; }");
            sb.AppendLine(".status-unpaid { color: #dc3545; font-weight: bold; }");
            sb.AppendLine("</style></head><body>");

            // --- Report Header ---
            sb.AppendLine("<div class='header'>");
            sb.AppendLine("<div class='header-left'>");
            sb.AppendLine($"<h1>Bill List Report</h1>");
            sb.AppendLine($"<p><strong>Generated:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>");
            // sb.AppendLine($"<p><strong>Company:</strong> {company?.CompanyName ?? "Your Company"}</p>");
            // sb.AppendLine($"<p>{company?.Address?.Replace("\n", "<br>")}</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class='header-right'>");
            sb.AppendLine($"<h2>Report Summary</h2>");
            sb.AppendLine($"<p><strong>Total Bills:</strong> {bills.Count}</p>");
            sb.AppendLine($"<p><strong>Total Amount:</strong> ₹{bills.Sum(b => b.OriginalAmount):N2}</p>");
            sb.AppendLine($"<p><strong>Total Balance:</strong> ₹{bills.Sum(b => b.Balance):N2}</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            // --- Filter Information ---
            sb.AppendLine("<div class='filter-info'>");
            sb.AppendLine("<h3>Filter Details</h3>");
            sb.AppendLine($"<p><strong>Date Range:</strong> {dateRange}</p>");
            sb.AppendLine($"<p><strong>Status Filter:</strong> {statusFilter}</p>");
            if (!string.IsNullOrWhiteSpace(searchFilter))
            {
                sb.AppendLine($"<p><strong>{searchFilter}</strong></p>");
            }
            sb.AppendLine("</div>");

            // --- Bills Table ---
            sb.AppendLine("<table>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th>Bill No</th>");
            sb.AppendLine("<th>Date</th>");
            sb.AppendLine("<th>Party</th>");
            sb.AppendLine("<th>Broker</th>");
            sb.AppendLine("<th class='text-right'>Amount</th>");
            sb.AppendLine("<th class='text-right'>Charges</th>");
            sb.AppendLine("<th class='text-right'>Cheque Firm1</th>");
            sb.AppendLine("<th class='text-right'>Cheque Firm2</th>");
            sb.AppendLine("<th class='text-right'>Balance</th>");
            sb.AppendLine("<th>Status</th>");
            sb.AppendLine("</tr>");

            foreach (var bill in bills)
            {
                string statusClass = bill.Status switch
                {
                    "Paid" => "status-paid",
                    "Partial" => "status-partial",
                    "Unpaid" => "status-unpaid",
                    _ => ""
                };

                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{bill.BillNo}</td>");
                sb.AppendLine($"<td>{bill.BillDate:dd/MM/yyyy}</td>");
                sb.AppendLine($"<td>{bill.PartyName}</td>");
                sb.AppendLine($"<td>{bill.BrokerName ?? "No Broker"}</td>");
                sb.AppendLine($"<td class='text-right'>₹{bill.OriginalAmount:N2}</td>");
                sb.AppendLine($"<td class='text-right'>₹{bill.AdditionalCharges:N2}</td>");
                sb.AppendLine($"<td class='text-right'>₹{bill.ChequeAmountFirm1:N2}</td>");
                sb.AppendLine($"<td class='text-right'>₹{bill.ChequeAmountFirm2:N2}</td>");
                sb.AppendLine($"<td class='text-right'>₹{bill.Balance:N2}</td>");
                sb.AppendLine($"<td class='{statusClass}'>{bill.Status}</td>");
                sb.AppendLine("</tr>");
            }

            // --- Summary Row ---
            sb.AppendLine("<tr class='total-row'>");
            sb.AppendLine("<td colspan='4'><strong>Total</strong></td>");
            sb.AppendLine($"<td class='text-right'><strong>₹{bills.Sum(b => b.OriginalAmount):N2}</strong></td>");
            sb.AppendLine($"<td class='text-right'><strong>₹{bills.Sum(b => b.AdditionalCharges):N2}</strong></td>");
            sb.AppendLine($"<td class='text-right'><strong>₹{bills.Sum(b => b.ChequeAmountFirm1):N2}</strong></td>");
            sb.AppendLine($"<td class='text-right'><strong>₹{bills.Sum(b => b.ChequeAmountFirm2):N2}</strong></td>");
            sb.AppendLine($"<td class='text-right'><strong>₹{bills.Sum(b => b.Balance):N2}</strong></td>");
            sb.AppendLine("<td></td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</table>");

            // --- Summary Box ---
            sb.AppendLine("<div class='summary-box'>");
            sb.AppendLine("<h3>Summary by Status</h3>");
            
            var statusGroups = bills.GroupBy(b => b.Status).OrderBy(g => g.Key);
            foreach (var group in statusGroups)
            {
                decimal groupAmount = group.Sum(b => b.OriginalAmount);
                decimal groupBalance = group.Sum(b => b.Balance);
                int groupCount = group.Count();
                
                sb.AppendLine($"<p><strong>{group.Key}:</strong> {groupCount} bills, Amount: ₹{groupAmount:N2}, Balance: ₹{groupBalance:N2}</p>");
            }
            sb.AppendLine("</div>");

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        #endregion
    }
}
