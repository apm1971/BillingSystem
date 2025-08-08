namespace SaleBillSystem.NET.Forms
{
    partial class SaleBillUserControl
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnQuickAddParty = new System.Windows.Forms.Button();
            this.cmbBroker = new System.Windows.Forms.ComboBox();
            this.btnQuickAddBroker = new System.Windows.Forms.Button();
            this.cmbParty = new System.Windows.Forms.ComboBox();
            this.lblPartyDetails = new System.Windows.Forms.Label();
            this.lblBroker = new System.Windows.Forms.Label();
            this.lblParty = new System.Windows.Forms.Label();
            this.txtBillDate = new System.Windows.Forms.TextBox();
            this.lblBillDate = new System.Windows.Forms.Label();
            this.txtBillNo = new System.Windows.Forms.TextBox();
            this.lblBillNo = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dgvItems = new System.Windows.Forms.DataGridView();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txtChequeAmountFirm2 = new System.Windows.Forms.TextBox();
            this.lblChequeAmountFirm2 = new System.Windows.Forms.Label();
            this.txtChequeAmountFirm1 = new System.Windows.Forms.TextBox();
            this.lblChequeAmountFirm1 = new System.Windows.Forms.Label();
            this.txtAdditionalCharges = new System.Windows.Forms.TextBox();
            this.lblAdditionalCharges = new System.Windows.Forms.Label();
            this.lblNetAmount = new System.Windows.Forms.Label();
            this.lblTotalCharges = new System.Windows.Forms.Label();
            this.lblTotalAmount = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblShortcutsInfo = new System.Windows.Forms.Label();

            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvItems)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.Location = new System.Drawing.Point(529, 11);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(118, 26);
            this.lblTitle.TabIndex = 5;
            this.lblTitle.Text = "New Sale";
            // 
            // lblShortcutsInfo
            // 
            this.lblShortcutsInfo.AutoSize = true;
            this.lblShortcutsInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblShortcutsInfo.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblShortcutsInfo.Location = new System.Drawing.Point(529, 37);
            this.lblShortcutsInfo.Name = "lblShortcutsInfo";
            this.lblShortcutsInfo.Size = new System.Drawing.Size(230, 13);
            this.lblShortcutsInfo.TabIndex = 6;
            this.lblShortcutsInfo.Text = "Use Ctrl+I to quickly add a new item";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnQuickAddParty);
            this.groupBox1.Controls.Add(this.cmbBroker);
            this.groupBox1.Controls.Add(this.btnQuickAddBroker);
            this.groupBox1.Controls.Add(this.cmbParty);
            this.groupBox1.Controls.Add(this.lblPartyDetails);
            this.groupBox1.Controls.Add(this.lblBroker);
            this.groupBox1.Controls.Add(this.lblParty);
            this.groupBox1.Controls.Add(this.txtBillDate);
            this.groupBox1.Controls.Add(this.lblBillDate);
            this.groupBox1.Controls.Add(this.txtBillNo);
            this.groupBox1.Controls.Add(this.lblBillNo);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(10, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1180, 150);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Bill Details";
            // 
            // btnQuickAddParty
            // 
            this.btnQuickAddParty.Location = new System.Drawing.Point(355, 80);
            this.btnQuickAddParty.Name = "btnQuickAddParty";
            this.btnQuickAddParty.Size = new System.Drawing.Size(30, 21);
            this.btnQuickAddParty.TabIndex = 11;
            this.btnQuickAddParty.Text = "+";
            this.btnQuickAddParty.UseVisualStyleBackColor = true;
            this.btnQuickAddParty.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            // 
            // cmbBroker
            // 
            this.cmbBroker.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbBroker.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbBroker.FormattingEnabled = true;
            this.cmbBroker.Location = new System.Drawing.Point(100, 109);
            this.cmbBroker.Name = "cmbBroker";
            this.cmbBroker.Size = new System.Drawing.Size(250, 21);
            this.cmbBroker.TabIndex = 3;
            // 
            // btnQuickAddBroker
            // 
            this.btnQuickAddBroker.Location = new System.Drawing.Point(355, 109);
            this.btnQuickAddBroker.Name = "btnQuickAddBroker";
            this.btnQuickAddBroker.Size = new System.Drawing.Size(30, 21);
            this.btnQuickAddBroker.TabIndex = 12;
            this.btnQuickAddBroker.Text = "+";
            this.btnQuickAddBroker.UseVisualStyleBackColor = true;
            this.btnQuickAddBroker.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            // 
            // cmbParty
            // 
            this.cmbParty.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbParty.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbParty.FormattingEnabled = true;
            this.cmbParty.Location = new System.Drawing.Point(100, 80);
            this.cmbParty.Name = "cmbParty";
            this.cmbParty.Size = new System.Drawing.Size(250, 21);
            this.cmbParty.TabIndex = 2;
            // 
            // lblPartyDetails
            // 
            this.lblPartyDetails.AutoSize = true;
            this.lblPartyDetails.Location = new System.Drawing.Point(420, 83);
            this.lblPartyDetails.Name = "lblPartyDetails";
            this.lblPartyDetails.Size = new System.Drawing.Size(126, 13);
            this.lblPartyDetails.TabIndex = 10;
            this.lblPartyDetails.Text = "Party details will appear here";
            // 
            // lblBroker
            // 
            this.lblBroker.AutoSize = true;
            this.lblBroker.Location = new System.Drawing.Point(20, 112);
            this.lblBroker.Name = "lblBroker";
            this.lblBroker.Size = new System.Drawing.Size(41, 13);
            this.lblBroker.TabIndex = 8;
            this.lblBroker.Text = "Broker:";
            // 
            // lblParty
            // 
            this.lblParty.AutoSize = true;
            this.lblParty.Location = new System.Drawing.Point(20, 83);
            this.lblParty.Name = "lblParty";
            this.lblParty.Size = new System.Drawing.Size(34, 13);
            this.lblParty.TabIndex = 4;
            this.lblParty.Text = "Party:";
            // 
            // txtBillDate
            // 
            this.txtBillDate.Location = new System.Drawing.Point(100, 53);
            this.txtBillDate.Name = "txtBillDate";
            this.txtBillDate.Size = new System.Drawing.Size(120, 20);
            this.txtBillDate.TabIndex = 1;
            this.txtBillDate.PlaceholderText = "dd-mm-yyyy";
            // 
            // lblBillDate
            // 
            this.lblBillDate.AutoSize = true;
            this.lblBillDate.Location = new System.Drawing.Point(20, 56);
            this.lblBillDate.Name = "lblBillDate";
            this.lblBillDate.Size = new System.Drawing.Size(52, 13);
            this.lblBillDate.TabIndex = 2;
            this.lblBillDate.Text = "Bill Date:";
            // 
            // txtBillNo
            // 
            this.txtBillNo.Location = new System.Drawing.Point(100, 25);
            this.txtBillNo.Name = "txtBillNo";
            this.txtBillNo.Size = new System.Drawing.Size(120, 20);
            this.txtBillNo.TabIndex = 0;
            // 
            // lblBillNo
            // 
            this.lblBillNo.AutoSize = true;
            this.lblBillNo.Location = new System.Drawing.Point(20, 28);
            this.lblBillNo.Name = "lblBillNo";
            this.lblBillNo.Size = new System.Drawing.Size(43, 13);
            this.lblBillNo.TabIndex = 0;
            this.lblBillNo.Text = "Bill No:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.dgvItems);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(10, 160);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(10);
            this.groupBox2.Size = new System.Drawing.Size(1180, 331);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Bill Items (Press Enter to add new row, F8 to delete)";
            // 
            // dgvItems
            // 
            this.dgvItems.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvItems.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvItems.Location = new System.Drawing.Point(10, 23);
            this.dgvItems.Name = "dgvItems";
            this.dgvItems.Size = new System.Drawing.Size(1160, 298);
            this.dgvItems.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtChequeAmountFirm2);
            this.groupBox3.Controls.Add(this.lblChequeAmountFirm2);
            this.groupBox3.Controls.Add(this.txtChequeAmountFirm1);
            this.groupBox3.Controls.Add(this.lblChequeAmountFirm1);
            this.groupBox3.Controls.Add(this.txtAdditionalCharges);
            this.groupBox3.Controls.Add(this.lblAdditionalCharges);
            this.groupBox3.Controls.Add(this.lblNetAmount);
            this.groupBox3.Controls.Add(this.lblTotalCharges);
            this.groupBox3.Controls.Add(this.lblTotalAmount);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox3.Location = new System.Drawing.Point(10, 491);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(1180, 90);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Totals";
            // 
            // txtChequeAmountFirm1
            // 
            this.txtChequeAmountFirm1.Location = new System.Drawing.Point(150, 53);
            this.txtChequeAmountFirm1.Name = "txtChequeAmountFirm1";
            this.txtChequeAmountFirm1.Size = new System.Drawing.Size(100, 20);
            this.txtChequeAmountFirm1.TabIndex = 5;
            this.txtChequeAmountFirm1.Text = "0.00";
            // 
            // lblChequeAmountFirm1
            // 
            this.lblChequeAmountFirm1.AutoSize = true;
            this.lblChequeAmountFirm1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.lblChequeAmountFirm1.Location = new System.Drawing.Point(20, 53);
            this.lblChequeAmountFirm1.Name = "lblChequeAmountFirm1";
            this.lblChequeAmountFirm1.Size = new System.Drawing.Size(128, 16);
            this.lblChequeAmountFirm1.TabIndex = 4;
            this.lblChequeAmountFirm1.Text = "Cheque Amt Firm1:";
            // 
            // txtChequeAmountFirm2
            // 
            this.txtChequeAmountFirm2.Location = new System.Drawing.Point(380, 53);
            this.txtChequeAmountFirm2.Name = "txtChequeAmountFirm2";
            this.txtChequeAmountFirm2.Size = new System.Drawing.Size(100, 20);
            this.txtChequeAmountFirm2.TabIndex = 7;
            this.txtChequeAmountFirm2.Text = "0.00";
            // 
            // lblChequeAmountFirm2
            // 
            this.lblChequeAmountFirm2.AutoSize = true;
            this.lblChequeAmountFirm2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.lblChequeAmountFirm2.Location = new System.Drawing.Point(250, 53);
            this.lblChequeAmountFirm2.Name = "lblChequeAmountFirm2";
            this.lblChequeAmountFirm2.Size = new System.Drawing.Size(128, 16);
            this.lblChequeAmountFirm2.TabIndex = 6;
            this.lblChequeAmountFirm2.Text = "Cheque Amt Firm2:";
            // 
            // txtAdditionalCharges
            // 
            this.txtAdditionalCharges.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.txtAdditionalCharges.Location = new System.Drawing.Point(650, 53);
            this.txtAdditionalCharges.Name = "txtAdditionalCharges";
            this.txtAdditionalCharges.Size = new System.Drawing.Size(120, 22);
            this.txtAdditionalCharges.TabIndex = 0;
            this.txtAdditionalCharges.Text = "0.00";
            this.txtAdditionalCharges.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // lblAdditionalCharges
            // 
            this.lblAdditionalCharges.AutoSize = true;
            this.lblAdditionalCharges.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblAdditionalCharges.Location = new System.Drawing.Point(500, 53);
            this.lblAdditionalCharges.Name = "lblAdditionalCharges";
            this.lblAdditionalCharges.Size = new System.Drawing.Size(134, 16);
            this.lblAdditionalCharges.TabIndex = 3;
            this.lblAdditionalCharges.Text = "Additional Charges:";
            // 
            // lblNetAmount
            // 
            this.lblNetAmount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNetAmount.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.lblNetAmount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.lblNetAmount.Location = new System.Drawing.Point(880, 23);
            this.lblNetAmount.Name = "lblNetAmount";
            this.lblNetAmount.Size = new System.Drawing.Size(280, 24);
            this.lblNetAmount.TabIndex = 2;
            this.lblNetAmount.Text = "NET AMOUNT: ₹0.00";
            this.lblNetAmount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblTotalCharges
            // 
            this.lblTotalCharges.AutoSize = true;
            this.lblTotalCharges.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblTotalCharges.Location = new System.Drawing.Point(210, 23);
            this.lblTotalCharges.Name = "lblTotalCharges";
            this.lblTotalCharges.Size = new System.Drawing.Size(139, 16);
            this.lblTotalCharges.TabIndex = 1;
            this.lblTotalCharges.Text = "Item Charges: ₹0.00";
            // 
            // lblTotalAmount
            // 
            this.lblTotalAmount.AutoSize = true;
            this.lblTotalAmount.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblTotalAmount.Location = new System.Drawing.Point(20, 23);
            this.lblTotalAmount.Name = "lblTotalAmount";
            this.lblTotalAmount.Size = new System.Drawing.Size(126, 16);
            this.lblTotalAmount.TabIndex = 0;
            this.lblTotalAmount.Text = "Item Total: ₹0.00";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnSave);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(10, 561);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1180, 50);
            this.panel1.TabIndex = 3;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(1067, 10);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 30);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Close (Esc)";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(961, 10);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 30);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Save (Ctrl+S)";
            this.btnSave.UseVisualStyleBackColor = true;
            // 

            // 
            // SaleBillUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblShortcutsInfo);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.Name = "SaleBillUserControl";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Size = new System.Drawing.Size(1200, 621);
            this.Load += new System.EventHandler(this.SaleBillUserControl_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvItems)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblPartyDetails;
        private System.Windows.Forms.Label lblBroker;
        private System.Windows.Forms.Label lblParty;
        private System.Windows.Forms.TextBox txtBillDate;
        private System.Windows.Forms.Label lblBillDate;
        private System.Windows.Forms.TextBox txtBillNo;
        private System.Windows.Forms.Label lblBillNo;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView dgvItems;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox txtAdditionalCharges;
        private System.Windows.Forms.Label lblAdditionalCharges;
        private System.Windows.Forms.Label lblNetAmount;
        private System.Windows.Forms.Label lblTotalCharges;
        private System.Windows.Forms.Label lblTotalAmount;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.ComboBox cmbParty;
        private System.Windows.Forms.ComboBox cmbBroker;
        private System.Windows.Forms.TextBox txtChequeAmountFirm1;
        private System.Windows.Forms.Label lblChequeAmountFirm1;
        private System.Windows.Forms.TextBox txtChequeAmountFirm2;
        private System.Windows.Forms.Label lblChequeAmountFirm2;
        private System.Windows.Forms.Button btnQuickAddParty;
        private System.Windows.Forms.Button btnQuickAddBroker;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblShortcutsInfo;
    }
}
