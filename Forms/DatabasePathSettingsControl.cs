using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Utils;

namespace SaleBillSystem.NET.Forms
{
    /// <summary>
    /// Control for managing database path settings - allows admin to choose database location
    /// </summary>
    public partial class DatabasePathSettingsControl : UserControl
    {
        private TextBox txtCurrentPath;
        private TextBox txtNewPath;
        private Label lblStatus;
        private RadioButton rbDefault;
        private RadioButton rbCustom;
        private Button btnBrowse;
        private Button btnApply;
        private Button btnCreateNew;
        private Button btnCopyToLocation;

        public DatabasePathSettingsControl()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10f);
            this.Padding = new Padding(30);

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(20)
            };
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Title
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Current path
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Radio buttons
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // New path selection
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Buttons
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Status/Info

            // === TITLE ===
            var lblTitle = new Label
            {
                Text = "📁 Database Path Settings",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                Dock = DockStyle.Fill,
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainPanel.Controls.Add(lblTitle, 0, 0);

            // === CURRENT PATH SECTION ===
            var currentPathPanel = new Panel { Dock = DockStyle.Fill, Height = 80 };
            
            var lblCurrentPath = new Label
            {
                Text = "Current Database Location:",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Location = new Point(0, 5),
                AutoSize = true
            };
            
            txtCurrentPath = new TextBox
            {
                Font = new Font("Segoe UI", 10f),
                ReadOnly = true,
                BackColor = Color.FromArgb(245, 245, 245),
                Location = new Point(0, 30),
                Width = 700,
                BorderStyle = BorderStyle.FixedSingle
            };

            currentPathPanel.Controls.Add(lblCurrentPath);
            currentPathPanel.Controls.Add(txtCurrentPath);
            mainPanel.Controls.Add(currentPathPanel, 0, 1);

            // === RADIO BUTTONS SECTION ===
            var radioPanel = new Panel { Dock = DockStyle.Fill, Height = 80 };

            var lblChoose = new Label
            {
                Text = "Choose Database Location:",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Location = new Point(0, 5),
                AutoSize = true
            };

            rbDefault = new RadioButton
            {
                Text = $"Use Default Location (Application Folder)",
                Font = new Font("Segoe UI", 10f),
                Location = new Point(0, 30),
                AutoSize = true,
                Checked = !SettingsManager.HasCustomDatabasePath
            };
            rbDefault.CheckedChanged += RadioButton_CheckedChanged;

            rbCustom = new RadioButton
            {
                Text = "Use Custom Location (Network Drive, External Folder, etc.)",
                Font = new Font("Segoe UI", 10f),
                Location = new Point(0, 55),
                AutoSize = true,
                Checked = SettingsManager.HasCustomDatabasePath
            };
            rbCustom.CheckedChanged += RadioButton_CheckedChanged;

            radioPanel.Controls.Add(lblChoose);
            radioPanel.Controls.Add(rbDefault);
            radioPanel.Controls.Add(rbCustom);
            mainPanel.Controls.Add(radioPanel, 0, 2);

            // === NEW PATH SELECTION ===
            var newPathPanel = new Panel { Dock = DockStyle.Fill, Height = 80 };

            var lblNewPath = new Label
            {
                Text = "Select New Database Path:",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Location = new Point(0, 5),
                AutoSize = true
            };

            txtNewPath = new TextBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(0, 30),
                Width = 580,
                BorderStyle = BorderStyle.FixedSingle,
                Enabled = rbCustom.Checked
            };

            btnBrowse = new Button
            {
                Text = "Browse...",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Size = new Size(100, 28),
                Location = new Point(590, 29),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = rbCustom.Checked
            };
            btnBrowse.Click += BtnBrowse_Click;

            newPathPanel.Controls.Add(lblNewPath);
            newPathPanel.Controls.Add(txtNewPath);
            newPathPanel.Controls.Add(btnBrowse);
            mainPanel.Controls.Add(newPathPanel, 0, 3);

            // === BUTTONS SECTION ===
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Height = 60,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 10, 0, 0)
            };

            btnApply = new Button
            {
                Text = "✓ Apply Changes",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 10, 0)
            };
            btnApply.Click += BtnApply_Click;

            btnCopyToLocation = new Button
            {
                Text = "📋 Copy DB to Location",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Size = new Size(180, 40),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 10, 0)
            };
            btnCopyToLocation.Click += BtnCopyToLocation_Click;

            btnCreateNew = new Button
            {
                Text = "🆕 Create New Database",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Size = new Size(190, 40),
                BackColor = Color.FromArgb(243, 156, 18),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 10, 0)
            };
            btnCreateNew.Click += BtnCreateNew_Click;

            buttonPanel.Controls.Add(btnApply);
            buttonPanel.Controls.Add(btnCopyToLocation);
            buttonPanel.Controls.Add(btnCreateNew);
            mainPanel.Controls.Add(buttonPanel, 0, 4);

            // === INFO/STATUS SECTION ===
            var infoPanel = new Panel { Dock = DockStyle.Fill };

            lblStatus = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = Color.Gray,
                Location = new Point(0, 10),
                AutoSize = true
            };

            var lblInfo = new Label
            {
                Text = "ℹ️ Important Notes:\n\n" +
                       "• The database path is stored in a local settings file.\n" +
                       "• You can use network paths (e.g., \\\\server\\share\\database.accdb) for multi-user access.\n" +
                       "• Make sure you have read/write permissions to the selected location.\n" +
                       "• After changing the path, the application will need to restart.\n" +
                       "• Use 'Copy DB to Location' to copy your existing database to a new location.\n" +
                       "• Use 'Create New Database' to create a fresh database at the selected location.",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(100, 100, 100),
                Location = new Point(0, 50),
                AutoSize = true
            };

            infoPanel.Controls.Add(lblStatus);
            infoPanel.Controls.Add(lblInfo);
            mainPanel.Controls.Add(infoPanel, 0, 5);

            this.Controls.Add(mainPanel);
        }

        private void LoadCurrentSettings()
        {
            string currentPath = SettingsManager.ActiveDatabasePath;
            txtCurrentPath.Text = currentPath;

            if (SettingsManager.HasCustomDatabasePath)
            {
                rbCustom.Checked = true;
                txtNewPath.Text = SettingsManager.DatabasePath;
            }
            else
            {
                rbDefault.Checked = true;
                txtNewPath.Text = "";
            }

            UpdateStatus($"Current database: {(File.Exists(currentPath) ? "Found ✓" : "Not Found ✗")}", 
                File.Exists(currentPath) ? Color.Green : Color.Red);
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            bool isCustom = rbCustom.Checked;
            txtNewPath.Enabled = isCustom;
            btnBrowse.Enabled = isCustom;

            if (!isCustom)
            {
                txtNewPath.Text = "";
            }
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Database File";
                ofd.Filter = "Access Database (*.accdb)|*.accdb|All Files (*.*)|*.*";
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtNewPath.Text = ofd.FileName;
                    UpdateStatus("Database file selected. Click 'Apply Changes' to use this database.", Color.Blue);
                }
            }
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            try
            {
                if (rbDefault.Checked)
                {
                    // Clear custom path, use default
                    SettingsManager.DatabasePath = null;
                    
                    MessageBox.Show(
                        "Database path has been reset to default.\n\n" +
                        $"Default path: {SettingsManager.DefaultDatabasePath}\n\n" +
                        "The application needs to restart for changes to take effect.",
                        "Settings Saved",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    
                    PromptRestart();
                }
                else
                {
                    // Validate custom path
                    if (string.IsNullOrWhiteSpace(txtNewPath.Text))
                    {
                        MessageBox.Show("Please select a database file.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (!File.Exists(txtNewPath.Text))
                    {
                        var result = MessageBox.Show(
                            "The selected database file does not exist.\n\n" +
                            "Would you like to:\n" +
                            "• Click 'Yes' to create a new database at this location\n" +
                            "• Click 'No' to select a different file",
                            "File Not Found",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            CreateNewDatabaseAtPath(txtNewPath.Text);
                        }
                        return;
                    }

                    // Test if the file is accessible
                    try
                    {
                        using (var fs = File.Open(txtNewPath.Text, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                        {
                            // File is accessible
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Cannot access the selected database file:\n{ex.Message}\n\n" +
                            "Make sure you have read/write permissions.",
                            "Access Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }

                    // Save the custom path
                    SettingsManager.DatabasePath = txtNewPath.Text;

                    MessageBox.Show(
                        "Database path has been updated.\n\n" +
                        $"New path: {txtNewPath.Text}\n\n" +
                        "The application needs to restart for changes to take effect.",
                        "Settings Saved",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    PromptRestart();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCopyToLocation_Click(object sender, EventArgs e)
        {
            try
            {
                // First, select destination folder
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Title = "Choose Location to Copy Database";
                    sfd.Filter = "Access Database (*.accdb)|*.accdb";
                    sfd.FileName = "SaleSystem.accdb";
                    sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        string sourcePath = SettingsManager.ActiveDatabasePath;
                        string destPath = sfd.FileName;

                        if (!File.Exists(sourcePath))
                        {
                            MessageBox.Show("Source database not found.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        if (File.Exists(destPath))
                        {
                            var result = MessageBox.Show(
                                "A file already exists at this location.\n\n" +
                                "Do you want to overwrite it?",
                                "File Exists",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning);

                            if (result != DialogResult.Yes)
                                return;
                        }

                        // Copy the database
                        File.Copy(sourcePath, destPath, true);

                        // Update the path in the textbox
                        txtNewPath.Text = destPath;
                        rbCustom.Checked = true;

                        MessageBox.Show(
                            "Database copied successfully!\n\n" +
                            $"New location: {destPath}\n\n" +
                            "Click 'Apply Changes' to start using the database at this location.",
                            "Copy Complete",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        UpdateStatus("Database copied. Click 'Apply Changes' to use the new location.", Color.Green);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying database: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCreateNew_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Title = "Choose Location for New Database";
                    sfd.Filter = "Access Database (*.accdb)|*.accdb";
                    sfd.FileName = "SaleSystem.accdb";
                    sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        if (File.Exists(sfd.FileName))
                        {
                            var result = MessageBox.Show(
                                "A file already exists at this location.\n\n" +
                                "Do you want to overwrite it with a fresh database?",
                                "File Exists",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning);

                            if (result != DialogResult.Yes)
                                return;

                            File.Delete(sfd.FileName);
                        }

                        CreateNewDatabaseAtPath(sfd.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateNewDatabaseAtPath(string path)
        {
            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Copy template or create new database
                string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "template.accdb");
                string defaultDbPath = SettingsManager.DefaultDatabasePath;

                if (File.Exists(templatePath))
                {
                    File.Copy(templatePath, path, true);
                }
                else if (File.Exists(defaultDbPath))
                {
                    // Use existing default database as template
                    File.Copy(defaultDbPath, path, true);
                }
                else
                {
                    MessageBox.Show(
                        "No template database found.\n\n" +
                        "Please start the application once with the default database path to create the initial database, " +
                        "then come back here to copy it to your desired location.",
                        "Template Not Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Update textbox and save setting
                txtNewPath.Text = path;
                rbCustom.Checked = true;
                SettingsManager.DatabasePath = path;

                MessageBox.Show(
                    "New database created successfully!\n\n" +
                    $"Location: {path}\n\n" +
                    "The application needs to restart to use the new database.",
                    "Database Created",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                PromptRestart();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating database: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PromptRestart()
        {
            var result = MessageBox.Show(
                "Would you like to restart the application now?",
                "Restart Required",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Application.Restart();
            }
            else
            {
                LoadCurrentSettings();
            }
        }

        private void UpdateStatus(string message, Color color)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }
    }
}
