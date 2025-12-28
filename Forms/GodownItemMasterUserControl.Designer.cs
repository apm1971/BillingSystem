namespace SaleBillSystem.NET.Forms
{
    partial class GodownItemMasterUserControl
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
            txtSearch = new TextBox();
            label1 = new Label();
            groupBox2 = new GroupBox();
            dgvItems = new DataGridView();
            groupBox3 = new GroupBox();
            txtItemName = new TextBox();
            label3 = new Label();
            panel1 = new Panel();
            btnDelete = new Button();
            btnSave = new Button();
            btnNew = new Button();
            statusStrip1 = new StatusStrip();
            lblTotalItems = new ToolStripStatusLabel();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvItems).BeginInit();
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
            groupBox2.Controls.Add(dgvItems);
            groupBox2.Dock = DockStyle.Left;
            groupBox2.Location = new Point(0, 60);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(750, 540);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Items List";
            // 
            // dgvItems
            // 
            dgvItems.BackgroundColor = SystemColors.Window;
            dgvItems.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvItems.Dock = DockStyle.Fill;
            dgvItems.Location = new Point(3, 19);
            dgvItems.Name = "dgvItems";
            dgvItems.Size = new Size(744, 518);
            dgvItems.TabIndex = 0;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(txtItemName);
            groupBox3.Controls.Add(label3);
            groupBox3.Dock = DockStyle.Fill;
            groupBox3.Location = new Point(750, 60);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(450, 450);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "Item Details";
            // 
            // txtItemName
            // 
            txtItemName.Font = new Font("Microsoft Sans Serif", 10F);
            txtItemName.Location = new Point(150, 80);
            txtItemName.Name = "txtItemName";
            txtItemName.Size = new Size(250, 23);
            txtItemName.TabIndex = 1;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Microsoft Sans Serif", 10F);
            label3.Location = new Point(20, 83);
            label3.Name = "label3";
            label3.Size = new Size(79, 17);
            label3.TabIndex = 0;
            label3.Text = "Item Name:";
            // 
            // panel1
            // 
            panel1.Controls.Add(btnDelete);
            panel1.Controls.Add(btnSave);
            panel1.Controls.Add(btnNew);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(750, 510);
            panel1.Name = "panel1";
            panel1.Size = new Size(450, 90);
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
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblTotalItems });
            statusStrip1.Location = new Point(0, 600);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1200, 22);
            statusStrip1.TabIndex = 4;
            statusStrip1.Text = "statusStrip1";
            // 
            // lblTotalItems
            // 
            lblTotalItems.Name = "lblTotalItems";
            lblTotalItems.Size = new Size(76, 17);
            lblTotalItems.Text = "Total Items: 0";
            // 
            // GodownItemMasterUserControl
            // 
            Controls.Add(groupBox3);
            Controls.Add(panel1);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(statusStrip1);
            Name = "GodownItemMasterUserControl";
            Size = new Size(1200, 622);
            Load += GodownItemMasterUserControl_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvItems).EndInit();
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
        private System.Windows.Forms.DataGridView dgvItems;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox txtItemName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblTotalItems;
    }
}

