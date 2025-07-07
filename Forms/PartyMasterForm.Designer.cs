namespace SaleBillSystem.NET.Forms
{
    partial class PartyMasterForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelTop = new System.Windows.Forms.Panel();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.lblTotalParties = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnNew = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.panelForm = new System.Windows.Forms.Panel();
            this.txtOutstandingAmount = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtCreditDays = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtCreditLimit = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cmbBroker = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtPhone = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtCity = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtAddress = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPartyName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.dgvParties = new System.Windows.Forms.DataGridView();
            this.panelTop.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panelForm.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvParties)).BeginInit();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.txtSearch);
            this.panelTop.Controls.Add(this.label10);
            this.panelTop.Controls.Add(this.lblTotalParties);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(984, 40);
            this.panelTop.TabIndex = 0;
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(95, 9);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(260, 23);
            this.txtSearch.TabIndex = 2;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(12, 12);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(77, 15);
            this.label10.TabIndex = 1;
            this.label10.Text = "Search Party:";
            // 
            // lblTotalParties
            // 
            this.lblTotalParties.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTotalParties.AutoSize = true;
            this.lblTotalParties.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTotalParties.Location = new System.Drawing.Point(865, 13);
            this.lblTotalParties.Name = "lblTotalParties";
            this.lblTotalParties.Size = new System.Drawing.Size(81, 15);
            this.lblTotalParties.TabIndex = 0;
            this.lblTotalParties.Text = "Total Parties: ";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Controls.Add(this.btnDelete);
            this.panel1.Controls.Add(this.btnNew);
            this.panel1.Controls.Add(this.btnSave);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 511);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(984, 50);
            this.panel1.TabIndex = 1;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(879, 10);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(93, 28);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDelete.Location = new System.Drawing.Point(212, 10);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(93, 28);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnNew
            // 
            this.btnNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnNew.Location = new System.Drawing.Point(113, 10);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(93, 28);
            this.btnNew.TabIndex = 1;
            this.btnNew.Text = "New";
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSave.Location = new System.Drawing.Point(14, 10);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(93, 28);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // panelForm
            // 
            this.panelForm.Controls.Add(this.txtOutstandingAmount);
            this.panelForm.Controls.Add(this.label9);
            this.panelForm.Controls.Add(this.txtCreditDays);
            this.panelForm.Controls.Add(this.label11);
            this.panelForm.Controls.Add(this.txtCreditLimit);
            this.panelForm.Controls.Add(this.label8);
            this.panelForm.Controls.Add(this.txtEmail);
            this.panelForm.Controls.Add(this.label6);
            this.panelForm.Controls.Add(this.cmbBroker);
            this.panelForm.Controls.Add(this.label7);
            this.panelForm.Controls.Add(this.txtPhone);
            this.panelForm.Controls.Add(this.label5);
            this.panelForm.Controls.Add(this.txtCity);
            this.panelForm.Controls.Add(this.label4);
            this.panelForm.Controls.Add(this.txtAddress);
            this.panelForm.Controls.Add(this.label3);
            this.panelForm.Controls.Add(this.txtPartyName);
            this.panelForm.Controls.Add(this.label2);
            this.panelForm.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelForm.Location = new System.Drawing.Point(684, 40);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(300, 471);
            this.panelForm.TabIndex = 2;
            // 
            // txtOutstandingAmount
            // 
            this.txtOutstandingAmount.Location = new System.Drawing.Point(16, 417);
            this.txtOutstandingAmount.Name = "txtOutstandingAmount";
            this.txtOutstandingAmount.Size = new System.Drawing.Size(260, 23);
            this.txtOutstandingAmount.TabIndex = 17;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(16, 399);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(122, 15);
            this.label9.TabIndex = 16;
            this.label9.Text = "Outstanding Amount:";
            // 
            // txtCreditDays
            // 
            this.txtCreditDays.Location = new System.Drawing.Point(16, 369);
            this.txtCreditDays.Name = "txtCreditDays";
            this.txtCreditDays.Size = new System.Drawing.Size(100, 23);
            this.txtCreditDays.TabIndex = 13;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(16, 351);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(73, 15);
            this.label11.TabIndex = 12;
            this.label11.Text = "Credit Days:";
            // 
            // txtCreditLimit
            // 
            this.txtCreditLimit.Location = new System.Drawing.Point(16, 321);
            this.txtCreditLimit.Name = "txtCreditLimit";
            this.txtCreditLimit.Size = new System.Drawing.Size(260, 23);
            this.txtCreditLimit.TabIndex = 11;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(16, 303);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(71, 15);
            this.label8.TabIndex = 10;
            this.label8.Text = "Credit Limit:";
            // 
            // txtEmail
            // 
            this.txtEmail.Location = new System.Drawing.Point(16, 225);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(260, 23);
            this.txtEmail.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(16, 207);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(39, 15);
            this.label6.TabIndex = 8;
            this.label6.Text = "Email:";
            // 
            // cmbBroker
            // 
            this.cmbBroker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBroker.Location = new System.Drawing.Point(16, 273);
            this.cmbBroker.Name = "cmbBroker";
            this.cmbBroker.Size = new System.Drawing.Size(260, 23);
            this.cmbBroker.TabIndex = 19;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(16, 255);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(45, 15);
            this.label7.TabIndex = 18;
            this.label7.Text = "Broker:";
            // 
            // txtPhone
            // 
            this.txtPhone.Location = new System.Drawing.Point(16, 177);
            this.txtPhone.Name = "txtPhone";
            this.txtPhone.Size = new System.Drawing.Size(260, 23);
            this.txtPhone.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 159);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(44, 15);
            this.label5.TabIndex = 6;
            this.label5.Text = "Phone:";
            // 
            // txtCity
            // 
            this.txtCity.Location = new System.Drawing.Point(16, 129);
            this.txtCity.Name = "txtCity";
            this.txtCity.Size = new System.Drawing.Size(260, 23);
            this.txtCity.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 111);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(31, 15);
            this.label4.TabIndex = 4;
            this.label4.Text = "City:";
            // 
            // txtAddress
            // 
            this.txtAddress.Location = new System.Drawing.Point(16, 60);
            this.txtAddress.Multiline = true;
            this.txtAddress.Name = "txtAddress";
            this.txtAddress.Size = new System.Drawing.Size(260, 43);
            this.txtAddress.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 42);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 15);
            this.label3.TabIndex = 2;
            this.label3.Text = "Address:";
            // 
            // txtPartyName
            // 
            this.txtPartyName.Location = new System.Drawing.Point(16, 16);
            this.txtPartyName.Name = "txtPartyName";
            this.txtPartyName.Size = new System.Drawing.Size(260, 23);
            this.txtPartyName.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 15);
            this.label2.TabIndex = 0;
            this.label2.Text = "Party Name:*";
            // 
            // dgvParties
            // 
            this.dgvParties.AllowUserToAddRows = false;
            this.dgvParties.AllowUserToDeleteRows = false;
            this.dgvParties.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvParties.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvParties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvParties.Location = new System.Drawing.Point(0, 40);
            this.dgvParties.MultiSelect = false;
            this.dgvParties.Name = "dgvParties";
            this.dgvParties.ReadOnly = true;
            this.dgvParties.RowTemplate.Height = 25;
            this.dgvParties.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvParties.Size = new System.Drawing.Size(684, 471);
            this.dgvParties.TabIndex = 3;
            this.dgvParties.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvParties_CellDoubleClick);
            this.dgvParties.SelectionChanged += new System.EventHandler(this.dgvParties_SelectionChanged);
            // 
            // PartyMasterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 561);
            this.Controls.Add(this.dgvParties);
            this.Controls.Add(this.panelForm);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panelTop);
            this.Name = "PartyMasterForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Party Master";
            this.Load += new System.EventHandler(this.PartyMasterForm_Load);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panelForm.ResumeLayout(false);
            this.panelForm.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvParties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Panel panelTop;
        private Label lblTotalParties;
        private Panel panel1;
        private Button btnClose;
        private Button btnDelete;
        private Button btnNew;
        private Button btnSave;
        private Panel panelForm;
        private Label label2;
        private TextBox txtPartyName;
        private TextBox txtAddress;
        private Label label3;
        private TextBox txtCity;
        private Label label4;
        private TextBox txtPhone;
        private Label label5;
        private TextBox txtEmail;
        private Label label6;
        private TextBox txtCreditLimit;
        private Label label8;
        private TextBox txtCreditDays;
        private Label label11;
        private TextBox txtOutstandingAmount;
        private Label label9;
        private DataGridView dgvParties;
        private TextBox txtSearch;
        private Label label10;
        private ComboBox cmbBroker;
        private Label label7;
    }
} 