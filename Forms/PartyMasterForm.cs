using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace SaleBillSystem.NET.Forms
{
    public partial class PartyMasterUserControl  : UserControl
    {
        private List<Party> parties = new List<Party>();
        private List<Broker> brokers = new List<Broker>();
        private Party currentParty = new Party();
        private bool isNewParty = true;

        // No need for isDialog field or constructor parameter in a UserControl
        public PartyMasterUserControl()
        {
            InitializeComponent();
            LoadParties();
            LoadBrokers();

            // Handle key events for the UserControl
            this.KeyDown += PartyMasterUserControl_KeyDown;
        }

        private void PartyMasterUserControl_Load(object sender, EventArgs e)
        {
            ConfigureControls();
            SetupDataGrid();
            ClearForm();
        }

        private void ConfigureControls()
        {
            // Set up Tab order
            txtSearch.TabIndex = 0;
            txtPartyName.TabIndex = 1;
            txtAddress.TabIndex = 2;
            txtPhone.TabIndex = 3;
            cmbBroker.TabIndex = 4;
            btnSave.TabIndex = 5;
            btnNew.TabIndex = 6;
            btnDelete.TabIndex = 7;
            
            // Set up text fields to use uppercase
            txtPartyName.CharacterCasing = CharacterCasing.Upper;
            txtAddress.CharacterCasing = CharacterCasing.Upper;
            txtPhone.CharacterCasing = CharacterCasing.Upper;

            // Add KeyDown event handlers for all input controls
            txtSearch.KeyDown += txtSearch_KeyDown;
            txtPartyName.KeyDown += Control_KeyDown;
            txtAddress.KeyDown += Control_KeyDown;
            txtPhone.KeyDown += Control_KeyDown;
            cmbBroker.KeyDown += Control_KeyDown;
            dgvParties.KeyDown += Control_KeyDown;
            
            // Set up form controls
            txtSearch.PlaceholderText = "Type to search parties...";
            txtPartyName.PlaceholderText = "Enter party name";
            txtAddress.PlaceholderText = "Enter address";
            txtPhone.PlaceholderText = "Enter phone number";
            
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
            toolTip.SetToolTip(btnSave, "Save the current party (Ctrl+S)");
            toolTip.SetToolTip(btnNew, "Create a new party (Ctrl+N)");
            toolTip.SetToolTip(btnDelete, "Delete the selected party (F8)");
            toolTip.SetToolTip(txtSearch, "Search parties by name, phone, address, or broker (F3)");
            toolTip.SetToolTip(txtPartyName, "Enter party name (F2)");
            
            // Ensure keyboard events are captured from all controls
            // Note: KeyPreview is not available for UserControl, but we handle key events directly
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

        private List<Party> filteredParties = new List<Party>();

        private void SetupDataGrid()
        {
            // Configure data grid
            dgvParties.AutoGenerateColumns = false;
            dgvParties.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvParties.AllowUserToAddRows = false;
            dgvParties.AllowUserToDeleteRows = false;
            dgvParties.ReadOnly = true;
            dgvParties.MultiSelect = false;
            dgvParties.RowHeadersVisible = false;
            dgvParties.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = System.Drawing.Color.FromArgb(245, 245, 245)
            };

            // Set modern font and styling
            dgvParties.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            dgvParties.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            dgvParties.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            dgvParties.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dgvParties.ColumnHeadersHeight = 35;
            dgvParties.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Set row height for better readability
            dgvParties.RowTemplate.Height = 30;

            // Clear existing columns
            dgvParties.Columns.Clear();

            // Add columns with proper sizing
            dgvParties.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PartyID",
                HeaderText = "ID",
                Width = 60,
                Visible = false
            });

            dgvParties.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PartyName",
                HeaderText = "Party Name",
                Width = 250,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dgvParties.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Phone",
                HeaderText = "Phone",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleLeft }
            });

            dgvParties.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Address",
                HeaderText = "Address",
                Width = 200,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleLeft }
            });

            dgvParties.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "BrokerName",
                HeaderText = "Broker",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleLeft }
            });

            // Enable double buffering for smooth scrolling
            typeof(DataGridView).InvokeMember("DoubleBuffered", 
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, dgvParties, new object[] { true });
        }

        private void LoadParties()
        {
            try
            {
                parties = PartyService.GetAllParties();
                
                dgvParties.DataSource = null;
                dgvParties.DataSource = parties;

                lblTotalParties.Text = $"Total Parties: {parties.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading parties: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadBrokers()
        {
            try
            {
                            brokers = BrokerService.GetAllBrokers();
                SetupBrokerComboBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading brokers: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupBrokerComboBox()
        {
            if (cmbBroker != null)
            {
                // Create a list with an empty option
                var brokerList = new List<Broker> { new Broker { BrokerID = 0, BrokerName = "-- No Broker --" } };
                brokerList.AddRange(brokers);

                // First ensure DropDownStyle is set to DropDown (not DropDownList) 
                // before setting AutoComplete properties
                cmbBroker.DropDownStyle = ComboBoxStyle.DropDown;
                
                // Set up autocomplete for the broker combo box
                cmbBroker.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                cmbBroker.AutoCompleteSource = AutoCompleteSource.ListItems;
                
                // Then set the data source
                cmbBroker.DataSource = brokerList;
                cmbBroker.DisplayMember = "BrokerName";
                cmbBroker.ValueMember = "BrokerID";
                cmbBroker.SelectedValue = 0; // Default to "No Broker"
                
                // Add button for quickly adding a new broker next to the broker combo box
                var btnQuickAddBroker = new Button
                {
                    Text = "+",
                    Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold),
                    Size = new System.Drawing.Size(30, 21),
                    Location = new System.Drawing.Point(cmbBroker.Right + 5, cmbBroker.Top),
                    TabIndex = cmbBroker.TabIndex + 1
                };
                btnQuickAddBroker.Click += BtnQuickAddBroker_Click;
                this.Controls.Add(btnQuickAddBroker);
            }
        }

        private void BtnQuickAddBroker_Click(object sender, EventArgs e)
        {
            using (var quickAddBrokerForm = new QuickAddBrokerForm())
            {
                if (quickAddBrokerForm.ShowDialog() == DialogResult.OK)
                {
                    // Refresh brokers list and select the newly added broker
                    ReloadBrokers(quickAddBrokerForm.NewBroker?.BrokerID);
                }
            }
        }

        private void ReloadBrokers(int? selectBrokerId = null)
        {
            // Store current selection if we're not selecting a specific broker
            int? currentBrokerId = selectBrokerId;
            if (!currentBrokerId.HasValue && cmbBroker.SelectedValue is int selectedBrokerId)
            {
                currentBrokerId = selectedBrokerId;
            }
            
            // Reload brokers
            brokers = BrokerService.GetAllBrokers();
            
            // Temporarily clear AutoComplete settings to avoid errors when changing DataSource
            cmbBroker.AutoCompleteMode = AutoCompleteMode.None;
            
            // Reset data source
            cmbBroker.DataSource = null;
            
            // Create a list with an empty option
            var brokerList = new List<Broker> { new Broker { BrokerID = 0, BrokerName = "-- No Broker --" } };
            brokerList.AddRange(brokers);
            
            // Make sure DropDownStyle is correct
            cmbBroker.DropDownStyle = ComboBoxStyle.DropDown;
            
            // Set the data source
            cmbBroker.DataSource = brokerList;
            cmbBroker.DisplayMember = "BrokerName";
            cmbBroker.ValueMember = "BrokerID";
            
            // Restore selected item if possible
            if (currentBrokerId.HasValue)
            {
                cmbBroker.SelectedValue = currentBrokerId.Value;
            }
            
            // Restore AutoComplete settings
            cmbBroker.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbBroker.AutoCompleteSource = AutoCompleteSource.ListItems;
        }

        private void ClearForm()
        {
            currentParty = new Party();
            isNewParty = true;

            txtPartyName.Text = string.Empty;
            txtAddress.Text = string.Empty;
            txtPhone.Text = string.Empty;

            // Reset broker selection
            if (cmbBroker != null)
            {
                cmbBroker.SelectedValue = 0; // No Broker
            }

            txtPartyName.Focus();
            btnDelete.Enabled = false;
        }

        private void PopulateForm(Party party)
        {
            currentParty = party;
            isNewParty = false;

            txtPartyName.Text = party.PartyName;
            txtAddress.Text = party.Address;
            txtPhone.Text = party.Phone;

            // Set broker selection
            if (cmbBroker != null)
            {
                if (party.BrokerID.HasValue && party.BrokerID.Value > 0)
                {
                    cmbBroker.SelectedValue = party.BrokerID.Value;
                }
                else
                {
                    cmbBroker.SelectedValue = 0; // No Broker
                }
            }

            btnDelete.Enabled = true;
        }

        private Party GetPartyFromForm()
        {
            Party party = new Party
            {
                PartyID = currentParty.PartyID,
                PartyName = txtPartyName.Text.Trim().ToUpper(),
                Address = txtAddress.Text.Trim().ToUpper(),
                Phone = txtPhone.Text.Trim().ToUpper(),
                CompanyID = Program.ActiveCompany?.CompanyID ?? 1
            };

            // Set broker information
            if (cmbBroker != null && cmbBroker.SelectedValue is int brokerID && brokerID > 0)
            {
                var broker = brokers.FirstOrDefault(b => b.BrokerID == brokerID);
                if (broker != null)
                {
                    party.BrokerID = broker.BrokerID;
                }
            }
            else
            {
                party.BrokerID = null;
            }

            return party;
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtPartyName.Text))
            {
                MessageBox.Show("Please enter Party Name", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPartyName.Focus();
                return false;
            }

            // Check for duplicate party name
            if (PartyService.PartyExists(txtPartyName.Text.Trim(), isNewParty ? null : currentParty.PartyID))
            {
                MessageBox.Show("A party with this name already exists", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPartyName.Focus();
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
                Party party = GetPartyFromForm();
                bool success;

                if (isNewParty)
                {
                    success = PartyService.AddParty(party);
                    if (success)
                        MessageBox.Show("Party added successfully", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Debug information
                    string debugInfo = $"Updating party: ID={party.PartyID}, Name={party.PartyName}, CompanyID={party.CompanyID}";
                    System.Diagnostics.Debug.WriteLine(debugInfo);
                    
                    success = PartyService.UpdateParty(party);
                    if (success)
                        MessageBox.Show("Party updated successfully", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show("Failed to update party. No rows were affected.", "Update Failed",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                if (success)
                {
                    LoadParties();
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving party: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (currentParty.PartyID == 0)
                return;

            // Check if party has bills linked to it
            if (BillService.HasBillsForParty(currentParty.PartyID))
            {
                MessageBox.Show("Cannot delete this party because it has bills linked to it. Please delete or reassign all bills first.", 
                    "Cannot Delete Party", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this party?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    bool success = PartyService.DeleteParty(currentParty.PartyID);

                    if (success)
                    {
                        MessageBox.Show("Party deleted successfully", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadParties();
                        ClearForm();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting party: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // The btnClose_Click event handler is removed as UserControls don't 'close' themselves.
        // The parent form/container will handle the closing.

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            // Simple filtering on the client side
            string searchText = txtSearch.Text.ToLower().Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                filteredParties = parties;
                dgvParties.DataSource = parties;
            }
            else
            {
                filteredParties = parties.FindAll(p =>
                    p.PartyName.ToLower().Contains(searchText) ||
                    p.Phone.ToLower().Contains(searchText) ||
                    p.Address.ToLower().Contains(searchText) ||
                    (p.BrokerName != null && p.BrokerName.ToLower().Contains(searchText))
                );

                dgvParties.DataSource = null;
                dgvParties.DataSource = filteredParties;
            }

            lblTotalParties.Text = $"Total Parties: {filteredParties.Count}";
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Prevent the beep sound
                e.Handled = true;

                // If there are filtered parties, select the first one
                if (filteredParties.Count > 0)
                {
                    // Select the first row in the grid
                    dgvParties.ClearSelection();
                    dgvParties.Rows[0].Selected = true;

                    // Populate the form with the selected party
                    PopulateForm(filteredParties[0]);

                    // Move focus to the party name field
                    txtPartyName.Focus();
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
                // F2 to focus on party name
                e.Handled = true;
                e.SuppressKeyPress = true;
                txtPartyName.Focus();
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
                dgvParties.Focus();
            }
            else if (e.KeyCode == Keys.F8)
            {
                // F8 to delete selected party
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

        private void dgvParties_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvParties.SelectedRows.Count > 0)
            {
                int selectedIndex = dgvParties.SelectedRows[0].Index;

                if (selectedIndex >= 0)
                {
                    // Use the filtered list if it's being displayed
                    List<Party> currentList = dgvParties.DataSource as List<Party>;
                    if (currentList != null && selectedIndex < currentList.Count)
                    {
                        Party selectedParty = currentList[selectedIndex];
                        PopulateForm(selectedParty);
                    }
                }
            }
        }

        private void dgvParties_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Use the filtered list if it's being displayed
                List<Party> currentList = dgvParties.DataSource as List<Party>;
                if (currentList != null && e.RowIndex < currentList.Count)
                {
                    Party selectedParty = currentList[e.RowIndex];
                    PopulateForm(selectedParty);
                    txtPartyName.Focus();
                }
            }
        }

        #endregion

        private void PartyMasterUserControl_KeyDown(object sender, KeyEventArgs e)
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

        // Public event to notify the parent form if a party is selected (useful for dialog-like behavior)
        public event EventHandler<PartySelectedEventArgs> PartySelected;

        // Custom EventArgs for passing the selected Party
        public class PartySelectedEventArgs : EventArgs
        {
            public Party SelectedParty { get; }
            public PartySelectedEventArgs(Party party)
            {
                SelectedParty = party;
            }
        }

        // Method to call when a party is selected and confirmed (e.g., from a double-click or a "Select" button)
        private void OnPartySelected(Party party)
        {
            PartySelected?.Invoke(this, new PartySelectedEventArgs(party));
        }
    }
}