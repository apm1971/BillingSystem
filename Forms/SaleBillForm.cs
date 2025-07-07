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
    public partial class SaleBillForm : Form
    {
        private Bill currentBill;
        private List<Party> parties;
        private List<Item> items;
        private bool isEditMode;

        public SaleBillForm(Bill? bill = null)
        {
            InitializeComponent();
            isEditMode = bill != null;
            currentBill = bill ?? new Bill();
            LoadData();
            SetupForm();
        }

        private void LoadData()
        {
            try
            {
                parties = PartyService.GetAllParties();
                items = ItemService.GetAllItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void SetupForm()
        {
            this.Text = isEditMode ? "Edit Sale Bill" : "New Sale Bill";
            
            // Setup party combo box
            cmbParty.DataSource = parties;
            cmbParty.DisplayMember = "PartyName";
            cmbParty.ValueMember = "PartyID";
            cmbParty.SelectedIndex = -1;

            // Setup item combo box in grid
            var itemColumn = (DataGridViewComboBoxColumn)dgvItems.Columns["ItemName"];
            itemColumn.DataSource = items;
            itemColumn.DisplayMember = "ItemName";
            itemColumn.ValueMember = "ItemID";

            // Load bill data if editing
            if (isEditMode)
            {
                LoadBillData();
            }
            else
            {
                // Generate new bill number
                txtBillNo.Text = GenerateNewBillNumber();
                dtpBillDate.Value = DateTime.Today;
            }

            // Setup event handlers
            cmbParty.SelectedIndexChanged += CmbParty_SelectedIndexChanged;
            dgvItems.CellValueChanged += DgvItems_CellValueChanged;
            dgvItems.CellEndEdit += DgvItems_CellEndEdit;
            dgvItems.UserDeletedRow += DgvItems_UserDeletedRow;
            
            // Set tooltips for add buttons
            var toolTip = new ToolTip();
            toolTip.SetToolTip(btnAddParty, "Add New Party");
            toolTip.SetToolTip(btnAddItem, "Add New Item");
        }

        private void LoadBillData()
        {
            txtBillNo.Text = currentBill.BillNo;
            dtpBillDate.Value = currentBill.BillDate;
            
            // Select party
            cmbParty.SelectedValue = currentBill.PartyID;
            
            // Load items
            foreach (var billItem in currentBill.BillItems)
            {
                int rowIndex = dgvItems.Rows.Add();
                var row = dgvItems.Rows[rowIndex];
                row.Cells["ItemName"].Value = billItem.ItemID;
                row.Cells["Quantity"].Value = billItem.Quantity;
                row.Cells["Rate"].Value = billItem.Rate;
                row.Cells["Amount"].Value = billItem.Amount;
                row.Cells["Charges"].Value = billItem.Charges;
                row.Cells["TotalAmount"].Value = billItem.TotalAmount;
            }
            
            CalculateTotals();
        }

        private string GenerateNewBillNumber()
        {
            return $"BILL-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmmss}";
        }

        private void CmbParty_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbParty.SelectedValue is int partyId)
            {
                var party = parties.FirstOrDefault(p => p.PartyID == partyId);
                if (party != null)
                {
                    lblPartyDetails.Text = $"{party.FullAddress}\n{party.ContactInfo}";
                }
            }
        }

        private void DgvItems_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                CalculateRowTotal(e.RowIndex);
            }
        }

        private void DgvItems_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                CalculateRowTotal(e.RowIndex);
            }
        }

        private void DgvItems_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            CalculateTotals();
        }

        private void CalculateRowTotal(int rowIndex)
        {
            try
            {
                var row = dgvItems.Rows[rowIndex];
                
                // Auto-fill item details when item is selected
                if (row.Cells["ItemName"].Value != null)
                {
                    int itemId = Convert.ToInt32(row.Cells["ItemName"].Value);
                    var item = items.FirstOrDefault(i => i.ItemID == itemId);
                    if (item != null)
                    {
                        if (row.Cells["Rate"].Value == null || Convert.ToDouble(row.Cells["Rate"].Value) == 0)
                        {
                            row.Cells["Rate"].Value = item.Rate;
                        }
                        if (row.Cells["Charges"].Value == null || Convert.ToDouble(row.Cells["Charges"].Value) == 0)
                        {
                            row.Cells["Charges"].Value = item.Charges;
                        }
                    }
                }

                // Calculate amounts
                double quantity = Convert.ToDouble(row.Cells["Quantity"].Value ?? 0);
                double rate = Convert.ToDouble(row.Cells["Rate"].Value ?? 0);
                double charges = Convert.ToDouble(row.Cells["Charges"].Value ?? 0);

                double amount = quantity * rate;
                double totalAmount = amount + charges;

                row.Cells["Amount"].Value = Math.Round(amount, 2);
                row.Cells["TotalAmount"].Value = Math.Round(totalAmount, 2);

                CalculateTotals();
            }
            catch (Exception ex)
            {
                // Handle conversion errors silently
            }
        }

        private void CalculateTotals()
        {
            double totalAmount = 0;
            double totalCharges = 0;
            double netAmount = 0;

            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (!row.IsNewRow)
                {
                    totalAmount += Convert.ToDouble(row.Cells["Amount"].Value ?? 0);
                    totalCharges += Convert.ToDouble(row.Cells["Charges"].Value ?? 0);
                    netAmount += Convert.ToDouble(row.Cells["TotalAmount"].Value ?? 0);
                }
            }

            lblTotalAmount.Text = $"Total Amount: {totalAmount:F2}";
            lblTotalCharges.Text = $"Total Charges: {totalCharges:F2}";
            lblNetAmount.Text = $"Net Amount: {netAmount:F2}";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateForm())
                    return;

                // Prepare bill data
                currentBill.BillNo = txtBillNo.Text;
                currentBill.BillDate = dtpBillDate.Value;
                currentBill.PartyID = Convert.ToInt32(cmbParty.SelectedValue);
                currentBill.PartyName = cmbParty.Text;

                // Clear existing items
                currentBill.BillItems.Clear();

                // Add items from grid
                foreach (DataGridViewRow row in dgvItems.Rows)
                {
                    if (!row.IsNewRow && row.Cells["ItemName"].Value != null)
                    {
                        var billItem = new BillItem
                        {
                            BillID = currentBill.BillID,
                            ItemID = Convert.ToInt32(row.Cells["ItemName"].Value),
                            ItemName = items.FirstOrDefault(i => i.ItemID == Convert.ToInt32(row.Cells["ItemName"].Value))?.ItemName ?? "",
                            Quantity = Convert.ToDouble(row.Cells["Quantity"].Value ?? 0),
                            Rate = Convert.ToDouble(row.Cells["Rate"].Value ?? 0),
                            Amount = Convert.ToDouble(row.Cells["Amount"].Value ?? 0),
                            Charges = Convert.ToDouble(row.Cells["Charges"].Value ?? 0),
                            TotalAmount = Convert.ToDouble(row.Cells["TotalAmount"].Value ?? 0)
                        };
                        currentBill.BillItems.Add(billItem);
                    }
                }

                // Calculate totals
                currentBill.CalculateTotals();

                // Save to database
                if (BillService.SaveBill(currentBill))
                {
                    MessageBox.Show("Bill saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to save bill. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving bill: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtBillNo.Text))
            {
                MessageBox.Show("Please enter a bill number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBillNo.Focus();
                return false;
            }

            if (cmbParty.SelectedValue == null)
            {
                MessageBox.Show("Please select a party.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbParty.Focus();
                return false;
            }

            int itemCount = 0;
            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (!row.IsNewRow && row.Cells["ItemName"].Value != null)
                {
                    itemCount++;
                }
            }

            if (itemCount == 0)
            {
                MessageBox.Show("Please add at least one item.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dgvItems.Focus();
                return false;
            }

            return true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnAddParty_Click(object sender, EventArgs e)
        {
            try
            {
                // Open the Quick Add Party form
                var quickAddForm = new QuickAddPartyForm();
                if (quickAddForm.ShowDialog() == DialogResult.OK)
                {
                    // Refresh the party list
                    RefreshPartyList();
                    
                    // Select the newly added party
                    if (quickAddForm.NewParty != null)
                    {
                        cmbParty.SelectedValue = quickAddForm.NewParty.PartyID;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Quick Add Party: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshPartyList()
        {
            try
            {
                // Remember the currently selected party
                int selectedPartyId = cmbParty.SelectedValue != null ? (int)cmbParty.SelectedValue : -1;
                
                // Reload parties from database
                parties = PartyService.GetAllParties();
                
                // Update the combo box
                cmbParty.DataSource = null;
                cmbParty.DataSource = parties;
                cmbParty.DisplayMember = "PartyName";
                cmbParty.ValueMember = "PartyID";
                
                // Restore selection if possible
                if (selectedPartyId != -1)
                {
                    cmbParty.SelectedValue = selectedPartyId;
                }
                else
                {
                    cmbParty.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing party list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAddItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Open the Quick Add Item form
                var quickAddForm = new QuickAddItemForm();
                if (quickAddForm.ShowDialog() == DialogResult.OK)
                {
                    // Refresh the items list
                    RefreshItemsList();
                    
                    // The newly added item will be available in the dropdown
                    // User can select it manually from the dropdown
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Quick Add Item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshItemsList()
        {
            try
            {
                // Reload items from database
                items = ItemService.GetAllItems();
                
                // Update the combo box in the grid
                var itemColumn = (DataGridViewComboBoxColumn)dgvItems.Columns["ItemName"];
                itemColumn.DataSource = null;
                itemColumn.DataSource = items;
                itemColumn.DisplayMember = "ItemName";
                itemColumn.ValueMember = "ItemID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing items list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 