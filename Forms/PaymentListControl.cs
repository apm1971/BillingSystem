using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class PaymentListControl : UserControl
    {
        public event EventHandler? CloseRequested;
        private List<PaymentViewModel> _allPayments;

        public PaymentListControl()
        {
            InitializeComponent();
        }

        private void PaymentListControl_Load(object? sender, EventArgs e)
        {
            SetupDataGridView();
            LoadPayments();
            SetupEventHandlers();
        }

        #region Setup

        private void SetupDataGridView()
        {
            dgvPayments.AutoGenerateColumns = false;
            dgvPayments.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPayments.MultiSelect = false;
            dgvPayments.AllowUserToAddRows = false;
            dgvPayments.RowHeadersVisible = false;
            dgvPayments.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            dgvPayments.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvPayments.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvPayments.DefaultCellStyle.Font = new Font("Segoe UI", 9.75F);
            dgvPayments.RowTemplate.Height = 28;

            dgvPayments.Columns.Clear();
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PaymentID", HeaderText = "ID", Width = 80 });
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PaymentDate", HeaderText = "Payment Date", DefaultCellStyle = new DataGridViewCellStyle { Format = "dd-MMM-yyyy" }, Width = 120 });
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PartyName", HeaderText = "Party Name", Width = 200 });
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BrokerName", HeaderText = "Broker Name", Width = 150 });
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PaymentMethod", HeaderText = "Method", Width = 100 });
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Reference", HeaderText = "Reference", Width = 150 });
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalAmountPaid", HeaderText = "Amount Paid (₹)", DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        }

        private void SetupEventHandlers()
        {
            txtSearch.TextChanged += TxtSearch_TextChanged;
            btnNewPayment.Click += BtnNewPayment_Click;
            btnDelete.Click += BtnDelete_Click;
            btnViewTrace.Click += BtnViewTrace_Click;
            btnRefresh.Click += BtnRefresh_Click;
            dgvPayments.CellDoubleClick += DgvPayments_CellDoubleClick;
            btnApplyDateFilter.Click += BtnApplyDateFilter_Click;
            this.KeyDown += PaymentListControl_KeyDown;
            
            // Initialize date pickers
            dtpFromDate.Value = DateTime.Now.AddMonths(-1); // Default to 1 month back
            dtpToDate.Value = DateTime.Now;                // Default to today
        }

        #endregion

        #region Data Operations

        private void LoadPayments()
        {
            try
            {
                int companyId = 1; // Replace with Program.ActiveCompany.CompanyID
                _allPayments = PaymentService.GetAllPaymentsForDisplay(companyId);
                FilterAndBindPayments();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading payments: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void LoadPaymentsWithDateFilter()
        {
            try
            {
                int companyId = 1; // Replace with Program.ActiveCompany.CompanyID
                _allPayments = PaymentService.GetPaymentsInDateRange(companyId, dtpFromDate.Value.Date, dtpToDate.Value.Date);
                FilterAndBindPayments();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading payments with date filter: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterAndBindPayments()
        {
            string searchText = txtSearch.Text.Trim().ToLower();
            List<PaymentViewModel> filteredList;

            if (string.IsNullOrEmpty(searchText))
            {
                filteredList = _allPayments;
            }
            else
            {
                filteredList = _allPayments
                    .Where(p => p.PartyName.ToLower().Contains(searchText) ||
                                p.BrokerName.ToLower().Contains(searchText) ||
                                (p.Reference != null && p.Reference.ToLower().Contains(searchText)))
                    .ToList();
            }
            dgvPayments.DataSource = filteredList;
            
            // Update status label with count
            lblPaymentCount.Text = $"Total Payments: {filteredList.Count}";
        }

        #endregion

        #region Event Handlers

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            FilterAndBindPayments();
        }

        private void BtnNewPayment_Click(object? sender, EventArgs e)
        {
            var parentForm = this.FindForm() as MainForm;
            parentForm?.ShowControl(new PaymentEntryControl());
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvPayments.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a payment to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedPayment = dgvPayments.SelectedRows[0].DataBoundItem as PaymentViewModel;
            if (selectedPayment == null) return;

            // Check permissions before allowing delete
            if (!PermissionManager.ValidateDeleteOperation(ModuleType.Payments, "payment"))
                return;

            if (MessageBox.Show($"Are you sure you want to delete Payment ID {selectedPayment.PaymentID}?\nThis will also delete all associated ledger entries and cannot be undone.", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // Show loading cursor and disable controls to prevent double-clicking
                Cursor.Current = Cursors.WaitCursor;
                btnDelete.Enabled = false;
                dgvPayments.Enabled = false;
                
                // Use background worker to prevent UI hanging during deletion
                var backgroundWorker = new System.ComponentModel.BackgroundWorker();
                backgroundWorker.DoWork += (sender, e) =>
                {
                    try
                    {
                        bool success = PaymentService.DeletePayment(selectedPayment.PaymentID);
                        e.Result = new { Success = success, PaymentID = selectedPayment.PaymentID };
                    }
                    catch (Exception ex)
                    {
                        e.Result = new { Success = false, Error = ex.Message, PaymentID = selectedPayment.PaymentID };
                    }
                };

                backgroundWorker.RunWorkerCompleted += (sender, e) =>
                {
                    // Restore cursor and re-enable controls
                    Cursor.Current = Cursors.Default;
                    btnDelete.Enabled = true;
                    dgvPayments.Enabled = true;
                    
                    dynamic result = e.Result;
                    if (result.Success)
                    {
                        MessageBox.Show("Payment deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadPayments(); // Refresh the list
                    }
                    else if (result.Error != null)
                    {
                        MessageBox.Show($"Error deleting payment: {result.Error}", "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                backgroundWorker.RunWorkerAsync();
            }
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            LoadPayments();
        }

        private void BtnApplyDateFilter_Click(object? sender, EventArgs e)
        {
            // Validate date range
            if (dtpFromDate.Value > dtpToDate.Value)
            {
                MessageBox.Show("From Date cannot be later than To Date", "Invalid Date Range", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LoadPaymentsWithDateFilter();
        }

        private void DgvPayments_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (dgvPayments.SelectedRows.Count == 0) return;

            var selectedPayment = dgvPayments.SelectedRows[0].DataBoundItem as PaymentViewModel;
            if (selectedPayment == null) return;

            ShowPaymentTrace(selectedPayment);
        }

        private void ShowPaymentTrace(PaymentViewModel payment)
        {
            try
            {
                var paymentTrace = PaymentService.GetPaymentTrace(payment.PaymentID);
                
                if (!paymentTrace.Any())
                {
                    MessageBox.Show($"No transaction details found for Payment ID {payment.PaymentID}.", "No Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Create and show the payment trace form
                var traceForm = new PaymentTraceForm(payment, paymentTrace);
                traceForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading payment trace: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnViewTrace_Click(object? sender, EventArgs e)
        {
            if (dgvPayments.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a payment to view its trace.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedPayment = dgvPayments.SelectedRows[0].DataBoundItem as PaymentViewModel;
            if (selectedPayment == null) return;

            ShowPaymentTrace(selectedPayment);
        }

        private void PaymentListControl_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.F2)
            {
                BtnViewTrace_Click(sender, e);
            }
            else if (e.KeyCode == Keys.F5)
            {
                LoadPayments(); // Refresh with no date filters
            }
            else if (e.KeyCode == Keys.F6)
            {
                // Apply date filters
                BtnApplyDateFilter_Click(sender, e);
            }
        }

        #endregion
    }
}