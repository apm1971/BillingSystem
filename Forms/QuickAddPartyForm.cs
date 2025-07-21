using System;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class QuickAddPartyForm : Form
    {
        public Party? NewParty { get; private set; }

        public QuickAddPartyForm()
        {
            InitializeComponent();
            SetupForm();
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
            txtCity.CharacterCasing = CharacterCasing.Upper;
            txtPhone.CharacterCasing = CharacterCasing.Upper;
            // Email can remain in mixed case as emails are case-sensitive
            // txtEmail.CharacterCasing = CharacterCasing.Upper;
            
            // Add a button to open full Party Master form
            Button btnOpenPartyMaster = new Button();
            btnOpenPartyMaster.Text = "Open Full Party Master";
            btnOpenPartyMaster.Location = new System.Drawing.Point(btnCancel.Left - 150, btnCancel.Top);
            btnOpenPartyMaster.Size = new System.Drawing.Size(140, btnCancel.Height);
            btnOpenPartyMaster.Click += btnOpenPartyMaster_Click;
            this.Controls.Add(btnOpenPartyMaster);
            
            txtPartyName.Focus();
        }

        private void btnOpenPartyMaster_Click(object sender, EventArgs e)
        {
            // Open Party Master form in dialog mode
            using (var partyMasterForm = new PartyMasterForm(true))
            {
                if (partyMasterForm.ShowDialog() == DialogResult.OK)
                {
                    // A party was added in the Party Master form, so we can close this form
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
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
                    City = txtCity.Text.Trim().ToUpper(),
                    Phone = txtPhone.Text.Trim().ToUpper(),
                    Email = txtEmail.Text.Trim(), // Email remains as-is (case-sensitive)
                    // CreditLimit = 0,
                    CreditDays = 0,
                    // OutstandingAmount = 0
                };

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