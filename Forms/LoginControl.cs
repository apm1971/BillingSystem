using System;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class LoginControl : UserControl
    {
        // Event to notify the MainForm when login is successful
        public event EventHandler<User> LoginSuccess;

        public LoginControl()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            // Trigger the login attempt
            AttemptLogin();
        }

        private void AttemptLogin()
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please enter both username and password.", "Login Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                User authenticatedUser = UserService.AuthenticateUser(txtUsername.Text.Trim(), txtPassword.Text);

                if (authenticatedUser != null)
                {
                    // Fire the LoginSuccess event and pass the authenticated user
                    LoginSuccess?.Invoke(this, authenticatedUser);
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Login Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        // This handles keyboard input for a smooth experience
        private void LoginControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Prevents the "ding" sound
                AttemptLogin();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                // Optionally, you can add logic to close the app
                Application.Exit();
            }
        }
        
        // Ensure the username field gets focus when the control is shown
        private void LoginControl_Load(object sender, EventArgs e)
        {
            txtUsername.Focus();
        }
    }
}