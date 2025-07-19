using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace SaleBillSystem.NET.Tools
{
    public partial class LicenseKeyGenerator : Form
    {
        private TextBox txtHardwareId;
        private TextBox txtCompanyName;
        private TextBox txtGeneratedKey;
        private Button btnGenerate;
        private Button btnCopy;

        public LicenseKeyGenerator()
        {
            SetupForm();
        }

        private void SetupForm()
        {
            this.Text = "License Key Generator";
            this.Size = new System.Drawing.Size(400, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Create controls
            Label lblHardwareId = new Label
            {
                Text = "Hardware ID:",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };

            txtHardwareId = new TextBox
            {
                Location = new System.Drawing.Point(20, 40),
                Width = 340
            };

            Label lblCompanyName = new Label
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

            btnGenerate = new Button
            {
                Text = "Generate Key",
                Location = new System.Drawing.Point(20, 130),
                Width = 100
            };

            txtGeneratedKey = new TextBox
            {
                Location = new System.Drawing.Point(20, 170),
                Width = 340,
                ReadOnly = true
            };

            btnCopy = new Button
            {
                Text = "Copy Key",
                Location = new System.Drawing.Point(280, 130),
                Width = 80
            };

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                lblHardwareId, txtHardwareId,
                lblCompanyName, txtCompanyName,
                btnGenerate, txtGeneratedKey,
                btnCopy
            });

            // Wire up events
            btnGenerate.Click += BtnGenerate_Click;
            btnCopy.Click += BtnCopy_Click;
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHardwareId.Text) || 
                string.IsNullOrWhiteSpace(txtCompanyName.Text))
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Generate a license key based on hardware ID and company name
            string input = $"{txtHardwareId.Text}|{txtCompanyName.Text}|{DateTime.Now.ToString("yyyyMMdd")}";
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                string hash = BitConverter.ToString(hashBytes).Replace("-", "");
                
                // Format the key in groups of 4 characters
                string formattedKey = "";
                for (int i = 0; i < 20; i += 4)
                {
                    if (formattedKey.Length > 0) formattedKey += "-";
                    formattedKey += hash.Substring(i, 4);
                }

                txtGeneratedKey.Text = formattedKey;
            }
        }

        private void BtnCopy_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtGeneratedKey.Text))
            {
                Clipboard.SetText(txtGeneratedKey.Text);
                MessageBox.Show("License key copied to clipboard!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
} 