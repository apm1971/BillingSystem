namespace SaleBillSystem.NET.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private LoginControl loginControl;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatusUser;
        private ToolStripStatusLabel lblStatusSeparator;
        private ToolStripStatusLabel lblStatusInfo;
        private Panel panelMain;
        private Panel panelHeader;
        private Label lblWelcome;
        private Label lblUserName;
        private Label lblDateTime;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            mainMenuStrip = new MenuStrip();
            loginControl = new LoginControl();
            statusStrip = new StatusStrip();
            lblStatusUser = new ToolStripStatusLabel();
            lblStatusSeparator = new ToolStripStatusLabel();
            lblStatusInfo = new ToolStripStatusLabel();
            panelMain = new Panel();
            panelHeader = new Panel();
            lblWelcome = new Label();
            lblUserName = new Label();
            lblDateTime = new Label();
            mainMenuStrip.SuspendLayout();
            statusStrip.SuspendLayout();
            panelHeader.SuspendLayout();
            SuspendLayout();
            // 
            // mainMenuStrip
            // 
            mainMenuStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(102)))), ((int)(((byte)(153)))));
            mainMenuStrip.ForeColor = System.Drawing.Color.White;
            mainMenuStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            mainMenuStrip.Items.AddRange(new ToolStripItem[] { });
            mainMenuStrip.Location = new Point(0, 0);
            mainMenuStrip.Name = "mainMenuStrip";
            mainMenuStrip.Padding = new Padding(0, 0, 10, 0);
            mainMenuStrip.Size = new Size(933, 35);
            mainMenuStrip.TabIndex = 1;
            mainMenuStrip.Text = "menuStrip1";
            mainMenuStrip.Visible = false;
            // 
            // statusStrip
            // 
            statusStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(102)))), ((int)(((byte)(153)))));
            statusStrip.ForeColor = System.Drawing.Color.White;
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatusUser, lblStatusSeparator, lblStatusInfo });
            statusStrip.Location = new Point(0, 545);
            statusStrip.Name = "statusStrip";
            statusStrip.Padding = new Padding(0, 0, 10, 0);
            statusStrip.Size = new Size(933, 24);
            statusStrip.SizingGrip = false;
            statusStrip.TabIndex = 3;
            statusStrip.Text = "statusStrip1";
            statusStrip.Visible = false;
            // 
            // lblStatusUser
            // 
            lblStatusUser.Name = "lblStatusUser";
            lblStatusUser.Size = new Size(100, 19);
            lblStatusUser.Text = "User: ";
            lblStatusUser.Spring = false;
            // 
            // lblStatusSeparator
            // 
            lblStatusSeparator.BorderStyle = Border3DStyle.Etched;
            lblStatusSeparator.Name = "lblStatusSeparator";
            lblStatusSeparator.Size = new Size(10, 24);
            // 
            // lblStatusInfo
            // 
            lblStatusInfo.Name = "lblStatusInfo";
            lblStatusInfo.Size = new Size(80, 19);
            lblStatusInfo.Spring = true;
            lblStatusInfo.Text = "Ready";
            lblStatusInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panelMain
            // 
            panelMain.Controls.Add(panelHeader);
            panelMain.Dock = DockStyle.Fill;
            panelMain.Location = new Point(0, 35);
            panelMain.Name = "panelMain";
            panelMain.Size = new Size(933, 510);
            panelMain.TabIndex = 4;
            // 
            // panelHeader
            // 
            panelHeader.BackColor = System.Drawing.Color.White;
            panelHeader.Controls.Add(lblDateTime);
            panelHeader.Controls.Add(lblUserName);
            panelHeader.Controls.Add(lblWelcome);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(933, 60);
            panelHeader.TabIndex = 0;
            panelHeader.Visible = false;
            // 
            // lblWelcome
            // 
            lblWelcome.AutoSize = true;
            lblWelcome.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            lblWelcome.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(102)))), ((int)(((byte)(153)))));
            lblWelcome.Location = new Point(20, 12);
            lblWelcome.Name = "lblWelcome";
            lblWelcome.Size = new Size(137, 30);
            lblWelcome.TabIndex = 0;
            lblWelcome.Text = "Welcome Back";
            // 
            // lblUserName
            // 
            lblUserName.AutoSize = true;
            lblUserName.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            lblUserName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            lblUserName.Location = new Point(175, 20);
            lblUserName.Name = "lblUserName";
            lblUserName.Size = new Size(60, 21);
            lblUserName.TabIndex = 1;
            lblUserName.Text = "User";
            // 
            // lblDateTime
            // 
            lblDateTime.AutoSize = true;
            lblDateTime.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            lblDateTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            lblDateTime.Location = new Point(830, 20);
            lblDateTime.Name = "lblDateTime";
            lblDateTime.Size = new Size(83, 15);
            lblDateTime.TabIndex = 2;
            lblDateTime.Text = "Date: ";
            // 
            // loginControl
            // 
            loginControl.Dock = DockStyle.Fill;
            loginControl.Location = new Point(0, 0);
            loginControl.Margin = new Padding(5, 3, 5, 3);
            loginControl.Name = "loginControl";
            loginControl.Size = new Size(933, 519);
            loginControl.TabIndex = 0;
            loginControl.Visible = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(245)))), ((int)(((byte)(250)))));
            ClientSize = new Size(933, 569);
            Controls.Add(statusStrip);
            Controls.Add(panelMain);
            Controls.Add(mainMenuStrip);
            Controls.Add(loginControl);
            loginControl.BringToFront();
            MainMenuStrip = mainMenuStrip;
            Margin = new Padding(4, 3, 4, 3);
            MinimumSize = new Size(900, 600);
            Name = "MainForm";
            Text = "Sale Bill System";
            WindowState = FormWindowState.Maximized;
            Load += MainForm_Load;
            mainMenuStrip.ResumeLayout(false);
            mainMenuStrip.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}