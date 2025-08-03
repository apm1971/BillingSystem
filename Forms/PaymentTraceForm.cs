using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SaleBillSystem.NET.Models;

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

            var btnClose = new Button
            {
                Text = "Close",
                Location = new Point(780, 510),
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK
            };

            // Add controls to form
            this.Controls.AddRange(new Control[] { lblTitle, lblPaymentInfo, dgvTrace, btnClose });

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
    }
} 