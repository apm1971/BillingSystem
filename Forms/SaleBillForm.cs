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
        private DateTimePicker dtpDueDate;
        private Label lblDueDate;
        private ComboBox cmbBroker;
        private Label lblBroker;
        private List<Broker> brokers;


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
                brokers = BrokerService.GetAllBrokers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void SetupForm()
        {
            // Set form properties for responsive design
            this.Text = isEditMode ? "Edit Bill" : "New Bill";
            this.StartPosition = FormStartPosition.CenterParent;
            this.WindowState = FormWindowState.Normal; // Start normal instead of maximized
            this.Size = new Size(1000, 700); // Set default size
            this.MinimumSize = new Size(1000, 700);
            
            // Enable auto-scaling for different screen sizes
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            
            // Set up form resize handling
            this.Resize += SaleBillForm_Resize;
            
            // Add missing controls that aren't in Designer
            AddMissingControls();
            
            // Configure existing controls
            ConfigureExistingControls();
            
            // Setup responsive layout
            SetupResponsiveLayout();
            
            // Load combo boxes
            LoadComboBoxes();
            
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

        private void AddMissingControls()
        {
            // Add Due Date controls to groupBox1
            lblDueDate = new Label
            {
                Text = "Due Date:",
                Location = new Point(360, 57),
                Size = new Size(60, 15),
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular)
            };
            
            dtpDueDate = new DateTimePicker
            {
                Location = new Point(425, 53),
                Size = new Size(120, 23),
                Format = DateTimePickerFormat.Short
            };
            
            // Add Broker controls to groupBox1
            lblBroker = new Label
            {
                Text = "Broker:",
                Location = new Point(20, 112),
                Size = new Size(60, 15),
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular)
            };
            
            cmbBroker = new ComboBox
            {
                Location = new Point(100, 109),
                Size = new Size(250, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            
            // Add controls to groupBox1
            groupBox1.Controls.AddRange(new Control[] {
                lblDueDate, dtpDueDate, lblBroker, cmbBroker
            });
        }

        private void ConfigureExistingControls()
        {
            // Configure txtBillNo
            txtBillNo.ReadOnly = true;
            txtBillNo.BackColor = Color.LightGray;
            
            // Configure buttons with better styling
            btnSave.BackColor = Color.FromArgb(0, 122, 204);
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            
            btnCancel.BackColor = Color.FromArgb(204, 82, 0);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            
            // Configure labels with better styling
            lblTotalAmount.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            lblTotalCharges.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            
            // Ensure Net Amount is properly styled and initialized
            lblNetAmount.Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold);
            lblNetAmount.ForeColor = Color.FromArgb(0, 0, 192); // Deep blue color
            lblNetAmount.Text = "Net Amount: ₹0.00";
            
            // Setup DataGridView
            SetupDataGridView();
        }

        private void SetupResponsiveLayout()
        {
            // Set anchors for responsive behavior
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupBox3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            
            // Set button anchors
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            
            // Set up initial layout
            PerformResponsiveLayout();
        }

        private void SetupEventHandlers()
        {
            // Add event handlers
            cmbParty.SelectedIndexChanged += CmbParty_SelectedIndexChanged;
            cmbBroker.SelectedIndexChanged += CmbBroker_SelectedIndexChanged;
            dtpBillDate.ValueChanged += DtpBillDate_ValueChanged;
            dgvItems.CellValueChanged += DgvItems_CellValueChanged;
            dgvItems.CellEndEdit += DgvItems_CellEndEdit;
            dgvItems.UserDeletedRow += DgvItems_UserDeletedRow;
            dgvItems.DefaultValuesNeeded += DgvItems_DefaultValuesNeeded;
            dgvItems.DataError += DgvItems_DataError;
            dgvItems.RowsAdded += DgvItems_RowsAdded;
            this.KeyDown += SaleBillForm_KeyDown;
            this.KeyPreview = true;
        }

        private void SaleBillForm_Resize(object sender, EventArgs e)
        {
            PerformResponsiveLayout();
        }

        private void PerformResponsiveLayout()
        {
            if (this.Width < 1000 || this.Height < 700) return;
            
            // Calculate margins
            int margin = 12;
            int buttonHeight = 35;
            int groupBoxSpacing = 10;
            
            // Resize groupBox1 (header)
            groupBox1.Location = new Point(margin, margin);
            groupBox1.Size = new Size(this.Width - (margin * 2), 145); // Increased height for broker controls
            
            // Resize groupBox2 (items)
            int group2Top = groupBox1.Bottom + groupBoxSpacing;
            int group2Height = this.Height - group2Top - 160; // Reduced height to leave more space for bottom controls
            groupBox2.Location = new Point(margin, group2Top);
            groupBox2.Size = new Size(this.Width - (margin * 2), group2Height);
            
            // Resize DataGridView within groupBox2
            dgvItems.Location = new Point(15, 22);
            dgvItems.Size = new Size(groupBox2.Width - 70, groupBox2.Height - 35);
            
            // Position Add Item button
            btnAddItem.Location = new Point(groupBox2.Width - 50, 22);
            
            // Resize groupBox3 (totals)
            int group3Top = groupBox2.Bottom + groupBoxSpacing;
            groupBox3.Location = new Point(margin, group3Top);
            groupBox3.Size = new Size(this.Width - (margin * 2), 70);
            
            // Position total labels within groupBox3
            int labelWidth = 200;
            int labelSpacing = 20;

            lblTotalAmount.Location = new Point(labelSpacing, 25);
            lblTotalAmount.Size = new Size(labelWidth, 20);
            lblTotalAmount.TextAlign = ContentAlignment.MiddleLeft;

            // Position net amount label in the center with more width for better visibility
            lblNetAmount.Location = new Point((groupBox3.Width - 400) / 2, 20);
            lblNetAmount.Size = new Size(400, 30);
            lblNetAmount.TextAlign = ContentAlignment.MiddleCenter;

            // Position total charges label on the right
            lblTotalCharges.Location = new Point(groupBox3.Width - labelWidth - labelSpacing, 25);
            lblTotalCharges.Size = new Size(labelWidth, 20);
            lblTotalCharges.TextAlign = ContentAlignment.MiddleRight;
            
            // Position buttons below the totals group box, ensuring visibility
            int buttonY = groupBox3.Bottom + groupBoxSpacing;
            // Ensure buttons don't go beyond visible area - limit to form height minus some margin
            buttonY = Math.Min(buttonY, this.ClientSize.Height - buttonHeight - margin);
            
            btnCancel.Location = new Point(this.Width - margin - 120, buttonY);
            btnCancel.Size = new Size(100, buttonHeight);
            
            btnSave.Location = new Point(btnCancel.Left - 110, buttonY);
            btnSave.Size = new Size(100, buttonHeight);
            
            // Adjust party details label size and position
            lblPartyDetails.Location = new Point(550, 25);
            lblPartyDetails.Size = new Size(groupBox1.Width - 570, 105);
        }
        
        private void SetTabOrder()
        {
            // Set tab order for logical keyboard navigation
            txtBillNo.TabIndex = 0;
            dtpBillDate.TabIndex = 1;
            cmbParty.TabIndex = 2;
            dtpDueDate.TabIndex = 3;
            cmbBroker.TabIndex = 4;
            dgvItems.TabIndex = 5;
            btnSave.TabIndex = 6;
            btnCancel.TabIndex = 7;
            btnAddParty.TabIndex = 8;
            btnAddItem.TabIndex = 9;
        }
        
        private void SaleBillForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                // Ctrl+S: Save
                btnSave_Click(sender, e);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                // Escape: Cancel
                btnCancel_Click(sender, e);
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.Shift && e.KeyCode == Keys.P)
            {
                // Ctrl+Shift+P: Add Party
                btnAddParty_Click(sender, e);
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.Shift && e.KeyCode == Keys.I)
            {
                // Ctrl+Shift+I: Add Item
                btnAddItem_Click(sender, e);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F1)
            {
                // F1: Help
                ShowBillEntryHelp();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F2)
            {
                // F2: Focus on Party
                cmbParty.Focus();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F3)
            {
                // F3: Focus on Items grid
                dgvItems.Focus();
                if (dgvItems.Rows.Count > 0)
                {
                    dgvItems.CurrentCell = dgvItems.Rows[0].Cells[0];
                }
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.N)
            {
                // Ctrl+N: Add New row in items grid
                if (dgvItems.Focused)
                {
                    dgvItems.Rows.Add();
                    dgvItems.CurrentCell = dgvItems.Rows[dgvItems.Rows.Count - 1].Cells[0];
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                // Delete: Delete current row in items grid
                if (dgvItems.Focused && dgvItems.CurrentRow != null && !dgvItems.CurrentRow.IsNewRow)
                {
                    dgvItems.Rows.Remove(dgvItems.CurrentRow);
                    CalculateTotals();
                    e.SuppressKeyPress = true;
                }
            }
        }
        
        private void ShowBillEntryHelp()
        {
            MessageBox.Show(
                "Bill Entry Keyboard Shortcuts:\n\n" +
                "Ctrl+S: Save Bill\n" +
                "Escape: Cancel\n" +
                "Ctrl+Shift+P: Add New Party\n" +
                "Ctrl+Shift+I: Add New Item\n" +
                "F2: Focus on Party Selection\n" +
                "F3: Focus on Items Grid\n" +
                "Ctrl+N: Add New Row (in Items Grid)\n" +
                "Delete: Delete Current Row (in Items Grid)\n" +
                "F1: Show this help\n\n" +
                "Navigation:\n" +
                "Tab: Move to next field\n" +
                "Shift+Tab: Move to previous field\n" +
                "Enter: In grid, move to next cell\n" +
                "Arrow Keys: Navigate in grid\n" +
                "F4: Open dropdown (in combo boxes)",
                "Bill Entry Help",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }





        private void SetupBrokerComboBox()
        {
            // Create a list with an empty option
            var brokerList = new List<Broker> { new Broker { BrokerID = 0, BrokerName = "-- No Broker --" } };
            brokerList.AddRange(brokers);

            cmbBroker.DataSource = brokerList;
            cmbBroker.DisplayMember = "BrokerName";
            cmbBroker.ValueMember = "BrokerID";
            cmbBroker.SelectedValue = 0; // Default to "No Broker"

            // Add event handler
            cmbBroker.SelectedIndexChanged += CmbBroker_SelectedIndexChanged;
        }

        private void CmbBroker_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Update current bill's broker information
            if (cmbBroker.SelectedValue is int brokerID && brokerID > 0)
            {
                var broker = brokers.FirstOrDefault(b => b.BrokerID == brokerID);
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

        private void DtpBillDate_ValueChanged(object sender, EventArgs e)
        {
            // When bill date changes, recalculate due date if a party is selected
            if (cmbParty.SelectedValue is int partyId)
            {
                var party = parties.FirstOrDefault(p => p.PartyID == partyId);
                if (party != null)
                {
                    CalculateDueDate(party.CreditDays);
                }
            }
        }

        private void LoadBillData()
        {
            if (currentBill == null) return;

            txtBillNo.Text = currentBill.BillNo;
            dtpBillDate.Value = currentBill.BillDate;
            dtpDueDate.Value = currentBill.DueDate;

            // Set party
            cmbParty.SelectedValue = currentBill.PartyID;

            // Set broker
            if (currentBill.BrokerID.HasValue)
            {
                cmbBroker.SelectedValue = currentBill.BrokerID.Value;
            }

            // Load items
            foreach (var item in currentBill.BillItems)
            {
                int rowIndex = dgvItems.Rows.Add();
                DataGridViewRow row = dgvItems.Rows[rowIndex];
                
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

        private void CmbParty_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbParty.SelectedValue is int partyId)
            {
                var party = parties.FirstOrDefault(p => p.PartyID == partyId);
                if (party != null)
                {
                    var brokerInfo = !string.IsNullOrEmpty(party.BrokerName) ? $"\n{party.BrokerInfo}" : "";
                    lblPartyDetails.Text = $"{party.FullAddress}\n{party.ContactInfo}\nCredit Days: {party.CreditDays}{brokerInfo}";
                    
                    // Auto-calculate due date based on bill date and party's credit days
                    CalculateDueDate(party.CreditDays);
                    
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
        }

        private void CalculateDueDate(int creditDays)
        {
            try
            {
                // Calculate due date as bill date + credit days
                var dueDate = dtpBillDate.Value.AddDays(creditDays);
                
                // Set the due date in the control and currentBill
                dtpDueDate.Value = dueDate;
                currentBill.DueDate = dueDate;
            }
            catch (Exception ex)
            {
                // Handle errors silently for now
                var defaultDueDate = dtpBillDate.Value.AddDays(30); // Default to 30 days
                dtpDueDate.Value = defaultDueDate;
                currentBill.DueDate = defaultDueDate;
            }
        }

        private void DgvItems_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Ignore events for header row or out of range
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;
            
            // Ignore the "new row" at the end
            if (e.RowIndex >= dgvItems.Rows.Count || dgvItems.Rows[e.RowIndex].IsNewRow)
                return;
            
            try
            {
                var row = dgvItems.Rows[e.RowIndex];
                var columnName = dgvItems.Columns[e.ColumnIndex].Name;
                
                // Handle ItemName selection specifically - auto-fill rate and charges
                if (columnName == "ItemName" && row.Cells["ItemName"].Value != null)
                {
                    int itemId = Convert.ToInt32(row.Cells["ItemName"].Value);
                    var item = items.FirstOrDefault(i => i.ItemID == itemId);
                    if (item != null)
                    {
                        // Set default quantity if it's not already set
                        if (row.Cells["Quantity"].Value == null || Convert.ToDouble(row.Cells["Quantity"].Value) == 0)
                        {
                            row.Cells["Quantity"].Value = 1.0;
                        }
                        
                        // Set item rate and charges
                        row.Cells["Rate"].Value = item.Rate;
                        row.Cells["Charges"].Value = item.Charges;
                    }
                }
                
                // Calculate row totals
                CalculateRowTotal(e.RowIndex);
                
                // If this is the last non-new row and we're adding a new item, tab to the next row automatically
                if (e.RowIndex == dgvItems.Rows.Count - 2 && columnName == "TotalAmount")
                {
                    // Add focus to the new row if needed
                    if (dgvItems.Rows.Count < 20) // Limit automatic row addition
                    {
                        try
                        {
                            dgvItems.CurrentCell = dgvItems.Rows[e.RowIndex + 1].Cells["ItemName"];
                            dgvItems.BeginEdit(true);
                        }
                        catch
                        {
                            // Ignore errors
                        }
                    }
                }
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

        private void DgvItems_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // Prevent error dialogs from showing
            e.ThrowException = false;
            
            // Try to set the cell value to null or default
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
            {
                var column = dgvItems.Columns[e.ColumnIndex];
                if (column != null)
                {
                    try
                    {
                        if (column.Name == "ItemName")
                        {
                            dgvItems.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = null;
                        }
                        else if (column.Name == "Quantity" || column.Name == "Rate" || column.Name == "Charges")
                        {
                            dgvItems.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
                        }
                    }
                    catch
                    {
                        // Ignore any errors in error handling
                    }
                }
            }
        }

        private void DgvItems_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells["Quantity"].Value = 1.0;
            e.Row.Cells["Rate"].Value = 0.0;
            e.Row.Cells["Charges"].Value = 0.0;
            e.Row.Cells["Amount"].Value = 0.0;
            e.Row.Cells["TotalAmount"].Value = 0.0;
        }

        private void DgvItems_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            // Focus on the item cell of the new row for easier data entry
            if (e.RowIndex == dgvItems.Rows.Count - 2) // Second to last row (last is the "new row" placeholder)
            {
                try
                {
                    dgvItems.CurrentCell = dgvItems.Rows[e.RowIndex].Cells["ItemName"];
                    dgvItems.BeginEdit(true);
                }
                catch
                {
                    // Ignore errors
                }
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
                        // Set default quantity to 1 if it's empty or 0
                        if (row.Cells["Quantity"].Value == null || Convert.ToDouble(row.Cells["Quantity"].Value) == 0)
                        {
                            row.Cells["Quantity"].Value = 1.0;
                        }
                        
                        // Set rate from item if empty
                        if (row.Cells["Rate"].Value == null || Convert.ToDouble(row.Cells["Rate"].Value) == 0)
                        {
                            row.Cells["Rate"].Value = item.Rate;
                        }
                        
                        // Set charges from item if empty
                        if (row.Cells["Charges"].Value == null || Convert.ToDouble(row.Cells["Charges"].Value) == 0)
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
                double quantity = Convert.ToDouble(row.Cells["Quantity"].Value);
                double rate = Convert.ToDouble(row.Cells["Rate"].Value);
                double charges = Convert.ToDouble(row.Cells["Charges"].Value);

                double amount = quantity * rate;
                double totalAmount = amount + charges;

                row.Cells["Amount"].Value = Math.Round(amount, 2);
                row.Cells["TotalAmount"].Value = Math.Round(totalAmount, 2);

                CalculateTotals();
            }
            catch (Exception ex)
            {
                // Log error but don't show message box to prevent disrupting user experience
                System.Diagnostics.Debug.WriteLine($"Error calculating row total: {ex.Message}");
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

            // Update the current bill amounts
            currentBill.TotalAmount = Math.Round(totalAmount, 2);
            currentBill.TotalCharges = Math.Round(totalCharges, 2);
            currentBill.NetAmount = Math.Round(netAmount, 2);

            // Update labels with proper formatting
            lblTotalAmount.Text = $"Total Amount: ₹{totalAmount:N2}";
            lblTotalCharges.Text = $"Total Charges: ₹{totalCharges:N2}";
            
            // Make net amount more prominent with explicit value
            lblNetAmount.Text = $"NET AMOUNT: ₹{netAmount:N2}";
            
            // Ensure visibility by setting a minimum width based on content
            using (Graphics g = lblNetAmount.CreateGraphics())
            {
                SizeF textSize = g.MeasureString(lblNetAmount.Text, lblNetAmount.Font);
                lblNetAmount.MinimumSize = new Size((int)textSize.Width + 20, lblNetAmount.Height);
            }
            
            // Force immediate refresh of the labels
            lblTotalAmount.Refresh();
            lblTotalCharges.Refresh();
            lblNetAmount.Refresh();
        }

        private void LoadComboBoxes()
        {
            // Setup party combo box
            cmbParty.DataSource = parties;
            cmbParty.DisplayMember = "PartyName";
            cmbParty.ValueMember = "PartyID";
            cmbParty.SelectedIndex = -1;

            // Setup broker combo box
            SetupBrokerComboBox();
        }

        private void SetupDataGridView()
        {
            dgvItems.AutoGenerateColumns = false;
            dgvItems.AllowUserToAddRows = true;
            dgvItems.AllowUserToDeleteRows = true;
            dgvItems.RowHeadersVisible = false;
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

            // Remove previous handlers to prevent duplicate subscriptions
            dgvItems.CellValueChanged -= DgvItems_CellValueChanged;
            dgvItems.CellEndEdit -= DgvItems_CellEndEdit;
            dgvItems.UserDeletedRow -= DgvItems_UserDeletedRow;
            
            // Add event handlers
            dgvItems.CellValueChanged += DgvItems_CellValueChanged;
            dgvItems.CellEndEdit += DgvItems_CellEndEdit;
            dgvItems.UserDeletedRow += DgvItems_UserDeletedRow;
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
                currentBill.DueDate = dtpDueDate.Value; // Read due date from control
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

            if (dtpDueDate.Value < dtpBillDate.Value)
            {
                MessageBox.Show("Due date cannot be earlier than bill date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpDueDate.Focus();
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