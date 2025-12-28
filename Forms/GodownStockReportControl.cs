using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    /// <summary>
    /// Display model for stock report including subtotal rows
    /// </summary>
    public class StockReportDisplayRow
    {
        public string GodownName { get; set; }
        public string ItemName { get; set; }
        public double? OpeningStock { get; set; }
        public double? DirectInward { get; set; }
        public double? TransferIn { get; set; }
        public double? TotalInward { get; set; }
        public double? DirectOutward { get; set; }
        public double? TransferOut { get; set; }
        public double? TotalOutward { get; set; }
        public double? CurrentStock { get; set; }
        public bool IsSubtotal { get; set; }
        public bool IsGrandTotal { get; set; }
    }

    public partial class GodownStockReportControl : UserControl
    {
        private List<Godown> godowns = new List<Godown>();
        private List<GodownItem> items = new List<GodownItem>();
        private List<GodownStockViewModel> stockData = new List<GodownStockViewModel>();
        private List<StockReportDisplayRow> displayData = new List<StockReportDisplayRow>();
        private bool _isLoading = false;

        public GodownStockReportControl()
        {
            InitializeComponent();
        }

        private void GodownStockReportControl_Load(object sender, EventArgs e)
        {
            // Show loading cursor while loading data
            this.Cursor = Cursors.WaitCursor;
            _isLoading = true;
            try
            {
                LoadData();
                SetupDataGrid();
                LoadStockReport();
            }
            finally
            {
                _isLoading = false;
                this.Cursor = Cursors.Default;
            }
        }

        private void LoadData()
        {
            try
            {
                godowns = GodownService.GetAllGodowns();
                items = GodownItemService.GetAllGodownItems();

                // Setup autocomplete for filter combo boxes
                cmbFilterGodown.DropDownStyle = ComboBoxStyle.DropDown;
                cmbFilterGodown.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                cmbFilterGodown.AutoCompleteSource = AutoCompleteSource.ListItems;
                
                var godownList = new List<string> { "All Godowns" };
                godownList.AddRange(godowns.Select(g => g.GodownName).Distinct());
                cmbFilterGodown.DataSource = godownList;
                cmbFilterGodown.SelectedIndex = 0;

                cmbFilterItem.DropDownStyle = ComboBoxStyle.DropDown;
                cmbFilterItem.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                cmbFilterItem.AutoCompleteSource = AutoCompleteSource.ListItems;
                
                var itemList = new List<string> { "All Items" };
                itemList.AddRange(items.Select(i => i.ItemName).Distinct());
                cmbFilterItem.DataSource = itemList;
                cmbFilterItem.SelectedIndex = 0;

                // Set date picker to today
                dtpAsOnDate.Value = DateTime.Today;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupDataGrid()
        {
            dgvStockReport.AutoGenerateColumns = false;
            dgvStockReport.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvStockReport.AllowUserToAddRows = false;
            dgvStockReport.AllowUserToDeleteRows = false;
            dgvStockReport.ReadOnly = true;
            dgvStockReport.MultiSelect = false;
            dgvStockReport.RowHeadersVisible = false;
            dgvStockReport.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(245, 245, 245)
            };

            dgvStockReport.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            dgvStockReport.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvStockReport.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(64, 64, 64);
            dgvStockReport.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvStockReport.ColumnHeadersHeight = 35;
            dgvStockReport.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvStockReport.RowTemplate.Height = 30;

            dgvStockReport.Columns.Clear();

            dgvStockReport.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "GodownName",
                HeaderText = "Godown Name",
                Width = 150,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            });

            dgvStockReport.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ItemName",
                HeaderText = "Item Name",
                Width = 180,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            });

            dgvStockReport.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OpeningStock",
                HeaderText = "Opening",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            // Direct Inward column (blue tint for inward)
            dgvStockReport.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DirectInward",
                HeaderText = "Direct In",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    BackColor = Color.FromArgb(230, 240, 255)
                }
            });

            // Transfer In column (light blue for transfer in)
            dgvStockReport.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TransferIn",
                HeaderText = "Transfer In",
                Width = 85,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    BackColor = Color.FromArgb(200, 230, 255)
                }
            });

            dgvStockReport.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TotalInward",
                HeaderText = "Total In",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    BackColor = Color.FromArgb(180, 220, 255),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                }
            });

            // Direct Outward column (orange tint for outward)
            dgvStockReport.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DirectOutward",
                HeaderText = "Direct Out",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    BackColor = Color.FromArgb(255, 240, 220)
                }
            });

            // Transfer Out column (light orange for transfer out)
            dgvStockReport.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TransferOut",
                HeaderText = "Transfer Out",
                Width = 90,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    BackColor = Color.FromArgb(255, 220, 180)
                }
            });

            dgvStockReport.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TotalOutward",
                HeaderText = "Total Out",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    BackColor = Color.FromArgb(255, 200, 150),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                }
            });

            dgvStockReport.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CurrentStock",
                HeaderText = "Current Stock",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    BackColor = Color.LightGreen,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                }
            });
        }

        private void LoadStockReport()
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                int? godownID = null;
                int? itemID = null;
                DateTime? asOnDate = dtpAsOnDate.Value.Date;

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

                stockData = GodownStockService.GetCurrentStock(godownID, itemID, asOnDate);

                // Build display data with subtotals
                displayData = BuildDisplayDataWithSubtotals(stockData);

                dgvStockReport.DataSource = null;
                dgvStockReport.DataSource = displayData;

                // Apply row formatting for subtotals
                FormatGridRows();

                string dateText = asOnDate.HasValue ? $" as on {asOnDate.Value:dd-MM-yyyy}" : "";
                lblTotalRecords.Text = $"Total Records: {stockData.Count}{dateText}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading stock report: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private List<StockReportDisplayRow> BuildDisplayDataWithSubtotals(List<GodownStockViewModel> data)
        {
            var result = new List<StockReportDisplayRow>();

            // Group by godown
            var groupedData = data.GroupBy(d => d.GodownName).OrderBy(g => g.Key);

            // Grand totals
            double grandOpeningStock = 0, grandDirectInward = 0, grandTransferIn = 0;
            double grandDirectOutward = 0, grandTransferOut = 0, grandCurrentStock = 0;

            foreach (var godownGroup in groupedData)
            {
                // Subtotals for this godown
                double subOpeningStock = 0, subDirectInward = 0, subTransferIn = 0;
                double subDirectOutward = 0, subTransferOut = 0, subCurrentStock = 0;

                foreach (var item in godownGroup.OrderBy(i => i.ItemName))
                {
                    result.Add(new StockReportDisplayRow
                    {
                        GodownName = item.GodownName,
                        ItemName = item.ItemName,
                        OpeningStock = item.OpeningStock,
                        DirectInward = item.DirectInward,
                        TransferIn = item.TransferIn,
                        TotalInward = item.TotalInward,
                        DirectOutward = item.DirectOutward,
                        TransferOut = item.TransferOut,
                        TotalOutward = item.TotalOutward,
                        CurrentStock = item.CurrentStock,
                        IsSubtotal = false,
                        IsGrandTotal = false
                    });

                    // Accumulate subtotals
                    subOpeningStock += item.OpeningStock;
                    subDirectInward += item.DirectInward;
                    subTransferIn += item.TransferIn;
                    subDirectOutward += item.DirectOutward;
                    subTransferOut += item.TransferOut;
                    subCurrentStock += item.CurrentStock;
                }

                // Add subtotal row for this godown
                result.Add(new StockReportDisplayRow
                {
                    GodownName = $"Subtotal: {godownGroup.Key}",
                    ItemName = "",
                    OpeningStock = subOpeningStock,
                    DirectInward = subDirectInward,
                    TransferIn = subTransferIn,
                    TotalInward = subDirectInward + subTransferIn,
                    DirectOutward = subDirectOutward,
                    TransferOut = subTransferOut,
                    TotalOutward = subDirectOutward + subTransferOut,
                    CurrentStock = subCurrentStock,
                    IsSubtotal = true,
                    IsGrandTotal = false
                });

                // Accumulate grand totals
                grandOpeningStock += subOpeningStock;
                grandDirectInward += subDirectInward;
                grandTransferIn += subTransferIn;
                grandDirectOutward += subDirectOutward;
                grandTransferOut += subTransferOut;
                grandCurrentStock += subCurrentStock;
            }

            // Add grand total row if there's data from multiple godowns
            if (groupedData.Count() > 1)
            {
                result.Add(new StockReportDisplayRow
                {
                    GodownName = "GRAND TOTAL",
                    ItemName = "",
                    OpeningStock = grandOpeningStock,
                    DirectInward = grandDirectInward,
                    TransferIn = grandTransferIn,
                    TotalInward = grandDirectInward + grandTransferIn,
                    DirectOutward = grandDirectOutward,
                    TransferOut = grandTransferOut,
                    TotalOutward = grandDirectOutward + grandTransferOut,
                    CurrentStock = grandCurrentStock,
                    IsSubtotal = false,
                    IsGrandTotal = true
                });
            }

            return result;
        }

        private void FormatGridRows()
        {
            foreach (DataGridViewRow row in dgvStockReport.Rows)
            {
                if (row.DataBoundItem is StockReportDisplayRow displayRow)
                {
                    if (displayRow.IsGrandTotal)
                    {
                        // Grand total row - dark background, bold white text
                        row.DefaultCellStyle.BackColor = Color.FromArgb(50, 50, 50);
                        row.DefaultCellStyle.ForeColor = Color.White;
                        row.DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                    }
                    else if (displayRow.IsSubtotal)
                    {
                        // Subtotal row - light gray background, bold text
                        row.DefaultCellStyle.BackColor = Color.FromArgb(200, 200, 200);
                        row.DefaultCellStyle.ForeColor = Color.Black;
                        row.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    }
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadStockReport();
        }

        private void cmbFilterGodown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isLoading)
            {
                LoadStockReport();
            }
        }

        private void cmbFilterItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isLoading)
            {
                LoadStockReport();
            }
        }

        private void dtpAsOnDate_ValueChanged(object sender, EventArgs e)
        {
            if (!_isLoading)
            {
                LoadStockReport();
            }
        }

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
            if (displayData == null || displayData.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Excel Files (*.csv)|*.csv|All Files (*.*)|*.*";
                saveDialog.DefaultExt = "csv";
                saveDialog.FileName = $"GodownStockReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        this.Cursor = Cursors.WaitCursor;

                        StringBuilder sb = new StringBuilder();

                        // Add report header
                        sb.AppendLine($"Godown Stock Report - As on {dtpAsOnDate.Value:dd-MM-yyyy}");
                        sb.AppendLine($"Generated on: {DateTime.Now:dd-MM-yyyy HH:mm:ss}");
                        sb.AppendLine();

                        // Add column headers
                        sb.AppendLine("Godown Name,Item Name,Opening,Direct In,Transfer In,Total In,Direct Out,Transfer Out,Total Out,Current Stock");

                        // Add data rows
                        foreach (var row in displayData)
                        {
                            string godownName = EscapeCsvField(row.GodownName ?? "");
                            string itemName = EscapeCsvField(row.ItemName ?? "");

                            sb.AppendLine($"{godownName},{itemName},{row.OpeningStock:N2},{row.DirectInward:N2},{row.TransferIn:N2},{row.TotalInward:N2},{row.DirectOutward:N2},{row.TransferOut:N2},{row.TotalOutward:N2},{row.CurrentStock:N2}");
                        }

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
            if (displayData == null || displayData.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "HTML Files (*.html)|*.html|All Files (*.*)|*.*";
                saveDialog.DefaultExt = "html";
                saveDialog.FileName = $"GodownStockReport_{DateTime.Now:yyyyMMdd_HHmmss}.html";

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
                        html.AppendLine("<title>Godown Stock Report</title>");
                        html.AppendLine("<style>");
                        html.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; margin: 20px; }");
                        html.AppendLine("h1 { text-align: center; color: #333; margin-bottom: 5px; }");
                        html.AppendLine("h3 { text-align: center; color: #666; margin-top: 5px; }");
                        html.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; font-size: 11px; }");
                        html.AppendLine("th { background-color: #404040; color: white; padding: 8px 4px; text-align: center; border: 1px solid #333; }");
                        html.AppendLine("td { padding: 6px 4px; border: 1px solid #ddd; }");
                        html.AppendLine("td.number { text-align: right; }");
                        html.AppendLine("tr:nth-child(even) { background-color: #f9f9f9; }");
                        html.AppendLine(".subtotal { background-color: #c8c8c8 !important; font-weight: bold; }");
                        html.AppendLine(".grandtotal { background-color: #404040 !important; color: white; font-weight: bold; }");
                        html.AppendLine(".inward { background-color: #e6f0ff; }");
                        html.AppendLine(".transfer-in { background-color: #cce5ff; }");
                        html.AppendLine(".total-in { background-color: #b4dcff; font-weight: bold; }");
                        html.AppendLine(".outward { background-color: #fff0dc; }");
                        html.AppendLine(".transfer-out { background-color: #ffdcb4; }");
                        html.AppendLine(".total-out { background-color: #ffc896; font-weight: bold; }");
                        html.AppendLine(".current { background-color: #90ee90; font-weight: bold; }");
                        html.AppendLine("@media print { body { margin: 0; } table { font-size: 9px; } }");
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
                        html.AppendLine("<h1>Godown Stock Report</h1>");
                        html.AppendLine($"<h3>As on {dtpAsOnDate.Value:dd-MM-yyyy}</h3>");
                        html.AppendLine($"<p style=\"text-align: center; color: #999;\">Generated on: {DateTime.Now:dd-MM-yyyy HH:mm:ss}</p>");

                        // Table
                        html.AppendLine("<table>");
                        html.AppendLine("<thead><tr>");
                        html.AppendLine("<th>Godown Name</th>");
                        html.AppendLine("<th>Item Name</th>");
                        html.AppendLine("<th>Opening</th>");
                        html.AppendLine("<th class=\"inward\">Direct In</th>");
                        html.AppendLine("<th class=\"transfer-in\">Transfer In</th>");
                        html.AppendLine("<th class=\"total-in\">Total In</th>");
                        html.AppendLine("<th class=\"outward\">Direct Out</th>");
                        html.AppendLine("<th class=\"transfer-out\">Transfer Out</th>");
                        html.AppendLine("<th class=\"total-out\">Total Out</th>");
                        html.AppendLine("<th class=\"current\">Current Stock</th>");
                        html.AppendLine("</tr></thead>");
                        html.AppendLine("<tbody>");

                        // Data rows
                        foreach (var row in displayData)
                        {
                            string rowClass = "";
                            if (row.IsGrandTotal) rowClass = "grandtotal";
                            else if (row.IsSubtotal) rowClass = "subtotal";

                            html.AppendLine($"<tr class=\"{rowClass}\">");
                            html.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(row.GodownName ?? "")}</td>");
                            html.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(row.ItemName ?? "")}</td>");
                            html.AppendLine($"<td class=\"number\">{row.OpeningStock:N2}</td>");
                            html.AppendLine($"<td class=\"number inward\">{row.DirectInward:N2}</td>");
                            html.AppendLine($"<td class=\"number transfer-in\">{row.TransferIn:N2}</td>");
                            html.AppendLine($"<td class=\"number total-in\">{row.TotalInward:N2}</td>");
                            html.AppendLine($"<td class=\"number outward\">{row.DirectOutward:N2}</td>");
                            html.AppendLine($"<td class=\"number transfer-out\">{row.TransferOut:N2}</td>");
                            html.AppendLine($"<td class=\"number total-out\">{row.TotalOutward:N2}</td>");
                            html.AppendLine($"<td class=\"number current\">{row.CurrentStock:N2}</td>");
                            html.AppendLine("</tr>");
                        }

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

