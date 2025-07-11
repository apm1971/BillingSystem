using System;
using System.Collections.Generic;
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
            LoadPayments();
            this.KeyPreview = true; // Enable form to receive key events first
            this.KeyDown += PaymentListForm_KeyDown;
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
            else if (e.KeyCode == Keys.Delete)
            {
                // Delete: Delete selected payment
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
                "Delete: Delete Selected Payment\n" +
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
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
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
            
            // Payment Method column
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PaymentMethod",
                HeaderText = "Method",
                DataPropertyName = "PaymentMethod",
                Width = 120
            });
            
            // Reference column
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Reference",
                HeaderText = "Reference",
                DataPropertyName = "Reference",
                Width = 150
            });
            
            // Party column
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PartyInfo",
                HeaderText = "Party",
                DataPropertyName = "PartyInfo",
                Width = 200
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
                filteredPayments = payments;
                RefreshGrid();
                
                // Initialize button states
                DgvPayments_SelectionChanged(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading payments: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshGrid()
        {
            // Create display data
            var paymentData = filteredPayments.Select(p => new
            {
                PaymentID = p.PaymentID,
                PaymentDate = p.PaymentDate,
                PaymentAmount = p.PaymentAmount,
                PaymentMethod = p.PaymentMethod,
                Reference = p.Reference,
                PartyInfo = p.PartyInfo,
                BillCount = p.PaymentDetails.Count,
                Notes = p.Notes
            }).ToList();

            dgvPayments.DataSource = paymentData;
            
            lblTotalPayments.Text = $"Total Payments: {filteredPayments.Count}";
            lblTotalAmount.Text = $"Total Amount: ₹{filteredPayments.Sum(p => p.PaymentAmount):N2}";
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            FilterPayments();
        }

        private void FilterPayments()
        {
            string searchText = txtSearch.Text.Trim().ToLower();
            
            if (string.IsNullOrEmpty(searchText))
            {
                filteredPayments = payments;
            }
            else
            {
                filteredPayments = payments.Where(p =>
                    p.PaymentMethod.ToLower().Contains(searchText) ||
                    p.Reference.ToLower().Contains(searchText) ||
                    p.Notes.ToLower().Contains(searchText) ||
                    p.PrimaryPartyName.ToLower().Contains(searchText) ||
                    p.PaymentDetails.Any(pd => pd.BillNo.ToLower().Contains(searchText) || 
                                              pd.PartyName.ToLower().Contains(searchText))
                ).ToList();
            }
            
            RefreshGrid();
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
                EditPayment(paymentId);
            }
        }
        
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvPayments.SelectedRows.Count > 0)
            {
                int paymentId = Convert.ToInt32(dgvPayments.SelectedRows[0].Cells["PaymentID"].Value);
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
            string details = $"Payment Details\n";
            details += $"==================\n";
            details += $"Payment ID: {payment.PaymentID}\n";
            details += $"Date: {payment.PaymentDate:dd/MM/yyyy}\n";
            details += $"Amount: ₹{payment.PaymentAmount:N2}\n";
            details += $"Method: {payment.PaymentMethod}\n";
            details += $"Reference: {payment.Reference}\n";
            details += $"Notes: {payment.Notes}\n\n";
            
            details += $"Bill Allocations:\n";
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
    }
} 