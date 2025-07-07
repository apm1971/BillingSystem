using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class BillListForm : Form
    {
        private List<Bill> bills = new List<Bill>();
        private List<Bill> filteredBills = new List<Bill>();

        public BillListForm()
        {
            InitializeComponent();
            ConfigureControls();
            SetupDataGrid();
        }

        private void BillListForm_Load(object sender, EventArgs e)
        {
            LoadBills();
        }

        private void ConfigureControls()
        {
            // Set form properties
            this.Text = "Bill List";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Setup date filters
            dtpFromDate.Value = DateTime.Today.AddMonths(-1);
            dtpToDate.Value = DateTime.Today;

            // Setup search functionality
            txtSearch.TextChanged += txtSearch_TextChanged;
            dtpFromDate.ValueChanged += DateFilter_Changed;
            dtpToDate.ValueChanged += DateFilter_Changed;
        }

        private void SetupDataGrid()
        {
            dgvBills.AutoGenerateColumns = false;
            dgvBills.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBills.MultiSelect = false;
            dgvBills.ReadOnly = true;
            dgvBills.AllowUserToAddRows = false;
            dgvBills.AllowUserToDeleteRows = false;

            // Configure columns
            dgvBills.Columns.Clear();

            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BillNo",
                HeaderText = "Bill No",
                DataPropertyName = "BillNo",
                Width = 120
            });

            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BillDate",
                HeaderText = "Bill Date",
                DataPropertyName = "BillDate",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            });

            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DueDate",
                HeaderText = "Due Date",
                DataPropertyName = "DueDate",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            });

            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PartyName",
                HeaderText = "Party Name",
                DataPropertyName = "PartyName",
                Width = 180
            });

            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BrokerName",
                HeaderText = "Broker",
                DataPropertyName = "BrokerName",
                Width = 150
            });

            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TotalAmount",
                HeaderText = "Total Amount",
                DataPropertyName = "TotalAmount",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TotalCharges",
                HeaderText = "Total Charges",
                DataPropertyName = "TotalCharges",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "NetAmount",
                HeaderText = "Net Amount",
                DataPropertyName = "NetAmount",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ItemCount",
                HeaderText = "Items",
                DataPropertyName = "ItemCount",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PaidAmount",
                HeaderText = "Paid Amount",
                DataPropertyName = "PaidAmount",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BalanceAmount",
                HeaderText = "Balance",
                DataPropertyName = "BalanceAmount",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PaymentStatus",
                HeaderText = "Payment Status",
                DataPropertyName = "PaymentStatusText",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Event handlers
            dgvBills.SelectionChanged += dgvBills_SelectionChanged;
            dgvBills.CellDoubleClick += dgvBills_CellDoubleClick;
            dgvBills.KeyDown += dgvBills_KeyDown;
        }

        private void LoadBills()
        {
            try
            {
                bills = BillService.GetAllBills();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bills: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilters()
        {
            try
            {
                filteredBills = bills.Where(b => 
                    b.BillDate.Date >= dtpFromDate.Value.Date && 
                    b.BillDate.Date <= dtpToDate.Value.Date).ToList();

                // Apply search filter
                string searchText = txtSearch.Text.ToLower().Trim();
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    filteredBills = filteredBills.Where(b =>
                        b.BillNo.ToLower().Contains(searchText) ||
                        b.PartyName.ToLower().Contains(searchText) ||
                        (!string.IsNullOrEmpty(b.BrokerName) && b.BrokerName.ToLower().Contains(searchText))
                    ).ToList();
                }

                // Add item count and payment info for display
                var billsWithItemCount = filteredBills.Select(b => new
                {
                    BillID = b.BillID,
                    BillNo = b.BillNo,
                    BillDate = b.BillDate,
                    DueDate = b.DueDate,
                    PartyName = b.PartyName,
                    BrokerName = string.IsNullOrEmpty(b.BrokerName) ? "No Broker" : b.BrokerName,
                    TotalAmount = b.TotalAmount,
                    TotalCharges = b.TotalCharges,
                    NetAmount = b.NetAmount,
                    PaidAmount = b.PaidAmount,
                    BalanceAmount = b.BalanceAmount,
                    PaymentStatusText = b.PaymentStatusText,
                    ItemCount = b.BillItems.Count
                }).ToList();

                dgvBills.DataSource = billsWithItemCount;
                
                lblTotalBills.Text = $"Total Bills: {filteredBills.Count}";
                lblTotalAmount.Text = $"Total Amount: {filteredBills.Sum(b => b.NetAmount):N2}";

                // Enable/disable buttons based on selection
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying filters: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = dgvBills.SelectedRows.Count > 0;
            btnEdit.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;
            btnView.Enabled = hasSelection;
        }

        private Bill GetSelectedBill()
        {
            if (dgvBills.SelectedRows.Count > 0)
            {
                int selectedIndex = dgvBills.SelectedRows[0].Index;
                if (selectedIndex >= 0 && selectedIndex < filteredBills.Count)
                {
                    return filteredBills[selectedIndex];
                }
            }
            return null;
        }

        private void EditSelectedBill()
        {
            var selectedBill = GetSelectedBill();
            if (selectedBill != null)
            {
                var editForm = new SaleBillForm(selectedBill);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadBills(); // Refresh the list
                }
            }
        }

        private void ViewSelectedBill()
        {
            var selectedBill = GetSelectedBill();
            if (selectedBill != null)
            {
                // Create a read-only view or detailed view of the bill
                string overdueStatus = selectedBill.IsOverdue ? " (OVERDUE)" : 
                                     selectedBill.DaysUntilDue <= 3 ? " (DUE SOON)" : "";
                string billDetails = $"Bill Details:\n\n" +
                    $"Bill No: {selectedBill.BillNo}\n" +
                    $"Bill Date: {selectedBill.BillDate:dd/MM/yyyy}\n" +
                    $"Due Date: {selectedBill.DueDate:dd/MM/yyyy}{overdueStatus}\n" +
                    $"Party: {selectedBill.PartyName}\n" +
                    $"Total Amount: {selectedBill.TotalAmount:N2}\n" +
                    $"Total Charges: {selectedBill.TotalCharges:N2}\n" +
                    $"Net Amount: {selectedBill.NetAmount:N2}\n" +
                    $"Number of Items: {selectedBill.BillItems.Count}\n\n";

                if (selectedBill.BillItems.Count > 0)
                {
                    billDetails += "Items:\n";
                    foreach (var item in selectedBill.BillItems)
                    {
                        billDetails += $"• {item.ItemName} - Qty: {item.Quantity:N2}, Rate: {item.Rate:N2}, Amount: {item.TotalAmount:N2}\n";
                    }
                }

                MessageBox.Show(billDetails, "Bill Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DeleteSelectedBill()
        {
            var selectedBill = GetSelectedBill();
            if (selectedBill != null)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete Bill No: {selectedBill.BillNo}?\n\n" +
                    $"This action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        if (BillService.DeleteBill(selectedBill.BillID))
                        {
                            MessageBox.Show("Bill deleted successfully!", "Success", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadBills(); // Refresh the list
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete bill. Please try again.", "Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting bill: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        #region Event Handlers

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadBills();
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            var newBillForm = new SaleBillForm();
            if (newBillForm.ShowDialog() == DialogResult.OK)
            {
                LoadBills(); // Refresh the list
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            EditSelectedBill();
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            ViewSelectedBill();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DeleteSelectedBill();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dgvBills_SelectionChanged(object sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void dgvBills_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                EditSelectedBill();
            }
        }

        private void dgvBills_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    EditSelectedBill();
                    e.Handled = true;
                    break;
                case Keys.Delete:
                    DeleteSelectedBill();
                    e.Handled = true;
                    break;
                case Keys.F5:
                    LoadBills();
                    e.Handled = true;
                    break;
                case Keys.Space:
                    ViewSelectedBill();
                    e.Handled = true;
                    break;
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void DateFilter_Changed(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        #endregion
    }
} 