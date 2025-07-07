using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SaleBillSystem.NET.Forms
{
    public partial class BrokerListForm : Form
    {
        private List<Broker> brokers = new List<Broker>();
        private List<Broker> filteredBrokers = new List<Broker>();

        public BrokerListForm()
        {
            InitializeComponent();
            ConfigureControls();
            SetupDataGrid();
        }

        private void BrokerListForm_Load(object sender, EventArgs e)
        {
            LoadBrokers();
        }

        private void ConfigureControls()
        {
            // Set form properties
            this.Text = "Broker List";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;

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

            dgvBrokers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Email",
                HeaderText = "Email",
                DataPropertyName = "Email",
                Width = 250
            });

            dgvBrokers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ContactInfo",
                HeaderText = "Contact Info",
                DataPropertyName = "ContactInfo",
                Width = 300
            });

            // Event handlers
            dgvBrokers.SelectionChanged += dgvBrokers_SelectionChanged;
            dgvBrokers.CellDoubleClick += dgvBrokers_CellDoubleClick;
            dgvBrokers.KeyDown += dgvBrokers_KeyDown;
        }

        private void LoadBrokers()
        {
            try
            {
                brokers = BrokerService.GetAllBrokers();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading brokers: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilters()
        {
            try
            {
                filteredBrokers = brokers.ToList();

                // Apply search filter
                string searchText = txtSearch.Text.ToLower().Trim();
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    filteredBrokers = filteredBrokers.Where(b =>
                        b.BrokerName.ToLower().Contains(searchText) ||
                        b.Phone.ToLower().Contains(searchText) ||
                        b.Email.ToLower().Contains(searchText)
                    ).ToList();
                }

                dgvBrokers.DataSource = filteredBrokers;
                
                lblTotalBrokers.Text = $"Total Brokers: {filteredBrokers.Count}";

                // Enable/disable buttons based on selection
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying filters: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = dgvBrokers.SelectedRows.Count > 0;
            btnEdit.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;
            btnView.Enabled = hasSelection;
        }

        private Broker GetSelectedBroker()
        {
            if (dgvBrokers.SelectedRows.Count > 0)
            {
                return (Broker)dgvBrokers.SelectedRows[0].DataBoundItem;
            }
            return null;
        }

        private void EditSelectedBroker()
        {
            var selectedBroker = GetSelectedBroker();
            if (selectedBroker != null)
            {
                var form = new BrokerMasterForm(selectedBroker);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadBrokers();
                }
            }
        }

        private void ViewSelectedBroker()
        {
            var selectedBroker = GetSelectedBroker();
            if (selectedBroker != null)
            {
                string message = $"Broker Details:\n\n" +
                    $"Name: {selectedBroker.BrokerName}\n" +
                    $"Phone: {selectedBroker.Phone}\n" +
                    $"Email: {selectedBroker.Email}";

                MessageBox.Show(message, "Broker Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DeleteSelectedBroker()
        {
            var selectedBroker = GetSelectedBroker();
            if (selectedBroker != null)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete broker '{selectedBroker.BrokerName}'?\n\n" +
                    "This action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        if (BrokerService.DeleteBroker(selectedBroker.BrokerID))
                        {
                            MessageBox.Show("Broker deleted successfully.", "Success", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadBrokers();
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete broker.", "Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting broker: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadBrokers();
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            var form = new BrokerMasterForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadBrokers();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            EditSelectedBroker();
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            ViewSelectedBroker();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DeleteSelectedBroker();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dgvBrokers_SelectionChanged(object sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void dgvBrokers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                EditSelectedBroker();
            }
        }

        private void dgvBrokers_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedBroker();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                EditSelectedBroker();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                LoadBrokers();
                e.Handled = true;
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }
    }
} 