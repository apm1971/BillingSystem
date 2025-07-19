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
using OfficeOpenXml; // Added for EPPlus
// Import iTextSharp with alias to avoid conflicts
using iTextSharp.text;
using iTextSharp.text.pdf;
using Font = System.Drawing.Font;
using Rectangle = System.Drawing.Rectangle;

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
            SetupStatusComboBox();
            LoadData();
            this.KeyPreview = true;
            this.KeyDown += OutstandingReportForm_KeyDown;
        }

        private void SetupStatusComboBox()
        {
            // Add payment status options
            cmbStatus.Items.Add("All");
            cmbStatus.Items.Add("Paid");
            cmbStatus.Items.Add("Partial");
            cmbStatus.Items.Add("Unpaid");
            cmbStatus.SelectedIndex = 0; // Default to "All"
        }

        private void SetupEventHandlers()
        {
            // Setup search event handler
            txtSearch.TextChanged += txtSearch_TextChanged;
            cmbParty.SelectedIndexChanged += cmbParty_SelectedIndexChanged;
            cmbStatus.SelectedIndexChanged += cmbStatus_SelectedIndexChanged;
            
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
                "Bills Report Keyboard Shortcuts:\n\n" +
                "F5: Refresh Data\n" +
                "Ctrl+E: Export to Excel (.xlsx)\n" +
                "Ctrl+P: Export to PDF (.pdf)\n" +
                "Ctrl+F: Focus on Search\n" +
                "Escape: Close Report\n" +
                "F1: Show this help\n\n" +
                "Filters:\n" +
                "- Party: Filter by specific party\n" +
                "- Status: Filter by payment status (All/Paid/Partial/Unpaid)\n" +
                "- Search: Search by bill number, party name, or broker\n\n" +
                "Navigation:\n" +
                "Tab: Move between controls\n" +
                "Enter: View bill details\n" +
                "Arrow Keys: Navigate in grid",
                "Bills Report Help",
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
            this.Text = "Bills Report";
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
            
            // Interest/Discount Info
            dgvOutstanding.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "InterestDiscountInfo",
                HeaderText = "Adjustment",
                DataPropertyName = "InterestDiscountInfo",
                Width = 90,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            
            // Adjusted Net Amount
            dgvOutstanding.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AdjustedNetAmount",
                HeaderText = "Adjusted Amount",
                DataPropertyName = "AdjustedNetAmount",
                Width = 120,
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
                        var columnName = dgvOutstanding.Columns[e.ColumnIndex].Name;
                        
                        // Format the payment status column
                        if (columnName == "PaymentStatusText")
                        {
                            string status = e.Value.ToString();
                            
                            if (status == "Paid")
                            {
                                e.CellStyle.ForeColor = Color.Green;
                                e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                            }
                            else if (status == "Partial")
                            {
                                e.CellStyle.ForeColor = Color.Blue;
                                e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                            }
                            else if (status == "Unpaid")
                            {
                                e.CellStyle.ForeColor = Color.Red;
                            }
                        }
                        // Format the interest/discount info
                        else if (columnName == "InterestDiscountInfo")
                        {
                            string value = e.Value?.ToString() ?? string.Empty;
                            
                            if (value.StartsWith("+"))
                            {
                                e.CellStyle.ForeColor = Color.Red; // Interest is red
                                e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                            }
                            else if (value.StartsWith("-"))
                            {
                                e.CellStyle.ForeColor = Color.Green; // Discount is green
                                e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                            }
                        }
                        // Format the adjusted net amount
                        else if (columnName == "AdjustedNetAmount")
                        {
                            // Get payment status from the same row
                            var statusCell = dgvOutstanding.Rows[e.RowIndex].Cells["PaymentStatusText"];
                            if (statusCell != null && statusCell.Value != null)
                            {
                                string status = statusCell.Value.ToString();
                                if (status == "Unpaid")
                                {
                                    // Hide adjusted amount for unpaid bills by making it same as regular net amount
                                    var netAmountCell = dgvOutstanding.Rows[e.RowIndex].Cells["NetAmount"];
                                    if (netAmountCell != null && netAmountCell.Value != null)
                                    {
                                        e.Value = netAmountCell.Value;
                                    }
                                }
                                else
                                {
                                    // For paid/partial: show in bold with appropriate color
                                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                                    
                                    // Get interest/discount info to determine color
                                    var infoCell = dgvOutstanding.Rows[e.RowIndex].Cells["InterestDiscountInfo"];
                                    if (infoCell != null && infoCell.Value != null)
                                    {
                                        string info = infoCell.Value.ToString();
                                        if (info.StartsWith("+"))
                                        {
                                            e.CellStyle.ForeColor = Color.Firebrick; // Higher amount due to interest
                                        }
                                        else if (info.StartsWith("-"))
                                        {
                                            e.CellStyle.ForeColor = Color.DarkGreen; // Lower amount due to discount
                                        }
                                    }
                                }
                            }
                        }
                        // Color code based on overdue status
                        else if (data.DaysOverdue > 30)
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

                // Apply status filter
                string statusFilter = cmbStatus.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
                {
                    filteredBills = filteredBills.Where(b => b.PaymentStatusText == statusFilter).ToList();
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
                        InterestDiscountInfo = b.GetInterestDiscountInfo(),
                        AdjustedNetAmount = b.AdjustedNetAmount,
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
                    double totalAdjustedNetAmount = filteredBills.Sum(b => b.AdjustedNetAmount);
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
                        InterestDiscountInfo = "",
                        AdjustedNetAmount = totalAdjustedNetAmount,
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
                // Get selected party name for the report title
                string partyFilter = "All Parties";
                if (cmbParty.SelectedValue is int partyId && partyId > 0)
                {
                    partyFilter = cmbParty.Text;
                }

                // Get selected status filter
                string statusFilter = cmbStatus.SelectedItem?.ToString() ?? "All";

                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                    DefaultExt = "xlsx",
                    FileName = $"Outstanding_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    // Add EPPlus license context for non-commercial use
                    OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                    
                    using (var package = new OfficeOpenXml.ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Outstanding Report");
                        
                        // Add report title
                        worksheet.Cells[1, 1].Value = $"Outstanding Report - {Program.ActiveCompany?.CompanyName}";
                        worksheet.Cells[1, 1, 1, 12].Merge = true;
                        worksheet.Cells[1, 1].Style.Font.Size = 16;
                        worksheet.Cells[1, 1].Style.Font.Bold = true;
                        worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        
                        // Add party filter info
                        worksheet.Cells[2, 1].Value = $"Party: {partyFilter} | Status: {statusFilter}";
                        worksheet.Cells[2, 1, 2, 12].Merge = true;
                        worksheet.Cells[2, 1].Style.Font.Size = 12;
                        worksheet.Cells[2, 1].Style.Font.Bold = true;
                        worksheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        
                        // Add generated date
                        worksheet.Cells[3, 1].Value = $"Generated on: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                        worksheet.Cells[3, 1, 3, 12].Merge = true;
                        worksheet.Cells[3, 1].Style.Font.Size = 10;
                        worksheet.Cells[3, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        
                        // Add total summary
                        worksheet.Cells[4, 1].Value = $"Total Bills: {filteredBills.Count}, Total Outstanding: ₹{filteredBills.Sum(b => b.BalanceAmount):N2}";
                        worksheet.Cells[4, 1, 4, 12].Merge = true;
                        worksheet.Cells[4, 1].Style.Font.Size = 10;
                        worksheet.Cells[4, 1].Style.Font.Bold = true;
                        worksheet.Cells[4, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        
                        // Add header row at row 6
                        var headers = new string[] { 
                            "Bill No", "Bill Date", "Due Date", "Party Name", "Broker", 
                            "Bill Amount", "Adjustment", "Adjusted Amount", "Paid Amount", 
                            "Outstanding", "Days Overdue", "Status"
                        };
                        
                        for (int i = 0; i < headers.Length; i++)
                        {
                            worksheet.Cells[6, i + 1].Value = headers[i];
                            worksheet.Cells[6, i + 1].Style.Font.Bold = true;
                            worksheet.Cells[6, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            worksheet.Cells[6, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(64, 64, 64));
                            worksheet.Cells[6, i + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
                        }
                        
                        // Add data rows starting at row 7
                        int row = 7;
                    foreach (var bill in filteredBills.OrderBy(x => x.PartyName).ThenBy(x => x.BillDate))
                    {
                        int daysOverdue = bill.IsOverdue ? bill.DaysOverdue : (bill.DaysUntilDue >= 0 ? -bill.DaysUntilDue : bill.DaysOverdue);
                        string brokerName = string.IsNullOrEmpty(bill.BrokerName) ? "No Broker" : bill.BrokerName;
                        
                            // Apply row color based on overdue status
                            var fillColor = System.Drawing.Color.White;
                            if (daysOverdue > 30)
                                fillColor = System.Drawing.Color.FromArgb(255, 200, 200); // Light red for very overdue
                            else if (daysOverdue > 0)
                                fillColor = System.Drawing.Color.FromArgb(255, 240, 200); // Light orange for overdue
                            else if (daysOverdue < 0 && daysOverdue >= -7)
                                fillColor = System.Drawing.Color.FromArgb(255, 255, 200); // Light yellow for due soon
                            
                            worksheet.Cells[row, 1].Value = bill.BillNo;
                            worksheet.Cells[row, 2].Value = bill.BillDate;
                            worksheet.Cells[row, 2].Style.Numberformat.Format = "dd/MM/yyyy";
                            worksheet.Cells[row, 3].Value = bill.DueDate;
                            worksheet.Cells[row, 3].Style.Numberformat.Format = "dd/MM/yyyy";
                            worksheet.Cells[row, 4].Value = bill.PartyName;
                            worksheet.Cells[row, 5].Value = brokerName;
                            worksheet.Cells[row, 6].Value = bill.NetAmount;
                            worksheet.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";
                            worksheet.Cells[row, 7].Value = bill.GetInterestDiscountInfo();
                            
                            // Format interest/discount cell
                            if (bill.GetInterestDiscountInfo().StartsWith("+"))
                            {
                                worksheet.Cells[row, 7].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                                worksheet.Cells[row, 7].Style.Font.Bold = true;
                            }
                            else if (bill.GetInterestDiscountInfo().StartsWith("-"))
                            {
                                worksheet.Cells[row, 7].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                                worksheet.Cells[row, 7].Style.Font.Bold = true;
                            }
                            
                            worksheet.Cells[row, 8].Value = bill.AdjustedNetAmount;
                            worksheet.Cells[row, 8].Style.Numberformat.Format = "#,##0.00";
                            worksheet.Cells[row, 9].Value = bill.PaidAmount;
                            worksheet.Cells[row, 9].Style.Numberformat.Format = "#,##0.00";
                            worksheet.Cells[row, 10].Value = bill.BalanceAmount;
                            worksheet.Cells[row, 10].Style.Numberformat.Format = "#,##0.00";
                            worksheet.Cells[row, 10].Style.Font.Bold = true;
                            worksheet.Cells[row, 11].Value = daysOverdue;
                            worksheet.Cells[row, 12].Value = bill.PaymentStatusText;
                            
                            // Format status cell
                            if (bill.PaymentStatusText == "Paid")
                            {
                                worksheet.Cells[row, 12].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                                worksheet.Cells[row, 12].Style.Font.Bold = true;
                            }
                            else if (bill.PaymentStatusText == "Partial")
                            {
                                worksheet.Cells[row, 12].Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                                worksheet.Cells[row, 12].Style.Font.Bold = true;
                            }
                            else if (bill.PaymentStatusText == "Unpaid")
                            {
                                worksheet.Cells[row, 12].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                            }
                            
                            // Apply row background color
                            if (fillColor != System.Drawing.Color.White)
                            {
                                var rowRange = worksheet.Cells[row, 1, row, 12];
                                rowRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                rowRange.Style.Fill.BackgroundColor.SetColor(fillColor);
                            }
                            
                            row++;
                        }
                        
                        // Add total row
                        worksheet.Cells[row, 1].Value = "TOTAL";
                        worksheet.Cells[row, 1].Style.Font.Bold = true;
                        worksheet.Cells[row, 4].Value = $"{filteredBills.Count} bills";
                        worksheet.Cells[row, 6].Value = filteredBills.Sum(b => b.NetAmount);
                        worksheet.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";
                        worksheet.Cells[row, 8].Value = filteredBills.Sum(b => b.AdjustedNetAmount);
                        worksheet.Cells[row, 8].Style.Numberformat.Format = "#,##0.00";
                        worksheet.Cells[row, 9].Value = filteredBills.Sum(b => b.PaidAmount);
                        worksheet.Cells[row, 9].Style.Numberformat.Format = "#,##0.00";
                        worksheet.Cells[row, 10].Value = filteredBills.Sum(b => b.BalanceAmount);
                        worksheet.Cells[row, 10].Style.Numberformat.Format = "#,##0.00";
                        
                        var totalRowRange = worksheet.Cells[row, 1, row, 12];
                        totalRowRange.Style.Font.Bold = true;
                        totalRowRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        totalRowRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(220, 220, 220));
                        
                        // Auto-fit columns
                        for (int i = 1; i <= 12; i++)
                        {
                            worksheet.Column(i).AutoFit();
                        }
                        
                        // Save the Excel package
                        package.SaveAs(new FileInfo(saveDialog.FileName));
                    }
                    
                    MessageBox.Show($"Report exported successfully to:\n{saveDialog.FileName}", "Export Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                // Get selected party name for the report title
                string partyFilter = "All Parties";
                if (cmbParty.SelectedValue is int partyId && partyId > 0)
                {
                    partyFilter = cmbParty.Text;
                }

                // Get selected status filter
                string statusFilter = cmbStatus.SelectedItem?.ToString() ?? "All";

                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*",
                    DefaultExt = "pdf",
                    FileName = $"Outstanding_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    // Create a new PDF document
                    using (var fileStream = new FileStream(saveDialog.FileName, FileMode.Create))
                    {
                        // Document setup
                        var document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4.Rotate(), 30, 30, 30, 30);
                        var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, fileStream);
                        document.Open();

                        // Add fonts
                        var baseFont = iTextSharp.text.pdf.BaseFont.CreateFont(
                            iTextSharp.text.pdf.BaseFont.HELVETICA,
                            iTextSharp.text.pdf.BaseFont.CP1252,
                            iTextSharp.text.pdf.BaseFont.NOT_EMBEDDED);

                        var normalFont = new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.NORMAL);
                        var boldFont = new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.BOLD);
                        var titleFont = new iTextSharp.text.Font(baseFont, 16, iTextSharp.text.Font.BOLD);
                        var subtitleFont = new iTextSharp.text.Font(baseFont, 12, iTextSharp.text.Font.BOLD);
                        var smallFont = new iTextSharp.text.Font(baseFont, 8, iTextSharp.text.Font.NORMAL);

                        // Colors
                        var redFont = new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(255, 0, 0));
                        var redBoldFont = new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(255, 0, 0));
                        var greenFont = new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(0, 128, 0));
                        var greenBoldFont = new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(0, 128, 0));
                        var blueFont = new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(0, 0, 255));
                        var blueBoldFont = new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(0, 0, 255));

                        // Report title
                        var titlePara = new iTextSharp.text.Paragraph($"Outstanding Report - {Program.ActiveCompany?.CompanyName}", titleFont);
                        titlePara.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                        document.Add(titlePara);

                        // Party filter
                        var filterPara = new iTextSharp.text.Paragraph($"Party: {partyFilter} | Status: {statusFilter}", subtitleFont);
                        filterPara.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                        document.Add(filterPara);

                        // Date and totals
                        var datePara = new iTextSharp.text.Paragraph($"Generated on: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", normalFont);
                        datePara.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                        document.Add(datePara);
                        
                        var totalsPara = new iTextSharp.text.Paragraph(
                            $"Total Bills: {filteredBills.Count}, Total Outstanding: ₹{filteredBills.Sum(b => b.BalanceAmount):N2}", 
                            boldFont);
                        totalsPara.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                        document.Add(totalsPara);
                        
                        document.Add(new iTextSharp.text.Paragraph(" ")); // Add a blank line

                        // Create table
                        var table = new iTextSharp.text.pdf.PdfPTable(12)
                        {
                            WidthPercentage = 100,
                            SpacingBefore = 10f,
                            SpacingAfter = 10f
                        };

                        // Set column widths
                        float[] columnWidths = new float[] { 5f, 6f, 6f, 12f, 9f, 8f, 8f, 8f, 8f, 8f, 6f, 6f };
                        table.SetWidths(columnWidths);

                        // Header cells
                        string[] headers = new string[] {
                            "Bill No", "Bill Date", "Due Date", "Party Name", "Broker", 
                            "Bill Amount", "Adjustment", "Adjusted Amt", "Paid Amount", 
                            "Outstanding", "Days Due", "Status"
                        };

                        var headerColor = new iTextSharp.text.BaseColor(64, 64, 64);
                        foreach (string header in headers)
                        {
                            var cell = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(header, new iTextSharp.text.Font(baseFont, 9, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(255, 255, 255))))
                            {
                                BackgroundColor = headerColor,
                                HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER,
                                VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE,
                                Padding = 4
                            };
                            table.AddCell(cell);
                        }

                        // Data rows
                        foreach (var bill in filteredBills.OrderBy(x => x.PartyName).ThenBy(x => x.BillDate))
                        {
                            int daysOverdue = bill.IsOverdue ? bill.DaysOverdue : (bill.DaysUntilDue >= 0 ? -bill.DaysUntilDue : bill.DaysOverdue);
                            string brokerName = string.IsNullOrEmpty(bill.BrokerName) ? "No Broker" : bill.BrokerName;
                            
                            // Background color based on overdue status
                            iTextSharp.text.BaseColor bgColor = null;
                            if (daysOverdue > 30)
                                bgColor = new iTextSharp.text.BaseColor(255, 200, 200);
                            else if (daysOverdue > 0)
                                bgColor = new iTextSharp.text.BaseColor(255, 240, 200);
                            else if (daysOverdue < 0 && daysOverdue >= -7)
                                bgColor = new iTextSharp.text.BaseColor(255, 255, 200);
                            
                            // Add data cells
                            AddCell(table, bill.BillNo, normalFont, iTextSharp.text.Element.ALIGN_LEFT, bgColor);
                            AddCell(table, bill.BillDate.ToString("dd/MM/yyyy"), normalFont, iTextSharp.text.Element.ALIGN_CENTER, bgColor);
                            AddCell(table, bill.DueDate.ToString("dd/MM/yyyy"), normalFont, iTextSharp.text.Element.ALIGN_CENTER, bgColor);
                            AddCell(table, bill.PartyName, normalFont, iTextSharp.text.Element.ALIGN_LEFT, bgColor);
                            AddCell(table, brokerName, normalFont, iTextSharp.text.Element.ALIGN_LEFT, bgColor);
                            AddCell(table, bill.NetAmount.ToString("N2"), normalFont, iTextSharp.text.Element.ALIGN_RIGHT, bgColor);
                            
                            // Interest/Discount cell
                            string interestDiscount = bill.GetInterestDiscountInfo();
                            if (interestDiscount.StartsWith("+"))
                                AddCell(table, interestDiscount, redBoldFont, iTextSharp.text.Element.ALIGN_RIGHT, bgColor);
                            else if (interestDiscount.StartsWith("-"))
                                AddCell(table, interestDiscount, greenBoldFont, iTextSharp.text.Element.ALIGN_RIGHT, bgColor);
                            else
                                AddCell(table, interestDiscount, normalFont, iTextSharp.text.Element.ALIGN_RIGHT, bgColor);
                            
                            AddCell(table, bill.AdjustedNetAmount.ToString("N2"), boldFont, iTextSharp.text.Element.ALIGN_RIGHT, bgColor);
                            AddCell(table, bill.PaidAmount.ToString("N2"), normalFont, iTextSharp.text.Element.ALIGN_RIGHT, bgColor);
                            AddCell(table, bill.BalanceAmount.ToString("N2"), boldFont, iTextSharp.text.Element.ALIGN_RIGHT, bgColor);
                            AddCell(table, daysOverdue.ToString(), normalFont, iTextSharp.text.Element.ALIGN_CENTER, bgColor);
                            
                            // Status cell
                            if (bill.PaymentStatusText == "Paid")
                                AddCell(table, bill.PaymentStatusText, greenBoldFont, iTextSharp.text.Element.ALIGN_CENTER, bgColor);
                            else if (bill.PaymentStatusText == "Partial")
                                AddCell(table, bill.PaymentStatusText, blueBoldFont, iTextSharp.text.Element.ALIGN_CENTER, bgColor);
                            else
                                AddCell(table, bill.PaymentStatusText, redFont, iTextSharp.text.Element.ALIGN_CENTER, bgColor);
                        }
                        
                        // Add summary row
                        var summaryBgColor = new iTextSharp.text.BaseColor(220, 220, 220);
                        
                        AddCell(table, "TOTAL", boldFont, iTextSharp.text.Element.ALIGN_LEFT, summaryBgColor);
                        AddCell(table, "", boldFont, iTextSharp.text.Element.ALIGN_CENTER, summaryBgColor);
                        AddCell(table, "", boldFont, iTextSharp.text.Element.ALIGN_CENTER, summaryBgColor);
                        AddCell(table, $"{filteredBills.Count} bills", boldFont, iTextSharp.text.Element.ALIGN_LEFT, summaryBgColor);
                        AddCell(table, "", boldFont, iTextSharp.text.Element.ALIGN_LEFT, summaryBgColor);
                        AddCell(table, filteredBills.Sum(b => b.NetAmount).ToString("N2"), boldFont, iTextSharp.text.Element.ALIGN_RIGHT, summaryBgColor);
                        AddCell(table, "", boldFont, iTextSharp.text.Element.ALIGN_RIGHT, summaryBgColor);
                        AddCell(table, filteredBills.Sum(b => b.AdjustedNetAmount).ToString("N2"), boldFont, iTextSharp.text.Element.ALIGN_RIGHT, summaryBgColor);
                        AddCell(table, filteredBills.Sum(b => b.PaidAmount).ToString("N2"), boldFont, iTextSharp.text.Element.ALIGN_RIGHT, summaryBgColor);
                        AddCell(table, filteredBills.Sum(b => b.BalanceAmount).ToString("N2"), boldFont, iTextSharp.text.Element.ALIGN_RIGHT, summaryBgColor);
                        AddCell(table, "", boldFont, iTextSharp.text.Element.ALIGN_CENTER, summaryBgColor);
                        AddCell(table, "", boldFont, iTextSharp.text.Element.ALIGN_CENTER, summaryBgColor);
                        
                        // Add the table to the document
                        document.Add(table);
                        
                        // Add footer
                        document.Add(new iTextSharp.text.Paragraph(" "));
                        var footerPara = new iTextSharp.text.Paragraph($"Report generated from {Program.ActiveCompany?.CompanyName} Billing System", smallFont);
                        footerPara.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                        document.Add(footerPara);
                        
                        // Close the document
                        document.Close();
                    }
                    
                    MessageBox.Show($"Report exported successfully to:\n{saveDialog.FileName}", "Export Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to PDF: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void AddCell(iTextSharp.text.pdf.PdfPTable table, string text, iTextSharp.text.Font font, 
            int alignment, iTextSharp.text.BaseColor bgColor = null)
        {
            var cell = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(text, font))
            {
                HorizontalAlignment = alignment,
                VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE,
                Padding = 4
            };
            
            if (bgColor != null)
                cell.BackgroundColor = bgColor;
                
            table.AddCell(cell);
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

        private void cmbStatus_SelectedIndexChanged(object sender, EventArgs e)
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
        public string InterestDiscountInfo { get; set; } = string.Empty;
        public double AdjustedNetAmount { get; set; }
    }
} 