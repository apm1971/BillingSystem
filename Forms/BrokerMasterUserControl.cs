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
            // Set up Tab order
            txtSearch.TabIndex = 0;
            txtBrokerName.TabIndex = 1;
            txtPhone.TabIndex = 2;
            btnSave.TabIndex = 3;
            btnNew.TabIndex = 4;
            btnDelete.TabIndex = 5;
            
            // Set up text fields to use uppercase
            txtBrokerName.CharacterCasing = CharacterCasing.Upper;
            txtPhone.CharacterCasing = CharacterCasing.Upper;

            // Configure text boxes
            txtBrokerName.MaxLength = 100;
            txtPhone.MaxLength = 20;

            // Setup search functionality
            txtSearch.TextChanged += txtSearch_TextChanged;
            
            // Add KeyDown event handlers for all input controls
            txtSearch.KeyDown += txtSearch_KeyDown;
            txtBrokerName.KeyDown += Control_KeyDown;
            txtPhone.KeyDown += Control_KeyDown;
            dgvBrokers.KeyDown += Control_KeyDown;
            
            // Set up form controls
            txtSearch.PlaceholderText = "Type to search brokers...";
            txtBrokerName.PlaceholderText = "Enter broker name";
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
            toolTip.SetToolTip(btnSave, "Save the current broker (Ctrl+S)");
            toolTip.SetToolTip(btnNew, "Create a new broker (Ctrl+N)");
            toolTip.SetToolTip(btnDelete, "Delete the selected broker (F8)");
            toolTip.SetToolTip(txtSearch, "Search brokers by name or phone (F3)");
            toolTip.SetToolTip(txtBrokerName, "Enter broker name (F2)");
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

        private void SetupDataGrid()
        {
            // Configure data grid
            dgvBrokers.AutoGenerateColumns = false;
            dgvBrokers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBrokers.AllowUserToAddRows = false;
            dgvBrokers.AllowUserToDeleteRows = false;
            dgvBrokers.ReadOnly = true;
            dgvBrokers.MultiSelect = false;
            dgvBrokers.RowHeadersVisible = false;
            dgvBrokers.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = System.Drawing.Color.FromArgb(245, 245, 245)
            };

            // Set modern font and styling
            dgvBrokers.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            dgvBrokers.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            dgvBrokers.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            dgvBrokers.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dgvBrokers.ColumnHeadersHeight = 35;
            dgvBrokers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Set row height for better readability
            dgvBrokers.RowTemplate.Height = 30;

            // Clear existing columns
            dgvBrokers.Columns.Clear();

            // Add columns with proper sizing
            dgvBrokers.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "BrokerID",
                HeaderText = "ID",
                Width = 60,
                Visible = false
            });

            dgvBrokers.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "BrokerName",
                HeaderText = "Broker Name",
                Width = 300,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dgvBrokers.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Phone",
                HeaderText = "Phone",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleLeft }
            });

            // Enable double buffering for smooth scrolling
            typeof(DataGridView).InvokeMember("DoubleBuffered", 
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, dgvBrokers, new object[] { true });

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

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Prevent the beep sound
                e.Handled = true;

                // If there are filtered brokers, select the first one
                if (filteredBrokers.Count > 0)
                {
                    // Select the first row in the grid
                    dgvBrokers.ClearSelection();
                    dgvBrokers.Rows[0].Selected = true;

                    // Populate the form with the selected broker
                    PopulateForm(filteredBrokers[0]);

                    // Move focus to the broker name field
                    txtBrokerName.Focus();
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
                // F2 to focus on broker name
                e.Handled = true;
                e.SuppressKeyPress = true;
                txtBrokerName.Focus();
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
                dgvBrokers.Focus();
            }
            else if (e.KeyCode == Keys.F8)
            {
                // F8 to delete selected broker
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

        private void BrokerMasterUserControl_KeyDown(object sender, KeyEventArgs e)
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