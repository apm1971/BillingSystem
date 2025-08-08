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
            cmbStatus = new ComboBox();
            lblStatus = new Label();
            dtpEndDate = new DateTimePicker();
            dtpStartDate = new DateTimePicker();
            lblEndDate = new Label();
            lblStartDate = new Label();
            btnRefresh = new Button();
            btnDeleteBill = new Button();
            btnViewDetails = new Button();
            btnEditBill = new Button();
            btnNewBill = new Button();
            txtSearch = new TextBox();
            lblSearch = new Label();
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
            panel1.Controls.Add(cmbStatus);
            panel1.Controls.Add(lblStatus);
            panel1.Controls.Add(dtpEndDate);
            panel1.Controls.Add(dtpStartDate);
            panel1.Controls.Add(lblEndDate);
            panel1.Controls.Add(lblStartDate);
            panel1.Controls.Add(btnRefresh);
            panel1.Controls.Add(btnDeleteBill);
            panel1.Controls.Add(btnViewDetails);
            panel1.Controls.Add(btnEditBill);
            panel1.Controls.Add(btnNewBill);
            panel1.Controls.Add(txtSearch);
            panel1.Controls.Add(lblSearch);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1314, 60);
            panel1.TabIndex = 0;
            // 
            // cmbStatus
            // 
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatus.FormattingEnabled = true;
            cmbStatus.Items.AddRange(new object[] { "All", "Paid", "Partial", "Unpaid" });
            cmbStatus.Location = new Point(640, 18);
            cmbStatus.Name = "cmbStatus";
            cmbStatus.Size = new Size(80, 28);
            cmbStatus.TabIndex = 12;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(595, 21);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(52, 20);
            lblStatus.TabIndex = 11;
            lblStatus.Text = "Status:";
            // 
            // dtpEndDate
            // 
            dtpEndDate.Format = DateTimePickerFormat.Short;
            dtpEndDate.Location = new Point(490, 18);
            dtpEndDate.Name = "dtpEndDate";
            dtpEndDate.Size = new Size(100, 27);
            dtpEndDate.TabIndex = 10;
            // 
            // dtpStartDate
            // 
            dtpStartDate.Format = DateTimePickerFormat.Short;
            dtpStartDate.Location = new Point(350, 18);
            dtpStartDate.Name = "dtpStartDate";
            dtpStartDate.Size = new Size(100, 27);
            dtpStartDate.TabIndex = 9;
            // 
            // lblEndDate
            // 
            lblEndDate.AutoSize = true;
            lblEndDate.Location = new Point(460, 21);
            lblEndDate.Name = "lblEndDate";
            lblEndDate.Size = new Size(28, 20);
            lblEndDate.TabIndex = 8;
            lblEndDate.Text = "To:";
            // 
            // lblStartDate
            // 
            lblStartDate.AutoSize = true;
            lblStartDate.Location = new Point(315, 21);
            lblStartDate.Name = "lblStartDate";
            lblStartDate.Size = new Size(46, 20);
            lblStartDate.TabIndex = 7;
            lblStartDate.Text = "From:";
            // 
            // btnRefresh
            // 
            btnRefresh.BackColor = Color.FromArgb(0, 122, 204);
            btnRefresh.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.Location = new Point(1175, 15);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(100, 30);
            btnRefresh.TabIndex = 5;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = false;
            // 
            // btnDeleteBill
            // 
            btnDeleteBill.BackColor = Color.FromArgb(204, 82, 0);
            btnDeleteBill.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            btnDeleteBill.ForeColor = Color.White;
            btnDeleteBill.Location = new Point(1060, 15);
            btnDeleteBill.Name = "btnDeleteBill";
            btnDeleteBill.Size = new Size(100, 30);
            btnDeleteBill.TabIndex = 4;
            btnDeleteBill.Text = "Delete";
            btnDeleteBill.UseVisualStyleBackColor = false;
            // 
            // btnViewDetails
            // 
            btnViewDetails.BackColor = Color.FromArgb(0, 150, 136);
            btnViewDetails.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            btnViewDetails.ForeColor = Color.White;
            btnViewDetails.Location = new Point(840, 15);
            btnViewDetails.Name = "btnViewDetails";
            btnViewDetails.Size = new Size(100, 30);
            btnViewDetails.TabIndex = 6;
            btnViewDetails.Text = "View Details";
            btnViewDetails.UseVisualStyleBackColor = false;
            // 
            // btnEditBill
            // 
            btnEditBill.BackColor = Color.FromArgb(0, 122, 204);
            btnEditBill.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            btnEditBill.ForeColor = Color.White;
            btnEditBill.Location = new Point(950, 15);
            btnEditBill.Name = "btnEditBill";
            btnEditBill.Size = new Size(100, 30);
            btnEditBill.TabIndex = 3;
            btnEditBill.Text = "Edit";
            btnEditBill.UseVisualStyleBackColor = false;
            // 
            // btnNewBill
            // 
            btnNewBill.BackColor = Color.FromArgb(0, 122, 204);
            btnNewBill.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            btnNewBill.ForeColor = Color.White;
            btnNewBill.Location = new Point(730, 15);
            btnNewBill.Name = "btnNewBill";
            btnNewBill.Size = new Size(100, 30);
            btnNewBill.TabIndex = 2;
            btnNewBill.Text = "New Bill";
            btnNewBill.UseVisualStyleBackColor = false;
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(100, 18);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(200, 27);
            txtSearch.TabIndex = 1;
            // 
            // lblSearch
            // 
            lblSearch.AutoSize = true;
            lblSearch.Location = new Point(20, 21);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(56, 20);
            lblSearch.TabIndex = 0;
            lblSearch.Text = "Search:";
            // 
            // dgvBills
            // 
            dgvBills.BackgroundColor = SystemColors.Window;
            dgvBills.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvBills.Dock = DockStyle.Fill;
            dgvBills.Location = new Point(0, 60);
            dgvBills.Name = "dgvBills";
            dgvBills.RowHeadersWidth = 51;
            dgvBills.Size = new Size(1314, 570);
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
            panelSummary.Size = new Size(1314, 40);
            panelSummary.TabIndex = 2;
            // 
            // lblChequeFirm2Value
            // 
            lblChequeFirm2Value.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblChequeFirm2Value.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblChequeFirm2Value.ForeColor = Color.DarkCyan;
            lblChequeFirm2Value.Location = new Point(678, 12);
            lblChequeFirm2Value.Name = "lblChequeFirm2Value";
            lblChequeFirm2Value.Size = new Size(100, 23);
            lblChequeFirm2Value.TabIndex = 11;
            lblChequeFirm2Value.Text = "₹0.00";
            lblChequeFirm2Value.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblChequeFirm2
            // 
            lblChequeFirm2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblChequeFirm2.AutoSize = true;
            lblChequeFirm2.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblChequeFirm2.Location = new Point(532, 11);
            lblChequeFirm2.Name = "lblChequeFirm2";
            lblChequeFirm2.Size = new Size(140, 25);
            lblChequeFirm2.TabIndex = 10;
            lblChequeFirm2.Text = "Cheque Firm2:";
            lblChequeFirm2.Click += lblChequeFirm2_Click;
            // 
            // lblChequeFirm1Value
            // 
            lblChequeFirm1Value.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblChequeFirm1Value.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblChequeFirm1Value.ForeColor = Color.DarkCyan;
            lblChequeFirm1Value.Location = new Point(421, 12);
            lblChequeFirm1Value.Name = "lblChequeFirm1Value";
            lblChequeFirm1Value.Size = new Size(85, 23);
            lblChequeFirm1Value.TabIndex = 9;
            lblChequeFirm1Value.Text = "₹0.00";
            lblChequeFirm1Value.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblChequeFirm1
            // 
            lblChequeFirm1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblChequeFirm1.AutoSize = true;
            lblChequeFirm1.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblChequeFirm1.Location = new Point(263, 10);
            lblChequeFirm1.Name = "lblChequeFirm1";
            lblChequeFirm1.Size = new Size(140, 25);
            lblChequeFirm1.TabIndex = 8;
            lblChequeFirm1.Text = "Cheque Firm1:";
            // 
            // lblNetAmountValue
            // 
            lblNetAmountValue.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblNetAmountValue.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblNetAmountValue.ForeColor = Color.Black;
            lblNetAmountValue.Location = new Point(929, 10);
            lblNetAmountValue.Name = "lblNetAmountValue";
            lblNetAmountValue.Size = new Size(120, 23);
            lblNetAmountValue.TabIndex = 7;
            lblNetAmountValue.Text = "₹0.00";
            lblNetAmountValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblNetAmount
            // 
            lblNetAmount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblNetAmount.AutoSize = true;
            lblNetAmount.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblNetAmount.Location = new Point(800, 10);
            lblNetAmount.Name = "lblNetAmount";
            lblNetAmount.Size = new Size(127, 25);
            lblNetAmount.TabIndex = 6;
            lblNetAmount.Text = "Net Amount:";
            // 
            // lblTotalBalanceValue
            // 
            lblTotalBalanceValue.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTotalBalanceValue.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTotalBalanceValue.ForeColor = Color.Red;
            lblTotalBalanceValue.Location = new Point(1189, 10);
            lblTotalBalanceValue.Name = "lblTotalBalanceValue";
            lblTotalBalanceValue.Size = new Size(120, 23);
            lblTotalBalanceValue.TabIndex = 3;
            lblTotalBalanceValue.Text = "₹0.00";
            lblTotalBalanceValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblTotalBalance
            // 
            lblTotalBalance.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTotalBalance.AutoSize = true;
            lblTotalBalance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTotalBalance.Location = new Point(1059, 10);
            lblTotalBalance.Name = "lblTotalBalance";
            lblTotalBalance.Size = new Size(133, 25);
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
            Size = new Size(1314, 670);
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
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnDeleteBill;
        private System.Windows.Forms.Button btnViewDetails;
        private System.Windows.Forms.Button btnEditBill;
        private System.Windows.Forms.Button btnNewBill;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearch;
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