namespace SaleBillSystem.NET.Forms
{
    partial class PaymentEntryForm
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
            this.groupBoxPayment = new System.Windows.Forms.GroupBox();
            this.lblPaymentDate = new System.Windows.Forms.Label();
            this.dtpPaymentDate = new System.Windows.Forms.DateTimePicker();
            this.lblPaymentAmount = new System.Windows.Forms.Label();
            this.txtPaymentAmount = new System.Windows.Forms.TextBox();
            this.lblPaymentMethod = new System.Windows.Forms.Label();
            this.cmbPaymentMethod = new System.Windows.Forms.ComboBox();
            this.lblReference = new System.Windows.Forms.Label();
            this.txtReference = new System.Windows.Forms.TextBox();
            this.lblNotes = new System.Windows.Forms.Label();
            this.txtNotes = new System.Windows.Forms.TextBox();
            
            this.groupBoxFilter = new System.Windows.Forms.GroupBox();
            this.rbFilterByParty = new System.Windows.Forms.RadioButton();
            this.rbFilterByBroker = new System.Windows.Forms.RadioButton();
            this.lblParty = new System.Windows.Forms.Label();
            this.cmbParty = new System.Windows.Forms.ComboBox();
            this.lblBroker = new System.Windows.Forms.Label();
            this.cmbBroker = new System.Windows.Forms.ComboBox();
            
            this.groupBoxBills = new System.Windows.Forms.GroupBox();
            this.dgvBills = new System.Windows.Forms.DataGridView();
            this.lblTotalBills = new System.Windows.Forms.Label();
            this.lblTotalOutstanding = new System.Windows.Forms.Label();
            this.btnAutoAllocate = new System.Windows.Forms.Button();
            this.btnClearAllocation = new System.Windows.Forms.Button();
            
            this.groupBoxSummary = new System.Windows.Forms.GroupBox();
            this.lblAllocatedAmount = new System.Windows.Forms.Label();
            this.lblUnallocatedAmount = new System.Windows.Forms.Label();
            
            this.panelButtons = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            
            this.groupBoxPayment.SuspendLayout();
            this.groupBoxFilter.SuspendLayout();
            this.groupBoxBills.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBills)).BeginInit();
            this.groupBoxSummary.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.SuspendLayout();

            // groupBoxPayment
            this.groupBoxPayment.Controls.Add(this.lblPaymentDate);
            this.groupBoxPayment.Controls.Add(this.dtpPaymentDate);
            this.groupBoxPayment.Controls.Add(this.lblPaymentAmount);
            this.groupBoxPayment.Controls.Add(this.txtPaymentAmount);
            this.groupBoxPayment.Controls.Add(this.lblPaymentMethod);
            this.groupBoxPayment.Controls.Add(this.cmbPaymentMethod);
            this.groupBoxPayment.Controls.Add(this.lblReference);
            this.groupBoxPayment.Controls.Add(this.txtReference);
            this.groupBoxPayment.Controls.Add(this.lblNotes);
            this.groupBoxPayment.Controls.Add(this.txtNotes);
            this.groupBoxPayment.Location = new System.Drawing.Point(12, 12);
            this.groupBoxPayment.Name = "groupBoxPayment";
            this.groupBoxPayment.Size = new System.Drawing.Size(760, 120);
            this.groupBoxPayment.TabIndex = 0;
            this.groupBoxPayment.TabStop = false;
            this.groupBoxPayment.Text = "Payment Details";

            // lblPaymentDate
            this.lblPaymentDate.AutoSize = true;
            this.lblPaymentDate.Location = new System.Drawing.Point(15, 25);
            this.lblPaymentDate.Name = "lblPaymentDate";
            this.lblPaymentDate.Size = new System.Drawing.Size(84, 15);
            this.lblPaymentDate.TabIndex = 0;
            this.lblPaymentDate.Text = "Payment Date:";

            // dtpPaymentDate
            this.dtpPaymentDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpPaymentDate.Location = new System.Drawing.Point(105, 22);
            this.dtpPaymentDate.Name = "dtpPaymentDate";
            this.dtpPaymentDate.Size = new System.Drawing.Size(120, 23);
            this.dtpPaymentDate.TabIndex = 1;

            // lblPaymentAmount
            this.lblPaymentAmount.AutoSize = true;
            this.lblPaymentAmount.Location = new System.Drawing.Point(250, 25);
            this.lblPaymentAmount.Name = "lblPaymentAmount";
            this.lblPaymentAmount.Size = new System.Drawing.Size(97, 15);
            this.lblPaymentAmount.TabIndex = 2;
            this.lblPaymentAmount.Text = "Payment Amount:";

            // txtPaymentAmount
            this.txtPaymentAmount.Location = new System.Drawing.Point(350, 22);
            this.txtPaymentAmount.Name = "txtPaymentAmount";
            this.txtPaymentAmount.Size = new System.Drawing.Size(120, 23);
            this.txtPaymentAmount.TabIndex = 3;
            this.txtPaymentAmount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;

            // lblPaymentMethod
            this.lblPaymentMethod.AutoSize = true;
            this.lblPaymentMethod.Location = new System.Drawing.Point(490, 25);
            this.lblPaymentMethod.Name = "lblPaymentMethod";
            this.lblPaymentMethod.Size = new System.Drawing.Size(96, 15);
            this.lblPaymentMethod.TabIndex = 4;
            this.lblPaymentMethod.Text = "Payment Method:";

            // cmbPaymentMethod
            this.cmbPaymentMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPaymentMethod.Location = new System.Drawing.Point(590, 22);
            this.cmbPaymentMethod.Name = "cmbPaymentMethod";
            this.cmbPaymentMethod.Size = new System.Drawing.Size(150, 23);
            this.cmbPaymentMethod.TabIndex = 5;

            // lblReference
            this.lblReference.AutoSize = true;
            this.lblReference.Location = new System.Drawing.Point(15, 60);
            this.lblReference.Name = "lblReference";
            this.lblReference.Size = new System.Drawing.Size(62, 15);
            this.lblReference.TabIndex = 6;
            this.lblReference.Text = "Reference:";

            // txtReference
            this.txtReference.Location = new System.Drawing.Point(105, 57);
            this.txtReference.Name = "txtReference";
            this.txtReference.Size = new System.Drawing.Size(200, 23);
            this.txtReference.TabIndex = 7;

            // lblNotes
            this.lblNotes.AutoSize = true;
            this.lblNotes.Location = new System.Drawing.Point(350, 60);
            this.lblNotes.Name = "lblNotes";
            this.lblNotes.Size = new System.Drawing.Size(41, 15);
            this.lblNotes.TabIndex = 8;
            this.lblNotes.Text = "Notes:";

            // txtNotes
            this.txtNotes.Location = new System.Drawing.Point(400, 57);
            this.txtNotes.Multiline = true;
            this.txtNotes.Name = "txtNotes";
            this.txtNotes.Size = new System.Drawing.Size(340, 50);
            this.txtNotes.TabIndex = 9;

            // groupBoxFilter
            this.groupBoxFilter.Controls.Add(this.rbFilterByParty);
            this.groupBoxFilter.Controls.Add(this.rbFilterByBroker);
            this.groupBoxFilter.Controls.Add(this.lblParty);
            this.groupBoxFilter.Controls.Add(this.cmbParty);
            this.groupBoxFilter.Controls.Add(this.lblBroker);
            this.groupBoxFilter.Controls.Add(this.cmbBroker);
            this.groupBoxFilter.Location = new System.Drawing.Point(12, 140);
            this.groupBoxFilter.Name = "groupBoxFilter";
            this.groupBoxFilter.Size = new System.Drawing.Size(760, 80);
            this.groupBoxFilter.TabIndex = 1;
            this.groupBoxFilter.TabStop = false;
            this.groupBoxFilter.Text = "Filter Outstanding Bills";

            // rbFilterByParty
            this.rbFilterByParty.AutoSize = true;
            this.rbFilterByParty.Checked = true;
            this.rbFilterByParty.Location = new System.Drawing.Point(15, 25);
            this.rbFilterByParty.Name = "rbFilterByParty";
            this.rbFilterByParty.Size = new System.Drawing.Size(70, 19);
            this.rbFilterByParty.TabIndex = 0;
            this.rbFilterByParty.TabStop = true;
            this.rbFilterByParty.Text = "By Party";
            this.rbFilterByParty.UseVisualStyleBackColor = true;

            // rbFilterByBroker
            this.rbFilterByBroker.AutoSize = true;
            this.rbFilterByBroker.Location = new System.Drawing.Point(400, 25);
            this.rbFilterByBroker.Name = "rbFilterByBroker";
            this.rbFilterByBroker.Size = new System.Drawing.Size(79, 19);
            this.rbFilterByBroker.TabIndex = 1;
            this.rbFilterByBroker.Text = "By Broker";
            this.rbFilterByBroker.UseVisualStyleBackColor = true;

            // lblParty
            this.lblParty.AutoSize = true;
            this.lblParty.Location = new System.Drawing.Point(100, 27);
            this.lblParty.Name = "lblParty";
            this.lblParty.Size = new System.Drawing.Size(38, 15);
            this.lblParty.TabIndex = 2;
            this.lblParty.Text = "Party:";

            // cmbParty
            this.cmbParty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbParty.Location = new System.Drawing.Point(145, 24);
            this.cmbParty.Name = "cmbParty";
            this.cmbParty.Size = new System.Drawing.Size(200, 23);
            this.cmbParty.TabIndex = 3;

            // lblBroker
            this.lblBroker.AutoSize = true;
            this.lblBroker.Location = new System.Drawing.Point(490, 27);
            this.lblBroker.Name = "lblBroker";
            this.lblBroker.Size = new System.Drawing.Size(45, 15);
            this.lblBroker.TabIndex = 4;
            this.lblBroker.Text = "Broker:";

            // cmbBroker
            this.cmbBroker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBroker.Enabled = false;
            this.cmbBroker.Location = new System.Drawing.Point(540, 24);
            this.cmbBroker.Name = "cmbBroker";
            this.cmbBroker.Size = new System.Drawing.Size(200, 23);
            this.cmbBroker.TabIndex = 5;

            // groupBoxBills
            this.groupBoxBills.Controls.Add(this.dgvBills);
            this.groupBoxBills.Controls.Add(this.lblTotalBills);
            this.groupBoxBills.Controls.Add(this.lblTotalOutstanding);
            this.groupBoxBills.Controls.Add(this.btnAutoAllocate);
            this.groupBoxBills.Controls.Add(this.btnClearAllocation);
            this.groupBoxBills.Location = new System.Drawing.Point(12, 230);
            this.groupBoxBills.Name = "groupBoxBills";
            this.groupBoxBills.Size = new System.Drawing.Size(760, 320);
            this.groupBoxBills.TabIndex = 2;
            this.groupBoxBills.TabStop = false;
            this.groupBoxBills.Text = "Outstanding Bills";

            // dgvBills
            this.dgvBills.AllowUserToAddRows = false;
            this.dgvBills.AllowUserToDeleteRows = false;
            this.dgvBills.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvBills.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvBills.Location = new System.Drawing.Point(15, 50);
            this.dgvBills.Name = "dgvBills";
            this.dgvBills.RowTemplate.Height = 25;
            this.dgvBills.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvBills.Size = new System.Drawing.Size(730, 220);
            this.dgvBills.TabIndex = 4;

            // lblTotalBills
            this.lblTotalBills.AutoSize = true;
            this.lblTotalBills.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotalBills.Location = new System.Drawing.Point(15, 25);
            this.lblTotalBills.Name = "lblTotalBills";
            this.lblTotalBills.Size = new System.Drawing.Size(116, 15);
            this.lblTotalBills.TabIndex = 0;
            this.lblTotalBills.Text = "Outstanding Bills: 0";

            // lblTotalOutstanding
            this.lblTotalOutstanding.AutoSize = true;
            this.lblTotalOutstanding.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotalOutstanding.Location = new System.Drawing.Point(250, 25);
            this.lblTotalOutstanding.Name = "lblTotalOutstanding";
            this.lblTotalOutstanding.Size = new System.Drawing.Size(143, 15);
            this.lblTotalOutstanding.TabIndex = 1;
            this.lblTotalOutstanding.Text = "Total Outstanding: ₹0.00";

            // btnAutoAllocate
            this.btnAutoAllocate.Location = new System.Drawing.Point(560, 280);
            this.btnAutoAllocate.Name = "btnAutoAllocate";
            this.btnAutoAllocate.Size = new System.Drawing.Size(90, 30);
            this.btnAutoAllocate.TabIndex = 2;
            this.btnAutoAllocate.Text = "Auto Allocate";
            this.btnAutoAllocate.UseVisualStyleBackColor = true;
            this.btnAutoAllocate.Click += new System.EventHandler(this.btnAutoAllocate_Click);

            // btnClearAllocation
            this.btnClearAllocation.Location = new System.Drawing.Point(655, 280);
            this.btnClearAllocation.Name = "btnClearAllocation";
            this.btnClearAllocation.Size = new System.Drawing.Size(90, 30);
            this.btnClearAllocation.TabIndex = 3;
            this.btnClearAllocation.Text = "Clear All";
            this.btnClearAllocation.UseVisualStyleBackColor = true;
            this.btnClearAllocation.Click += new System.EventHandler(this.btnClearAllocation_Click);

            // groupBoxSummary
            this.groupBoxSummary.Controls.Add(this.lblAllocatedAmount);
            this.groupBoxSummary.Controls.Add(this.lblUnallocatedAmount);
            this.groupBoxSummary.Location = new System.Drawing.Point(12, 560);
            this.groupBoxSummary.Name = "groupBoxSummary";
            this.groupBoxSummary.Size = new System.Drawing.Size(760, 60);
            this.groupBoxSummary.TabIndex = 3;
            this.groupBoxSummary.TabStop = false;
            this.groupBoxSummary.Text = "Payment Summary";

            // lblAllocatedAmount
            this.lblAllocatedAmount.AutoSize = true;
            this.lblAllocatedAmount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblAllocatedAmount.Location = new System.Drawing.Point(15, 25);
            this.lblAllocatedAmount.Name = "lblAllocatedAmount";
            this.lblAllocatedAmount.Size = new System.Drawing.Size(89, 15);
            this.lblAllocatedAmount.TabIndex = 0;
            this.lblAllocatedAmount.Text = "Allocated: ₹0.00";

            // lblUnallocatedAmount
            this.lblUnallocatedAmount.AutoSize = true;
            this.lblUnallocatedAmount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblUnallocatedAmount.Location = new System.Drawing.Point(250, 25);
            this.lblUnallocatedAmount.Name = "lblUnallocatedAmount";
            this.lblUnallocatedAmount.Size = new System.Drawing.Size(103, 15);
            this.lblUnallocatedAmount.TabIndex = 1;
            this.lblUnallocatedAmount.Text = "Unallocated: ₹0.00";

            // panelButtons
            this.panelButtons.Controls.Add(this.btnSave);
            this.panelButtons.Controls.Add(this.btnCancel);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Location = new System.Drawing.Point(0, 635);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(784, 50);
            this.panelButtons.TabIndex = 4;

            // btnSave
            this.btnSave.Location = new System.Drawing.Point(620, 10);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 30);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            // btnCancel
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(700, 10);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 30);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // PaymentEntryForm
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(784, 685);
            this.Controls.Add(this.panelButtons);
            this.Controls.Add(this.groupBoxSummary);
            this.Controls.Add(this.groupBoxBills);
            this.Controls.Add(this.groupBoxFilter);
            this.Controls.Add(this.groupBoxPayment);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PaymentEntryForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Payment Entry";
            this.groupBoxPayment.ResumeLayout(false);
            this.groupBoxPayment.PerformLayout();
            this.groupBoxFilter.ResumeLayout(false);
            this.groupBoxFilter.PerformLayout();
            this.groupBoxBills.ResumeLayout(false);
            this.groupBoxBills.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBills)).EndInit();
            this.groupBoxSummary.ResumeLayout(false);
            this.groupBoxSummary.PerformLayout();
            this.panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox groupBoxPayment;
        private System.Windows.Forms.Label lblPaymentDate;
        private System.Windows.Forms.DateTimePicker dtpPaymentDate;
        private System.Windows.Forms.Label lblPaymentAmount;
        private System.Windows.Forms.TextBox txtPaymentAmount;
        private System.Windows.Forms.Label lblPaymentMethod;
        private System.Windows.Forms.ComboBox cmbPaymentMethod;
        private System.Windows.Forms.Label lblReference;
        private System.Windows.Forms.TextBox txtReference;
        private System.Windows.Forms.Label lblNotes;
        private System.Windows.Forms.TextBox txtNotes;
        
        private System.Windows.Forms.GroupBox groupBoxFilter;
        private System.Windows.Forms.RadioButton rbFilterByParty;
        private System.Windows.Forms.RadioButton rbFilterByBroker;
        private System.Windows.Forms.Label lblParty;
        private System.Windows.Forms.ComboBox cmbParty;
        private System.Windows.Forms.Label lblBroker;
        private System.Windows.Forms.ComboBox cmbBroker;
        
        private System.Windows.Forms.GroupBox groupBoxBills;
        private System.Windows.Forms.DataGridView dgvBills;
        private System.Windows.Forms.Label lblTotalBills;
        private System.Windows.Forms.Label lblTotalOutstanding;
        private System.Windows.Forms.Button btnAutoAllocate;
        private System.Windows.Forms.Button btnClearAllocation;
        
        private System.Windows.Forms.GroupBox groupBoxSummary;
        private System.Windows.Forms.Label lblAllocatedAmount;
        private System.Windows.Forms.Label lblUnallocatedAmount;
        
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
} 