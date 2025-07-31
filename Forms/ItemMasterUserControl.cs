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
            // Configure text boxes
            txtItemName.MaxLength = 100;
            txtUnit.MaxLength = 20;
            txtDefaultRate.TextAlign = HorizontalAlignment.Right;
            txtCharges.TextAlign = HorizontalAlignment.Right;

            // Set default values
            txtDefaultRate.Text = "0.00";
            txtCharges.Text = "0.00";

            // Setup search functionality
            txtSearch.TextChanged += txtSearch_TextChanged;
        }

        private void SetupDataGrid()
        {
            dgvItems.AutoGenerateColumns = false;
            dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvItems.MultiSelect = false;
            dgvItems.ReadOnly = true;
            dgvItems.AllowUserToAddRows = false;
            dgvItems.AllowUserToDeleteRows = false;

            // Configure columns
            dgvItems.Columns.Clear();

            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ItemName",
                HeaderText = "Item Name",
                DataPropertyName = "ItemName",
                Width = 200
            });

            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Unit",
                HeaderText = "Unit",
                DataPropertyName = "Unit",
                Width = 80
            });

            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DefaultRate",
                HeaderText = "Default Rate",
                DataPropertyName = "DefaultRate",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Charges",
                HeaderText = "Charges",
                DataPropertyName = "Charges",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

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

        private void ItemMasterUserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                // In a UserControl, you typically don't close the control itself with Escape.
                // You might raise an event for the parent form to handle or simply do nothing.
                e.Handled = true;
                // If you want to signal the parent form to close, you'd raise an event:
                // OnCloseRequested?.Invoke(this, EventArgs.Empty);
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                // Handle Ctrl+S
                e.Handled = true;
                btnSave.PerformClick();
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