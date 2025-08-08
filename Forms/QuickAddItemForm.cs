using System;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class QuickAddItemForm : Form
    {
        public Item? NewItem { get; private set; }

        public QuickAddItemForm()
        {
            InitializeComponent();
            SetupForm();
            
            // Enable key preview to handle keyboard shortcuts
            this.KeyPreview = true;
            this.KeyDown += QuickAddItemForm_KeyDown;
        }

        private void QuickAddItemForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                // Handle Escape key
                e.Handled = true;
                btnCancel.PerformClick();
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                // Handle Ctrl+S
                e.Handled = true;
                btnSave.PerformClick();
            }
        }

        private void SetupForm()
        {
            this.Text = "Quick Add Item";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // Set text fields to use uppercase
            txtItemName.CharacterCasing = CharacterCasing.Upper;
            txtUnit.CharacterCasing = CharacterCasing.Upper;
            
            txtItemName.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                decimal defaultRate = 0;
                decimal.TryParse(txtDefaultRate.Text, out defaultRate);
                
                decimal charges = 0;
                decimal.TryParse(txtCharges.Text, out charges);
                
                var item = new Item
                {
                    ItemName = txtItemName.Text.Trim().ToUpper(),
                    Unit = txtUnit.Text.Trim().ToUpper(),
                    DefaultRate = defaultRate,
                    Charges = charges
                };

                if (ItemService.AddItem(item))
                {
                    // Get the newly added item with its ID
                    var items = ItemService.GetAllItems();
                    NewItem = items.Find(i => i.ItemName == item.ItemName);
                    
                    MessageBox.Show("Item added successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to add item. Please try again.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding item: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

            // Check for duplicate item name
            if (ItemService.ItemExists(txtItemName.Text.Trim(), null))
            {
                MessageBox.Show("An item with this name already exists", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtItemName.Focus();
                return false;
            }

            // Validate numeric fields
            decimal temp;
            if (!decimal.TryParse(txtDefaultRate.Text, out temp))
            {
                MessageBox.Show("Please enter a valid Default Rate", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDefaultRate.Focus();
                return false;
            }

            if (!decimal.TryParse(txtCharges.Text, out temp))
            {
                MessageBox.Show("Please enter a valid Charges amount", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCharges.Focus();
                return false;
            }

            return true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
} 