namespace SaleBillSystem.NET.Forms
{
    partial class GodownOpeningStockControl
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
            dtpAsOnDate = new DateTimePicker();
            label2 = new Label();
            cmbGodown = new ComboBox();
            label1 = new Label();
            groupBox2 = new GroupBox();
            dgvOpeningStock = new DataGridView();
            panel1 = new Panel();
            btnAddRow = new Button();
            btnClear = new Button();
            btnSave = new Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvOpeningStock).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(dtpAsOnDate);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(cmbGodown);
            groupBox1.Controls.Add(label1);
            groupBox1.Dock = DockStyle.Top;
            groupBox1.Location = new Point(0, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(1200, 80);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Godown Selection";
            // 
            // dtpAsOnDate
            // 
            dtpAsOnDate.Font = new Font("Microsoft Sans Serif", 10F);
            dtpAsOnDate.Location = new Point(500, 30);
            dtpAsOnDate.Name = "dtpAsOnDate";
            dtpAsOnDate.Size = new Size(200, 23);
            dtpAsOnDate.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 10F);
            label2.Location = new Point(400, 33);
            label2.Name = "label2";
            label2.Size = new Size(75, 17);
            label2.TabIndex = 2;
            label2.Text = "As On Date:";
            // 
            // cmbGodown
            // 
            cmbGodown.Font = new Font("Microsoft Sans Serif", 10F);
            cmbGodown.FormattingEnabled = true;
            cmbGodown.Location = new Point(150, 30);
            cmbGodown.Name = "cmbGodown";
            cmbGodown.Size = new Size(200, 24);
            cmbGodown.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft Sans Serif", 10F);
            label1.Location = new Point(20, 33);
            label1.Name = "label1";
            label1.Size = new Size(108, 17);
            label1.TabIndex = 0;
            label1.Text = "Select Godown:";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(dgvOpeningStock);
            groupBox2.Dock = DockStyle.Fill;
            groupBox2.Location = new Point(0, 80);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(1200, 490);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Opening Stock";
            // 
            // dgvOpeningStock
            // 
            dgvOpeningStock.BackgroundColor = SystemColors.Window;
            dgvOpeningStock.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvOpeningStock.Dock = DockStyle.Fill;
            dgvOpeningStock.Location = new Point(3, 19);
            dgvOpeningStock.Name = "dgvOpeningStock";
            dgvOpeningStock.Size = new Size(1194, 468);
            dgvOpeningStock.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnAddRow);
            panel1.Controls.Add(btnClear);
            panel1.Controls.Add(btnSave);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 570);
            panel1.Name = "panel1";
            panel1.Size = new Size(1200, 52);
            panel1.TabIndex = 2;
            // 
            // btnAddRow
            // 
            btnAddRow.BackColor = Color.LightBlue;
            btnAddRow.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            btnAddRow.Location = new Point(10, 10);
            btnAddRow.Name = "btnAddRow";
            btnAddRow.Size = new Size(90, 35);
            btnAddRow.TabIndex = 2;
            btnAddRow.Text = "Add Row";
            btnAddRow.UseVisualStyleBackColor = false;
            btnAddRow.Click += btnAddRow_Click;
            // 
            // btnClear
            // 
            btnClear.BackColor = Color.LightGray;
            btnClear.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            btnClear.Location = new Point(230, 10);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(90, 35);
            btnClear.TabIndex = 1;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = false;
            btnClear.Click += btnClear_Click;
            // 
            // btnSave
            // 
            btnSave.BackColor = Color.LightGreen;
            btnSave.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            btnSave.Location = new Point(120, 10);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(90, 35);
            btnSave.TabIndex = 0;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += btnSave_Click;
            // 
            // GodownOpeningStockControl
            // 
            Controls.Add(groupBox2);
            Controls.Add(panel1);
            Controls.Add(groupBox1);
            Name = "GodownOpeningStockControl";
            Size = new Size(1200, 622);
            Load += GodownOpeningStockControl_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvOpeningStock).EndInit();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DateTimePicker dtpAsOnDate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbGodown;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView dgvOpeningStock;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnAddRow;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnSave;
    }
}

