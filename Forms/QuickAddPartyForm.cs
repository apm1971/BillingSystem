using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;
using System.Linq;

namespace SaleBillSystem.NET.Forms
{
    public partial class QuickAddPartyForm : Form
    {
        public Party? NewParty { get; private set; }
        private List<Broker> brokers = new List<Broker>();

        public QuickAddPartyForm()
        {
            InitializeComponent();
            SetupForm();
            LoadBrokers();
            
            // Enable key preview to handle keyboard shortcuts
            this.KeyPreview = true;
            this.KeyDown += QuickAddPartyForm_KeyDown;
        }

        private void QuickAddPartyForm_KeyDown(object sender, KeyEventArgs e)
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

        private void SetupForm()
        {
            this.Text = "Quick Add Party";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // Set text fields to use uppercase
            txtPartyName.CharacterCasing = CharacterCasing.Upper;
            txtAddress.CharacterCasing = CharacterCasing.Upper;
            txtPhone.CharacterCasing = CharacterCasing.Upper;
            
            txtPartyName.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                var party = new Party
                {
                    PartyName = txtPartyName.Text.Trim().ToUpper(),
                    Address = txtAddress.Text.Trim().ToUpper(),
                    Phone = txtPhone.Text.Trim().ToUpper(),
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

                if (PartyService.AddParty(party))
                {
                    // Get the newly added party with its ID
                    var parties = PartyService.GetAllParties();
                    NewParty = parties.Find(p => p.PartyName == party.PartyName);
                    
                    MessageBox.Show("Party added successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to add party. Please try again.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding party: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            if (PartyService.PartyExists(txtPartyName.Text.Trim(), null))
            {
                MessageBox.Show("A party with this name already exists", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPartyName.Focus();
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