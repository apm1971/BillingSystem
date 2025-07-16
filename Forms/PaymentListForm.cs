using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class PaymentListForm : Form
    {
        private List<Payment> payments;
        private List<Payment> filteredPayments;

        public PaymentListForm()
        {
            InitializeComponent();
            SetupDataGrid();
            ConfigureControls(); // Add this line to initialize date filters
            LoadPayments();
            this.KeyPreview = true; // Enable form to receive key events first
            this.KeyDown += PaymentListForm_KeyDown;
        }
        
        // Add this method to configure date filters and search
        private void ConfigureControls()
        {
            // Set form properties
            this.Text = "Payment List";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Setup date filters - add DateTimePicker controls for FromDate and ToDate
            // You'll need to add these controls to the form's designer first
            dtpFromDate.Value = DateTime.Today.AddMonths(-1);
            dtpToDate.Value = DateTime.Today;

            // Setup search functionality
            txtSearch.TextChanged += txtSearch_TextChanged;
            dtpFromDate.ValueChanged += DateFilter_Changed;
            dtpToDate.ValueChanged += DateFilter_Changed;
        }
        
        private void PaymentListForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                // F5: Refresh
                LoadPayments();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.F2)
            {
                // Enter or F2: View details
                if (dgvPayments.SelectedRows.Count > 0)
                {
                    btnView_Click(sender, e);
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.Shift && e.KeyCode == Keys.Delete) // Use Shift+Delete instead of just Delete
            {
                // Shift+Delete: Delete selected payment
                if (dgvPayments.SelectedRows.Count > 0)
                {
                    btnDelete_Click(sender, e);
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.Control && e.KeyCode == Keys.N)
            {
                // Ctrl+N: New payment
                var paymentForm = new PaymentEntryForm();
                paymentForm.ShowDialog();
                if (paymentForm.DialogResult == DialogResult.OK)
                {
                    LoadPayments();
                }
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                // Escape: Close
                this.Close();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.F)
            {
                // Ctrl+F: Focus on search
                txtSearch.Focus();
                txtSearch.SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F1)
            {
                // F1: Help
                ShowPaymentListHelp();
                e.SuppressKeyPress = true;
            }
        }
        
        private void ShowPaymentListHelp()
        {
            MessageBox.Show(
                "Payment List Keyboard Shortcuts:\n\n" +
                "F5: Refresh List\n" +
                "Enter or F2: View Payment Details\n" +
                "Shift+Delete: Delete Selected Payment\n" + // Updated shortcut text
                "Ctrl+N: New Payment\n" +
                "Ctrl+F: Focus on Search\n" +
                "Escape: Close\n" +
                "F1: Show this help\n\n" +
                "Navigation:\n" +
                "Arrow Keys: Navigate in list\n" +
                "Page Up/Down: Scroll list\n" +
                "Home/End: Go to first/last item\n" +
                "Tab: Move between controls",
                "Payment List Help",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void SetupDataGrid()
        {
            dgvPayments.AutoGenerateColumns = false;
            dgvPayments.AllowUserToAddRows = false;
            dgvPayments.AllowUserToDeleteRows = false;
            dgvPayments.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPayments.MultiSelect = false;
            dgvPayments.ReadOnly = true;
            
            // Enable row header to show row numbers
            dgvPayments.RowHeadersVisible = true;
            dgvPayments.RowHeadersWidth = 40;
            
            dgvPayments.Columns.Clear();
            
            // Hidden PaymentID column
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PaymentID",
                DataPropertyName = "PaymentID",
                Visible = false
            });
            
            // Payment Date column
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PaymentDate",
                HeaderText = "Date",
                DataPropertyName = "PaymentDate",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy", NullValue = null }
            });
            
            // Payment Amount column
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PaymentAmount",
                HeaderText = "Amount",
                DataPropertyName = "PaymentAmount",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            
            // Adjusted Amount column
            var adjustedColumn = new DataGridViewTextBoxColumn
            {
                Name = "AdjustedAmount",
                HeaderText = "Adjusted Amt",
                DataPropertyName = "AdjustedAmount",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            };
            dgvPayments.Columns.Add(adjustedColumn);
            
            // Unallocated Amount column
            var unallocatedColumn = new DataGridViewTextBoxColumn
            {
                Name = "UnallocatedAmount",
                HeaderText = "Unallocated",
                DataPropertyName = "UnallocatedAmount",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            };
            dgvPayments.Columns.Add(unallocatedColumn);
            
            // Payment Method column
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PaymentMethod",
                HeaderText = "Method",
                DataPropertyName = "PaymentMethod",
                Width = 120
            });
            
            // Party Name column
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PrimaryPartyName",
                HeaderText = "Party Name",
                DataPropertyName = "PrimaryPartyName",
                Width = 180
            });
            
            // Broker Name column
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PrimaryBrokerName",
                HeaderText = "Broker Name",
                DataPropertyName = "PrimaryBrokerName",
                Width = 180
            });
            
            // Bill Count column
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BillCount",
                HeaderText = "Bills",
                DataPropertyName = "BillCount",
                Width = 60,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            
            // Notes column
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Notes",
                HeaderText = "Notes",
                DataPropertyName = "Notes",
                Width = 200
            });
            
            // Set up event handlers
            dgvPayments.CellDoubleClick += dgvPayments_CellDoubleClick;
            dgvPayments.SelectionChanged += DgvPayments_SelectionChanged;
            dgvPayments.CellFormatting += DgvPayments_CellFormatting;
        }
        
        // Add cell formatting handler
        private void DgvPayments_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Value == null)
                return;
            
            // Check if this is a summary row
            if (dgvPayments.Rows[e.RowIndex].DataBoundItem is PaymentDisplayData data && data.IsSummaryRow)
            {
                // Style the summary row
                e.CellStyle.Font = new System.Drawing.Font(dgvPayments.Font, System.Drawing.FontStyle.Bold);
                e.CellStyle.BackColor = System.Drawing.Color.LightGray;
                
                // Hide the date for summary row
                if (dgvPayments.Columns[e.ColumnIndex].Name == "PaymentDate")
                {
                    e.Value = null;
                    e.FormattingApplied = true;
                }
                
                return;
            }
                
            // Format the Adjusted Amount column
            if (dgvPayments.Columns[e.ColumnIndex].Name == "AdjustedAmount")
            {
                double value = Convert.ToDouble(e.Value);
                double paymentAmount = 0;
                
                // Get the payment amount from the same row
                object paymentAmountObj = dgvPayments.Rows[e.RowIndex].Cells["PaymentAmount"].Value;
                if (paymentAmountObj != null)
                {
                    paymentAmount = Convert.ToDouble(paymentAmountObj);
                }
                
                // Color code the adjusted amount
                if (value == paymentAmount)
                {
                    // Fully adjusted - green
                    e.CellStyle.ForeColor = System.Drawing.Color.Green;
                }
                else if (value > 0)
                {
                    // Partially adjusted - dark green
                    e.CellStyle.ForeColor = System.Drawing.Color.DarkGreen;
                }
            }
            
            // Format the Unallocated Amount column
            if (dgvPayments.Columns[e.ColumnIndex].Name == "UnallocatedAmount")
            {
                double value = Convert.ToDouble(e.Value);
                
                // Color code the unallocated amount
                if (value > 0)
                {
                    // Unallocated funds - red
                    e.CellStyle.ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    // Fully allocated - green
                    e.CellStyle.ForeColor = System.Drawing.Color.Green;
                }
            }
        }

        private void DgvPayments_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = dgvPayments.SelectedRows.Count > 0;
            btnView.Enabled = hasSelection;
            btnEdit.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;
        }

        private void LoadPayments()
        {
            try
            {
                payments = PaymentService.GetAllPayments();
                ApplyFilters(); // Use ApplyFilters instead of directly setting filteredPayments
                
                // Initialize button states
                DgvPayments_SelectionChanged(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading payments: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Replace FilterPayments with this method
        private void ApplyFilters()
        {
            try
            {
                // Apply date range filter first
                filteredPayments = payments.Where(p => 
                    p.PaymentDate.Date >= dtpFromDate.Value.Date && 
                    p.PaymentDate.Date <= dtpToDate.Value.Date).ToList();
                
                // Then apply search text filter
                string searchText = txtSearch.Text.ToLower().Trim();
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    filteredPayments = filteredPayments.Where(p =>
                        p.PaymentMethod.ToLower().Contains(searchText) ||
                        p.PrimaryPartyName.ToLower().Contains(searchText) ||
                        p.Reference.ToLower().Contains(searchText) ||
                        p.Notes.ToLower().Contains(searchText) ||
                        // Search in payment details
                        p.PaymentDetails.Any(pd => pd.BillNo.ToLower().Contains(searchText) || 
                                                pd.PartyName.ToLower().Contains(searchText)) ||
                        // Search by broker name in associated bills
                        SearchByBrokerName(p, searchText)
                    ).ToList();
                }
                
                RefreshGrid();
                
                // Labels are now updated in RefreshGrid
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying filters: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper method to search payments by broker name
        private bool SearchByBrokerName(Payment payment, string searchText)
        {
            try
            {
                // Check if any associated bill has a broker name that matches the search text
                foreach (var detail in payment.PaymentDetails)
                {
                    if (detail.BillID <= 0)
                        continue;
                        
                    var bill = BillService.GetBillByID(detail.BillID);
                    if (bill != null && !string.IsNullOrEmpty(bill.BrokerName) && 
                        bill.BrokerName.ToLower().Contains(searchText))
                    {
                        return true;
                    }
                }
                
                return false;
            }
            catch
            {
                // In case of any error, return false to avoid filtering issues
                return false;
            }
        }

        private void RefreshGrid()
        {
            try
            {
                // Create a list of display data
                var displayList = new List<PaymentDisplayData>();
                
                // Add regular payment rows
                foreach (var p in filteredPayments)
                {
                    // Extract the primary broker name from payment details
                    string primaryBrokerName = "";
                    if (p.PaymentDetails.Any() && p.PaymentDetails.FirstOrDefault()?.BillID > 0)
                    {
                        // Try to get broker info from the first bill
                        var bill = BillService.GetBillByID(p.PaymentDetails.First().BillID);
                        if (bill != null && !string.IsNullOrEmpty(bill.BrokerName))
                        {
                            primaryBrokerName = bill.BrokerName;
                        }
                    }
                    
                    // Use the calculated properties from the Payment model
                    displayList.Add(new PaymentDisplayData
                    {
                        PaymentID = p.PaymentID,
                        PaymentDate = p.PaymentDate,
                        PaymentAmount = p.PaymentAmount,
                        AdjustedAmount = p.AdjustedAmount,
                        UnallocatedAmount = p.UnallocatedAmount,
                        PaymentMethod = p.PaymentMethod,
                        PrimaryPartyName = p.PrimaryPartyName,
                        PrimaryBrokerName = primaryBrokerName,
                        BillCount = p.PaymentDetails.Count,
                        Notes = p.Notes,
                        IsSummaryRow = false
                    });
                }

                // Reset the flag
                isSummaryRowAdded = false;
                
                // Calculate totals
                double totalAmount = filteredPayments.Sum(p => p.PaymentAmount);
                double totalAdjusted = filteredPayments.Sum(p => p.AdjustedAmount);
                double totalUnallocated = filteredPayments.Sum(p => p.UnallocatedAmount);
                
                // Add the summary row if there are payments
                if (filteredPayments.Count > 0)
                {
                    // Add a summary row to the display data
                    displayList.Add(new PaymentDisplayData
                    {
                        PaymentID = -1, // Use -1 to identify the summary row
                        PaymentDate = null, // Set to null for summary row
                        PaymentAmount = totalAmount,
                        AdjustedAmount = totalAdjusted,
                        UnallocatedAmount = totalUnallocated,
                        PaymentMethod = "TOTAL",
                        PrimaryPartyName = $"{filteredPayments.Count} payments",
                        PrimaryBrokerName = "",
                        BillCount = 0,
                        Notes = "",
                        IsSummaryRow = true
                    });
                    
                    isSummaryRowAdded = true;
                }
                
                // Create a binding list from the display list
                var bindingList = new BindingList<PaymentDisplayData>(displayList);
                
                // Clear the data source first
                dgvPayments.DataSource = null;
                
                // Set the new data source
                dgvPayments.DataSource = bindingList;
                
                // Update summary labels
                lblTotalPayments.Text = $"Total Payments: {filteredPayments.Count}";
                lblTotalAmount.Text = $"Total Amount: ₹{totalAmount:N2}";
                
                // Add or update additional labels for adjusted and unallocated amounts
                if (lblTotalAdjusted == null)
                {
                    lblTotalAdjusted = new Label
                    {
                        AutoSize = true,
                        Location = new System.Drawing.Point(lblTotalAmount.Right + 20, lblTotalAmount.Top),
                        Name = "lblTotalAdjusted",
                        ForeColor = System.Drawing.Color.DarkGreen
                    };
                    this.Controls.Add(lblTotalAdjusted);
                }
                
                if (lblTotalUnallocated == null)
                {
                    lblTotalUnallocated = new Label
                    {
                        AutoSize = true,
                        Location = new System.Drawing.Point(lblTotalAdjusted.Right + 20, lblTotalAmount.Top),
                        Name = "lblTotalUnallocated",
                        ForeColor = System.Drawing.Color.Red
                    };
                    this.Controls.Add(lblTotalUnallocated);
                }
                
                lblTotalAdjusted.Text = $"Adjusted: ₹{totalAdjusted:N2}";
                lblTotalUnallocated.Text = $"Unallocated: ₹{totalUnallocated:N2}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing grid: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        // Add a class for the payment display data
        private class PaymentDisplayData
        {
            public int PaymentID { get; set; }
            public DateTime? PaymentDate { get; set; }
            public double PaymentAmount { get; set; }
            public double AdjustedAmount { get; set; }
            public double UnallocatedAmount { get; set; }
            public string PaymentMethod { get; set; } = string.Empty;
            public string PrimaryPartyName { get; set; } = string.Empty;
            public string PrimaryBrokerName { get; set; } = string.Empty;
            public int BillCount { get; set; }
            public string Notes { get; set; } = string.Empty;
            public bool IsSummaryRow { get; set; } = false;
        }
        
        // Add private fields for the new labels
        private Label lblTotalAdjusted;
        private Label lblTotalUnallocated;

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        // Add this method for date filter changes
        private void DateFilter_Changed(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void dgvPayments_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                ViewPaymentDetails();
            }
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            ViewPaymentDetails();
        }
        
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvPayments.SelectedRows.Count > 0)
            {
                int paymentId = Convert.ToInt32(dgvPayments.SelectedRows[0].Cells["PaymentID"].Value);
                
                // Skip if this is the summary row
                if (paymentId == -1)
                    return;
                    
                EditPayment(paymentId);
            }
        }
        
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvPayments.SelectedRows.Count > 0)
            {
                int paymentId = Convert.ToInt32(dgvPayments.SelectedRows[0].Cells["PaymentID"].Value);
                
                // Skip if this is the summary row
                if (paymentId == -1)
                    return;
                    
                DeletePayment(paymentId);
            }
        }

        private void DeletePayment(int paymentId)
        {
            var payment = payments.FirstOrDefault(p => p.PaymentID == paymentId);
            if (payment == null) return;
            
            // Confirm deletion
            var result = MessageBox.Show(
                $"Are you sure you want to delete Payment #{payment.PaymentID} for ₹{payment.PaymentAmount:N2} dated {payment.PaymentDate:dd/MM/yyyy}?\n\n" +
                "This will revert the payment status of all associated bills.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
                
            if (result == DialogResult.Yes)
            {
                try
                {
                    // Delete the payment
                    if (PaymentService.DeletePayment(paymentId))
                    {
                        MessageBox.Show("Payment deleted successfully.", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadPayments();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete payment.", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting payment: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ViewPaymentDetails()
        {
            if (dgvPayments.SelectedRows.Count > 0)
            {
                int paymentId = Convert.ToInt32(dgvPayments.SelectedRows[0].Cells["PaymentID"].Value);
                
                // Skip if this is the summary row
                if (paymentId == -1)
                    return;
                    
                Payment payment = payments.First(p => p.PaymentID == paymentId);
                
                ShowPaymentDetails(payment);
            }
        }

        private void EditPayment(int paymentId)
        {
            try
            {
                // Open the PaymentEntryForm in edit mode
                using (var editForm = new PaymentEntryForm(paymentId))
                {
                    var result = editForm.ShowDialog(this);
                    
                    // If the payment was saved, refresh the list
                    if (result == DialogResult.OK)
                    {
                        LoadPayments();
                        MessageBox.Show("Payment updated successfully.", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening payment for editing: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowPaymentDetails(Payment payment)
        {
            // Get broker information if available
            string brokerInfo = "";
            if (payment.PaymentDetails.Any() && payment.PaymentDetails.FirstOrDefault()?.BillID > 0)
            {
                var bill = BillService.GetBillByID(payment.PaymentDetails.First().BillID);
                if (bill != null && !string.IsNullOrEmpty(bill.BrokerName))
                {
                    brokerInfo = bill.BrokerName;
                }
            }

            // Use the calculated properties from the Payment model
            string details = $"Payment Details\n";
            details += $"==================\n";
            details += $"Payment ID: {payment.PaymentID}\n";
            details += $"Date: {payment.PaymentDate:dd/MM/yyyy}\n";
            details += $"Amount: ₹{payment.PaymentAmount:N2}\n";
            details += $"Adjusted Amount: ₹{payment.AdjustedAmount:N2}\n";
            details += $"Unallocated Amount: ₹{payment.UnallocatedAmount:N2}\n";
            details += $"Method: {payment.PaymentMethod}\n";
            details += $"Party: {payment.PrimaryPartyName}\n";
            
            if (!string.IsNullOrEmpty(brokerInfo))
            {
                details += $"Broker: {brokerInfo}\n";
            }
            
            if (!string.IsNullOrEmpty(payment.Reference))
            {
                details += $"Reference: {payment.Reference}\n";
            }
            
            if (!string.IsNullOrEmpty(payment.Notes))
            {
                details += $"Notes: {payment.Notes}\n";
            }
            
            details += $"\nBill Allocations: {payment.PaymentDetails.Count}\n";
            details += $"=================\n";
            
            foreach (var detail in payment.PaymentDetails)
            {
                details += $"Bill: {detail.BillNo} - {detail.PartyName}\n";
                details += $"  Bill Amount: ₹{detail.BillAmount:N2}\n";
                details += $"  Previous Paid: ₹{detail.PreviousPaid:N2}\n";
                details += $"  Balance Before: ₹{detail.BalanceBefore:N2}\n";
                details += $"  Allocated: ₹{detail.AllocatedAmount:N2}\n";
                details += $"  Balance After: ₹{detail.BalanceAfter:N2}\n\n";
            }
            
            MessageBox.Show(details, "Payment Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadPayments();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Flag to track if summary row has been added
        private bool isSummaryRowAdded = false;
    }
} 