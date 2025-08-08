using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class BillListUserControl : UserControl
    {
        private List<Bill> _allBills;
        private List<Party> _parties;
        private List<Broker> _brokers;
        private DataGridViewColumn _sortedColumn;
        private SortOrder _sortOrder = SortOrder.None;


        public BillListUserControl()
        {
            InitializeComponent();
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

                // Update status for each bill based on due amount and populate broker names from broker ID
                foreach (var bill in _allBills)
                {
                    // Calculate balance from transaction ledger
                    decimal dueAmount = LedgerService.GetDueAmount(bill.BillID);
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
            List<Bill> filteredBills = _allBills;

            // Apply date filter
            DateTime startDate = dtpStartDate.Value.Date;
            DateTime endDate = dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1); // Include entire end date

            filteredBills = filteredBills.Where(b =>
                b.BillDate >= startDate &&
                b.BillDate <= endDate
            ).ToList();

            // Apply status filter if not "All"
            string selectedStatus = cmbStatus.SelectedItem.ToString();
            if (selectedStatus != "All")
            {
                filteredBills = filteredBills.Where(b => b.Status == selectedStatus).ToList();
            }

            // Apply text filter if specified
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredBills = filteredBills.Where(b =>
                    b.BillNo.ToLower().Contains(searchText) ||
                    b.PartyName.ToLower().Contains(searchText) ||
                    (b.BrokerName?.ToLower().Contains(searchText) ?? false)
                ).ToList();
            }

            dgvBills.DataSource = null;
            dgvBills.DataSource = filteredBills;

            // Calculate and display totals
            UpdateTotals(filteredBills);
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

            // Sort the data based on the column's DataPropertyName
            switch (column.DataPropertyName)
            {
                case "BillNo":
                    _allBills = (_sortOrder == SortOrder.Ascending) ? _allBills.OrderBy(b => b.BillNo).ToList() : _allBills.OrderByDescending(b => b.BillNo).ToList();
                    break;
                case "BillDate":
                    _allBills = (_sortOrder == SortOrder.Ascending) ? _allBills.OrderBy(b => b.BillDate).ToList() : _allBills.OrderByDescending(b => b.BillDate).ToList();
                    break;
                case "PartyName":
                    _allBills = (_sortOrder == SortOrder.Ascending) ? _allBills.OrderBy(b => b.PartyName).ToList() : _allBills.OrderByDescending(b => b.PartyName).ToList();
                    break;
                case "BrokerName":
                    _allBills = (_sortOrder == SortOrder.Ascending) ? _allBills.OrderBy(b => b.BrokerName).ToList() : _allBills.OrderByDescending(b => b.BrokerName).ToList();
                    break;
                case "OriginalAmount":
                    _allBills = (_sortOrder == SortOrder.Ascending) ? _allBills.OrderBy(b => b.OriginalAmount).ToList() : _allBills.OrderByDescending(b => b.OriginalAmount).ToList();
                    break;
                case "Balance":
                    _allBills = (_sortOrder == SortOrder.Ascending) ? _allBills.OrderBy(b => b.Balance).ToList() : _allBills.OrderByDescending(b => b.Balance).ToList();
                    break;
                case "Status":
                    _allBills = (_sortOrder == SortOrder.Ascending) ? _allBills.OrderBy(b => b.Status).ToList() : _allBills.OrderByDescending(b => b.Status).ToList();
                    break;
            }

            // Update the sort glyph on the header cell
            column.HeaderCell.SortGlyphDirection = _sortOrder;
            RefreshGrid();
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
    }
}
