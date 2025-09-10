using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class ItemMasterUserControl : UserControl
    {
        private List<Item> items = new List<Item>();
        private List<Item> filteredItems = new List<Item>();
        private Item currentItem = new Item();
        private bool isNewItem = true;

        public ItemMasterUserControl()
        {
            InitializeComponent();
            LoadItems();
            ConfigureControls();
            SetupDataGrid();
            
            // Handle key events for the UserControl
            this.KeyDown += ItemMasterUserControl_KeyDown;
        }

        private void ItemMasterUserControl_Load(object sender, EventArgs e)
        {
            ConfigureControls();
            SetupDataGrid();
            ClearForm();
        }

        private void ConfigureControls()
        {
            // Set up Tab order
            txtSearch.TabIndex = 0;
            txtItemName.TabIndex = 1;
            txtUnit.TabIndex = 2;
            txtDefaultRate.TabIndex = 3;
            txtCharges.TabIndex = 4;
            txtSubQuantity.TabIndex = 5;
            btnSave.TabIndex = 6;
            btnNew.TabIndex = 7;
            btnDelete.TabIndex = 8;
            
            // Set up text fields to use uppercase
            txtItemName.CharacterCasing = CharacterCasing.Upper;
            txtUnit.CharacterCasing = CharacterCasing.Upper;
            txtSubQuantity.CharacterCasing = CharacterCasing.Upper;

            // Configure text boxes
            txtItemName.MaxLength = 100;
            txtUnit.MaxLength = 20;
            txtSubQuantity.MaxLength = 50;
            txtDefaultRate.TextAlign = HorizontalAlignment.Right;
            txtCharges.TextAlign = HorizontalAlignment.Right;

            // Set default values
            txtDefaultRate.Text = "0.00";
            txtCharges.Text = "0.00";

            // Setup search functionality
            txtSearch.TextChanged += txtSearch_TextChanged;
            
            // Add KeyDown event handlers for all input controls
            txtSearch.KeyDown += txtSearch_KeyDown;
            txtItemName.KeyDown += Control_KeyDown;
            txtUnit.KeyDown += Control_KeyDown;
            txtDefaultRate.KeyDown += Control_KeyDown;
            txtCharges.KeyDown += Control_KeyDown;
            txtSubQuantity.KeyDown += Control_KeyDown;
            dgvItems.KeyDown += Control_KeyDown;
            
            // Set up form controls
            txtSearch.PlaceholderText = "Type to search items...";
            txtItemName.PlaceholderText = "Enter item name";
            txtUnit.PlaceholderText = "Enter unit (e.g., KG, PCS)";
            txtDefaultRate.PlaceholderText = "Enter default rate";
            txtCharges.PlaceholderText = "Enter charges per sub-quantity";
            txtSubQuantity.PlaceholderText = "Enter sub-quantity unit (e.g., BAG, BOX)";
            
            // Set up button styles with shortcuts
            SetupButtonStyle(btnSave, System.Drawing.Color.FromArgb(0, 122, 204));
            SetupButtonStyle(btnNew, System.Drawing.Color.FromArgb(40, 167, 69));
            SetupButtonStyle(btnDelete, System.Drawing.Color.FromArgb(220, 53, 69));
            
            // Update button text to show shortcuts
            btnSave.Text = "Save (Ctrl+S)";
            btnNew.Text = "New (Ctrl+N)";
            btnDelete.Text = "Delete (F8)";
            
            // Add tooltips for shortcuts
            var toolTip = new ToolTip();
            toolTip.SetToolTip(btnSave, "Save the current item (Ctrl+S)");
            toolTip.SetToolTip(btnNew, "Create a new item (Ctrl+N)");
            toolTip.SetToolTip(btnDelete, "Delete the selected item (F8)");
            toolTip.SetToolTip(txtSearch, "Search items by name (F3)");
            toolTip.SetToolTip(txtItemName, "Enter item name (F2)");
            toolTip.SetToolTip(txtSubQuantity, "Enter the sub-quantity unit (e.g., BAG, BOX)");
            toolTip.SetToolTip(txtCharges, "Enter charges per sub-quantity unit");
        }

        private void SetupButtonStyle(Button button, System.Drawing.Color baseColor)
        {
            button.BackColor = baseColor;
            button.ForeColor = System.Drawing.Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            
            // Add hover effects
            button.MouseEnter += (s, e) => {
                button.BackColor = System.Drawing.Color.FromArgb(
                    Math.Min(255, baseColor.R + 20),
                    Math.Min(255, baseColor.G + 20),
                    Math.Min(255, baseColor.B + 20)
                );
            };
            
            button.MouseLeave += (s, e) => {
                button.BackColor = baseColor;
            };
        }

        private void SetupDataGrid()
        {
            // Configure data grid
            dgvItems.AutoGenerateColumns = false;
            dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvItems.AllowUserToAddRows = false;
            dgvItems.AllowUserToDeleteRows = false;
            dgvItems.ReadOnly = true;
            dgvItems.MultiSelect = false;
            dgvItems.RowHeadersVisible = false;
            dgvItems.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = System.Drawing.Color.FromArgb(245, 245, 245)
            };

            // Set modern font and styling
            dgvItems.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            dgvItems.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            dgvItems.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            dgvItems.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dgvItems.ColumnHeadersHeight = 35;
            dgvItems.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Set row height for better readability
            dgvItems.RowTemplate.Height = 30;

            // Clear existing columns
            dgvItems.Columns.Clear();

            // Add columns with proper sizing
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ItemID",
                HeaderText = "ID",
                Width = 60,
                Visible = false
            });

            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ItemName",
                HeaderText = "Item Name",
                Width = 200,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            });

            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Unit",
                HeaderText = "Unit",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleLeft }
            });

            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DefaultRate",
                HeaderText = "Default Rate",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Charges",
                HeaderText = "Charges",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SubQuantity",
                HeaderText = "Sub-Quantity",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleLeft }
            });

            // Enable double buffering for smooth scrolling
            typeof(DataGridView).InvokeMember("DoubleBuffered", 
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, dgvItems, new object[] { true });

            // Event handlers
            dgvItems.SelectionChanged += dgvItems_SelectionChanged;
            dgvItems.CellDoubleClick += dgvItems_CellDoubleClick;
        }

        private void LoadItems()
        {
            try
            {
                items = ItemService.GetAllItems();
                dgvItems.DataSource = null;
                dgvItems.DataSource = items;

                lblTotalItems.Text = $"Total Items: {items.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading items: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearForm()
        {
            currentItem = new Item();
            isNewItem = true;

            txtItemName.Text = string.Empty;
            txtUnit.Text = string.Empty;
            txtDefaultRate.Text = "0.00";
            txtCharges.Text = "0.00";
            txtSubQuantity.Text = string.Empty;

            txtItemName.Focus();
            btnDelete.Enabled = false;
        }

        private void PopulateForm(Item item)
        {
            currentItem = item;
            isNewItem = false;

            txtItemName.Text = item.ItemName;
            txtUnit.Text = item.Unit;
            txtDefaultRate.Text = item.DefaultRate.ToString("N2");
            txtCharges.Text = item.Charges.ToString("N2");
            txtSubQuantity.Text = item.SubQuantity;

            btnDelete.Enabled = true;
        }

        private Item GetItemFromForm()
        {
            Item item = new Item
            {
                ItemID = currentItem.ItemID,
                ItemName = txtItemName.Text.Trim(),
                Unit = txtUnit.Text.Trim(),
                DefaultRate = Convert.ToDecimal(txtDefaultRate.Text),
                Charges = Convert.ToDecimal(txtCharges.Text),
                SubQuantity = txtSubQuantity.Text.Trim(),
                CompanyID = Program.ActiveCompany?.CompanyID ?? 1
            };

            return item;
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtItemName.Text))
            {
                MessageBox.Show("Please enter Item Name", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtItemName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtUnit.Text))
            {
                MessageBox.Show("Please enter Unit", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUnit.Focus();
                return false;
            }

            return true;
        }

        #region Event Handlers

        private void btnNew_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                Item item = GetItemFromForm();
                bool success;

                if (isNewItem)
                {
                    success = ItemService.AddItem(item);
                    if (success)
                        MessageBox.Show("Item added successfully", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Debug information
                    string debugInfo = $"Updating item: ID={item.ItemID}, Name={item.ItemName}, CompanyID={item.CompanyID}";
                    System.Diagnostics.Debug.WriteLine(debugInfo);
                    
                    success = ItemService.UpdateItem(item);
                    if (success)
                        MessageBox.Show("Item updated successfully", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show("Failed to update item. No rows were affected.", "Update Failed",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                if (success)
                {
                    LoadItems();
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving item: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (currentItem.ItemID == 0)
                return;

            // Check permissions before allowing delete
            if (!PermissionManager.ValidateDeleteOperation(ModuleType.Masters, "item"))
                return;

            if (MessageBox.Show("Are you sure you want to delete this item?", "Confirm Delete", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    bool success = ItemService.DeleteItem(currentItem.ItemID);

                    if (success)
                    {
                        MessageBox.Show("Item deleted successfully", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadItems();
                        ClearForm();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting item: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvItems_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvItems.SelectedRows.Count > 0)
            {
                int selectedIndex = dgvItems.SelectedRows[0].Index;

                // Use the current displayed list (filtered or full)
                var currentList = string.IsNullOrWhiteSpace(txtSearch.Text.Trim()) ? items : filteredItems;

                if (selectedIndex >= 0 && selectedIndex < currentList.Count)
                {
                    Item selectedItem = currentList[selectedIndex];
                    PopulateForm(selectedItem);
                }
            }
        }

        private void dgvItems_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Use the current displayed list (filtered or full)
            var currentList = string.IsNullOrWhiteSpace(txtSearch.Text.Trim()) ? items : filteredItems;

            if (e.RowIndex >= 0 && e.RowIndex < currentList.Count)
            {
                Item selectedItem = currentList[e.RowIndex];
                PopulateForm(selectedItem);
                txtItemName.Focus();
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            // Simple filtering on the client side
            string searchText = txtSearch.Text.ToLower().Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                filteredItems = items;
                dgvItems.DataSource = items;
            }
            else
            {
                filteredItems = items.FindAll(i => 
                    i.ItemName.ToLower().Contains(searchText)
                );

                dgvItems.DataSource = null;
                dgvItems.DataSource = filteredItems;
            }

            lblTotalItems.Text = $"Total Items: {filteredItems.Count}";

            // If there are filtered items, select the first one
            if (filteredItems.Count > 0)
            {
                dgvItems.ClearSelection();
                dgvItems.Rows[0].Selected = true;
                // The selection change event will handle populating the form
            }
            else
            {
                ClearForm(); // Clear the form if no items match the search
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Prevent the beep sound
                e.Handled = true;

                // If there are filtered items, select the first one
                if (filteredItems.Count > 0)
                {
                    // Select the first row in the grid
                    dgvItems.ClearSelection();
                    dgvItems.Rows[0].Selected = true;

                    // Populate the form with the selected item
                    PopulateForm(filteredItems[0]);

                    // Move focus to the item name field
                    txtItemName.Focus();
                }
            }
        }

        private void Control_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle keyboard shortcuts for all controls
            if (e.Control && e.KeyCode == Keys.S)
            {
                // Handle Ctrl+S
                e.Handled = true;
                e.SuppressKeyPress = true;
                btnSave.PerformClick();
            }
            else if (e.Control && e.KeyCode == Keys.N)
            {
                // Handle Ctrl+N
                e.Handled = true;
                e.SuppressKeyPress = true;
                btnNew.PerformClick();
            }
            else if (e.KeyCode == Keys.F2)
            {
                // F2 to focus on item name
                e.Handled = true;
                e.SuppressKeyPress = true;
                txtItemName.Focus();
            }
            else if (e.KeyCode == Keys.F3)
            {
                // F3 to focus on search
                e.Handled = true;
                e.SuppressKeyPress = true;
                txtSearch.Focus();
            }
            else if (e.KeyCode == Keys.F4)
            {
                // F4 to focus on grid
                e.Handled = true;
                e.SuppressKeyPress = true;
                dgvItems.Focus();
            }
            else if (e.KeyCode == Keys.F8)
            {
                // F8 to delete selected item
                e.Handled = true;
                e.SuppressKeyPress = true;
                btnDelete.PerformClick();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                // Clear search or clear form
                e.Handled = true;
                e.SuppressKeyPress = true;
                if (txtSearch.Focused && !string.IsNullOrEmpty(txtSearch.Text))
                {
                    txtSearch.Clear();
                    txtSearch.Focus();
                }
                else if (!txtSearch.Focused)
                {
                    ClearForm();
                    txtSearch.Focus();
                }
            }
        }

        private void ItemMasterUserControl_KeyDown(object sender, KeyEventArgs e)
        {
            // This method now only handles key events when the UserControl itself has focus
            // Most keyboard shortcuts are handled by individual controls via Control_KeyDown
            if (e.KeyCode == Keys.Escape)
            {
                // Clear search or clear form
                if (txtSearch.Focused && !string.IsNullOrEmpty(txtSearch.Text))
                {
                    txtSearch.Clear();
                    txtSearch.Focus();
                }
                else if (!txtSearch.Focused)
                {
                    ClearForm();
                    txtSearch.Focus();
                }
                e.Handled = true;
            }
        }

        #endregion

        // Public event to notify the parent form if an item is selected (useful for dialog-like behavior)
        public event EventHandler<ItemSelectedEventArgs> ItemSelected;

        // Custom EventArgs for passing the selected Item
        public class ItemSelectedEventArgs : EventArgs
        {
            public Item SelectedItem { get; }
            public ItemSelectedEventArgs(Item item)
            {
                SelectedItem = item;
            }
        }

        // Method to call when an item is selected and confirmed (e.g., from a double-click or a "Select" button)
        private void OnItemSelected(Item item)
        {
            ItemSelected?.Invoke(this, new ItemSelectedEventArgs(item));
        }
    }
} 