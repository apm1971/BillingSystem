using System;
using System.Drawing;
using System.Windows.Forms;
using SaleBillSystem.NET.Utils;

namespace SaleBillSystem.NET.Forms
{
    /// <summary>
    /// License Generator Form - FOR DEVELOPER USE ONLY
    /// Use this form to generate license keys for your customers
    /// Access via: Utilities menu (only visible to admin users)
    /// </summary>
    public class LicenseGeneratorForm : Form
    {
        private TextBox txtHardwareId;
        private TextBox txtCustomerName;
        private TextBox txtGeneratedKey;
        private Button btnGenerate;
        private Button btnCopyKey;
        private Label lblMyHwid;

        public LicenseGeneratorForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "License Key Generator (Developer Tool)";
            this.Size = new Size(550, 420);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(45, 45, 48);

            int y = 20;
            int leftMargin = 25;

            // Title
            var lblTitle = new Label
            {
                Text = "🔑 License Key Generator",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(leftMargin, y),
                Size = new Size(500, 35)
            };
            this.Controls.Add(lblTitle);
            y += 40;

            // Warning
            var lblWarning = new Label
            {
                Text = "⚠️ This tool is for generating customer license keys only.\n    Keep this tool secure and do not distribute it!",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(255, 193, 7),
                Location = new Point(leftMargin, y),
                Size = new Size(500, 40)
            };
            this.Controls.Add(lblWarning);
            y += 50;

            // Your Hardware ID (for reference)
            lblMyHwid = new Label
            {
                Text = $"Your Machine Hardware ID: {LicenseManager.GetHardwareId()}",
                Font = new Font("Consolas", 9),
                ForeColor = Color.Gray,
                Location = new Point(leftMargin, y),
                Size = new Size(500, 20)
            };
            this.Controls.Add(lblMyHwid);
            y += 35;

            // Customer Hardware ID Label
            var lblHwid = new Label
            {
                Text = "Customer's Hardware ID:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(leftMargin, y),
                Size = new Size(300, 22)
            };
            this.Controls.Add(lblHwid);
            y += 25;

            // Customer Hardware ID TextBox
            txtHardwareId = new TextBox
            {
                Location = new Point(leftMargin, y),
                Size = new Size(480, 28),
                Font = new Font("Consolas", 11),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                CharacterCasing = CharacterCasing.Upper
            };
            this.Controls.Add(txtHardwareId);
            y += 45;

            // Customer Name Label
            var lblName = new Label
            {
                Text = "Customer / Company Name:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(leftMargin, y),
                Size = new Size(300, 22)
            };
            this.Controls.Add(lblName);
            y += 25;

            // Customer Name TextBox
            txtCustomerName = new TextBox
            {
                Location = new Point(leftMargin, y),
                Size = new Size(480, 28),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(txtCustomerName);
            y += 45;

            // Generate Button
            btnGenerate = new Button
            {
                Text = "🔐 Generate License Key",
                Location = new Point(leftMargin, y),
                Size = new Size(200, 38),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnGenerate.FlatAppearance.BorderSize = 0;
            btnGenerate.Click += BtnGenerate_Click;
            this.Controls.Add(btnGenerate);
            y += 55;

            // Generated Key Label
            var lblGenerated = new Label
            {
                Text = "Generated License Key:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.LightGreen,
                Location = new Point(leftMargin, y),
                Size = new Size(300, 22)
            };
            this.Controls.Add(lblGenerated);
            y += 25;

            // Generated Key TextBox
            txtGeneratedKey = new TextBox
            {
                Location = new Point(leftMargin, y),
                Size = new Size(380, 30),
                Font = new Font("Consolas", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.LightGreen,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                TextAlign = HorizontalAlignment.Center
            };
            this.Controls.Add(txtGeneratedKey);

            // Copy Key Button
            btnCopyKey = new Button
            {
                Text = "📋 Copy",
                Location = new Point(415, y - 1),
                Size = new Size(90, 32),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnCopyKey.FlatAppearance.BorderSize = 0;
            btnCopyKey.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(txtGeneratedKey.Text))
                {
                    Clipboard.SetText(txtGeneratedKey.Text);
                    MessageBox.Show($"License key copied!\n\nSend this to: {txtCustomerName.Text}",
                        "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            this.Controls.Add(btnCopyKey);

            // Handle Enter key
            this.AcceptButton = btnGenerate;
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHardwareId.Text))
            {
                MessageBox.Show("Please enter customer's Hardware ID.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHardwareId.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                MessageBox.Show("Please enter customer/company name.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCustomerName.Focus();
                return;
            }

            // Validate Hardware ID format (should be XXXX-XXXX-XXXX-XXXX or 16 chars)
            string hwid = txtHardwareId.Text.Replace("-", "").Replace(" ", "").Trim();
            if (hwid.Length != 16)
            {
                MessageBox.Show("Hardware ID should be 16 characters (format: XXXX-XXXX-XXXX-XXXX).",
                    "Invalid Hardware ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHardwareId.Focus();
                return;
            }

            // Generate license key
            string licenseKey = LicenseManager.GenerateLicenseKey(
                txtHardwareId.Text.Trim(),
                txtCustomerName.Text.Trim()
            );

            txtGeneratedKey.Text = licenseKey;
            btnCopyKey.Enabled = true;

            // Select the generated key for easy copying
            txtGeneratedKey.SelectAll();
            txtGeneratedKey.Focus();
        }
    }
}

