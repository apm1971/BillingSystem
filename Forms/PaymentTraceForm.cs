using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SaleBillSystem.NET.Models;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace SaleBillSystem.NET.Forms
{
    public partial class PaymentTraceForm : Form
    {
        private readonly PaymentViewModel _payment;
        private readonly List<PaymentTraceViewModel> _paymentTrace;

        public PaymentTraceForm(PaymentViewModel payment, List<PaymentTraceViewModel> paymentTrace)
        {
            _payment = payment;
            _paymentTrace = paymentTrace;
            InitializeComponent();
            SetupForm();
        }

        private void InitializeComponent()
        {
            this.Text = $"Payment Trace - ID: {_payment.PaymentID}";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Create controls
            var lblTitle = new Label
            {
                Text = $"Payment Trace for Payment ID: {_payment.PaymentID}",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            var lblPaymentInfo = new Label
            {
                Text = $"Date: {_payment.PaymentDate:dd-MMM-yyyy} | Party: {_payment.PartyName} | Amount: ₹{_payment.TotalAmountPaid:N2}",
                Font = new Font("Segoe UI", 10F),
                Location = new Point(20, 50),
                AutoSize = true
            };

            var dgvTrace = new DataGridView
            {
                Location = new Point(20, 90),
                Size = new Size(840, 400),
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(64, 64, 64),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                },
                EnableHeadersVisualStyles = false,
                GridColor = Color.LightGray,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(245, 245, 245)
                }
            };

            // Setup columns
            dgvTrace.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "TransactionID", HeaderText = "Txn ID", DataPropertyName = "TransactionID", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "BillNo", HeaderText = "Bill No", DataPropertyName = "BillNo", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "BillDate", HeaderText = "Bill Date", DataPropertyName = "BillDate", DefaultCellStyle = new DataGridViewCellStyle { Format = "dd-MMM-yyyy" }, Width = 100 },
                new DataGridViewTextBoxColumn { Name = "BillAmount", HeaderText = "Bill Amount", DataPropertyName = "BillAmount", DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }, Width = 120 },
                new DataGridViewTextBoxColumn { Name = "AppliedAmount", HeaderText = "Applied Amount", DataPropertyName = "AppliedAmount", DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }, Width = 120 },
                new DataGridViewTextBoxColumn { Name = "TransactionType", HeaderText = "Type", DataPropertyName = "TransactionType", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description", DataPropertyName = "Description", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
            });

            var btnPrint = new Button
            {
                Text = "Print",
                Location = new Point(680, 510),
                Size = new Size(80, 30),
                Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold)
            };
            btnPrint.Click += BtnPrint_Click;

            var btnClose = new Button
            {
                Text = "Close",
                Location = new Point(780, 510),
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK
            };

            // Add controls to form
            this.Controls.AddRange(new Control[] { lblTitle, lblPaymentInfo, dgvTrace, btnPrint, btnClose });

            // Bind data
            dgvTrace.DataSource = _paymentTrace;

            // Add summary information
            var totalApplied = _paymentTrace.Sum(t => t.AppliedAmount);
            var lblSummary = new Label
            {
                Text = $"Total Applied: ₹{totalApplied:N2} | Total Transactions: {_paymentTrace.Count}",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(20, 510),
                AutoSize = true
            };
            this.Controls.Add(lblSummary);

            // Add cell formatting for better visual presentation
            dgvTrace.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex == dgvTrace.Columns["AppliedAmount"].Index && e.Value != null)
                {
                    if (decimal.TryParse(e.Value.ToString(), out decimal amount))
                    {
                        if (amount > 0)
                        {
                            e.CellStyle.ForeColor = Color.Green;
                            e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                        }
                    }
                }
            };
        }

        private void SetupForm()
        {
            // Additional setup if needed
        }

        public void AutoPrint()
        {
            // Automatically trigger print when form opens
            BtnPrint_Click(this, EventArgs.Empty);
        }

        #region Printing and Exporting

        private void BtnPrint_Click(object? sender, EventArgs e)
        {
            if (!_paymentTrace.Any())
            {
                MessageBox.Show("No payment trace data to print.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string htmlContent = GenerateHtmlReport(_payment, _paymentTrace);
                string tempFilePath = Path.Combine(Path.GetTempPath(), $"PaymentTrace_{_payment.PaymentID}.html");
                File.WriteAllText(tempFilePath, htmlContent);

                // Open the file in the default web browser
                Process.Start(new ProcessStartInfo(tempFilePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not generate or open the report: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GenerateHtmlReport(PaymentViewModel payment, List<PaymentTraceViewModel> paymentTrace)
        {
            var sb = new StringBuilder();

            // --- HTML and CSS Styling ---
            sb.AppendLine("<html><head><title>Payment Trace Report</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: 'Segoe UI', sans-serif; margin: 20px; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
            sb.AppendLine("th, td { border: 1px solid #ccc; padding: 8px; text-align: left; }");
            sb.AppendLine("th { background-color: #f2f2f2; }");
            sb.AppendLine(".header { display: flex; justify-content: space-between; border-bottom: 2px solid #333; padding-bottom: 10px; }");
            sb.AppendLine(".header-left, .header-right { width: 48%; }");
            sb.AppendLine(".text-right { text-align: right; }");
            sb.AppendLine(".total-row { font-weight: bold; background-color: #f8f8f8; }");
            sb.AppendLine(".payment-info { background-color: #f0f8ff; padding: 15px; border-radius: 5px; margin: 20px 0; }");
            sb.AppendLine("h1, h2 { margin: 0; }");
            sb.AppendLine(".positive-amount { color: green; font-weight: bold; }");
            sb.AppendLine("</style></head><body>");

            // --- Report Header ---
            sb.AppendLine($"<h1>Payment Trace Report</h1>");
            sb.AppendLine($"<p>Generated on: {DateTime.Now:dd-MMM-yyyy HH:mm}</p>");
            
            // --- Payment Information ---
            sb.AppendLine("<div class='payment-info'>");
            sb.AppendLine($"<h2>Payment Details</h2>");
            sb.AppendLine($"<p><strong>Payment ID:</strong> {payment.PaymentID}</p>");
            sb.AppendLine($"<p><strong>Payment Date:</strong> {payment.PaymentDate:dd-MMM-yyyy}</p>");
            sb.AppendLine($"<p><strong>Party:</strong> {payment.PartyName}</p>");
            sb.AppendLine($"<p><strong>Total Amount Paid:</strong> ₹{payment.TotalAmountPaid:N2}</p>");
            sb.AppendLine($"<p><strong>Payment Method:</strong> {payment.PaymentMethod}</p>");
            if (!string.IsNullOrEmpty(payment.Reference))
            {
                sb.AppendLine($"<p><strong>Reference:</strong> {payment.Reference}</p>");
            }
            sb.AppendLine("</div>");

            // --- Trace Table ---
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Txn ID</th><th>Bill No</th><th>Bill Date</th><th class='text-right'>Bill Amount (₹)</th><th class='text-right'>Applied Amount (₹)</th><th>Type</th><th>Description</th></tr>");
            
            foreach (var entry in paymentTrace)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{entry.TransactionID}</td>");
                sb.AppendLine($"<td>{entry.BillNo}</td>");
                sb.AppendLine($"<td>{entry.BillDate:dd-MMM-yyyy}</td>");
                sb.AppendLine($"<td class='text-right'>{entry.BillAmount:N2}</td>");
                
                // Calculate applied amount based on transaction type
                decimal appliedAmount = 0;
                if (entry.TransactionType == "Payment")
                {
                    appliedAmount = entry.CreditAmount;
                }
                else if (entry.TransactionType == "Interest")
                {
                    appliedAmount = entry.DebitAmount;
                }
                else if (entry.TransactionType == "Discount")
                {
                    appliedAmount = -entry.CreditAmount; // Negative for discount
                }
                else if (entry.TransactionType == "Brokerage")
                {
                    appliedAmount = -entry.CreditAmount; // Negative for brokerage (similar to discount)
                }
                
                string amountClass = appliedAmount > 0 ? "positive-amount" : "";
                sb.AppendLine($"<td class='text-right {amountClass}'>{appliedAmount:N2}</td>");
                sb.AppendLine($"<td>{entry.TransactionType}</td>");
                sb.AppendLine($"<td>{entry.Description}</td>");
                sb.AppendLine("</tr>");
            }

            // --- Summary Row ---
            var totalApplied = paymentTrace.Where(t => t.TransactionType == "Payment").Sum(t => t.CreditAmount);
            var totalInterest = paymentTrace.Where(t => t.TransactionType == "Interest").Sum(t => t.DebitAmount);
            var totalDiscount = paymentTrace.Where(t => t.TransactionType == "Discount").Sum(t => t.CreditAmount);
            var totalBrokerage = paymentTrace.Where(t => t.TransactionType == "Brokerage").Sum(t => t.CreditAmount);

            sb.AppendLine("<tr class='total-row'>");
            sb.AppendLine("<td colspan='4'><strong>Summary</strong></td>");
            sb.AppendLine($"<td class='text-right'><strong>Payments: ₹{totalApplied:N2}</strong></td>");
            sb.AppendLine("<td colspan='2'><strong>Interest: ₹" + totalInterest.ToString("N2") + " | Discount: ₹" + totalDiscount.ToString("N2") + " | Brokerage: ₹" + totalBrokerage.ToString("N2") + "</strong></td>");
            sb.AppendLine("</tr>");

            sb.AppendLine("</table>");
            
            // --- Footer ---
            sb.AppendLine("<div style='margin-top: 30px; text-align: center; color: #666;'>");
            sb.AppendLine("<p>This report shows the complete trace of how payment ID " + payment.PaymentID + " was applied across different bills and transactions.</p>");
            sb.AppendLine("</div>");
            
            sb.AppendLine("</body></html>");

            return sb.ToString();
        }

        #endregion
    }
} 