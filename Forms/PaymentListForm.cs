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
            LoadPayments();
        }

        private void LoadPayments()
        {
            try
            {
                payments = PaymentService.GetAllPayments();
                filteredPayments = payments;
                RefreshGrid();
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

        private void ViewPaymentDetails()
        {
            if (dgvPayments.SelectedRows.Count > 0)
            {
                int paymentId = Convert.ToInt32(dgvPayments.SelectedRows[0].Cells["PaymentID"].Value);
                Payment payment = payments.First(p => p.PaymentID == paymentId);
                
                ShowPaymentDetails(payment);
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

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvPayments.SelectedRows.Count > 0)
            {
                int paymentId = Convert.ToInt32(dgvPayments.SelectedRows[0].Cells["PaymentID"].Value);
                Payment payment = payments.First(p => p.PaymentID == paymentId);
                
                if (MessageBox.Show(
                    $"Are you sure you want to delete payment #{paymentId}?\n\n" +
                    $"Payment Date: {payment.PaymentDate:dd/MM/yyyy}\n" +
                    $"Amount: ₹{payment.PaymentAmount:N2}\n" +
                    $"Method: {payment.PaymentMethod}\n\n" +
                    "This action cannot be undone!",
                    "Confirm Delete Payment",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try
                    {
                        if (PaymentService.DeletePayment(paymentId))
                        {
                            MessageBox.Show("Payment deleted successfully!", "Success", 
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
        }

        private void dgvPayments_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = dgvPayments.SelectedRows.Count > 0;
            btnView.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;
        }
    }
} 