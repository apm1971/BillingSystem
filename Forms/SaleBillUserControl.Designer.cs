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
            groupBox1 = new GroupBox();
            lblShortcutsInfo = new Label();
            btnQuickAddParty = new Button();
            lblTitle = new Label();
            cmbBroker = new ComboBox();
            btnQuickAddBroker = new Button();
            cmbParty = new ComboBox();
            lblPartyDetails = new Label();
            lblBroker = new Label();
            lblParty = new Label();
            txtBillDate = new TextBox();
            lblBillDate = new Label();
            txtBillNo = new TextBox();
            lblBillNo = new Label();
            groupBox2 = new GroupBox();
            dgvItems = new DataGridView();
            groupBox3 = new GroupBox();
            txtChequeAmountFirm2 = new TextBox();
            lblChequeAmountFirm2 = new Label();
            txtChequeAmountFirm1 = new TextBox();
            lblChequeAmountFirm1 = new Label();
            txtAdditionalCharges = new TextBox();
            lblAdditionalCharges = new Label();
            lblNetAmount = new Label();
            lblTotalCharges = new Label();
            lblTotalAmount = new Label();
            panel1 = new Panel();
            btnSave = new Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvItems).BeginInit();
            groupBox3.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(lblShortcutsInfo);
            groupBox1.Controls.Add(btnQuickAddParty);
            groupBox1.Controls.Add(lblTitle);
            groupBox1.Controls.Add(cmbBroker);
            groupBox1.Controls.Add(btnQuickAddBroker);
            groupBox1.Controls.Add(cmbParty);
            groupBox1.Controls.Add(lblPartyDetails);
            groupBox1.Controls.Add(lblBroker);
            groupBox1.Controls.Add(lblParty);
            groupBox1.Controls.Add(txtBillDate);
            groupBox1.Controls.Add(lblBillDate);
            groupBox1.Controls.Add(txtBillNo);
            groupBox1.Controls.Add(lblBillNo);
            groupBox1.Dock = DockStyle.Top;
            groupBox1.Location = new Point(10, 10);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(1180, 150);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Bill Details";
            // 
            // lblShortcutsInfo
            // 
            lblShortcutsInfo.AutoSize = true;
            lblShortcutsInfo.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold);
            lblShortcutsInfo.ForeColor = Color.DarkBlue;
            lblShortcutsInfo.Location = new Point(955, 134);
            lblShortcutsInfo.Name = "lblShortcutsInfo";
            lblShortcutsInfo.Size = new Size(212, 13);
            lblShortcutsInfo.TabIndex = 6;
            lblShortcutsInfo.Text = "Use Ctrl+I to quickly add a new item";
            // 
            // btnQuickAddParty
            // 
            btnQuickAddParty.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold);
            btnQuickAddParty.Location = new Point(355, 80);
            btnQuickAddParty.Name = "btnQuickAddParty";
            btnQuickAddParty.Size = new Size(30, 21);
            btnQuickAddParty.TabIndex = 11;
            btnQuickAddParty.Text = "+";
            btnQuickAddParty.UseVisualStyleBackColor = true;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Bold);
            lblTitle.Location = new Point(420, 17);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(114, 26);
            lblTitle.TabIndex = 5;
            lblTitle.Text = "New Sale";
            // 
            // cmbBroker
            // 
            cmbBroker.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbBroker.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbBroker.FormattingEnabled = true;
            cmbBroker.Location = new Point(100, 109);
            cmbBroker.Name = "cmbBroker";
            cmbBroker.Size = new Size(250, 23);
            cmbBroker.TabIndex = 3;
            // 
            // btnQuickAddBroker
            // 
            btnQuickAddBroker.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold);
            btnQuickAddBroker.Location = new Point(355, 109);
            btnQuickAddBroker.Name = "btnQuickAddBroker";
            btnQuickAddBroker.Size = new Size(30, 21);
            btnQuickAddBroker.TabIndex = 12;
            btnQuickAddBroker.Text = "+";
            btnQuickAddBroker.UseVisualStyleBackColor = true;
            // 
            // cmbParty
            // 
            cmbParty.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbParty.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbParty.FormattingEnabled = true;
            cmbParty.Location = new Point(100, 80);
            cmbParty.Name = "cmbParty";
            cmbParty.Size = new Size(250, 23);
            cmbParty.TabIndex = 2;
            // 
            // lblPartyDetails
            // 
            lblPartyDetails.AutoSize = true;
            lblPartyDetails.Location = new Point(420, 83);
            lblPartyDetails.Name = "lblPartyDetails";
            lblPartyDetails.Size = new Size(157, 15);
            lblPartyDetails.TabIndex = 10;
            lblPartyDetails.Text = "Party details will appear here";
            // 
            // lblBroker
            // 
            lblBroker.AutoSize = true;
            lblBroker.Location = new Point(20, 112);
            lblBroker.Name = "lblBroker";
            lblBroker.Size = new Size(44, 15);
            lblBroker.TabIndex = 8;
            lblBroker.Text = "Broker:";
            // 
            // lblParty
            // 
            lblParty.AutoSize = true;
            lblParty.Location = new Point(20, 83);
            lblParty.Name = "lblParty";
            lblParty.Size = new Size(37, 15);
            lblParty.TabIndex = 4;
            lblParty.Text = "Party:";
            // 
            // txtBillDate
            // 
            txtBillDate.Location = new Point(100, 53);
            txtBillDate.Name = "txtBillDate";
            txtBillDate.PlaceholderText = "dd-mm-yyyy";
            txtBillDate.Size = new Size(120, 23);
            txtBillDate.TabIndex = 1;
            // 
            // lblBillDate
            // 
            lblBillDate.AutoSize = true;
            lblBillDate.Location = new Point(20, 56);
            lblBillDate.Name = "lblBillDate";
            lblBillDate.Size = new Size(53, 15);
            lblBillDate.TabIndex = 2;
            lblBillDate.Text = "Bill Date:";
            // 
            // txtBillNo
            // 
            txtBillNo.Location = new Point(100, 25);
            txtBillNo.Name = "txtBillNo";
            txtBillNo.Size = new Size(120, 23);
            txtBillNo.TabIndex = 0;
            // 
            // lblBillNo
            // 
            lblBillNo.AutoSize = true;
            lblBillNo.Location = new Point(20, 28);
            lblBillNo.Name = "lblBillNo";
            lblBillNo.Size = new Size(45, 15);
            lblBillNo.TabIndex = 0;
            lblBillNo.Text = "Bill No:";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(dgvItems);
            groupBox2.Dock = DockStyle.Fill;
            groupBox2.Location = new Point(10, 160);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(10);
            groupBox2.Size = new Size(1180, 311);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Bill Items (Press Enter to add new row, F8 to delete)";
            // 
            // dgvItems
            // 
            dgvItems.BackgroundColor = SystemColors.Window;
            dgvItems.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvItems.Dock = DockStyle.Fill;
            dgvItems.Location = new Point(10, 26);
            dgvItems.Name = "dgvItems";
            dgvItems.Size = new Size(1160, 275);
            dgvItems.TabIndex = 0;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(txtChequeAmountFirm2);
            groupBox3.Controls.Add(lblChequeAmountFirm2);
            groupBox3.Controls.Add(txtChequeAmountFirm1);
            groupBox3.Controls.Add(lblChequeAmountFirm1);
            groupBox3.Controls.Add(txtAdditionalCharges);
            groupBox3.Controls.Add(lblAdditionalCharges);
            groupBox3.Controls.Add(lblNetAmount);
            groupBox3.Controls.Add(lblTotalCharges);
            groupBox3.Controls.Add(lblTotalAmount);
            groupBox3.Dock = DockStyle.Bottom;
            groupBox3.Location = new Point(10, 471);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(1180, 90);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "Totals";
            // 
            // txtChequeAmountFirm2
            // 
            txtChequeAmountFirm2.Location = new Point(380, 53);
            txtChequeAmountFirm2.Name = "txtChequeAmountFirm2";
            txtChequeAmountFirm2.Size = new Size(100, 23);
            txtChequeAmountFirm2.TabIndex = 7;
            txtChequeAmountFirm2.Text = "0.00";
            // 
            // lblChequeAmountFirm2
            // 
            lblChequeAmountFirm2.AutoSize = true;
            lblChequeAmountFirm2.Font = new Font("Microsoft Sans Serif", 9.75F);
            lblChequeAmountFirm2.Location = new Point(250, 53);
            lblChequeAmountFirm2.Name = "lblChequeAmountFirm2";
            lblChequeAmountFirm2.Size = new Size(119, 16);
            lblChequeAmountFirm2.TabIndex = 6;
            lblChequeAmountFirm2.Text = "Cheque Amt Firm2:";
            // 
            // txtChequeAmountFirm1
            // 
            txtChequeAmountFirm1.Location = new Point(150, 53);
            txtChequeAmountFirm1.Name = "txtChequeAmountFirm1";
            txtChequeAmountFirm1.Size = new Size(100, 23);
            txtChequeAmountFirm1.TabIndex = 5;
            txtChequeAmountFirm1.Text = "0.00";
            // 
            // lblChequeAmountFirm1
            // 
            lblChequeAmountFirm1.AutoSize = true;
            lblChequeAmountFirm1.Font = new Font("Microsoft Sans Serif", 9.75F);
            lblChequeAmountFirm1.Location = new Point(20, 53);
            lblChequeAmountFirm1.Name = "lblChequeAmountFirm1";
            lblChequeAmountFirm1.Size = new Size(119, 16);
            lblChequeAmountFirm1.TabIndex = 4;
            lblChequeAmountFirm1.Text = "Cheque Amt Firm1:";
            // 
            // txtAdditionalCharges
            // 
            txtAdditionalCharges.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold);
            txtAdditionalCharges.Location = new Point(660, 17);
            txtAdditionalCharges.Name = "txtAdditionalCharges";
            txtAdditionalCharges.Size = new Size(120, 22);
            txtAdditionalCharges.TabIndex = 0;
            txtAdditionalCharges.Text = "0.00";
            txtAdditionalCharges.TextAlign = HorizontalAlignment.Right;
            // 
            // lblAdditionalCharges
            // 
            lblAdditionalCharges.AutoSize = true;
            lblAdditionalCharges.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold);
            lblAdditionalCharges.Location = new Point(511, 20);
            lblAdditionalCharges.Name = "lblAdditionalCharges";
            lblAdditionalCharges.Size = new Size(143, 16);
            lblAdditionalCharges.TabIndex = 3;
            lblAdditionalCharges.Text = "Additional Charges:";
            // 
            // lblNetAmount
            // 
            lblNetAmount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblNetAmount.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblNetAmount.ForeColor = Color.FromArgb(0, 0, 192);
            lblNetAmount.Location = new Point(845, 23);
            lblNetAmount.Name = "lblNetAmount";
            lblNetAmount.Size = new Size(280, 24);
            lblNetAmount.TabIndex = 2;
            lblNetAmount.Text = "NET AMOUNT: ₹0.00";
            lblNetAmount.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblTotalCharges
            // 
            lblTotalCharges.AutoSize = true;
            lblTotalCharges.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold);
            lblTotalCharges.Location = new Point(210, 23);
            lblTotalCharges.Name = "lblTotalCharges";
            lblTotalCharges.Size = new Size(142, 16);
            lblTotalCharges.TabIndex = 1;
            lblTotalCharges.Text = "Item Charges: ₹0.00";
            // 
            // lblTotalAmount
            // 
            lblTotalAmount.AutoSize = true;
            lblTotalAmount.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold);
            lblTotalAmount.Location = new Point(20, 23);
            lblTotalAmount.Name = "lblTotalAmount";
            lblTotalAmount.Size = new Size(120, 16);
            lblTotalAmount.TabIndex = 0;
            lblTotalAmount.Text = "Item Total: ₹0.00";
            // 
            // panel1
            // 
            panel1.Controls.Add(btnSave);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(10, 561);
            panel1.Name = "panel1";
            panel1.Size = new Size(1180, 50);
            panel1.TabIndex = 3;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSave.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnSave.Location = new Point(1025, 6);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(100, 30);
            btnSave.TabIndex = 0;
            btnSave.Text = "Save (Ctrl+S)";
            btnSave.UseVisualStyleBackColor = true;
            // 
            // SaleBillUserControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(groupBox2);
            Controls.Add(groupBox3);
            Controls.Add(groupBox1);
            Controls.Add(panel1);
            Name = "SaleBillUserControl";
            Padding = new Padding(10);
            Size = new Size(1200, 621);
            Load += SaleBillUserControl_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvItems).EndInit();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
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
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.ComboBox cmbParty;
        private System.Windows.Forms.ComboBox cmbBroker;
        private System.Windows.Forms.TextBox txtChequeAmountFirm1;
        private System.Windows.Forms.Label lblChequeAmountFirm1;
        private System.Windows.Forms.TextBox txtChequeAmountFirm2;
        private System.Windows.Forms.Label lblChequeAmountFirm2;
        private System.Windows.Forms.Button btnQuickAddParty;
        private System.Windows.Forms.Button btnQuickAddBroker;
        private System.Windows.Forms.Label lblShortcutsInfo;
        private Label lblTitle;
    }
}
