using System;
using System.Drawing;
using System.Windows.Forms;
using SaleBillSystem.NET.Utils;

namespace SaleBillSystem.NET.Forms
{
    public class ActivationForm : Form
    {
        private TextBox txtHardwareId;
        private TextBox txtCustomerName;
        private TextBox txtLicenseKey;
        private Button btnActivate;
        private Button btnCopyHwid;
        private Button btnExit;
        private Label lblStatus;

        public bool IsActivated { get; private set; } = false;

        public ActivationForm()
        {
            InitializeComponent();
            txtHardwareId.Text = LicenseManager.GetHardwareId();
        }

        private void InitializeComponent()
        {
            this.Text = "Software Activation - Sale Bill System";
            this.Size = new Size(520, 380);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            int y = 20;
            int leftMargin = 25;

            // Title
            var lblTitle = new Label
            {
                Text = "🔐 Software Activation Required",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                Location = new Point(leftMargin, y),
                Size = new Size(460, 35)
            };
            this.Controls.Add(lblTitle);
            y += 45;

            // Subtitle
            var lblSubtitle = new Label
            {
                Text = "Please contact vendor with your Hardware ID to get a license key.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(leftMargin, y),
                Size = new Size(460, 20)
            };
            this.Controls.Add(lblSubtitle);
            y += 35;

            // Hardware ID Label
            var lblHwid = new Label
            {
                Text = "Your Hardware ID (send this to vendor):",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(leftMargin, y),
                Size = new Size(300, 20)
            };
            this.Controls.Add(lblHwid);
            y += 22;

            // Hardware ID TextBox
            txtHardwareId = new TextBox
            {
                Location = new Point(leftMargin, y),
                Size = new Size(340, 28),
                ReadOnly = true,
                Font = new Font("Consolas", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(255, 255, 220),
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = HorizontalAlignment.Center
            };
            this.Controls.Add(txtHardwareId);

            // Copy Button
            btnCopyHwid = new Button
            {
                Text = "📋 Copy",
                Location = new Point(375, y - 1),
                Size = new Size(90, 30),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(240, 240, 240),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCopyHwid.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btnCopyHwid.Click += (s, e) =>
            {
                Clipboard.SetText(txtHardwareId.Text);
                MessageBox.Show("Hardware ID copied to clipboard!\n\nSend this to your vendor to get your license key.",
                    "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            this.Controls.Add(btnCopyHwid);
            y += 50;

            // Customer Name Label
            var lblName = new Label
            {
                Text = "Customer / Company Name:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(leftMargin, y),
                Size = new Size(200, 20)
            };
            this.Controls.Add(lblName);
            y += 22;

            // Customer Name TextBox
            txtCustomerName = new TextBox
            {
                Location = new Point(leftMargin, y),
                Size = new Size(440, 26),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(txtCustomerName);
            y += 40;

            // License Key Label
            var lblKey = new Label
            {
                Text = "License Key:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(leftMargin, y),
                Size = new Size(200, 20)
            };
            this.Controls.Add(lblKey);
            y += 22;

            // License Key TextBox
            txtLicenseKey = new TextBox
            {
                Location = new Point(leftMargin, y),
                Size = new Size(440, 28),
                Font = new Font("Consolas", 11),
                BorderStyle = BorderStyle.FixedSingle,
                CharacterCasing = CharacterCasing.Upper
            };
            this.Controls.Add(txtLicenseKey);
            y += 40;

            // Status Label
            lblStatus = new Label
            {
                Text = "",
                Location = new Point(leftMargin, y),
                Size = new Size(440, 20),
                ForeColor = Color.Red,
                Font = new Font("Segoe UI", 9)
            };
            this.Controls.Add(lblStatus);
            y += 30;

            // Activate Button
            btnActivate = new Button
            {
                Text = "✓ Activate Software",
                Location = new Point(140, y),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnActivate.FlatAppearance.BorderSize = 0;
            btnActivate.Click += BtnActivate_Click;
            this.Controls.Add(btnActivate);

            // Exit Button
            btnExit = new Button
            {
                Text = "Exit",
                Location = new Point(295, y),
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(158, 158, 158),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Click += (s, e) => this.Close();
            this.Controls.Add(btnExit);

            // Handle Enter key
            this.AcceptButton = btnActivate;
        }

        private void BtnActivate_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "";

            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                lblStatus.Text = "Please enter customer/company name.";
                txtCustomerName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtLicenseKey.Text))
            {
                lblStatus.Text = "Please enter license key.";
                txtLicenseKey.Focus();
                return;
            }

            var result = LicenseManager.ActivateLicense(
                txtCustomerName.Text.Trim(),
                txtLicenseKey.Text.Trim()
            );

            if (result.Success)
            {
                IsActivated = true;
                MessageBox.Show(result.Message, "Activation Successful",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                lblStatus.Text = result.Message;
                lblStatus.ForeColor = Color.Red;
            }
        }
    }
}

