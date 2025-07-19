using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using SaleBillSystem.NET.Data;

namespace SaleBillSystem.NET.Forms
{
    public partial class SettingsForm : Form
    {
        private TextBox txtInterestRate;
        private TextBox txtDiscountRate;
        private TextBox txtCompanyName;
        private TextBox txtCompanyAddress;
        private TextBox txtDatabasePath;
        private Button btnBrowseDatabase;
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
            this.Size = new Size(600, 500);
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
                Size = new Size(300, 23)
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
                Size = new Size(300, 60),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            
            // Database Path
            Label lblDatabasePath = new Label
            {
                Text = "Database Path:",
                Location = new Point(20, 230),
                Size = new Size(200, 20),
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular)
            };
            
            txtDatabasePath = new TextBox
            {
                Location = new Point(250, 228),
                Size = new Size(250, 23),
                ReadOnly = true
            };
            
            btnBrowseDatabase = new Button
            {
                Text = "Browse...",
                Location = new Point(510, 227),
                Size = new Size(70, 25)
            };
            
            Label lblDatabaseHelp = new Label
            {
                Text = "Select database location (e.g. on a USB drive)",
                Location = new Point(250, 255),
                Size = new Size(300, 20),
                Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Italic),
                ForeColor = Color.Gray
            };

            // Buttons
            btnSave = new Button
            {
                Text = "Save",
                Location = new Point(300, 400),
                Size = new Size(80, 30),
                BackColor = Color.LightGreen,
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold)
            };

            btnReset = new Button
            {
                Text = "Reset to Default",
                Location = new Point(390, 400),
                Size = new Size(100, 30),
                BackColor = Color.LightBlue,
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular)
            };

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(500, 400),
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
            mainPanel.Controls.Add(lblDatabasePath);
            mainPanel.Controls.Add(txtDatabasePath);
            mainPanel.Controls.Add(btnBrowseDatabase);
            mainPanel.Controls.Add(lblDatabaseHelp);
            mainPanel.Controls.Add(btnSave);
            mainPanel.Controls.Add(btnReset);
            mainPanel.Controls.Add(btnCancel);

            this.Controls.Add(mainPanel);

            // Event handlers
            btnSave.Click += BtnSave_Click;
            btnReset.Click += BtnReset_Click;
            btnCancel.Click += BtnCancel_Click;
            btnBrowseDatabase.Click += BtnBrowseDatabase_Click;
            this.KeyDown += SettingsForm_KeyDown;

            // Set tab order
            txtInterestRate.TabIndex = 0;
            txtDiscountRate.TabIndex = 1;
            txtCompanyName.TabIndex = 2;
            txtCompanyAddress.TabIndex = 3;
            txtDatabasePath.TabIndex = 4;
            btnBrowseDatabase.TabIndex = 5;
            btnSave.TabIndex = 6;
            btnReset.TabIndex = 7;
            btnCancel.TabIndex = 8;
        }

        private void LoadSettings()
        {
            try
            {
                txtInterestRate.Text = SettingsService.GetInterestRate().ToString("F2");
                txtDiscountRate.Text = SettingsService.GetDiscountRate().ToString("F2");
                txtCompanyName.Text = SettingsService.GetCompanyName();
                txtCompanyAddress.Text = SettingsService.GetCompanyAddress();
                
                // Load database path
                string dbPath = DatabaseManager.CustomDatabasePath;
                if (string.IsNullOrEmpty(dbPath))
                {
                    // Show default path if no custom path is set
                    dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "SaleSystem.accdb");
                }
                txtDatabasePath.Text = dbPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBrowseDatabase_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Filter = "Access Database (*.accdb)|*.accdb";
                    dialog.Title = "Select Database Location";
                    dialog.CheckFileExists = false;
                    dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    
                    // If we have a current path, use its directory as initial directory
                    if (!string.IsNullOrEmpty(txtDatabasePath.Text) && File.Exists(txtDatabasePath.Text))
                    {
                        dialog.InitialDirectory = Path.GetDirectoryName(txtDatabasePath.Text);
                        dialog.FileName = Path.GetFileName(txtDatabasePath.Text);
                    }
                    else
                    {
                        dialog.FileName = "SaleSystem.accdb";
                    }
                    
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        txtDatabasePath.Text = dialog.FileName;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting database path: {ex.Message}", "Error", 
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
                string databasePath = txtDatabasePath.Text.Trim();

                // Save settings
                bool success = true;
                success &= SettingsService.SetInterestRate(interestRate);
                success &= SettingsService.SetDiscountRate(discountRate);
                success &= SettingsService.SetCompanyName(companyName);
                success &= SettingsService.SetCompanyAddress(companyAddress);

                // Check if database path has changed
                string currentDbPath = DatabaseManager.CustomDatabasePath;
                if (currentDbPath != databasePath && !string.IsNullOrEmpty(databasePath))
                {
                    // Confirm database path change
                    if (MessageBox.Show(
                        "Changing the database path will restart the application. Are you sure you want to continue?",
                        "Confirm Database Change",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        // Set the database path (this will restart the app)
                        SettingsService.SetDatabasePath(databasePath);
                    }
                }
                else if (success)
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
                txtDatabasePath.Text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "SaleSystem.accdb");
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void SettingsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            else if (e.KeyCode == Keys.Enter && e.Control)
            {
                BtnSave_Click(sender, e);
            }
        }

        private bool ValidateInputs()
        {
            // Validate interest rate
            if (!double.TryParse(txtInterestRate.Text, out double interestRate) || interestRate < 0)
            {
                MessageBox.Show("Please enter a valid interest rate", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtInterestRate.Focus();
                return false;
            }

            // Validate discount rate
            if (!double.TryParse(txtDiscountRate.Text, out double discountRate) || discountRate < 0)
            {
                MessageBox.Show("Please enter a valid discount rate", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDiscountRate.Focus();
                return false;
            }

            // Validate company name
            if (string.IsNullOrWhiteSpace(txtCompanyName.Text))
            {
                MessageBox.Show("Please enter a company name", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCompanyName.Focus();
                return false;
            }

            return true;
        }
    }
} 