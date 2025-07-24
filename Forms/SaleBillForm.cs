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
        private ContextMenuStrip gridContextMenu;
        private Button btnViewPayments; // Add button field

        // Add fields for searchable party dropdown
        private List<Party> filteredParties;
        private bool isSearching = false;
        private string lastSearchText = string.Empty;

        // Add fields for searchable broker dropdown
        private List<Broker> filteredBrokers;
        private bool isBrokerSearching = false;
        private string lastBrokerSearchText = string.Empty;


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
                filteredParties = new List<Party>(parties); // Initialize filtered parties
                items = ItemService.GetAllItems();
                brokers = BrokerService.GetAllBrokers();
                filteredBrokers = new List<Broker>(brokers); // Initialize filtered brokers
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
                DropDownStyle = ComboBoxStyle.DropDown
            };

            // Add View Payments button
            btnViewPayments = new Button
            {
                Text = "View Payments",
                Location = new Point(550, 109),
                Size = new Size(120, 23),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold),
                Visible = isEditMode // Only show in edit mode
            };
            btnViewPayments.Click += BtnViewPayments_Click;
            
            // Add controls to groupBox1
            groupBox1.Controls.AddRange(new Control[] {
                lblDueDate, dtpDueDate, lblBroker, cmbBroker, btnViewPayments
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
            cmbParty.TextChanged += CmbParty_TextChanged; // Add search functionality
            cmbParty.KeyDown += CmbParty_KeyDown; // Handle special keys
            cmbBroker.SelectedIndexChanged += CmbBroker_SelectedIndexChanged;
            cmbBroker.TextChanged += CmbBroker_TextChanged; // Add search functionality
            cmbBroker.KeyDown += CmbBroker_KeyDown; // Handle special keys
            dtpBillDate.ValueChanged += DtpBillDate_ValueChanged;
            dgvItems.CellValueChanged += DgvItems_CellValueChanged;
            dgvItems.CellEndEdit += DgvItems_CellEndEdit;
            dgvItems.UserDeletedRow += DgvItems_UserDeletedRow;
            dgvItems.DefaultValuesNeeded += DgvItems_DefaultValuesNeeded;
            dgvItems.DataError += DgvItems_DataError;
            dgvItems.RowsAdded += DgvItems_RowsAdded;
            dgvItems.KeyDown += DgvItems_KeyDown;
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
            else if (e.KeyCode == Keys.F8 || (e.Control && e.KeyCode == Keys.D))
            {
                // F8 or Ctrl+D: Delete current row if grid is focused
                if (dgvItems.Focused || dgvItems.ContainsFocus)
                {
                    DeleteCurrentRow();
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
                "F8 or Ctrl+D: Delete Current Row (in Items Grid)\n" +
                "Right-Click: Show Context Menu for Row Operations\n" +
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
            // Use the new searchable setup method
            SetupSearchableBrokerComboBox();
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

        private void CmbBroker_TextChanged(object sender, EventArgs e)
        {
            if (cmbBroker.SelectedValue is int brokerId)
            {
                currentBill.BrokerID = brokerId > 0 ? brokerId : (int?)null;
                currentBill.BrokerName = brokerId > 0 ? cmbBroker.Text : string.Empty;
            }
        }

        private void CmbBroker_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                // If there's exactly one filtered broker, select it
                case Keys.Enter:
                    e.SuppressKeyPress = true;
                    e.Handled = true;


                     // If a broker is selected or text matches exactly, move to next control
                    if (cmbBroker.SelectedValue is int brokerId)
                    {
                        dgvItems.Focus();
                        return;
                    }
                    
                    // If text matches a broker name exactly, select that broker
                    var matchingBroker = brokers.FirstOrDefault(b => 
                        b.BrokerName.Equals(cmbBroker.Text, StringComparison.OrdinalIgnoreCase));
                    if (matchingBroker != null)
                    {
                        cmbBroker.SelectedValue = matchingBroker.BrokerID;
                        dgvItems.Focus();
                        return;
                    }
                    
                    // Move focus to next control
                    dgvItems.Focus();

                    break;
                case Keys.Escape:
                    cmbBroker.SelectedValue = 0; // Select "No Broker"
                    cmbBroker.Text = "";
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    break;
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

            // Show View Payments button in edit mode
            btnViewPayments.Visible = true;

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
    if (isSearching) return; // Prevent interference during search operations
    
    if (cmbParty.SelectedValue is int partyId && partyId > 0)
    {
        var party = parties.FirstOrDefault(p => p.PartyID == partyId);
        if (party != null)
                {
                    SelectParty(party);

    }
    else
    {
        // Clear party details if no party selected
        lblPartyDetails.Text = "Party details will appear here";
    }
}
        }
        private void CmbParty_TextChanged(object sender, EventArgs e)
        {
    // If text is empty, clear party details
    if (string.IsNullOrWhiteSpace(cmbParty.Text))
    {
        lblPartyDetails.Text = "Party details will appear here";
        return;
    }

 // If a party is selected, update details
    if (cmbParty.SelectedValue is int partyId && partyId > 0)
    {
        var party = parties.FirstOrDefault(p => p.PartyID == partyId);
        if (party != null)
        {
            SelectParty(party);
        }
    }
}

private void SelectParty(Party party)
{
    if (party == null) return;
    
    // Temporarily disable search to prevent interference
    isSearching = true;
    try
    {
        cmbParty.SelectedValue = party.PartyID;
        cmbParty.Text = party.PartyName;
        
        // Update party details
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
    finally
    {
        isSearching = false;
    }
}
        private void CmbParty_KeyDown(object sender, KeyEventArgs e)
{
    switch (e.KeyCode)
    {
        case Keys.Enter:
            e.SuppressKeyPress = true;
            e.Handled = true;
            
            // If a party is selected, move to next control
            if (cmbParty.SelectedValue is int partyId && partyId > 0)
            {
                                var party = parties.FirstOrDefault(p => p.PartyID == partyId);
                if (party != null)
                {
                                        SelectParty(party);
                    dtpBillDate.Focus();
                }
                return;
            }
                        
            // If text matches a party name exactly, select that party
            var matchingParty = parties.FirstOrDefault(p => 
                p.PartyName.Equals(cmbParty.Text, StringComparison.OrdinalIgnoreCase));
            if (matchingParty != null)
            {
                                SelectParty(matchingParty);
                dtpBillDate.Focus();
                return;
            }
 
            
            // Move to next control anyway
            dtpBillDate.Focus();
            break;

        case Keys.Escape:
            cmbParty.Text = "";
            lblPartyDetails.Text = "Party details will appear here";
            e.SuppressKeyPress = true;
            e.Handled = true;
            break;
    }
}
 private void RefreshPartyDropdown()
{
    // Store current text and cursor position BEFORE any changes
    string currentText = cmbParty.Text;
    int currentCursorPosition = cmbParty.SelectionStart;
    
    // Temporarily remove event handlers to prevent interference
    cmbParty.TextChanged -= CmbParty_TextChanged;
    cmbParty.SelectedIndexChanged -= CmbParty_SelectedIndexChanged;
    
    try
    {
        // Update data source
        cmbParty.DataSource = null;
        cmbParty.DataSource = filteredParties;
        cmbParty.DisplayMember = "PartyName";
        cmbParty.ValueMember = "PartyID";
        
        // Restore the original text EXACTLY as user typed it
        cmbParty.Text = currentText;
        
        // Restore cursor position safely
        if (currentCursorPosition >= 0 && currentCursorPosition <= currentText.Length)
        {
            cmbParty.SelectionStart = currentCursorPosition;
            cmbParty.SelectionLength = 0;
        }
        
        // Show dropdown if there are filtered results and text is not empty
        if (filteredParties.Count > 0 && !string.IsNullOrWhiteSpace(currentText))
        {
            cmbParty.DroppedDown = true;
        }
    }
    finally
    {
        // Re-add event handlers
        cmbParty.TextChanged += CmbParty_TextChanged;
        cmbParty.SelectedIndexChanged += CmbParty_SelectedIndexChanged;
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
                if (columnName == "ItemName")
                {
                    if (row.Cells["ItemName"].Value != null)
                    {
                        int itemId = Convert.ToInt32(row.Cells["ItemName"].Value);
                        var item = items.FirstOrDefault(i => i.ItemID == itemId);
                        if (item != null)
                        {
                            // Initialize all values when an item is first selected
                            row.Cells["Quantity"].Value = 1.0;
                            row.Cells["Rate"].Value = item.Rate;
                            row.Cells["Charges"].Value = item.Charges;
                            row.Cells["Amount"].Value = item.Rate; // Quantity (1) * Rate
                            row.Cells["TotalAmount"].Value = item.Rate + item.Charges;
                        }
                    }
                    else
                    {
                        // Clear all values if item is deselected
                        row.Cells["Quantity"].Value = null;
                        row.Cells["Rate"].Value = null;
                        row.Cells["Charges"].Value = null;
                        row.Cells["Amount"].Value = null;
                        row.Cells["TotalAmount"].Value = null;
                    }
                }
                // Special handling for the Charges column to ensure 0 is accepted
                else if (columnName == "Charges")
                {
                    // Ensure we recalculate even if charges is set to 0
                    CalculateRowTotal(e.RowIndex);
                }
                else
                {
                    // Only calculate row total if we have an item selected
                    if (row.Cells["ItemName"].Value != null)
                    {
                        CalculateRowTotal(e.RowIndex);
                    }
                }
                
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
            // Don't set any default values for new rows
            // Values will be set when an item is selected
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
                        
                        // Only set charges from item if the cell is null (not if it's explicitly set to 0)
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
            // Setup party combo box with search functionality
            SetupSearchablePartyComboBox();

            // Setup broker combo box with search functionality
            SetupSearchableBrokerComboBox();
        }

         private void SetupSearchablePartyComboBox()
{
    // Configure party combo box
    cmbParty.DropDownStyle = ComboBoxStyle.DropDown;
    cmbParty.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
    cmbParty.AutoCompleteSource = AutoCompleteSource.ListItems;
    
    // Remove any existing handlers
    cmbParty.TextChanged -= CmbParty_TextChanged;
    cmbParty.SelectedIndexChanged -= CmbParty_SelectedIndexChanged;
    cmbParty.KeyDown -= CmbParty_KeyDown;
    
    // Add event handlers
    cmbParty.TextChanged += CmbParty_TextChanged;
    cmbParty.SelectedIndexChanged += CmbParty_SelectedIndexChanged;
    cmbParty.KeyDown += CmbParty_KeyDown;
    
    // Set up data source
    cmbParty.DataSource = null;
    cmbParty.DataSource = parties;
    cmbParty.DisplayMember = "PartyName";
    cmbParty.ValueMember = "PartyID";
    cmbParty.SelectedIndex = -1;
    cmbParty.Text = string.Empty;
}

       private void SetupSearchableBrokerComboBox()
        {
            // Configure broker combo box
            cmbBroker.DropDownStyle = ComboBoxStyle.DropDown;
            cmbBroker.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbBroker.AutoCompleteSource = AutoCompleteSource.ListItems;
            
            // Remove any existing handlers
            cmbBroker.TextChanged -= CmbBroker_TextChanged;
            cmbBroker.SelectedIndexChanged -= CmbBroker_SelectedIndexChanged;
            cmbBroker.KeyDown -= CmbBroker_KeyDown;
            
            // Add event handlers
            cmbBroker.TextChanged += CmbBroker_TextChanged;
            cmbBroker.SelectedIndexChanged += CmbBroker_SelectedIndexChanged;
            cmbBroker.KeyDown += CmbBroker_KeyDown;
            
            // Create list with "No Broker" option
            var brokerList = new List<Broker> { new Broker { BrokerID = 0, BrokerName = "-- No Broker --" } };
            brokerList.AddRange(brokers);
            
            // Set up data source
            cmbBroker.DataSource = null;
            cmbBroker.DataSource = brokerList;
            cmbBroker.DisplayMember = "BrokerName";
            cmbBroker.ValueMember = "BrokerID";
            cmbBroker.SelectedValue = 0;
            cmbBroker.Text = string.Empty;
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

            SetupGridContextMenu();
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
            dgvItems.KeyDown -= DgvItems_KeyDown;
            dgvItems.MouseClick -= DgvItems_MouseClick;
            dgvItems.EditingControlShowing -= DgvItems_EditingControlShowing;
            
            // Add event handlers
            dgvItems.CellValueChanged += DgvItems_CellValueChanged;
            dgvItems.CellEndEdit += DgvItems_CellEndEdit;
            dgvItems.UserDeletedRow += DgvItems_UserDeletedRow;
            dgvItems.KeyDown += DgvItems_KeyDown;
            dgvItems.MouseClick += DgvItems_MouseClick;
            dgvItems.EditingControlShowing += DgvItems_EditingControlShowing;
        }

        // Add this new method to handle ComboBox selection change immediately
        private void DgvItems_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dgvItems.CurrentCell.ColumnIndex == dgvItems.Columns["ItemName"].Index && e.Control is ComboBox comboBox)
            {
                // Remove previous event handler to avoid multiple subscriptions
                comboBox.SelectedIndexChanged -= ComboBox_SelectedIndexChanged;
                
                // Add event handler for SelectedIndexChanged
                comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
            }
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedValue != null)
            {
                // Get the current row
                int rowIndex = dgvItems.CurrentCell.RowIndex;
                if (rowIndex >= 0 && rowIndex < dgvItems.Rows.Count)
                {
                    var row = dgvItems.Rows[rowIndex];
                    
                    try
                    {
                        // Get the selected item
                        int itemId = Convert.ToInt32(comboBox.SelectedValue);
                        var item = items.FirstOrDefault(i => i.ItemID == itemId);
                        
                        if (item != null)
                        {
                            // Set values immediately without waiting for cell value changed event
                            row.Cells["Quantity"].Value = 1.0;
                            row.Cells["Rate"].Value = item.Rate;
                            row.Cells["Charges"].Value = item.Charges;
                            row.Cells["Amount"].Value = item.Rate; // Quantity (1) * Rate
                            row.Cells["TotalAmount"].Value = item.Rate + item.Charges;
                            
                            // Update totals
                            CalculateTotals();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in combo box selection: {ex.Message}");
                    }
                }
            }
        }

        private void SetupGridContextMenu()
        {
            // Create context menu
            gridContextMenu = new ContextMenuStrip();
            
            // Add "Delete Row" menu item
            var deleteItem = new ToolStripMenuItem("Delete Row");
            deleteItem.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            // Remove the icon that might be causing issues
            // deleteItem.Image = SystemIcons.Delete.ToBitmap();
            deleteItem.Click += DeleteMenuItem_Click;
            
            // Add "Add New Row" menu item
            var addItem = new ToolStripMenuItem("Add New Row");
            addItem.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular);
            addItem.Click += AddRowMenuItem_Click;
            
            // Add items to context menu
            gridContextMenu.Items.Add(deleteItem);
            gridContextMenu.Items.Add(new ToolStripSeparator());
            gridContextMenu.Items.Add(addItem);
            
            // Assign context menu to grid
            dgvItems.ContextMenuStrip = gridContextMenu;
        }

        private void DeleteMenuItem_Click(object sender, EventArgs e)
        {
            DeleteCurrentRow();
        }

        private void AddRowMenuItem_Click(object sender, EventArgs e)
        {
            int newRowIndex = dgvItems.Rows.Add();
            dgvItems.CurrentCell = dgvItems.Rows[newRowIndex].Cells["ItemName"];
            dgvItems.BeginEdit(true);
        }

        private void DgvItems_MouseClick(object sender, MouseEventArgs e)
        {
            // Show context menu on right-click
            if (e.Button == MouseButtons.Right)
            {
                // Get the row under the mouse
                DataGridView.HitTestInfo hitTest = dgvItems.HitTest(e.X, e.Y);
                if (hitTest.RowIndex >= 0 && hitTest.RowIndex < dgvItems.Rows.Count)
                {
                    dgvItems.ClearSelection();
                    dgvItems.Rows[hitTest.RowIndex].Selected = true;
                    
                    // Enable/disable delete option based on whether it's a new row
                    gridContextMenu.Items[0].Enabled = !dgvItems.Rows[hitTest.RowIndex].IsNewRow;
                    
                    // Show context menu
                    gridContextMenu.Show(dgvItems, e.Location);
                }
            }
        }

        private void DeleteCurrentRow()
        {
            try
            {
                // First check if we have a current cell selected
                if (dgvItems.CurrentCell != null)
                {
                    int rowIndex = dgvItems.CurrentCell.RowIndex;
                    
                    // Make sure it's a valid row (not the new row at the end)
                    if (rowIndex >= 0 && rowIndex < dgvItems.Rows.Count && !dgvItems.Rows[rowIndex].IsNewRow)
                    {
                        if (MessageBox.Show("Are you sure you want to delete this item?", "Confirm Delete",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            // Select the entire row to ensure proper deletion
                            dgvItems.Rows[rowIndex].Selected = true;
                            
                            // Remove the row by index
                            dgvItems.Rows.RemoveAt(rowIndex);
                            
                            // Recalculate totals
                            CalculateTotals();
                            
                            // Select another row if available
                            if (dgvItems.Rows.Count > 0 && rowIndex < dgvItems.Rows.Count)
                            {
                                dgvItems.CurrentCell = dgvItems.Rows[rowIndex].Cells[0];
                            }
                            else if (dgvItems.Rows.Count > 0 && rowIndex > 0)
                            {
                                dgvItems.CurrentCell = dgvItems.Rows[rowIndex - 1].Cells[0];
                            }
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

        private void DgvItems_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle F8 key for deletion
            if (e.KeyCode == Keys.F8 || (e.Control && e.KeyCode == Keys.D))
            {
                DeleteCurrentRow();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
            {
                return;
            }
            
            try
            {
                // Validate bill date is within financial year
                if (!CompanyService.IsDateWithinFinancialYear(dtpBillDate.Value.Date))
                {
                    MessageBox.Show("Bill date must be within the current financial year.", "Date Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dtpBillDate.Focus();
                    return;
                }
                
                // Get data from form
                currentBill.BillNo = txtBillNo.Text; // Add this line to set the bill number
                currentBill.BillDate = dtpBillDate.Value.Date;
                currentBill.DueDate = dtpDueDate.Value.Date;
                
                // Get party information
                if (cmbParty.SelectedValue is int partyId && partyId > 0)
                {
                    currentBill.PartyID = partyId;
                    currentBill.PartyName = cmbParty.Text;
                }
                else
                {
                    MessageBox.Show("Please select a party.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbParty.Focus();
                    return;
                }

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
                int billID;
                if (BillService.SaveBill(currentBill, out billID))
                {
                    currentBill.BillID = billID; // Update the bill ID
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

        private bool ValidateInputs()
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
                    double quantity = Convert.ToDouble(row.Cells["Quantity"].Value ?? 0);
                    if (quantity <= 0)
                    {
                        MessageBox.Show($"Quantity cannot be zero or negative for item: {items.FirstOrDefault(i => i.ItemID == Convert.ToInt32(row.Cells["ItemName"].Value))?.ItemName}", 
                            "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        dgvItems.CurrentCell = row.Cells["Quantity"];
                        dgvItems.BeginEdit(true);
                        return false;
                    }

                    // Check for zero rate
                    double rate = Convert.ToDouble(row.Cells["Rate"].Value ?? 0);
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
                filteredParties = new List<Party>(parties); // Update filtered parties
                
                // Update the combo box using searchable setup
                SetupSearchablePartyComboBox();
                
                // Restore selection if possible
                if (selectedPartyId != -1)
                {
                    cmbParty.SelectedValue = selectedPartyId;
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

        private void BtnViewPayments_Click(object sender, EventArgs e)
        {
            try
            {
                // Get all payments for this bill
                var payments = PaymentService.GetAllPayments()
                    .Where(p => p.PaymentDetails.Any(pd => pd.BillID == currentBill.BillID))
                    .OrderByDescending(p => p.PaymentDate)
                    .ToList();

                if (payments.Count == 0)
                {
                    MessageBox.Show("No payments found for this bill.", "Payment Details",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Create and show payment details form
                using (var form = new Form())
                {
                    form.Text = $"Payment Details - Bill #{currentBill.BillNo}";
                    form.StartPosition = FormStartPosition.CenterParent;
                    form.Size = new Size(800, 400);
                    form.MinimizeBox = false;
                    form.MaximizeBox = false;
                    form.FormBorderStyle = FormBorderStyle.FixedDialog;

                    // Create DataGridView for payments
                    var dgvPayments = new DataGridView
                    {
                        Dock = DockStyle.Fill,
                        AutoGenerateColumns = false,
                        AllowUserToAddRows = false,
                        AllowUserToDeleteRows = false,
                        ReadOnly = true,
                        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                        BackgroundColor = Color.White,
                        BorderStyle = BorderStyle.Fixed3D,
                        ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                        {
                            BackColor = Color.FromArgb(64, 64, 64),
                            ForeColor = Color.White,
                            Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold)
                        }
                    };

                    // Add columns
                    dgvPayments.Columns.AddRange(new DataGridViewColumn[]
                    {
                        new DataGridViewTextBoxColumn
                        {
                            Name = "PaymentDate",
                            HeaderText = "Payment Date",
                            DataPropertyName = "PaymentDate",
                            Width = 120,
                            DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
                        },
                        new DataGridViewTextBoxColumn
                        {
                            Name = "PaymentMethod",
                            HeaderText = "Payment Mode",
                            DataPropertyName = "PaymentMethod",
                            Width = 120
                        },
                        new DataGridViewTextBoxColumn
                        {
                            Name = "Reference",
                            HeaderText = "Reference",
                            DataPropertyName = "Reference",
                            Width = 120
                        },
                        new DataGridViewTextBoxColumn
                        {
                            Name = "AllocatedAmount",
                            HeaderText = "Amount",
                            Width = 120,
                            DefaultCellStyle = new DataGridViewCellStyle 
                            { 
                                Format = "N2",
                                Alignment = DataGridViewContentAlignment.MiddleRight
                            }
                        },
                        new DataGridViewTextBoxColumn
                        {
                            Name = "Interest",
                            HeaderText = "Interest",
                            Width = 100,
                            DefaultCellStyle = new DataGridViewCellStyle 
                            { 
                                Format = "N2",
                                Alignment = DataGridViewContentAlignment.MiddleRight
                            }
                        },
                        new DataGridViewTextBoxColumn
                        {
                            Name = "Discount",
                            HeaderText = "Discount",
                            Width = 100,
                            DefaultCellStyle = new DataGridViewCellStyle 
                            { 
                                Format = "N2",
                                Alignment = DataGridViewContentAlignment.MiddleRight
                            }
                        }
                    });

                    // Add rows
                    foreach (var payment in payments)
                    {
                        var detail = payment.PaymentDetails.First(pd => pd.BillID == currentBill.BillID);
                        var (interest, discount, _) = PaymentService.CalculateInterestAndDiscount(currentBill, payment.PaymentDate);
                        
                        dgvPayments.Rows.Add(
                            payment.PaymentDate,
                            payment.PaymentMethod,
                            payment.Reference,
                            detail.AllocatedAmount,
                            interest,
                            discount
                        );
                    }

                    // Add total row
                    var totalRow = dgvPayments.Rows[dgvPayments.Rows.Add()];
                    totalRow.DefaultCellStyle.BackColor = Color.LightGray;
                    totalRow.DefaultCellStyle.Font = new Font(dgvPayments.Font, FontStyle.Bold);
                    totalRow.Cells[0].Value = "Total";
                    totalRow.Cells[3].Value = payments.Sum(p => p.PaymentDetails.First(pd => pd.BillID == currentBill.BillID).AllocatedAmount);
                    totalRow.Cells[4].Value = payments.Sum(p => PaymentService.CalculateInterestAndDiscount(currentBill, p.PaymentDate).interestAmount);
                    totalRow.Cells[5].Value = payments.Sum(p => PaymentService.CalculateInterestAndDiscount(currentBill, p.PaymentDate).discountAmount);

                    form.Controls.Add(dgvPayments);
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error viewing payments: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 