using System;
using System.Drawing;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;

namespace SaleBillSystem.NET.Forms
{
    public partial class SettingsForm : Form
    {
        private TextBox txtInterestRate;
        private TextBox txtDiscountRate;
        private TextBox txtCompanyName;
        private TextBox txtCompanyAddress;
        private Button btnSave;
        private Button btnCancel;
        private Button btnReset;

        public SettingsForm()
        {
            InitializeComponent();
            SetupForm();
            LoadSettings();
        }

        private void SetupForm()
        {
            this.Text = "Application Settings";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.KeyPreview = true;

            // Create main panel
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Interest Rate
            Label lblInterestRate = new Label
            {
                Text = "Interest Rate (% per annum):",
                Location = new Point(20, 30),
                Size = new Size(200, 20),
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular)
            };

            txtInterestRate = new TextBox
            {
                Location = new Point(250, 28),
                Size = new Size(100, 23),
                TextAlign = HorizontalAlignment.Right
            };

            Label lblInterestHelp = new Label
            {
                Text = "Applied to overdue bills (daily calculation)",
                Location = new Point(360, 30),
                Size = new Size(200, 20),
                Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Italic),
                ForeColor = Color.Gray
            };

            // Discount Rate
            Label lblDiscountRate = new Label
            {
                Text = "Discount Rate (%):",
                Location = new Point(20, 70),
                Size = new Size(200, 20),
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular)
            };

            txtDiscountRate = new TextBox
            {
                Location = new Point(250, 68),
                Size = new Size(100, 23),
                TextAlign = HorizontalAlignment.Right
            };

            Label lblDiscountHelp = new Label
            {
                Text = "Applied for early payment before due date",
                Location = new Point(360, 70),
                Size = new Size(200, 20),
                Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Italic),
                ForeColor = Color.Gray
            };

            // Company Name
            Label lblCompanyName = new Label
            {
                Text = "Company Name:",
                Location = new Point(20, 110),
                Size = new Size(200, 20),
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular)
            };

            txtCompanyName = new TextBox
            {
                Location = new Point(250, 108),
                Size = new Size(200, 23)
            };

            // Company Address
            Label lblCompanyAddress = new Label
            {
                Text = "Company Address:",
                Location = new Point(20, 150),
                Size = new Size(200, 20),
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular)
            };

            txtCompanyAddress = new TextBox
            {
                Location = new Point(250, 148),
                Size = new Size(200, 60),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            // Buttons
            btnSave = new Button
            {
                Text = "Save",
                Location = new Point(200, 300),
                Size = new Size(80, 30),
                BackColor = Color.LightGreen,
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold)
            };

            btnReset = new Button
            {
                Text = "Reset to Default",
                Location = new Point(290, 300),
                Size = new Size(100, 30),
                BackColor = Color.LightBlue,
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular)
            };

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(400, 300),
                Size = new Size(80, 30),
                BackColor = Color.LightCoral,
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold)
            };

            // Add controls to main panel
            mainPanel.Controls.Add(lblInterestRate);
            mainPanel.Controls.Add(txtInterestRate);
            mainPanel.Controls.Add(lblInterestHelp);
            mainPanel.Controls.Add(lblDiscountRate);
            mainPanel.Controls.Add(txtDiscountRate);
            mainPanel.Controls.Add(lblDiscountHelp);
            mainPanel.Controls.Add(lblCompanyName);
            mainPanel.Controls.Add(txtCompanyName);
            mainPanel.Controls.Add(lblCompanyAddress);
            mainPanel.Controls.Add(txtCompanyAddress);
            mainPanel.Controls.Add(btnSave);
            mainPanel.Controls.Add(btnReset);
            mainPanel.Controls.Add(btnCancel);

            this.Controls.Add(mainPanel);

            // Event handlers
            btnSave.Click += BtnSave_Click;
            btnReset.Click += BtnReset_Click;
            btnCancel.Click += BtnCancel_Click;
            this.KeyDown += SettingsForm_KeyDown;

            // Set tab order
            txtInterestRate.TabIndex = 0;
            txtDiscountRate.TabIndex = 1;
            txtCompanyName.TabIndex = 2;
            txtCompanyAddress.TabIndex = 3;
            btnSave.TabIndex = 4;
            btnReset.TabIndex = 5;
            btnCancel.TabIndex = 6;
        }

        private void LoadSettings()
        {
            try
            {
                txtInterestRate.Text = SettingsService.GetInterestRate().ToString("F2");
                txtDiscountRate.Text = SettingsService.GetDiscountRate().ToString("F2");
                txtCompanyName.Text = SettingsService.GetCompanyName();
                txtCompanyAddress.Text = SettingsService.GetCompanyAddress();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateInputs())
                    return;

                double interestRate = Convert.ToDouble(txtInterestRate.Text);
                double discountRate = Convert.ToDouble(txtDiscountRate.Text);
                string companyName = txtCompanyName.Text.Trim();
                string companyAddress = txtCompanyAddress.Text.Trim();

                // Save settings
                bool success = true;
                success &= SettingsService.SetInterestRate(interestRate);
                success &= SettingsService.SetDiscountRate(discountRate);
                success &= SettingsService.SetCompanyName(companyName);
                success &= SettingsService.SetCompanyAddress(companyAddress);

                if (success)
                {
                    MessageBox.Show("Settings saved successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to save some settings. Please try again.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Reset all settings to default values?", "Confirm Reset", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                txtInterestRate.Text = "12.00";
                txtDiscountRate.Text = "1.00";
                txtCompanyName.Text = "Your Company Name";
                txtCompanyAddress.Text = "Your Company Address";
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void SettingsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                BtnSave_Click(sender, e);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                BtnCancel_Click(sender, e);
                e.SuppressKeyPress = true;
            }
        }

        private bool ValidateInputs()
        {
            // Validate interest rate
            if (!double.TryParse(txtInterestRate.Text, out double interestRate) || 
                interestRate < 0 || interestRate > 100)
            {
                MessageBox.Show("Please enter a valid interest rate between 0 and 100.", 
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtInterestRate.Focus();
                return false;
            }

            // Validate discount rate
            if (!double.TryParse(txtDiscountRate.Text, out double discountRate) || 
                discountRate < 0 || discountRate > 100)
            {
                MessageBox.Show("Please enter a valid discount rate between 0 and 100.", 
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDiscountRate.Focus();
                return false;
            }

            // Validate company name
            if (string.IsNullOrWhiteSpace(txtCompanyName.Text))
            {
                MessageBox.Show("Please enter a company name.", 
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCompanyName.Focus();
                return false;
            }

            return true;
        }
    }
} 