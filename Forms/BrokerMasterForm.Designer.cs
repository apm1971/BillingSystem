namespace SaleBillSystem.NET.Forms
{
    partial class BrokerMasterForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblBrokerName = new System.Windows.Forms.Label();
            this.txtBrokerName = new System.Windows.Forms.TextBox();
            this.lblPhone = new System.Windows.Forms.Label();
            this.txtPhone = new System.Windows.Forms.TextBox();
            this.lblEmail = new System.Windows.Forms.Label();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();

            // lblBrokerName
            this.lblBrokerName.AutoSize = true;
            this.lblBrokerName.Location = new System.Drawing.Point(20, 25);
            this.lblBrokerName.Name = "lblBrokerName";
            this.lblBrokerName.Size = new System.Drawing.Size(78, 15);
            this.lblBrokerName.TabIndex = 0;
            this.lblBrokerName.Text = "Broker Name:";

            // txtBrokerName
            this.txtBrokerName.Location = new System.Drawing.Point(120, 22);
            this.txtBrokerName.MaxLength = 100;
            this.txtBrokerName.Name = "txtBrokerName";
            this.txtBrokerName.Size = new System.Drawing.Size(250, 23);
            this.txtBrokerName.TabIndex = 1;

            // lblPhone
            this.lblPhone.AutoSize = true;
            this.lblPhone.Location = new System.Drawing.Point(20, 65);
            this.lblPhone.Name = "lblPhone";
            this.lblPhone.Size = new System.Drawing.Size(44, 15);
            this.lblPhone.TabIndex = 2;
            this.lblPhone.Text = "Phone:";

            // txtPhone
            this.txtPhone.Location = new System.Drawing.Point(120, 62);
            this.txtPhone.MaxLength = 20;
            this.txtPhone.Name = "txtPhone";
            this.txtPhone.Size = new System.Drawing.Size(250, 23);
            this.txtPhone.TabIndex = 3;

            // lblEmail
            this.lblEmail.AutoSize = true;
            this.lblEmail.Location = new System.Drawing.Point(20, 105);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(39, 15);
            this.lblEmail.TabIndex = 4;
            this.lblEmail.Text = "Email:";

            // txtEmail
            this.txtEmail.Location = new System.Drawing.Point(120, 102);
            this.txtEmail.MaxLength = 100;
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(250, 23);
            this.txtEmail.TabIndex = 5;

            // btnSave
            this.btnSave.Location = new System.Drawing.Point(200, 150);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(80, 30);
            this.btnSave.TabIndex = 6;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            // btnCancel
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(290, 150);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 30);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // BrokerMasterForm
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(400, 200);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.lblEmail);
            this.Controls.Add(this.txtPhone);
            this.Controls.Add(this.lblPhone);
            this.Controls.Add(this.txtBrokerName);
            this.Controls.Add(this.lblBrokerName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BrokerMasterForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Broker Master";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblBrokerName;
        private System.Windows.Forms.TextBox txtBrokerName;
        private System.Windows.Forms.Label lblPhone;
        private System.Windows.Forms.TextBox txtPhone;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
} 