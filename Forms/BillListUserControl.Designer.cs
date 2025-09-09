namespace SaleBillSystem.NET.Forms
{
    partial class BillListUserControl
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
            panel1 = new Panel();
            cmbBroker = new ComboBox();
            lblBroker = new Label();
            cmbParty = new ComboBox();
            lblParty = new Label();
            cmbStatus = new ComboBox();
            lblStatus = new Label();
            dtpEndDate = new DateTimePicker();
            dtpStartDate = new DateTimePicker();
            lblEndDate = new Label();
            lblStartDate = new Label();
            btnDeleteBill = new Button();
            btnViewDetails = new Button();
            btnEditBill = new Button();
            btnNewBill = new Button();
            btnPrint = new Button();
            dgvBills = new DataGridView();
            panelSummary = new Panel();
            lblChequeFirm2Value = new Label();
            lblChequeFirm2 = new Label();
            lblChequeFirm1Value = new Label();
            lblChequeFirm1 = new Label();
            lblNetAmountValue = new Label();
            lblNetAmount = new Label();
            lblTotalBalanceValue = new Label();
            lblTotalBalance = new Label();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvBills).BeginInit();
            panelSummary.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(cmbBroker);
            panel1.Controls.Add(lblBroker);
            panel1.Controls.Add(cmbParty);
            panel1.Controls.Add(lblParty);
            panel1.Controls.Add(cmbStatus);
            panel1.Controls.Add(lblStatus);
            panel1.Controls.Add(dtpEndDate);
            panel1.Controls.Add(dtpStartDate);
            panel1.Controls.Add(lblEndDate);
            panel1.Controls.Add(lblStartDate);
            panel1.Controls.Add(btnDeleteBill);
            panel1.Controls.Add(btnViewDetails);
            panel1.Controls.Add(btnEditBill);
            panel1.Controls.Add(btnNewBill);
            panel1.Controls.Add(btnPrint);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1564, 91);
            panel1.TabIndex = 0;
            // 
            // cmbBroker
            // 
            cmbBroker.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbBroker.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbBroker.FormattingEnabled = true;
            cmbBroker.Location = new Point(667, 19);
            cmbBroker.Name = "cmbBroker";
            cmbBroker.Size = new Size(140, 23);
            cmbBroker.TabIndex = 14;
            // 
            // lblBroker
            // 
            lblBroker.AutoSize = true;
            lblBroker.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblBroker.Location = new Point(600, 21);
            lblBroker.Name = "lblBroker";
            lblBroker.Size = new Size(61, 20);
            lblBroker.TabIndex = 13;
            lblBroker.Text = "Broker:";
            // 
            // cmbParty
            // 
            cmbParty.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbParty.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbParty.FormattingEnabled = true;
            cmbParty.Location = new Point(446, 19);
            cmbParty.Name = "cmbParty";
            cmbParty.Size = new Size(140, 23);
            cmbParty.TabIndex = 16;
            // 
            // lblParty
            // 
            lblParty.AutoSize = true;
            lblParty.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblParty.Location = new Point(386, 21);
            lblParty.Name = "lblParty";
            lblParty.Size = new Size(54, 21);
            lblParty.TabIndex = 15;
            lblParty.Text = "Party:";
            // 
            // cmbStatus
            // 
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatus.FormattingEnabled = true;
            cmbStatus.Items.AddRange(new object[] { "All", "Paid", "Partial", "Unpaid" });
            cmbStatus.Location = new Point(1166, 59);
            cmbStatus.Name = "cmbStatus";
            cmbStatus.Size = new Size(80, 23);
            cmbStatus.TabIndex = 12;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblStatus.Location = new Point(1166, 26);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(61, 21);
            lblStatus.TabIndex = 11;
            lblStatus.Text = "Status:";
            lblStatus.Click += lblStatus_Click;
            // 
            // dtpEndDate
            // 
            dtpEndDate.Format = DateTimePickerFormat.Short;
            dtpEndDate.Location = new Point(268, 18);
            dtpEndDate.Name = "dtpEndDate";
            dtpEndDate.Size = new Size(100, 23);
            dtpEndDate.TabIndex = 10;
            // 
            // dtpStartDate
            // 
            dtpStartDate.Format = DateTimePickerFormat.Short;
            dtpStartDate.Location = new Point(93, 20);
            dtpStartDate.Name = "dtpStartDate";
            dtpStartDate.Size = new Size(100, 23);
            dtpStartDate.TabIndex = 9;
            // 
            // lblEndDate
            // 
            lblEndDate.AutoSize = true;
            lblEndDate.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblEndDate.Location = new Point(222, 22);
            lblEndDate.Name = "lblEndDate";
            lblEndDate.Size = new Size(30, 20);
            lblEndDate.TabIndex = 8;
            lblEndDate.Text = "To:";
            // 
            // lblStartDate
            // 
            lblStartDate.AutoSize = true;
            lblStartDate.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblStartDate.Location = new Point(33, 24);
            lblStartDate.Name = "lblStartDate";
            lblStartDate.Size = new Size(50, 20);
            lblStartDate.TabIndex = 7;
            lblStartDate.Text = "From:";
            // 
            // btnDeleteBill
            // 
            btnDeleteBill.BackColor = Color.FromArgb(204, 82, 0);
            btnDeleteBill.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            btnDeleteBill.ForeColor = Color.White;
            btnDeleteBill.Location = new Point(957, 55);
            btnDeleteBill.Name = "btnDeleteBill";
            btnDeleteBill.Size = new Size(92, 30);
            btnDeleteBill.TabIndex = 4;
            btnDeleteBill.Text = "Delete";
            btnDeleteBill.UseVisualStyleBackColor = false;
            // 
            // btnViewDetails
            // 
            btnViewDetails.BackColor = Color.FromArgb(0, 150, 136);
            btnViewDetails.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            btnViewDetails.ForeColor = Color.White;
            btnViewDetails.Location = new Point(957, 14);
            btnViewDetails.Name = "btnViewDetails";
            btnViewDetails.Size = new Size(88, 30);
            btnViewDetails.TabIndex = 6;
            btnViewDetails.Text = "View";
            btnViewDetails.UseVisualStyleBackColor = false;
            // 
            // btnEditBill
            // 
            btnEditBill.BackColor = Color.FromArgb(0, 122, 204);
            btnEditBill.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            btnEditBill.ForeColor = Color.White;
            btnEditBill.Location = new Point(826, 53);
            btnEditBill.Name = "btnEditBill";
            btnEditBill.Size = new Size(94, 30);
            btnEditBill.TabIndex = 3;
            btnEditBill.Text = "Edit";
            btnEditBill.UseVisualStyleBackColor = false;
            // 
            // btnNewBill
            // 
            btnNewBill.BackColor = Color.FromArgb(0, 122, 204);
            btnNewBill.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            btnNewBill.ForeColor = Color.White;
            btnNewBill.Location = new Point(826, 11);
            btnNewBill.Name = "btnNewBill";
            btnNewBill.Size = new Size(94, 30);
            btnNewBill.TabIndex = 2;
            btnNewBill.Text = "New Bill";
            btnNewBill.UseVisualStyleBackColor = false;
            // 
            // btnPrint
            // 
            btnPrint.BackColor = Color.FromArgb(0, 150, 136);
            btnPrint.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            btnPrint.ForeColor = Color.White;
            btnPrint.Location = new Point(1063, 53);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(82, 30);
            btnPrint.TabIndex = 13;
            btnPrint.Text = "Print";
            btnPrint.UseVisualStyleBackColor = false;
            // 
            // dgvBills
            // 
            dgvBills.BackgroundColor = SystemColors.Window;
            dgvBills.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvBills.Dock = DockStyle.Fill;
            dgvBills.Location = new Point(0, 91);
            dgvBills.Name = "dgvBills";
            dgvBills.RowHeadersWidth = 51;
            dgvBills.Size = new Size(1564, 539);
            dgvBills.TabIndex = 1;
            dgvBills.CellContentClick += dgvBills_CellContentClick;
            // 
            // panelSummary
            // 
            panelSummary.BackColor = Color.FromArgb(245, 245, 245);
            panelSummary.BorderStyle = BorderStyle.FixedSingle;
            panelSummary.Controls.Add(lblChequeFirm2Value);
            panelSummary.Controls.Add(lblChequeFirm2);
            panelSummary.Controls.Add(lblChequeFirm1Value);
            panelSummary.Controls.Add(lblChequeFirm1);
            panelSummary.Controls.Add(lblNetAmountValue);
            panelSummary.Controls.Add(lblNetAmount);
            panelSummary.Controls.Add(lblTotalBalanceValue);
            panelSummary.Controls.Add(lblTotalBalance);
            panelSummary.Dock = DockStyle.Bottom;
            panelSummary.Location = new Point(0, 630);
            panelSummary.Name = "panelSummary";
            panelSummary.Size = new Size(1564, 40);
            panelSummary.TabIndex = 2;
            // 
            // lblChequeFirm2Value
            // 
            lblChequeFirm2Value.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblChequeFirm2Value.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblChequeFirm2Value.ForeColor = Color.DarkCyan;
            lblChequeFirm2Value.Location = new Point(878, 8);
            lblChequeFirm2Value.Name = "lblChequeFirm2Value";
            lblChequeFirm2Value.Size = new Size(147, 23);
            lblChequeFirm2Value.TabIndex = 11;
            lblChequeFirm2Value.Text = "₹0.00";
            lblChequeFirm2Value.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblChequeFirm2
            // 
            lblChequeFirm2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblChequeFirm2.AutoSize = true;
            lblChequeFirm2.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblChequeFirm2.Location = new Point(751, 9);
            lblChequeFirm2.Name = "lblChequeFirm2";
            lblChequeFirm2.Size = new Size(110, 20);
            lblChequeFirm2.TabIndex = 10;
            lblChequeFirm2.Text = "Cheque Firm2:";
            lblChequeFirm2.Click += lblChequeFirm2_Click;
            // 
            // lblChequeFirm1Value
            // 
            lblChequeFirm1Value.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblChequeFirm1Value.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblChequeFirm1Value.ForeColor = Color.DarkCyan;
            lblChequeFirm1Value.Location = new Point(590, 8);
            lblChequeFirm1Value.Name = "lblChequeFirm1Value";
            lblChequeFirm1Value.Size = new Size(123, 23);
            lblChequeFirm1Value.TabIndex = 9;
            lblChequeFirm1Value.Text = "₹0.00";
            lblChequeFirm1Value.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblChequeFirm1
            // 
            lblChequeFirm1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblChequeFirm1.AutoSize = true;
            lblChequeFirm1.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblChequeFirm1.Location = new Point(451, 8);
            lblChequeFirm1.Name = "lblChequeFirm1";
            lblChequeFirm1.Size = new Size(110, 20);
            lblChequeFirm1.TabIndex = 8;
            lblChequeFirm1.Text = "Cheque Firm1:";
            // 
            // lblNetAmountValue
            // 
            lblNetAmountValue.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblNetAmountValue.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblNetAmountValue.ForeColor = Color.Black;
            lblNetAmountValue.Location = new Point(1179, 10);
            lblNetAmountValue.Name = "lblNetAmountValue";
            lblNetAmountValue.Size = new Size(120, 23);
            lblNetAmountValue.TabIndex = 7;
            lblNetAmountValue.Text = "₹0.00";
            lblNetAmountValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblNetAmount
            // 
            lblNetAmount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblNetAmount.AutoSize = true;
            lblNetAmount.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblNetAmount.Location = new Point(1062, 10);
            lblNetAmount.Name = "lblNetAmount";
            lblNetAmount.Size = new Size(101, 20);
            lblNetAmount.TabIndex = 6;
            lblNetAmount.Text = "Net Amount:";
            // 
            // lblTotalBalanceValue
            // 
            lblTotalBalanceValue.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTotalBalanceValue.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTotalBalanceValue.ForeColor = Color.Red;
            lblTotalBalanceValue.ImageAlign = ContentAlignment.MiddleLeft;
            lblTotalBalanceValue.Location = new Point(1421, 10);
            lblTotalBalanceValue.Name = "lblTotalBalanceValue";
            lblTotalBalanceValue.Size = new Size(120, 23);
            lblTotalBalanceValue.TabIndex = 3;
            lblTotalBalanceValue.Text = "₹0.00";
            lblTotalBalanceValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTotalBalance
            // 
            lblTotalBalance.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTotalBalance.AutoSize = true;
            lblTotalBalance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTotalBalance.Location = new Point(1303, 12);
            lblTotalBalance.Name = "lblTotalBalance";
            lblTotalBalance.Size = new Size(106, 20);
            lblTotalBalance.TabIndex = 1;
            lblTotalBalance.Text = "Total Balance:";
            lblTotalBalance.Click += lblTotalBalance_Click;
            // 
            // BillListUserControl
            // 
            Controls.Add(dgvBills);
            Controls.Add(panelSummary);
            Controls.Add(panel1);
            Name = "BillListUserControl";
            Size = new Size(1564, 670);
            Load += BillListUserControl_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvBills).EndInit();
            panelSummary.ResumeLayout(false);
            panelSummary.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnDeleteBill;
        private System.Windows.Forms.Button btnViewDetails;
        private System.Windows.Forms.Button btnEditBill;
        private System.Windows.Forms.Button btnNewBill;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.ComboBox cmbParty;
        private System.Windows.Forms.Label lblParty;
        private System.Windows.Forms.ComboBox cmbBroker;
        private System.Windows.Forms.Label lblBroker;
        private System.Windows.Forms.DataGridView dgvBills;
        private System.Windows.Forms.DateTimePicker dtpStartDate;
        private System.Windows.Forms.DateTimePicker dtpEndDate;
        private System.Windows.Forms.Label lblStartDate;
        private System.Windows.Forms.Label lblEndDate;
        private System.Windows.Forms.ComboBox cmbStatus;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Panel panelSummary;
        private System.Windows.Forms.Label lblTotalBalanceValue;
        private System.Windows.Forms.Label lblTotalBalance;
        private System.Windows.Forms.Label lblNetAmountValue;
        private System.Windows.Forms.Label lblNetAmount;
        private System.Windows.Forms.Label lblChequeFirm2Value;
        private System.Windows.Forms.Label lblChequeFirm2;
        private System.Windows.Forms.Label lblChequeFirm1Value;
        private System.Windows.Forms.Label lblChequeFirm1;
    }
} 