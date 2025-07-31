using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class BrokerMasterUserControl : UserControl
    {
        private List<Broker> brokers = new List<Broker>();
        private List<Broker> filteredBrokers = new List<Broker>();
        private Broker currentBroker = new Broker();
        private bool isNewBroker = true;

        public BrokerMasterUserControl()
        {
            InitializeComponent();
            LoadBrokers();
            ConfigureControls();
            SetupDataGrid();
            
            // Handle key events for the UserControl
            this.KeyDown += BrokerMasterUserControl_KeyDown;
        }

        private void BrokerMasterUserControl_Load(object sender, EventArgs e)
        {
            ConfigureControls();
            SetupDataGrid();
            ClearForm();
        }

        private void ConfigureControls()
        {
            // Configure text boxes
            txtBrokerName.MaxLength = 100;
            txtPhone.MaxLength = 20;
            
            // Set text fields to use uppercase
            txtBrokerName.CharacterCasing = CharacterCasing.Upper;
            txtPhone.CharacterCasing = CharacterCasing.Upper;

            // Setup search functionality
            txtSearch.TextChanged += txtSearch_TextChanged;
        }

        private void SetupDataGrid()
        {
            dgvBrokers.AutoGenerateColumns = false;
            dgvBrokers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBrokers.MultiSelect = false;
            dgvBrokers.ReadOnly = true;
            dgvBrokers.AllowUserToAddRows = false;
            dgvBrokers.AllowUserToDeleteRows = false;

            // Configure columns
            dgvBrokers.Columns.Clear();

            dgvBrokers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BrokerName",
                HeaderText = "Broker Name",
                DataPropertyName = "BrokerName",
                Width = 200
            });

            dgvBrokers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Phone",
                HeaderText = "Phone",
                DataPropertyName = "Phone",
                Width = 150
            });

            // Event handlers
            dgvBrokers.SelectionChanged += dgvBrokers_SelectionChanged;
            dgvBrokers.CellDoubleClick += dgvBrokers_CellDoubleClick;
        }

        private void LoadBrokers()
        {
            try
            {
                brokers = BrokerService.GetAllBrokers();
                dgvBrokers.DataSource = null;
                dgvBrokers.DataSource = brokers;

                lblTotalBrokers.Text = $"Total Brokers: {brokers.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading brokers: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearForm()
        {
            currentBroker = new Broker();
            isNewBroker = true;

            txtBrokerName.Text = string.Empty;
            txtPhone.Text = string.Empty;

            txtBrokerName.Focus();
            btnDelete.Enabled = false;
        }

        private void PopulateForm(Broker broker)
        {
            currentBroker = broker;
            isNewBroker = false;

            txtBrokerName.Text = broker.BrokerName;
            txtPhone.Text = broker.Phone;

            btnDelete.Enabled = true;
        }

        private Broker GetBrokerFromForm()
        {
            Broker broker = new Broker
            {
                BrokerID = currentBroker.BrokerID,
                BrokerName = txtBrokerName.Text.Trim().ToUpper(),
                Phone = txtPhone.Text.Trim().ToUpper(),
                CompanyID = Program.ActiveCompany?.CompanyID ?? 1
            };

            return broker;
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtBrokerName.Text))
            {
                MessageBox.Show("Please enter Broker Name", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBrokerName.Focus();
                return false;
            }

            string brokerName = txtBrokerName.Text.Trim();
            int? excludeId = isNewBroker ? null : currentBroker.BrokerID;
            
            if (BrokerService.BrokerExists(brokerName, excludeId))
            {
                MessageBox.Show("A broker with this name already exists.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBrokerName.Focus();
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
                Broker broker = GetBrokerFromForm();
                bool success;

                if (isNewBroker)
                {
                    success = BrokerService.AddBroker(broker);
                    if (success)
                        MessageBox.Show("Broker added successfully", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Debug information
                    string debugInfo = $"Updating broker: ID={broker.BrokerID}, Name={broker.BrokerName}, CompanyID={broker.CompanyID}";
                    System.Diagnostics.Debug.WriteLine(debugInfo);
                    
                    success = BrokerService.UpdateBroker(broker);
                    if (success)
                        MessageBox.Show("Broker updated successfully", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show("Failed to update broker. No rows were affected.", "Update Failed",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                if (success)
                {
                    LoadBrokers();
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving broker: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (currentBroker.BrokerID == 0)
                return;

            if (MessageBox.Show("Are you sure you want to delete this broker?", "Confirm Delete", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    bool success = BrokerService.DeleteBroker(currentBroker.BrokerID);

                    if (success)
                    {
                        MessageBox.Show("Broker deleted successfully", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadBrokers();
                        ClearForm();
                    }
                    else
                    {
                        MessageBox.Show("Cannot delete broker. It may be used in bills or parties.", "Delete Failed",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting broker: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvBrokers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvBrokers.SelectedRows.Count > 0)
            {
                int selectedIndex = dgvBrokers.SelectedRows[0].Index;

                // Use the current displayed list (filtered or full)
                var currentList = string.IsNullOrWhiteSpace(txtSearch.Text.Trim()) ? brokers : filteredBrokers;

                if (selectedIndex >= 0 && selectedIndex < currentList.Count)
                {
                    Broker selectedBroker = currentList[selectedIndex];
                    PopulateForm(selectedBroker);
                }
            }
        }

        private void dgvBrokers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Use the current displayed list (filtered or full)
            var currentList = string.IsNullOrWhiteSpace(txtSearch.Text.Trim()) ? brokers : filteredBrokers;

            if (e.RowIndex >= 0 && e.RowIndex < currentList.Count)
            {
                Broker selectedBroker = currentList[e.RowIndex];
                PopulateForm(selectedBroker);
                txtBrokerName.Focus();
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            // Simple filtering on the client side
            string searchText = txtSearch.Text.ToLower().Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                filteredBrokers = brokers;
                dgvBrokers.DataSource = brokers;
            }
            else
            {
                filteredBrokers = brokers.FindAll(b => 
                    b.BrokerName.ToLower().Contains(searchText) ||
                    b.Phone.ToLower().Contains(searchText)
                );

                dgvBrokers.DataSource = null;
                dgvBrokers.DataSource = filteredBrokers;
            }

            lblTotalBrokers.Text = $"Total Brokers: {filteredBrokers.Count}";

            // If there are filtered brokers, select the first one
            if (filteredBrokers.Count > 0)
            {
                dgvBrokers.ClearSelection();
                dgvBrokers.Rows[0].Selected = true;
                // The selection change event will handle populating the form
            }
            else
            {
                ClearForm(); // Clear the form if no brokers match the search
            }
        }

        private void BrokerMasterUserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                // In a UserControl, you typically don't close the control itself with Escape.
                // You might raise an event for the parent form to handle or simply do nothing.
                e.Handled = true;
                // If you want to signal the parent form to close, you'd raise an event:
                // OnCloseRequested?.Invoke(this, EventArgs.Empty);
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                // Handle Ctrl+S
                e.Handled = true;
                btnSave.PerformClick();
            }
        }

        #endregion

        // Public event to notify the parent form if a broker is selected (useful for dialog-like behavior)
        public event EventHandler<BrokerSelectedEventArgs> BrokerSelected;

        // Custom EventArgs for passing the selected Broker
        public class BrokerSelectedEventArgs : EventArgs
        {
            public Broker SelectedBroker { get; }
            public BrokerSelectedEventArgs(Broker broker)
            {
                SelectedBroker = broker;
            }
        }

        // Method to call when a broker is selected and confirmed (e.g., from a double-click or a "Select" button)
        private void OnBrokerSelected(Broker broker)
        {
            BrokerSelected?.Invoke(this, new BrokerSelectedEventArgs(broker));
        }
    }
} 