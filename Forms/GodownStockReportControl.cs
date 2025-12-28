using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class GodownStockReportControl : UserControl
    {
        private List<Godown> godowns = new List<Godown>();
        private List<GodownItem> items = new List<GodownItem>();
        private List<GodownStockViewModel> stockData = new List<GodownStockViewModel>();
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
                
                dgvStockReport.DataSource = null;
                dgvStockReport.DataSource = stockData;

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
    }
}

