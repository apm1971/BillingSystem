using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SaleBillSystem.NET.Models;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Data.OleDb;
using SaleBillSystem.NET.Data;  // Add this namespace for DatabaseManager

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
                Text = $"Total Applied: ₹{Math.Round(totalApplied):N0} | Total Transactions: {_paymentTrace.Count}",
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
            // Check if all bills were paid - only auto-print the enhanced report if this is true
            bool areAllBillsPaid = CheckIfAllBillsPaid(_paymentTrace);
            
            if (areAllBillsPaid)
            {
                // Automatically trigger print when form opens
                BtnPrint_Click(this, EventArgs.Empty);
            }
            // Otherwise, let the user view the regular payment trace without auto-printing
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
                // Show loading cursor immediately
                Cursor.Current = Cursors.WaitCursor;
                
                // Use background worker to prevent UI hanging
                var backgroundWorker = new System.ComponentModel.BackgroundWorker();
                backgroundWorker.DoWork += (sender, e) =>
                {
                    try
                    {
                        bool areAllBillsPaid = CheckIfAllBillsPaid(_paymentTrace);
                        
                        if (!areAllBillsPaid)
                        {
                            e.Result = new { Success = false, Message = "Enhanced payment report is only available when all selected bills are fully paid.", IsPartialPayment = true };
                            return;
                        }
                        
                        string htmlContent = GenerateHtmlReport(_payment, _paymentTrace);
                        string tempFilePath = Path.Combine(Path.GetTempPath(), $"PaymentTrace_{_payment.PaymentID}.html");
                        File.WriteAllText(tempFilePath, htmlContent);

                        e.Result = new { Success = true, FilePath = tempFilePath };
                    }
                    catch (Exception ex)
                    {
                        e.Result = new { Success = false, Message = ex.Message, IsPartialPayment = false };
                    }
                };

                backgroundWorker.RunWorkerCompleted += (sender, e) =>
                {
                    Cursor.Current = Cursors.Default;
                    
                    dynamic result = e.Result;
                    if (result.Success)
                    {
                        // Open the file in the default web browser
                        Process.Start(new ProcessStartInfo(result.FilePath) { UseShellExecute = true });
                    }
                    else
                    {
                        if (result.IsPartialPayment)
                        {
                            MessageBox.Show(result.Message, "Partial Payment", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show($"Could not generate or open the report: {result.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                };

                backgroundWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show($"Could not generate or open the report: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GenerateHtmlReport(PaymentViewModel payment, List<PaymentTraceViewModel> paymentTrace)
        {
            var sb = new StringBuilder();
            bool areAllBillsPaid = CheckIfAllBillsPaid(paymentTrace);
            var billGroups = paymentTrace.Where(p => p.BillID != null).GroupBy(p => p.BillID);

            // Get previous payments for these bills
            var previousPayments = GetPreviousPayments(paymentTrace, payment.PaymentID);
            
            // Fetch party and broker details for each bill
            var billDetails = GetBillPartyBrokerDetails(billGroups.Select(g => g.Key.Value).ToList());
            
            // --- HTML and CSS Styling ---
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><title>Payment Receipt</title>");
            sb.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            sb.AppendLine("<style>");
            sb.AppendLine("@page { size: A6; margin: 5mm; }"); // A6 page size with small margins
            sb.AppendLine("body { font-family: 'Arial', sans-serif; margin: 0; padding: 5px; font-size: 8pt; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 5px; font-size: 7pt; }");
            sb.AppendLine("th, td { border: 0.5px solid #ccc; padding: 2px; text-align: left; }");
            sb.AppendLine("th { background-color: #f2f2f2; font-weight: bold; }");
            sb.AppendLine(".header { border-bottom: 1px solid #333; padding-bottom: 5px; margin-bottom: 5px; }");
            sb.AppendLine(".text-right { text-align: right; }");
            sb.AppendLine(".total-row { font-weight: bold; background-color: #f8f8f8; }");
            sb.AppendLine(".payment-info { background-color: #f0f8ff; padding: 5px; border-radius: 3px; margin: 5px 0; }");
            sb.AppendLine(".summary-box { background-color: #f5f5f5; padding: 5px; border-radius: 3px; margin: 5px 0; border: 0.5px solid #ddd; }");
            sb.AppendLine(".payment-tables { margin-top: 5px; }"); // Changed from flex to block for small screens
            sb.AppendLine(".payment-table { margin-bottom: 5px; }");
            sb.AppendLine("h1 { font-size: 10pt; margin: 3px 0; text-align: center; }");
            sb.AppendLine("h2 { font-size: 9pt; margin: 3px 0; }");
            sb.AppendLine("h3 { font-size: 8pt; margin: 3px 0; }");
            sb.AppendLine("h4 { font-size: 8pt; margin: 3px 0; }");
            sb.AppendLine("p { margin: 2px 0; }");
            sb.AppendLine(".positive-amount { color: green; font-weight: bold; }");
            sb.AppendLine(".negative-amount { color: red; font-weight: bold; }");
            sb.AppendLine(".section { margin-bottom: 5px; }");
            sb.AppendLine(".compact-info { display: flex; flex-wrap: wrap; }");
            sb.AppendLine(".compact-info div { flex: 1; min-width: 120px; margin-bottom: 2px; }");
            sb.AppendLine("@media print {");
            sb.AppendLine("  .page-break { page-break-before: always; }");
            sb.AppendLine("  body { width: 105mm; height: 148mm; }"); // A6 dimensions
            sb.AppendLine("}");
            sb.AppendLine("</style></head><body>");

            // --- Report Header ---
            sb.AppendLine($"<h1>Payment Receipt</h1>");
            sb.AppendLine($"<p>Generated on: {DateTime.Now:dd-MMM-yyyy HH:mm}</p>");
            
            // --- Payment Information ---
            sb.AppendLine("<div class='payment-info'>");
            sb.AppendLine("<div class='header'>");
            sb.AppendLine($"<h2>Payment Details</h2>");
            
            sb.AppendLine("<div class='compact-info'>");
            sb.AppendLine($"<div><strong>Payment ID:</strong> {payment.PaymentID}</div>");
            sb.AppendLine($"<div><strong>Date:</strong> {payment.PaymentDate:dd-MMM-yyyy}</div>");
            sb.AppendLine($"<div><strong>Method:</strong> {payment.PaymentMethod}</div>");
            sb.AppendLine($"<div><strong>Amount:</strong> ₹{Math.Round(payment.TotalAmountPaid):N0}</div>");
            
            if (!string.IsNullOrEmpty(payment.Reference))
            {
                sb.AppendLine($"<div><strong>Ref:</strong> {payment.Reference}</div>");
            }
            sb.AppendLine("</div>"); // End compact-info div
            
            // Add Cheque Firm amounts if payment method is Cheque
            if (payment.PaymentMethod == "Cheque")
            {
                sb.AppendLine("<div style='margin-top: 2px;'>");
                if (payment.ChequeAmountFirm1 > 0)
                {
                    sb.AppendLine($"<span style='margin-right: 10px;'><strong>Firm 1:</strong> ₹{Math.Round(payment.ChequeAmountFirm1):N0}</span>");
                }
                if (payment.ChequeAmountFirm2 > 0)
                {
                    sb.AppendLine($"<span><strong>Firm 2:</strong> ₹{Math.Round(payment.ChequeAmountFirm2):N0}</span>");
                }
                sb.AppendLine("</div>");
            }
            
            sb.AppendLine("</div>"); // End header div
            sb.AppendLine("</div>"); // End payment-info div

            // --- Bills Table ---
            sb.AppendLine("<div class='section'>");
            sb.AppendLine("<h3>Bill Details</h3>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Broker</th><th>Party</th><th>Date</th><th class='text-right'>Amount (₹)</th></tr>");
            
            decimal totalBillAmount = 0;
            decimal totalPreviousCash = 0;
            decimal totalPreviousCheque = 0;
            
            // Group transactions by bill and summarize
            foreach (var billGroup in billGroups)
            {
                var bill = billGroup.First(); // Get one transaction for this bill to display bill details
                if (!bill.BillID.HasValue) continue;
                
                totalBillAmount += bill.BillAmount;
                
                // Get previous payments by method for this bill
                Dictionary<string, List<PreviousPaymentInfo>> billPreviousPayments = new Dictionary<string, List<PreviousPaymentInfo>>();
                if (previousPayments.ContainsKey(bill.BillID.Value))
                {
                    billPreviousPayments = previousPayments[bill.BillID.Value];
                }
                
                // Calculate previous payment totals by method safely
                decimal previousCash = billPreviousPayments.ContainsKey("Cash") ? billPreviousPayments["Cash"].Sum(p => p.Amount) : 0;
                decimal previousCheque = billPreviousPayments.ContainsKey("Cheque") ? billPreviousPayments["Cheque"].Sum(p => p.Amount) : 0;
                
                totalPreviousCash += previousCash;
                totalPreviousCheque += previousCheque;
                
                // Get party and broker information for this bill
                string partyName = "Unknown";
                string brokerName = "";
                if (billDetails.ContainsKey(bill.BillID.Value))
                {
                    var detail = billDetails[bill.BillID.Value];
                    partyName = detail.PartyName;
                    brokerName = detail.BrokerName;
                }
                
                // Format date to be more compact
                string dateStr = bill.BillDate?.ToString("dd-MMM-yy") ?? "N/A";
                
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{brokerName}</td>");
                sb.AppendLine($"<td>{partyName}</td>");
                sb.AppendLine($"<td>{dateStr}</td>");
                sb.AppendLine($"<td class='text-right'>{Math.Round(bill.BillAmount):N0}</td>");
                sb.AppendLine("</tr>");
            }
            
            sb.AppendLine("<tr class='total-row'>");
            sb.AppendLine("<td colspan='3'><strong>Total</strong></td>");
            sb.AppendLine($"<td class='text-right'><strong>₹{Math.Round(totalBillAmount):N0}</strong></td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");

            // --- Previous Payments Tables ---
            sb.AppendLine("<div class='section'>");
            sb.AppendLine("<h3>Previous Payments</h3>");
            
            // --- Combined Previous Payments Table ---
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Broker</th><th>Method</th><th>Date</th><th class='text-right'>Amount (₹)</th></tr>");
            
            // First add cash payments
            foreach (var billGroup in billGroups)
            {
                var bill = billGroup.First();
                if (!bill.BillID.HasValue) continue;
                
                // Check for cash payments
                if (previousPayments.ContainsKey(bill.BillID.Value) && 
                    previousPayments[bill.BillID.Value].ContainsKey("Cash"))
                {
                    var cashPayments = previousPayments[bill.BillID.Value]["Cash"];
                    foreach (var cashPayment in cashPayments)
                    {
                        if (cashPayment.Amount > 0)
                        {
                            // Get broker information for this bill
                            string brokerName = "";
                            if (billDetails.ContainsKey(bill.BillID.Value))
                            {
                                var detail = billDetails[bill.BillID.Value];
                                brokerName = detail.BrokerName;
                            }
                            
                            sb.AppendLine("<tr>");
                            sb.AppendLine($"<td>{brokerName}</td>");
                            sb.AppendLine($"<td>Cash</td>");
                            sb.AppendLine($"<td>{cashPayment.PaymentDate:dd-MMM-yy}</td>");
                            sb.AppendLine($"<td class='text-right'>{Math.Round(cashPayment.Amount):N0}</td>");
                            sb.AppendLine("</tr>");
                        }
                    }
                }
            }
            
            // Then add cheque payments
            foreach (var billGroup in billGroups)
            {
                var bill = billGroup.First();
                if (!bill.BillID.HasValue) continue;
                
                // Check for cheque payments
                if (previousPayments.ContainsKey(bill.BillID.Value) && 
                    previousPayments[bill.BillID.Value].ContainsKey("Cheque"))
                {
                    var chequePayments = previousPayments[bill.BillID.Value]["Cheque"];
                    foreach (var chequePayment in chequePayments)
                    {
                        if (chequePayment.Amount > 0)
                        {
                            // Get firm amounts for this bill's previous payments
                            decimal firm1Amount = 0;
                            decimal firm2Amount = 0;
                            
                            try
                            {
                                // Try to get the firm amounts from the payment records
                                GetChequeAmountsByFirm(bill.BillID.Value, out firm1Amount, out firm2Amount);
                            }
                            catch (Exception ex)
                            {
                                // If there's an error, just log it and continue with zeros
                                System.Diagnostics.Debug.WriteLine($"Error getting firm amounts: {ex.Message}");
                            }
                            
                            string firmDetails = "";
                            if (firm1Amount > 0 || firm2Amount > 0)
                            {
                                firmDetails = $" (F1: ₹{Math.Round(firm1Amount):N0}, F2: ₹{Math.Round(firm2Amount):N0})";
                            }
                            
                            // Get broker information for this bill
                            string brokerName = "";
                            if (billDetails.ContainsKey(bill.BillID.Value))
                            {
                                var detail = billDetails[bill.BillID.Value];
                                brokerName = detail.BrokerName;
                            }
                            
                            sb.AppendLine("<tr>");
                            sb.AppendLine($"<td>{brokerName}</td>");
                            sb.AppendLine($"<td>Cheque{firmDetails}</td>");
                            sb.AppendLine($"<td>{chequePayment.PaymentDate:dd-MMM-yy}</td>");
                            sb.AppendLine($"<td class='text-right'>{Math.Round(chequePayment.Amount):N0}</td>");
                            sb.AppendLine("</tr>");
                        }
                    }
                }
            }
            
            // Get totals for firm amounts
            decimal totalFirm1 = 0;
            decimal totalFirm2 = 0;
            
            // Calculate totals for previous payments
            foreach (var billGroup in billGroups)
            {
                var bill = billGroup.First();
                if (!bill.BillID.HasValue) continue;
                
                // Only process bills that have cheque payments
                if (previousPayments.ContainsKey(bill.BillID.Value) && previousPayments[bill.BillID.Value].ContainsKey("Cheque") && previousPayments[bill.BillID.Value]["Cheque"].Any(p => p.Amount > 0))
                {
                    GetChequeAmountsByFirm(bill.BillID.Value, out decimal firm1, out decimal firm2);
                    totalFirm1 += firm1;
                    totalFirm2 += firm2;
                }
            }
            
            // Add totals row
            if (totalPreviousCash > 0 || totalPreviousCheque > 0)
            {
                sb.AppendLine("<tr class='total-row'>");
                sb.AppendLine("<td colspan='3'><strong>Total Previous Payments</strong></td>");
                sb.AppendLine($"<td class='text-right'><strong>₹{Math.Round(totalPreviousCash + totalPreviousCheque):N0}</strong></td>");
                sb.AppendLine("</tr>");
                
                // Add firm details if applicable
                if (totalFirm1 > 0 || totalFirm2 > 0)
                {
                    sb.AppendLine("<tr class='total-row'>");
                    sb.AppendLine("<td colspan='3'><em>Firm Breakdown</em></td>");
                    sb.AppendLine($"<td class='text-right'><em>F1: ₹{Math.Round(totalFirm1):N0}, F2: ₹{Math.Round(totalFirm2):N0}</em></td>");
                    sb.AppendLine("</tr>");
                }
            }
            
            sb.AppendLine("</table>");
            sb.AppendLine("</div>"); // End section div
            
            // Calculate totals
            decimal totalPayment = paymentTrace.Where(t => t.TransactionType == "Payment").Sum(t => t.CreditAmount);
            decimal totalInterest = paymentTrace.Where(t => t.TransactionType == "Interest").Sum(t => t.DebitAmount);
            decimal totalDiscount = paymentTrace.Where(t => t.TransactionType == "Discount").Sum(t => t.CreditAmount);
            decimal totalBrokerage = paymentTrace.Where(t => t.TransactionType == "Brokerage").Sum(t => t.CreditAmount);
            
            // Only show detailed calculations if all bills are paid with this payment
            bool showFinalSettlement = areAllBillsPaid;
            
            foreach (var entry in paymentTrace)
            {
                string amountClass = "";
                decimal amount = 0;
                
                if (entry.TransactionType == "Payment")
                {
                    amount = entry.CreditAmount;
                    amountClass = "positive-amount";
                }
                else if (entry.TransactionType == "Interest")
                {
                    amount = entry.DebitAmount;
                    amountClass = "negative-amount";
                }
                else if (entry.TransactionType == "Discount" || entry.TransactionType == "Brokerage")
                {
                    amount = entry.CreditAmount;
                    amountClass = "positive-amount";
                }
                
                // Get party and broker information for this transaction
                string partyName = "Unknown";
                string brokerName = "";
                if (entry.BillID.HasValue && billDetails.ContainsKey(entry.BillID.Value))
                {
                    var detail = billDetails[entry.BillID.Value];
                    partyName = detail.PartyName;
                    brokerName = detail.BrokerName;
                }
                
                // sb.AppendLine("<tr>");
                // sb.AppendLine($"<td>{brokerName}</td>");
                // sb.AppendLine($"<td>{partyName}</td>");
                // sb.AppendLine($"<td>{entry.TransactionType}</td>");
                // sb.AppendLine($"<td class='text-right {amountClass}'>{amount:N2}</td>");
                // sb.AppendLine($"<td>{entry.Description}</td>");
                // sb.AppendLine("</tr>");
            }
            
            // sb.AppendLine("<tr class='total-row'>");
            // sb.AppendLine("<td colspan='4'><strong>Totals</strong></td>");
            // sb.AppendLine($"<td class='text-right'><strong>Payments: ₹{totalPayment:N2}</strong></td>");
            // sb.AppendLine("<td></td>");
            // sb.AppendLine("</tr>");
            
            // sb.AppendLine("</table>");
            // sb.AppendLine("</div>");
            
            // Show Summary Calculation only if this is the final payment for all bills
            if (showFinalSettlement)
            {
                decimal totalPreviousPayments = Math.Round(totalPreviousCash + totalPreviousCheque);
                decimal netPayableAmount = Math.Round(totalBillAmount) + Math.Round(totalInterest) - Math.Round(totalDiscount) - Math.Round(totalBrokerage) - totalPreviousPayments;
                
                sb.AppendLine("<div class='summary-box'>");
                sb.AppendLine("<h3>Payment Summary</h3>");
                sb.AppendLine("<table>");
                sb.AppendLine("<tr><td>Bill Amount</td><td class='text-right'>₹" + Math.Round(totalBillAmount).ToString("N0") + "</td></tr>");
                
                // Combine previous payments into one line
                if (totalPreviousCash > 0 || totalPreviousCheque > 0)
                {
                    sb.AppendLine("<tr><td>Previous Payments (-)</td><td class='text-right'>₹" + Math.Round(totalPreviousPayments).ToString("N0") + "</td></tr>");
                }
                
                // Only show interest if greater than zero
                if (totalInterest > 0)
                {
                    sb.AppendLine("<tr><td>Interest (+)</td><td class='text-right'>₹" + Math.Round(totalInterest).ToString("N0") + "</td></tr>");
                }
                
                // Only show discount if greater than zero
                if (totalDiscount > 0)
                {
                    sb.AppendLine("<tr><td>Discount (-)</td><td class='text-right'>₹" + Math.Round(totalDiscount).ToString("N0") + "</td></tr>");
                }
                
                // Only show brokerage if greater than zero
                if (totalBrokerage > 0)
                {
                    sb.AppendLine("<tr><td>Brokerage (-)</td><td class='text-right'>₹" + Math.Round(totalBrokerage).ToString("N0") + "</td></tr>");
                }
                
                // Round the net payable amount
                decimal roundedNetPayableAmount = Math.Round(netPayableAmount);
                
                sb.AppendLine("<tr class='total-row'><td><strong>Net Amount</strong></td><td class='text-right'><strong>₹" + roundedNetPayableAmount.ToString("N0") + "</strong></td></tr>");
                sb.AppendLine("</table>");
                
                // Verify payment amounts match net payable - use a simpler indicator
                if (Math.Round(payment.TotalAmountPaid) == roundedNetPayableAmount)
                {
                    sb.AppendLine("<p style='color: green; font-size: 7pt; margin: 2px 0;'>✓ Payment matches net amount</p>");
                }
                else
                {
                    sb.AppendLine("<p style='color: red; font-size: 7pt; margin: 2px 0;'>⚠ Payment differs from net amount</p>");
                }
                sb.AppendLine("</div>");
            }
            
            // --- Footer ---
            sb.AppendLine("<div style='margin-top: 10px; text-align: center; color: #666; font-size: 7pt;'>");
            sb.AppendLine("<p>Thank you for your business!</p>");
            sb.AppendLine("<p style='font-size: 6pt;'>Generated by Sale Bill System</p>");
            sb.AppendLine("</div>");
            
            sb.AppendLine("</body></html>");

            return sb.ToString();
        }

        // Helper method to check if all bills in the payment trace are fully paid
        private bool CheckIfAllBillsPaid(List<PaymentTraceViewModel> paymentTrace)
        {
            // First, check if all bill statuses in the database are set to "Paid"
            if (AreBillStatusesPaid(paymentTrace))
            {
                return true;
            }

            // If not all bills show as paid in the database, do a calculation to double-check
            // Group transactions by bill
            var billGroups = paymentTrace.Where(p => p.BillID != null).GroupBy(p => p.BillID);
            var previousPayments = GetPreviousPayments(paymentTrace, _payment.PaymentID);
            
            foreach (var billGroup in billGroups)
            {
                // Get the bill details from the first transaction in the group
                var bill = billGroup.First();
                
                if (!bill.BillID.HasValue)
                    continue;
                    
                // Calculate total bill amount (bill original amount)
                decimal billAmount = bill.BillAmount;
                
                // Get previous payments for this bill safely
                decimal previousPayment = 0;
                if (previousPayments.ContainsKey(bill.BillID.Value))
                {
                    var methodDict = previousPayments[bill.BillID.Value];
                    // Add Cash payments if present
                    if (methodDict.ContainsKey("Cash")) 
                        previousPayment += methodDict["Cash"].Sum(p => p.Amount);
                    // Add Cheque payments if present
                    if (methodDict.ContainsKey("Cheque"))
                        previousPayment += methodDict["Cheque"].Sum(p => p.Amount);
                }
                
                // Calculate payments applied to this bill in this payment
                decimal currentPayment = billGroup.Where(t => t.TransactionType == "Payment").Sum(t => t.CreditAmount);
                
                // Calculate interest, discount, and brokerage for this bill
                decimal interest = billGroup.Where(t => t.TransactionType == "Interest").Sum(t => t.DebitAmount);
                decimal discount = billGroup.Where(t => t.TransactionType == "Discount").Sum(t => t.CreditAmount);
                decimal brokerage = billGroup.Where(t => t.TransactionType == "Brokerage").Sum(t => t.CreditAmount);
                
                // Calculate net payable for this bill (taking previous payments into account)
                decimal netPayable = billAmount + interest - discount - brokerage - previousPayment;
                
                // Check if total payments (previous + current) covers the net payable amount
                decimal totalPayments = previousPayment + currentPayment;
                if (Math.Round(totalPayments) != Math.Round(netPayable))
                {
                    return false;
                }
            }
            
            return true;
        }

        // Helper method to verify bill statuses directly from the database - SAFE
        private bool AreBillStatusesPaid(List<PaymentTraceViewModel> paymentTrace)
        {
            var billIds = paymentTrace
                .Where(p => p.BillID.HasValue)
                .Select(p => p.BillID.Value)
                .Distinct()
                .ToList();
            
            if (billIds.Count == 0)
                return false;
                
            try
            {
                int paidCount = 0;
                
                // Check each bill status individually to avoid SQL injection
                foreach (var billId in billIds)
                {
                    try
                    {
                        string sql = "SELECT Status FROM BillMaster WHERE BillID = ?";
                        var parameter = new OleDbParameter("BillID", billId);
                        
                        object result = DatabaseManager.ExecuteScalar(sql, parameter);
                        string status = result?.ToString() ?? "";
                        
                        if (status == "Paid")
                        {
                            paidCount++;
                        }
                    }
                    catch (Exception billEx)
                    {
                        // Handle individual bill errors gracefully
                        System.Diagnostics.Debug.WriteLine($"Error checking status for bill {billId}: {billEx.Message}");
                        // Assume not paid if we can't check
                    }
                }
                
                // Return true only if all bills are paid
                return paidCount == billIds.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking bill statuses: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Helper method to get previous payments for each bill - SAFE with individual queries
        private Dictionary<int, Dictionary<string, List<PreviousPaymentInfo>>> GetPreviousPayments(List<PaymentTraceViewModel> currentPaymentTrace, int currentPaymentId)
        {
            var result = new Dictionary<int, Dictionary<string, List<PreviousPaymentInfo>>>();
            
            // Get all bill IDs from the current payment trace
            var billIds = currentPaymentTrace
                .Where(p => p.BillID.HasValue)
                .Select(p => p.BillID.Value)
                .Distinct()
                .ToList();
            
            if (billIds.Count == 0)
                return result;
            
            try
            {
                // Process each bill individually to avoid SQL injection and handle errors gracefully
                foreach (var billId in billIds)
                {
                    // Initialize empty collections for this bill
                    result[billId] = new Dictionary<string, List<PreviousPaymentInfo>>
                    {
                        ["Cash"] = new List<PreviousPaymentInfo>(),
                        ["Cheque"] = new List<PreviousPaymentInfo>()
                    };
                    
                    try
                    {
                        string sql = @"
                            SELECT BillID, TransactionID, PaymentID, TransactionType, DebitAmount, CreditAmount, PaymentMethod, TransactionDate 
                            FROM TransactionLedger 
                            WHERE BillID = ? AND TransactionType = 'Payment' AND PaymentID <> ?
                            ORDER BY TransactionDate";
                        
                        var parameters = new OleDbParameter[]
                        {
                            new OleDbParameter("BillID", billId),
                            new OleDbParameter("PaymentID", currentPaymentId)
                        };
                        
                        DataTable dt = DatabaseManager.ExecuteQuery(sql, parameters);
                        
                        foreach (DataRow row in dt.Rows)
                        {
                            string paymentMethod = row["PaymentMethod"] != DBNull.Value ? row["PaymentMethod"].ToString() : "Cash";
                            decimal amount = Convert.ToDecimal(row["CreditAmount"]);
                            DateTime paymentDate = Convert.ToDateTime(row["TransactionDate"]);
                            
                            // Default to "Cash" for empty payment methods
                            if (string.IsNullOrWhiteSpace(paymentMethod))
                                paymentMethod = "Cash";
                            
                            var paymentInfo = new PreviousPaymentInfo
                            {
                                Amount = amount,
                                PaymentDate = paymentDate
                            };
                            
                            // Ensure the payment method exists in the dictionary
                            if (!result[billId].ContainsKey(paymentMethod))
                            {
                                result[billId][paymentMethod] = new List<PreviousPaymentInfo>();
                            }
                            
                            result[billId][paymentMethod].Add(paymentInfo);
                        }
                    }
                    catch (Exception billEx)
                    {
                        // Handle individual bill errors gracefully
                        System.Diagnostics.Debug.WriteLine($"Error fetching previous payments for bill {billId}: {billEx.Message}");
                        // Keep the empty collections for this bill
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching previous payments: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            return result;
        }
        
        // Helper class to store previous payment information
        private class PreviousPaymentInfo
        {
            public decimal Amount { get; set; }
            public DateTime PaymentDate { get; set; }
        }

        // Helper method to fetch party and broker details for each bill
        private class BillDetail
        {
            public string PartyName { get; set; } = "Unknown";
            public string BrokerName { get; set; } = "";
            public int PartyID { get; set; }
            public int? BrokerID { get; set; }
        }

        private Dictionary<int, BillDetail> GetBillPartyBrokerDetails(List<int> billIds)
        {
            var result = new Dictionary<int, BillDetail>();
            
            if (billIds == null || billIds.Count == 0)
                return result;
            
            try
            {
                // Process bills individually to avoid SQL injection and handle errors gracefully
                foreach (var billId in billIds.Distinct()) // Remove duplicates
                {
                    try
                    {
                        string sql = @"
                            SELECT b.BillID, b.PartyID, b.BrokerID, p.PartyName, bm.BrokerName
                            FROM (BillMaster b 
                            LEFT JOIN PartyMaster p ON b.PartyID = p.PartyID)
                            LEFT JOIN BrokerMaster bm ON b.BrokerID = bm.BrokerID
                            WHERE b.BillID = ?";
                        
                        var parameter = new OleDbParameter("BillID", billId);
                        DataTable dt = DatabaseManager.ExecuteQuery(sql, parameter);
                        
                        if (dt.Rows.Count > 0)
                        {
                            DataRow row = dt.Rows[0];
                            
                            var detail = new BillDetail
                            {
                                PartyID = Convert.ToInt32(row["PartyID"]),
                                PartyName = row["PartyName"] != DBNull.Value ? row["PartyName"].ToString() : "Unknown",
                                BrokerID = row["BrokerID"] != DBNull.Value ? Convert.ToInt32(row["BrokerID"]) : (int?)null,
                                BrokerName = row["BrokerName"] != DBNull.Value ? row["BrokerName"].ToString() : ""
                            };
                            
                            result[billId] = detail;
                        }
                        else
                        {
                            // Bill not found, create default entry
                            result[billId] = new BillDetail
                            {
                                PartyID = 0,
                                PartyName = "Unknown",
                                BrokerID = null,
                                BrokerName = ""
                            };
                        }
                    }
                    catch (Exception billEx)
                    {
                        // Handle individual bill errors gracefully
                        System.Diagnostics.Debug.WriteLine($"Error fetching details for bill {billId}: {billEx.Message}");
                        
                        // Create default entry for this bill
                        result[billId] = new BillDetail
                        {
                            PartyID = 0,
                            PartyName = "Unknown",
                            BrokerID = null,
                            BrokerName = ""
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching bill details: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            return result;
        }

        #endregion
        
        // Helper method to get cheque amounts by firm for a specific bill
        private void GetChequeAmountsByFirm(int billId, out decimal firm1Amount, out decimal firm2Amount)
        {
            firm1Amount = 0;
            firm2Amount = 0;
            
            try
            {
                string sql = @"
                    SELECT pm.ChequeAmountFirm1, pm.ChequeAmountFirm2
                    FROM PaymentMaster pm, TransactionLedger tl 
                    WHERE pm.PaymentID = tl.PaymentID 
                    AND tl.BillID = ? 
                    AND tl.TransactionType = 'Payment' 
                    AND pm.PaymentMethod = 'Cheque'";
                
                var parameter = new OleDbParameter("BillID", billId);
                
                DataTable dt = DatabaseManager.ExecuteQuery(sql, parameter);
                
                foreach (DataRow row in dt.Rows)
                {
                    if (row["ChequeAmountFirm1"] != DBNull.Value)
                    {
                        firm1Amount += Convert.ToDecimal(row["ChequeAmountFirm1"]);
                    }
                    
                    if (row["ChequeAmountFirm2"] != DBNull.Value)
                    {
                        firm2Amount += Convert.ToDecimal(row["ChequeAmountFirm2"]);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting cheque amounts by firm: {ex.Message}");
            }
        }
    }
} 