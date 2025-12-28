namespace SaleBillSystem.NET.Forms
{
    partial class GodownStockReportControl
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
            dtpAsOnDate = new DateTimePicker();
            lblAsOnDate = new Label();
            cmbFilterItem = new ComboBox();
            label2 = new Label();
            cmbFilterGodown = new ComboBox();
            label1 = new Label();
            groupBox2 = new GroupBox();
            dgvStockReport = new DataGridView();
            statusStrip1 = new StatusStrip();
            lblTotalRecords = new ToolStripStatusLabel();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvStockReport).BeginInit();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            //
            // groupBox1
            //
            groupBox1.Controls.Add(btnExportPDF);
            groupBox1.Controls.Add(btnExportExcel);
            groupBox1.Controls.Add(btnRefresh);
            groupBox1.Controls.Add(dtpAsOnDate);
            groupBox1.Controls.Add(lblAsOnDate);
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
            // btnRefresh
            //
            btnRefresh.BackColor = Color.LightBlue;
            btnRefresh.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            btnRefresh.Location = new Point(830, 30);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(100, 30);
            btnRefresh.TabIndex = 6;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += btnRefresh_Click;
            //
            // btnExportExcel
            //
            btnExportExcel.BackColor = Color.FromArgb(34, 139, 34);
            btnExportExcel.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            btnExportExcel.ForeColor = Color.White;
            btnExportExcel.Location = new Point(940, 30);
            btnExportExcel.Name = "btnExportExcel";
            btnExportExcel.Size = new Size(110, 30);
            btnExportExcel.TabIndex = 7;
            btnExportExcel.Text = "Export Excel";
            btnExportExcel.UseVisualStyleBackColor = false;
            btnExportExcel.Click += btnExportExcel_Click;
            //
            // btnExportPDF
            //
            btnExportPDF.BackColor = Color.FromArgb(220, 53, 69);
            btnExportPDF.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            btnExportPDF.ForeColor = Color.White;
            btnExportPDF.Location = new Point(1060, 30);
            btnExportPDF.Name = "btnExportPDF";
            btnExportPDF.Size = new Size(110, 30);
            btnExportPDF.TabIndex = 8;
            btnExportPDF.Text = "Export PDF";
            btnExportPDF.UseVisualStyleBackColor = false;
            btnExportPDF.Click += btnExportPDF_Click;
            // 
            // dtpAsOnDate
            // 
            dtpAsOnDate.CustomFormat = "dd-MM-yyyy";
            dtpAsOnDate.Font = new Font("Microsoft Sans Serif", 10F);
            dtpAsOnDate.Format = DateTimePickerFormat.Custom;
            dtpAsOnDate.Location = new Point(600, 30);
            dtpAsOnDate.Name = "dtpAsOnDate";
            dtpAsOnDate.Size = new Size(150, 23);
            dtpAsOnDate.TabIndex = 5;
            dtpAsOnDate.ValueChanged += dtpAsOnDate_ValueChanged;
            // 
            // lblAsOnDate
            // 
            lblAsOnDate.AutoSize = true;
            lblAsOnDate.Font = new Font("Microsoft Sans Serif", 10F);
            lblAsOnDate.Location = new Point(510, 33);
            lblAsOnDate.Name = "lblAsOnDate";
            lblAsOnDate.Size = new Size(84, 17);
            lblAsOnDate.TabIndex = 4;
            lblAsOnDate.Text = "As On Date:";
            // 
            // cmbFilterItem
            // 
            cmbFilterItem.Font = new Font("Microsoft Sans Serif", 10F);
            cmbFilterItem.FormattingEnabled = true;
            cmbFilterItem.Location = new Point(150, 50);
            cmbFilterItem.Name = "cmbFilterItem";
            cmbFilterItem.Size = new Size(300, 24);
            cmbFilterItem.TabIndex = 3;
            cmbFilterItem.SelectedIndexChanged += cmbFilterItem_SelectedIndexChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 10F);
            label2.Location = new Point(20, 53);
            label2.Name = "label2";
            label2.Size = new Size(79, 17);
            label2.TabIndex = 2;
            label2.Text = "Filter Item:";
            // 
            // cmbFilterGodown
            // 
            cmbFilterGodown.Font = new Font("Microsoft Sans Serif", 10F);
            cmbFilterGodown.FormattingEnabled = true;
            cmbFilterGodown.Location = new Point(150, 20);
            cmbFilterGodown.Name = "cmbFilterGodown";
            cmbFilterGodown.Size = new Size(300, 24);
            cmbFilterGodown.TabIndex = 1;
            cmbFilterGodown.SelectedIndexChanged += cmbFilterGodown_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft Sans Serif", 10F);
            label1.Location = new Point(20, 23);
            label1.Name = "label1";
            label1.Size = new Size(108, 17);
            label1.TabIndex = 0;
            label1.Text = "Filter Godown:";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(dgvStockReport);
            groupBox2.Dock = DockStyle.Fill;
            groupBox2.Location = new Point(0, 80);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(1200, 520);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Stock Report";
            // 
            // dgvStockReport
            // 
            dgvStockReport.BackgroundColor = SystemColors.Window;
            dgvStockReport.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvStockReport.Dock = DockStyle.Fill;
            dgvStockReport.Location = new Point(3, 19);
            dgvStockReport.Name = "dgvStockReport";
            dgvStockReport.Size = new Size(1194, 498);
            dgvStockReport.TabIndex = 0;
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
            // GodownStockReportControl
            // 
            Controls.Add(groupBox2);
            Controls.Add(statusStrip1);
            Controls.Add(groupBox1);
            Name = "GodownStockReportControl";
            Size = new Size(1200, 622);
            Load += GodownStockReportControl_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvStockReport).EndInit();
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
        private System.Windows.Forms.DateTimePicker dtpAsOnDate;
        private System.Windows.Forms.Label lblAsOnDate;
        private System.Windows.Forms.ComboBox cmbFilterItem;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbFilterGodown;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView dgvStockReport;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblTotalRecords;
    }
}

