using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SaleBillSystem.NET.Models;
using SaleBillSystem.NET.Services;
using SaleBillSystem.NET.Data;

namespace SaleBillSystem.NET.Forms
{
    public partial class PaymentReportsControl : UserControl
    {
        public event EventHandler? CloseRequested;

        public PaymentReportsControl()
        {
            InitializeComponent();
            LoadFilterData();
            SetDefaultFilters();
        }

        private void LoadFilterData()
        {
            try
            {
                // Load parties directly from services for auto-search functionality (same as BillLedgerControl)
                var parties = PartyService.GetAllParties();
                cmbParty.DataSource = parties;
                cmbParty.DisplayMember = "PartyName";
                cmbParty.ValueMember = "PartyID";
                cmbParty.SelectedIndex = -1; // No selection initially for auto-search

                // Load brokers directly from services for auto-search functionality (same as BillLedgerControl)
                var brokers = BrokerService.GetAllBrokers();
                cmbBroker.DataSource = brokers;
                cmbBroker.DisplayMember = "BrokerName";
                cmbBroker.ValueMember = "BrokerID";
                cmbBroker.SelectedIndex = -1; // No selection initially for auto-search
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
            cmbParty.SelectedIndex = -1; // Clear party selection for auto-search
            cmbBroker.SelectedIndex = -1; // Clear broker selection for auto-search
            LoadReports();
        }

        private void LoadReports()
        {
            try
            {
                var fromDate = dtpFromDate.Value.Date;
                var toDate = dtpToDate.Value.Date; // Use same date logic as other forms

                int? partyId = null;
                int? brokerId = null;

                // Get selected party ID from the bound data
                if (cmbParty.SelectedValue is int selectedPartyId && selectedPartyId > 0)
                {
                    partyId = selectedPartyId;
                }

                // Get selected broker ID from the bound data
                if (cmbBroker.SelectedValue is int selectedBrokerId && selectedBrokerId > 0)
                {
                    brokerId = selectedBrokerId;
                }

                var reports = PaymentReportService.GetPaymentReports(
                    fromDate, toDate, partyId, brokerId);

                dgvReports.DataSource = reports;

                // Update button state
                btnViewReport.Enabled = reports.Count > 0;
                btnDeleteSettlement.Enabled = reports.Count > 0;
                
                // Debug: Show filter results
                System.Diagnostics.Debug.WriteLine($"Filter results: {reports.Count} reports found");
                System.Diagnostics.Debug.WriteLine($"Filter params: PartyID={partyId}, BrokerID={brokerId}, FromDate={fromDate:yyyy-MM-dd}, ToDate={toDate:yyyy-MM-dd}");
                if (reports.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"First report: Party='{reports[0].PartyName}', Broker='{reports[0].BrokerName}', Date={reports[0].PaymentDate:yyyy-MM-dd}");
                }
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
                var reportId = (int)selectedRow.Cells["ReportID"].Value;

                var reportData = PaymentReportService.GetPaymentReport(reportId);
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

        private void BtnClose_Click(object sender, EventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void PaymentReportsControl_Load(object sender, EventArgs e)
        {
            LoadReports();
        }

        private void cmbBroker_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadReports(); // Auto-refresh when broker selection changes
        }

        private void cmbParty_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadReports(); // Auto-refresh when party selection changes
        }

        private void dtpFromDate_ValueChanged(object sender, EventArgs e)
        {
            LoadReports(); // Auto-refresh when from date changes
        }

        private void dtpToDate_ValueChanged(object sender, EventArgs e)
        {
            LoadReports(); // Auto-refresh when to date changes
        }

        private void BtnDeleteSettlement_Click(object sender, EventArgs e)
        {
            if (dgvReports.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a settlement to delete.", "Selection Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Check permissions before allowing delete
            if (!PermissionManager.ValidateDeleteOperation(ModuleType.Reports, "settlement"))
                return;

            try
            {
                var selectedRow = dgvReports.SelectedRows[0];
                var reportId = (int)selectedRow.Cells["ReportID"].Value;
                var partyName = selectedRow.Cells["PartyName"].Value?.ToString() ?? "Unknown";
                var brokerName = selectedRow.Cells["BrokerName"].Value?.ToString() ?? "Unknown";
                var paymentDate = (DateTime)selectedRow.Cells["PaymentDate"].Value;
                var totalAmount = (decimal)selectedRow.Cells["TotalAmount"].Value;

                var confirmResult = MessageBox.Show(
                    $"Are you sure you want to delete this settlement?\n\n" +
                    $"Party: {partyName}\n" +
                    $"Broker: {brokerName}\n" +
                    $"Date: {paymentDate:dd-MMM-yyyy}\n" +
                    $"Amount: ₹{totalAmount:N2}\n\n" +
                    $"This will:\n" +
                    $"• Delete the cash payment\n" +
                    $"• Remove all advance utilizations\n" +
                    $"• Delete any reversal payments\n" +
                    $"• Remove the payment report\n\n" +
                    $"This action cannot be undone!",
                    "Confirm Delete Settlement",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (confirmResult == DialogResult.Yes)
                {
                    bool success = PaymentReportService.DeleteSettlement(reportId);
                    if (success)
                    {
                        // Refresh the reports list
                        LoadReports();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting settlement: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}