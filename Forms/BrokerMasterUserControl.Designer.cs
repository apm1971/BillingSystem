namespace SaleBillSystem.NET.Forms
{
    partial class BrokerMasterUserControl
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
            groupBox1 = new GroupBox();
            txtSearch = new TextBox();
            label1 = new Label();
            groupBox2 = new GroupBox();
            dgvBrokers = new DataGridView();
            groupBox3 = new GroupBox();
            txtBrokerageRate = new TextBox();
            label9 = new Label();
            txtDiscountRate = new TextBox();
            label8 = new Label();
            txtDiscountDays = new TextBox();
            label7 = new Label();
            txtInterestRate = new TextBox();
            label6 = new Label();
            txtInterestDays = new TextBox();
            label5 = new Label();
            txtPhone = new TextBox();
            label4 = new Label();
            txtBrokerName = new TextBox();
            label3 = new Label();
            panel1 = new Panel();
            btnDelete = new Button();
            btnSave = new Button();
            btnNew = new Button();
            statusStrip1 = new StatusStrip();
            lblTotalBrokers = new ToolStripStatusLabel();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvBrokers).BeginInit();
            groupBox3.SuspendLayout();
            panel1.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(txtSearch);
            groupBox1.Controls.Add(label1);
            groupBox1.Dock = DockStyle.Top;
            groupBox1.Location = new Point(0, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(1200, 60);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Search";
            // 
            // txtSearch
            // 
            txtSearch.Font = new Font("Microsoft Sans Serif", 10F);
            txtSearch.Location = new Point(100, 25);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(300, 23);
            txtSearch.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft Sans Serif", 10F);
            label1.Location = new Point(20, 28);
            label1.Name = "label1";
            label1.Size = new Size(57, 17);
            label1.TabIndex = 0;
            label1.Text = "Search:";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(dgvBrokers);
            groupBox2.Dock = DockStyle.Left;
            groupBox2.Location = new Point(0, 60);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(849, 540);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Brokers List";
            // 
            // dgvBrokers
            // 
            dgvBrokers.BackgroundColor = SystemColors.Window;
            dgvBrokers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvBrokers.Dock = DockStyle.Fill;
            dgvBrokers.Location = new Point(3, 19);
            dgvBrokers.Name = "dgvBrokers";
            dgvBrokers.Size = new Size(843, 518);
            dgvBrokers.TabIndex = 0;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(txtBrokerageRate);
            groupBox3.Controls.Add(label9);
            groupBox3.Controls.Add(txtDiscountRate);
            groupBox3.Controls.Add(label8);
            groupBox3.Controls.Add(txtDiscountDays);
            groupBox3.Controls.Add(label7);
            groupBox3.Controls.Add(txtInterestRate);
            groupBox3.Controls.Add(label6);
            groupBox3.Controls.Add(txtInterestDays);
            groupBox3.Controls.Add(label5);
            groupBox3.Controls.Add(txtPhone);
            groupBox3.Controls.Add(label4);
            groupBox3.Controls.Add(txtBrokerName);
            groupBox3.Controls.Add(label3);
            groupBox3.Dock = DockStyle.Fill;
            groupBox3.Location = new Point(849, 60);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(351, 450);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "Broker Details";
            // 
            // txtBrokerageRate
            // 
            txtBrokerageRate.Font = new Font("Microsoft Sans Serif", 10F);
            txtBrokerageRate.Location = new Point(150, 290);
            txtBrokerageRate.Name = "txtBrokerageRate";
            txtBrokerageRate.Size = new Size(100, 23);
            txtBrokerageRate.TabIndex = 15;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Microsoft Sans Serif", 10F);
            label9.Location = new Point(20, 293);
            label9.Name = "label9";
            label9.Size = new Size(112, 17);
            label9.TabIndex = 14;
            label9.Text = "Brokerage Rate:";
            // 
            // txtDiscountRate
            // 
            txtDiscountRate.Font = new Font("Microsoft Sans Serif", 10F);
            txtDiscountRate.Location = new Point(150, 255);
            txtDiscountRate.Name = "txtDiscountRate";
            txtDiscountRate.Size = new Size(100, 23);
            txtDiscountRate.TabIndex = 13;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Microsoft Sans Serif", 10F);
            label8.Location = new Point(20, 258);
            label8.Name = "label8";
            label8.Size = new Size(101, 17);
            label8.TabIndex = 12;
            label8.Text = "Discount Rate:";
            // 
            // txtDiscountDays
            // 
            txtDiscountDays.Font = new Font("Microsoft Sans Serif", 10F);
            txtDiscountDays.Location = new Point(150, 220);
            txtDiscountDays.Name = "txtDiscountDays";
            txtDiscountDays.Size = new Size(100, 23);
            txtDiscountDays.TabIndex = 11;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Microsoft Sans Serif", 10F);
            label7.Location = new Point(20, 223);
            label7.Name = "label7";
            label7.Size = new Size(103, 17);
            label7.TabIndex = 10;
            label7.Text = "Discount Days:";
            // 
            // txtInterestRate
            // 
            txtInterestRate.Font = new Font("Microsoft Sans Serif", 10F);
            txtInterestRate.Location = new Point(150, 185);
            txtInterestRate.Name = "txtInterestRate";
            txtInterestRate.Size = new Size(100, 23);
            txtInterestRate.TabIndex = 9;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Microsoft Sans Serif", 10F);
            label6.Location = new Point(20, 188);
            label6.Name = "label6";
            label6.Size = new Size(93, 17);
            label6.TabIndex = 8;
            label6.Text = "Interest Rate:";
            // 
            // txtInterestDays
            // 
            txtInterestDays.Font = new Font("Microsoft Sans Serif", 10F);
            txtInterestDays.Location = new Point(150, 150);
            txtInterestDays.Name = "txtInterestDays";
            txtInterestDays.Size = new Size(100, 23);
            txtInterestDays.TabIndex = 7;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Microsoft Sans Serif", 10F);
            label5.Location = new Point(20, 153);
            label5.Name = "label5";
            label5.Size = new Size(95, 17);
            label5.TabIndex = 6;
            label5.Text = "Interest Days:";
            // 
            // txtPhone
            // 
            txtPhone.Font = new Font("Microsoft Sans Serif", 10F);
            txtPhone.Location = new Point(151, 118);
            txtPhone.Name = "txtPhone";
            txtPhone.Size = new Size(194, 23);
            txtPhone.TabIndex = 5;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Microsoft Sans Serif", 10F);
            label4.Location = new Point(20, 118);
            label4.Name = "label4";
            label4.Size = new Size(53, 17);
            label4.TabIndex = 4;
            label4.Text = "Phone:";
            // 
            // txtBrokerName
            // 
            txtBrokerName.Font = new Font("Microsoft Sans Serif", 10F);
            txtBrokerName.Location = new Point(150, 83);
            txtBrokerName.Name = "txtBrokerName";
            txtBrokerName.Size = new Size(195, 23);
            txtBrokerName.TabIndex = 3;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Microsoft Sans Serif", 10F);
            label3.Location = new Point(20, 83);
            label3.Name = "label3";
            label3.Size = new Size(95, 17);
            label3.TabIndex = 2;
            label3.Text = "Broker Name:";
            // 
            // panel1
            // 
            panel1.Controls.Add(btnDelete);
            panel1.Controls.Add(btnSave);
            panel1.Controls.Add(btnNew);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(849, 510);
            panel1.Name = "panel1";
            panel1.Size = new Size(351, 90);
            panel1.TabIndex = 3;
            // 
            // btnDelete
            // 
            btnDelete.BackColor = Color.LightCoral;
            btnDelete.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            btnDelete.Location = new Point(230, 20);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(90, 40);
            btnDelete.TabIndex = 2;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = false;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnSave
            // 
            btnSave.BackColor = Color.LightGreen;
            btnSave.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            btnSave.Location = new Point(120, 20);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(90, 40);
            btnSave.TabIndex = 1;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += btnSave_Click;
            // 
            // btnNew
            // 
            btnNew.BackColor = Color.LightBlue;
            btnNew.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            btnNew.Location = new Point(20, 20);
            btnNew.Name = "btnNew";
            btnNew.Size = new Size(90, 40);
            btnNew.TabIndex = 0;
            btnNew.Text = "New";
            btnNew.UseVisualStyleBackColor = false;
            btnNew.Click += btnNew_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblTotalBrokers });
            statusStrip1.Location = new Point(0, 600);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1200, 22);
            statusStrip1.TabIndex = 4;
            statusStrip1.Text = "statusStrip1";
            // 
            // lblTotalBrokers
            // 
            lblTotalBrokers.Name = "lblTotalBrokers";
            lblTotalBrokers.Size = new Size(86, 17);
            lblTotalBrokers.Text = "Total Brokers: 0";
            // 
            // BrokerMasterUserControl
            // 
            Controls.Add(groupBox3);
            Controls.Add(panel1);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(statusStrip1);
            Name = "BrokerMasterUserControl";
            Size = new Size(1200, 622);
            Load += BrokerMasterUserControl_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvBrokers).EndInit();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            panel1.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView dgvBrokers;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox txtBrokerageRate;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtDiscountRate;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtDiscountDays;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtInterestRate;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtInterestDays;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtPhone;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtBrokerName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblTotalBrokers;
    }
} 