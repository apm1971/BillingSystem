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
            pnlChequeDetails = new Panel();
            txtChequeAmountFirm2 = new TextBox();
            lblChequeAmountFirm2 = new Label();
            txtChequeAmountFirm1 = new TextBox();
            lblChequeAmountFirm1 = new Label();
            lblChequeDetails = new Label();
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
            pnlAdvanceDisplay = new Panel();
            lblAdvanceTitle = new Label();
            lblAdvanceCash = new Label();
            lblAdvanceFirm1 = new Label();
            lblAdvanceFirm2 = new Label();
            btnClear = new Button();
            btnSave = new Button();
            toolTip1 = new ToolTip(components);
            pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvOutstandingBills).BeginInit();
            gbPayment.SuspendLayout();
            pnlChequeDetails.SuspendLayout();
            pnlButtons.SuspendLayout();
            pnlAdvanceDisplay.SuspendLayout();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.Controls.Add(cmbBroker);
            pnlTop.Controls.Add(lblBroker);
            pnlTop.Controls.Add(cmbParty);
            pnlTop.Controls.Add(lblParty);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(11, 11);
            pnlTop.Margin = new Padding(4);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(1410, 46);
            pnlTop.TabIndex = 0;
            // 
            // cmbBroker
            // 
            cmbBroker.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbBroker.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbBroker.FormattingEnabled = true;
            cmbBroker.Location = new Point(570, 10);
            cmbBroker.Margin = new Padding(4);
            cmbBroker.Name = "cmbBroker";
            cmbBroker.Size = new Size(350, 23);
            cmbBroker.TabIndex = 1;
            // 
            // lblBroker
            // 
            lblBroker.AutoSize = true;
            lblBroker.Location = new Point(470, 14);
            lblBroker.Margin = new Padding(4, 0, 4, 0);
            lblBroker.Name = "lblBroker";
            lblBroker.Size = new Size(78, 15);
            lblBroker.TabIndex = 0;
            lblBroker.Text = "Select Broker:";
            // 
            // cmbParty
            // 
            cmbParty.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbParty.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbParty.FormattingEnabled = true;
            cmbParty.Location = new Point(102, 10);
            cmbParty.Margin = new Padding(4);
            cmbParty.Name = "cmbParty";
            cmbParty.Size = new Size(350, 23);
            cmbParty.TabIndex = 0;
            // 
            // lblParty
            // 
            lblParty.AutoSize = true;
            lblParty.Location = new Point(4, 14);
            lblParty.Margin = new Padding(4, 0, 4, 0);
            lblParty.Name = "lblParty";
            lblParty.Size = new Size(80, 15);
            lblParty.TabIndex = 0;
            lblParty.Text = "Select a Party:";
            // 
            // dgvOutstandingBills
            // 
            dgvOutstandingBills.AllowUserToAddRows = false;
            dgvOutstandingBills.AllowUserToDeleteRows = false;
            dgvOutstandingBills.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvOutstandingBills.Dock = DockStyle.Top;
            dgvOutstandingBills.Location = new Point(11, 57);
            dgvOutstandingBills.Margin = new Padding(4);
            dgvOutstandingBills.Name = "dgvOutstandingBills";
            dgvOutstandingBills.RowHeadersWidth = 51;
            dgvOutstandingBills.Size = new Size(1410, 346);
            dgvOutstandingBills.TabIndex = 1;
            // 
            // btnCalculate
            // 
            btnCalculate.BackColor = Color.LightPink;
            btnCalculate.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCalculate.ForeColor = Color.Black;
            btnCalculate.Location = new Point(501, 57);
            btnCalculate.Margin = new Padding(4);
            btnCalculate.Name = "btnCalculate";
            btnCalculate.Size = new Size(116, 33);
            btnCalculate.TabIndex = 10;
            btnCalculate.Text = "Calculate";
            toolTip1.SetToolTip(btnCalculate, "Calculate final amount based on the terms provided");
            btnCalculate.UseVisualStyleBackColor = false;
            btnCalculate.Click += btnCalculate_Click_2;
            // 
            // txtBrokerageRate
            // 
            txtBrokerageRate.Location = new Point(163, 101);
            txtBrokerageRate.Margin = new Padding(4);
            txtBrokerageRate.Name = "txtBrokerageRate";
            txtBrokerageRate.Size = new Size(92, 23);
            txtBrokerageRate.TabIndex = 4;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(47, 104);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(89, 15);
            label7.TabIndex = 6;
            label7.Text = "Brokerage Rate:";
            // 
            // txtInterestRate
            // 
            txtInterestRate.Location = new Point(102, 63);
            txtInterestRate.Margin = new Padding(4);
            txtInterestRate.Name = "txtInterestRate";
            txtInterestRate.Size = new Size(76, 23);
            txtInterestRate.TabIndex = 1;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(8, 66);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(75, 15);
            label3.TabIndex = 4;
            label3.Text = "Interest Rate:";
            // 
            // txtDiscountRate
            // 
            txtDiscountRate.Location = new Point(292, 63);
            txtDiscountRate.Margin = new Padding(4);
            txtDiscountRate.Name = "txtDiscountRate";
            txtDiscountRate.Size = new Size(74, 23);
            txtDiscountRate.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(194, 66);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(83, 15);
            label2.TabIndex = 2;
            label2.Text = "Discount Rate:";
            // 
            // txtInterestDays
            // 
            txtInterestDays.Location = new Point(102, 34);
            txtInterestDays.Margin = new Padding(4);
            txtInterestDays.Name = "txtInterestDays";
            txtInterestDays.Size = new Size(76, 23);
            txtInterestDays.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 37);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(77, 15);
            label1.TabIndex = 0;
            label1.Text = "Interest Days:";
            // 
            // txtDiscountDays
            // 
            txtDiscountDays.Location = new Point(291, 35);
            txtDiscountDays.Margin = new Padding(4);
            txtDiscountDays.Name = "txtDiscountDays";
            txtDiscountDays.Size = new Size(75, 23);
            txtDiscountDays.TabIndex = 2;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(192, 38);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(85, 15);
            label8.TabIndex = 7;
            label8.Text = "Discount Days:";
            // 
            // lblFinalAmount
            // 
            lblFinalAmount.AutoSize = true;
            lblFinalAmount.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblFinalAmount.ForeColor = Color.ForestGreen;
            lblFinalAmount.Location = new Point(744, 22);
            lblFinalAmount.Margin = new Padding(4, 0, 4, 0);
            lblFinalAmount.Name = "lblFinalAmount";
            lblFinalAmount.Size = new Size(164, 20);
            lblFinalAmount.TabIndex = 2;
            lblFinalAmount.Text = "Amount Due: ₹0.00";
            // 
            // lblInterestValue
            // 
            lblInterestValue.AutoSize = true;
            lblInterestValue.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblInterestValue.Location = new Point(501, 25);
            lblInterestValue.Margin = new Padding(4, 0, 4, 0);
            lblInterestValue.Name = "lblInterestValue";
            lblInterestValue.Size = new Size(114, 17);
            lblInterestValue.TabIndex = 1;
            lblInterestValue.Text = "Interest: ₹0.00";
            // 
            // lblDiscountValue
            // 
            lblDiscountValue.AutoSize = true;
            lblDiscountValue.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblDiscountValue.Location = new Point(47, 25);
            lblDiscountValue.Margin = new Padding(4, 0, 4, 0);
            lblDiscountValue.Name = "lblDiscountValue";
            lblDiscountValue.Size = new Size(122, 17);
            lblDiscountValue.TabIndex = 0;
            lblDiscountValue.Text = "Discount: ₹0.00";
            // 
            // lblBrokerageValue
            // 
            lblBrokerageValue.AutoSize = true;
            lblBrokerageValue.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblBrokerageValue.Location = new Point(265, 25);
            lblBrokerageValue.Margin = new Padding(4, 0, 4, 0);
            lblBrokerageValue.Name = "lblBrokerageValue";
            lblBrokerageValue.Size = new Size(134, 17);
            lblBrokerageValue.TabIndex = 3;
            lblBrokerageValue.Text = "Brokerage: ₹0.00";
            // 
            // gbPayment
            // 
            gbPayment.Controls.Add(pnlChequeDetails);
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
            gbPayment.Location = new Point(11, 403);
            gbPayment.Margin = new Padding(4);
            gbPayment.Name = "gbPayment";
            gbPayment.Padding = new Padding(4);
            gbPayment.Size = new Size(1410, 183);
            gbPayment.TabIndex = 4;
            gbPayment.TabStop = false;
            gbPayment.Text = "Payment Details";
            // 
            // pnlChequeDetails
            // 
            pnlChequeDetails.Controls.Add(txtChequeAmountFirm2);
            pnlChequeDetails.Controls.Add(lblChequeAmountFirm2);
            pnlChequeDetails.Controls.Add(txtChequeAmountFirm1);
            pnlChequeDetails.Controls.Add(lblChequeAmountFirm1);
            pnlChequeDetails.Controls.Add(lblChequeDetails);
            pnlChequeDetails.Location = new Point(970, 57);
            pnlChequeDetails.Name = "pnlChequeDetails";
            pnlChequeDetails.Size = new Size(300, 100);
            pnlChequeDetails.TabIndex = 13;
            pnlChequeDetails.Visible = false;
            // 
            // txtChequeAmountFirm2
            // 
            txtChequeAmountFirm2.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtChequeAmountFirm2.Location = new Point(150, 65);
            txtChequeAmountFirm2.Margin = new Padding(4);
            txtChequeAmountFirm2.Name = "txtChequeAmountFirm2";
            txtChequeAmountFirm2.Size = new Size(130, 22);
            txtChequeAmountFirm2.TabIndex = 15;
            txtChequeAmountFirm2.Text = "0.00";
            txtChequeAmountFirm2.TextChanged += TxtChequeAmount_TextChanged;
            // 
            // lblChequeAmountFirm2
            // 
            lblChequeAmountFirm2.AutoSize = true;
            lblChequeAmountFirm2.Location = new Point(13, 68);
            lblChequeAmountFirm2.Margin = new Padding(4, 0, 4, 0);
            lblChequeAmountFirm2.Name = "lblChequeAmountFirm2";
            lblChequeAmountFirm2.Size = new Size(131, 15);
            lblChequeAmountFirm2.TabIndex = 14;
            lblChequeAmountFirm2.Text = "Cheque Amount Firm2:";
            // 
            // txtChequeAmountFirm1
            // 
            txtChequeAmountFirm1.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtChequeAmountFirm1.Location = new Point(150, 35);
            txtChequeAmountFirm1.Margin = new Padding(4);
            txtChequeAmountFirm1.Name = "txtChequeAmountFirm1";
            txtChequeAmountFirm1.Size = new Size(130, 22);
            txtChequeAmountFirm1.TabIndex = 13;
            txtChequeAmountFirm1.Text = "0.00";
            txtChequeAmountFirm1.TextChanged += TxtChequeAmount_TextChanged;
            // 
            // lblChequeAmountFirm1
            // 
            lblChequeAmountFirm1.AutoSize = true;
            lblChequeAmountFirm1.Location = new Point(13, 38);
            lblChequeAmountFirm1.Margin = new Padding(4, 0, 4, 0);
            lblChequeAmountFirm1.Name = "lblChequeAmountFirm1";
            lblChequeAmountFirm1.Size = new Size(131, 15);
            lblChequeAmountFirm1.TabIndex = 12;
            lblChequeAmountFirm1.Text = "Cheque Amount Firm1:";
            // 
            // lblChequeDetails
            // 
            lblChequeDetails.AutoSize = true;
            lblChequeDetails.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblChequeDetails.Location = new Point(13, 10);
            lblChequeDetails.Margin = new Padding(4, 0, 4, 0);
            lblChequeDetails.Name = "lblChequeDetails";
            lblChequeDetails.Size = new Size(93, 15);
            lblChequeDetails.TabIndex = 11;
            lblChequeDetails.Text = "Cheque Details:";
            // 
            // btnAutoAllocate
            // 
            btnAutoAllocate.Location = new Point(970, 20);
            btnAutoAllocate.Margin = new Padding(4);
            btnAutoAllocate.Name = "btnAutoAllocate";
            btnAutoAllocate.Size = new Size(130, 22);
            btnAutoAllocate.TabIndex = 7;
            btnAutoAllocate.Text = "Allocate";
            toolTip1.SetToolTip(btnAutoAllocate, "Automatically apply the payment amount to the oldest bills first");
            btnAutoAllocate.UseVisualStyleBackColor = true;
            // 
            // txtPaymentDate
            // 
            txtPaymentDate.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtPaymentDate.Location = new Point(501, 25);
            txtPaymentDate.Margin = new Padding(4);
            txtPaymentDate.MaxLength = 10;
            txtPaymentDate.Name = "txtPaymentDate";
            txtPaymentDate.Size = new Size(116, 22);
            txtPaymentDate.TabIndex = 5;
            // 
            // lblPaymentDate
            // 
            lblPaymentDate.AutoSize = true;
            lblPaymentDate.Location = new Point(390, 27);
            lblPaymentDate.Margin = new Padding(4, 0, 4, 0);
            lblPaymentDate.Name = "lblPaymentDate";
            lblPaymentDate.Size = new Size(84, 15);
            lblPaymentDate.TabIndex = 5;
            lblPaymentDate.Text = "Payment Date:";
            // 
            // txtReference
            // 
            txtReference.Location = new Point(777, 96);
            txtReference.Margin = new Padding(4);
            txtReference.Name = "txtReference";
            txtReference.Size = new Size(175, 23);
            txtReference.TabIndex = 9;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(639, 101);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(104, 15);
            label6.TabIndex = 4;
            label6.Text = "Reference / Notes:";
            // 
            // cmbPaymentMethod
            // 
            cmbPaymentMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPaymentMethod.FormattingEnabled = true;
            cmbPaymentMethod.Items.AddRange(new object[] { "Cash", "Cheque" });
            cmbPaymentMethod.Location = new Point(777, 57);
            cmbPaymentMethod.Margin = new Padding(4);
            cmbPaymentMethod.Name = "cmbPaymentMethod";
            cmbPaymentMethod.Size = new Size(175, 23);
            cmbPaymentMethod.TabIndex = 8;
            cmbPaymentMethod.SelectedIndexChanged += CmbPaymentMethod_SelectedIndexChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(645, 63);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(102, 15);
            label5.TabIndex = 2;
            label5.Text = "Payment Method:";
            // 
            // txtPaymentAmount
            // 
            txtPaymentAmount.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtPaymentAmount.Location = new Point(777, 22);
            txtPaymentAmount.Margin = new Padding(4);
            txtPaymentAmount.Name = "txtPaymentAmount";
            txtPaymentAmount.Size = new Size(175, 22);
            txtPaymentAmount.TabIndex = 6;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(644, 25);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(104, 15);
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
            pnlButtons.Location = new Point(11, 586);
            pnlButtons.Margin = new Padding(4);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(1410, 50);
            pnlButtons.TabIndex = 5;
            // 
            // pnlAdvanceDisplay
            // 
            pnlAdvanceDisplay.BackColor = Color.LightYellow;
            pnlAdvanceDisplay.BorderStyle = BorderStyle.FixedSingle;
            pnlAdvanceDisplay.Controls.Add(lblAdvanceTitle);
            pnlAdvanceDisplay.Controls.Add(lblAdvanceCash);
            pnlAdvanceDisplay.Controls.Add(lblAdvanceFirm1);
            pnlAdvanceDisplay.Controls.Add(lblAdvanceFirm2);
            pnlAdvanceDisplay.Location = new Point(11, 539);
            pnlAdvanceDisplay.Name = "pnlAdvanceDisplay";
            pnlAdvanceDisplay.Size = new Size(600, 40);
            pnlAdvanceDisplay.TabIndex = 15;
            pnlAdvanceDisplay.Visible = false;
            // 
            // lblAdvanceTitle
            // 
            lblAdvanceTitle.AutoSize = true;
            lblAdvanceTitle.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            lblAdvanceTitle.Location = new Point(5, 3);
            lblAdvanceTitle.Name = "lblAdvanceTitle";
            lblAdvanceTitle.Size = new Size(84, 13);
            lblAdvanceTitle.TabIndex = 0;
            lblAdvanceTitle.Text = "Advance Avail:";
            // 
            // lblAdvanceCash
            // 
            lblAdvanceCash.AutoSize = true;
            lblAdvanceCash.Font = new Font("Segoe UI", 8.25F);
            lblAdvanceCash.ForeColor = Color.Green;
            lblAdvanceCash.Location = new Point(5, 20);
            lblAdvanceCash.Name = "lblAdvanceCash";
            lblAdvanceCash.Size = new Size(65, 13);
            lblAdvanceCash.TabIndex = 1;
            lblAdvanceCash.Text = "Cash: ₹0.00";
            // 
            // lblAdvanceFirm1
            // 
            lblAdvanceFirm1.AutoSize = true;
            lblAdvanceFirm1.Font = new Font("Segoe UI", 8.25F);
            lblAdvanceFirm1.ForeColor = Color.Blue;
            lblAdvanceFirm1.Location = new Point(150, 20);
            lblAdvanceFirm1.Name = "lblAdvanceFirm1";
            lblAdvanceFirm1.Size = new Size(68, 13);
            lblAdvanceFirm1.TabIndex = 2;
            lblAdvanceFirm1.Text = "Firm1: ₹0.00";
            // 
            // lblAdvanceFirm2
            // 
            lblAdvanceFirm2.AutoSize = true;
            lblAdvanceFirm2.Font = new Font("Segoe UI", 8.25F);
            lblAdvanceFirm2.ForeColor = Color.Blue;
            lblAdvanceFirm2.Location = new Point(300, 20);
            lblAdvanceFirm2.Name = "lblAdvanceFirm2";
            lblAdvanceFirm2.Size = new Size(68, 13);
            lblAdvanceFirm2.TabIndex = 3;
            lblAdvanceFirm2.Text = "Firm2: ₹0.00";
            // 
            // btnClear
            // 
            btnClear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClear.Location = new Point(1277, 12);
            btnClear.Margin = new Padding(4);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(116, 32);
            btnClear.TabIndex = 12;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSave.BackColor = Color.LightGreen;
            btnSave.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnSave.Location = new Point(1134, 11);
            btnSave.Margin = new Padding(4);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(123, 33);
            btnSave.TabIndex = 11;
            btnSave.Text = "Save Payment";
            btnSave.UseVisualStyleBackColor = false;
            // 
            // PaymentEntryControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlAdvanceDisplay);
            Controls.Add(gbPayment);
            Controls.Add(dgvOutstandingBills);
            Controls.Add(pnlTop);
            Controls.Add(pnlButtons);
            Margin = new Padding(4);
            Name = "PaymentEntryControl";
            Padding = new Padding(11);
            Size = new Size(1432, 647);
            Load += PaymentEntryControl_Load;
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvOutstandingBills).EndInit();
            gbPayment.ResumeLayout(false);
            gbPayment.PerformLayout();
            pnlChequeDetails.ResumeLayout(false);
            pnlChequeDetails.PerformLayout();
            pnlButtons.ResumeLayout(false);
            pnlButtons.PerformLayout();
            pnlAdvanceDisplay.ResumeLayout(false);
            pnlAdvanceDisplay.PerformLayout();
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
        private System.Windows.Forms.Panel pnlChequeDetails;
        private System.Windows.Forms.TextBox txtChequeAmountFirm1;
        private System.Windows.Forms.Label lblChequeAmountFirm1;
        private System.Windows.Forms.TextBox txtChequeAmountFirm2;
        private System.Windows.Forms.Label lblChequeAmountFirm2;
        private System.Windows.Forms.Label lblChequeDetails;
        private System.Windows.Forms.Panel pnlAdvanceDisplay;
        private System.Windows.Forms.Label lblAdvanceCash;
        private System.Windows.Forms.Label lblAdvanceFirm1;
        private System.Windows.Forms.Label lblAdvanceFirm2;
        private System.Windows.Forms.Label lblAdvanceTitle;
    }
}
