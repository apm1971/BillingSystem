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
        
        private Control? _searchTarget;

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
            
            // Item column as ComboBox
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
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Charges", HeaderText = "Charges", DataPropertyName = "Charges", DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }, Width = 80 });
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

private void SetupEventHandlers()
{
    // Party and Broker search
    txtPartyName.TextChanged += (s, e) => {
        if (txtPartyName.Focused)
        {
            ShowSearchPanel(txtPartyName, _allParties.Select(p => p.PartyName).ToList());
        }
    };
    
    txtBrokerName.TextChanged += (s, e) => {
        if (txtBrokerName.Focused)
        {
            ShowSearchPanel(txtBrokerName, _allBrokers.Select(b => b.BrokerName).ToList());
        }
    };
    
    txtPartyName.KeyDown += SearchTextBox_KeyDown;
    txtBrokerName.KeyDown += SearchTextBox_KeyDown;
    
    txtPartyName.Leave += (s, e) => HideSearchPanel();
    txtBrokerName.Leave += (s, e) => HideSearchPanel();
    
    // Setup search list box events ONLY ONCE
    SetupSearchListBoxEvents();

    // DataGridView events
    dgvItems.CellEndEdit += (s, e) => CalculateRowTotal(e.RowIndex);
    dgvItems.UserDeletedRow += (s, e) => CalculateTotals();
    dgvItems.KeyDown += DgvItems_KeyDown;
    
    // Add event for ComboBox selection
    dgvItems.CellValueChanged += DgvItems_CellValueChanged;
    
    txtAdditionalCharges.TextChanged += (s, e) => CalculateTotals();
    btnSave.Click += BtnSave_Click;
    btnCancel.Click += (s, e) => CloseRequested?.Invoke(this, EventArgs.Empty);
}


        #endregion

        #region Form State & Data Loading

       private void ClearForm()
{
    _currentBill = new Bill();
    txtBillNo.Text = BillService.GenerateNewBillNumber();
    txtBillDate.Text = DateTime.Now.ToString("dd-MM-yyyy");
    txtPartyName.Clear();
    txtBrokerName.Clear();
    lblPartyDetails.Text = "Party details will appear here";
    txtAdditionalCharges.Text = "0.00";
    
    // Initialize with empty list and use BindingSource
    _currentBill.BillItems = new List<BillItem>();
    
    var bindingSource = new BindingSource();
    bindingSource.DataSource = _currentBill.BillItems;
    dgvItems.DataSource = bindingSource;
    
    AddNewGridRow(); 
    
    CalculateTotals();
    txtPartyName.Focus();
}

        private void LoadBillData()
        {
            txtBillNo.Text = _currentBill.BillNo;
            txtBillDate.Text = _currentBill.BillDate.ToString("dd-MM-yyyy");
            
            var party = _allParties.FirstOrDefault(p => p.PartyID == _currentBill.PartyID);
            if (party != null)
            {
                txtPartyName.Text = party.PartyName;
                lblPartyDetails.Text = $"Address: {party.Address}\nPhone: {party.Phone}";
            }

            txtBrokerName.Text = _currentBill.BrokerName;
            txtAdditionalCharges.Text = _currentBill.AdditionalCharges.ToString("N2");

            dgvItems.DataSource = null;
            dgvItems.DataSource = _currentBill.BillItems;
            CalculateTotals();
        }

        #endregion

        #region Search Panel Logic

        private void ShowSearchPanel(TextBox target, List<string> dataSource)
{
    System.Diagnostics.Debug.WriteLine($"ShowSearchPanel called for target: {target.Name}, ActiveControl: {this.ActiveControl?.Name}");
    
    if (this.ActiveControl != target) 
    {
        System.Diagnostics.Debug.WriteLine("ShowSearchPanel: ActiveControl != target, returning");
        return;
    }

    _searchTarget = target;
    var filteredData = dataSource
        .Where(s => s.ToLower().Contains(target.Text.ToLower()))
        .ToList();

    System.Diagnostics.Debug.WriteLine($"ShowSearchPanel: Filtered data count: {filteredData.Count}");

    if (filteredData.Any() && !string.IsNullOrWhiteSpace(target.Text))
    {
        searchListBox.DataSource = filteredData;
        
        // Better positioning logic
        Point searchPosition;
        
        // Check if it's a party/broker textbox (they are direct children of groupBox1)
        if (target == txtPartyName || target == txtBrokerName)
        {
            // For party/broker textboxes - use their actual position
            var parentControl = target.Parent; // This should be groupBox1
            var targetLocationInForm = parentControl.PointToScreen(target.Location);
            var locationInThisControl = this.PointToClient(targetLocationInForm);
            searchPosition = new Point(locationInThisControl.X, locationInThisControl.Y + target.Height);
        }
        else if (target.Parent == dgvItems || target.GetType().Name.Contains("DataGridView"))
        {
            // For grid cell textbox - get the actual cell position
            var currentCell = dgvItems.CurrentCell;
            if (currentCell != null)
            {
                var cellRect = dgvItems.GetCellDisplayRectangle(currentCell.ColumnIndex, currentCell.RowIndex, false);
                var gridLocationInForm = dgvItems.PointToScreen(cellRect.Location);
                var locationInThisControl = this.PointToClient(gridLocationInForm);
                searchPosition = new Point(locationInThisControl.X, locationInThisControl.Y + cellRect.Height);
            }
            else
            {
                // Fallback position
                searchPosition = new Point(dgvItems.Left + 50, dgvItems.Top + 100);
            }
        }
        else
        {
            // Fallback for any other textbox
            searchPosition = new Point(target.Left, target.Bottom);
        }
        
        searchListBox.Location = searchPosition;
        searchListBox.Width = Math.Max(250, target.Width);
        searchListBox.Height = Math.Min(120, filteredData.Count * 16 + 10);
        searchListBox.Visible = true;
        searchListBox.BringToFront();
        
        // Auto-select first item for better UX
        if (searchListBox.Items.Count > 0)
        {
            searchListBox.SelectedIndex = 0;
        }
        
        // Ensure the searchListBox can receive focus
        searchListBox.TabStop = true;
        
        System.Diagnostics.Debug.WriteLine($"ShowSearchPanel: Search panel made visible at {searchPosition}, Items: {searchListBox.Items.Count}");
    }
    else
    {
        searchListBox.Visible = false;
        System.Diagnostics.Debug.WriteLine("ShowSearchPanel: No filtered data, hiding search panel");
    }
}

private void HideSearchPanel()
{
    // Use a small delay to allow for selection before hiding
    var timer = new System.Windows.Forms.Timer();
    timer.Interval = 150; // Increased delay slightly
    timer.Tick += (s, e) =>
    {
        timer.Stop();
        if (!searchListBox.Focused && !searchListBox.ClientRectangle.Contains(searchListBox.PointToClient(Cursor.Position)))
        {
            searchListBox.Visible = false;
        }
    };
    timer.Start();
}

private void SelectFromSearchPanel()
{
    try
    {
        System.Diagnostics.Debug.WriteLine("=== SelectFromSearchPanel called ===");
        
        if (searchListBox.SelectedItem == null)
        {
            System.Diagnostics.Debug.WriteLine("ERROR: No item selected in search panel");
            return;
        }
        
        string selectedValue = searchListBox.SelectedItem.ToString();
        System.Diagnostics.Debug.WriteLine($"Selected value: '{selectedValue}'");
        System.Diagnostics.Debug.WriteLine($"Search target type: {_searchTarget?.GetType().Name ?? "null"}");

        if (_searchTarget == txtPartyName)
        {
            System.Diagnostics.Debug.WriteLine("Processing party selection");
            var party = _allParties.FirstOrDefault(p => p.PartyName == selectedValue);
            if (party != null)
            {
                _currentBill.PartyID = party.PartyID;
                txtPartyName.Text = party.PartyName;
                lblPartyDetails.Text = $"Address: {party.Address}\nPhone: {party.Phone}";
                
                if (party.BrokerID.HasValue)
                {
                    var broker = _allBrokers.FirstOrDefault(b => b.BrokerID == party.BrokerID.Value);
                    if (broker != null)
                    {
                        _currentBill.BrokerID = broker.BrokerID;
                        txtBrokerName.Text = broker.BrokerName;
                    }
                }
                System.Diagnostics.Debug.WriteLine($"Party selected successfully: {party.PartyName}");
            }
            searchListBox.Visible = false;
            txtBrokerName.Focus();
        }
        else if (_searchTarget == txtBrokerName)
        {
            System.Diagnostics.Debug.WriteLine("Processing broker selection");
            var broker = _allBrokers.FirstOrDefault(b => b.BrokerName == selectedValue);
            if (broker != null)
            {
                _currentBill.BrokerID = broker.BrokerID;
                txtBrokerName.Text = broker.BrokerName;
                System.Diagnostics.Debug.WriteLine($"Broker selected successfully: {broker.BrokerName}");
            }
            searchListBox.Visible = false;
            dgvItems.Focus();
            if (dgvItems.Rows.Count > 0)
            {
                dgvItems.CurrentCell = dgvItems.Rows[0].Cells["ItemName"];
                dgvItems.BeginEdit(true);
            }
        }
        else if (_searchTarget is TextBox gridTextBox)
        {
            System.Diagnostics.Debug.WriteLine("Processing GRID ITEM selection");
            
            var item = _allItems.FirstOrDefault(i => i.ItemName == selectedValue);
            if (item != null)
            {
                System.Diagnostics.Debug.WriteLine($"Item found: {item.ItemName}, ID: {item.ItemID}");
                
                var row = dgvItems.CurrentRow;
                if (row != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Current row index: {row.Index}");
                    
                    var billItem = row.DataBoundItem as BillItem;
                    if (billItem != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Updating BillItem properties");
                        billItem.ItemID = item.ItemID;
                        billItem.ItemName = item.ItemName;
                        billItem.Rate = item.DefaultRate;
                        billItem.Charges = item.Charges;
                        billItem.Quantity = 1;
                        
                        System.Diagnostics.Debug.WriteLine($"BillItem updated: ItemName={billItem.ItemName}, Rate={billItem.Rate}");
                        
                        // Force refresh the display
                        if (dgvItems.DataSource is BindingSource bs)
                        {
                            System.Diagnostics.Debug.WriteLine("Refreshing BindingSource");
                            bs.ResetBindings(false);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Refreshing DataGridView");
                            dgvItems.Refresh();
                        }
                        
                        CalculateRowTotal(row.Index);
                        
                        searchListBox.Visible = false;
                        
                        // Move to quantity column
                        this.BeginInvoke((Action)(() => {
                            try
                            {
                                System.Diagnostics.Debug.WriteLine("Moving to Quantity column");
                                if (row.Index < dgvItems.Rows.Count && dgvItems.Columns.Contains("Quantity"))
                                {
                                    dgvItems.CurrentCell = dgvItems.Rows[row.Index].Cells["Quantity"];
                                    dgvItems.BeginEdit(true);
                                    System.Diagnostics.Debug.WriteLine("Successfully moved to Quantity column");
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error moving to quantity: {ex.Message}");
                            }
                        }));
                        
                        System.Diagnostics.Debug.WriteLine("Grid item selection completed successfully");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("ERROR: Row DataBoundItem is not BillItem");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: No current row selected");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: Item not found: {selectedValue}");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"ERROR: Unknown search target: {_searchTarget?.GetType().Name ?? "null"}");
        }
        
        System.Diagnostics.Debug.WriteLine("=== SelectFromSearchPanel completed ===");
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"EXCEPTION in SelectFromSearchPanel: {ex.Message}");
        System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        MessageBox.Show($"Error selecting item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

        #endregion

        #region Keyboard & Grid Navigation

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
{
    if (keyData == Keys.Escape)
    {
        if (searchListBox.Visible)
        {
            searchListBox.Visible = false;
            return true;
        }
        CloseRequested?.Invoke(this, EventArgs.Empty);
        return true;
    }
    if (keyData == (Keys.Control | Keys.S))
    {
        BtnSave_Click(this, EventArgs.Empty);
        return true;
    }
    if (keyData == Keys.F2) { txtPartyName.Focus(); return true; }
    if (keyData == Keys.F3) { 
        dgvItems.Focus(); 
        if (dgvItems.Rows.Count > 0)
        {
            dgvItems.CurrentCell = dgvItems.Rows[0].Cells[0];
            dgvItems.BeginEdit(true);
        }
        return true; 
    }
    if (keyData == Keys.F4) { txtAdditionalCharges.Focus(); return true; }
    if (keyData == Keys.F8) { 
        // F8 to delete current row
        if (dgvItems.Focused || dgvItems.IsCurrentCellInEditMode)
        {
            DeleteCurrentRow(); 
            return true; 
        }
    }

    return base.ProcessCmdKey(ref msg, keyData);
}

       private void SearchTextBox_KeyDown(object? sender, KeyEventArgs e)
{
    if (e.KeyCode == Keys.Down && searchListBox.Visible)
    {
        if (searchListBox.Items.Count > 0)
        {
            searchListBox.Focus();
            searchListBox.SelectedIndex = Math.Max(0, searchListBox.SelectedIndex);
        }
        e.SuppressKeyPress = true;
    }
    else if (e.KeyCode == Keys.Enter)
    {
        if (searchListBox.Visible && searchListBox.SelectedItem != null)
        {
            SelectFromSearchPanel();
        }
        else
        {
            // Move to next control
            if (sender == txtPartyName)
            {
                txtBrokerName.Focus();
            }
            else if (sender == txtBrokerName)
            {
                dgvItems.Focus();
                if (dgvItems.Rows.Count > 0)
                {
                    dgvItems.CurrentCell = dgvItems.Rows[0].Cells["ItemName"];
                    dgvItems.BeginEdit(true);
                }
            }
        }
        e.SuppressKeyPress = true;
    }
    else if (e.KeyCode == Keys.Escape)
    {
        searchListBox.Visible = false;
        e.SuppressKeyPress = true;
    }
}
        
   private void SearchListBox_KeyDown(object? sender, KeyEventArgs e)
{
    System.Diagnostics.Debug.WriteLine($"SearchListBox KeyDown: {e.KeyCode}, SelectedIndex: {searchListBox.SelectedIndex}, ItemsCount: {searchListBox.Items.Count}");
    
    // Handle Enter key
    if (e.KeyCode == Keys.Enter)
    {
        System.Diagnostics.Debug.WriteLine("SearchListBox: Enter pressed");
        e.Handled = true;
        e.SuppressKeyPress = true;
        
        // Ensure we have a selection
        if (searchListBox.SelectedIndex < 0 && searchListBox.Items.Count > 0)
        {
            searchListBox.SelectedIndex = 0;
            System.Diagnostics.Debug.WriteLine("SearchListBox: Auto-selected first item");
        }
        
        // Select the item
        if (searchListBox.SelectedItem != null)
        {
            System.Diagnostics.Debug.WriteLine($"SearchListBox: Selecting item: {searchListBox.SelectedItem}");
            SelectFromSearchPanel();
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("SearchListBox: No selected item found");
        }
    }
    // Handle Escape key
    else if (e.KeyCode == Keys.Escape)
    {
        System.Diagnostics.Debug.WriteLine("SearchListBox: Escape pressed");
        e.SuppressKeyPress = true;
        searchListBox.Visible = false;
        if (_searchTarget != null)
        {
            _searchTarget.Focus();
        }
    }
    // Handle Up/Down navigation (let them work normally)
    else if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
    {
        System.Diagnostics.Debug.WriteLine($"SearchListBox: Navigation key {e.KeyCode}");
        // Don't suppress these keys - let them work normally
    }
}
private void SearchListBox_MouseEnter(object? sender, EventArgs e)
{
    // Ensure the listbox can receive keyboard input
    if (searchListBox.Visible && searchListBox.Items.Count > 0)
    {
        if (searchListBox.SelectedIndex < 0)
        {
            searchListBox.SelectedIndex = 0;
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

private void SetupSearchListBoxEvents()
{
    // Clear any existing events first
    searchListBox.KeyDown -= SearchListBox_KeyDown;
    searchListBox.PreviewKeyDown -= SearchListBox_PreviewKeyDown;
    searchListBox.MouseDoubleClick -= SearchListBox_DoubleClick;
    searchListBox.MouseEnter -= SearchListBox_MouseEnter;
    
    // Add events
    searchListBox.KeyDown += SearchListBox_KeyDown;
    searchListBox.PreviewKeyDown += SearchListBox_PreviewKeyDown;
    searchListBox.MouseDoubleClick += SearchListBox_DoubleClick;
    searchListBox.MouseEnter += SearchListBox_MouseEnter;
    
    searchListBox.MouseClick += (s, e) => {
        System.Diagnostics.Debug.WriteLine("SearchListBox clicked");
        if (searchListBox.SelectedItem != null)
        {
            SelectFromSearchPanel();
        }
    };
    
    // Important settings
    searchListBox.TabStop = true;
    searchListBox.SelectionMode = SelectionMode.One;
    searchListBox.IntegralHeight = false; // Allow partial height
}
        
        private void SearchListBox_DoubleClick(object? sender, EventArgs e)
        {
            SelectFromSearchPanel();
        }

        private void DgvItems_EditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e)
{
    // No longer needed since we're using ComboBox for ItemName column
    // The ComboBox will handle its own events
}

private void GridItemTextBox_Leave(object? sender, EventArgs e)
{
    HideSearchPanel();
}

        private void GridItemTextBox_TextChanged(object? sender, EventArgs e)
{
    System.Diagnostics.Debug.WriteLine($"GridItemTextBox_TextChanged called, sender: {sender?.GetType().Name}");
    if (sender is TextBox tb)
    {
        System.Diagnostics.Debug.WriteLine($"GridItemTextBox_TextChanged: Text = '{tb.Text}', Calling ShowSearchPanel");
        ShowSearchPanel(tb, _allItems.Select(i => i.ItemName).ToList());
    }
}

        private void GridItemTextBox_KeyDown(object? sender, KeyEventArgs e)
{
    System.Diagnostics.Debug.WriteLine($"GridTextBox KeyDown: {e.KeyCode}, SearchVisible: {searchListBox.Visible}, ItemsCount: {searchListBox.Items.Count}");
    
    // Handle Down arrow key
    if (e.KeyCode == Keys.Down)
    {
        if (searchListBox.Visible && searchListBox.Items.Count > 0)
        {
            System.Diagnostics.Debug.WriteLine("Down arrow: Moving focus to searchListBox");
            e.SuppressKeyPress = true;
            
            // Immediately focus the searchListBox
            searchListBox.Focus();
            if (searchListBox.SelectedIndex < 0)
            {
                searchListBox.SelectedIndex = 0;
            }
            return;
        }
    }
    
    // Handle Enter key
    if (e.KeyCode == Keys.Enter)
    {
        System.Diagnostics.Debug.WriteLine("Enter pressed in GridTextBox");
        e.SuppressKeyPress = true;
        
        if (searchListBox.Visible && searchListBox.Items.Count > 0)
        {
            System.Diagnostics.Debug.WriteLine("Enter: Search panel visible, selecting item");
            
            // Ensure we have a selection
            if (searchListBox.SelectedIndex < 0)
            {
                searchListBox.SelectedIndex = 0;
            }
            
            if (searchListBox.SelectedItem != null)
            {
                System.Diagnostics.Debug.WriteLine($"Enter: Selecting item: {searchListBox.SelectedItem}");
                
                // End current edit mode
                if (dgvItems.IsCurrentCellInEditMode)
                {
                    dgvItems.EndEdit();
                }
                
                // Select the item
                SelectFromSearchPanel();
                return;
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Enter: No search panel or no items, moving to next cell");
            MoveToNextCell();
        }
    }
    
    // Handle Escape key
    if (e.KeyCode == Keys.Escape)
    {
        System.Diagnostics.Debug.WriteLine("Escape pressed in GridTextBox");
        e.SuppressKeyPress = true;
        searchListBox.Visible = false;
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


private void SearchListBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
{
    System.Diagnostics.Debug.WriteLine($"SearchListBox PreviewKeyDown: {e.KeyCode}");
    if (e.KeyCode == Keys.Enter)
    {
        e.IsInputKey = true; // Make sure Enter is treated as input
        System.Diagnostics.Debug.WriteLine("Enter marked as input key");
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
            billItem.Amount = (decimal)billItem.Quantity * billItem.Rate;
            billItem.TotalAmount = billItem.Amount + billItem.Charges;
            
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
            decimal totalItemCharges = _currentBill.BillItems.Sum(i => i.Charges);
            decimal.TryParse(txtAdditionalCharges.Text, out decimal oneTimeCharges);

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
                _currentBill.AdditionalCharges = decimal.Parse(txtAdditionalCharges.Text);
                _currentBill.OriginalAmount = _currentBill.BillItems.Sum(i => i.Amount);
                _currentBill.CompanyID = 0; // Replace with Program.ActiveCompany.CompanyID
                _currentBill.Status = "Unpaid"; // Set default status
                
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
                txtPartyName.Focus();
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