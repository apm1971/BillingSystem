using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class SaleBillUserControl : UserControl
    {
        public event EventHandler<BillSavedEventArgs>? BillSaved;
        public event EventHandler? CloseRequested;

        private Bill _currentBill;
        private List<Party> _allParties = new List<Party>();
        private List<Item> _allItems = new List<Item>();
        private List<Broker> _allBrokers = new List<Broker>();
        private bool _isEditMode;
        


        public SaleBillUserControl(Bill? bill = null)
        {
            InitializeComponent();
            _isEditMode = bill != null;
            _currentBill = bill ?? new Bill();
        }

        private void SaleBillUserControl_Load(object? sender, EventArgs e)
        {
            LoadData();
            SetupForm();
        }

        #region Initial Setup

        private void LoadData()
        {
            try
            {
                _allParties = PartyService.GetAllParties();
                _allItems = ItemService.GetAllItems();
                _allBrokers = BrokerService.GetAllBrokers();
                
                // Populate the ComboBox after loading items
                PopulateItemComboBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupForm()
        {
            txtBillNo.ReadOnly = true;
            txtBillNo.BackColor = Color.LightGray;
            
            SetupDataGridView();
            SetupEventHandlers();
            
            // Populate the Item ComboBox
            PopulateItemComboBox();
            
            // Setup Party and Broker ComboBoxes
            SetupPartyComboBox();
            SetupBrokerComboBox();
            
            // Show shortcuts info
            if (lblShortcutsInfo != null)
            {
                lblShortcutsInfo.Visible = true;
            }
            
            if (_isEditMode)
            {
                LoadBillData();
            }
            else
            {
                ClearForm();
            }
        }

        private void SetupDataGridView()
        {
            dgvItems.AutoGenerateColumns = false;
            dgvItems.AllowUserToAddRows = false; 
            dgvItems.RowHeadersVisible = false;
            dgvItems.EditMode = DataGridViewEditMode.EditOnEnter;

            // Define Columns
            dgvItems.Columns.Clear();
            
            // Item column as ComboBox with auto-complete
            var itemColumn = new DataGridViewComboBoxColumn 
            { 
                Name = "ItemName", 
                HeaderText = "Item", 
                DataPropertyName = "ItemName", 
                Width = 250,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton
            };
            dgvItems.Columns.Add(itemColumn);
            
            // Other columns as TextBox
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "Quantity", DataPropertyName = "Quantity", Width = 80, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Rate", HeaderText = "Rate", DataPropertyName = "Rate", DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }, Width = 100 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Amount", HeaderText = "Amount", DataPropertyName = "Amount", DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, BackColor = Color.LightGray }, Width = 120, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "SubQuantity", HeaderText = "Sub-Qty", DataPropertyName = "SubQuantity", DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }, Width = 80 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "SubQuantityUnit", HeaderText = "Unit Type", DataPropertyName = "SubQuantityUnit", Width = 80, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Charges", HeaderText = "Charges", DataPropertyName = "Charges", DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }, Width = 80 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "TotalCharges", HeaderText = "Total Charges", DataPropertyName = "TotalCharges", DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, BackColor = Color.LightGray }, Width = 120, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "TotalAmount", HeaderText = "Total", DataPropertyName = "TotalAmount", DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, BackColor = Color.LightGray }, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
        }

        private void PopulateItemComboBox()
        {
            if (dgvItems.Columns["ItemName"] is DataGridViewComboBoxColumn itemColumn)
            {
                // Get all item names
                var itemNames = _allItems.Select(i => i.ItemName).ToList();
                itemColumn.DataSource = itemNames;
            }
        }

        private void SetupPartyComboBox()
        {
            cmbParty.DataSource = _allParties;
            cmbParty.DisplayMember = "PartyName";
            cmbParty.ValueMember = "PartyID";
            cmbParty.SelectedIndexChanged += CmbParty_SelectedIndexChanged;
        }

        private void SetupBrokerComboBox()
        {
            cmbBroker.DataSource = _allBrokers;
            cmbBroker.DisplayMember = "BrokerName";
            cmbBroker.ValueMember = "BrokerID";
            cmbBroker.SelectedIndexChanged += CmbBroker_SelectedIndexChanged;
        }

        private void SetupEventHandlers()
        {
            // TextBox keyboard events
            txtBillDate.KeyDown += TxtBillDate_KeyDown;
            
            // ComboBox keyboard events
            cmbParty.KeyDown += CmbParty_KeyDown;
            cmbBroker.KeyDown += CmbBroker_KeyDown;
            
            // DataGridView events
            dgvItems.CellEndEdit += (s, e) => CalculateRowTotal(e.RowIndex);
            dgvItems.UserDeletedRow += (s, e) => CalculateTotals();
            dgvItems.KeyDown += DgvItems_KeyDown;
            dgvItems.EditingControlShowing += DgvItems_EditingControlShowing;
            
            // Add event for ComboBox selection
            dgvItems.CellValueChanged += DgvItems_CellValueChanged;
            
            txtAdditionalCharges.TextChanged += (s, e) => CalculateTotals();
            btnSave.Click += BtnSave_Click;
            // btnCancel.Click += (s, e) => CloseRequested?.Invoke(this, EventArgs.Empty);
            
            // Quick Add Party button - Check if it exists before adding event handler
            if (btnQuickAddParty != null)
            {
                btnQuickAddParty.Click += BtnQuickAddParty_Click;
            }
            
            // Quick Add Broker button - Check if it exists before adding event handler
            if (btnQuickAddBroker != null)
            {
                btnQuickAddBroker.Click += BtnQuickAddBroker_Click;
            }
            
            // Setup tooltips
            SetupTooltips();
        }

        private void BtnQuickAddParty_Click(object? sender, EventArgs e)
        {
            using (var quickAddPartyForm = new QuickAddPartyForm())
            {
                if (quickAddPartyForm.ShowDialog() == DialogResult.OK)
                {
                    // Refresh parties list and select the newly added party
                    ReloadParties(quickAddPartyForm.NewParty?.PartyID);
                }
            }
        }

        private void ReloadParties(int? selectPartyId = null)
        {
            // Store current selection if we're not selecting a specific party
            int? currentPartyId = selectPartyId;
            if (!currentPartyId.HasValue && cmbParty.SelectedValue is int selectedPartyId)
            {
                currentPartyId = selectedPartyId;
            }
            
            // Reload parties
            _allParties = PartyService.GetAllParties();
            
            // Unbind event temporarily to avoid triggering while updating
            cmbParty.SelectedIndexChanged -= CmbParty_SelectedIndexChanged;
            
            // Reset data source
            cmbParty.DataSource = null;
            cmbParty.DataSource = _allParties;
            cmbParty.DisplayMember = "PartyName";
            cmbParty.ValueMember = "PartyID";
            
            // Restore selected item if possible
            if (currentPartyId.HasValue)
            {
                cmbParty.SelectedValue = currentPartyId.Value;
            }
            
            // Reattach event
            cmbParty.SelectedIndexChanged += CmbParty_SelectedIndexChanged;
            
            // Force update if a party is selected
            if (cmbParty.SelectedIndex >= 0)
            {
                CmbParty_SelectedIndexChanged(cmbParty, EventArgs.Empty);
            }
        }

        private void BtnQuickAddBroker_Click(object? sender, EventArgs e)
        {
            using (var quickAddBrokerForm = new QuickAddBrokerForm())
            {
                if (quickAddBrokerForm.ShowDialog() == DialogResult.OK)
                {
                    // Refresh brokers list and select the newly added broker
                    ReloadBrokers(quickAddBrokerForm.NewBroker?.BrokerID);
                }
            }
        }

        private void ReloadBrokers(int? selectBrokerId = null)
        {
            // Store current selection if we're not selecting a specific broker
            int? currentBrokerId = selectBrokerId;
            if (!currentBrokerId.HasValue && cmbBroker.SelectedValue is int selectedBrokerId)
            {
                currentBrokerId = selectedBrokerId;
            }
            
            // Reload brokers
            _allBrokers = BrokerService.GetAllBrokers();
            
            // Unbind event temporarily to avoid triggering while updating
            cmbBroker.SelectedIndexChanged -= CmbBroker_SelectedIndexChanged;
            
            // Reset data source
            cmbBroker.DataSource = null;
            cmbBroker.DataSource = _allBrokers;
            cmbBroker.DisplayMember = "BrokerName";
            cmbBroker.ValueMember = "BrokerID";
            
            // Restore selected item if possible
            if (currentBrokerId.HasValue)
            {
                cmbBroker.SelectedValue = currentBrokerId.Value;
            }
            
            // Reattach event
            cmbBroker.SelectedIndexChanged += CmbBroker_SelectedIndexChanged;
            
            // Force update if a broker is selected
            if (cmbBroker.SelectedIndex >= 0)
            {
                CmbBroker_SelectedIndexChanged(cmbBroker, EventArgs.Empty);
            }
        }

        private void TxtBillDate_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                cmbParty.Focus();
            }
        }

        private void DgvItems_EditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e)
        {
            // Configure auto-complete for item ComboBox
            if (e.Control is ComboBox comboBox && dgvItems.CurrentCell?.OwningColumn?.Name == "ItemName")
            {
                comboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                comboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
                comboBox.DropDownStyle = ComboBoxStyle.DropDown;
                
                // Add keyboard event for the ComboBox
                comboBox.KeyDown -= ComboBox_KeyDown; // Remove previous handler to avoid duplicates
                comboBox.KeyDown += ComboBox_KeyDown;
            }
        }

        private void ComboBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    dgvItems.EndEdit();
                    
                    // Move to next cell
                    var currentCell = dgvItems.CurrentCell;
                    if (currentCell != null)
                    {
                        int nextCol = currentCell.ColumnIndex + 1;
                        while (nextCol < dgvItems.Columns.Count && dgvItems.Columns[nextCol].ReadOnly)
                        {
                            nextCol++;
                        }
                        
                        if (nextCol < dgvItems.Columns.Count)
                        {
                            this.BeginInvoke((Action)(() => {
                                dgvItems.CurrentCell = dgvItems.Rows[currentCell.RowIndex].Cells[nextCol];
                                dgvItems.BeginEdit(true);
                            }));
                        }
                    }
                }
                else if (e.KeyCode == Keys.Down && !comboBox.DroppedDown)
                {
                    // Open dropdown on first Down arrow
                    comboBox.DroppedDown = true;
                    e.SuppressKeyPress = true;
                }
            }
        }

        private void SetupTooltips()
        {
            var toolTip = new ToolTip();
            toolTip.SetToolTip(txtBillDate, "Enter bill date in dd-mm-yyyy format (F1)");
            toolTip.SetToolTip(cmbParty, "Type to search or use dropdown to select party (F2)");
            
            // Check if btnQuickAddParty exists before setting its tooltip
            if (btnQuickAddParty != null)
            {
                toolTip.SetToolTip(btnQuickAddParty, "Add a new party quickly");
            }
            
            toolTip.SetToolTip(cmbBroker, "Type to search or use dropdown to select broker");
            
            // Check if btnQuickAddBroker exists before setting its tooltip
            if (btnQuickAddBroker != null)
            {
                toolTip.SetToolTip(btnQuickAddBroker, "Add a new broker quickly");
            }
            
            toolTip.SetToolTip(dgvItems, "Items: Type to search or use dropdown to select items. Press Enter to move to next field.");
        }


        #endregion

        #region Form State & Data Loading

       private void ClearForm()
{
    _currentBill = new Bill();
    txtBillNo.Text = BillService.GenerateNewBillNumber();
    // txtBillDate.Text = DateTime.Now.ToString("dd-MM-yyyy");
    DateTime? lastBillDate = BillService.GetLastBillDate();
    txtBillDate.Text = lastBillDate?.ToString("dd-MM-yyyy") ?? DateTime.Now.ToString("dd-MM-yyyy");
    
    cmbParty.SelectedIndex = -1;
    cmbBroker.SelectedIndex = -1;
    lblPartyDetails.Text = "Party details will appear here";
    txtAdditionalCharges.Text = "0.00";
    txtChequeAmountFirm1.Text = "0.00";
    txtChequeAmountFirm2.Text = "0.00";
    
    // Initialize with empty list and use BindingSource
    _currentBill.BillItems = new List<BillItem>();
    
    var bindingSource = new BindingSource();
    bindingSource.DataSource = _currentBill.BillItems;
    dgvItems.DataSource = bindingSource;
    
    AddNewGridRow(); 
    
    CalculateTotals();
    txtBillDate.Focus();
}

        private void LoadBillData()
        {
            txtBillNo.Text = _currentBill.BillNo;
            txtBillDate.Text = _currentBill.BillDate.ToString("dd-MM-yyyy");
            
            // Set party ComboBox
            if (_currentBill.PartyID > 0)
            {
                cmbParty.SelectedValue = _currentBill.PartyID;
            }
            else
            {
                cmbParty.SelectedIndex = -1;
            }

            // Set broker ComboBox
            if (_currentBill.BrokerID > 0)
            {
                cmbBroker.SelectedValue = _currentBill.BrokerID;
            }
            else
            {
                cmbBroker.SelectedIndex = -1;
            }

            txtAdditionalCharges.Text = _currentBill.AdditionalCharges.ToString("N2");
            txtChequeAmountFirm1.Text = _currentBill.ChequeAmountFirm1.ToString("N2");
            txtChequeAmountFirm2.Text = _currentBill.ChequeAmountFirm2.ToString("N2");

            // Ensure BillItems are loaded
            if (_currentBill.BillItems == null)
            {
                _currentBill.BillItems = new List<BillItem>();
            }
            
            // Load bill items from database if not already loaded
            if (_currentBill.BillID > 0 && _currentBill.BillItems.Count == 0)
            {
                _currentBill.BillItems = BillService.GetBillDetails(_currentBill.BillID);
            }

            // Set DataGridView data source
            dgvItems.DataSource = null;
            dgvItems.DataSource = _currentBill.BillItems;
            
            // Refresh the grid to ensure items are displayed
            dgvItems.Refresh();
            
            CalculateTotals();
        }

        #endregion

        #region ComboBox Event Handlers

        private void CmbParty_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbParty.SelectedValue is int partyId && partyId > 0)
            {
                var selectedParty = _allParties.FirstOrDefault(p => p.PartyID == partyId);
                if (selectedParty != null)
                {
                    _currentBill.PartyID = partyId;
                    lblPartyDetails.Text = $"Address: {selectedParty.Address}\nPhone: {selectedParty.Phone}";
                    
                    // Auto-select broker if party has one
                    if (selectedParty.BrokerID.HasValue)
                    {
                        cmbBroker.SelectedValue = selectedParty.BrokerID.Value;
                    }
                }
            }
            else
            {
                _currentBill.PartyID = 0;
                lblPartyDetails.Text = "Party details will appear here";
            }
        }

        private void CmbBroker_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbBroker.SelectedValue is int brokerId && brokerId > 0)
            {
                _currentBill.BrokerID = brokerId;
            }
            else
            {
                _currentBill.BrokerID = 0;
            }
        }

        #endregion

        #region Keyboard & Grid Navigation

        private void CmbParty_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                cmbBroker.Focus();
            }
            else if (e.KeyCode == Keys.Down && cmbParty.DroppedDown)
            {
                // Allow normal dropdown navigation
                return;
            }
            else if (e.KeyCode == Keys.Down && !cmbParty.DroppedDown)
            {
                // Open dropdown on first Down arrow
                cmbParty.DroppedDown = true;
                e.SuppressKeyPress = true;
            }
        }

        private void CmbBroker_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                dgvItems.Focus();
                if (dgvItems.Rows.Count > 0)
                {
                    dgvItems.CurrentCell = dgvItems.Rows[0].Cells["ItemName"];
                    dgvItems.BeginEdit(true);
                }
            }
            else if (e.KeyCode == Keys.Down && cmbBroker.DroppedDown)
            {
                // Allow normal dropdown navigation
                return;
            }
            else if (e.KeyCode == Keys.Down && !cmbBroker.DroppedDown)
            {
                // Open dropdown on first Down arrow
                cmbBroker.DroppedDown = true;
                e.SuppressKeyPress = true;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Handle custom shortcuts
            if (keyData == Keys.Escape)
            {
                CloseRequested?.Invoke(this, EventArgs.Empty);
                return true;
            }
            if (keyData == (Keys.Control | Keys.S))
            {
                BtnSave_Click(this, EventArgs.Empty);
                return true;
            }
            if (keyData == Keys.F1)
            {
                txtBillDate.Focus();
                return true;
            }
            if (keyData == Keys.F2)
            {
                cmbParty.Focus();
                return true;
            }
            if (keyData == Keys.F3)
            {
                dgvItems.Focus();
                if (dgvItems.Rows.Count > 0)
                {
                    dgvItems.CurrentCell = dgvItems.Rows[0].Cells[0];
                    dgvItems.BeginEdit(true);
                }
                return true;
            }
            if (keyData == Keys.F4)
            {
                txtAdditionalCharges.Focus();
                return true;
            }
            if (keyData == Keys.F8)
            {
                // F8 to delete current row
                if (dgvItems.Focused || dgvItems.IsCurrentCellInEditMode)
                {
                    DeleteCurrentRow();
                    return true;
                }
            }
            else if (keyData == (Keys.Control | Keys.P))
            {
                // Ctrl+P - Quick add party
                if (btnQuickAddParty != null)
                {
                    btnQuickAddParty.PerformClick();
                    return true;
                }
            }
            else if (keyData == (Keys.Control | Keys.B))
            {
                // Ctrl+B - Quick add broker
                if (btnQuickAddBroker != null)
                {
                    btnQuickAddBroker.PerformClick();
                    return true;
                }
            }
            else if (keyData == (Keys.Control | Keys.I))
            {
                // Ctrl+I - Quick add item
                QuickAddItem();
                return true;
            }
            
            return base.ProcessCmdKey(ref msg, keyData);
        }
        
        private void QuickAddItem()
        {
            using (var quickAddItemForm = new QuickAddItemForm())
            {
                if (quickAddItemForm.ShowDialog() == DialogResult.OK)
                {
                    ReloadItems(quickAddItemForm.NewItem?.ItemName);
                    
                    // Add a note at the top of the form about the shortcut
                    MessageBox.Show("Item added successfully!\n\nRemember: Use Ctrl+I to quickly add new items.", 
                        "Quick Item Add", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        
        private void ReloadItems(string? selectItemName = null)
        {
            // Store current selection if we're not selecting a specific item
            string? currentItemName = selectItemName;
            
            // Reload items
            _allItems = ItemService.GetAllItems();
            
            // Update combo box column data source
            PopulateItemComboBox();
            
            // If we have a new item, try to select it in the current row
            if (!string.IsNullOrEmpty(currentItemName) && dgvItems.CurrentRow != null)
            {
                dgvItems.CurrentRow.Cells["ItemName"].Value = currentItemName;
                
                // Try to find the item in our list to auto-fill other fields
                var newItem = _allItems.FirstOrDefault(i => i.ItemName == currentItemName);
                if (newItem != null)
                {
                    // Don't try to set Unit as there's no Unit column
                    dgvItems.CurrentRow.Cells["Rate"].Value = newItem.DefaultRate;
                    dgvItems.CurrentRow.Cells["Charges"].Value = newItem.Charges;
                    dgvItems.CurrentRow.Cells["Quantity"].Value = 1;
                    
                    // Calculate row total
                    CalculateRowTotal(dgvItems.CurrentRow.Index);
                }
            }
        }


// protected override bool ProcessDialogKey(Keys keyData)
// {
//     // Handle Enter key globally when search panel is visible
//     if (keyData == Keys.Enter && searchListBox.Visible)
//     {
//         if (searchListBox.Items.Count > 0)
//         {
//             if (searchListBox.SelectedIndex < 0)
//             {
//                 searchListBox.SelectedIndex = 0;
//             }
            
//             if (searchListBox.SelectedItem != null)
//             {
//                 System.Diagnostics.Debug.WriteLine("ProcessDialogKey: Selecting from search panel");
//                 SelectFromSearchPanel();
//                 return true; // Key handled
//             }
//         }
//     }
    
//     // Handle F8 key globally for deleting rows
//     if (keyData == Keys.F8 && dgvItems.Focused)
//     {
//         DeleteCurrentRow();
//         return true; // Key handled
//     }
    
//     return base.ProcessDialogKey(keyData);
// }
private void DeleteCurrentRow()
{
    try
    {
        if (dgvItems.CurrentRow != null && 
            dgvItems.CurrentRow.Index >= 0 && 
            dgvItems.CurrentRow.Index < _currentBill.BillItems.Count)
        {
            int rowIndex = dgvItems.CurrentRow.Index;
            
            // Remove from data source
            _currentBill.BillItems.RemoveAt(rowIndex);
            
            // Refresh the grid
            if (dgvItems.DataSource is BindingSource bs)
            {
                bs.ResetBindings(false);
            }
            else
            {
                dgvItems.DataSource = null;
                dgvItems.DataSource = _currentBill.BillItems;
            }
            
            CalculateTotals();
            
            // If list is empty, add a new row
            if (_currentBill.BillItems.Count == 0)
            {
                AddNewGridRow();
            }
            else
            {
                // Set focus to the same row index or last row if deleted row was the last
                int newRowIndex = Math.Min(rowIndex, dgvItems.Rows.Count - 1);
                if (newRowIndex >= 0 && dgvItems.Rows.Count > 0)
                {
                    dgvItems.CurrentCell = dgvItems.Rows[newRowIndex].Cells[0];
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"Row {rowIndex} deleted successfully");
        }
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error deleting row: {ex.Message}");
        MessageBox.Show($"Error deleting row: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

        private void DgvItems_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            // Handle item selection from ComboBox
            if (e.ColumnIndex == dgvItems.Columns["ItemName"]?.Index && e.RowIndex >= 0)
            {
                var selectedItemName = dgvItems.Rows[e.RowIndex].Cells["ItemName"].Value?.ToString();
                if (!string.IsNullOrEmpty(selectedItemName))
                {
                    var selectedItem = _allItems.FirstOrDefault(i => i.ItemName == selectedItemName);
                    if (selectedItem != null)
                    {
                        var billItem = _currentBill.BillItems[e.RowIndex];
                        billItem.ItemID = selectedItem.ItemID;
                        billItem.ItemName = selectedItem.ItemName;
                        billItem.Rate = selectedItem.DefaultRate;
                        billItem.Charges = selectedItem.Charges;
                        billItem.SubQuantityUnit = selectedItem.SubQuantity; // Copy the text description from Item to BillItem.SubQuantityUnit
                        billItem.SubQuantity = 1; // Initialize sub-quantity to 1
                        billItem.Quantity = 1;
                        
                        // Refresh the display
                        if (dgvItems.DataSource is BindingSource bs)
                        {
                            bs.ResetBindings(false);
                        }
                        
                        CalculateRowTotal(e.RowIndex);
                        
                        // Move to quantity column
                        this.BeginInvoke((Action)(() => {
                            if (e.RowIndex < dgvItems.Rows.Count && dgvItems.Columns.Contains("Quantity"))
                            {
                                dgvItems.CurrentCell = dgvItems.Rows[e.RowIndex].Cells["Quantity"];
                                dgvItems.BeginEdit(true);
                            }
                        }));
                    }
                }
            }
        }
private void MoveToNextCell()
{
    var currentCell = dgvItems.CurrentCell;
    if (currentCell != null)
    {
        // End current edit
        if (dgvItems.IsCurrentCellInEditMode)
        {
            dgvItems.EndEdit();
        }
        
        // Move to next cell
        int nextCol = currentCell.ColumnIndex + 1;
        
        // Skip read-only columns
        while (nextCol < dgvItems.Columns.Count && dgvItems.Columns[nextCol].ReadOnly)
        {
            nextCol++;
        }
        
        if (nextCol < dgvItems.Columns.Count)
        {
            this.BeginInvoke((Action)(() => {
                dgvItems.CurrentCell = dgvItems.Rows[currentCell.RowIndex].Cells[nextCol];
                dgvItems.BeginEdit(true);
            }));
        }
    }
}





     private void DgvItems_KeyDown(object? sender, KeyEventArgs e)
{
    try
    {
        // Handle normal grid navigation
        if (e.KeyCode == Keys.Enter && dgvItems.CurrentCell != null)
        {
            e.SuppressKeyPress = true;
            
            var currentCell = dgvItems.CurrentCell;
            var currentRow = currentCell.RowIndex;
            var currentCol = currentCell.ColumnIndex;
            
            // If we're at the last column, move to next row or add new row
            if (currentCol == dgvItems.Columns.Count - 1)
            {
                if (currentRow == dgvItems.Rows.Count - 1)
                {
                    // Add new row if we're at the last row
                    AddNewGridRow();
                }
                else
                {
                    // Move to first column of next row
                    dgvItems.CurrentCell = dgvItems.Rows[currentRow + 1].Cells[0];
                    dgvItems.BeginEdit(true);
                }
            }
            else
            {
                // Move to next column in same row
                dgvItems.CurrentCell = dgvItems.Rows[currentRow].Cells[currentCol + 1];
                dgvItems.BeginEdit(true);
            }
        }
        else if (e.KeyCode == Keys.Tab)
        {
            // Handle Tab key similar to Enter
            e.SuppressKeyPress = true;
            var currentCell = dgvItems.CurrentCell;
            if (currentCell != null)
            {
                var currentRow = currentCell.RowIndex;
                var currentCol = currentCell.ColumnIndex;
                
                if (currentCol == dgvItems.Columns.Count - 1)
                {
                    if (currentRow == dgvItems.Rows.Count - 1)
                    {
                        AddNewGridRow();
                    }
                    else
                    {
                        dgvItems.CurrentCell = dgvItems.Rows[currentRow + 1].Cells[0];
                        dgvItems.BeginEdit(true);
                    }
                }
                else
                {
                    dgvItems.CurrentCell = dgvItems.Rows[currentRow].Cells[currentCol + 1];
                    dgvItems.BeginEdit(true);
                }
            }
        }
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error in DgvItems_KeyDown: {ex.Message}");
    }
}

        private void AddNewGridRow()
        {
            _currentBill.BillItems.Add(new BillItem());
            dgvItems.DataSource = null; // Refresh binding
            dgvItems.DataSource = _currentBill.BillItems;
            
            // --- THIS IS THE FIX ---
            // Use BeginInvoke to safely set the current cell after the UI has updated.
            this.BeginInvoke((Action)(() => {
                if (dgvItems.Rows.Count > 0)
                {
                    dgvItems.CurrentCell = dgvItems.Rows[dgvItems.Rows.Count - 1].Cells["ItemName"];
                    dgvItems.BeginEdit(true);
                }
            }));
        }

        #endregion

        #region Calculation & Saving

        private void CalculateRowTotal(int rowIndex)
{
    try
    {
        if (rowIndex < 0 || rowIndex >= _currentBill.BillItems.Count || 
            rowIndex >= dgvItems.Rows.Count) 
            return;

        var billItem = _currentBill.BillItems[rowIndex];
        if (billItem != null)
        {
            // Calculate amount based on quantity * rate
            billItem.Amount = (decimal)billItem.Quantity * billItem.Rate;
            
            // Calculate total charges based on SubQuantity * Charges
            billItem.TotalCharges = billItem.SubQuantity * billItem.Charges;
            
            // Calculate total with amount + totalCharges
            billItem.TotalAmount = billItem.Amount + billItem.TotalCharges;
            
            // Use BindingSource refresh instead of dgvItems.Refresh()
            if (dgvItems.DataSource is BindingSource bs)
            {
                bs.ResetBindings(false);
            }
            else
            {
                dgvItems.Refresh();
            }
            
            CalculateTotals();
        }
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error calculating row total: {ex.Message}");
    }
}

        private void CalculateTotals()
        {
            decimal totalItemAmount = _currentBill.BillItems.Sum(i => i.Amount);
            decimal totalItemCharges = _currentBill.BillItems.Sum(i => i.TotalCharges);
            decimal.TryParse(txtAdditionalCharges.Text, out decimal oneTimeCharges);

            // Update the current bill's OriginalAmount to include item charges
            _currentBill.OriginalAmount = totalItemAmount + totalItemCharges;
            _currentBill.AdditionalCharges = oneTimeCharges;

            decimal netAmount = totalItemAmount + totalItemCharges + oneTimeCharges;

            lblTotalAmount.Text = $"Item Total: ₹{totalItemAmount:N2}";
            lblTotalCharges.Text = $"Item Charges: ₹{totalItemCharges:N2}";
            lblNetAmount.Text = $"NET AMOUNT: ₹{netAmount:N2}";
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                _currentBill.BillNo = txtBillNo.Text;
                _currentBill.BillDate = DateTime.ParseExact(txtBillDate.Text, "dd-MM-yyyy", null);
                // OriginalAmount and AdditionalCharges are already updated by CalculateTotals()
                _currentBill.CompanyID = 1; // Replace with Program.ActiveCompany.CompanyID
                _currentBill.Status = "Unpaid"; // Set default status
                
                // Save cheque amount values
                decimal.TryParse(txtChequeAmountFirm1.Text, out decimal chequeAmountFirm1);
                decimal.TryParse(txtChequeAmountFirm2.Text, out decimal chequeAmountFirm2);
                _currentBill.ChequeAmountFirm1 = chequeAmountFirm1;
                _currentBill.ChequeAmountFirm2 = chequeAmountFirm2;
                
                if (BillService.SaveBill(_currentBill, out int billId))
                {
                    _currentBill.BillID = billId; // Update the ID on the current object
                    MessageBox.Show("Bill saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    BillSaved?.Invoke(this, new BillSavedEventArgs(_currentBill));
                    
                    if (!_isEditMode)
                    {
                        ClearForm();
                    }
                    else
                    {
                        CloseRequested?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Items " + _currentBill);
                MessageBox.Show($"Error saving bill: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInputs()
        {
            if (_currentBill.PartyID == 0)
            {
                MessageBox.Show("Please select a party.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbParty.Focus();
                return false;
            }
            if (!_currentBill.BillItems.Any(i => i.ItemID > 0))
            {
                MessageBox.Show("Please add at least one item to the bill.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dgvItems.Focus();
                return false;
            }
            return true;
        }

        #endregion
        
        public class BillSavedEventArgs : EventArgs
        {
            public Bill SavedBill { get; }
            public BillSavedEventArgs(Bill bill) { SavedBill = bill; }
        }
    }
}