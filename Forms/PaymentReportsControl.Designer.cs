namespace SaleBillSystem.NET.Forms
{
    partial class PaymentReportsControl
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
            lblTitle = new Label();
            filterPanel = new Panel();
            btnClear = new Button();
            btnSearch = new Button();
            cmbBroker = new ComboBox();
            lblBroker = new Label();
            cmbParty = new ComboBox();
            lblParty = new Label();
            dtpToDate = new DateTimePicker();
            lblToDate = new Label();
            dtpFromDate = new DateTimePicker();
            lblFromDate = new Label();
            dgvReports = new DataGridView();
            btnViewReport = new Button();
            btnClose = new Button();
            filterPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvReports).BeginInit();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.DarkBlue;
            lblTitle.Location = new Point(9, 8);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(191, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Payment Reports";
            // 
            // filterPanel
            // 
            filterPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            filterPanel.BackColor = SystemColors.Control;
            filterPanel.BorderStyle = BorderStyle.FixedSingle;
            filterPanel.Controls.Add(btnClear);
            filterPanel.Controls.Add(btnSearch);
            filterPanel.Controls.Add(cmbBroker);
            filterPanel.Controls.Add(lblBroker);
            filterPanel.Controls.Add(cmbParty);
            filterPanel.Controls.Add(lblParty);
            filterPanel.Controls.Add(dtpToDate);
            filterPanel.Controls.Add(lblToDate);
            filterPanel.Controls.Add(dtpFromDate);
            filterPanel.Controls.Add(lblFromDate);
            filterPanel.Location = new Point(9, 38);
            filterPanel.Margin = new Padding(3, 2, 3, 2);
            filterPanel.Name = "filterPanel";
            filterPanel.Size = new Size(1228, 60);
            filterPanel.TabIndex = 1;
            // 
            // btnClear
            // 
            btnClear.Location = new Point(88, 34);
            btnClear.Margin = new Padding(3, 2, 3, 2);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(88, 19);
            btnClear.TabIndex = 9;
            btnClear.Text = "Clear Filters";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += BtnClear_Click;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(9, 34);
            btnSearch.Margin = new Padding(3, 2, 3, 2);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(70, 19);
            btnSearch.TabIndex = 8;
            btnSearch.Text = "Search";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += BtnSearch_Click;
            // 
            // cmbBroker
            // 
            cmbBroker.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbBroker.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbBroker.FormattingEnabled = true;
            cmbBroker.Location = new Point(774, 8);
            cmbBroker.Margin = new Padding(3, 2, 3, 2);
            cmbBroker.Name = "cmbBroker";
            cmbBroker.Size = new Size(266, 23);
            cmbBroker.TabIndex = 7;
            cmbBroker.SelectedIndexChanged += cmbBroker_SelectedIndexChanged;
            // 
            // lblBroker
            // 
            lblBroker.AutoSize = true;
            lblBroker.Location = new Point(715, 12);
            lblBroker.Name = "lblBroker";
            lblBroker.Size = new Size(44, 15);
            lblBroker.TabIndex = 6;
            lblBroker.Text = "Broker:";
            // 
            // cmbParty
            // 
            cmbParty.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbParty.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbParty.FormattingEnabled = true;
            cmbParty.Location = new Point(429, 9);
            cmbParty.Margin = new Padding(3, 2, 3, 2);
            cmbParty.Name = "cmbParty";
            cmbParty.Size = new Size(232, 23);
            cmbParty.TabIndex = 5;
            cmbParty.SelectedIndexChanged += cmbParty_SelectedIndexChanged;
            // 
            // lblParty
            // 
            lblParty.AutoSize = true;
            lblParty.Location = new Point(368, 11);
            lblParty.Name = "lblParty";
            lblParty.Size = new Size(37, 15);
            lblParty.TabIndex = 4;
            lblParty.Text = "Party:";
            // 
            // dtpToDate
            // 
            dtpToDate.Format = DateTimePickerFormat.Short;
            dtpToDate.Location = new Point(245, 9);
            dtpToDate.Margin = new Padding(3, 2, 3, 2);
            dtpToDate.Name = "dtpToDate";
            dtpToDate.Size = new Size(106, 23);
            dtpToDate.TabIndex = 3;
            dtpToDate.ValueChanged += dtpToDate_ValueChanged;
            // 
            // lblToDate
            // 
            lblToDate.AutoSize = true;
            lblToDate.Location = new Point(192, 11);
            lblToDate.Name = "lblToDate";
            lblToDate.Size = new Size(49, 15);
            lblToDate.TabIndex = 2;
            lblToDate.Text = "To Date:";
            // 
            // dtpFromDate
            // 
            dtpFromDate.Format = DateTimePickerFormat.Short;
            dtpFromDate.Location = new Point(70, 9);
            dtpFromDate.Margin = new Padding(3, 2, 3, 2);
            dtpFromDate.Name = "dtpFromDate";
            dtpFromDate.Size = new Size(106, 23);
            dtpFromDate.TabIndex = 1;
            dtpFromDate.ValueChanged += dtpFromDate_ValueChanged;
            // 
            // lblFromDate
            // 
            lblFromDate.AutoSize = true;
            lblFromDate.Location = new Point(9, 11);
            lblFromDate.Name = "lblFromDate";
            lblFromDate.Size = new Size(65, 15);
            lblFromDate.TabIndex = 0;
            lblFromDate.Text = "From Date:";
            // 
            // dgvReports
            // 
            dgvReports.AllowUserToAddRows = false;
            dgvReports.AllowUserToDeleteRows = false;
            dgvReports.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvReports.BackgroundColor = SystemColors.Window;
            dgvReports.BorderStyle = BorderStyle.Fixed3D;
            dgvReports.Location = new Point(9, 105);
            dgvReports.Margin = new Padding(3, 2, 3, 2);
            dgvReports.MultiSelect = false;
            dgvReports.Name = "dgvReports";
            dgvReports.ReadOnly = true;
            dgvReports.RowHeadersVisible = false;
            dgvReports.RowHeadersWidth = 51;
            dgvReports.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvReports.Size = new Size(1227, 270);
            dgvReports.TabIndex = 2;
            dgvReports.DoubleClick += DgvReports_DoubleClick;
            // 
            // btnViewReport
            // 
            btnViewReport.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnViewReport.Location = new Point(9, 382);
            btnViewReport.Margin = new Padding(3, 2, 3, 2);
            btnViewReport.Name = "btnViewReport";
            btnViewReport.Size = new Size(88, 22);
            btnViewReport.TabIndex = 3;
            btnViewReport.Text = "View Report";
            btnViewReport.UseVisualStyleBackColor = true;
            btnViewReport.Click += BtnViewReport_Click;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnClose.Location = new Point(105, 382);
            btnClose.Margin = new Padding(3, 2, 3, 2);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(70, 22);
            btnClose.TabIndex = 4;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += BtnClose_Click;
            // 
            // PaymentReportsControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            Controls.Add(btnClose);
            Controls.Add(btnViewReport);
            Controls.Add(dgvReports);
            Controls.Add(filterPanel);
            Controls.Add(lblTitle);
            Margin = new Padding(3, 2, 3, 2);
            Name = "PaymentReportsControl";
            Size = new Size(1245, 412);
            Load += PaymentReportsControl_Load;
            filterPanel.ResumeLayout(false);
            filterPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvReports).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel filterPanel;
        private System.Windows.Forms.Label lblFromDate;
        private System.Windows.Forms.DateTimePicker dtpFromDate;
        private System.Windows.Forms.Label lblToDate;
        private System.Windows.Forms.DateTimePicker dtpToDate;
        private System.Windows.Forms.Label lblParty;
        private System.Windows.Forms.ComboBox cmbParty;
        private System.Windows.Forms.Label lblBroker;
        private System.Windows.Forms.ComboBox cmbBroker;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.DataGridView dgvReports;
        private System.Windows.Forms.Button btnViewReport;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.DataGridViewTextBoxColumn PaymentID;
        private System.Windows.Forms.DataGridViewTextBoxColumn PaymentDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn PartyName;
        private System.Windows.Forms.DataGridViewTextBoxColumn BrokerName;
        private System.Windows.Forms.DataGridViewTextBoxColumn TotalAmount;
        private System.Windows.Forms.DataGridViewTextBoxColumn PaymentMethod;
    }
}