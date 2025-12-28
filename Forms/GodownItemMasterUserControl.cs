using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class GodownItemMasterUserControl : UserControl
    {
        private List<GodownItem> items = new List<GodownItem>();
        private List<GodownItem> filteredItems = new List<GodownItem>();
        private GodownItem currentItem = new GodownItem();
        private bool isNewItem = true;

        public GodownItemMasterUserControl()
        {
            InitializeComponent();
            LoadItems();
            ConfigureControls();
            SetupDataGrid();
            
            this.KeyDown += GodownItemMasterUserControl_KeyDown;
        }

        private void GodownItemMasterUserControl_Load(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ConfigureControls()
        {
            txtSearch.TabIndex = 0;
            txtItemName.TabIndex = 1;
            btnSave.TabIndex = 2;
            btnNew.TabIndex = 3;
            btnDelete.TabIndex = 4;
            
            txtItemName.CharacterCasing = CharacterCasing.Upper;
            txtItemName.MaxLength = 255;

            txtSearch.TextChanged += txtSearch_TextChanged;
            
            txtSearch.KeyDown += txtSearch_KeyDown;
            txtItemName.KeyDown += Control_KeyDown;
            dgvItems.KeyDown += Control_KeyDown;
            
            txtSearch.PlaceholderText = "Type to search items...";
            txtItemName.PlaceholderText = "Enter item name";
            
            SetupButtonStyle(btnSave, Color.FromArgb(0, 122, 204));
            SetupButtonStyle(btnNew, Color.FromArgb(40, 167, 69));
            SetupButtonStyle(btnDelete, Color.FromArgb(220, 53, 69));
            
            btnSave.Text = "Save (Ctrl+S)";
            btnNew.Text = "New (Ctrl+N)";
            btnDelete.Text = "Delete (F8)";
        }

        private void SetupButtonStyle(Button button, Color baseColor)
        {
            button.BackColor = baseColor;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            
            button.MouseEnter += (s, e) => {
                button.BackColor = Color.FromArgb(
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
            dgvItems.AutoGenerateColumns = false;
            dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvItems.AllowUserToAddRows = false;
            dgvItems.AllowUserToDeleteRows = false;
            dgvItems.ReadOnly = true;
            dgvItems.MultiSelect = false;
            dgvItems.RowHeadersVisible = false;
            dgvItems.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(245, 245, 245)
            };

            dgvItems.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            dgvItems.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(64, 64, 64);
            dgvItems.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvItems.ColumnHeadersHeight = 35;
            dgvItems.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvItems.RowTemplate.Height = 30;

            dgvItems.Columns.Clear();

            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "GodownItemID",
                HeaderText = "ID",
                Width = 60,
                Visible = false
            });

            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ItemName",
                HeaderText = "Item Name",
                Width = 500,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            typeof(DataGridView).InvokeMember("DoubleBuffered", 
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, dgvItems, new object[] { true });

            dgvItems.SelectionChanged += dgvItems_SelectionChanged;
            dgvItems.CellDoubleClick += dgvItems_CellDoubleClick;
        }

        private void LoadItems()
        {
            try
            {
                items = GodownItemService.GetAllGodownItems();
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
            currentItem = new GodownItem();
            isNewItem = true;

            txtItemName.Text = string.Empty;
            txtItemName.Focus();
            btnDelete.Enabled = false;
        }

        private void PopulateForm(GodownItem item)
        {
            currentItem = item;
            isNewItem = false;

            txtItemName.Text = item.ItemName;
            btnDelete.Enabled = true;
        }

        private GodownItem GetItemFromForm()
        {
            return new GodownItem
            {
                GodownItemID = currentItem.GodownItemID,
                ItemName = txtItemName.Text.Trim()
            };
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

            if (isNewItem && GodownItemService.GodownItemExists(txtItemName.Text.Trim()))
            {
                MessageBox.Show("An item with this name already exists", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtItemName.Focus();
                return false;
            }

            if (!isNewItem && GodownItemService.GodownItemExists(txtItemName.Text.Trim(), currentItem.GodownItemID))
            {
                MessageBox.Show("An item with this name already exists", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtItemName.Focus();
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
                GodownItem item = GetItemFromForm();
                bool success;

                if (isNewItem)
                {
                    success = GodownItemService.AddGodownItem(item);
                    if (success)
                        MessageBox.Show("Item added successfully", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    success = GodownItemService.UpdateGodownItem(item);
                    if (success)
                        MessageBox.Show("Item updated successfully", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            if (currentItem.GodownItemID == 0)
                return;

            if (MessageBox.Show("Are you sure you want to delete this item?", "Confirm Delete", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    bool success = GodownItemService.DeleteGodownItem(currentItem.GodownItemID);

                    if (success)
                    {
                        MessageBox.Show("Item deleted successfully", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadItems();
                        ClearForm();
                    }
                    else
                    {
                        MessageBox.Show("Cannot delete item. It may have transactions or opening stock.", "Delete Failed", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                var currentList = string.IsNullOrWhiteSpace(txtSearch.Text.Trim()) ? items : filteredItems;

                if (selectedIndex >= 0 && selectedIndex < currentList.Count)
                {
                    GodownItem selectedItem = currentList[selectedIndex];
                    PopulateForm(selectedItem);
                }
            }
        }

        private void dgvItems_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            var currentList = string.IsNullOrWhiteSpace(txtSearch.Text.Trim()) ? items : filteredItems;

            if (e.RowIndex >= 0 && e.RowIndex < currentList.Count)
            {
                GodownItem selectedItem = currentList[e.RowIndex];
                PopulateForm(selectedItem);
                txtItemName.Focus();
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
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

            if (filteredItems.Count > 0)
            {
                dgvItems.ClearSelection();
                dgvItems.Rows[0].Selected = true;
            }
            else
            {
                ClearForm();
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;

                if (filteredItems.Count > 0)
                {
                    dgvItems.ClearSelection();
                    dgvItems.Rows[0].Selected = true;
                    PopulateForm(filteredItems[0]);
                    txtItemName.Focus();
                }
            }
        }

        private void Control_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                btnSave.PerformClick();
            }
            else if (e.Control && e.KeyCode == Keys.N)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                btnNew.PerformClick();
            }
            else if (e.KeyCode == Keys.F2)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                txtItemName.Focus();
            }
            else if (e.KeyCode == Keys.F3)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                txtSearch.Focus();
            }
            else if (e.KeyCode == Keys.F8)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                btnDelete.PerformClick();
            }
            else if (e.KeyCode == Keys.Escape)
            {
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

        private void GodownItemMasterUserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
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
    }
}

