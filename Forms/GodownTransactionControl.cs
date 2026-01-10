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
                Text = "Shortcuts: Ctrl+S = Save | F8 = Delete Row | F4 = Add Godown | F5 = Add Item | Enter = Next Cell/Add Row",
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
            
            // F4 = Quick Add New Godown
            if (keyData == Keys.F4)
            {
                OpenQuickAddGodown();
                return true;
            }
            
            // F5 = Quick Add New Item
            if (keyData == Keys.F5)
            {
                OpenQuickAddItem();
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

        private void btnGenerateReport_Click(object sender, EventArgs e)
        {
            ShowTransactionReport();
        }

        private void ShowTransactionReport()
        {
            if (transactionDetails.Count == 0 || !transactionDetails.Any(d => d.GodownItemID > 0 && d.Quantity > 0))
            {
                MessageBox.Show("No transaction items to generate report.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var reportForm = new Form())
            {
                reportForm.Text = "Transaction Report";
                reportForm.Size = new Size(700, 600);
                reportForm.StartPosition = FormStartPosition.CenterParent;
                reportForm.FormBorderStyle = FormBorderStyle.Sizable;
                reportForm.MinimizeBox = false;
                reportForm.MaximizeBox = true;
                reportForm.KeyPreview = true;

                // Top panel with Print button
                var topPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 50,
                    BackColor = Color.WhiteSmoke,
                    Padding = new Padding(10)
                };

                var btnPrint = new Button
                {
                    Text = "🖨️ Print",
                    Location = new Point(10, 10),
                    Size = new Size(100, 30),
                    BackColor = Color.LightBlue,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat
                };

                var btnClose = new Button
                {
                    Text = "Close",
                    Location = new Point(120, 10),
                    Size = new Size(80, 30),
                    BackColor = Color.LightGray,
                    Font = new Font("Segoe UI", 10F),
                    FlatStyle = FlatStyle.Flat
                };

                topPanel.Controls.AddRange(new Control[] { btnPrint, btnClose });

                // Report content panel
                var reportPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    BackColor = Color.White,
                    Padding = new Padding(20)
                };

                // Create report content
                var reportContent = new RichTextBox
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    BorderStyle = BorderStyle.None,
                    BackColor = Color.White,
                    Font = new Font("Segoe UI", 10F)
                };

                // Build report text
                string transactionType = cmbTransactionType.SelectedItem?.ToString() ?? "";
                string godownInfo = "";
                
                if (transactionType == "Transfer")
                {
                    string fromGodown = (cmbFromGodown.SelectedItem as Godown)?.GodownName ?? "";
                    string toGodown = (cmbToGodown.SelectedItem as Godown)?.GodownName ?? "";
                    godownInfo = $"From Godown: {fromGodown}\nTo Godown: {toGodown}";
                }
                else if (transactionType == "Inward")
                {
                    string toGodown = (cmbToGodown.SelectedItem as Godown)?.GodownName ?? "";
                    godownInfo = $"To Godown: {toGodown}";
                }
                else if (transactionType == "Outward")
                {
                    string fromGodown = (cmbToGodown.SelectedItem as Godown)?.GodownName ?? "";
                    godownInfo = $"From Godown: {fromGodown}";
                }

                string reportText = $@"
══════════════════════════════════════════════════════════
                    GODOWN TRANSACTION REPORT
══════════════════════════════════════════════════════════

Transaction Type: {transactionType}
Transaction Date: {dtpTransactionDate.Value:dd-MMM-yyyy}
Transaction No: {(string.IsNullOrWhiteSpace(txtTransactionNo.Text) ? "(Auto-generated)" : txtTransactionNo.Text)}
Reference No: {txtReferenceNo.Text}
{godownInfo}

──────────────────────────────────────────────────────────
ITEM DETAILS
──────────────────────────────────────────────────────────

";

                int sno = 1;
                double totalQty = 0;
                foreach (var detail in transactionDetails.Where(d => d.GodownItemID > 0 && d.Quantity > 0))
                {
                    reportText += $"{sno,3}. {detail.ItemName,-40} Qty: {detail.Quantity:N2}\n";
                    totalQty += detail.Quantity;
                    sno++;
                }

                reportText += $@"
──────────────────────────────────────────────────────────
TOTAL ITEMS: {sno - 1}                    TOTAL QUANTITY: {totalQty:N2}
══════════════════════════════════════════════════════════

Generated on: {DateTime.Now:dd-MMM-yyyy hh:mm:ss tt}
";

                reportContent.Text = reportText;
                reportPanel.Controls.Add(reportContent);

                reportForm.Controls.Add(reportPanel);
                reportForm.Controls.Add(topPanel);

                // Print button click
                btnPrint.Click += (s, ev) =>
                {
                    try
                    {
                        var printDoc = new System.Drawing.Printing.PrintDocument();
                        printDoc.PrintPage += (sender, args) =>
                        {
                            args.Graphics.DrawString(reportContent.Text, 
                                new Font("Consolas", 10F), 
                                Brushes.Black, 
                                new RectangleF(50, 50, args.PageBounds.Width - 100, args.PageBounds.Height - 100));
                        };

                        using (var printDialog = new PrintDialog())
                        {
                            printDialog.Document = printDoc;
                            if (printDialog.ShowDialog() == DialogResult.OK)
                            {
                                printDoc.Print();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error printing: {ex.Message}", "Print Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                btnClose.Click += (s, ev) => reportForm.Close();

                reportForm.KeyDown += (s, ev) =>
                {
                    if (ev.KeyCode == Keys.Escape)
                    {
                        reportForm.Close();
                    }
                    else if (ev.Control && ev.KeyCode == Keys.P)
                    {
                        btnPrint.PerformClick();
                        ev.SuppressKeyPress = true;
                    }
                };

                reportForm.ShowDialog();
            }
        }

        #region Quick Add Dialogs

        private void OpenQuickAddGodown()
        {
            using (var form = new Form())
            {
                form.Text = "Quick Add Godown (F4)";
                form.Size = new Size(400, 200);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.KeyPreview = true;

                // Create simple input form
                var lblName = new Label { Text = "Godown Name:", Location = new Point(20, 25), AutoSize = true };
                var txtName = new TextBox { Location = new Point(150, 22), Size = new Size(220, 25) };

                var lblShortName = new Label { Text = "Short Name:", Location = new Point(20, 60), AutoSize = true };
                var txtShortName = new TextBox { Location = new Point(150, 57), Size = new Size(220, 25) };

                var btnSave = new Button 
                { 
                    Text = "Save (Ctrl+S)", 
                    Location = new Point(150, 100), 
                    Size = new Size(100, 35),
                    BackColor = Color.LightGreen
                };
                var btnCancel = new Button 
                { 
                    Text = "Cancel (Esc)", 
                    Location = new Point(260, 100), 
                    Size = new Size(100, 35),
                    BackColor = Color.LightCoral
                };

                form.Controls.AddRange(new Control[] { lblName, txtName, lblShortName, txtShortName, btnSave, btnCancel });

                btnSave.Click += (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(txtName.Text))
                    {
                        MessageBox.Show("Please enter a godown name", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtName.Focus();
                        return;
                    }

                    try
                    {
                        var newGodown = new Godown
                        {
                            GodownName = txtName.Text.Trim(),
                            GodownShortName = txtShortName.Text.Trim()
                        };

                        bool success = GodownService.AddGodown(newGodown);
                        if (success)
                        {
                            MessageBox.Show("Godown added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            form.DialogResult = DialogResult.OK;
                            form.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error adding godown: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                btnCancel.Click += (s, e) => form.Close();

                // Handle keyboard shortcuts
                form.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Escape)
                    {
                        form.Close();
                    }
                    else if (e.Control && e.KeyCode == Keys.S)
                    {
                        btnSave.PerformClick();
                        e.SuppressKeyPress = true;
                    }
                };

                txtName.Focus();
                form.ShowDialog();

                // Refresh godown list after closing
                RefreshGodownList();
            }
        }

        private void OpenQuickAddItem()
        {
            using (var form = new Form())
            {
                form.Text = "Quick Add Item (F5)";
                form.Size = new Size(400, 170);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.KeyPreview = true;

                // Create simple input form
                var lblName = new Label { Text = "Item Name:", Location = new Point(20, 25), AutoSize = true };
                var txtName = new TextBox { Location = new Point(120, 22), Size = new Size(250, 25) };

                var btnSave = new Button 
                { 
                    Text = "Save (Ctrl+S)", 
                    Location = new Point(120, 65), 
                    Size = new Size(100, 35),
                    BackColor = Color.LightGreen
                };
                var btnCancel = new Button 
                { 
                    Text = "Cancel (Esc)", 
                    Location = new Point(230, 65), 
                    Size = new Size(100, 35),
                    BackColor = Color.LightCoral
                };

                form.Controls.AddRange(new Control[] { lblName, txtName, btnSave, btnCancel });

                btnSave.Click += (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(txtName.Text))
                    {
                        MessageBox.Show("Please enter an item name", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtName.Focus();
                        return;
                    }

                    try
                    {
                        var newItem = new GodownItem
                        {
                            ItemName = txtName.Text.Trim()
                        };

                        bool success = GodownItemService.AddGodownItem(newItem);
                        if (success)
                        {
                            MessageBox.Show("Item added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            form.DialogResult = DialogResult.OK;
                            form.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error adding item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                btnCancel.Click += (s, e) => form.Close();

                // Handle keyboard shortcuts
                form.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Escape)
                    {
                        form.Close();
                    }
                    else if (e.Control && e.KeyCode == Keys.S)
                    {
                        btnSave.PerformClick();
                        e.SuppressKeyPress = true;
                    }
                };

                txtName.Focus();
                form.ShowDialog();

                // Refresh item list after closing
                RefreshItemList();
            }
        }

        private void RefreshGodownList()
        {
            try
            {
                godowns = GodownService.GetAllGodowns();
                
                // Refresh From Godown combo
                var selectedFromGodown = cmbFromGodown.SelectedValue;
                cmbFromGodown.DataSource = null;
                cmbFromGodown.DataSource = new List<Godown>(godowns);
                cmbFromGodown.DisplayMember = "GodownName";
                cmbFromGodown.ValueMember = "GodownID";
                if (selectedFromGodown != null)
                {
                    cmbFromGodown.SelectedValue = selectedFromGodown;
                }

                // Refresh To Godown combo
                var selectedToGodown = cmbToGodown.SelectedValue;
                cmbToGodown.DataSource = null;
                cmbToGodown.DataSource = new List<Godown>(godowns);
                cmbToGodown.DisplayMember = "GodownName";
                cmbToGodown.ValueMember = "GodownID";
                if (selectedToGodown != null)
                {
                    cmbToGodown.SelectedValue = selectedToGodown;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing godown list: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshItemList()
        {
            try
            {
                items = GodownItemService.GetAllGodownItems();

                // Update ComboBox column DataSource
                if (dgvTransactionDetails.Columns["ItemName"] is DataGridViewComboBoxColumn itemColumn && items.Count > 0)
                {
                    var itemNames = items.Select(i => i.ItemName).Distinct().ToList();
                    itemColumn.DataSource = itemNames;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing item list: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion
    }
}
