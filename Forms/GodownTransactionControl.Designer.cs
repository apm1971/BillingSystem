namespace SaleBillSystem.NET.Forms
{
    partial class GodownTransactionControl
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
            txtReferenceNo = new TextBox();
            label5 = new Label();
            dtpTransactionDate = new DateTimePicker();
            label4 = new Label();
            txtTransactionNo = new TextBox();
            label3 = new Label();
            cmbTransactionType = new ComboBox();
            label2 = new Label();
            cmbToGodown = new ComboBox();
            lblToGodown = new Label();
            cmbFromGodown = new ComboBox();
            lblFromGodown = new Label();
            groupBox2 = new GroupBox();
            dgvTransactionDetails = new DataGridView();
            panel1 = new Panel();
            btnRemoveRow = new Button();
            btnAddRow = new Button();
            panel2 = new Panel();
            btnClear = new Button();
            btnSave = new Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTransactionDetails).BeginInit();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(txtReferenceNo);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(dtpTransactionDate);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(txtTransactionNo);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(cmbTransactionType);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(cmbToGodown);
            groupBox1.Controls.Add(lblToGodown);
            groupBox1.Controls.Add(cmbFromGodown);
            groupBox1.Controls.Add(lblFromGodown);
            groupBox1.Dock = DockStyle.Top;
            groupBox1.Location = new Point(0, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(1200, 120);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Transaction Details";
            // 
            // txtReferenceNo
            // 
            txtReferenceNo.Font = new Font("Microsoft Sans Serif", 10F);
            txtReferenceNo.Location = new Point(500, 80);
            txtReferenceNo.Name = "txtReferenceNo";
            txtReferenceNo.Size = new Size(200, 23);
            txtReferenceNo.TabIndex = 11;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Microsoft Sans Serif", 10F);
            label5.Location = new Point(400, 83);
            label5.Name = "label5";
            label5.Size = new Size(95, 17);
            label5.TabIndex = 10;
            label5.Text = "Reference No:";
            // 
            // dtpTransactionDate
            // 
            dtpTransactionDate.Font = new Font("Microsoft Sans Serif", 10F);
            dtpTransactionDate.Location = new Point(150, 80);
            dtpTransactionDate.Name = "dtpTransactionDate";
            dtpTransactionDate.Size = new Size(200, 23);
            dtpTransactionDate.TabIndex = 9;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Microsoft Sans Serif", 10F);
            label4.Location = new Point(20, 83);
            label4.Name = "label4";
            label4.Size = new Size(118, 17);
            label4.TabIndex = 8;
            label4.Text = "Transaction Date:";
            // 
            // txtTransactionNo
            // 
            txtTransactionNo.Font = new Font("Microsoft Sans Serif", 10F);
            txtTransactionNo.Location = new Point(500, 45);
            txtTransactionNo.Name = "txtTransactionNo";
            txtTransactionNo.Size = new Size(200, 23);
            txtTransactionNo.TabIndex = 7;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Microsoft Sans Serif", 10F);
            label3.Location = new Point(400, 48);
            label3.Name = "label3";
            label3.Size = new Size(108, 17);
            label3.TabIndex = 6;
            label3.Text = "Transaction No:";
            // 
            // cmbTransactionType
            // 
            cmbTransactionType.Font = new Font("Microsoft Sans Serif", 10F);
            cmbTransactionType.FormattingEnabled = true;
            cmbTransactionType.Location = new Point(150, 45);
            cmbTransactionType.Name = "cmbTransactionType";
            cmbTransactionType.Size = new Size(200, 24);
            cmbTransactionType.TabIndex = 5;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 10F);
            label2.Location = new Point(20, 48);
            label2.Name = "label2";
            label2.Size = new Size(118, 17);
            label2.TabIndex = 4;
            label2.Text = "Transaction Type:";
            // 
            // cmbToGodown
            // 
            cmbToGodown.Font = new Font("Microsoft Sans Serif", 10F);
            cmbToGodown.FormattingEnabled = true;
            cmbToGodown.Location = new Point(500, 15);
            cmbToGodown.Name = "cmbToGodown";
            cmbToGodown.Size = new Size(200, 24);
            cmbToGodown.TabIndex = 3;
            // 
            // lblToGodown
            // 
            lblToGodown.AutoSize = true;
            lblToGodown.Font = new Font("Microsoft Sans Serif", 10F);
            lblToGodown.Location = new Point(400, 18);
            lblToGodown.Name = "lblToGodown";
            lblToGodown.Size = new Size(85, 17);
            lblToGodown.TabIndex = 2;
            lblToGodown.Text = "To Godown:";
            // 
            // cmbFromGodown
            // 
            cmbFromGodown.Font = new Font("Microsoft Sans Serif", 10F);
            cmbFromGodown.FormattingEnabled = true;
            cmbFromGodown.Location = new Point(150, 15);
            cmbFromGodown.Name = "cmbFromGodown";
            cmbFromGodown.Size = new Size(200, 24);
            cmbFromGodown.TabIndex = 1;
            // 
            // lblFromGodown
            // 
            lblFromGodown.AutoSize = true;
            lblFromGodown.Font = new Font("Microsoft Sans Serif", 10F);
            lblFromGodown.Location = new Point(20, 18);
            lblFromGodown.Name = "lblFromGodown";
            lblFromGodown.Size = new Size(100, 17);
            lblFromGodown.TabIndex = 0;
            lblFromGodown.Text = "From Godown:";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(dgvTransactionDetails);
            groupBox2.Controls.Add(panel1);
            groupBox2.Dock = DockStyle.Fill;
            groupBox2.Location = new Point(0, 120);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(1200, 450);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Transaction Items";
            // 
            // dgvTransactionDetails
            // 
            dgvTransactionDetails.BackgroundColor = SystemColors.Window;
            dgvTransactionDetails.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTransactionDetails.Dock = DockStyle.Fill;
            dgvTransactionDetails.Location = new Point(3, 19);
            dgvTransactionDetails.Name = "dgvTransactionDetails";
            dgvTransactionDetails.Size = new Size(1194, 398);
            dgvTransactionDetails.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnRemoveRow);
            panel1.Controls.Add(btnAddRow);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(3, 417);
            panel1.Name = "panel1";
            panel1.Size = new Size(1194, 30);
            panel1.TabIndex = 1;
            // 
            // btnRemoveRow
            // 
            btnRemoveRow.BackColor = Color.LightCoral;
            btnRemoveRow.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            btnRemoveRow.Location = new Point(120, 3);
            btnRemoveRow.Name = "btnRemoveRow";
            btnRemoveRow.Size = new Size(100, 25);
            btnRemoveRow.TabIndex = 1;
            btnRemoveRow.Text = "Remove Row";
            btnRemoveRow.UseVisualStyleBackColor = false;
            btnRemoveRow.Click += btnRemoveRow_Click;
            // 
            // btnAddRow
            // 
            btnAddRow.BackColor = Color.LightBlue;
            btnAddRow.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            btnAddRow.Location = new Point(10, 3);
            btnAddRow.Name = "btnAddRow";
            btnAddRow.Size = new Size(100, 25);
            btnAddRow.TabIndex = 0;
            btnAddRow.Text = "Add Row";
            btnAddRow.UseVisualStyleBackColor = false;
            btnAddRow.Click += btnAddRow_Click;
            // 
            // panel2
            // 
            panel2.Controls.Add(btnClear);
            panel2.Controls.Add(btnSave);
            panel2.Dock = DockStyle.Bottom;
            panel2.Location = new Point(0, 570);
            panel2.Name = "panel2";
            panel2.Size = new Size(1200, 52);
            panel2.TabIndex = 2;
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
            // GodownTransactionControl
            // 
            Controls.Add(groupBox2);
            Controls.Add(panel2);
            Controls.Add(groupBox1);
            Name = "GodownTransactionControl";
            Size = new Size(1200, 622);
            Load += GodownTransactionControl_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvTransactionDetails).EndInit();
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtReferenceNo;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DateTimePicker dtpTransactionDate;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtTransactionNo;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbTransactionType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbToGodown;
        private System.Windows.Forms.Label lblToGodown;
        private System.Windows.Forms.ComboBox cmbFromGodown;
        private System.Windows.Forms.Label lblFromGodown;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView dgvTransactionDetails;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnRemoveRow;
        private System.Windows.Forms.Button btnAddRow;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnSave;
    }
}

