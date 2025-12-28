using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class GodownStockLedgerControl : UserControl
    {
        private List<Godown> godowns = new List<Godown>();
        private List<GodownItem> items = new List<GodownItem>();
        private List<GodownLedgerViewModel> ledgerData = new List<GodownLedgerViewModel>();
        private bool _isLoading = false;

        public GodownStockLedgerControl()
        {
            InitializeComponent();
        }

        private void GodownStockLedgerControl_Load(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            _isLoading = true;
            try
            {
                LoadFilters();
                SetupDataGrid();

                // Set default date range (current month)
                dtpFromDate.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                dtpToDate.Value = DateTime.Today;

                LoadLedger();
            }
            finally
            {
                _isLoading = false;
                this.Cursor = Cursors.Default;
            }
        }

        private void LoadFilters()
        {
            try
            {
                godowns = GodownService.GetAllGodowns();
                items = GodownItemService.GetAllGodownItems();

                // Setup godown combo
                cmbFilterGodown.DropDownStyle = ComboBoxStyle.DropDown;
                cmbFilterGodown.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                cmbFilterGodown.AutoCompleteSource = AutoCompleteSource.ListItems;

                var godownList = new List<string> { "All Godowns" };
                godownList.AddRange(godowns.Select(g => g.GodownName).Distinct());
                cmbFilterGodown.DataSource = godownList;
                cmbFilterGodown.SelectedIndex = 0;

                // Setup item combo
                cmbFilterItem.DropDownStyle = ComboBoxStyle.DropDown;
                cmbFilterItem.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                cmbFilterItem.AutoCompleteSource = AutoCompleteSource.ListItems;

                var itemList = new List<string> { "All Items" };
                itemList.AddRange(items.Select(i => i.ItemName).Distinct());
                cmbFilterItem.DataSource = itemList;
                cmbFilterItem.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading filters: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupDataGrid()
        {
            dgvLedger.AutoGenerateColumns = false;
            dgvLedger.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLedger.AllowUserToAddRows = false;
            dgvLedger.AllowUserToDeleteRows = false;
            dgvLedger.ReadOnly = true;
            dgvLedger.MultiSelect = false;
            dgvLedger.RowHeadersVisible = false;
            dgvLedger.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(245, 245, 245)
            };

            dgvLedger.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            dgvLedger.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvLedger.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(64, 64, 64);
            dgvLedger.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvLedger.ColumnHeadersHeight = 35;
            dgvLedger.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvLedger.RowTemplate.Height = 28;

            dgvLedger.Columns.Clear();

            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TransactionDateFormatted",
                HeaderText = "Date",
                Width = 90,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            });

            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TransactionNo",
                HeaderText = "Trans. No",
                Width = 110,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            });

            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "GodownName",
                HeaderText = "Godown",
                Width = 130,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            });

            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ItemName",
                HeaderText = "Item",
                Width = 180,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            });

            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TransactionType",
                HeaderText = "Type",
                Width = 80,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            });

            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "InwardQty",
                HeaderText = "Inward",
                Width = 90,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    BackColor = Color.FromArgb(200, 230, 255)
                }
            });

            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OutwardQty",
                HeaderText = "Outward",
                Width = 90,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    BackColor = Color.FromArgb(255, 220, 180)
                }
            });

            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "BalanceQty",
                HeaderText = "Balance",
                Width = 90,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    BackColor = Color.LightGreen,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                }
            });

            dgvLedger.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Remarks",
                HeaderText = "Remarks",
                Width = 180,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            // Enable double buffering
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, dgvLedger, new object[] { true });
        }

        private void LoadLedger()
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                int? godownID = null;
                int? itemID = null;
                DateTime fromDate = dtpFromDate.Value.Date;
                DateTime toDate = dtpToDate.Value.Date;

                if (cmbFilterGodown.SelectedItem != null && cmbFilterGodown.SelectedIndex > 0)
                {
                    string selectedGodown = cmbFilterGodown.SelectedItem.ToString() ?? "";
                    var godown = godowns.FirstOrDefault(g => g.GodownName == selectedGodown);
                    if (godown != null)
                    {
                        godownID = godown.GodownID;
                    }
                }

                if (cmbFilterItem.SelectedItem != null && cmbFilterItem.SelectedIndex > 0)
                {
                    string selectedItem = cmbFilterItem.SelectedItem.ToString() ?? "";
                    var item = items.FirstOrDefault(i => i.ItemName == selectedItem);
                    if (item != null)
                    {
                        itemID = item.GodownItemID;
                    }
                }

                ledgerData = GodownStockService.GetLedgerEntriesWithRunningBalance(fromDate, toDate, godownID, itemID);

                dgvLedger.DataSource = null;
                dgvLedger.DataSource = ledgerData;

                // Format rows based on transaction type
                FormatGridRows();

                // Calculate totals for status bar
                double totalInward = ledgerData.Sum(l => l.InwardQty);
                double totalOutward = ledgerData.Sum(l => l.OutwardQty);

                lblTotalRecords.Text = $"Records: {ledgerData.Count} | Total Inward: {totalInward:N2} | Total Outward: {totalOutward:N2} | Period: {fromDate:dd-MM-yyyy} to {toDate:dd-MM-yyyy}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading ledger: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void FormatGridRows()
        {
            // Column indices: 0=Date, 1=TransNo, 2=Godown, 3=Item, 4=Type, 5=Inward, 6=Outward, 7=Balance, 8=Remarks
            const int TYPE_COL = 4;

            foreach (DataGridViewRow row in dgvLedger.Rows)
            {
                if (row.DataBoundItem is GodownLedgerViewModel entry)
                {
                    // Color code based on transaction type
                    if (entry.TransactionType == "Opening")
                    {
                        // Opening balance row - highlight entire row with distinct style
                        row.DefaultCellStyle.BackColor = Color.FromArgb(230, 230, 250); // Lavender
                        row.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                        row.Cells[TYPE_COL].Style.BackColor = Color.FromArgb(180, 180, 220);
                        row.Cells[TYPE_COL].Style.ForeColor = Color.DarkBlue;
                    }
                    else if (entry.TransactionType == "Transfer")
                    {
                        row.Cells[TYPE_COL].Style.BackColor = Color.FromArgb(255, 240, 200);
                        row.Cells[TYPE_COL].Style.ForeColor = Color.DarkOrange;
                    }
                    else if (entry.TransactionType == "Inward")
                    {
                        row.Cells[TYPE_COL].Style.BackColor = Color.FromArgb(200, 255, 200);
                        row.Cells[TYPE_COL].Style.ForeColor = Color.DarkGreen;
                    }
                    else if (entry.TransactionType == "Outward")
                    {
                        row.Cells[TYPE_COL].Style.BackColor = Color.FromArgb(255, 200, 200);
                        row.Cells[TYPE_COL].Style.ForeColor = Color.DarkRed;
                    }
                }
            }
        }

        private void dgvLedger_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Column indices: 5=Inward, 6=Outward
            if (e.ColumnIndex == 5 || e.ColumnIndex == 6)
            {
                if (e.Value != null && e.Value is double doubleValue && doubleValue == 0)
                {
                    e.Value = "-";
                    e.FormattingApplied = true;
                }
            }
        }

        #region Event Handlers

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadLedger();
        }

        private void cmbFilterGodown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isLoading)
            {
                LoadLedger();
            }
        }

        private void cmbFilterItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isLoading)
            {
                LoadLedger();
            }
        }

        private void dtpFromDate_ValueChanged(object sender, EventArgs e)
        {
            if (!_isLoading)
            {
                // Ensure from date is not after to date
                if (dtpFromDate.Value > dtpToDate.Value)
                {
                    dtpToDate.Value = dtpFromDate.Value;
                }
                LoadLedger();
            }
        }

        private void dtpToDate_ValueChanged(object sender, EventArgs e)
        {
            if (!_isLoading)
            {
                // Ensure to date is not before from date
                if (dtpToDate.Value < dtpFromDate.Value)
                {
                    dtpFromDate.Value = dtpToDate.Value;
                }
                LoadLedger();
            }
        }

        #endregion

        #region Export Functionality

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            ExportToExcel();
        }

        private void btnExportPDF_Click(object sender, EventArgs e)
        {
            ExportToPDF();
        }

        private void ExportToExcel()
        {
            if (ledgerData == null || ledgerData.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Excel Files (*.csv)|*.csv|All Files (*.*)|*.*";
                saveDialog.DefaultExt = "csv";
                saveDialog.FileName = $"GodownStockLedger_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        this.Cursor = Cursors.WaitCursor;

                        StringBuilder sb = new StringBuilder();

                        // Add report header
                        sb.AppendLine($"Godown Stock Ledger");
                        sb.AppendLine($"Period: {dtpFromDate.Value:dd-MM-yyyy} to {dtpToDate.Value:dd-MM-yyyy}");
                        sb.AppendLine($"Generated on: {DateTime.Now:dd-MM-yyyy HH:mm:ss}");
                        sb.AppendLine();

                        // Add column headers
                        sb.AppendLine("Date,Transaction No,Godown,Item,Type,Inward,Outward,Balance,Remarks");

                        // Add data rows
                        foreach (var row in ledgerData)
                        {
                            string godownName = EscapeCsvField(row.GodownName ?? "");
                            string itemName = EscapeCsvField(row.ItemName ?? "");
                            string remarks = EscapeCsvField(row.Remarks ?? "");

                            sb.AppendLine($"{row.TransactionDate:dd-MM-yyyy},{row.TransactionNo},{godownName},{itemName},{row.TransactionType},{row.InwardQty:N2},{row.OutwardQty:N2},{row.BalanceQty:N2},{remarks}");
                        }

                        // Add totals
                        double totalInward = ledgerData.Sum(l => l.InwardQty);
                        double totalOutward = ledgerData.Sum(l => l.OutwardQty);
                        sb.AppendLine();
                        sb.AppendLine($"TOTALS,,,,,{totalInward:N2},{totalOutward:N2},,");

                        File.WriteAllText(saveDialog.FileName, sb.ToString(), Encoding.UTF8);

                        MessageBox.Show($"Report exported successfully to:\n{saveDialog.FileName}",
                            "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Open the file
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exporting to Excel: {ex.Message}",
                            "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        this.Cursor = Cursors.Default;
                    }
                }
            }
        }

        private string EscapeCsvField(string field)
        {
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }

        private void ExportToPDF()
        {
            if (ledgerData == null || ledgerData.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "HTML Files (*.html)|*.html|All Files (*.*)|*.*";
                saveDialog.DefaultExt = "html";
                saveDialog.FileName = $"GodownStockLedger_{DateTime.Now:yyyyMMdd_HHmmss}.html";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        this.Cursor = Cursors.WaitCursor;

                        StringBuilder html = new StringBuilder();

                        // HTML header with print-friendly CSS
                        html.AppendLine("<!DOCTYPE html>");
                        html.AppendLine("<html><head>");
                        html.AppendLine("<meta charset=\"UTF-8\">");
                        html.AppendLine("<title>Godown Stock Ledger</title>");
                        html.AppendLine("<style>");
                        html.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; margin: 20px; }");
                        html.AppendLine("h1 { text-align: center; color: #333; margin-bottom: 5px; }");
                        html.AppendLine("h3 { text-align: center; color: #666; margin-top: 5px; }");
                        html.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; font-size: 10px; }");
                        html.AppendLine("th { background-color: #404040; color: white; padding: 8px 4px; text-align: center; border: 1px solid #333; }");
                        html.AppendLine("td { padding: 5px 4px; border: 1px solid #ddd; }");
                        html.AppendLine("td.number { text-align: right; }");
                        html.AppendLine("td.center { text-align: center; }");
                        html.AppendLine("tr:nth-child(even) { background-color: #f9f9f9; }");
                        html.AppendLine(".inward { background-color: #cce5ff; }");
                        html.AppendLine(".outward { background-color: #ffdcb4; }");
                        html.AppendLine(".balance { background-color: #90ee90; font-weight: bold; }");
                        html.AppendLine(".type-inward { background-color: #c8ffc8; color: darkgreen; }");
                        html.AppendLine(".type-outward { background-color: #ffc8c8; color: darkred; }");
                        html.AppendLine(".type-transfer { background-color: #fff0c8; color: darkorange; }");
                        html.AppendLine(".totals { background-color: #404040; color: white; font-weight: bold; }");
                        html.AppendLine("@media print { body { margin: 0; } table { font-size: 8px; } }");
                        html.AppendLine(".print-btn { padding: 10px 20px; background: #007bff; color: white; border: none; cursor: pointer; margin: 10px; font-size: 14px; }");
                        html.AppendLine(".print-btn:hover { background: #0056b3; }");
                        html.AppendLine("@media print { .no-print { display: none; } }");
                        html.AppendLine("</style>");
                        html.AppendLine("</head><body>");

                        // Print button
                        html.AppendLine("<div class=\"no-print\" style=\"text-align: center;\">");
                        html.AppendLine("<button class=\"print-btn\" onclick=\"window.print()\">Print / Save as PDF</button>");
                        html.AppendLine("</div>");

                        // Report header
                        html.AppendLine("<h1>Godown Stock Ledger</h1>");
                        html.AppendLine($"<h3>Period: {dtpFromDate.Value:dd-MM-yyyy} to {dtpToDate.Value:dd-MM-yyyy}</h3>");
                        html.AppendLine($"<p style=\"text-align: center; color: #999;\">Generated on: {DateTime.Now:dd-MM-yyyy HH:mm:ss}</p>");

                        // Table
                        html.AppendLine("<table>");
                        html.AppendLine("<thead><tr>");
                        html.AppendLine("<th>Date</th>");
                        html.AppendLine("<th>Trans. No</th>");
                        html.AppendLine("<th>Godown</th>");
                        html.AppendLine("<th>Item</th>");
                        html.AppendLine("<th>Type</th>");
                        html.AppendLine("<th class=\"inward\">Inward</th>");
                        html.AppendLine("<th class=\"outward\">Outward</th>");
                        html.AppendLine("<th class=\"balance\">Balance</th>");
                        html.AppendLine("<th>Remarks</th>");
                        html.AppendLine("</tr></thead>");
                        html.AppendLine("<tbody>");

                        // Data rows
                        foreach (var row in ledgerData)
                        {
                            string typeClass = row.TransactionType == "Inward" ? "type-inward" :
                                              row.TransactionType == "Outward" ? "type-outward" : "type-transfer";

                            string inwardDisplay = row.InwardQty > 0 ? row.InwardQty.ToString("N2") : "-";
                            string outwardDisplay = row.OutwardQty > 0 ? row.OutwardQty.ToString("N2") : "-";

                            html.AppendLine("<tr>");
                            html.AppendLine($"<td class=\"center\">{row.TransactionDate:dd-MM-yyyy}</td>");
                            html.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(row.TransactionNo ?? "")}</td>");
                            html.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(row.GodownName ?? "")}</td>");
                            html.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(row.ItemName ?? "")}</td>");
                            html.AppendLine($"<td class=\"center {typeClass}\">{row.TransactionType}</td>");
                            html.AppendLine($"<td class=\"number inward\">{inwardDisplay}</td>");
                            html.AppendLine($"<td class=\"number outward\">{outwardDisplay}</td>");
                            html.AppendLine($"<td class=\"number balance\">{row.BalanceQty:N2}</td>");
                            html.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(row.Remarks ?? "")}</td>");
                            html.AppendLine("</tr>");
                        }

                        // Totals row
                        double totalInward = ledgerData.Sum(l => l.InwardQty);
                        double totalOutward = ledgerData.Sum(l => l.OutwardQty);
                        html.AppendLine($"<tr class=\"totals\">");
                        html.AppendLine("<td colspan=\"5\" style=\"text-align: right;\">TOTALS:</td>");
                        html.AppendLine($"<td class=\"number\">{totalInward:N2}</td>");
                        html.AppendLine($"<td class=\"number\">{totalOutward:N2}</td>");
                        html.AppendLine("<td></td>");
                        html.AppendLine("<td></td>");
                        html.AppendLine("</tr>");

                        html.AppendLine("</tbody></table>");

                        // Footer
                        html.AppendLine("<p style=\"text-align: center; margin-top: 20px; color: #999; font-size: 10px;\">To save as PDF: Click Print button, then choose 'Save as PDF' as the destination.</p>");

                        html.AppendLine("</body></html>");

                        File.WriteAllText(saveDialog.FileName, html.ToString(), Encoding.UTF8);

                        MessageBox.Show($"Report exported successfully to:\n{saveDialog.FileName}\n\nThe file will open in your browser. Use the Print button to save as PDF.",
                            "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Open the file in default browser
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exporting to PDF: {ex.Message}",
                            "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        this.Cursor = Cursors.Default;
                    }
                }
            }
        }

        #endregion
    }
}
