using System;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class BrokerMasterForm : Form
    {
        private Broker currentBroker;
        private bool isEditMode;

        public BrokerMasterForm(Broker? broker = null)
        {
            InitializeComponent();
            isEditMode = broker != null;
            currentBroker = broker ?? new Broker();
            SetupForm();
        }

        private void SetupForm()
        {
            this.Text = isEditMode ? "Edit Broker" : "New Broker";
            
            if (isEditMode)
            {
                LoadBrokerData();
            }
        }

        private void LoadBrokerData()
        {
            txtBrokerName.Text = currentBroker.BrokerName;
            txtPhone.Text = currentBroker.Phone;
            txtEmail.Text = currentBroker.Email;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
            {
                return;
            }

            try
            {
                var broker = new Broker
                {
                    BrokerID = currentBroker.BrokerID,
                    BrokerName = txtBrokerName.Text.Trim(),
                    Phone = txtPhone.Text.Trim(),
                    Email = txtEmail.Text.Trim()
                };

                bool success;
                
                if (isEditMode)
                {
                    success = BrokerService.UpdateBroker(broker);
                }
                else
                {
                    success = BrokerService.AddBroker(broker);
                }

                if (success)
                {
                    MessageBox.Show($"Broker {(isEditMode ? "updated" : "created")} successfully!",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show($"Failed to {(isEditMode ? "update" : "create")} broker.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving broker: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtBrokerName.Text))
            {
                MessageBox.Show("Please enter broker name.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBrokerName.Focus();
                return false;
            }

            string brokerName = txtBrokerName.Text.Trim();
            int? excludeId = isEditMode ? currentBroker.BrokerID : null;
            
            if (BrokerService.BrokerExists(brokerName, excludeId))
            {
                MessageBox.Show("A broker with this name already exists.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBrokerName.Focus();
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