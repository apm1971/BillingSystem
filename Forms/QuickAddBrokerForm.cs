using System;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class QuickAddBrokerForm : Form
    {
        public Broker? NewBroker { get; private set; }

        public QuickAddBrokerForm()
        {
            InitializeComponent();
            SetupForm();
            
            // Enable key preview to handle keyboard shortcuts
            this.KeyPreview = true;
            this.KeyDown += QuickAddBrokerForm_KeyDown;
        }

        private void QuickAddBrokerForm_KeyDown(object sender, KeyEventArgs e)
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

        private void SetupForm()
        {
            this.Text = "Quick Add Broker";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // Set text fields to use uppercase
            txtBrokerName.CharacterCasing = CharacterCasing.Upper;
            txtPhone.CharacterCasing = CharacterCasing.Upper;
            
            // Load default rates and days from settings
            txtInterestDays.Text = SettingsService.GetDefaultInterestDays().ToString();
            txtInterestRate.Text = SettingsService.GetDefaultInterestRate().ToString("F2");
            txtDiscountDays.Text = SettingsService.GetDefaultDiscountDays().ToString();
            txtDiscountRate.Text = SettingsService.GetDefaultDiscountRate().ToString("F2");
            txtBrokerageRate.Text = SettingsService.GetDefaultBrokerageRate().ToString("F2");
            
            txtBrokerName.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                var broker = new Broker
                {
                    BrokerName = txtBrokerName.Text.Trim().ToUpper(),
                    Phone = txtPhone.Text.Trim().ToUpper(),
                    InterestDays = Convert.ToInt32(txtInterestDays.Text),
                    InterestRate = Convert.ToDecimal(txtInterestRate.Text),
                    DiscountDays = Convert.ToInt32(txtDiscountDays.Text),
                    DiscountRate = Convert.ToDecimal(txtDiscountRate.Text),
                    BrokerageRate = Convert.ToDecimal(txtBrokerageRate.Text)
                };

                if (BrokerService.AddBroker(broker))
                {
                    // Get the newly added broker with its ID
                    var brokers = BrokerService.GetAllBrokers();
                    NewBroker = brokers.Find(b => b.BrokerName == broker.BrokerName);
                    
                    MessageBox.Show("Broker added successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to add broker. Please try again.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding broker: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

            // Check for duplicate broker name
            if (BrokerService.BrokerExists(txtBrokerName.Text.Trim(), null))
            {
                MessageBox.Show("A broker with this name already exists", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBrokerName.Focus();
                return false;
            }

            // Validate numeric fields
            if (!int.TryParse(txtInterestDays.Text, out _) ||
                !decimal.TryParse(txtInterestRate.Text, out _) ||
                !int.TryParse(txtDiscountDays.Text, out _) ||
                !decimal.TryParse(txtDiscountRate.Text, out _) ||
                !decimal.TryParse(txtBrokerageRate.Text, out _))
            {
                MessageBox.Show("Please enter valid numeric values for days and rates", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
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