namespace SaleBillSystem.NET.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MenuStrip mainMenuStrip;

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
            // loginControl = new LoginControl();
            SuspendLayout();
            // 
            // mainMenuStrip
            // 
            mainMenuStrip.Location = new Point(0, 0);
            mainMenuStrip.Name = "mainMenuStrip";
            mainMenuStrip.Padding = new Padding(7, 2, 0, 2);
            mainMenuStrip.Size = new Size(933, 28);
            mainMenuStrip.TabIndex = 1;
            mainMenuStrip.Text = "menuStrip1";
            mainMenuStrip.Visible = false;
            // 
            // loginControl
            // 
            // loginControl.Dock = DockStyle.Fill;
            // loginControl.Location = new Point(0, 0);
            // loginControl.Margin = new Padding(5, 3, 5, 3);
            // loginControl.Name = "loginControl";
            // loginControl.Size = new Size(933, 519);
            // loginControl.TabIndex = 0;
            // loginControl.Visible = false;
            // loginControl.Load += loginControl_Load;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(933, 519);
            // Controls.Add(loginControl);
            Controls.Add(mainMenuStrip);
            MainMenuStrip = mainMenuStrip;
            Margin = new Padding(4, 3, 4, 3);
            Name = "MainForm";
            Text = "Sale Bill System";
            WindowState = FormWindowState.Maximized;
            Load += MainForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        // private LoginControl loginControl;
    }
}