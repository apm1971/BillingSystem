namespace SaleBillSystem.NET.Forms
{
    partial class AdvancePaymentEntryControl
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
            pnlEntry = new Panel();
            btnDelete = new Button();
            btnClear = new Button();
            btnSave = new Button();
            pnlChequeDetails = new Panel();
            txtChequeAmountFirm2 = new TextBox();
            lblChequeAmountFirm2 = new Label();
            txtChequeAmountFirm1 = new TextBox();
            lblChequeAmountFirm1 = new Label();
            txtReference = new TextBox();
            lblReference = new Label();
            cmbPaymentMethod = new ComboBox();
            lblPaymentMethod = new Label();
            nudAmount = new NumericUpDown();
            lblAmount = new Label();
            txtPaymentDate = new TextBox();
            lblPaymentDate = new Label();
            pnlGrid = new Panel();
            dgvAdvancePayments = new DataGridView();
            lblTotalAdvances = new Label();
            toolTip1 = new ToolTip(components);
            pnlTop.SuspendLayout();
            pnlEntry.SuspendLayout();
            pnlChequeDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudAmount).BeginInit();
            pnlGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAdvancePayments).BeginInit();
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
            pnlTop.Padding = new Padding(5);
            pnlTop.Size = new Size(1178, 50);
            pnlTop.TabIndex = 0;
            // 
            // cmbBroker
            // 
            cmbBroker.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbBroker.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbBroker.FormattingEnabled = true;
            cmbBroker.Location = new Point(570, 14);
            cmbBroker.Margin = new Padding(4);
            cmbBroker.Name = "cmbBroker";
            cmbBroker.Size = new Size(300, 23);
            cmbBroker.TabIndex = 1;
            toolTip1.SetToolTip(cmbBroker, "Select a broker for this advance payment (optional)");
            // 
            // lblBroker
            // 
            lblBroker.AutoSize = true;
            lblBroker.Location = new Point(480, 18);
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
            cmbParty.Location = new Point(102, 14);
            cmbParty.Margin = new Padding(4);
            cmbParty.Name = "cmbParty";
            cmbParty.Size = new Size(300, 23);
            cmbParty.TabIndex = 0;
            toolTip1.SetToolTip(cmbParty, "Select a party for this advance payment (required if no broker selected)");
            // 
            // lblParty
            // 
            lblParty.AutoSize = true;
            lblParty.Location = new Point(9, 18);
            lblParty.Margin = new Padding(4, 0, 4, 0);
            lblParty.Name = "lblParty";
            lblParty.Size = new Size(80, 15);
            lblParty.TabIndex = 0;
            lblParty.Text = "Select a Party:";
            // 
            // pnlEntry
            // 
            pnlEntry.BackColor = Color.FromArgb(240, 248, 255);
            pnlEntry.BorderStyle = BorderStyle.FixedSingle;
            pnlEntry.Controls.Add(pnlChequeDetails);
            pnlEntry.Controls.Add(btnDelete);
            pnlEntry.Controls.Add(btnClear);
            pnlEntry.Controls.Add(btnSave);
            pnlEntry.Controls.Add(txtReference);
            pnlEntry.Controls.Add(lblReference);
            pnlEntry.Controls.Add(cmbPaymentMethod);
            pnlEntry.Controls.Add(lblPaymentMethod);
            pnlEntry.Controls.Add(nudAmount);
            pnlEntry.Controls.Add(lblAmount);
            pnlEntry.Controls.Add(txtPaymentDate);
            pnlEntry.Controls.Add(lblPaymentDate);
            pnlEntry.Dock = DockStyle.Top;
            pnlEntry.Location = new Point(11, 61);
            pnlEntry.Margin = new Padding(4);
            pnlEntry.Name = "pnlEntry";
            pnlEntry.Padding = new Padding(10);
            pnlEntry.Size = new Size(1178, 160);
            pnlEntry.TabIndex = 1;
            // 
            // btnClear
            // 
            btnClear.BackColor = Color.LightGray;
            btnClear.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnClear.Location = new Point(835, 115);
            btnClear.Margin = new Padding(4);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(100, 30);
            btnClear.TabIndex = 5;
            btnClear.Text = "&Clear";
            toolTip1.SetToolTip(btnClear, "Clear all fields and reset the form");
            btnClear.UseVisualStyleBackColor = false;
            // 
            // btnSave
            // 
            btnSave.BackColor = Color.LightGreen;
            btnSave.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnSave.Location = new Point(725, 115);
            btnSave.Margin = new Padding(4);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(100, 30);
            btnSave.TabIndex = 4;
            btnSave.Text = "&Save";
            toolTip1.SetToolTip(btnSave, "Save the advance payment entry (Ctrl+S)");
            btnSave.UseVisualStyleBackColor = false;
            // 
            // btnDelete
            // 
            btnDelete.BackColor = Color.LightCoral;
            btnDelete.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnDelete.Location = new Point(945, 115);
            btnDelete.Margin = new Padding(4);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(100, 30);
            btnDelete.TabIndex = 6;
            btnDelete.Text = "&Delete";
            toolTip1.SetToolTip(btnDelete, "Delete selected advance payment (Del)");
            btnDelete.UseVisualStyleBackColor = false;
            // 
            // pnlChequeDetails
            // 
            pnlChequeDetails.Controls.Add(txtChequeAmountFirm2);
            pnlChequeDetails.Controls.Add(lblChequeAmountFirm2);
            pnlChequeDetails.Controls.Add(txtChequeAmountFirm1);
            pnlChequeDetails.Controls.Add(lblChequeAmountFirm1);
            pnlChequeDetails.Location = new Point(15, 75);
            pnlChequeDetails.Name = "pnlChequeDetails";
            pnlChequeDetails.Size = new Size(500, 35);
            pnlChequeDetails.TabIndex = 6;
            pnlChequeDetails.Visible = false;
            // 
            // txtChequeAmountFirm2
            // 
            txtChequeAmountFirm2.Location = new Point(370, 6);
            txtChequeAmountFirm2.Name = "txtChequeAmountFirm2";
            txtChequeAmountFirm2.PlaceholderText = "0.00";
            txtChequeAmountFirm2.Size = new Size(100, 23);
            txtChequeAmountFirm2.TabIndex = 1;
            txtChequeAmountFirm2.TextAlign = HorizontalAlignment.Right;
            toolTip1.SetToolTip(txtChequeAmountFirm2, "Enter Firm 2 cheque amount");
            // 
            // lblChequeAmountFirm2
            // 
            lblChequeAmountFirm2.AutoSize = true;
            lblChequeAmountFirm2.Location = new Point(270, 10);
            lblChequeAmountFirm2.Name = "lblChequeAmountFirm2";
            lblChequeAmountFirm2.Size = new Size(94, 15);
            lblChequeAmountFirm2.TabIndex = 0;
            lblChequeAmountFirm2.Text = "Firm 2 Amount:";
            // 
            // txtChequeAmountFirm1
            // 
            txtChequeAmountFirm1.Location = new Point(120, 6);
            txtChequeAmountFirm1.Name = "txtChequeAmountFirm1";
            txtChequeAmountFirm1.PlaceholderText = "0.00";
            txtChequeAmountFirm1.Size = new Size(100, 23);
            txtChequeAmountFirm1.TabIndex = 0;
            txtChequeAmountFirm1.TextAlign = HorizontalAlignment.Right;
            toolTip1.SetToolTip(txtChequeAmountFirm1, "Enter Firm 1 cheque amount");
            // 
            // lblChequeAmountFirm1
            // 
            lblChequeAmountFirm1.AutoSize = true;
            lblChequeAmountFirm1.Location = new Point(20, 10);
            lblChequeAmountFirm1.Name = "lblChequeAmountFirm1";
            lblChequeAmountFirm1.Size = new Size(94, 15);
            lblChequeAmountFirm1.TabIndex = 0;
            lblChequeAmountFirm1.Text = "Firm 1 Amount:";
            // 
            // txtReference
            // 
            txtReference.CharacterCasing = CharacterCasing.Upper;
            txtReference.Location = new Point(725, 38);
            txtReference.Margin = new Padding(4);
            txtReference.MaxLength = 255;
            txtReference.Name = "txtReference";
            txtReference.Size = new Size(210, 23);
            txtReference.TabIndex = 3;
            toolTip1.SetToolTip(txtReference, "Optional reference number or description");
            // 
            // lblReference
            // 
            lblReference.AutoSize = true;
            lblReference.Location = new Point(650, 42);
            lblReference.Margin = new Padding(4, 0, 4, 0);
            lblReference.Name = "lblReference";
            lblReference.Size = new Size(64, 15);
            lblReference.TabIndex = 0;
            lblReference.Text = "Reference:";
            // 
            // cmbPaymentMethod
            // 
            cmbPaymentMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPaymentMethod.FormattingEnabled = true;
            cmbPaymentMethod.Location = new Point(480, 38);
            cmbPaymentMethod.Margin = new Padding(4);
            cmbPaymentMethod.Name = "cmbPaymentMethod";
            cmbPaymentMethod.Size = new Size(150, 23);
            cmbPaymentMethod.TabIndex = 2;
            toolTip1.SetToolTip(cmbPaymentMethod, "Select the payment method");
            // 
            // lblPaymentMethod
            // 
            lblPaymentMethod.AutoSize = true;
            lblPaymentMethod.Location = new Point(370, 42);
            lblPaymentMethod.Margin = new Padding(4, 0, 4, 0);
            lblPaymentMethod.Name = "lblPaymentMethod";
            lblPaymentMethod.Size = new Size(101, 15);
            lblPaymentMethod.TabIndex = 0;
            lblPaymentMethod.Text = "Payment Method:";
            // 
            // nudAmount
            // 
            nudAmount.DecimalPlaces = 2;
            nudAmount.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            nudAmount.ForeColor = Color.Green;
            nudAmount.Location = new Point(235, 38);
            nudAmount.Margin = new Padding(4);
            nudAmount.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            nudAmount.Name = "nudAmount";
            nudAmount.Size = new Size(120, 25);
            nudAmount.TabIndex = 1;
            nudAmount.TextAlign = HorizontalAlignment.Right;
            nudAmount.ThousandsSeparator = true;
            toolTip1.SetToolTip(nudAmount, "Enter the advance amount");
            // 
            // lblAmount
            // 
            lblAmount.AutoSize = true;
            lblAmount.Location = new Point(175, 42);
            lblAmount.Margin = new Padding(4, 0, 4, 0);
            lblAmount.Name = "lblAmount";
            lblAmount.Size = new Size(54, 15);
            lblAmount.TabIndex = 0;
            lblAmount.Text = "Amount:";
            // 
            // txtPaymentDate
            // 
            txtPaymentDate.Location = new Point(102, 38);
            txtPaymentDate.Margin = new Padding(4);
            txtPaymentDate.MaxLength = 10;
            txtPaymentDate.Name = "txtPaymentDate";
            txtPaymentDate.PlaceholderText = "dd-mm-yyyy";
            txtPaymentDate.Size = new Size(120, 23);
            txtPaymentDate.TabIndex = 0;
            toolTip1.SetToolTip(txtPaymentDate, "Enter the payment date (dd-mm-yyyy format)");
            // 
            // lblPaymentDate
            // 
            lblPaymentDate.AutoSize = true;
            lblPaymentDate.Location = new Point(15, 42);
            lblPaymentDate.Margin = new Padding(4, 0, 4, 0);
            lblPaymentDate.Name = "lblPaymentDate";
            lblPaymentDate.Size = new Size(85, 15);
            lblPaymentDate.TabIndex = 0;
            lblPaymentDate.Text = "Payment Date:";
            // 
            // pnlGrid
            // 
            pnlGrid.Controls.Add(dgvAdvancePayments);
            pnlGrid.Controls.Add(lblTotalAdvances);
            pnlGrid.Dock = DockStyle.Fill;
            pnlGrid.Location = new Point(11, 221);
            pnlGrid.Margin = new Padding(4);
            pnlGrid.Name = "pnlGrid";
            pnlGrid.Size = new Size(1178, 428);
            pnlGrid.TabIndex = 2;
            // 
            // dgvAdvancePayments
            // 
            dgvAdvancePayments.AllowUserToAddRows = false;
            dgvAdvancePayments.AllowUserToDeleteRows = false;
            dgvAdvancePayments.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvAdvancePayments.Dock = DockStyle.Fill;
            dgvAdvancePayments.Location = new Point(0, 0);
            dgvAdvancePayments.Margin = new Padding(4);
            dgvAdvancePayments.Name = "dgvAdvancePayments";
            dgvAdvancePayments.ReadOnly = true;
            dgvAdvancePayments.RowHeadersWidth = 51;
            dgvAdvancePayments.Size = new Size(1178, 393);
            dgvAdvancePayments.TabIndex = 0;
            // 
            // lblTotalAdvances
            // 
            lblTotalAdvances.BackColor = Color.FromArgb(255, 255, 192);
            lblTotalAdvances.BorderStyle = BorderStyle.FixedSingle;
            lblTotalAdvances.Dock = DockStyle.Bottom;
            lblTotalAdvances.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold);
            lblTotalAdvances.ForeColor = Color.DarkGreen;
            lblTotalAdvances.Location = new Point(0, 393);
            lblTotalAdvances.Margin = new Padding(4, 0, 4, 0);
            lblTotalAdvances.Name = "lblTotalAdvances";
            lblTotalAdvances.Padding = new Padding(10, 5, 10, 5);
            lblTotalAdvances.Size = new Size(1178, 35);
            lblTotalAdvances.TabIndex = 1;
            lblTotalAdvances.Text = "Total Advances: ₹0.00";
            lblTotalAdvances.TextAlign = ContentAlignment.MiddleRight;
            // 
            // AdvancePaymentEntryControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlGrid);
            Controls.Add(pnlEntry);
            Controls.Add(pnlTop);
            Margin = new Padding(4);
            Name = "AdvancePaymentEntryControl";
            Padding = new Padding(11);
            Size = new Size(1200, 660);
            Load += AdvancePaymentEntryControl_Load;
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            pnlEntry.ResumeLayout(false);
            pnlEntry.PerformLayout();
            pnlChequeDetails.ResumeLayout(false);
            pnlChequeDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudAmount).EndInit();
            pnlGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvAdvancePayments).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlTop;
        private ComboBox cmbBroker;
        private Label lblBroker;
        private ComboBox cmbParty;
        private Label lblParty;
        private Panel pnlEntry;
        private Button btnDelete;
        private Button btnClear;
        private Button btnSave;
        private TextBox txtReference;
        private Label lblReference;
        private ComboBox cmbPaymentMethod;
        private Label lblPaymentMethod;
        private NumericUpDown nudAmount;
        private Label lblAmount;
        private TextBox txtPaymentDate;
        private Label lblPaymentDate;
        private Panel pnlGrid;
        private DataGridView dgvAdvancePayments;
        private Label lblTotalAdvances;
        private Panel pnlChequeDetails;
        private TextBox txtChequeAmountFirm2;
        private Label lblChequeAmountFirm2;
        private TextBox txtChequeAmountFirm1;
        private Label lblChequeAmountFirm1;
        private ToolTip toolTip1;
    }
}