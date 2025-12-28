namespace SaleBillSystem.NET.Forms
{
    partial class GodownStockLedgerControl
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
            btnExportPDF = new Button();
            btnExportExcel = new Button();
            btnRefresh = new Button();
            dtpToDate = new DateTimePicker();
            lblToDate = new Label();
            dtpFromDate = new DateTimePicker();
            lblFromDate = new Label();
            cmbFilterItem = new ComboBox();
            label2 = new Label();
            cmbFilterGodown = new ComboBox();
            label1 = new Label();
            groupBox2 = new GroupBox();
            dgvLedger = new DataGridView();
            statusStrip1 = new StatusStrip();
            lblTotalRecords = new ToolStripStatusLabel();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLedger).BeginInit();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            //
            // groupBox1
            //
            groupBox1.Controls.Add(btnExportPDF);
            groupBox1.Controls.Add(btnExportExcel);
            groupBox1.Controls.Add(btnRefresh);
            groupBox1.Controls.Add(dtpToDate);
            groupBox1.Controls.Add(lblToDate);
            groupBox1.Controls.Add(dtpFromDate);
            groupBox1.Controls.Add(lblFromDate);
            groupBox1.Controls.Add(cmbFilterItem);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(cmbFilterGodown);
            groupBox1.Controls.Add(label1);
            groupBox1.Dock = DockStyle.Top;
            groupBox1.Location = new Point(0, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(1200, 80);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Filters";
            //
            // label1
            //
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft Sans Serif", 10F);
            label1.Location = new Point(20, 23);
            label1.Name = "label1";
            label1.Size = new Size(63, 17);
            label1.TabIndex = 0;
            label1.Text = "Godown:";
            //
            // cmbFilterGodown
            //
            cmbFilterGodown.Font = new Font("Microsoft Sans Serif", 10F);
            cmbFilterGodown.FormattingEnabled = true;
            cmbFilterGodown.Location = new Point(90, 20);
            cmbFilterGodown.Name = "cmbFilterGodown";
            cmbFilterGodown.Size = new Size(200, 24);
            cmbFilterGodown.TabIndex = 1;
            cmbFilterGodown.SelectedIndexChanged += cmbFilterGodown_SelectedIndexChanged;
            //
            // label2
            //
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 10F);
            label2.Location = new Point(20, 53);
            label2.Name = "label2";
            label2.Size = new Size(38, 17);
            label2.TabIndex = 2;
            label2.Text = "Item:";
            //
            // cmbFilterItem
            //
            cmbFilterItem.Font = new Font("Microsoft Sans Serif", 10F);
            cmbFilterItem.FormattingEnabled = true;
            cmbFilterItem.Location = new Point(90, 50);
            cmbFilterItem.Name = "cmbFilterItem";
            cmbFilterItem.Size = new Size(200, 24);
            cmbFilterItem.TabIndex = 3;
            cmbFilterItem.SelectedIndexChanged += cmbFilterItem_SelectedIndexChanged;
            //
            // lblFromDate
            //
            lblFromDate.AutoSize = true;
            lblFromDate.Font = new Font("Microsoft Sans Serif", 10F);
            lblFromDate.Location = new Point(310, 23);
            lblFromDate.Name = "lblFromDate";
            lblFromDate.Size = new Size(79, 17);
            lblFromDate.TabIndex = 4;
            lblFromDate.Text = "From Date:";
            //
            // dtpFromDate
            //
            dtpFromDate.CustomFormat = "dd-MM-yyyy";
            dtpFromDate.Font = new Font("Microsoft Sans Serif", 10F);
            dtpFromDate.Format = DateTimePickerFormat.Custom;
            dtpFromDate.Location = new Point(395, 20);
            dtpFromDate.Name = "dtpFromDate";
            dtpFromDate.Size = new Size(120, 23);
            dtpFromDate.TabIndex = 5;
            dtpFromDate.ValueChanged += dtpFromDate_ValueChanged;
            //
            // lblToDate
            //
            lblToDate.AutoSize = true;
            lblToDate.Font = new Font("Microsoft Sans Serif", 10F);
            lblToDate.Location = new Point(310, 53);
            lblToDate.Name = "lblToDate";
            lblToDate.Size = new Size(63, 17);
            lblToDate.TabIndex = 6;
            lblToDate.Text = "To Date:";
            //
            // dtpToDate
            //
            dtpToDate.CustomFormat = "dd-MM-yyyy";
            dtpToDate.Font = new Font("Microsoft Sans Serif", 10F);
            dtpToDate.Format = DateTimePickerFormat.Custom;
            dtpToDate.Location = new Point(395, 50);
            dtpToDate.Name = "dtpToDate";
            dtpToDate.Size = new Size(120, 23);
            dtpToDate.TabIndex = 7;
            dtpToDate.ValueChanged += dtpToDate_ValueChanged;
            //
            // btnRefresh
            //
            btnRefresh.BackColor = Color.LightBlue;
            btnRefresh.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            btnRefresh.Location = new Point(540, 35);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(100, 30);
            btnRefresh.TabIndex = 8;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += btnRefresh_Click;
            //
            // btnExportExcel
            //
            btnExportExcel.BackColor = Color.FromArgb(34, 139, 34);
            btnExportExcel.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            btnExportExcel.ForeColor = Color.White;
            btnExportExcel.Location = new Point(650, 35);
            btnExportExcel.Name = "btnExportExcel";
            btnExportExcel.Size = new Size(110, 30);
            btnExportExcel.TabIndex = 9;
            btnExportExcel.Text = "Export Excel";
            btnExportExcel.UseVisualStyleBackColor = false;
            btnExportExcel.Click += btnExportExcel_Click;
            //
            // btnExportPDF
            //
            btnExportPDF.BackColor = Color.FromArgb(220, 53, 69);
            btnExportPDF.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            btnExportPDF.ForeColor = Color.White;
            btnExportPDF.Location = new Point(770, 35);
            btnExportPDF.Name = "btnExportPDF";
            btnExportPDF.Size = new Size(110, 30);
            btnExportPDF.TabIndex = 10;
            btnExportPDF.Text = "Export PDF";
            btnExportPDF.UseVisualStyleBackColor = false;
            btnExportPDF.Click += btnExportPDF_Click;
            //
            // groupBox2
            //
            groupBox2.Controls.Add(dgvLedger);
            groupBox2.Dock = DockStyle.Fill;
            groupBox2.Location = new Point(0, 80);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(1200, 520);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Stock Ledger";
            //
            // dgvLedger
            //
            dgvLedger.BackgroundColor = SystemColors.Window;
            dgvLedger.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvLedger.Dock = DockStyle.Fill;
            dgvLedger.Location = new Point(3, 19);
            dgvLedger.Name = "dgvLedger";
            dgvLedger.Size = new Size(1194, 498);
            dgvLedger.TabIndex = 0;
            dgvLedger.CellFormatting += dgvLedger_CellFormatting;
            //
            // statusStrip1
            //
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblTotalRecords });
            statusStrip1.Location = new Point(0, 600);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1200, 22);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            //
            // lblTotalRecords
            //
            lblTotalRecords.Name = "lblTotalRecords";
            lblTotalRecords.Size = new Size(103, 17);
            lblTotalRecords.Text = "Total Records: 0";
            //
            // GodownStockLedgerControl
            //
            Controls.Add(groupBox2);
            Controls.Add(statusStrip1);
            Controls.Add(groupBox1);
            Name = "GodownStockLedgerControl";
            Size = new Size(1200, 622);
            Load += GodownStockLedgerControl_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvLedger).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnExportExcel;
        private System.Windows.Forms.Button btnExportPDF;
        private System.Windows.Forms.DateTimePicker dtpFromDate;
        private System.Windows.Forms.Label lblFromDate;
        private System.Windows.Forms.DateTimePicker dtpToDate;
        private System.Windows.Forms.Label lblToDate;
        private System.Windows.Forms.ComboBox cmbFilterItem;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbFilterGodown;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView dgvLedger;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblTotalRecords;
    }
}
