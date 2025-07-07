namespace SaleBillSystem.NET.Forms
{
    partial class BrokerListForm
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
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblTotalBrokers = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnView = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnNew = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.dgvBrokers = new System.Windows.Forms.DataGridView();
            this.panelTop.SuspendLayout();
            this.panelButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBrokers)).BeginInit();
            this.SuspendLayout();

            // panelTop
            this.panelTop.Controls.Add(this.lblTotalBrokers);
            this.panelTop.Controls.Add(this.txtSearch);
            this.panelTop.Controls.Add(this.lblSearch);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(800, 50);
            this.panelTop.TabIndex = 0;

            // lblSearch
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new System.Drawing.Point(12, 15);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(45, 15);
            this.lblSearch.TabIndex = 0;
            this.lblSearch.Text = "Search:";

            // txtSearch
            this.txtSearch.Location = new System.Drawing.Point(63, 12);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(300, 23);
            this.txtSearch.TabIndex = 1;

            // lblTotalBrokers
            this.lblTotalBrokers.AutoSize = true;
            this.lblTotalBrokers.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotalBrokers.Location = new System.Drawing.Point(400, 15);
            this.lblTotalBrokers.Name = "lblTotalBrokers";
            this.lblTotalBrokers.Size = new System.Drawing.Size(95, 15);
            this.lblTotalBrokers.TabIndex = 2;
            this.lblTotalBrokers.Text = "Total Brokers: 0";

            // panelButtons
            this.panelButtons.Controls.Add(this.btnClose);
            this.panelButtons.Controls.Add(this.btnDelete);
            this.panelButtons.Controls.Add(this.btnView);
            this.panelButtons.Controls.Add(this.btnEdit);
            this.panelButtons.Controls.Add(this.btnNew);
            this.panelButtons.Controls.Add(this.btnRefresh);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Location = new System.Drawing.Point(0, 400);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(800, 50);
            this.panelButtons.TabIndex = 2;

            // btnRefresh
            this.btnRefresh.Location = new System.Drawing.Point(12, 10);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 30);
            this.btnRefresh.TabIndex = 0;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

            // btnNew
            this.btnNew.Location = new System.Drawing.Point(100, 10);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(75, 30);
            this.btnNew.TabIndex = 1;
            this.btnNew.Text = "New";
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);

            // btnEdit
            this.btnEdit.Enabled = false;
            this.btnEdit.Location = new System.Drawing.Point(188, 10);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(75, 30);
            this.btnEdit.TabIndex = 2;
            this.btnEdit.Text = "Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);

            // btnView
            this.btnView.Enabled = false;
            this.btnView.Location = new System.Drawing.Point(276, 10);
            this.btnView.Name = "btnView";
            this.btnView.Size = new System.Drawing.Size(75, 30);
            this.btnView.TabIndex = 3;
            this.btnView.Text = "View";
            this.btnView.UseVisualStyleBackColor = true;
            this.btnView.Click += new System.EventHandler(this.btnView_Click);

            // btnDelete
            this.btnDelete.Enabled = false;
            this.btnDelete.Location = new System.Drawing.Point(364, 10);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 30);
            this.btnDelete.TabIndex = 4;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);

            // btnClose
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(713, 10);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 30);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            // dgvBrokers
            this.dgvBrokers.AllowUserToAddRows = false;
            this.dgvBrokers.AllowUserToDeleteRows = false;
            this.dgvBrokers.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvBrokers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvBrokers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvBrokers.Location = new System.Drawing.Point(0, 50);
            this.dgvBrokers.MultiSelect = false;
            this.dgvBrokers.Name = "dgvBrokers";
            this.dgvBrokers.ReadOnly = true;
            this.dgvBrokers.RowTemplate.Height = 25;
            this.dgvBrokers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvBrokers.Size = new System.Drawing.Size(800, 350);
            this.dgvBrokers.TabIndex = 1;

            // BrokerListForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dgvBrokers);
            this.Controls.Add(this.panelButtons);
            this.Controls.Add(this.panelTop);
            this.Name = "BrokerListForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Broker List";
            this.Load += new System.EventHandler(this.BrokerListForm_Load);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvBrokers)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblTotalBrokers;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnView;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.DataGridView dgvBrokers;
    }
} 