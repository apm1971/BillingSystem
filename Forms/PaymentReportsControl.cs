using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SaleBillSystem.NET.Models;
using SaleBillSystem.NET.Services;

namespace SaleBillSystem.NET.Forms
{
    public partial class PaymentReportsControl : UserControl
    {
        public event EventHandler? CloseRequested;

        private DateTimePicker dtpFromDate;
        private DateTimePicker dtpToDate;
        private ComboBox cmbParty;
        private ComboBox cmbBroker;
        private Button btnSearch;
        private Button btnClear;
        private DataGridView dgvReports;
        private Button btnViewReport;
        private Button btnClose;

        public PaymentReportsControl()
        {
            InitializeComponent();
            LoadFilterData();
            SetDefaultFilters();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1000, 600);
            this.BackColor = SystemColors.Control;

            // Title Label
            var lblTitle = new Label
            {
                Text = "Payment Reports",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true,
                ForeColor = Color.DarkBlue
            };

            // Filter Panel
            var filterPanel = new Panel
            {
                Location = new Point(10, 50),
                Size = new Size(980, 80),
                BackColor = SystemColors.Control,
                BorderStyle = BorderStyle.FixedSingle
            };

            // From Date
            var lblFromDate = new Label
            {
                Text = "From Date:",
                Location = new Point(10, 15),
                AutoSize = true
            };

            dtpFromDate = new DateTimePicker
            {
                Location = new Point(80, 12),
                Width = 120,
                Format = DateTimePickerFormat.Short
            };

            // To Date
            var lblToDate = new Label
            {
                Text = "To Date:",
                Location = new Point(220, 15),
                AutoSize = true
            };

            dtpToDate = new DateTimePicker
            {
                Location = new Point(280, 12),
                Width = 120,
                Format = DateTimePickerFormat.Short
            };

            // Party Filter
            var lblParty = new Label
            {
                Text = "Party:",
                Location = new Point(420, 15),
                AutoSize = true
            };

            cmbParty = new ComboBox
            {
                Location = new Point(470, 12),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Broker Filter
            var lblBroker = new Label
            {
                Text = "Broker:",
                Location = new Point(640, 15),
                AutoSize = true
            };

            cmbBroker = new ComboBox
            {
                Location = new Point(700, 12),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Search Button
            btnSearch = new Button
            {
                Text = "Search",
                Location = new Point(10, 45),
                Width = 80,
                Height = 25,
                UseVisualStyleBackColor = true
            };
            btnSearch.Click += BtnSearch_Click;

            // Clear Button
            btnClear = new Button
            {
                Text = "Clear Filters",
                Location = new Point(100, 45),
                Width = 100,
                Height = 25,
                UseVisualStyleBackColor = true
            };
            btnClear.Click += BtnClear_Click;

            // Add controls to filter panel
            filterPanel.Controls.AddRange(new Control[]
            {
                lblFromDate, dtpFromDate, lblToDate, dtpToDate,
                lblParty, cmbParty, lblBroker, cmbBroker,
                btnSearch, btnClear
            });

            // Data Grid
            dgvReports = new DataGridView
            {
                Location = new Point(10, 140),
                Size = new Size(980, 400),
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = SystemColors.Window,
                BorderStyle = BorderStyle.Fixed3D
            };

            // Add columns
            dgvReports.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn
                {
                    Name = "PaymentID",
                    HeaderText = "Payment ID",
                    DataPropertyName = "PaymentID",
                    Width = 100
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "PaymentDate",
                    HeaderText = "Date",
                    DataPropertyName = "PaymentDate",
                    Width = 100,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "dd-MMM-yyyy" }
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "PartyName",
                    HeaderText = "Party",
                    DataPropertyName = "PartyName",
                    Width = 200
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "BrokerName",
                    HeaderText = "Broker",
                    DataPropertyName = "BrokerName",
                    Width = 150
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "TotalAmount",
                    HeaderText = "Amount",
                    DataPropertyName = "TotalAmount",
                    Width = 120,
                    DefaultCellStyle = new DataGridViewCellStyle 
                    { 
                        Format = "C2",
                        Alignment = DataGridViewContentAlignment.MiddleRight
                    }
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "PaymentMethod",
                    HeaderText = "Method",
                    DataPropertyName = "PaymentMethod",
                    Width = 180
                }
            });

            dgvReports.DoubleClick += DgvReports_DoubleClick;

            // Bottom Panel for buttons
            btnViewReport = new Button
            {
                Text = "View Report",
                Location = new Point(10, 550),
                Width = 100,
                Height = 30,
                UseVisualStyleBackColor = true
            };
            btnViewReport.Click += BtnViewReport_Click;

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(120, 550),
                Width = 80,
                Height = 30,
                UseVisualStyleBackColor = true
            };
            btnClose.Click += (s, e) => CloseRequested?.Invoke(this, EventArgs.Empty);

            // Add controls to user control
            this.Controls.AddRange(new Control[] 
            { 
                lblTitle, filterPanel, dgvReports, btnViewReport, btnClose 
            });
        }

        private void LoadFilterData()
        {
            try
            {
                // Load parties
                var parties = PaymentReportService.GetDistinctParties();
                cmbParty.Items.Clear();
                cmbParty.Items.Add("All");
                cmbParty.Items.AddRange(parties.ToArray());
                cmbParty.SelectedIndex = 0;

                // Load brokers
                var brokers = PaymentReportService.GetDistinctBrokers();
                cmbBroker.Items.Clear();
                cmbBroker.Items.Add("All");
                cmbBroker.Items.AddRange(brokers.ToArray());
                cmbBroker.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading filter data: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetDefaultFilters()
        {
            // Set date range to current month
            dtpFromDate.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dtpToDate.Value = DateTime.Now;
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            LoadReports();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            SetDefaultFilters();
            cmbParty.SelectedIndex = 0;
            cmbBroker.SelectedIndex = 0;
            LoadReports();
        }

        private void LoadReports()
        {
            try
            {
                var fromDate = dtpFromDate.Value.Date;
                var toDate = dtpToDate.Value.Date.AddDays(1).AddTicks(-1); // End of day

                var partyName = cmbParty.SelectedItem?.ToString();
                var brokerName = cmbBroker.SelectedItem?.ToString();

                var reports = PaymentReportService.GetPaymentReports(
                    fromDate, toDate, partyName, brokerName);

                dgvReports.DataSource = reports;

                // Update button state
                btnViewReport.Enabled = reports.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading reports: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvReports_DoubleClick(object sender, EventArgs e)
        {
            ViewSelectedReport();
        }

        private void BtnViewReport_Click(object sender, EventArgs e)
        {
            ViewSelectedReport();
        }

        private void ViewSelectedReport()
        {
            if (dgvReports.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a report to view.", "Selection Required", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var selectedRow = dgvReports.SelectedRows[0];
                var paymentId = (int)selectedRow.Cells["PaymentID"].Value;

                var reportData = PaymentReportService.GetPaymentReport(paymentId);
                if (reportData == null)
                {
                    MessageBox.Show("Report data not found.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Show the report using the existing PaymentReportForm
                var reportForm = new PaymentReportForm(reportData);
                reportForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error viewing report: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadReports();
        }
    }
}