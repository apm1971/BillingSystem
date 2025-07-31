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
        private Bill currentBill;
        private List<Party> parties;
        private List<Item> items;
        private List<Broker> brokers;
        private bool isEditMode;
        private List<Party> filteredParties;
        private List<Broker> filteredBrokers;
        private bool isSearching = false;
        private bool isBrokerSearching = false;
        private bool isClearing = false; // Flag to prevent validation during form clearing

        public SaleBillUserControl(Bill? bill = null)
        {
            InitializeComponent();
            isEditMode = bill != null;
            currentBill = bill ?? new Bill();
            LoadData();
            SetupForm();
        }

        private void SaleBillUserControl_Load(object sender, EventArgs e)
        {
            LoadData();
            SetupForm();
        }

        private void LoadData()
        {
            try
            {
                parties = PartyService.GetAllParties();
                filteredParties = new List<Party>(parties);
                items = ItemService.GetAllItems();
                brokers = BrokerService.GetAllBrokers();
                filteredBrokers = new List<Broker>(brokers);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupForm()
        {
            // Configure text boxes
            txtBillNo.ReadOnly = true;
            txtBillNo.BackColor = Color.LightGray;
            
            // Configure additional charges text box
            txtAdditionalCharges.Text = "0.00";
            txtAdditionalCharges.TextAlign = HorizontalAlignment.Right;
            
            // Configure date text boxes
            txtBillDate.MaxLength = 10;
            txtBillDate.PlaceholderText = "dd-mm-yyyy";
            // txtDueDate.MaxLength = 10;
            // txtDueDate.PlaceholderText = "dd-mm-yyyy";
            
            // Set default bill date to today
            if (!isEditMode)
            {
                txtBillDate.Text = DateTime.Now.ToString("dd-MM-yyyy");
                // txtDueDate.Text = DateTime.Now.AddDays(30).ToString("dd-MM-yyyy");
            }

            // Setup combo boxes
            SetupComboBoxes();
            
            // Setup DataGridView
            SetupDataGridView();
            
            // Setup event handlers
            SetupEventHandlers();
            
            // Load bill data if editing
            if (isEditMode)
            {
                LoadBillData();
            }
            else
            {
                // Generate new bill number
                txtBillNo.Text = GenerateNewBillNumber();
            }
        }

        private void SetupComboBoxes()
        {
            // Setup party combo box
            cmbParty.DropDownStyle = ComboBoxStyle.DropDown;
            cmbParty.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbParty.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbParty.DataSource = parties;
            cmbParty.DisplayMember = "PartyName";
            cmbParty.ValueMember = "PartyID";
            cmbParty.SelectedIndex = -1;

            // Setup broker combo box
            var brokerList = new List<Broker> { new Broker { BrokerID = 0, BrokerName = "-- No Broker --" } };
            brokerList.AddRange(brokers);
            cmbBroker.DropDownStyle = ComboBoxStyle.DropDown;
            cmbBroker.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbBroker.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbBroker.DataSource = brokerList;
            cmbBroker.DisplayMember = "BrokerName";
            cmbBroker.ValueMember = "BrokerID";
            cmbBroker.SelectedValue = 0;
        }

        private void SetupDataGridView()
        {
            dgvItems.AutoGenerateColumns = false;
            dgvItems.AllowUserToAddRows = true;
            dgvItems.AllowUserToDeleteRows = true;
            dgvItems.RowHeadersVisible = true;
            dgvItems.RowHeadersWidth = 30;
            dgvItems.BackgroundColor = Color.White;
            dgvItems.BorderStyle = BorderStyle.Fixed3D;
            dgvItems.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvItems.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
            dgvItems.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(64, 64, 64);
            dgvItems.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvItems.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            dgvItems.EnableHeadersVisualStyles = false;
            dgvItems.GridColor = Color.LightGray;
            dgvItems.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgvItems.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Clear existing columns
            dgvItems.Columns.Clear();

            // Item Name (ComboBox)
            var itemColumn = new DataGridViewComboBoxColumn
            {
                Name = "ItemName",
                HeaderText = "Item",
                DataSource = items,
                DisplayMember = "ItemName",
                ValueMember = "ItemID",
                FillWeight = 200,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox,
                FlatStyle = FlatStyle.Flat
            };
            dgvItems.Columns.Add(itemColumn);

            // Quantity
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Quantity",
                HeaderText = "Quantity",
                FillWeight = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            // Rate
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Rate",
                HeaderText = "Rate",
                FillWeight = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            // Amount
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Amount",
                HeaderText = "Amount",
                FillWeight = 100,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, BackColor = Color.LightGray }
            });

            // Charges
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Charges",
                HeaderText = "Charges",
                FillWeight = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            // Total Amount
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TotalAmount",
                HeaderText = "Total",
                FillWeight = 100,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, BackColor = Color.LightGray, Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold) }
            });
        }

        private void SetupEventHandlers()
        {
            // Text changed events
            txtBillDate.TextChanged += TxtBillDate_TextChanged;
            txtAdditionalCharges.TextChanged += TxtAdditionalCharges_TextChanged;
            txtAdditionalCharges.Leave += TxtAdditionalCharges_Leave;
            
            // Combo box events
            cmbParty.SelectedIndexChanged += CmbParty_SelectedIndexChanged;
            cmbBroker.SelectedIndexChanged += CmbBroker_SelectedIndexChanged;
            
            // DataGridView events
            dgvItems.CellValueChanged += DgvItems_CellValueChanged;
            dgvItems.CellEndEdit += DgvItems_CellEndEdit;
            dgvItems.UserDeletedRow += DgvItems_UserDeletedRow;
            dgvItems.KeyDown += DgvItems_KeyDown;
            
            // Button events
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;
            btnAddItem.Click += BtnAddItem_Click;
            
            // Form events
            this.KeyDown += SaleBillUserControl_KeyDown;
        }

        private void TxtBillDate_TextChanged(object sender, EventArgs e)
        {
            // Auto-format date as user types
            if (txtBillDate.Text.Length == 2 && !txtBillDate.Text.Contains("-"))
            {
                txtBillDate.Text += "-";
                txtBillDate.SelectionStart = txtBillDate.Text.Length;
            }
            else if (txtBillDate.Text.Length == 5 && txtBillDate.Text.Count(c => c == '-') == 1)
            {
                txtBillDate.Text += "-";
                txtBillDate.SelectionStart = txtBillDate.Text.Length;
            }
        }

        private void TxtAdditionalCharges_TextChanged(object sender, EventArgs e)
        {
            // Recalculate totals when additional charges change
            CalculateTotals();
        }

        private void TxtAdditionalCharges_Leave(object sender, EventArgs e)
        {
            // Format the additional charges value when leaving the field
            if (decimal.TryParse(txtAdditionalCharges.Text, out decimal value))
            {
                txtAdditionalCharges.Text = value.ToString("0.00");
            }
            else
            {
                txtAdditionalCharges.Text = "0.00";
            }
        }

        // private void TxtDueDate_TextChanged(object sender, EventArgs e)
        // {
        //     // Auto-format date as user types
        //     if (txtDueDate.Text.Length == 2 && !txtDueDate.Text.Contains("-"))
        //     {
        //         txtDueDate.Text += "-";
        //         txtDueDate.SelectionStart = txtDueDate.Text.Length;
        //     }
        //     else if (txtDueDate.Text.Length == 5 && txtDueDate.Text.Count(c => c == '-') == 1)
        //     {
        //         txtDueDate.Text += "-";
        //         txtDueDate.SelectionStart = txtDueDate.Text.Length;
        //     }
        // }

        private void CmbParty_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbParty.SelectedValue is int partyId && partyId > 0)
            {
                var party = parties.FirstOrDefault(p => p.PartyID == partyId);
                if (party != null)
                {
                    // Update party details
                    lblPartyDetails.Text = $"Address: {party.Address}\nPhone: {party.Phone}\nBroker: {party.BrokerName}";
                    
                    // Auto-calculate due date based on party's credit days
                    // CalculateDueDate(party.CreditDays);
                    
                    // Auto-select broker if party has one
                    if (party.BrokerID.HasValue && party.BrokerID.Value > 0)
                    {
                        cmbBroker.SelectedValue = party.BrokerID.Value;
                    }
                    else
                    {
                        cmbBroker.SelectedValue = 0; // No Broker
                    }
                }
            }
            else
            {
                lblPartyDetails.Text = "Party details will appear here";
            }
        }

        private void CmbBroker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbBroker.SelectedValue is int brokerId && brokerId > 0)
            {
                var broker = brokers.FirstOrDefault(b => b.BrokerID == brokerId);
                if (broker != null)
                {
                    currentBill.BrokerID = broker.BrokerID;
                    currentBill.BrokerName = broker.BrokerName;
                }
            }
            else
            {
                currentBill.BrokerID = null;
                currentBill.BrokerName = string.Empty;
            }
        }



        private void DgvItems_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (e.RowIndex >= dgvItems.Rows.Count || dgvItems.Rows[e.RowIndex].IsNewRow) return;

            try
            {
                var row = dgvItems.Rows[e.RowIndex];
                var columnName = dgvItems.Columns[e.ColumnIndex].Name;

                // Handle ItemName selection - auto-fill rate and charges
                if (columnName == "ItemName" && row.Cells["ItemName"].Value != null)
                {
                    int itemId = Convert.ToInt32(row.Cells["ItemName"].Value);
                    var item = items.FirstOrDefault(i => i.ItemID == itemId);
                    if (item != null)
                    {
                        if (row.Cells["Quantity"].Value == null || Convert.ToDouble(row.Cells["Quantity"].Value) == 0)
                        {
                            row.Cells["Quantity"].Value = 1.0;
                        }
                        row.Cells["Rate"].Value = item.DefaultRate;
                        row.Cells["Charges"].Value = item.Charges;
                    }
                }

                CalculateRowTotal(e.RowIndex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in cell value changed: {ex.Message}");
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

        private void DgvItems_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F8 || (e.Control && e.KeyCode == Keys.D))
            {
                DeleteCurrentRow();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void CalculateRowTotal(int rowIndex)
        {
            try
            {
                if (rowIndex < 0 || rowIndex >= dgvItems.Rows.Count || dgvItems.Rows[rowIndex].IsNewRow)
                    return;

                var row = dgvItems.Rows[rowIndex];

                // Auto-fill item details when item is selected
                if (row.Cells["ItemName"].Value != null)
                {
                    int itemId = Convert.ToInt32(row.Cells["ItemName"].Value);
                    var item = items.FirstOrDefault(i => i.ItemID == itemId);
                    if (item != null)
                    {
                        if (row.Cells["Quantity"].Value == null || Convert.ToDouble(row.Cells["Quantity"].Value) == 0)
                        {
                            row.Cells["Quantity"].Value = 1.0;
                        }
                        if (row.Cells["Rate"].Value == null || Convert.ToDouble(row.Cells["Rate"].Value) == 0)
                        {
                            row.Cells["Rate"].Value = item.DefaultRate;
                        }
                        if (row.Cells["Charges"].Value == null)
                        {
                            row.Cells["Charges"].Value = item.Charges;
                        }
                    }
                }

                // Ensure numeric values
                if (row.Cells["Quantity"].Value == null) row.Cells["Quantity"].Value = 0;
                if (row.Cells["Rate"].Value == null) row.Cells["Rate"].Value = 0;
                if (row.Cells["Charges"].Value == null) row.Cells["Charges"].Value = 0;

                // Calculate amounts
                decimal quantity = Convert.ToDecimal(row.Cells["Quantity"].Value);
                decimal rate = Convert.ToDecimal(row.Cells["Rate"].Value);
                decimal charges = Convert.ToDecimal(row.Cells["Charges"].Value);

                decimal amount = quantity * rate;
                decimal totalAmount = amount + charges;

                row.Cells["Amount"].Value = Math.Round(amount, 2);
                row.Cells["TotalAmount"].Value = Math.Round(totalAmount, 2);

                CalculateTotals();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating row total: {ex.Message}");
            }
        }

        private void CalculateTotals()
        {
            decimal totalAmount = 0;
            decimal totalCharges = 0;
            decimal netAmount = 0;

            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (!row.IsNewRow)
                {
                    totalAmount += Convert.ToDecimal(row.Cells["Amount"].Value ?? 0);
                    totalCharges += Convert.ToDecimal(row.Cells["Charges"].Value ?? 0);
                    netAmount += Convert.ToDecimal(row.Cells["TotalAmount"].Value ?? 0);
                }
            }

            // Get additional charges from the text box
            decimal additionalCharges = 0;
            if (decimal.TryParse(txtAdditionalCharges.Text, out additionalCharges))
            {
                additionalCharges = Math.Round(additionalCharges, 2);
            }
            else
            {
                additionalCharges = 0;
                txtAdditionalCharges.Text = "0.00";
            }

            // Calculate final net amount including additional charges
            decimal finalNetAmount = netAmount + additionalCharges;

            // Update the current bill amounts
            currentBill.OriginalAmount = Math.Round(totalAmount, 2);
            currentBill.AdditionalCharges = Math.Round(additionalCharges, 2);

            // Update labels
            lblTotalAmount.Text = $"Total Amount: ₹{totalAmount:N2}";
            lblTotalCharges.Text = $"Total Charges: ₹{totalCharges:N2}";
            lblNetAmount.Text = $"NET AMOUNT: ₹{finalNetAmount:N2}";
        }

        private void DeleteCurrentRow()
        {
            try
            {
                if (dgvItems.CurrentCell != null)
                {
                    int rowIndex = dgvItems.CurrentCell.RowIndex;
                    if (rowIndex >= 0 && rowIndex < dgvItems.Rows.Count && !dgvItems.Rows[rowIndex].IsNewRow)
                    {
                        if (MessageBox.Show("Are you sure you want to delete this item?", "Confirm Delete",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            dgvItems.Rows[rowIndex].Selected = true;
                            dgvItems.Rows.RemoveAt(rowIndex);
                            CalculateTotals();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting item: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadBillData()
        {
            if (currentBill == null) return;

            txtBillNo.Text = currentBill.BillNo;
            txtBillDate.Text = currentBill.BillDate.ToString("dd-MM-yyyy");
            // txtDueDate.Text = currentBill.DueDate.ToString("dd-MM-yyyy");

            // Set party
            cmbParty.SelectedValue = currentBill.PartyID;

            // Set broker
            if (currentBill.BrokerID.HasValue)
            {
                cmbBroker.SelectedValue = currentBill.BrokerID.Value;
            }

            // Set additional charges
            txtAdditionalCharges.Text = currentBill.AdditionalCharges.ToString("0.00");

            // Load items
            foreach (var item in currentBill.BillItems)
            {
                int rowIndex = dgvItems.Rows.Add();
                DataGridViewRow row = dgvItems.Rows[rowIndex];
                
                // Set the ItemName column to the ItemID (for ComboBox selection)
                row.Cells["ItemName"].Value = item.ItemID;
                row.Cells["Quantity"].Value = item.Quantity;
                row.Cells["Rate"].Value = item.Rate;
                row.Cells["Amount"].Value = item.Amount;
                row.Cells["Charges"].Value = item.Charges;
                row.Cells["TotalAmount"].Value = item.TotalAmount;
            }

            CalculateTotals();
        }

        private string GenerateNewBillNumber()
        {
            return BillService.GenerateNewBillNumber();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
            {
                return;
            }

            try
            {
                // Parse dates
                if (!DateTime.TryParseExact(txtBillDate.Text, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime billDate))
                {
                    MessageBox.Show("Please enter a valid bill date in dd-mm-yyyy format.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtBillDate.Focus();
                    return;
                }

                // if (!DateTime.TryParseExact(txtDueDate.Text, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dueDate))
                // {
                //     MessageBox.Show("Please enter a valid due date in dd-mm-yyyy format.", "Validation Error", 
                //         MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //     txtDueDate.Focus();
                //     return;
                // }

                // Get data from form
                currentBill.BillNo = txtBillNo.Text;
                currentBill.BillDate = billDate;
                // currentBill.DueDate = dueDate;

                // Get party information
                if (cmbParty.SelectedValue is int partyId && partyId > 0)
                {
                    currentBill.PartyID = partyId;
                }
                else
                {
                    MessageBox.Show("Please select a party.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbParty.Focus();
                    return;
                }

                // Get broker information
                if (cmbBroker.SelectedValue is int brokerId && brokerId > 0)
                {
                    currentBill.BrokerID = brokerId;
                    currentBill.BrokerName = brokers.FirstOrDefault(b => b.BrokerID == brokerId)?.BrokerName ?? "";
                }
                else
                {
                    currentBill.BrokerID = null;
                    currentBill.BrokerName = null;
                }

                // Set other required properties
                currentBill.OriginalAmount = Convert.ToDecimal(lblTotalAmount.Text.Replace("Total Amount: ", "").Replace("₹", "").Trim());
                // Get additional charges from the input field, not the label
                decimal additionalCharges = 0;
                if (decimal.TryParse(txtAdditionalCharges.Text, out additionalCharges))
                {
                    currentBill.AdditionalCharges = Math.Round(additionalCharges, 2);
                }
                else
                {
                    currentBill.AdditionalCharges = 0;
                }
                currentBill.Status = "Unpaid"; // Default status for new bills
                currentBill.Notes = ""; // Can be enhanced later to include a notes field
                currentBill.CompanyID = 1; // Default company ID, should be configurable

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
                            Rate = Convert.ToDecimal(row.Cells["Rate"].Value ?? 0),
                            Amount = Convert.ToDecimal(row.Cells["Amount"].Value ?? 0),
                            Charges = Convert.ToDecimal(row.Cells["Charges"].Value ?? 0),
                            TotalAmount = Convert.ToDecimal(row.Cells["TotalAmount"].Value ?? 0),
                            CompanyID = currentBill.CompanyID
                        };
                        currentBill.BillItems.Add(billItem);
                    }
                }

                // Save to database
                int billID;
                if (BillService.SaveBill(currentBill, out billID))
                {
                    currentBill.BillID = billID;
                    MessageBox.Show("Bill saved successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Raise the BillSaved event
                    OnBillSaved(currentBill);
                    
                    // Clear form for new bill (only if not in edit mode)
                    if (!isEditMode)
                    {
                        ClearForm();
                    }
                    else
                    {
                        // In edit mode, just refresh the form to show updated data
                        isEditMode = false; // Reset edit mode
                    }
                }
                else
                {
                    MessageBox.Show("Failed to save bill. Please try again.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving bill: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInputs()
        {
            // Skip validation if form is being cleared
            if (isClearing)
            {
                return true;
            }
            
            if (string.IsNullOrWhiteSpace(txtBillNo.Text))
            {
                MessageBox.Show("Please enter a bill number.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBillNo.Focus();
                return false;
            }

            if (cmbParty.SelectedValue == null)
            {
                MessageBox.Show("Please select a party.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbParty.Focus();
                return false;
            }

            if (!DateTime.TryParseExact(txtBillDate.Text, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime billDate))
            {
                MessageBox.Show("Please enter a valid bill date in dd-mm-yyyy format.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBillDate.Focus();
                return false;
            }

            // if (!DateTime.TryParseExact(txtDueDate.Text, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dueDate))
            // {
            //     MessageBox.Show("Please enter a valid due date in dd-mm-yyyy format.", "Validation Error", 
            //         MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //     txtDueDate.Focus();
            //     return false;
            // }

            // if (dueDate < billDate)
            // {
            //     MessageBox.Show("Due date cannot be earlier than bill date.", "Validation Error", 
            //         MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //     txtDueDate.Focus();
            //     return false;
            // }

            int itemCount = 0;
            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (!row.IsNewRow && row.Cells["ItemName"].Value != null)
                {
                    itemCount++;
                    decimal quantity = Convert.ToDecimal(row.Cells["Quantity"].Value ?? 0);
                    if (quantity <= 0)
                    {
                        MessageBox.Show($"Quantity cannot be zero or negative for item: {items.FirstOrDefault(i => i.ItemID == Convert.ToInt32(row.Cells["ItemName"].Value))?.ItemName}", 
                            "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        dgvItems.CurrentCell = row.Cells["Quantity"];
                        dgvItems.BeginEdit(true);
                        return false;
                    }

                    decimal rate = Convert.ToDecimal(row.Cells["Rate"].Value ?? 0);
                    if (rate <= 0)
                    {
                        MessageBox.Show($"Rate cannot be zero or negative for item: {items.FirstOrDefault(i => i.ItemID == Convert.ToInt32(row.Cells["ItemName"].Value))?.ItemName}", 
                            "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        dgvItems.CurrentCell = row.Cells["Rate"];
                        dgvItems.BeginEdit(true);
                        return false;
                    }
                }
            }

            if (itemCount == 0)
            {
                MessageBox.Show("Please add at least one item.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dgvItems.Focus();
                return false;
            }

            return true;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void BtnAddItem_Click(object sender, EventArgs e)
        {
            int newRowIndex = dgvItems.Rows.Add();
            dgvItems.CurrentCell = dgvItems.Rows[newRowIndex].Cells["ItemName"];
            dgvItems.BeginEdit(true);
        }

        private void ClearForm()
        {
            isClearing = true; // Set flag to prevent validation during clearing
            
            try
            {
                currentBill = new Bill();
                txtBillNo.Text = GenerateNewBillNumber();
                txtBillDate.Text = DateTime.Now.ToString("dd-MM-yyyy");
                // txtDueDate.Text = DateTime.Now.AddDays(30).ToString("dd-MM-yyyy");
                cmbParty.SelectedIndex = -1;
                cmbBroker.SelectedValue = 0;
                lblPartyDetails.Text = "Party details will appear here";
                txtAdditionalCharges.Text = "0.00";
                dgvItems.Rows.Clear();
                CalculateTotals();
            }
            finally
            {
                isClearing = false; // Reset flag after clearing
            }
        }

        private void SaleBillUserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                BtnSave_Click(sender, e);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                BtnCancel_Click(sender, e);
                e.Handled = true;
            }
        }

        // Public event to notify the parent form when a bill is saved
        public event EventHandler<BillSavedEventArgs> BillSaved;

        // Custom EventArgs for passing the saved Bill
        public class BillSavedEventArgs : EventArgs
        {
            public Bill SavedBill { get; }
            public BillSavedEventArgs(Bill bill)
            {
                SavedBill = bill;
            }
        }

        // Method to call when a bill is saved
        private void OnBillSaved(Bill bill)
        {
            BillSaved?.Invoke(this, new BillSavedEventArgs(bill));
        }
    }
} 