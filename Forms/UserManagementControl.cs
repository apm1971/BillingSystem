using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    /// <summary>
    /// User Management Control for admin to manage users and passwords
    /// </summary>
    public partial class UserManagementControl : UserControl
    {
        private DataGridView dgvUsers;
        private TextBox txtUsername;
        private TextBox txtDisplayName;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private CheckBox chkIsAdmin;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClear;
        private Label lblStatus;
        private int _selectedUserId = 0;

        public UserManagementControl()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10f);

            // Main layout panel
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(20)
            };
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 60)); // User list
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40)); // Edit form

            // === TOP SECTION: User List ===
            var listPanel = new Panel { Dock = DockStyle.Fill };
            
            var lblTitle = new Label
            {
                Text = "👥 User Management",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                Dock = DockStyle.Top,
                Height = 40
            };

            dgvUsers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    SelectionBackColor = Color.FromArgb(52, 152, 219),
                    SelectionForeColor = Color.White,
                    Padding = new Padding(5)
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    BackColor = Color.FromArgb(41, 128, 185),
                    ForeColor = Color.White,
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Padding = new Padding(5)
                },
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 40
            };
            dgvUsers.CellClick += DgvUsers_CellClick;

            listPanel.Controls.Add(dgvUsers);
            listPanel.Controls.Add(lblTitle);

            // === BOTTOM SECTION: Edit Form ===
            var formPanel = new Panel 
            { 
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 10, 0, 0)
            };

            var formLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 4,
                Padding = new Padding(10)
            };
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12)); // Labels
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38)); // Inputs
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12)); // Labels
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38)); // Inputs

            // Row 0: Section Title
            var lblFormTitle = new Label
            {
                Text = "Add / Edit User",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            formLayout.Controls.Add(lblFormTitle, 0, 0);
            formLayout.SetColumnSpan(lblFormTitle, 4);

            // Row 1: Username and Display Name
            formLayout.Controls.Add(CreateLabel("Username *:"), 0, 1);
            txtUsername = CreateTextBox();
            formLayout.Controls.Add(txtUsername, 1, 1);

            formLayout.Controls.Add(CreateLabel("Display Name:"), 2, 1);
            txtDisplayName = CreateTextBox();
            formLayout.Controls.Add(txtDisplayName, 3, 1);

            // Row 2: Password and Confirm Password
            formLayout.Controls.Add(CreateLabel("Password *:"), 0, 2);
            txtPassword = CreateTextBox();
            txtPassword.PasswordChar = '*';
            formLayout.Controls.Add(txtPassword, 1, 2);

            formLayout.Controls.Add(CreateLabel("Confirm Password *:"), 2, 2);
            txtConfirmPassword = CreateTextBox();
            txtConfirmPassword.PasswordChar = '*';
            formLayout.Controls.Add(txtConfirmPassword, 3, 2);

            // Row 3: IsAdmin checkbox and Buttons
            var checkPanel = new Panel { Dock = DockStyle.Fill };
            chkIsAdmin = new CheckBox
            {
                Text = "Is Admin",
                Font = new Font("Segoe UI", 10f),
                AutoSize = true,
                Location = new Point(5, 10)
            };
            checkPanel.Controls.Add(chkIsAdmin);
            formLayout.Controls.Add(checkPanel, 0, 3);
            formLayout.SetColumnSpan(checkPanel, 2);

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0),
                WrapContents = false
            };

            btnClear = CreateButton("Clear", Color.FromArgb(149, 165, 166));
            btnClear.Click += BtnClear_Click;
            buttonPanel.Controls.Add(btnClear);

            btnDelete = CreateButton("Delete", Color.FromArgb(231, 76, 60));
            btnDelete.Click += BtnDelete_Click;
            buttonPanel.Controls.Add(btnDelete);

            btnUpdate = CreateButton("Update", Color.FromArgb(243, 156, 18));
            btnUpdate.Click += BtnUpdate_Click;
            buttonPanel.Controls.Add(btnUpdate);

            btnAdd = CreateButton("Add User", Color.FromArgb(46, 204, 113));
            btnAdd.Click += BtnAdd_Click;
            buttonPanel.Controls.Add(btnAdd);

            formLayout.Controls.Add(buttonPanel, 2, 3);
            formLayout.SetColumnSpan(buttonPanel, 2);

            formPanel.Controls.Add(formLayout);

            // Status label
            lblStatus = new Label
            {
                Text = "",
                Dock = DockStyle.Bottom,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9f, FontStyle.Italic)
            };

            mainPanel.Controls.Add(listPanel, 0, 0);
            mainPanel.Controls.Add(formPanel, 0, 1);

            this.Controls.Add(mainPanel);
            this.Controls.Add(lblStatus);

            // Initial button states
            UpdateButtonStates();
        }

        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10f),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 10, 0)
            };
        }

        private TextBox CreateTextBox()
        {
            return new TextBox
            {
                Font = new Font("Segoe UI", 10f),
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5)
            };
        }

        private Button CreateButton(string text, Color backColor)
        {
            return new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Size = new Size(100, 35),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(5)
            };
        }

        private void LoadUsers()
        {
            try
            {
                List<User> users = UserService.GetAllUsers();
                
                dgvUsers.Columns.Clear();
                dgvUsers.DataSource = null;

                dgvUsers.Columns.Add("UserID", "ID");
                dgvUsers.Columns.Add("Username", "Username");
                dgvUsers.Columns.Add("DisplayName", "Display Name");
                dgvUsers.Columns.Add("IsAdmin", "Is Admin");

                dgvUsers.Columns["UserID"].Width = 50;
                dgvUsers.Columns["IsAdmin"].Width = 80;

                foreach (var user in users)
                {
                    dgvUsers.Rows.Add(user.UserID, user.Username, user.DisplayName, user.IsAdmin ? "Yes" : "No");
                }

                lblStatus.Text = $"Loaded {users.Count} user(s)";
                lblStatus.ForeColor = Color.Gray;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvUsers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvUsers.Rows.Count)
            {
                var row = dgvUsers.Rows[e.RowIndex];
                _selectedUserId = Convert.ToInt32(row.Cells["UserID"].Value);
                txtUsername.Text = row.Cells["Username"].Value?.ToString() ?? "";
                txtDisplayName.Text = row.Cells["DisplayName"].Value?.ToString() ?? "";
                chkIsAdmin.Checked = row.Cells["IsAdmin"].Value?.ToString() == "Yes";
                
                // Clear password fields when editing
                txtPassword.Clear();
                txtConfirmPassword.Clear();
                
                // Update placeholder text for password fields
                txtPassword.PlaceholderText = "(Leave blank to keep current)";
                txtConfirmPassword.PlaceholderText = "(Leave blank to keep current)";

                UpdateButtonStates();
                lblStatus.Text = $"Selected user: {txtUsername.Text}";
                lblStatus.ForeColor = Color.FromArgb(41, 128, 185);
            }
        }

        private void UpdateButtonStates()
        {
            bool isEditing = _selectedUserId > 0;
            btnAdd.Enabled = !isEditing;
            btnUpdate.Enabled = isEditing;
            btnDelete.Enabled = isEditing;
        }

        private bool ValidateInput(bool isUpdate = false)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Username is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return false;
            }

            // For new users, password is required
            if (!isUpdate && string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Password is required for new users.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return false;
            }

            // If password is provided (either new user or updating password), validate it
            if (!string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                if (txtPassword.Text.Length < 4)
                {
                    MessageBox.Show("Password must be at least 4 characters long.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return false;
                }

                if (txtPassword.Text != txtConfirmPassword.Text)
                {
                    MessageBox.Show("Password and Confirm Password do not match.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtConfirmPassword.Focus();
                    return false;
                }
            }

            return true;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput(false))
                return;

            try
            {
                // Check if username already exists
                var existingUser = UserService.GetUserByUsername(txtUsername.Text.Trim());
                if (existingUser != null)
                {
                    MessageBox.Show("A user with this username already exists. Please choose a different username.", 
                        "Username Exists", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtUsername.Focus();
                    return;
                }

                var newUser = new User
                {
                    UserID = 0, // New user
                    Username = txtUsername.Text.Trim(),
                    DisplayName = string.IsNullOrWhiteSpace(txtDisplayName.Text) ? txtUsername.Text.Trim() : txtDisplayName.Text.Trim(),
                    IsAdmin = chkIsAdmin.Checked
                };

                bool success = UserService.SaveUser(newUser, txtPassword.Text);
                
                if (success)
                {
                    MessageBox.Show("User added successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadUsers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding user: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (_selectedUserId == 0)
            {
                MessageBox.Show("Please select a user to update.", "No User Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateInput(true))
                return;

            try
            {
                // Check if username already exists (for another user)
                var existingUser = UserService.GetUserByUsername(txtUsername.Text.Trim());
                if (existingUser != null && existingUser.UserID != _selectedUserId)
                {
                    MessageBox.Show("A user with this username already exists. Please choose a different username.", 
                        "Username Exists", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtUsername.Focus();
                    return;
                }

                var userToUpdate = new User
                {
                    UserID = _selectedUserId,
                    Username = txtUsername.Text.Trim(),
                    DisplayName = string.IsNullOrWhiteSpace(txtDisplayName.Text) ? txtUsername.Text.Trim() : txtDisplayName.Text.Trim(),
                    IsAdmin = chkIsAdmin.Checked
                };

                // Pass password only if provided (otherwise it won't be updated)
                string passwordToUpdate = string.IsNullOrWhiteSpace(txtPassword.Text) ? null : txtPassword.Text;
                
                bool success = UserService.SaveUser(userToUpdate, passwordToUpdate);
                
                if (success)
                {
                    string message = "User updated successfully!";
                    if (!string.IsNullOrWhiteSpace(passwordToUpdate))
                    {
                        message += "\nPassword has been changed.";
                    }
                    
                    MessageBox.Show(message, "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadUsers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating user: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedUserId == 0)
            {
                MessageBox.Show("Please select a user to delete.", "No User Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Don't allow deleting the only admin
            var allUsers = UserService.GetAllUsers();
            var adminUsers = allUsers.FindAll(u => u.IsAdmin);
            var selectedUser = allUsers.Find(u => u.UserID == _selectedUserId);
            
            if (selectedUser != null && selectedUser.IsAdmin && adminUsers.Count <= 1)
            {
                MessageBox.Show("Cannot delete the only admin user. There must be at least one admin.", 
                    "Cannot Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Don't allow deleting yourself
            if (Program.CurrentUser != null && Program.CurrentUser.UserID == _selectedUserId)
            {
                MessageBox.Show("You cannot delete your own account while logged in.", 
                    "Cannot Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete user '{txtUsername.Text}'?\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                try
                {
                    bool success = UserService.DeleteUser(_selectedUserId);
                    
                    if (success)
                    {
                        MessageBox.Show("User deleted successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearForm();
                        LoadUsers();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting user: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            _selectedUserId = 0;
            txtUsername.Clear();
            txtDisplayName.Clear();
            txtPassword.Clear();
            txtConfirmPassword.Clear();
            chkIsAdmin.Checked = false;
            
            txtPassword.PlaceholderText = "";
            txtConfirmPassword.PlaceholderText = "";
            
            dgvUsers.ClearSelection();
            UpdateButtonStates();
            lblStatus.Text = "Form cleared";
            lblStatus.ForeColor = Color.Gray;
        }
    }
}
