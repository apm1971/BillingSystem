using System;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class LicenseForm : Form
    {
        private TextBox txtLicenseKey;
        private TextBox txtCompanyName;
        private TextBox txtEmail;
        private Button btnActivate;
        private Button btnCancel;
        private Label lblHardwareId;
        private Label lblActualHardwareId;

        public LicenseForm()
        {
            SetupForm();
        }

        private void SetupForm()
        {
            this.Text = "License Activation";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new System.Drawing.Size(400, 300);

            // Create controls
            Label lblLicenseKey = new Label
            {
                Text = "License Key:",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };

            txtLicenseKey = new TextBox
            {
                Location = new System.Drawing.Point(20, 40),
                Width = 340
            };

            Label lblCompany = new Label
            {
                Text = "Company Name:",
                Location = new System.Drawing.Point(20, 70),
                AutoSize = true
            };

            txtCompanyName = new TextBox
            {
                Location = new System.Drawing.Point(20, 90),
                Width = 340
            };

            Label lblEmailAddress = new Label
            {
                Text = "Email:",
                Location = new System.Drawing.Point(20, 120),
                AutoSize = true
            };

            txtEmail = new TextBox
            {
                Location = new System.Drawing.Point(20, 140),
                Width = 340
            };

            lblHardwareId = new Label
            {
                Text = "Hardware ID:",
                Location = new System.Drawing.Point(20, 170),
                AutoSize = true
            };

            lblActualHardwareId = new Label
            {
                Text = LicenseService.GetHardwareId(),
                Location = new System.Drawing.Point(20, 190),
                AutoSize = true,
                Font = new System.Drawing.Font(this.Font, System.Drawing.FontStyle.Bold)
            };

            btnActivate = new Button
            {
                Text = "Activate",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(180, 220),
                Width = 80
            };

            btnCancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(280, 220),
                Width = 80
            };

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                lblLicenseKey, txtLicenseKey,
                lblCompany, txtCompanyName,
                lblEmailAddress, txtEmail,
                lblHardwareId, lblActualHardwareId,
                btnActivate, btnCancel
            });

            // Wire up events
            btnActivate.Click += BtnActivate_Click;
            btnCancel.Click += BtnCancel_Click;
            this.KeyPreview = true;
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };
        }

        private void BtnActivate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLicenseKey.Text) ||
                string.IsNullOrWhiteSpace(txtCompanyName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            License license = new License
            {
                LicenseKey = txtLicenseKey.Text,
                HardwareId = LicenseService.GetHardwareId(),
                ExpiryDate = DateTime.Now.AddYears(1), // Set expiry to 1 year from now
                CompanyName = txtCompanyName.Text,
                Email = txtEmail.Text
            };

            if (LicenseService.SaveLicense(license))
            {
                MessageBox.Show("License activated successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Failed to activate license.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
} 