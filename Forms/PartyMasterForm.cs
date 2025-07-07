using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace SaleBillSystem.NET.Forms
{
    public partial class PartyMasterForm : Form
    {
        private List<Party> parties = new List<Party>();
        private List<Broker> brokers = new List<Broker>();
        private Party currentParty = new Party();
        private bool isNewParty = true;
        
        public PartyMasterForm()
        {
            InitializeComponent();
            LoadParties();
            LoadBrokers();
        }
        
        private void PartyMasterForm_Load(object sender, EventArgs e)
        {
            ConfigureControls();
            SetupDataGrid();
            ClearForm();
        }
        
        private void ConfigureControls()
        {
            // Set numeric format for TextBoxes
            txtCreditLimit.Text = "0.00";
            txtCreditDays.Text = "0";
            txtOutstandingAmount.Text = "0.00";
            
            // Set up Tab order
            txtPartyName.TabIndex = 0;
            txtAddress.TabIndex = 1;
            txtCity.TabIndex = 2;
            txtPhone.TabIndex = 3;
            txtEmail.TabIndex = 4;

            txtCreditLimit.TabIndex = 6;
            txtCreditDays.TabIndex = 7;
            txtOutstandingAmount.TabIndex = 8;
            btnSave.TabIndex = 9;
            btnNew.TabIndex = 10;
            btnDelete.TabIndex = 11;
            btnClose.TabIndex = 12;
        }
        
        private void SetupDataGrid()
        {
            // Configure data grid columns
            dgvParties.AutoGenerateColumns = false;
            dgvParties.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvParties.AllowUserToAddRows = false;
            dgvParties.AllowUserToDeleteRows = false;
            dgvParties.ReadOnly = true;
            dgvParties.MultiSelect = false;
            
            // Add columns to grid
            if (dgvParties.Columns.Count == 0)
            {
                dgvParties.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "PartyID",
                    HeaderText = "ID",
                    Width = 50,
                    Visible = false
                });
                
                dgvParties.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "PartyName",
                    HeaderText = "Party Name",
                    Width = 200
                });
                
                dgvParties.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "City",
                    HeaderText = "City",
                    Width = 120
                });
                
                dgvParties.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Phone",
                    HeaderText = "Phone",
                    Width = 100
                });
                

                
                dgvParties.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "OutstandingAmount",
                    HeaderText = "Outstanding",
                    Width = 100,
                    DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
                });
            }
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

                cmbBroker.DataSource = brokerList;
                cmbBroker.DisplayMember = "BrokerName";
                cmbBroker.ValueMember = "BrokerID";
                cmbBroker.SelectedValue = 0; // Default to "No Broker"
            }
        }
        
        private void ClearForm()
        {
            currentParty = new Party();
            isNewParty = true;
            
            txtPartyName.Text = string.Empty;
            txtAddress.Text = string.Empty;
            txtCity.Text = string.Empty;
            txtPhone.Text = string.Empty;
            txtEmail.Text = string.Empty;

            txtCreditLimit.Text = "0.00";
            txtCreditDays.Text = "0";
            txtOutstandingAmount.Text = "0.00";
            
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
            txtCity.Text = party.City;
            txtPhone.Text = party.Phone;
            txtEmail.Text = party.Email;

            txtCreditLimit.Text = party.CreditLimit.ToString("N2");
            txtCreditDays.Text = party.CreditDays.ToString();
            txtOutstandingAmount.Text = party.OutstandingAmount.ToString("N2");
            
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
                PartyName = txtPartyName.Text.Trim(),
                Address = txtAddress.Text.Trim(),
                City = txtCity.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                CreditLimit = Convert.ToDouble(txtCreditLimit.Text),
                CreditDays = Convert.ToInt32(txtCreditDays.Text),
                OutstandingAmount = Convert.ToDouble(txtOutstandingAmount.Text)
            };

            // Set broker information
            if (cmbBroker != null && cmbBroker.SelectedValue is int brokerID && brokerID > 0)
            {
                var broker = brokers.FirstOrDefault(b => b.BrokerID == brokerID);
                if (broker != null)
                {
                    party.BrokerID = broker.BrokerID;
                    party.BrokerName = broker.BrokerName;
                }
            }
            else
            {
                party.BrokerID = null;
                party.BrokerName = string.Empty;
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
                    success = PartyService.UpdateParty(party);
                    if (success)
                        MessageBox.Show("Party updated successfully", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                
                if (success)
                {
                    LoadParties();
                    ClearForm();
                    
                    // Set DialogResult to OK so calling forms know a party was saved
                    this.DialogResult = DialogResult.OK;
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
        
        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
        
        private void dgvParties_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvParties.SelectedRows.Count > 0)
            {
                int selectedIndex = dgvParties.SelectedRows[0].Index;
                
                if (selectedIndex >= 0 && selectedIndex < parties.Count)
                {
                    Party selectedParty = parties[selectedIndex];
                    PopulateForm(selectedParty);
                }
            }
        }
        
        private void dgvParties_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < parties.Count)
            {
                Party selectedParty = parties[e.RowIndex];
                PopulateForm(selectedParty);
                txtPartyName.Focus();
            }
        }
        
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            // Simple filtering on the client side
            string searchText = txtSearch.Text.ToLower().Trim();
            
            if (string.IsNullOrWhiteSpace(searchText))
            {
                LoadParties();
            }
            else
            {
                List<Party> filteredParties = parties.FindAll(p => 
                    p.PartyName.ToLower().Contains(searchText) ||
                    p.City.ToLower().Contains(searchText) ||
                    p.Phone.ToLower().Contains(searchText)
                );
                
                dgvParties.DataSource = null;
                dgvParties.DataSource = filteredParties;
                
                lblTotalParties.Text = $"Total Parties: {filteredParties.Count}";
            }
        }
        
        #endregion
    }
} 