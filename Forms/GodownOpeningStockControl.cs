using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class GodownOpeningStockControl : UserControl
    {
        private List<Godown> godowns = new List<Godown>();
        private List<GodownItem> items = new List<GodownItem>();
        private BindingList<GodownOpeningStock> currentStocks = new BindingList<GodownOpeningStock>();
        private int selectedGodownID = 0;

        public GodownOpeningStockControl()
        {
            InitializeComponent();
        }

        private void GodownOpeningStockControl_Load(object sender, EventArgs e)
        {
            LoadData();
            SetupDataGrid();
            SetupKeyboardShortcutsLabel();
        }

        private void SetupKeyboardShortcutsLabel()
        {
            // Add shortcuts info label if not present
            var lblShortcuts = new Label
            {
                Text = "Shortcuts: Ctrl+S = Save | F8 = Delete Row | Enter = Next Cell/Add Row | Down = Open Dropdown",
                Dock = DockStyle.Bottom,
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 8f),
                Height = 20,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0)
            };
            this.Controls.Add(lblShortcuts);
        }

        private void LoadData()
        {
            try
            {
                godowns = GodownService.GetAllGodowns();
                items = GodownItemService.GetAllGodownItems();

                // Setup autocomplete for godown combo box
                cmbGodown.DropDownStyle = ComboBoxStyle.DropDown;
                cmbGodown.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                cmbGodown.AutoCompleteSource = AutoCompleteSource.ListItems;
                cmbGodown.DataSource = new List<Godown>(godowns);
                cmbGodown.DisplayMember = "GodownName";
                cmbGodown.ValueMember = "GodownID";
                cmbGodown.SelectedIndexChanged += CmbGodown_SelectedIndexChanged;
                cmbGodown.KeyDown += CmbGodown_KeyDown;

                // Update ComboBox column DataSource if grid is already set up
                if (dgvOpeningStock.Columns["ItemName"] is DataGridViewComboBoxColumn itemColumn && items.Count > 0)
                {
                    var itemNames = items.Select(i => i.ItemName).Distinct().ToList();
                    itemColumn.DataSource = itemNames;
                }

                if (godowns.Count > 0)
                {
                    cmbGodown.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupDataGrid()
        {
            dgvOpeningStock.AutoGenerateColumns = false;
            dgvOpeningStock.AllowUserToAddRows = false;
            dgvOpeningStock.RowHeadersVisible = false;
            dgvOpeningStock.EditMode = DataGridViewEditMode.EditOnEnter;

            dgvOpeningStock.Columns.Clear();

            var itemColumn = new DataGridViewComboBoxColumn
            {
                Name = "ItemName",
                HeaderText = "Item Name",
                DataPropertyName = "ItemName",
                Width = 300,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton
            };
            
            // Set DataSource with items
            if (items.Count > 0)
            {
                itemColumn.DataSource = items.Select(i => i.ItemName).Distinct().ToList();
            }
            else
            {
                itemColumn.DataSource = new List<string>();
            }
            dgvOpeningStock.Columns.Add(itemColumn);

            dgvOpeningStock.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Quantity",
                HeaderText = "Quantity",
                DataPropertyName = "Quantity",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            dgvOpeningStock.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "GodownItemID",
                HeaderText = "ID",
                DataPropertyName = "GodownItemID",
                Visible = false
            });

            dgvOpeningStock.CellValueChanged += DgvOpeningStock_CellValueChanged;
            dgvOpeningStock.DataError += DgvOpeningStock_DataError;
            dgvOpeningStock.KeyDown += DgvOpeningStock_KeyDown;
            dgvOpeningStock.EditingControlShowing += DgvOpeningStock_EditingControlShowing;
            
            // Bind to BindingList - this allows add/remove without resetting DataSource
            dgvOpeningStock.DataSource = currentStocks;
        }

        #region Keyboard Navigation

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Ctrl+S = Save
            if (keyData == (Keys.Control | Keys.S))
            {
                btnSave_Click(this, EventArgs.Empty);
                return true;
            }
            
            // F8 = Delete current row
            if (keyData == Keys.F8)
            {
                if (dgvOpeningStock.Focused || dgvOpeningStock.IsCurrentCellInEditMode)
                {
                    DeleteCurrentRow();
                    return true;
                }
            }
            
            // Escape = Clear form
            if (keyData == Keys.Escape)
            {
                LoadOpeningStock();
                return true;
            }
            
            // F2 = Focus on godown
            if (keyData == Keys.F2)
            {
                cmbGodown.Focus();
                return true;
            }
            
            // F3 = Focus on grid
            if (keyData == Keys.F3)
            {
                FocusOnGrid();
                return true;
            }
            
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void CmbGodown_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                FocusOnGrid();
            }
            else if (e.KeyCode == Keys.Down && !cmbGodown.DroppedDown)
            {
                cmbGodown.DroppedDown = true;
                e.SuppressKeyPress = true;
            }
        }

        private void FocusOnGrid()
        {
            dgvOpeningStock.Focus();
            if (dgvOpeningStock.Rows.Count > 0)
            {
                dgvOpeningStock.CurrentCell = dgvOpeningStock.Rows[0].Cells["ItemName"];
                dgvOpeningStock.BeginEdit(true);
            }
            else
            {
                // Auto-add a row if grid is empty
                AddNewRow();
            }
        }

        private void DgvOpeningStock_EditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e)
        {
            // Remove previous handlers to avoid duplicates
            e.Control.KeyDown -= EditingControl_KeyDown;
            e.Control.KeyDown += EditingControl_KeyDown;
            
            // Configure auto-complete for item ComboBox
            if (e.Control is ComboBox comboBox && dgvOpeningStock.CurrentCell?.OwningColumn?.Name == "ItemName")
            {
                comboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                comboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
                comboBox.DropDownStyle = ComboBoxStyle.DropDown;
            }
        }

        private void EditingControl_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                
                var currentCell = dgvOpeningStock.CurrentCell;
                if (currentCell == null) return;
                
                // Capture state BEFORE EndEdit
                var currentRow = currentCell.RowIndex;
                var columnName = dgvOpeningStock.Columns[currentCell.ColumnIndex].Name;
                var totalRows = dgvOpeningStock.Rows.Count;
                var isLastRow = (currentRow == totalRows - 1);
                var isQuantityColumn = (columnName == "Quantity");
                
                dgvOpeningStock.EndEdit();
                
                // If at Quantity column, add new row or move to next row
                if (isQuantityColumn)
                {
                    if (isLastRow)
                    {
                        // Use BeginInvoke to ensure AddNewRow happens after EndEdit completes
                        this.BeginInvoke((Action)(() => {
                            AddNewRow();
                        }));
                    }
                    else
                    {
                        this.BeginInvoke((Action)(() => {
                            if (currentRow + 1 < dgvOpeningStock.Rows.Count)
                            {
                                dgvOpeningStock.CurrentCell = dgvOpeningStock.Rows[currentRow + 1].Cells["ItemName"];
                                dgvOpeningStock.BeginEdit(true);
                            }
                        }));
                    }
                }
                else
                {
                    // Move to Quantity column
                    this.BeginInvoke((Action)(() => {
                        if (currentRow < dgvOpeningStock.Rows.Count)
                        {
                            dgvOpeningStock.CurrentCell = dgvOpeningStock.Rows[currentRow].Cells["Quantity"];
                            dgvOpeningStock.BeginEdit(true);
                        }
                    }));
                }
            }
            else if (e.KeyCode == Keys.Down && sender is ComboBox comboBox && !comboBox.DroppedDown)
            {
                comboBox.DroppedDown = true;
                e.SuppressKeyPress = true;
            }
        }

        private void DgvOpeningStock_KeyDown(object? sender, KeyEventArgs e)
        {
            try
            {
                // Delete key - remove selected row
                if (e.KeyCode == Keys.Delete)
                {
                    DeleteCurrentRow();
                    e.Handled = true;
                }
                // Enter key when not in edit mode - start editing
                else if (e.KeyCode == Keys.Enter && !dgvOpeningStock.IsCurrentCellInEditMode)
                {
                    e.SuppressKeyPress = true;
                    if (dgvOpeningStock.CurrentCell != null)
                    {
                        dgvOpeningStock.BeginEdit(true);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DgvOpeningStock_KeyDown: {ex.Message}");
            }
        }

        private void DeleteCurrentRow()
        {
            try
            {
                int index = -1;
                
                if (dgvOpeningStock.SelectedRows.Count > 0)
                {
                    index = dgvOpeningStock.SelectedRows[0].Index;
                }
                else if (dgvOpeningStock.CurrentRow != null)
                {
                    index = dgvOpeningStock.CurrentRow.Index;
                }
                
                if (index >= 0 && index < currentStocks.Count)
                {
                    currentStocks.RemoveAt(index);
                    
                    // Set focus to same row or last row
                    if (currentStocks.Count > 0)
                    {
                        int newIndex = Math.Min(index, currentStocks.Count - 1);
                        this.BeginInvoke((Action)(() => {
                            dgvOpeningStock.CurrentCell = dgvOpeningStock.Rows[newIndex].Cells["ItemName"];
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting row: {ex.Message}");
            }
        }

        private void AddNewRow()
        {
            if (selectedGodownID == 0)
            {
                MessageBox.Show("Please select a godown first", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbGodown.Focus();
                return;
            }

            if (items.Count == 0)
            {
                MessageBox.Show("No items available. Please add items first.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Make sure ComboBox column has items
            if (dgvOpeningStock.Columns["ItemName"] is DataGridViewComboBoxColumn itemColumn)
            {
                if (itemColumn.DataSource == null || ((List<string>)itemColumn.DataSource).Count == 0)
                {
                    itemColumn.DataSource = items.Select(i => i.ItemName).Distinct().ToList();
                }
            }

            // Add new row - BindingList auto-updates the grid
            currentStocks.Add(new GodownOpeningStock
            {
                GodownID = selectedGodownID,
                GodownItemID = items[0].GodownItemID,
                ItemName = items[0].ItemName,
                Quantity = 0,
                AsOnDate = dtpAsOnDate.Value
            });

            // Focus on the new row's item cell
            this.BeginInvoke((Action)(() => {
                if (dgvOpeningStock.Rows.Count > 0)
                {
                    dgvOpeningStock.CurrentCell = dgvOpeningStock.Rows[dgvOpeningStock.Rows.Count - 1].Cells["ItemName"];
                    dgvOpeningStock.BeginEdit(true);
                }
            }));
        }

        #endregion

        private void CmbGodown_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbGodown.SelectedValue != null && cmbGodown.SelectedValue is int)
            {
                selectedGodownID = (int)cmbGodown.SelectedValue;
                LoadOpeningStock();
            }
        }

        private void LoadOpeningStock()
        {
            try
            {
                var stocks = GodownOpeningStockService.GetOpeningStockByGodown(selectedGodownID);
                
                // Populate item combo box
                if (dgvOpeningStock.Columns["ItemName"] is DataGridViewComboBoxColumn itemColumn)
                {
                    itemColumn.DataSource = items.Select(i => i.ItemName).ToList();
                }

                // Clear and add items to BindingList (no DataSource reset needed)
                currentStocks.Clear();
                foreach (var stock in stocks)
                {
                    if (stock.Quantity > 0)
                    {
                        currentStocks.Add(stock);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading opening stock: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvOpeningStock_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && e.RowIndex < dgvOpeningStock.Rows.Count)
            {
                if (dgvOpeningStock.Columns[e.ColumnIndex].Name == "ItemName")
                {
                    string? itemName = dgvOpeningStock.Rows[e.RowIndex].Cells["ItemName"].Value?.ToString();
                    if (!string.IsNullOrEmpty(itemName) && e.RowIndex < currentStocks.Count)
                    {
                        var item = items.FirstOrDefault(i => i.ItemName == itemName);
                        if (item != null)
                        {
                            currentStocks[e.RowIndex].GodownItemID = item.GodownItemID;
                            currentStocks[e.RowIndex].ItemName = item.ItemName;
                        }
                    }
                }
            }
        }

        private void DgvOpeningStock_DataError(object? sender, DataGridViewDataErrorEventArgs e)
        {
            // Suppress the error dialog - this can happen when ComboBox values don't match
            e.ThrowException = false;
            
            if (e.Exception != null)
            {
                System.Diagnostics.Debug.WriteLine($"DataGridView DataError: {e.Exception.Message}");
            }
        }

        private void btnAddRow_Click(object sender, EventArgs e)
        {
            AddNewRow();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (selectedGodownID == 0)
            {
                MessageBox.Show("Please select a godown", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                dgvOpeningStock.EndEdit();
                
                var stocksToSave = currentStocks
                    .Where(s => s.Quantity > 0 && s.GodownItemID > 0)
                    .ToList();

                foreach (var stock in stocksToSave)
                {
                    stock.AsOnDate = dtpAsOnDate.Value;
                }

                bool success = GodownOpeningStockService.SaveOpeningStockBatch(
                    selectedGodownID, 
                    stocksToSave, 
                    dtpAsOnDate.Value
                );

                if (success)
                {
                    MessageBox.Show("Opening stock saved successfully", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadOpeningStock();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving opening stock: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            LoadOpeningStock();
        }
    }
}
