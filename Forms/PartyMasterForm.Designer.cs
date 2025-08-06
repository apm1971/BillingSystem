namespace SaleBillSystem.NET.Forms
{
    partial class PartyMasterUserControl
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.lblTotalParties = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dgvParties = new System.Windows.Forms.DataGridView();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.cmbBroker = new System.Windows.Forms.ComboBox();
            this.lblBroker = new System.Windows.Forms.Label();
            this.txtPhone = new System.Windows.Forms.TextBox();
            this.lblPhone = new System.Windows.Forms.Label();
            this.txtAddress = new System.Windows.Forms.TextBox();
            this.lblAddress = new System.Windows.Forms.Label();
            this.txtPartyName = new System.Windows.Forms.TextBox();
            this.lblPartyName = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnNew = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvParties)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtSearch);
            this.groupBox1.Controls.Add(this.lblSearch);
            this.groupBox1.Controls.Add(this.lblTotalParties);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(10, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1180, 80);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Search & Navigation";
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(100, 30);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(300, 20);
            this.txtSearch.TabIndex = 0;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new System.Drawing.Point(20, 33);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(74, 13);
            this.lblSearch.TabIndex = 1;
            this.lblSearch.Text = "Search Party:";
            // 
            // lblTotalParties
            // 
            this.lblTotalParties.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTotalParties.AutoSize = true;
            this.lblTotalParties.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalParties.Location = new System.Drawing.Point(1050, 33);
            this.lblTotalParties.Name = "lblTotalParties";
            this.lblTotalParties.Size = new System.Drawing.Size(100, 15);
            this.lblTotalParties.TabIndex = 2;
            this.lblTotalParties.Text = "Total Parties: 0";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.dgvParties);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(10, 90);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(880, 460);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Party List";
            // 
            // dgvParties
            // 
            this.dgvParties.AllowUserToAddRows = false;
            this.dgvParties.AllowUserToDeleteRows = false;
            this.dgvParties.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvParties.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvParties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvParties.Location = new System.Drawing.Point(3, 16);
            this.dgvParties.MultiSelect = false;
            this.dgvParties.Name = "dgvParties";
            this.dgvParties.ReadOnly = true;
            this.dgvParties.RowTemplate.Height = 25;
            this.dgvParties.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvParties.Size = new System.Drawing.Size(874, 441);
            this.dgvParties.TabIndex = 0;
            this.dgvParties.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvParties_CellDoubleClick);
            this.dgvParties.SelectionChanged += new System.EventHandler(this.dgvParties_SelectionChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.cmbBroker);
            this.groupBox3.Controls.Add(this.lblBroker);
            this.groupBox3.Controls.Add(this.txtPhone);
            this.groupBox3.Controls.Add(this.lblPhone);
            this.groupBox3.Controls.Add(this.txtAddress);
            this.groupBox3.Controls.Add(this.lblAddress);
            this.groupBox3.Controls.Add(this.txtPartyName);
            this.groupBox3.Controls.Add(this.lblPartyName);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Right;
            this.groupBox3.Location = new System.Drawing.Point(890, 90);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(300, 460);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Party Details";
            // 
            // cmbBroker
            // 
            this.cmbBroker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBroker.Location = new System.Drawing.Point(20, 200);
            this.cmbBroker.Name = "cmbBroker";
            this.cmbBroker.Size = new System.Drawing.Size(260, 21);
            this.cmbBroker.TabIndex = 3;
            // 
            // lblBroker
            // 
            this.lblBroker.AutoSize = true;
            this.lblBroker.Location = new System.Drawing.Point(20, 184);
            this.lblBroker.Name = "lblBroker";
            this.lblBroker.Size = new System.Drawing.Size(41, 13);
            this.lblBroker.TabIndex = 6;
            this.lblBroker.Text = "Broker:";
            // 
            // txtPhone
            // 
            this.txtPhone.Location = new System.Drawing.Point(20, 150);
            this.txtPhone.Name = "txtPhone";
            this.txtPhone.Size = new System.Drawing.Size(260, 20);
            this.txtPhone.TabIndex = 2;
            // 
            // lblPhone
            // 
            this.lblPhone.AutoSize = true;
            this.lblPhone.Location = new System.Drawing.Point(20, 134);
            this.lblPhone.Name = "lblPhone";
            this.lblPhone.Size = new System.Drawing.Size(41, 13);
            this.lblPhone.TabIndex = 4;
            this.lblPhone.Text = "Phone:";
            // 
            // txtAddress
            // 
            this.txtAddress.Location = new System.Drawing.Point(20, 90);
            this.txtAddress.Multiline = true;
            this.txtAddress.Name = "txtAddress";
            this.txtAddress.Size = new System.Drawing.Size(260, 40);
            this.txtAddress.TabIndex = 1;
            // 
            // lblAddress
            // 
            this.lblAddress.AutoSize = true;
            this.lblAddress.Location = new System.Drawing.Point(20, 74);
            this.lblAddress.Name = "lblAddress";
            this.lblAddress.Size = new System.Drawing.Size(48, 13);
            this.lblAddress.TabIndex = 2;
            this.lblAddress.Text = "Address:";
            // 
            // txtPartyName
            // 
            this.txtPartyName.Location = new System.Drawing.Point(20, 30);
            this.txtPartyName.Name = "txtPartyName";
            this.txtPartyName.Size = new System.Drawing.Size(260, 20);
            this.txtPartyName.TabIndex = 0;
            // 
            // lblPartyName
            // 
            this.lblPartyName.AutoSize = true;
            this.lblPartyName.Location = new System.Drawing.Point(20, 14);
            this.lblPartyName.Name = "lblPartyName";
            this.lblPartyName.Size = new System.Drawing.Size(68, 13);
            this.lblPartyName.TabIndex = 0;
            this.lblPartyName.Text = "Party Name:*";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnDelete);
            this.panel1.Controls.Add(this.btnNew);
            this.panel1.Controls.Add(this.btnSave);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(10, 550);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1180, 50);
            this.panel1.TabIndex = 3;
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(200, 12);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(90, 28);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnNew
            // 
            this.btnNew.Location = new System.Drawing.Point(104, 12);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(90, 28);
            this.btnNew.TabIndex = 1;
            this.btnNew.Text = "New";
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(8, 12);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 28);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // PartyMasterUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.Name = "PartyMasterUserControl";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Size = new System.Drawing.Size(1200, 610);
            this.Load += new System.EventHandler(this.PartyMasterUserControl_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvParties)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.Label lblTotalParties;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView dgvParties;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox cmbBroker;
        private System.Windows.Forms.Label lblBroker;
        private System.Windows.Forms.TextBox txtPhone;
        private System.Windows.Forms.Label lblPhone;
        private System.Windows.Forms.TextBox txtAddress;
        private System.Windows.Forms.Label lblAddress;
        private System.Windows.Forms.TextBox txtPartyName;
        private System.Windows.Forms.Label lblPartyName;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.Button btnSave;
    }
}