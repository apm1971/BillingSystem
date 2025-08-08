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
            components = new System.ComponentModel.Container();
            pnlTop = new Panel();
            cmbBroker = new ComboBox();
            lblBroker = new Label();
            cmbParty = new ComboBox();
            lblParty = new Label();
            dgvOutstandingBills = new DataGridView();
            btnCalculate = new Button();
            txtBrokerageRate = new TextBox();
            label7 = new Label();
            txtInterestRate = new TextBox();
            label3 = new Label();
            txtDiscountRate = new TextBox();
            label2 = new Label();
            txtInterestDays = new TextBox();
            label1 = new Label();
            txtDiscountDays = new TextBox();
            label8 = new Label();
            lblFinalAmount = new Label();
            lblInterestValue = new Label();
            lblDiscountValue = new Label();
            lblBrokerageValue = new Label();
            gbPayment = new GroupBox();
            btnAutoAllocate = new Button();
            txtPaymentDate = new TextBox();
            lblPaymentDate = new Label();
            txtReference = new TextBox();
            label6 = new Label();
            cmbPaymentMethod = new ComboBox();
            label5 = new Label();
            txtPaymentAmount = new TextBox();
            label4 = new Label();
            pnlButtons = new Panel();
            btnClear = new Button();
            btnSave = new Button();
            toolTip1 = new ToolTip(components);
            pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvOutstandingBills).BeginInit();
            gbPayment.SuspendLayout();
            pnlButtons.SuspendLayout();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.Controls.Add(cmbBroker);
            pnlTop.Controls.Add(lblBroker);
            pnlTop.Controls.Add(cmbParty);
            pnlTop.Controls.Add(lblParty);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(13, 15);
            pnlTop.Margin = new Padding(4, 5, 4, 5);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(1610, 62);
            pnlTop.TabIndex = 0;
            // 
            // cmbBroker
            // 
            cmbBroker.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbBroker.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbBroker.FormattingEnabled = true;
            cmbBroker.Location = new Point(651, 14);
            cmbBroker.Margin = new Padding(4, 5, 4, 5);
            cmbBroker.Name = "cmbBroker";
            cmbBroker.Size = new Size(399, 28);
            cmbBroker.TabIndex = 1;
            // 
            // lblBroker
            // 
            lblBroker.AutoSize = true;
            lblBroker.Location = new Point(537, 18);
            lblBroker.Margin = new Padding(4, 0, 4, 0);
            lblBroker.Name = "lblBroker";
            lblBroker.Size = new Size(99, 20);
            lblBroker.TabIndex = 0;
            lblBroker.Text = "Select Broker:";
            // 
            // cmbParty
            // 
            cmbParty.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbParty.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbParty.FormattingEnabled = true;
            cmbParty.Location = new Point(117, 14);
            cmbParty.Margin = new Padding(4, 5, 4, 5);
            cmbParty.Name = "cmbParty";
            cmbParty.Size = new Size(399, 28);
            cmbParty.TabIndex = 0;
            // 
            // lblParty
            // 
            lblParty.AutoSize = true;
            lblParty.Location = new Point(4, 18);
            lblParty.Margin = new Padding(4, 0, 4, 0);
            lblParty.Name = "lblParty";
            lblParty.Size = new Size(100, 20);
            lblParty.TabIndex = 0;
            lblParty.Text = "Select a Party:";
            // 
            // dgvOutstandingBills
            // 
            dgvOutstandingBills.AllowUserToAddRows = false;
            dgvOutstandingBills.AllowUserToDeleteRows = false;
            dgvOutstandingBills.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvOutstandingBills.Dock = DockStyle.Top;
            dgvOutstandingBills.Location = new Point(13, 77);
            dgvOutstandingBills.Margin = new Padding(4, 5, 4, 5);
            dgvOutstandingBills.Name = "dgvOutstandingBills";
            dgvOutstandingBills.RowHeadersWidth = 51;
            dgvOutstandingBills.Size = new Size(1610, 650);
            dgvOutstandingBills.TabIndex = 1;
            // 
            // btnCalculate
            // 
            btnCalculate.BackColor = Color.LightPink;
            btnCalculate.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCalculate.ForeColor = Color.Black;
            btnCalculate.Location = new Point(573, 76);
            btnCalculate.Margin = new Padding(4, 5, 4, 5);
            btnCalculate.Name = "btnCalculate";
            btnCalculate.Size = new Size(132, 44);
            btnCalculate.TabIndex = 10;
            btnCalculate.Text = "Calculate";
            toolTip1.SetToolTip(btnCalculate, "Calculate final amount based on the terms provided");
            btnCalculate.UseVisualStyleBackColor = false;
            btnCalculate.Click += btnCalculate_Click_2;
            // 
            // txtBrokerageRate
            // 
            txtBrokerageRate.Location = new Point(186, 135);
            txtBrokerageRate.Margin = new Padding(4, 5, 4, 5);
            txtBrokerageRate.Name = "txtBrokerageRate";
            txtBrokerageRate.Size = new Size(105, 27);
            txtBrokerageRate.TabIndex = 4;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(54, 138);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(114, 20);
            label7.TabIndex = 6;
            label7.Text = "Brokerage Rate:";
            // 
            // txtInterestRate
            // 
            txtInterestRate.Location = new Point(117, 84);
            txtInterestRate.Margin = new Padding(4, 5, 4, 5);
            txtInterestRate.Name = "txtInterestRate";
            txtInterestRate.Size = new Size(86, 27);
            txtInterestRate.TabIndex = 1;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(9, 88);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(95, 20);
            label3.TabIndex = 4;
            label3.Text = "Interest Rate:";
            // 
            // txtDiscountRate
            // 
            txtDiscountRate.Location = new Point(334, 84);
            txtDiscountRate.Margin = new Padding(4, 5, 4, 5);
            txtDiscountRate.Name = "txtDiscountRate";
            txtDiscountRate.Size = new Size(84, 27);
            txtDiscountRate.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(222, 88);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(104, 20);
            label2.TabIndex = 2;
            label2.Text = "Discount Rate:";
            // 
            // txtInterestDays
            // 
            txtInterestDays.Location = new Point(117, 46);
            txtInterestDays.Margin = new Padding(4, 5, 4, 5);
            txtInterestDays.Name = "txtInterestDays";
            txtInterestDays.Size = new Size(86, 27);
            txtInterestDays.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(7, 49);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(97, 20);
            label1.TabIndex = 0;
            label1.Text = "Interest Days:";
            // 
            // txtDiscountDays
            // 
            txtDiscountDays.Location = new Point(333, 47);
            txtDiscountDays.Margin = new Padding(4, 5, 4, 5);
            txtDiscountDays.Name = "txtDiscountDays";
            txtDiscountDays.Size = new Size(85, 27);
            txtDiscountDays.TabIndex = 2;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(219, 50);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(106, 20);
            label8.TabIndex = 7;
            label8.Text = "Discount Days:";
            // 
            // lblFinalAmount
            // 
            lblFinalAmount.AutoSize = true;
            lblFinalAmount.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblFinalAmount.ForeColor = Color.ForestGreen;
            lblFinalAmount.Location = new Point(1017, 29);
            lblFinalAmount.Margin = new Padding(4, 0, 4, 0);
            lblFinalAmount.Name = "lblFinalAmount";
            lblFinalAmount.Size = new Size(251, 25);
            lblFinalAmount.TabIndex = 2;
            lblFinalAmount.Text = "Final Amount Due: ₹0.00";
            // 
            // lblInterestValue
            // 
            lblInterestValue.AutoSize = true;
            lblInterestValue.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblInterestValue.Location = new Point(799, 33);
            lblInterestValue.Margin = new Padding(4, 0, 4, 0);
            lblInterestValue.Name = "lblInterestValue";
            lblInterestValue.Size = new Size(130, 20);
            lblInterestValue.TabIndex = 1;
            lblInterestValue.Text = "Interest: ₹0.00";
            // 
            // lblDiscountValue
            // 
            lblDiscountValue.AutoSize = true;
            lblDiscountValue.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblDiscountValue.Location = new Point(292, 33);
            lblDiscountValue.Margin = new Padding(4, 0, 4, 0);
            lblDiscountValue.Name = "lblDiscountValue";
            lblDiscountValue.Size = new Size(141, 20);
            lblDiscountValue.TabIndex = 0;
            lblDiscountValue.Text = "Discount: ₹0.00";
            // 
            // lblBrokerageValue
            // 
            lblBrokerageValue.AutoSize = true;
            lblBrokerageValue.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblBrokerageValue.Location = new Point(537, 33);
            lblBrokerageValue.Margin = new Padding(4, 0, 4, 0);
            lblBrokerageValue.Name = "lblBrokerageValue";
            lblBrokerageValue.Size = new Size(152, 20);
            lblBrokerageValue.TabIndex = 3;
            lblBrokerageValue.Text = "Brokerage: ₹0.00";
            // 
            // gbPayment
            // 
            gbPayment.Controls.Add(btnAutoAllocate);
            gbPayment.Controls.Add(label7);
            gbPayment.Controls.Add(txtBrokerageRate);
            gbPayment.Controls.Add(txtPaymentDate);
            gbPayment.Controls.Add(btnCalculate);
            gbPayment.Controls.Add(lblPaymentDate);
            gbPayment.Controls.Add(label8);
            gbPayment.Controls.Add(txtDiscountDays);
            gbPayment.Controls.Add(label3);
            gbPayment.Controls.Add(txtInterestRate);
            gbPayment.Controls.Add(txtReference);
            gbPayment.Controls.Add(label6);
            gbPayment.Controls.Add(label2);
            gbPayment.Controls.Add(txtDiscountRate);
            gbPayment.Controls.Add(cmbPaymentMethod);
            gbPayment.Controls.Add(label5);
            gbPayment.Controls.Add(label1);
            gbPayment.Controls.Add(txtInterestDays);
            gbPayment.Controls.Add(txtPaymentAmount);
            gbPayment.Controls.Add(label4);
            gbPayment.Dock = DockStyle.Fill;
            gbPayment.Location = new Point(13, 582);
            gbPayment.Margin = new Padding(4, 5, 4, 5);
            gbPayment.Name = "gbPayment";
            gbPayment.Padding = new Padding(4, 5, 4, 5);
            gbPayment.Size = new Size(1610, 199);
            gbPayment.TabIndex = 4;
            gbPayment.TabStop = false;
            gbPayment.Text = "Payment Details";
            // 
            // btnAutoAllocate
            // 
            btnAutoAllocate.Location = new Point(1108, 27);
            btnAutoAllocate.Margin = new Padding(4, 5, 4, 5);
            btnAutoAllocate.Name = "btnAutoAllocate";
            btnAutoAllocate.Size = new Size(149, 29);
            btnAutoAllocate.TabIndex = 7;
            btnAutoAllocate.Text = "Allocate";
            toolTip1.SetToolTip(btnAutoAllocate, "Automatically apply the payment amount to the oldest bills first");
            btnAutoAllocate.UseVisualStyleBackColor = true;
            // 
            // txtPaymentDate
            // 
            txtPaymentDate.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtPaymentDate.Location = new Point(573, 33);
            txtPaymentDate.Margin = new Padding(4, 5, 4, 5);
            txtPaymentDate.MaxLength = 10;
            txtPaymentDate.Name = "txtPaymentDate";
            txtPaymentDate.Size = new Size(132, 26);
            txtPaymentDate.TabIndex = 5;
            // 
            // lblPaymentDate
            // 
            lblPaymentDate.AutoSize = true;
            lblPaymentDate.Location = new Point(446, 36);
            lblPaymentDate.Margin = new Padding(4, 0, 4, 0);
            lblPaymentDate.Name = "lblPaymentDate";
            lblPaymentDate.Size = new Size(104, 20);
            lblPaymentDate.TabIndex = 5;
            lblPaymentDate.Text = "Payment Date:";
            // 
            // txtReference
            // 
            txtReference.Location = new Point(888, 128);
            txtReference.Margin = new Padding(4, 5, 4, 5);
            txtReference.Name = "txtReference";
            txtReference.Size = new Size(199, 27);
            txtReference.TabIndex = 9;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(730, 135);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(131, 20);
            label6.TabIndex = 4;
            label6.Text = "Reference / Notes:";
            // 
            // cmbPaymentMethod
            // 
            cmbPaymentMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPaymentMethod.FormattingEnabled = true;
            cmbPaymentMethod.Items.AddRange(new object[] { "Cash", "UPI", "Card", "Cheque", "Bank Transfer" });
            cmbPaymentMethod.Location = new Point(888, 76);
            cmbPaymentMethod.Margin = new Padding(4, 5, 4, 5);
            cmbPaymentMethod.Name = "cmbPaymentMethod";
            cmbPaymentMethod.Size = new Size(199, 28);
            cmbPaymentMethod.TabIndex = 8;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(737, 84);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(124, 20);
            label5.TabIndex = 2;
            label5.Text = "Payment Method:";
            // 
            // txtPaymentAmount
            // 
            txtPaymentAmount.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtPaymentAmount.Location = new Point(888, 30);
            txtPaymentAmount.Margin = new Padding(4, 5, 4, 5);
            txtPaymentAmount.Name = "txtPaymentAmount";
            txtPaymentAmount.Size = new Size(199, 26);
            txtPaymentAmount.TabIndex = 6;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(736, 33);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(125, 20);
            label4.TabIndex = 0;
            label4.Text = "Payment Amount:";
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(lblInterestValue);
            pnlButtons.Controls.Add(lblFinalAmount);
            pnlButtons.Controls.Add(lblBrokerageValue);
            pnlButtons.Controls.Add(lblDiscountValue);
            pnlButtons.Controls.Add(btnClear);
            pnlButtons.Controls.Add(btnSave);
            pnlButtons.Dock = DockStyle.Bottom;
            pnlButtons.Location = new Point(13, 781);
            pnlButtons.Margin = new Padding(4, 5, 4, 5);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(1610, 67);
            pnlButtons.TabIndex = 5;
            // 
            // btnClear
            // 
            btnClear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClear.Location = new Point(1458, 16);
            btnClear.Margin = new Padding(4, 5, 4, 5);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(133, 42);
            btnClear.TabIndex = 12;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSave.BackColor = Color.LightGreen;
            btnSave.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnSave.Location = new Point(1295, 15);
            btnSave.Margin = new Padding(4, 5, 4, 5);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(141, 44);
            btnSave.TabIndex = 11;
            btnSave.Text = "Save Payment";
            btnSave.UseVisualStyleBackColor = false;
            // 
            // PaymentEntryControl
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(gbPayment);
            Controls.Add(dgvOutstandingBills);
            Controls.Add(pnlTop);
            Controls.Add(pnlButtons);
            Margin = new Padding(4, 5, 4, 5);
            Name = "PaymentEntryControl";
            Padding = new Padding(13, 15, 13, 15);
            Size = new Size(1636, 863);
            Load += PaymentEntryControl_Load;
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvOutstandingBills).EndInit();
            gbPayment.ResumeLayout(false);
            gbPayment.PerformLayout();
            pnlButtons.ResumeLayout(false);
            pnlButtons.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Label lblParty;
        private System.Windows.Forms.ComboBox cmbParty;
        private System.Windows.Forms.ComboBox cmbBroker;
        private System.Windows.Forms.Label lblBroker;
        private System.Windows.Forms.DataGridView dgvOutstandingBills;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtCreditDays;
        private System.Windows.Forms.TextBox txtInterestRate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtDiscountRate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnCalculate;
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
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label lblPaymentDate;
        private System.Windows.Forms.TextBox txtPaymentDate;
        private System.Windows.Forms.Button btnAutoAllocate;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.TextBox txtBrokerageRate;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblBrokerageValue;
        private System.Windows.Forms.TextBox txtInterestDays;
        private System.Windows.Forms.TextBox txtDiscountDays;
        private System.Windows.Forms.Label label8;
    }
}
