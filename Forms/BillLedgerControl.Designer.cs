namespace SaleBillSystem.NET.Forms
{
    partial class BillLedgerControl
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
            splitContainer = new SplitContainer();
            gbBills = new GroupBox();
            dgvBills = new DataGridView();
            pnlTop = new Panel();
            dtpToDate = new DateTimePicker();
            lblToDate = new Label();
            dtpFromDate = new DateTimePicker();
            lblFromDate = new Label();
            cmbPaymentStatus = new ComboBox();
            lblPaymentStatus = new Label();
            cmbBroker = new ComboBox();
            lblBroker = new Label();
            cmbParty = new ComboBox();
            lblParty = new Label();
            gbLedger = new GroupBox();
            dgvLedger = new DataGridView();
            pnlButtons = new Panel();
            btnRefresh = new Button();
            btnPrint = new Button();
            btnClearFilters = new Button();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            gbBills.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvBills).BeginInit();
            pnlTop.SuspendLayout();
            gbLedger.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLedger).BeginInit();
            pnlButtons.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer
            // 
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Location = new Point(12, 12);
            splitContainer.Margin = new Padding(4, 3, 4, 3);
            splitContainer.Name = "splitContainer";
            splitContainer.Orientation = Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(gbBills);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(gbLedger);
            splitContainer.Size = new Size(1370, 565);
            splitContainer.SplitterDistance = 264;
            splitContainer.SplitterWidth = 5;
            splitContainer.TabIndex = 0;
            // 
            // gbBills
            // 
            gbBills.Controls.Add(dgvBills);
            gbBills.Controls.Add(pnlTop);
            gbBills.Dock = DockStyle.Fill;
            gbBills.Location = new Point(0, 0);
            gbBills.Margin = new Padding(4, 3, 4, 3);
            gbBills.Name = "gbBills";
            gbBills.Padding = new Padding(12);
            gbBills.Size = new Size(1370, 264);
            gbBills.TabIndex = 0;
            gbBills.TabStop = false;
            gbBills.Text = "Bill Selection";
            // 
            // dgvBills
            // 
            dgvBills.AllowUserToAddRows = false;
            dgvBills.AllowUserToDeleteRows = false;
            dgvBills.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvBills.Dock = DockStyle.Fill;
            dgvBills.Location = new Point(12, 74);
            dgvBills.Margin = new Padding(4, 3, 4, 3);
            dgvBills.Name = "dgvBills";
            dgvBills.ReadOnly = true;
            dgvBills.Size = new Size(1346, 178);
            dgvBills.TabIndex = 1;
            // 
            // pnlTop
            // 
            pnlTop.Controls.Add(dtpToDate);
            pnlTop.Controls.Add(lblToDate);
            pnlTop.Controls.Add(dtpFromDate);
            pnlTop.Controls.Add(lblFromDate);
            pnlTop.Controls.Add(cmbPaymentStatus);
            pnlTop.Controls.Add(lblPaymentStatus);
            pnlTop.Controls.Add(cmbBroker);
            pnlTop.Controls.Add(lblBroker);
            pnlTop.Controls.Add(cmbParty);
            pnlTop.Controls.Add(lblParty);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(12, 28);
            pnlTop.Margin = new Padding(4, 3, 4, 3);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(1346, 46);
            pnlTop.TabIndex = 0;
            // 
            // dtpToDate
            // 
            dtpToDate.Format = DateTimePickerFormat.Short;
            dtpToDate.Location = new Point(1211, 10);
            dtpToDate.Margin = new Padding(4, 3, 4, 3);
            dtpToDate.Name = "dtpToDate";
            dtpToDate.Size = new Size(116, 23);
            dtpToDate.TabIndex = 7;
            dtpToDate.ValueChanged += dtpToDate_ValueChanged;
            // 
            // lblToDate
            // 
            lblToDate.AutoSize = true;
            lblToDate.Location = new Point(1181, 14);
            lblToDate.Margin = new Padding(4, 0, 4, 0);
            lblToDate.Name = "lblToDate";
            lblToDate.Size = new Size(22, 15);
            lblToDate.TabIndex = 7;
            lblToDate.Text = "To:";
            // 
            // dtpFromDate
            // 
            dtpFromDate.Format = DateTimePickerFormat.Short;
            dtpFromDate.Location = new Point(1034, 8);
            dtpFromDate.Margin = new Padding(4, 3, 4, 3);
            dtpFromDate.Name = "dtpFromDate";
            dtpFromDate.Size = new Size(116, 23);
            dtpFromDate.TabIndex = 6;
            dtpFromDate.ValueChanged += dtpFromDate_ValueChanged;
            // 
            // lblFromDate
            // 
            lblFromDate.AutoSize = true;
            lblFromDate.Location = new Point(988, 13);
            lblFromDate.Margin = new Padding(4, 0, 4, 0);
            lblFromDate.Name = "lblFromDate";
            lblFromDate.Size = new Size(38, 15);
            lblFromDate.TabIndex = 6;
            lblFromDate.Text = "From:";
            // 
            // cmbPaymentStatus
            // 
            cmbPaymentStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPaymentStatus.FormattingEnabled = true;
            cmbPaymentStatus.Location = new Point(830, 10);
            cmbPaymentStatus.Margin = new Padding(4, 3, 4, 3);
            cmbPaymentStatus.Name = "cmbPaymentStatus";
            cmbPaymentStatus.Size = new Size(115, 23);
            cmbPaymentStatus.TabIndex = 5;
            cmbPaymentStatus.SelectedIndexChanged += cmbPaymentStatus_SelectedIndexChanged;
            // 
            // lblPaymentStatus
            // 
            lblPaymentStatus.AutoSize = true;
            lblPaymentStatus.Location = new Point(771, 13);
            lblPaymentStatus.Margin = new Padding(4, 0, 4, 0);
            lblPaymentStatus.Name = "lblPaymentStatus";
            lblPaymentStatus.Size = new Size(42, 15);
            lblPaymentStatus.TabIndex = 4;
            lblPaymentStatus.Text = "Status:";
            // 
            // cmbBroker
            // 
            cmbBroker.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbBroker.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbBroker.FormattingEnabled = true;
            cmbBroker.Location = new Point(525, 10);
            cmbBroker.Margin = new Padding(4, 3, 4, 3);
            cmbBroker.Name = "cmbBroker";
            cmbBroker.Size = new Size(192, 23);
            cmbBroker.TabIndex = 1;
            // 
            // lblBroker
            // 
            lblBroker.AutoSize = true;
            lblBroker.Location = new Point(426, 14);
            lblBroker.Margin = new Padding(4, 0, 4, 0);
            lblBroker.Name = "lblBroker";
            lblBroker.Size = new Size(87, 15);
            lblBroker.TabIndex = 0;
            lblBroker.Text = "Select a Broker:";
            // 
            // cmbParty
            // 
            cmbParty.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbParty.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbParty.FormattingEnabled = true;
            cmbParty.Location = new Point(103, 10);
            cmbParty.Margin = new Padding(4, 3, 4, 3);
            cmbParty.Name = "cmbParty";
            cmbParty.Size = new Size(238, 23);
            cmbParty.TabIndex = 0;
            // 
            // lblParty
            // 
            lblParty.AutoSize = true;
            lblParty.Location = new Point(4, 14);
            lblParty.Margin = new Padding(4, 0, 4, 0);
            lblParty.Name = "lblParty";
            lblParty.Size = new Size(80, 15);
            lblParty.TabIndex = 0;
            lblParty.Text = "Select a Party:";
            // 
            // gbLedger
            // 
            gbLedger.Controls.Add(dgvLedger);
            gbLedger.Dock = DockStyle.Fill;
            gbLedger.Location = new Point(0, 0);
            gbLedger.Margin = new Padding(4, 3, 4, 3);
            gbLedger.Name = "gbLedger";
            gbLedger.Padding = new Padding(12);
            gbLedger.Size = new Size(1370, 296);
            gbLedger.TabIndex = 0;
            gbLedger.TabStop = false;
            gbLedger.Text = "Transaction History";
            // 
            // dgvLedger
            // 
            dgvLedger.AllowUserToAddRows = false;
            dgvLedger.AllowUserToDeleteRows = false;
            dgvLedger.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvLedger.Dock = DockStyle.Fill;
            dgvLedger.Location = new Point(12, 28);
            dgvLedger.Margin = new Padding(4, 3, 4, 3);
            dgvLedger.Name = "dgvLedger";
            dgvLedger.ReadOnly = true;
            dgvLedger.Size = new Size(1346, 256);
            dgvLedger.TabIndex = 0;
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnRefresh);
            pnlButtons.Controls.Add(btnPrint);
            pnlButtons.Controls.Add(btnClearFilters);
            pnlButtons.Dock = DockStyle.Bottom;
            pnlButtons.Location = new Point(12, 577);
            pnlButtons.Margin = new Padding(4, 3, 4, 3);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(1370, 58);
            pnlButtons.TabIndex = 1;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(23, 12);
            btnRefresh.Margin = new Padding(4, 3, 4, 3);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(117, 35);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "Refresh (F5)";
            btnRefresh.UseVisualStyleBackColor = true;
            // 
            // btnPrint
            // 
            btnPrint.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPrint.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnPrint.Location = new Point(246, 12);
            btnPrint.Margin = new Padding(4, 3, 4, 3);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(117, 35);
            btnPrint.TabIndex = 0;
            btnPrint.Text = "Print";
            btnPrint.UseVisualStyleBackColor = true;
            // 
            // btnClearFilters
            // 
            btnClearFilters.Location = new Point(1136, 12);
            btnClearFilters.Margin = new Padding(4, 3, 4, 3);
            btnClearFilters.Name = "btnClearFilters";
            btnClearFilters.Size = new Size(117, 35);
            btnClearFilters.TabIndex = 2;
            btnClearFilters.Text = "Clear Filters";
            btnClearFilters.UseVisualStyleBackColor = true;
            btnClearFilters.Click += btnClearFilters_Click;
            // 
            // BillLedgerControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(splitContainer);
            Controls.Add(pnlButtons);
            Margin = new Padding(4, 3, 4, 3);
            Name = "BillLedgerControl";
            Padding = new Padding(12);
            Size = new Size(1394, 647);
            Load += BillLedgerControl_Load;
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            gbBills.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvBills).EndInit();
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            gbLedger.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvLedger).EndInit();
            pnlButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.GroupBox gbBills;
        private System.Windows.Forms.DataGridView dgvBills;
        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.ComboBox cmbParty;
        private System.Windows.Forms.Label lblParty;
        private System.Windows.Forms.ComboBox cmbBroker;
        private System.Windows.Forms.Label lblBroker;
        private System.Windows.Forms.GroupBox gbLedger;
        private System.Windows.Forms.DataGridView dgvLedger;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnClearFilters;
        private System.Windows.Forms.Label lblLedgerTitle;
    }
}
