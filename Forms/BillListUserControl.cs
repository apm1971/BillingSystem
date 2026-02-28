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

        private void SetupPartyAndBrokerFilters()
        {
            try
            {
                // Setup Party filter - similar to BillLedgerControl
                var allParties = new List<Party> { new Party { PartyID = 0, PartyName = "All Parties" } };
                allParties.AddRange(_parties);

                cmbParty.DataSource = allParties;
                cmbParty.DisplayMember = "PartyName";
                cmbParty.ValueMember = "PartyID";
                cmbParty.SelectedIndex = 0; // Select "All Parties" by default

                // Setup Broker filter - similar to BillLedgerControl
                var allBrokers = new List<Broker> { new Broker { BrokerID = 0, BrokerName = "All Brokers" } };
                allBrokers.AddRange(_brokers);

                cmbBroker.DataSource = allBrokers;
                cmbBroker.DisplayMember = "BrokerName";
                cmbBroker.ValueMember = "BrokerID";
                cmbBroker.SelectedIndex = 0; // Select "All Brokers" by default
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting up filters: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupForm()
        {
            // Setup Party and Broker filters
            SetupPartyAndBrokerFilters();

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
            // btnRefresh.Click += BtnRefresh_Click;
            btnPrint.Click += BtnPrint_Click;

            // Add event handlers for Party and Broker filters
            cmbParty.SelectedIndexChanged += CmbParty_SelectedIndexChanged;
            cmbBroker.SelectedIndexChanged += CmbBroker_SelectedIndexChanged;

            dgvBills.CellDoubleClick += DgvBills_CellDoubleClick;
            this.KeyDown += BillListUserControl_KeyDown;

            // --- NEW: Add event handler for column header click for sorting ---
            dgvBills.ColumnHeaderMouseClick += DgvBills_ColumnHeaderMouseClick;

            // Add custom cell formatting for balance styling
            dgvBills.CellFormatting += DgvBills_CellFormatting;

            // Initialize date pickers
            dtpStartDate.Value = DateTime.Now.AddMonths(-3);
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

        private void CmbParty_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void CmbBroker_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            int? selectedPartyId = cmbParty.SelectedValue as int?;
            int? selectedBrokerId = cmbBroker.SelectedValue as int?;

            // Use more efficient LINQ query that creates a single filtered list
            _filteredBills = _allBills
                .Where(b =>
                    // Date filter
                    b.BillDate >= dtpStartDate.Value.Date &&
                    b.BillDate <= dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1) &&
                    // Status filter
                    (cmbStatus.SelectedItem.ToString() == "All" || b.Status == cmbStatus.SelectedItem.ToString()) &&
                    // Party filter
                    (!selectedPartyId.HasValue || selectedPartyId.Value == 0 || b.PartyID == selectedPartyId.Value) &&
                    // Broker filter
                    (!selectedBrokerId.HasValue || selectedBrokerId.Value == 0 || b.BrokerID == selectedBrokerId.Value)
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
                    // Secondary sort by date within the same party
                    _filteredBills = (_sortOrder == SortOrder.Ascending) 
                        ? _filteredBills.OrderBy(b => b.PartyName).ThenBy(b => b.BillDate).ToList() 
                        : _filteredBills.OrderByDescending(b => b.PartyName).ThenBy(b => b.BillDate).ToList();
                    break;
                case "BrokerName":
                    // Secondary sort by date within the same broker
                    _filteredBills = (_sortOrder == SortOrder.Ascending) 
                        ? _filteredBills.OrderBy(b => b.BrokerName).ThenBy(b => b.BillDate).ToList() 
                        : _filteredBills.OrderByDescending(b => b.BrokerName).ThenBy(b => b.BillDate).ToList();
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
        // private void BtnRefresh_Click(object sender, EventArgs e)
        // {
        //     LoadData();
        //     RefreshGrid();
        // }

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
                        details.AppendLine($"  Qty: {item.Quantity} × ₹{item.Rate:N2} = ₹{item.Amount:N2}");
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

                    // Create a custom dialog with View and Print buttons
                    ShowBillDetailDialog(selectedBill, billItems, details.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading bill details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Shows a dialog with bill details and a Print button for thermal printer
        /// </summary>
        private void ShowBillDetailDialog(Bill bill, List<BillItem> billItems, string detailsText)
        {
            var detailForm = new Form
            {
                Text = $"Bill Details - {bill.BillNo}",
                Size = new Size(500, 550),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowIcon = false
            };

            // Create text box to show details
            var txtDetails = new TextBox
            {
                Text = detailsText,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 10f),
                Location = new Point(10, 10),
                Size = new Size(464, 440),
                BackColor = Color.White
            };

            // Create button panel
            var buttonPanel = new Panel
            {
                Location = new Point(10, 460),
                Size = new Size(464, 40)
            };

            // Print button for 3-inch thermal printer
            var btnPrint = new Button
            {
                Text = "🖨️ Print (3\" Thermal)",
                Size = new Size(150, 35),
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.Click += (s, e) => PrintBillSlip(bill, billItems);

            // Close button
            var btnClose = new Button
            {
                Text = "Close",
                Size = new Size(100, 35),
                Location = new Point(364, 0),
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;

            buttonPanel.Controls.Add(btnPrint);
            buttonPanel.Controls.Add(btnClose);

            detailForm.Controls.Add(txtDetails);
            detailForm.Controls.Add(buttonPanel);
            detailForm.CancelButton = btnClose;

            detailForm.ShowDialog(this);
        }

        /// <summary>
        /// Generates and prints a bill slip optimized for 3-inch thermal printer (72mm / ~288 pixels)
        /// </summary>
        private void PrintBillSlip(Bill bill, List<BillItem> billItems)
        {
            try
            {
                string htmlContent = GenerateThermalBillSlip(bill, billItems);
                string tempFilePath = Path.Combine(Path.GetTempPath(), $"BillSlip_{bill.BillNo}_{DateTime.Now:yyyyMMdd_HHmmss}.html");
                File.WriteAllText(tempFilePath, htmlContent, Encoding.UTF8);

                // Open the file in the default web browser for printing
                Process.Start(new ProcessStartInfo(tempFilePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not generate or open the bill slip: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Generates HTML content optimized for 3-inch (72mm) thermal printer
        /// </summary>
        private string GenerateThermalBillSlip(Bill bill, List<BillItem> billItems)
        {
            var sb = new StringBuilder();

            // 3-inch thermal printer width is approximately 72mm (288 pixels at 100 DPI)
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head>");
            sb.AppendLine("<meta charset='UTF-8'>");
            sb.AppendLine("<title>Bill Slip</title>");
            sb.AppendLine("<style>");
            // Thermal printer styling for 3-inch width
            sb.AppendLine("@page { size: 72mm auto; margin: 2mm; }");
            sb.AppendLine("@media print { body { width: 72mm; } }");
            sb.AppendLine("body { font-family: 'Courier New', monospace; font-size: 10pt; width: 72mm; margin: 0 auto; padding: 2mm; line-height: 1.2; }");
            sb.AppendLine(".header { text-align: center; border-bottom: 1px dashed #000; padding-bottom: 3mm; margin-bottom: 2mm; }");
            sb.AppendLine(".header h2 { margin: 0; font-size: 12pt; }");
            sb.AppendLine(".header p { margin: 1mm 0; font-size: 9pt; }");
            sb.AppendLine(".info-row { display: flex; justify-content: space-between; font-size: 9pt; margin: 1mm 0; }");
            sb.AppendLine(".info-label { font-weight: bold; }");
            sb.AppendLine(".divider { border-bottom: 1px dashed #000; margin: 2mm 0; }");
            sb.AppendLine(".item { margin: 2mm 0; padding-bottom: 1mm; border-bottom: 1px dotted #ccc; }");
            sb.AppendLine(".item-name { font-weight: bold; font-size: 9pt; }");
            sb.AppendLine(".item-details { font-size: 8pt; margin-left: 2mm; }");
            sb.AppendLine(".item-amount { text-align: right; font-size: 9pt; }");
            sb.AppendLine(".totals { border-top: 1px dashed #000; margin-top: 2mm; padding-top: 2mm; }");
            sb.AppendLine(".total-row { display: flex; justify-content: space-between; font-size: 9pt; margin: 1mm 0; }");
            sb.AppendLine(".total-row.grand { font-weight: bold; font-size: 11pt; border-top: 1px solid #000; padding-top: 2mm; margin-top: 2mm; }");
            sb.AppendLine(".balance { font-weight: bold; font-size: 11pt; color: #c00; }");
            sb.AppendLine(".footer { text-align: center; margin-top: 3mm; font-size: 8pt; border-top: 1px dashed #000; padding-top: 2mm; }");
            sb.AppendLine(".status-paid { color: #090; }");
            sb.AppendLine(".status-partial { color: #00f; }");
            sb.AppendLine(".status-unpaid { color: #c00; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head><body>");

            // Header
            sb.AppendLine("<div class='header'>");
            sb.AppendLine("<h2>SALE BILL</h2>");
            sb.AppendLine($"<p><strong>{bill.BillNo}</strong></p>");
            sb.AppendLine($"<p>{bill.BillDate:dd-MM-yyyy}</p>");
            sb.AppendLine("</div>");

            // Party and Broker info
            sb.AppendLine("<div class='info-section'>");
            sb.AppendLine($"<div class='info-row'><span class='info-label'>Party:</span><span>{bill.PartyName}</span></div>");
            if (!string.IsNullOrEmpty(bill.BrokerName) && bill.BrokerName != "No Broker")
            {
                sb.AppendLine($"<div class='info-row'><span class='info-label'>Broker:</span><span>{bill.BrokerName}</span></div>");
            }

            string statusClass = bill.Status switch
            {
                "Paid" => "status-paid",
                "Partial" => "status-partial",
                "Unpaid" => "status-unpaid",
                _ => ""
            };
            sb.AppendLine($"<div class='info-row'><span class='info-label'>Status:</span><span class='{statusClass}'>{bill.Status}</span></div>");
            sb.AppendLine("</div>");

            sb.AppendLine("<div class='divider'></div>");

            // Items
            sb.AppendLine("<div class='items'>");
            decimal totalItemAmount = 0;
            decimal totalItemCharges = 0;

            foreach (var item in billItems)
            {
                sb.AppendLine("<div class='item'>");
                sb.AppendLine($"<div class='item-name'>{item.ItemName}</div>");
                sb.AppendLine($"<div class='item-details'>{item.Quantity} x ₹{item.Rate:N2}</div>");
                if (item.Charges > 0)
                {
                    sb.AppendLine($"<div class='item-details'>+ Charges: ₹{item.Charges:N2}</div>");
                }
                sb.AppendLine($"<div class='item-amount'>₹{item.TotalAmount:N2}</div>");
                sb.AppendLine("</div>");

                totalItemAmount += item.Amount;
                totalItemCharges += item.Charges;
            }
            sb.AppendLine("</div>");

            // Totals
            sb.AppendLine("<div class='totals'>");
            sb.AppendLine($"<div class='total-row'><span>Subtotal:</span><span>₹{totalItemAmount:N2}</span></div>");
            if (totalItemCharges > 0)
            {
                sb.AppendLine($"<div class='total-row'><span>Item Charges:</span><span>₹{totalItemCharges:N2}</span></div>");
            }
            if (bill.AdditionalCharges > 0)
            {
                sb.AppendLine($"<div class='total-row'><span>Add. Charges:</span><span>₹{bill.AdditionalCharges:N2}</span></div>");
            }
            sb.AppendLine($"<div class='total-row grand'><span>NET AMOUNT:</span><span>₹{bill.TotalAmount:N2}</span></div>");
            
            if (bill.Balance > 0)
            {
                sb.AppendLine($"<div class='total-row balance'><span>BALANCE DUE:</span><span>₹{bill.Balance:N2}</span></div>");
            }
            else if (bill.Balance == 0)
            {
                sb.AppendLine("<div class='total-row' style='color:#090;'><span>PAID IN FULL</span><span>✓</span></div>");
            }
            sb.AppendLine("</div>");

            // Notes
            if (!string.IsNullOrEmpty(bill.Notes))
            {
                sb.AppendLine("<div class='divider'></div>");
                sb.AppendLine($"<div style='font-size:8pt;'><strong>Notes:</strong> {bill.Notes}</div>");
            }

            // Footer
            sb.AppendLine("<div class='footer'>");
            sb.AppendLine($"<p>Printed: {DateTime.Now:dd-MM-yyyy HH:mm}</p>");
            sb.AppendLine("<p>Thank You!</p>");
            sb.AppendLine("</div>");

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private void DeleteSelectedBill()
        {
            if (dgvBills.CurrentRow?.DataBoundItem is Bill selectedBill)
            {
                // Check permissions before allowing delete
                if (!PermissionManager.ValidateDeleteOperation(ModuleType.Bills, "bill"))
                    return;

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
                // BtnRefresh_Click(sender, e);
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
            var sb = new StringBuilder();

            // Get filter details for the report header
            string statusFilter = cmbStatus.SelectedItem?.ToString() ?? "All";
            string dateRange = $"From: {dtpStartDate.Value:dd/MM/yyyy} To: {dtpEndDate.Value:dd/MM/yyyy}";
            string partyFilter = cmbParty.SelectedItem?.ToString() ?? "All Parties";
            string brokerFilter = cmbBroker.SelectedItem?.ToString() ?? "All Brokers";

            // --- HTML and CSS Styling ---
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><title>Bill List Report</title>");
            sb.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            sb.AppendLine("<style>");
            sb.AppendLine("@page { size: A4; margin: 10mm; }");
            sb.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; margin: 10px; font-size: 9pt; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 10px; }");
            sb.AppendLine("th, td { border: 1px solid #ccc; padding: 4px; text-align: left; font-size: 8pt; }");
            sb.AppendLine("th { background-color: #f2f2f2; font-weight: bold; }");
            sb.AppendLine(".compact-header { display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid #333; padding: 5px 0; margin-bottom: 10px; }");
            sb.AppendLine(".header-left { width: 40%; }");
            sb.AppendLine(".header-right { width: 58%; text-align: right; }");
            sb.AppendLine(".text-right { text-align: right; }");
            sb.AppendLine(".total-row { font-weight: bold; background-color: #f8f8f8; }");
            sb.AppendLine("h1 { margin: 0; color: #333; font-size: 14pt; }");
            sb.AppendLine("h2 { margin: 0; color: #333; font-size: 11pt; }");
            sb.AppendLine("h3 { margin: 0; color: #333; font-size: 10pt; }");
            sb.AppendLine("p { margin: 2px 0; font-size: 8pt; }");
            sb.AppendLine(".filter-info { background-color: #f0f8ff; padding: 5px; border-radius: 3px; margin: 5px 0; border: 1px solid #ddd; font-size: 8pt; }");
            sb.AppendLine(".summary-box { background-color: #f5f5f5; padding: 5px; border-radius: 3px; margin: 5px 0; border: 1px solid #ddd; font-size: 8pt; }");
            sb.AppendLine(".status-paid { color: #28a745; font-weight: bold; }");
            sb.AppendLine(".status-partial { color: #007bff; font-weight: bold; }");
            sb.AppendLine(".status-unpaid { color: #dc3545; font-weight: bold; }");
            sb.AppendLine(".compact-row { display: flex; justify-content: space-between; margin: 1px 0; }");
            sb.AppendLine(".compact-col { flex: 1; margin: 0 2px; }");
            sb.AppendLine(".compact-table { font-size: 7pt; }");
            sb.AppendLine(".compact-table th, .compact-table td { padding: 2px 3px; }");
            sb.AppendLine("</style></head><body>");

            // --- Compact Report Header ---
            sb.AppendLine("<div class='compact-header'>");
            sb.AppendLine("<div class='header-left'>");
            sb.AppendLine($"<h1>Bill List Report</h1>");
            sb.AppendLine($"<p style='margin: 1px 0; font-size: 8pt;'><strong>Generated:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class='header-right'>");
            sb.AppendLine($"<h2>Summary</h2>");
            sb.AppendLine($"<div class='compact-row'>");
            sb.AppendLine($"<div class='compact-col'><strong>Bills:</strong> {bills.Count}</div>");
            sb.AppendLine($"<div class='compact-col'><strong>Amount:</strong> ₹{bills.Sum(b => b.OriginalAmount):N0}</div>");
            sb.AppendLine("</div>");
            sb.AppendLine($"<div class='compact-row'>");
            sb.AppendLine($"<div class='compact-col'><strong>Balance:</strong> ₹{bills.Sum(b => b.Balance):N0}</div>");
            sb.AppendLine($"<div class='compact-col'><strong>Charges:</strong> ₹{bills.Sum(b => b.AdditionalCharges):N0}</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            // --- Compact Filter Information ---
            sb.AppendLine("<div class='filter-info'>");
            sb.AppendLine($"<div class='compact-row'>");
            sb.AppendLine($"<div class='compact-col'><strong>Date Range:</strong> {dateRange}</div>");
            sb.AppendLine($"<div class='compact-col'><strong>Status:</strong> {statusFilter}</div>");
            sb.AppendLine("</div>");
            sb.AppendLine($"<div class='compact-row'>");
            sb.AppendLine($"<div class='compact-col'><strong>Party:</strong> {partyFilter}</div>");
            sb.AppendLine($"<div class='compact-col'><strong>Broker:</strong> {brokerFilter}</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            // --- Compact Bills Table ---
            sb.AppendLine("<table class='compact-table'>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th style='width: 12%;'>Bill No</th>");
            sb.AppendLine("<th style='width: 8%;'>Date</th>");
            sb.AppendLine("<th style='width: 20%;'>Party</th>");
            sb.AppendLine("<th style='width: 15%;'>Broker</th>");
            sb.AppendLine("<th style='width: 10%;' class='text-right'>Amount</th>");
            sb.AppendLine("<th style='width: 10%;' class='text-right'>Charges</th>");
            sb.AppendLine("<th style='width: 10%;' class='text-right'>Cheque1</th>");
            sb.AppendLine("<th style='width: 10%;' class='text-right'>Cheque2</th>");
            sb.AppendLine("<th style='width: 10%;' class='text-right'>Balance</th>");
            sb.AppendLine("<th style='width: 5%;'>Status</th>");
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
                sb.AppendLine($"<td>{bill.BillDate:dd/MM/yy}</td>");
                sb.AppendLine($"<td>{bill.PartyName}</td>");
                sb.AppendLine($"<td>{bill.BrokerName ?? "No Broker"}</td>");
                sb.AppendLine($"<td class='text-right'>₹{bill.OriginalAmount:N0}</td>");
                sb.AppendLine($"<td class='text-right'>₹{bill.AdditionalCharges:N0}</td>");
                sb.AppendLine($"<td class='text-right'>₹{bill.ChequeAmountFirm1:N0}</td>");
                sb.AppendLine($"<td class='text-right'>₹{bill.ChequeAmountFirm2:N0}</td>");
                sb.AppendLine($"<td class='text-right'>₹{bill.Balance:N0}</td>");
                sb.AppendLine($"<td class='{statusClass}'>{bill.Status}</td>");
                sb.AppendLine("</tr>");
            }

            // --- Compact Summary Row ---
            sb.AppendLine("<tr class='total-row'>");
            sb.AppendLine("<td colspan='4'><strong>Total</strong></td>");
            sb.AppendLine($"<td class='text-right'><strong>₹{bills.Sum(b => b.OriginalAmount):N0}</strong></td>");
            sb.AppendLine($"<td class='text-right'><strong>₹{bills.Sum(b => b.AdditionalCharges):N0}</strong></td>");
            sb.AppendLine($"<td class='text-right'><strong>₹{bills.Sum(b => b.ChequeAmountFirm1):N0}</strong></td>");
            sb.AppendLine($"<td class='text-right'><strong>₹{bills.Sum(b => b.ChequeAmountFirm2):N0}</strong></td>");
            sb.AppendLine($"<td class='text-right'><strong>₹{bills.Sum(b => b.Balance):N0}</strong></td>");
            sb.AppendLine("<td></td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</table>");

            // --- Compact Summary Box ---
            sb.AppendLine("<div class='summary-box'>");
            sb.AppendLine("<h3>Status Summary</h3>");

            var statusGroups = bills.GroupBy(b => b.Status).OrderBy(g => g.Key);
            sb.AppendLine("<div class='compact-row'>");
            foreach (var group in statusGroups)
            {
                decimal groupAmount = group.Sum(b => b.OriginalAmount);
                decimal groupBalance = group.Sum(b => b.Balance);
                int groupCount = group.Count();

                sb.AppendLine($"<div class='compact-col'><strong>{group.Key}:</strong> {groupCount} bills<br>₹{groupAmount:N0} | ₹{groupBalance:N0}</div>");
            }
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        #endregion


        private void lblStatus_Click(object sender, EventArgs e)
        {

        }
    }
}
