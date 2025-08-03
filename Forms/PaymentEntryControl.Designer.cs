namespace SaleBillSystem.NET.Forms
{
    partial class PaymentEntryControl
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
            this.components = new System.ComponentModel.Container();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.cmbParty = new System.Windows.Forms.ComboBox();
            this.lblParty = new System.Windows.Forms.Label();
            this.cmbBroker = new System.Windows.Forms.ComboBox();
            this.lblBroker = new System.Windows.Forms.Label();
            this.dgvOutstandingBills = new System.Windows.Forms.DataGridView();
            this.gbReconciliation = new System.Windows.Forms.GroupBox();
            this.btnCalculate = new System.Windows.Forms.Button();
            this.chkApplyToAll = new System.Windows.Forms.CheckBox();
            this.txtInterestRate = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtDiscountRate = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtCreditDays = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.gbSummary = new System.Windows.Forms.GroupBox();
            this.lblFinalAmount = new System.Windows.Forms.Label();
            this.lblInterestValue = new System.Windows.Forms.Label();
            this.lblDiscountValue = new System.Windows.Forms.Label();
            this.gbPayment = new System.Windows.Forms.GroupBox();
            this.btnAutoAllocate = new System.Windows.Forms.Button();
            this.txtReference = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cmbPaymentMethod = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtPaymentAmount = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.lblPaymentDate = new System.Windows.Forms.Label();
            this.txtPaymentDate = new System.Windows.Forms.TextBox();
            this.pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOutstandingBills)).BeginInit();
            this.gbReconciliation.SuspendLayout();
            this.gbSummary.SuspendLayout();
            this.gbPayment.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.cmbBroker);
            this.pnlTop.Controls.Add(this.lblBroker);
            this.pnlTop.Controls.Add(this.cmbParty);
            this.pnlTop.Controls.Add(this.lblParty);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(10, 10);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(964, 40);
            this.pnlTop.TabIndex = 0;
            // 
            // cmbParty
            // 
            this.cmbParty.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbParty.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbParty.FormattingEnabled = true;
            this.cmbParty.Location = new System.Drawing.Point(88, 9);
            this.cmbParty.Name = "cmbParty";
            this.cmbParty.Size = new System.Drawing.Size(300, 21);
            this.cmbParty.TabIndex = 0;
            // 
            // lblParty
            // 
            this.lblParty.AutoSize = true;
            this.lblParty.Location = new System.Drawing.Point(3, 12);
            this.lblParty.Name = "lblParty";
            this.lblParty.Size = new System.Drawing.Size(79, 13);
            this.lblParty.TabIndex = 0;
            this.lblParty.Text = "Select a Party:";
            // 
            // cmbBroker
            // 
            this.cmbBroker.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbBroker.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbBroker.FormattingEnabled = true;
            this.cmbBroker.Location = new System.Drawing.Point(488, 9);
            this.cmbBroker.Name = "cmbBroker";
            this.cmbBroker.Size = new System.Drawing.Size(300, 21);
            this.cmbBroker.TabIndex = 1;
            // 
            // lblBroker
            // 
            this.lblBroker.AutoSize = true;
            this.lblBroker.Location = new System.Drawing.Point(403, 12);
            this.lblBroker.Name = "lblBroker";
            this.lblBroker.Size = new System.Drawing.Size(79, 13);
            this.lblBroker.TabIndex = 0;
            this.lblBroker.Text = "Select Broker:";
            // 
            // dgvOutstandingBills
            // 
            this.dgvOutstandingBills.AllowUserToAddRows = false;
            this.dgvOutstandingBills.AllowUserToDeleteRows = false;
            this.dgvOutstandingBills.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvOutstandingBills.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvOutstandingBills.Location = new System.Drawing.Point(10, 50);
            this.dgvOutstandingBills.Name = "dgvOutstandingBills";
            this.dgvOutstandingBills.Size = new System.Drawing.Size(964, 180);
            this.dgvOutstandingBills.TabIndex = 1;
            // 
            // gbReconciliation
            // 
            this.gbReconciliation.Controls.Add(this.btnCalculate);
            this.gbReconciliation.Controls.Add(this.chkApplyToAll);
            this.gbReconciliation.Controls.Add(this.txtInterestRate);
            this.gbReconciliation.Controls.Add(this.label3);
            this.gbReconciliation.Controls.Add(this.txtDiscountRate);
            this.gbReconciliation.Controls.Add(this.label2);
            this.gbReconciliation.Controls.Add(this.txtCreditDays);
            this.gbReconciliation.Controls.Add(this.label1);
            this.gbReconciliation.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbReconciliation.Location = new System.Drawing.Point(10, 230);
            this.gbReconciliation.Name = "gbReconciliation";
            this.gbReconciliation.Size = new System.Drawing.Size(964, 80);
            this.gbReconciliation.TabIndex = 2;
            this.gbReconciliation.TabStop = false;
            this.gbReconciliation.Text = "Reconciliation Console";
            // 
            // btnCalculate
            // 
            this.btnCalculate.Location = new System.Drawing.Point(620, 30);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new System.Drawing.Size(120, 25);
            this.btnCalculate.TabIndex = 4;
            this.btnCalculate.Text = "Calculate";
            this.toolTip1.SetToolTip(this.btnCalculate, "Calculate final amount based on the terms provided");
            this.btnCalculate.UseVisualStyleBackColor = true;
            // 
            // chkApplyToAll
            // 
            this.chkApplyToAll.AutoSize = true;
            this.chkApplyToAll.Location = new System.Drawing.Point(420, 35);
            this.chkApplyToAll.Name = "chkApplyToAll";
            this.chkApplyToAll.Size = new System.Drawing.Size(181, 17);
            this.chkApplyToAll.TabIndex = 3;
            this.chkApplyToAll.Text = "Apply these terms to ALL bills";
            this.toolTip1.SetToolTip(this.chkApplyToAll, "If checked, calculation will apply to all outstanding bills for this party");
            this.chkApplyToAll.UseVisualStyleBackColor = true;
            // 
            // txtInterestRate
            // 
            this.txtInterestRate.Location = new System.Drawing.Point(300, 33);
            this.txtInterestRate.Name = "txtInterestRate";
            this.txtInterestRate.Size = new System.Drawing.Size(80, 20);
            this.txtInterestRate.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(215, 36);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Interest Rate(%):";
            // 
            // txtDiscountRate
            // 
            this.txtDiscountRate.Location = new System.Drawing.Point(120, 52);
            this.txtDiscountRate.Name = "txtDiscountRate";
            this.txtDiscountRate.Size = new System.Drawing.Size(80, 20);
            this.txtDiscountRate.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Discount Rate(%):";
            // 
            // txtCreditDays
            // 
            this.txtCreditDays.Location = new System.Drawing.Point(120, 26);
            this.txtCreditDays.Name = "txtCreditDays";
            this.txtCreditDays.Size = new System.Drawing.Size(80, 20);
            this.txtCreditDays.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Credit Days:";
            // 
            // gbSummary
            // 
            this.gbSummary.Controls.Add(this.lblFinalAmount);
            this.gbSummary.Controls.Add(this.lblInterestValue);
            this.gbSummary.Controls.Add(this.lblDiscountValue);
            this.gbSummary.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbSummary.Location = new System.Drawing.Point(10, 310);
            this.gbSummary.Name = "gbSummary";
            this.gbSummary.Size = new System.Drawing.Size(964, 60);
            this.gbSummary.TabIndex = 3;
            this.gbSummary.TabStop = false;
            this.gbSummary.Text = "Financial Summary";
            // 
            // lblFinalAmount
            // 
            this.lblFinalAmount.AutoSize = true;
            this.lblFinalAmount.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFinalAmount.ForeColor = System.Drawing.Color.ForestGreen;
            this.lblFinalAmount.Location = new System.Drawing.Point(416, 25);
            this.lblFinalAmount.Name = "lblFinalAmount";
            this.lblFinalAmount.Size = new System.Drawing.Size(183, 20);
            this.lblFinalAmount.TabIndex = 2;
            this.lblFinalAmount.Text = "Final Amount Due: ₹0.00";
            // 
            // lblInterestValue
            // 
            this.lblInterestValue.AutoSize = true;
            this.lblInterestValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInterestValue.Location = new System.Drawing.Point(214, 28);
            this.lblInterestValue.Name = "lblInterestValue";
            this.lblInterestValue.Size = new System.Drawing.Size(130, 16);
            this.lblInterestValue.TabIndex = 1;
            this.lblInterestValue.Text = "Accrued Interest: ₹0.00";
            // 
            // lblDiscountValue
            // 
            this.lblDiscountValue.AutoSize = true;
            this.lblDiscountValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDiscountValue.Location = new System.Drawing.Point(14, 28);
            this.lblDiscountValue.Name = "lblDiscountValue";
            this.lblDiscountValue.Size = new System.Drawing.Size(132, 16);
            this.lblDiscountValue.TabIndex = 0;
            this.lblDiscountValue.Text = "Earned Discount: ₹0.00";
            // 
            // gbPayment
            // 
            this.gbPayment.Controls.Add(this.btnAutoAllocate);
            this.gbPayment.Controls.Add(this.txtPaymentDate);
            this.gbPayment.Controls.Add(this.lblPaymentDate);
            this.gbPayment.Controls.Add(this.txtReference);
            this.gbPayment.Controls.Add(this.label6);
            this.gbPayment.Controls.Add(this.cmbPaymentMethod);
            this.gbPayment.Controls.Add(this.label5);
            this.gbPayment.Controls.Add(this.txtPaymentAmount);
            this.gbPayment.Controls.Add(this.label4);
            this.gbPayment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbPayment.Location = new System.Drawing.Point(10, 370);
            this.gbPayment.Name = "gbPayment";
            this.gbPayment.Size = new System.Drawing.Size(964, 131);
            this.gbPayment.TabIndex = 4;
            this.gbPayment.TabStop = false;
            this.gbPayment.Text = "Payment Details";
            // 
            // btnAutoAllocate
            // 
            this.btnAutoAllocate.Location = new System.Drawing.Point(280, 28);
            this.btnAutoAllocate.Name = "btnAutoAllocate";
            this.btnAutoAllocate.Size = new System.Drawing.Size(120, 25);
            this.btnAutoAllocate.TabIndex = 1;
            this.btnAutoAllocate.Text = "Auto-Allocate (FIFO)";
            this.toolTip1.SetToolTip(this.btnAutoAllocate, "Automatically apply the payment amount to the oldest bills first");
            this.btnAutoAllocate.UseVisualStyleBackColor = true;
            // 
            // txtReference
            // 
            this.txtReference.Location = new System.Drawing.Point(120, 85);
            this.txtReference.Name = "txtReference";
            this.txtReference.Size = new System.Drawing.Size(280, 20);
            this.txtReference.TabIndex = 3;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(15, 88);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(99, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Reference / Notes:";
            // 
            // cmbPaymentMethod
            // 
            this.cmbPaymentMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPaymentMethod.FormattingEnabled = true;
            this.cmbPaymentMethod.Items.AddRange(new object[] {
            "Cash",
            "UPI",
            "Card",
            "Cheque",
            "Bank Transfer"});
            this.cmbPaymentMethod.Location = new System.Drawing.Point(120, 58);
            this.cmbPaymentMethod.Name = "cmbPaymentMethod";
            this.cmbPaymentMethod.Size = new System.Drawing.Size(150, 21);
            this.cmbPaymentMethod.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 61);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(87, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "Payment Method:";
            // 
            // txtPaymentAmount
            // 
            this.txtPaymentAmount.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPaymentAmount.Location = new System.Drawing.Point(120, 30);
            this.txtPaymentAmount.Name = "txtPaymentAmount";
            this.txtPaymentAmount.Size = new System.Drawing.Size(150, 22);
            this.txtPaymentAmount.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 34);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(87, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Payment Amount:";
            // 
            // pnlButtons
            // 
            this.pnlButtons.Controls.Add(this.btnClear);
            this.pnlButtons.Controls.Add(this.btnClose);
            this.pnlButtons.Controls.Add(this.btnSave);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location = new System.Drawing.Point(10, 501);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(964, 50);
            this.pnlButtons.TabIndex = 5;
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Location = new System.Drawing.Point(639, 10);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(100, 30);
            this.btnClear.TabIndex = 2;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(851, 10);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 30);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close (Esc)";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(745, 10);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 30);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Save Payment";
            this.btnSave.UseVisualStyleBackColor = true;
            // 
            // lblPaymentDate
            // 
            this.lblPaymentDate.AutoSize = true;
            this.lblPaymentDate.Location = new System.Drawing.Point(417, 34);
            this.lblPaymentDate.Name = "lblPaymentDate";
            this.lblPaymentDate.Size = new System.Drawing.Size(77, 13);
            this.lblPaymentDate.TabIndex = 5;
            this.lblPaymentDate.Text = "Payment Date:";
            // 
            // txtPaymentDate
            // 
            this.txtPaymentDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPaymentDate.Location = new System.Drawing.Point(497, 30);
            this.txtPaymentDate.MaxLength = 10;
            this.txtPaymentDate.Name = "txtPaymentDate";
            this.txtPaymentDate.Size = new System.Drawing.Size(100, 22);
            this.txtPaymentDate.TabIndex = 1;
            // 
            // PaymentEntryControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbPayment);
            this.Controls.Add(this.gbSummary);
            this.Controls.Add(this.gbReconciliation);
            this.Controls.Add(this.dgvOutstandingBills);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.pnlButtons);
            this.Name = "PaymentEntryControl";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Size = new System.Drawing.Size(984, 561);
            this.Load += new System.EventHandler(this.PaymentEntryControl_Load);
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOutstandingBills)).EndInit();
            this.gbReconciliation.ResumeLayout(false);
            this.gbReconciliation.PerformLayout();
            this.gbSummary.ResumeLayout(false);
            this.gbSummary.PerformLayout();
            this.gbPayment.ResumeLayout(false);
            this.gbPayment.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Label lblParty;
        private System.Windows.Forms.ComboBox cmbParty;
        private System.Windows.Forms.ComboBox cmbBroker;
        private System.Windows.Forms.Label lblBroker;
        private System.Windows.Forms.DataGridView dgvOutstandingBills;
        private System.Windows.Forms.GroupBox gbReconciliation;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtCreditDays;
        private System.Windows.Forms.TextBox txtInterestRate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtDiscountRate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkApplyToAll;
        private System.Windows.Forms.Button btnCalculate;
        private System.Windows.Forms.GroupBox gbSummary;
        private System.Windows.Forms.Label lblDiscountValue;
        private System.Windows.Forms.Label lblFinalAmount;
        private System.Windows.Forms.Label lblInterestValue;
        private System.Windows.Forms.GroupBox gbPayment;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtPaymentAmount;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmbPaymentMethod;
        private System.Windows.Forms.TextBox txtReference;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label lblPaymentDate;
        private System.Windows.Forms.TextBox txtPaymentDate;
        private System.Windows.Forms.Button btnAutoAllocate;
        private System.Windows.Forms.Button btnClear;
    }
}
