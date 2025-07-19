using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Text;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class OutstandingReportForm : Form
    {
        private List<Bill> outstandingBills = new List<Bill>();
        private List<Bill> filteredBills = new List<Bill>();
        private List<Party> parties = new List<Party>();

        public OutstandingReportForm()
        {
            InitializeComponent();
            SetupDataGridView();
            SetupEventHandlers();
            LoadData();
            this.KeyPreview = true;
            this.KeyDown += OutstandingReportForm_KeyDown;
        }

        private void SetupEventHandlers()
        {
            // Setup search event handler
            txtSearch.TextChanged += txtSearch_TextChanged;
            cmbParty.SelectedIndexChanged += cmbParty_SelectedIndexChanged;
            
            // Setup grid event handlers
            dgvOutstanding.SelectionChanged += dgvOutstanding_SelectionChanged;
            dgvOutstanding.CellDoubleClick += dgvOutstanding_CellDoubleClick;
        }

        private void OutstandingReportForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                // F5: Refresh
                LoadData();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.E)
            {
                // Ctrl+E: Export to Excel
                ExportToExcel();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.P)
            {
                // Ctrl+P: Export to PDF
                ExportToPDF();
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
                ShowOutstandingReportHelp();
                e.SuppressKeyPress = true;
            }
        }

        private void ShowOutstandingReportHelp()
        {
            MessageBox.Show(
                "Outstanding Report Keyboard Shortcuts:\n\n" +
                "F5: Refresh Data\n" +
                "Ctrl+E: Export to Excel\n" +
                "Ctrl+P: Export to PDF\n" +
                "Ctrl+F: Focus on Search\n" +
                "Escape: Close Report\n" +
                "F1: Show this help\n\n" +
                "Navigation:\n" +
                "Tab: Move between controls\n" +
                "Enter: View bill details\n" +
                "Arrow Keys: Navigate in grid",
                "Outstanding Report Help",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void LoadData()
        {
            try
            {
                // Load parties for filter dropdown
                parties = PartyService.GetAllParties();
                SetupPartyComboBox();

                // Load outstanding bills
                outstandingBills = PaymentService.GetAllOutstandingBills();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupPartyComboBox()
        {
            var partyList = new List<Party> { new Party { PartyID = 0, PartyName = "-- All Parties --" } };
            partyList.AddRange(parties.OrderBy(p => p.PartyName));

            cmbParty.DataSource = partyList;
            cmbParty.DisplayMember = "PartyName";
            cmbParty.ValueMember = "PartyID";
            cmbParty.SelectedValue = 0;
        }

        private void SetupDataGridView()
        {
            // Set form properties
            this.Text = "Outstanding Report";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;

            dgvOutstanding.AutoGenerateColumns = false;
            dgvOutstanding.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOutstanding.MultiSelect = false;
            dgvOutstanding.ReadOnly = true;
            dgvOutstanding.AllowUserToAddRows = false;
            dgvOutstanding.AllowUserToDeleteRows = false;
            dgvOutstanding.RowHeadersVisible = true;
            dgvOutstanding.RowHeadersWidth = 40;
            dgvOutstanding.BackgroundColor = Color.White;
            dgvOutstanding.BorderStyle = BorderStyle.Fixed3D;
            dgvOutstanding.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvOutstanding.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
            dgvOutstanding.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvOutstanding.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(64, 64, 64);
            dgvOutstanding.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvOutstanding.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            dgvOutstanding.EnableHeadersVisualStyles = false;
            dgvOutstanding.GridColor = Color.LightGray;
            dgvOutstanding.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);

            // Clear existing columns
            dgvOutstanding.Columns.Clear();

            // Bill ID (Hidden)
            dgvOutstanding.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BillID",
                HeaderText = "Bill ID",
                DataPropertyName = "BillID",
                Visible = false
            });

            // Bill Number
            dgvOutstanding.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BillNo",
                HeaderText = "Bill No",
                DataPropertyName = "BillNo",
                Width = 100
            });

            // Bill Date
            dgvOutstanding.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BillDate",
                HeaderText = "Bill Date",
                DataPropertyName = "BillDate",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            });

            // Due Date
            dgvOutstanding.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DueDate",
                HeaderText = "Due Date",
                DataPropertyName = "DueDate",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            });

            // Party Name
            dgvOutstanding.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PartyName",
                HeaderText = "Party",
                DataPropertyName = "PartyName",
                Width = 180
            });

            // Broker Name
            dgvOutstanding.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BrokerName",
                HeaderText = "Broker",
                DataPropertyName = "BrokerName",
                Width = 120
            });

            // Net Amount
            dgvOutstanding.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "NetAmount",
                HeaderText = "Bill Amount",
                DataPropertyName = "NetAmount",
                Width = 110,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            // Paid Amount
            dgvOutstanding.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PaidAmount",
                HeaderText = "Paid Amount",
                DataPropertyName = "PaidAmount",
                Width = 110,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            // Balance Amount
            dgvOutstanding.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BalanceAmount",
                HeaderText = "Outstanding",
                DataPropertyName = "BalanceAmount",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold) }
            });

            // Days Overdue
            dgvOutstanding.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DaysOverdue",
                HeaderText = "Days Overdue",
                DataPropertyName = "DaysOverdue",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Status
            dgvOutstanding.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PaymentStatusText",
                HeaderText = "Status",
                DataPropertyName = "PaymentStatusText",
                Width = 100
            });

            // Add cell formatting event
            dgvOutstanding.CellFormatting += DgvOutstanding_CellFormatting;
        }

        private void DgvOutstanding_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = dgvOutstanding.Rows[e.RowIndex];
                if (row.DataBoundItem is OutstandingDisplayData data)
                {
                    if (data.IsSummaryRow)
                    {
                        // Format summary row
                        e.CellStyle.BackColor = Color.FromArgb(220, 220, 220);
                        e.CellStyle.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
                        e.CellStyle.ForeColor = Color.Black;
                    }
                    else
                    {
                        // Color code based on overdue status
                        if (data.DaysOverdue > 30)
                        {
                            e.CellStyle.BackColor = Color.FromArgb(255, 200, 200); // Light red for very overdue
                        }
                        else if (data.DaysOverdue > 0)
                        {
                            e.CellStyle.BackColor = Color.FromArgb(255, 240, 200); // Light orange for overdue
                        }
                        else if (data.DaysOverdue < 0 && data.DaysOverdue >= -7)
                        {
                            e.CellStyle.BackColor = Color.FromArgb(255, 255, 200); // Light yellow for due soon
                        }
                    }
                }
            }
        }

        private void ApplyFilters()
        {
            try
            {
                filteredBills = outstandingBills.ToList();

                // Apply party filter
                if (cmbParty.SelectedValue is int partyId && partyId > 0)
                {
                    filteredBills = filteredBills.Where(b => b.PartyID == partyId).ToList();
                }

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

                // Create display data
                var displayList = new List<OutstandingDisplayData>();

                // Add regular bill rows
                foreach (var b in filteredBills.OrderBy(x => x.PartyName).ThenBy(x => x.BillDate))
                {
                    displayList.Add(new OutstandingDisplayData
                    {
                        BillID = b.BillID,
                        BillNo = b.BillNo,
                        BillDate = b.BillDate,
                        DueDate = b.DueDate,
                        PartyName = b.PartyName,
                        BrokerName = string.IsNullOrEmpty(b.BrokerName) ? "No Broker" : b.BrokerName,
                        NetAmount = b.NetAmount,
                        PaidAmount = b.PaidAmount,
                        BalanceAmount = b.BalanceAmount,
                        DaysOverdue = b.IsOverdue ? b.DaysOverdue : (b.DaysUntilDue >= 0 ? -b.DaysUntilDue : b.DaysOverdue),
                        PaymentStatusText = b.PaymentStatusText,
                        IsSummaryRow = false
                    });
                }

                // Add summary row if there are bills
                if (filteredBills.Count > 0)
                {
                    double totalNetAmount = filteredBills.Sum(b => b.NetAmount);
                    double totalPaidAmount = filteredBills.Sum(b => b.PaidAmount);
                    double totalBalanceAmount = filteredBills.Sum(b => b.BalanceAmount);

                    displayList.Add(new OutstandingDisplayData
                    {
                        BillID = -1, // Use -1 to identify the summary row
                        BillNo = "TOTAL",
                        BillDate = null,
                        DueDate = null,
                        PartyName = $"{filteredBills.Count} bills",
                        BrokerName = "",
                        NetAmount = totalNetAmount,
                        PaidAmount = totalPaidAmount,
                        BalanceAmount = totalBalanceAmount,
                        DaysOverdue = 0,
                        PaymentStatusText = "",
                        IsSummaryRow = true
                    });
                }

                // Create a binding list and set data source
                var bindingList = new BindingList<OutstandingDisplayData>(displayList);
                dgvOutstanding.DataSource = null;
                dgvOutstanding.DataSource = bindingList;

                // Update summary labels
                lblTotalBills.Text = $"Total Bills: {filteredBills.Count}";
                lblTotalAmount.Text = $"Total Outstanding: ₹{filteredBills.Sum(b => b.BalanceAmount):N2}";

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
            bool hasSelection = dgvOutstanding.SelectedRows.Count > 0;
            
            // Don't enable certain buttons for the summary row
            if (hasSelection && dgvOutstanding.SelectedRows[0].DataBoundItem is OutstandingDisplayData data && data.IsSummaryRow)
            {
                btnViewBill.Enabled = false;
                return;
            }
            
            btnViewBill.Enabled = hasSelection;
        }

        private void ExportToExcel()
        {
            if (filteredBills.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    DefaultExt = "csv",
                    FileName = $"Outstanding_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    var csv = new StringBuilder();
                    
                    // Add header
                    csv.AppendLine($"Outstanding Report - {Program.ActiveCompany?.CompanyName}");
                    csv.AppendLine($"Generated on: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                    csv.AppendLine($"Total Bills: {filteredBills.Count}, Total Outstanding: ₹{filteredBills.Sum(b => b.BalanceAmount):N2}");
                    csv.AppendLine();
                    
                    // Add column headers
                    csv.AppendLine("Bill No,Bill Date,Due Date,Party Name,Broker,Bill Amount,Paid Amount,Outstanding,Days Overdue,Status");
                    
                    // Add data rows
                    foreach (var bill in filteredBills.OrderBy(x => x.PartyName).ThenBy(x => x.BillDate))
                    {
                        int daysOverdue = bill.IsOverdue ? bill.DaysOverdue : (bill.DaysUntilDue >= 0 ? -bill.DaysUntilDue : bill.DaysOverdue);
                        string brokerName = string.IsNullOrEmpty(bill.BrokerName) ? "No Broker" : bill.BrokerName;
                        
                        csv.AppendLine($"\"{bill.BillNo}\",\"{bill.BillDate:dd/MM/yyyy}\",\"{bill.DueDate:dd/MM/yyyy}\",\"{bill.PartyName}\",\"{brokerName}\",{bill.NetAmount:F2},{bill.PaidAmount:F2},{bill.BalanceAmount:F2},{daysOverdue},\"{bill.PaymentStatusText}\"");
                    }
                    
                    File.WriteAllText(saveDialog.FileName, csv.ToString(), Encoding.UTF8);
                    
                    MessageBox.Show($"Report exported successfully to:\n{saveDialog.FileName}", "Export Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                    // Ask if user wants to open the file
                    if (MessageBox.Show("Do you want to open the exported file?", "Open File", 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to Excel: {ex.Message}", "Export Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToPDF()
        {
            if (filteredBills.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "HTML files (*.html)|*.html|All files (*.*)|*.*",
                    DefaultExt = "html",
                    FileName = $"Outstanding_Report_{DateTime.Now:yyyyMMdd_HHmmss}.html"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    var html = new StringBuilder();
                    
                    // HTML structure with styling
                    html.AppendLine("<!DOCTYPE html>");
                    html.AppendLine("<html><head><meta charset='UTF-8'>");
                    html.AppendLine("<title>Outstanding Report</title>");
                    html.AppendLine("<style>");
                    html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
                    html.AppendLine("h1 { color: #333; text-align: center; }");
                    html.AppendLine("h2 { color: #666; text-align: center; margin-bottom: 20px; }");
                    html.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
                    html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
                    html.AppendLine("th { background-color: #f2f2f2; font-weight: bold; }");
                    html.AppendLine("tr:nth-child(even) { background-color: #f9f9f9; }");
                    html.AppendLine(".number { text-align: right; }");
                    html.AppendLine(".overdue-high { background-color: #ffcccc; }");
                    html.AppendLine(".overdue-medium { background-color: #fff0cc; }");
                    html.AppendLine(".due-soon { background-color: #ffffcc; }");
                    html.AppendLine(".summary { background-color: #e6e6e6; font-weight: bold; }");
                    html.AppendLine("</style>");
                    html.AppendLine("</head><body>");
                    
                    // Header
                    html.AppendLine($"<h1>{Program.ActiveCompany?.CompanyName}</h1>");
                    html.AppendLine("<h1>Outstanding Report</h1>");
                    html.AppendLine($"<h2>Generated on: {DateTime.Now:dd/MM/yyyy HH:mm:ss}</h2>");
                    
                    // Summary
                    html.AppendLine($"<p><strong>Total Bills:</strong> {filteredBills.Count} | ");
                    html.AppendLine($"<strong>Total Outstanding:</strong> ₹{filteredBills.Sum(b => b.BalanceAmount):N2}</p>");
                    
                    // Table
                    html.AppendLine("<table>");
                    html.AppendLine("<tr>");
                    html.AppendLine("<th>Bill No</th><th>Bill Date</th><th>Due Date</th><th>Party Name</th>");
                    html.AppendLine("<th>Broker</th><th>Bill Amount</th><th>Paid Amount</th><th>Outstanding</th>");
                    html.AppendLine("<th>Days Overdue</th><th>Status</th>");
                    html.AppendLine("</tr>");
                    
                    foreach (var bill in filteredBills.OrderBy(x => x.PartyName).ThenBy(x => x.BillDate))
                    {
                        int daysOverdue = bill.IsOverdue ? bill.DaysOverdue : (bill.DaysUntilDue >= 0 ? -bill.DaysUntilDue : bill.DaysOverdue);
                        string brokerName = string.IsNullOrEmpty(bill.BrokerName) ? "No Broker" : bill.BrokerName;
                        
                        string rowClass = "";
                        if (daysOverdue > 30) rowClass = "overdue-high";
                        else if (daysOverdue > 0) rowClass = "overdue-medium";
                        else if (daysOverdue < 0 && daysOverdue >= -7) rowClass = "due-soon";
                        
                        html.AppendLine($"<tr class='{rowClass}'>");
                        html.AppendLine($"<td>{bill.BillNo}</td>");
                        html.AppendLine($"<td>{bill.BillDate:dd/MM/yyyy}</td>");
                        html.AppendLine($"<td>{bill.DueDate:dd/MM/yyyy}</td>");
                        html.AppendLine($"<td>{bill.PartyName}</td>");
                        html.AppendLine($"<td>{brokerName}</td>");
                        html.AppendLine($"<td class='number'>₹{bill.NetAmount:N2}</td>");
                        html.AppendLine($"<td class='number'>₹{bill.PaidAmount:N2}</td>");
                        html.AppendLine($"<td class='number'>₹{bill.BalanceAmount:N2}</td>");
                        html.AppendLine($"<td class='number'>{daysOverdue}</td>");
                        html.AppendLine($"<td>{bill.PaymentStatusText}</td>");
                        html.AppendLine("</tr>");
                    }
                    
                    // Summary row
                    html.AppendLine("<tr class='summary'>");
                    html.AppendLine($"<td colspan='5'>TOTAL ({filteredBills.Count} bills)</td>");
                    html.AppendLine($"<td class='number'>₹{filteredBills.Sum(b => b.NetAmount):N2}</td>");
                    html.AppendLine($"<td class='number'>₹{filteredBills.Sum(b => b.PaidAmount):N2}</td>");
                    html.AppendLine($"<td class='number'>₹{filteredBills.Sum(b => b.BalanceAmount):N2}</td>");
                    html.AppendLine("<td colspan='2'></td>");
                    html.AppendLine("</tr>");
                    
                    html.AppendLine("</table>");
                    html.AppendLine("</body></html>");
                    
                    File.WriteAllText(saveDialog.FileName, html.ToString(), Encoding.UTF8);
                    
                    MessageBox.Show($"Report exported successfully to:\n{saveDialog.FileName}", "Export Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                    // Ask if user wants to open the file
                    if (MessageBox.Show("Do you want to open the exported file?", "Open File", 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to PDF: {ex.Message}", "Export Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ViewSelectedBill()
        {
            var selectedBill = GetSelectedBill();
            if (selectedBill != null)
            {
                string dueDateInfo = selectedBill.DaysUntilDue > 0 ? $" (Due in {selectedBill.DaysUntilDue} days)" : 
                                    selectedBill.IsOverdue ? $" (Overdue by {selectedBill.DaysOverdue} days)" : " (Due today)";
                
                string billDetails = $"Outstanding Bill Details:\n\n" +
                    $"Bill No: {selectedBill.BillNo}\n" +
                    $"Bill Date: {selectedBill.BillDate:dd/MM/yyyy}\n" +
                    $"Due Date: {selectedBill.DueDate:dd/MM/yyyy}{dueDateInfo}\n" +
                    $"Party: {selectedBill.PartyName}\n" +
                    $"Broker: {(string.IsNullOrEmpty(selectedBill.BrokerName) ? "No Broker" : selectedBill.BrokerName)}\n" +
                    $"Bill Amount: ₹{selectedBill.NetAmount:N2}\n" +
                    $"Paid Amount: ₹{selectedBill.PaidAmount:N2}\n" +
                    $"Outstanding: ₹{selectedBill.BalanceAmount:N2}\n" +
                    $"Status: {selectedBill.PaymentStatusText}";

                if (!string.IsNullOrEmpty(selectedBill.Notes))
                {
                    billDetails += $"\n\nNotes: {selectedBill.Notes}";
                }

                MessageBox.Show(billDetails, "Bill Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private Bill GetSelectedBill()
        {
            if (dgvOutstanding.SelectedRows.Count > 0)
            {
                var selectedData = dgvOutstanding.SelectedRows[0].DataBoundItem as OutstandingDisplayData;
                if (selectedData != null && !selectedData.IsSummaryRow)
                {
                    return filteredBills.FirstOrDefault(b => b.BillID == selectedData.BillID);
                }
            }
            return null;
        }

        // Event handlers
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void cmbParty_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void dgvOutstanding_SelectionChanged(object sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void dgvOutstanding_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Skip if this is the summary row
                if (dgvOutstanding.Rows[e.RowIndex].DataBoundItem is OutstandingDisplayData data && data.IsSummaryRow)
                    return;
                    
                ViewSelectedBill();
            }
        }

        private void btnViewBill_Click(object sender, EventArgs e)
        {
            ViewSelectedBill();
        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            ExportToExcel();
        }

        private void btnExportPDF_Click(object sender, EventArgs e)
        {
            ExportToPDF();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

    // Class to hold outstanding bill display data including summary row
    public class OutstandingDisplayData
    {
        public int BillID { get; set; }
        public string BillNo { get; set; } = string.Empty;
        public DateTime? BillDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public string BrokerName { get; set; } = string.Empty;
        public double NetAmount { get; set; }
        public double PaidAmount { get; set; }
        public double BalanceAmount { get; set; }
        public int DaysOverdue { get; set; }
        public string PaymentStatusText { get; set; } = string.Empty;
        public bool IsSummaryRow { get; set; } = false;
    }
} 