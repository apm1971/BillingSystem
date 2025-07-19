namespace SaleBillSystem.NET.Forms
{
    partial class OutstandingReportForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBoxFilters = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbParty = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            
            this.panel2 = new System.Windows.Forms.Panel();
            this.dgvOutstanding = new System.Windows.Forms.DataGridView();
            
            this.panel3 = new System.Windows.Forms.Panel();
            this.groupBoxSummary = new System.Windows.Forms.GroupBox();
            this.lblTotalBills = new System.Windows.Forms.Label();
            this.lblTotalAmount = new System.Windows.Forms.Label();
            
            this.groupBoxActions = new System.Windows.Forms.GroupBox();
            this.btnViewBill = new System.Windows.Forms.Button();
            this.btnExportExcel = new System.Windows.Forms.Button();
            this.btnExportPDF = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            
            this.panel1.SuspendLayout();
            this.groupBoxFilters.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOutstanding)).BeginInit();
            this.panel3.SuspendLayout();
            this.groupBoxSummary.SuspendLayout();
            this.groupBoxActions.SuspendLayout();
            this.SuspendLayout();
            
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBoxFilters);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1200, 80);
            this.panel1.TabIndex = 0;
            
            // 
            // groupBoxFilters
            // 
            this.groupBoxFilters.Controls.Add(this.label1);
            this.groupBoxFilters.Controls.Add(this.cmbParty);
            this.groupBoxFilters.Controls.Add(this.label2);
            this.groupBoxFilters.Controls.Add(this.txtSearch);
            this.groupBoxFilters.Controls.Add(this.btnRefresh);
            this.groupBoxFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxFilters.Location = new System.Drawing.Point(0, 0);
            this.groupBoxFilters.Name = "groupBoxFilters";
            this.groupBoxFilters.Size = new System.Drawing.Size(1200, 80);
            this.groupBoxFilters.TabIndex = 0;
            this.groupBoxFilters.TabStop = false;
            this.groupBoxFilters.Text = "Filters";
            
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Party:";
            
            // 
            // cmbParty
            // 
            this.cmbParty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbParty.FormattingEnabled = true;
            this.cmbParty.Location = new System.Drawing.Point(70, 27);
            this.cmbParty.Name = "cmbParty";
            this.cmbParty.Size = new System.Drawing.Size(250, 23);
            this.cmbParty.TabIndex = 1;
            
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(350, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Search:";
            
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(400, 27);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.PlaceholderText = "Search by bill no, party name, or broker...";
            this.txtSearch.Size = new System.Drawing.Size(300, 23);
            this.txtSearch.TabIndex = 3;
            
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackColor = System.Drawing.Color.LightBlue;
            this.btnRefresh.Location = new System.Drawing.Point(720, 25);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(80, 27);
            this.btnRefresh.TabIndex = 4;
            this.btnRefresh.Text = "Refresh (F5)";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.dgvOutstanding);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 80);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1200, 400);
            this.panel2.TabIndex = 1;
            
            // 
            // dgvOutstanding
            // 
            this.dgvOutstanding.AllowUserToAddRows = false;
            this.dgvOutstanding.AllowUserToDeleteRows = false;
            this.dgvOutstanding.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.dgvOutstanding.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvOutstanding.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvOutstanding.Location = new System.Drawing.Point(0, 0);
            this.dgvOutstanding.Name = "dgvOutstanding";
            this.dgvOutstanding.ReadOnly = true;
            this.dgvOutstanding.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvOutstanding.Size = new System.Drawing.Size(1200, 400);
            this.dgvOutstanding.TabIndex = 0;
            
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.groupBoxSummary);
            this.panel3.Controls.Add(this.groupBoxActions);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(0, 480);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1200, 100);
            this.panel3.TabIndex = 2;
            
            // 
            // groupBoxSummary
            // 
            this.groupBoxSummary.Controls.Add(this.lblTotalBills);
            this.groupBoxSummary.Controls.Add(this.lblTotalAmount);
            this.groupBoxSummary.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBoxSummary.Location = new System.Drawing.Point(0, 0);
            this.groupBoxSummary.Name = "groupBoxSummary";
            this.groupBoxSummary.Size = new System.Drawing.Size(400, 100);
            this.groupBoxSummary.TabIndex = 0;
            this.groupBoxSummary.TabStop = false;
            this.groupBoxSummary.Text = "Summary";
            
            // 
            // lblTotalBills
            // 
            this.lblTotalBills.AutoSize = true;
            this.lblTotalBills.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotalBills.Location = new System.Drawing.Point(15, 25);
            this.lblTotalBills.Name = "lblTotalBills";
            this.lblTotalBills.Size = new System.Drawing.Size(77, 15);
            this.lblTotalBills.TabIndex = 0;
            this.lblTotalBills.Text = "Total Bills: 0";
            
            // 
            // lblTotalAmount
            // 
            this.lblTotalAmount.AutoSize = true;
            this.lblTotalAmount.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotalAmount.ForeColor = System.Drawing.Color.Red;
            this.lblTotalAmount.Location = new System.Drawing.Point(15, 50);
            this.lblTotalAmount.Name = "lblTotalAmount";
            this.lblTotalAmount.Size = new System.Drawing.Size(139, 15);
            this.lblTotalAmount.TabIndex = 1;
            this.lblTotalAmount.Text = "Total Outstanding: ₹0";
            
            // 
            // groupBoxActions
            // 
            this.groupBoxActions.Controls.Add(this.btnViewBill);
            this.groupBoxActions.Controls.Add(this.btnExportExcel);
            this.groupBoxActions.Controls.Add(this.btnExportPDF);
            this.groupBoxActions.Controls.Add(this.btnClose);
            this.groupBoxActions.Dock = System.Windows.Forms.DockStyle.Right;
            this.groupBoxActions.Location = new System.Drawing.Point(650, 0);
            this.groupBoxActions.Name = "groupBoxActions";
            this.groupBoxActions.Size = new System.Drawing.Size(550, 100);
            this.groupBoxActions.TabIndex = 1;
            this.groupBoxActions.TabStop = false;
            this.groupBoxActions.Text = "Actions";
            
            // 
            // btnViewBill
            // 
            this.btnViewBill.BackColor = System.Drawing.Color.LightGreen;
            this.btnViewBill.Location = new System.Drawing.Point(20, 25);
            this.btnViewBill.Name = "btnViewBill";
            this.btnViewBill.Size = new System.Drawing.Size(100, 30);
            this.btnViewBill.TabIndex = 0;
            this.btnViewBill.Text = "View Bill";
            this.btnViewBill.UseVisualStyleBackColor = false;
            this.btnViewBill.Click += new System.EventHandler(this.btnViewBill_Click);
            
            // 
            // btnExportExcel
            // 
            this.btnExportExcel.BackColor = System.Drawing.Color.LightYellow;
            this.btnExportExcel.Location = new System.Drawing.Point(140, 25);
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Size = new System.Drawing.Size(120, 30);
            this.btnExportExcel.TabIndex = 1;
            this.btnExportExcel.Text = "Export Excel (Ctrl+E)";
            this.btnExportExcel.UseVisualStyleBackColor = false;
            this.btnExportExcel.Click += new System.EventHandler(this.btnExportExcel_Click);
            
            // 
            // btnExportPDF
            // 
            this.btnExportPDF.BackColor = System.Drawing.Color.LightCoral;
            this.btnExportPDF.Location = new System.Drawing.Point(280, 25);
            this.btnExportPDF.Name = "btnExportPDF";
            this.btnExportPDF.Size = new System.Drawing.Size(120, 30);
            this.btnExportPDF.TabIndex = 2;
            this.btnExportPDF.Text = "Export HTML (Ctrl+P)";
            this.btnExportPDF.UseVisualStyleBackColor = false;
            this.btnExportPDF.Click += new System.EventHandler(this.btnExportPDF_Click);
            
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.LightGray;
            this.btnClose.Location = new System.Drawing.Point(420, 25);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 30);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close (Esc)";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            
            // 
            // OutstandingReportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 580);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel3);
            this.Name = "OutstandingReportForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Outstanding Report";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            
            this.panel1.ResumeLayout(false);
            this.groupBoxFilters.ResumeLayout(false);
            this.groupBoxFilters.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvOutstanding)).EndInit();
            this.panel3.ResumeLayout(false);
            this.groupBoxSummary.ResumeLayout(false);
            this.groupBoxSummary.PerformLayout();
            this.groupBoxActions.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBoxFilters;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbParty;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnRefresh;
        
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.DataGridView dgvOutstanding;
        
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.GroupBox groupBoxSummary;
        private System.Windows.Forms.Label lblTotalBills;
        private System.Windows.Forms.Label lblTotalAmount;
        
        private System.Windows.Forms.GroupBox groupBoxActions;
        private System.Windows.Forms.Button btnViewBill;
        private System.Windows.Forms.Button btnExportExcel;
        private System.Windows.Forms.Button btnExportPDF;
        private System.Windows.Forms.Button btnClose;
    }
} 