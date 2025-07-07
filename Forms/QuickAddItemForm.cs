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
        }

        private void SetupForm()
        {
            this.Text = "Quick Add Item";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // Set default values
            txtRate.Text = "0.00";
            txtCharges.Text = "0.00";
            txtStockQuantity.Text = "0.00";
            
            txtItemCode.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                var item = new Item
                {
                    ItemCode = txtItemCode.Text.Trim(),
                    ItemName = txtItemName.Text.Trim(),
                    Unit = txtUnit.Text.Trim(),
                    Rate = Convert.ToDouble(txtRate.Text),
                    Charges = Convert.ToDouble(txtCharges.Text),
                    StockQuantity = Convert.ToDouble(txtStockQuantity.Text)
                };

                if (ItemService.AddItem(item))
                {
                    // Get the newly added item with its ID
                    var items = ItemService.GetAllItems();
                    NewItem = items.Find(i => i.ItemCode == item.ItemCode);
                    
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
            if (string.IsNullOrWhiteSpace(txtItemCode.Text))
            {
                MessageBox.Show("Please enter Item Code", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtItemCode.Focus();
                return false;
            }

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

            // Check for duplicate item code
            if (ItemService.ItemCodeExists(txtItemCode.Text.Trim(), null))
            {
                MessageBox.Show("An item with this code already exists", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtItemCode.Focus();
                return false;
            }

            // Validate numeric fields
            if (!double.TryParse(txtRate.Text, out double rate) || rate < 0)
            {
                MessageBox.Show("Please enter a valid Rate", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRate.Focus();
                return false;
            }

            if (!double.TryParse(txtCharges.Text, out double charges) || charges < 0)
            {
                MessageBox.Show("Please enter a valid Charges amount", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCharges.Focus();
                return false;
            }

            if (!double.TryParse(txtStockQuantity.Text, out double stock) || stock < 0)
            {
                MessageBox.Show("Please enter a valid Stock Quantity", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStockQuantity.Focus();
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