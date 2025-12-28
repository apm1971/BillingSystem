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
    public partial class GodownTransactionControl : UserControl
    {
        private List<Godown> godowns = new List<Godown>();
        private List<GodownItem> items = new List<GodownItem>();
        private BindingList<GodownTransactionDetail> transactionDetails = new BindingList<GodownTransactionDetail>();

        public GodownTransactionControl()
        {
            InitializeComponent();
        }

        private void GodownTransactionControl_Load(object sender, EventArgs e)
        {
            LoadData();
            SetupDataGrid();
            UpdateGodownVisibility();
            SetupKeyboardShortcutsLabel();
            ClearForm();
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

                // Setup autocomplete for godown combo boxes
                cmbFromGodown.DropDownStyle = ComboBoxStyle.DropDown;
                cmbFromGodown.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                cmbFromGodown.AutoCompleteSource = AutoCompleteSource.ListItems;
                cmbFromGodown.DataSource = new List<Godown>(godowns);
                cmbFromGodown.DisplayMember = "GodownName";
                cmbFromGodown.ValueMember = "GodownID";
                cmbFromGodown.KeyDown += CmbGodown_KeyDown;

                cmbToGodown.DropDownStyle = ComboBoxStyle.DropDown;
                cmbToGodown.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                cmbToGodown.AutoCompleteSource = AutoCompleteSource.ListItems;
                cmbToGodown.DataSource = new List<Godown>(godowns);
                cmbToGodown.DisplayMember = "GodownName";
                cmbToGodown.ValueMember = "GodownID";
                cmbToGodown.KeyDown += CmbToGodown_KeyDown;

                cmbTransactionType.Items.Clear();
                cmbTransactionType.Items.AddRange(new[] { "Inward", "Outward", "Transfer" });
                cmbTransactionType.SelectedIndex = 0;
                cmbTransactionType.SelectedIndexChanged += CmbTransactionType_SelectedIndexChanged;
                cmbTransactionType.KeyDown += CmbTransactionType_KeyDown;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupDataGrid()
        {
            dgvTransactionDetails.AutoGenerateColumns = false;
            dgvTransactionDetails.AllowUserToAddRows = false;
            dgvTransactionDetails.RowHeadersVisible = false;
            dgvTransactionDetails.EditMode = DataGridViewEditMode.EditOnEnter;

            dgvTransactionDetails.Columns.Clear();

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
            dgvTransactionDetails.Columns.Add(itemColumn);

            dgvTransactionDetails.Columns.Add(new DataGridViewTextBoxColumn
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

            dgvTransactionDetails.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "GodownItemID",
                HeaderText = "ID",
                DataPropertyName = "GodownItemID",
                Visible = false
            });

            dgvTransactionDetails.CellValueChanged += DgvTransactionDetails_CellValueChanged;
            dgvTransactionDetails.DataError += DgvTransactionDetails_DataError;
            dgvTransactionDetails.KeyDown += DgvTransactionDetails_KeyDown;
            dgvTransactionDetails.EditingControlShowing += DgvTransactionDetails_EditingControlShowing;
            
            // Bind to BindingList - this allows add/remove without resetting DataSource
            dgvTransactionDetails.DataSource = transactionDetails;
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
                if (dgvTransactionDetails.Focused || dgvTransactionDetails.IsCurrentCellInEditMode)
                {
                    DeleteCurrentRow();
                    return true;
                }
            }
            
            // Escape = Clear form
            if (keyData == Keys.Escape)
            {
                ClearForm();
                return true;
            }
            
            // F2 = Focus on transaction type
            if (keyData == Keys.F2)
            {
                cmbTransactionType.Focus();
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

        private void CmbTransactionType_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                string transactionType = cmbTransactionType.SelectedItem?.ToString() ?? "";
                if (transactionType == "Transfer")
                {
                    cmbFromGodown.Focus();
                }
                else
                {
                    cmbToGodown.Focus();
                }
            }
        }

        private void CmbGodown_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                cmbToGodown.Focus();
            }
            else if (e.KeyCode == Keys.Down && sender is ComboBox cmb && !cmb.DroppedDown)
            {
                cmb.DroppedDown = true;
                e.SuppressKeyPress = true;
            }
        }

        private void CmbToGodown_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                FocusOnGrid();
            }
            else if (e.KeyCode == Keys.Down && !cmbToGodown.DroppedDown)
            {
                cmbToGodown.DroppedDown = true;
                e.SuppressKeyPress = true;
            }
        }

        private void FocusOnGrid()
        {
            dgvTransactionDetails.Focus();
            if (dgvTransactionDetails.Rows.Count > 0)
            {
                dgvTransactionDetails.CurrentCell = dgvTransactionDetails.Rows[0].Cells["ItemName"];
                dgvTransactionDetails.BeginEdit(true);
            }
            else
            {
                // Auto-add a row if grid is empty
                AddNewRow();
            }
        }

        private void DgvTransactionDetails_EditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e)
        {
            // Remove previous handlers to avoid duplicates
            e.Control.KeyDown -= EditingControl_KeyDown;
            e.Control.KeyDown += EditingControl_KeyDown;
            
            // Configure auto-complete for item ComboBox
            if (e.Control is ComboBox comboBox && dgvTransactionDetails.CurrentCell?.OwningColumn?.Name == "ItemName")
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
                
                var currentCell = dgvTransactionDetails.CurrentCell;
                if (currentCell == null) return;
                
                // Capture state BEFORE EndEdit
                var currentRow = currentCell.RowIndex;
                var columnName = dgvTransactionDetails.Columns[currentCell.ColumnIndex].Name;
                var totalRows = dgvTransactionDetails.Rows.Count;
                var isLastRow = (currentRow == totalRows - 1);
                var isQuantityColumn = (columnName == "Quantity");
                
                dgvTransactionDetails.EndEdit();
                
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
                            if (currentRow + 1 < dgvTransactionDetails.Rows.Count)
                            {
                                dgvTransactionDetails.CurrentCell = dgvTransactionDetails.Rows[currentRow + 1].Cells["ItemName"];
                                dgvTransactionDetails.BeginEdit(true);
                            }
                        }));
                    }
                }
                else
                {
                    // Move to Quantity column
                    this.BeginInvoke((Action)(() => {
                        if (currentRow < dgvTransactionDetails.Rows.Count)
                        {
                            dgvTransactionDetails.CurrentCell = dgvTransactionDetails.Rows[currentRow].Cells["Quantity"];
                            dgvTransactionDetails.BeginEdit(true);
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

        private void DgvTransactionDetails_KeyDown(object? sender, KeyEventArgs e)
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
                else if (e.KeyCode == Keys.Enter && !dgvTransactionDetails.IsCurrentCellInEditMode)
                {
                    e.SuppressKeyPress = true;
                    if (dgvTransactionDetails.CurrentCell != null)
                    {
                        dgvTransactionDetails.BeginEdit(true);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DgvTransactionDetails_KeyDown: {ex.Message}");
            }
        }

        private void DeleteCurrentRow()
        {
            try
            {
                int index = -1;
                
                if (dgvTransactionDetails.SelectedRows.Count > 0)
                {
                    index = dgvTransactionDetails.SelectedRows[0].Index;
                }
                else if (dgvTransactionDetails.CurrentRow != null)
                {
                    index = dgvTransactionDetails.CurrentRow.Index;
                }
                
                if (index >= 0 && index < transactionDetails.Count)
                {
                    transactionDetails.RemoveAt(index);
                    
                    // Set focus to same row or last row
                    if (transactionDetails.Count > 0)
                    {
                        int newIndex = Math.Min(index, transactionDetails.Count - 1);
                        this.BeginInvoke((Action)(() => {
                            dgvTransactionDetails.CurrentCell = dgvTransactionDetails.Rows[newIndex].Cells["ItemName"];
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
            if (items.Count == 0)
            {
                MessageBox.Show("No items available. Please add items first.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Make sure ComboBox column has items
            if (dgvTransactionDetails.Columns["ItemName"] is DataGridViewComboBoxColumn itemColumn)
            {
                if (itemColumn.DataSource == null || ((List<string>)itemColumn.DataSource).Count == 0)
                {
                    itemColumn.DataSource = items.Select(i => i.ItemName).Distinct().ToList();
                }
            }

            // Add a new row with the first item selected - BindingList will auto-update the grid
            var newDetail = new GodownTransactionDetail
            {
                GodownItemID = items[0].GodownItemID,
                ItemName = items[0].ItemName,
                Quantity = 0
            };

            transactionDetails.Add(newDetail);

            // Focus on the new row's item cell
            this.BeginInvoke((Action)(() => {
                if (dgvTransactionDetails.Rows.Count > 0)
                {
                    dgvTransactionDetails.CurrentCell = dgvTransactionDetails.Rows[dgvTransactionDetails.Rows.Count - 1].Cells["ItemName"];
                    dgvTransactionDetails.BeginEdit(true);
                }
            }));
        }

        #endregion

        private void DgvTransactionDetails_DataError(object? sender, DataGridViewDataErrorEventArgs e)
        {
            // Suppress the error dialog - this can happen when ComboBox values don't match
            e.ThrowException = false;

            if (e.Exception != null)
            {
                System.Diagnostics.Debug.WriteLine($"DataGridView DataError: {e.Exception.Message}");
            }
        }

        private void CmbTransactionType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateGodownVisibility();
        }

        private void UpdateGodownVisibility()
        {
            string? transactionType = cmbTransactionType.SelectedItem?.ToString();
            
            lblFromGodown.Visible = transactionType == "Transfer";
            cmbFromGodown.Visible = transactionType == "Transfer";
            
            lblToGodown.Visible = true;
            cmbToGodown.Visible = true;
            
            if (transactionType == "Inward")
            {
                lblToGodown.Text = "To Godown:";
            }
            else if (transactionType == "Outward")
            {
                lblToGodown.Text = "From Godown:";
            }
            else if (transactionType == "Transfer")
            {
                lblToGodown.Text = "To Godown:";
            }
        }

        private void DgvTransactionDetails_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && e.RowIndex < dgvTransactionDetails.Rows.Count)
            {
                if (dgvTransactionDetails.Columns[e.ColumnIndex].Name == "ItemName")
                {
                    string? itemName = dgvTransactionDetails.Rows[e.RowIndex].Cells["ItemName"].Value?.ToString();
                    if (!string.IsNullOrEmpty(itemName) && e.RowIndex < transactionDetails.Count)
                    {
                        var item = items.FirstOrDefault(i => i.ItemName == itemName);
                        if (item != null)
                        {
                            transactionDetails[e.RowIndex].GodownItemID = item.GodownItemID;
                            transactionDetails[e.RowIndex].ItemName = item.ItemName;
                        }
                    }
                }
            }
        }

        private void btnAddRow_Click(object sender, EventArgs e)
        {
            AddNewRow();
        }

        private void btnRemoveRow_Click(object sender, EventArgs e)
        {
            DeleteCurrentRow();
        }

        private void ClearForm()
        {
            txtTransactionNo.Text = "";
            dtpTransactionDate.Value = DateTime.Now;
            if (cmbTransactionType.Items.Count > 0)
            {
                cmbTransactionType.SelectedIndex = 0;
            }
            txtReferenceNo.Text = "";
            
            // Clear the BindingList
            transactionDetails.Clear();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                dgvTransactionDetails.EndEdit();
                
                var transaction = new GodownTransaction
                {
                    TransactionNo = string.IsNullOrWhiteSpace(txtTransactionNo.Text) 
                        ? GodownTransactionService.GenerateTransactionNo(cmbTransactionType.SelectedItem?.ToString() ?? "")
                        : txtTransactionNo.Text,
                    TransactionDate = dtpTransactionDate.Value,
                    TransactionType = cmbTransactionType.SelectedItem?.ToString() ?? "",
                    ReferenceNo = txtReferenceNo.Text,
                    CreatedDate = DateTime.Now,
                    CreatedBy = Program.CurrentUser?.UserID
                };

                string transactionType = transaction.TransactionType;
                
                if (transactionType == "Inward")
                {
                    transaction.ToGodownID = (int?)cmbToGodown.SelectedValue;
                }
                else if (transactionType == "Outward")
                {
                    transaction.ToGodownID = (int?)cmbToGodown.SelectedValue;
                }
                else if (transactionType == "Transfer")
                {
                    transaction.FromGodownID = (int?)cmbFromGodown.SelectedValue;
                    transaction.ToGodownID = (int?)cmbToGodown.SelectedValue;
                }

                // Get details from BindingList
                transaction.Details = transactionDetails
                    .Where(d => d.GodownItemID > 0 && !string.IsNullOrEmpty(d.ItemName) && d.Quantity > 0)
                    .ToList();
                transaction.TotalQuantity = transaction.Details.Sum(d => d.Quantity);

                if (transaction.Details.Count == 0)
                {
                    MessageBox.Show("Please add at least one item with quantity greater than zero", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool success = GodownTransactionService.CreateTransaction(transaction);

                if (success)
                {
                    MessageBox.Show("Transaction saved successfully", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving transaction: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateForm()
        {
            if (cmbTransactionType.SelectedItem == null)
            {
                MessageBox.Show("Please select transaction type", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            string transactionType = cmbTransactionType.SelectedItem.ToString() ?? "";

            if (transactionType == "Transfer")
            {
                if (cmbFromGodown.SelectedValue == null || cmbToGodown.SelectedValue == null)
                {
                    MessageBox.Show("Please select both from and to godowns", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (cmbFromGodown.SelectedValue.Equals(cmbToGodown.SelectedValue))
                {
                    MessageBox.Show("From and To godowns cannot be the same", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            else
            {
                if (cmbToGodown.SelectedValue == null)
                {
                    MessageBox.Show("Please select godown", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            if (transactionDetails.Count == 0 || !transactionDetails.Any(d => d.GodownItemID > 0 && d.Quantity > 0))
            {
                MessageBox.Show("Please add at least one item with quantity", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }
    }
}
