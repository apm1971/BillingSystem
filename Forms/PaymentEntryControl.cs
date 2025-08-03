using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class PaymentEntryControl : UserControl
    {
        public event EventHandler? CloseRequested;
        
        private List<Party> _parties = new List<Party>();
        private List<Broker> _brokers = new List<Broker>();
        private List<BillViewModel> _outstandingBills = new List<BillViewModel>();

        public PaymentEntryControl()
        {
            InitializeComponent();
        }

        private void PaymentEntryControl_Load(object? sender, EventArgs e)
        {
            SetupDataGridView(); // Setup grid style and columns first
            LoadInitialData();
            SetupEventHandlers();
            ClearForm();
        }

        #region Initial Setup

        private void SetupDataGridView()
        {
            dgvOutstandingBills.AutoGenerateColumns = false;
            dgvOutstandingBills.AllowUserToAddRows = false;
            dgvOutstandingBills.AllowUserToDeleteRows = false;
            
            dgvOutstandingBills.ReadOnly = false; 
            
            dgvOutstandingBills.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOutstandingBills.MultiSelect = true; 
            dgvOutstandingBills.RowHeadersVisible = false;
            dgvOutstandingBills.BackgroundColor = Color.White;
            dgvOutstandingBills.DefaultCellStyle.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular);
            dgvOutstandingBills.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvOutstandingBills.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvOutstandingBills.RowTemplate.Height = 28;

            dgvOutstandingBills.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            dgvOutstandingBills.DefaultCellStyle.SelectionForeColor = Color.White;

            dgvOutstandingBills.Columns.Clear();

            // Define Columns
            dgvOutstandingBills.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BillID", HeaderText = "ID", Visible = false, ReadOnly = true });
            dgvOutstandingBills.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BillNo", HeaderText = "Bill No.", Width = 120, ReadOnly = true });
            dgvOutstandingBills.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PartyName", HeaderText = "Party Name", Width = 150, ReadOnly = true });
            dgvOutstandingBills.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BillDate", HeaderText = "Bill Date", DefaultCellStyle = new DataGridViewCellStyle { Format = "dd-MMM-yyyy" }, Width = 120, ReadOnly = true });
            dgvOutstandingBills.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalAmount", HeaderText = "Total Amount", DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }, Width = 150, ReadOnly = true });
            dgvOutstandingBills.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BalanceDue", HeaderText = "Balance Due", Name="BalanceDue", DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9.75F, FontStyle.Bold), ForeColor = Color.Red }, Width = 150, ReadOnly = true });
            
            dgvOutstandingBills.Columns.Add(new DataGridViewTextBoxColumn { 
                DataPropertyName = "PaymentAllocation", 
                HeaderText = "Allocated Payment", 
                Name = "PaymentAllocation", 
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, BackColor = Color.LightYellow },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = false 
            });
        }

        private void LoadInitialData()
        {
            try
            {
                _parties = PartyService.GetAllParties();
                _brokers = BrokerService.GetAllBrokers();

                cmbParty.DataSource = _parties;
                cmbParty.DisplayMember = "PartyName";
                cmbParty.ValueMember = "PartyID";
                
                cmbBroker.DataSource = _brokers;
                cmbBroker.DisplayMember = "BrokerName";
                cmbBroker.ValueMember = "BrokerID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading initial data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupEventHandlers()
        {
            cmbParty.SelectedIndexChanged += CmbParty_SelectedIndexChanged;
            cmbBroker.SelectedIndexChanged += CmbBroker_SelectedIndexChanged;
            btnCalculate.Click += BtnCalculate_Click;
            btnAutoAllocate.Click += BtnAutoAllocate_Click;
            btnSave.Click += BtnSave_Click;
            btnClear.Click += BtnClear_Click;
            btnClose.Click += (s, e) => CloseRequested?.Invoke(this, EventArgs.Empty);
            
            dgvOutstandingBills.CellValueChanged += DgvOutstandingBills_CellValueChanged;
            txtPaymentDate.TextChanged += TxtPaymentDate_TextChanged;
        }

        private void ClearForm()
        {
            cmbParty.SelectedIndex = -1;
            cmbBroker.SelectedIndex = -1;
            dgvOutstandingBills.DataSource = null;
            _outstandingBills = new List<BillViewModel>();
            
            txtCreditDays.Text = SettingsService.GetDefaultCreditDays().ToString();
            txtDiscountRate.Text = SettingsService.GetDefaultDiscountRate().ToString("F2");
            txtInterestRate.Text = SettingsService.GetDefaultInterestRate().ToString("F2");
            
            lblDiscountValue.Text = "Earned Discount: ₹0.00";
            lblInterestValue.Text = "Accrued Interest: ₹0.00";
            lblFinalAmount.Text = "Final Amount Due: ₹0.00";

            txtPaymentAmount.Text = "0.00";
            txtPaymentDate.Text = DateTime.Now.ToString("dd-MM-yyyy");
            cmbPaymentMethod.SelectedIndex = 0;
            txtReference.Clear();
            
            cmbParty.Focus();
        }

        #endregion

        #region Data Loading & UI Updates

        private void CmbParty_SelectedIndexChanged(object? sender, EventArgs e)
        {
            LoadBillsBasedOnSelection();
        }

        private void CmbBroker_SelectedIndexChanged(object? sender, EventArgs e)
        {
            LoadBillsBasedOnSelection();
        }

        private void LoadBillsBasedOnSelection()
        {
            int? partyId = cmbParty.SelectedValue as int?;
            int? brokerId = cmbBroker.SelectedValue as int?;

            if (partyId.HasValue && partyId.Value > 0)
            {
                LoadOutstandingBills(partyId.Value, brokerId);
            }
            else if (brokerId.HasValue && brokerId.Value > 0)
            {
                LoadOutstandingBillsByBroker(brokerId.Value);
            }
            else
            {
                dgvOutstandingBills.DataSource = null;
                _outstandingBills.Clear();
            }
        }

        private void LoadOutstandingBills(int partyId, int? brokerId = null)
        {
            try
            {
                var bills = BillService.GetAllBillsForParty(partyId);
                
                // Filter by broker if specified
                if (brokerId.HasValue && brokerId.Value > 0)
                {
                    bills = bills.Where(b => b.BrokerID == brokerId.Value).ToList();
                }
                
                _outstandingBills = bills
                    .Select(b => new BillViewModel
                    {
                        BillID = b.BillID,
                        BillNo = b.BillNo,
                        PartyName = b.PartyName,
                        BillDate = b.BillDate,
                        OriginalAmount = b.OriginalAmount,
                        AdditionalCharges = b.AdditionalCharges,
                        BalanceDue = BillService.GetBillBalance(b.BillID),
                        PaymentAllocation = 0
                    })
                    .Where(b => b.BalanceDue > 0.01m)
                    .OrderBy(b => b.BillDate)
                    .ToList();

                dgvOutstandingBills.DataSource = _outstandingBills;
                ResetGridStyles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading outstanding bills: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadOutstandingBillsByBroker(int brokerId)
        {
            try
            {
                var allBills = BillService.GetAllBills();
                var bills = allBills.Where(b => b.BrokerID == brokerId).ToList();
                
                _outstandingBills = bills
                    .Select(b => new BillViewModel
                    {
                        BillID = b.BillID,
                        BillNo = b.BillNo,
                        PartyName = b.PartyName,
                        BillDate = b.BillDate,
                        OriginalAmount = b.OriginalAmount,
                        AdditionalCharges = b.AdditionalCharges,
                        BalanceDue = BillService.GetBillBalance(b.BillID),
                        PaymentAllocation = 0
                    })
                    .Where(b => b.BalanceDue > 0.01m)
                    .OrderBy(b => b.BillDate)
                    .ToList();

                dgvOutstandingBills.DataSource = _outstandingBills;
                ResetGridStyles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading outstanding bills by broker: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Core Logic

        private void BtnCalculate_Click(object? sender, EventArgs e)
        {
            if (!ValidateTerms(out int creditDays, out decimal discountRate, out decimal interestRate)) return;
            if (!DateTime.TryParseExact(txtPaymentDate.Text, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime paymentDate))
            {
                 MessageBox.Show("Please enter a valid payment date in dd-mm-yyyy format.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                 return;
            }

            decimal totalDiscount = 0;
            decimal totalInterest = 0;
            decimal totalAmountDue = 0;

            var billsToProcess = GetSelectedBillsFromGrid();
            if (!billsToProcess.Any())
            {
                MessageBox.Show("Please select one or more bills to reconcile, or check 'Apply to all'.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            dgvOutstandingBills.CellValueChanged -= DgvOutstandingBills_CellValueChanged;
            ResetGridStyles();

            foreach (var billVm in billsToProcess)
            {
                var fullBill = BillService.GetBillByID(billVm.BillID);
                if (fullBill == null) continue;

                var (interest, discount, finalAmount) = LedgerService.CalculateFinalSettlement(fullBill, creditDays, interestRate, discountRate, paymentDate);
                totalInterest += interest;
                totalDiscount += discount;
                totalAmountDue += finalAmount;

                billVm.PaymentAllocation = finalAmount;
                
                foreach(DataGridViewRow row in dgvOutstandingBills.Rows)
                {
                    if((row.DataBoundItem as BillViewModel)?.BillID == billVm.BillID)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightGreen;
                        break;
                    }
                }
            }
            
            dgvOutstandingBills.CellValueChanged += DgvOutstandingBills_CellValueChanged;

            dgvOutstandingBills.Refresh();
            UpdateTotalPaymentFromGrid();

            lblDiscountValue.Text = $"Earned Discount: ₹{totalDiscount:N2}";
            lblInterestValue.Text = $"Accrued Interest: ₹{totalInterest:N2}";
            lblFinalAmount.Text = $"Final Amount Due: ₹{totalAmountDue:N2}";
        }

        private void BtnAutoAllocate_Click(object? sender, EventArgs e)
        {
            if (!ValidateTerms(out int creditDays, out decimal discountRate, out decimal interestRate)) return;
            if (!DateTime.TryParseExact(txtPaymentDate.Text, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime paymentDate))
            {
                 MessageBox.Show("Please enter a valid payment date in dd-mm-yyyy format.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                 return;
            }
            if (!decimal.TryParse(txtPaymentAmount.Text, out decimal paymentAmount) || paymentAmount <= 0)
            {
                MessageBox.Show("Please enter a valid payment amount before auto-allocating.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPaymentAmount.Focus();
                return;
            }

            var billsToProcess = GetSelectedBillsFromGrid();
            if (!billsToProcess.Any())
            {
                MessageBox.Show("Please select bills to allocate payment to, or check 'Apply to all'.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dgvOutstandingBills.CellValueChanged -= DgvOutstandingBills_CellValueChanged;
            ResetGridStyles();

            decimal remainingAmount = paymentAmount;
            foreach (var billVm in billsToProcess.OrderBy(b => b.BillDate)) // Ensure FIFO on selected bills
            {
                if (remainingAmount <= 0)
                {
                    billVm.PaymentAllocation = 0;
                    continue;
                }

                var fullBill = BillService.GetBillByID(billVm.BillID);
                if (fullBill == null) continue;

                // Calculate the true amount needed to settle this bill
                var (_, _, settlementAmount) = LedgerService.CalculateFinalSettlement(fullBill, creditDays, interestRate, discountRate, paymentDate);

                decimal amountToAllocate = Math.Min(remainingAmount, settlementAmount);
                billVm.PaymentAllocation = amountToAllocate;
                remainingAmount -= amountToAllocate;
            }

            dgvOutstandingBills.CellValueChanged += DgvOutstandingBills_CellValueChanged;
            dgvOutstandingBills.Refresh();
            UpdateTotalPaymentFromGrid();
        }

        private void DgvOutstandingBills_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvOutstandingBills.Columns[e.ColumnIndex].Name == "PaymentAllocation")
            {
                UpdateTotalPaymentFromGrid();
                ResetGridStyles();
            }
        }

        private void UpdateTotalPaymentFromGrid()
        {
            decimal totalAllocated = _outstandingBills.Sum(b => b.PaymentAllocation);
            txtPaymentAmount.Text = totalAllocated.ToString("F2");
        }
        
        private void ResetGridStyles()
        {
            foreach (DataGridViewRow row in dgvOutstandingBills.Rows)
            {
                row.DefaultCellStyle.BackColor = dgvOutstandingBills.DefaultCellStyle.BackColor;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (!ValidatePayment(out decimal totalPaymentAmount, out DateTime paymentDate)) return;
            if (!ValidateTerms(out int creditDays, out decimal discountRate, out decimal interestRate)) return;

            var paymentsToSave = _outstandingBills.Where(b => b.PaymentAllocation > 0).ToList();
            if (!paymentsToSave.Any())
            {
                MessageBox.Show("No payments have been allocated to any bills.", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var conn = DatabaseManager.GetConnection())
            {
                conn.Open();
                var dbTransaction = conn.BeginTransaction();
                try
                {
                    // Determine the party ID for the payment master record
                    int partyId;
                    if (cmbParty.SelectedValue != null && (int)cmbParty.SelectedValue > 0)
                    {
                        // Party is directly selected
                        partyId = (int)cmbParty.SelectedValue;
                    }
                    else
                    {
                        // Only broker is selected, get party from the first bill being paid
                        var firstBill = BillService.GetBillByID(paymentsToSave.First().BillID);
                        if (firstBill == null)
                        {
                            MessageBox.Show("Unable to determine party for payment. Please select a party.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        partyId = firstBill.PartyID;
                    }

                    // Create a single master record for this payment event
                    var paymentMaster = new PaymentMaster
                    {
                        PartyID = partyId,
                        PaymentDate = paymentDate,
                        TotalAmountPaid = totalPaymentAmount,
                        PaymentMethod = cmbPaymentMethod.SelectedItem?.ToString() ?? "Cash",
                        Reference = txtReference.Text,
                        CompanyID = 1 // Replace with Program.ActiveCompany.CompanyID
                    };
                    int paymentId = PaymentService.SavePaymentMaster(paymentMaster, conn, dbTransaction);


                    foreach (var billVm in paymentsToSave)
                    {
                        var fullBill = BillService.GetBillByID(billVm.BillID);
                        if (fullBill == null) continue;
                        
                        var (interest, discount, finalAmount) = LedgerService.CalculateFinalSettlement(fullBill, creditDays, interestRate, discountRate, paymentDate);
                        bool isFinalSettlement = billVm.PaymentAllocation >= (billVm.BalanceDue + interest - discount);

                        if (isFinalSettlement)
                        {
                            if (interest > 0)
                            {
                                var interestTx = new Transaction {
                                    PaymentID = paymentId, PartyID = fullBill.PartyID, BillID = fullBill.BillID, TransactionDate = paymentDate,
                                    TransactionType = "Interest", Description = $"Interest on Bill No: {fullBill.BillNo}",
                                    DebitAmount = interest, UserID = 1, CompanyID = 1
                                };
                                LedgerService.AddTransaction(interestTx, conn, dbTransaction);
                            }

                            if (discount > 0)
                            {
                                var discountTx = new Transaction {
                                    PaymentID = paymentId, PartyID = fullBill.PartyID, BillID = fullBill.BillID, TransactionDate = paymentDate,
                                    TransactionType = "Discount", Description = $"Discount on Bill No: {fullBill.BillNo}",
                                    CreditAmount = discount, UserID = 1, CompanyID = 1
                                };
                                LedgerService.AddTransaction(discountTx, conn, dbTransaction);
                            }
                        }

                        var paymentTx = new Transaction
                        {
                            PartyID = fullBill.PartyID,
                            BillID = fullBill.BillID,
                            PaymentID = paymentId,
                            TransactionDate = paymentDate,
                            TransactionType = "Payment",
                            Description = $"Payment against Bill No: {fullBill.BillNo}",
                            CreditAmount = billVm.PaymentAllocation,
                            PaymentMethod = cmbPaymentMethod.SelectedItem?.ToString() ?? "Cash",
                            Reference = txtReference.Text,
                            UserID = 1, // Replace with Program.CurrentUser.UserID
                            CompanyID = 1 // Replace with Program.ActiveCompany.CompanyID
                        };
                        LedgerService.AddTransaction(paymentTx, conn, dbTransaction);
                    }
                    
                    // First commit the ledger transactions
                    dbTransaction.Commit();
                    
                    // Now update bill statuses in a new transaction
                    UpdateBillStatuses(paymentsToSave);
                    
                    MessageBox.Show("Payment(s) saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    MessageBox.Show($"Failed to save payment: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            ClearForm();
        }

        #endregion

        #region Bill Status Update

        /// <summary>
        /// Updates the status of bills after payment transactions are saved
        /// </summary>
        private void UpdateBillStatuses(List<BillViewModel> paidBills)
        {
            try
            {
                using (var conn = DatabaseManager.GetConnection())
                {
                    conn.Open();
                    var trans = conn.BeginTransaction();
                    
                    try
                    {
                        foreach (var billVm in paidBills)
                        {
                            // Calculate current balance after payment (now ledger transactions are committed)
                            decimal dueAmount = LedgerService.GetDueAmount(billVm.BillID);
                            
                            // Determine new status based on balance
                            string newStatus;
                            if (dueAmount <= 0)
                            {
                                newStatus = "Paid";
                            }
                            else if (dueAmount >= billVm.TotalAmount)
                            {
                                newStatus = "Unpaid";
                            }
                            else
                            {
                                newStatus = "Partial";
                            }
                            
                            // Update the bill status in the database
                            string updateSql = "UPDATE BillMaster SET Status = ? WHERE BillID = ?";
                            var statusParam = new OleDbParameter("Status", newStatus);
                            var billIdParam = new OleDbParameter("BillID", billVm.BillID);
                            
                            using (var cmd = new OleDbCommand(updateSql, conn, trans))
                            {
                                cmd.Parameters.Add(statusParam);
                                cmd.Parameters.Add(billIdParam);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't throw - we don't want to rollback the payment if status update fails
                System.Diagnostics.Debug.WriteLine($"Error updating bill statuses: {ex.Message}");
            }
        }

        #endregion

        #region Validation & Helpers
        
        private List<BillViewModel> GetSelectedBillsFromGrid()
        {
            if (chkApplyToAll.Checked)
            {
                return _outstandingBills.ToList();
            }

            var selectedBills = new List<BillViewModel>();
            foreach (DataGridViewRow row in dgvOutstandingBills.SelectedRows)
            {
                if (row.DataBoundItem is BillViewModel billVm)
                {
                    selectedBills.Add(billVm);
                }
            }
            return selectedBills;
        }
        
        private void TxtPaymentDate_TextChanged(object? sender, EventArgs e)
        {
            if (txtPaymentDate.Text.Length == 2 && !txtPaymentDate.Text.Contains("-"))
            {
                txtPaymentDate.Text += "-";
                txtPaymentDate.SelectionStart = txtPaymentDate.Text.Length;
            }
            else if (txtPaymentDate.Text.Length == 5 && txtPaymentDate.Text.Count(c => c == '-') == 1)
            {
                txtPaymentDate.Text += "-";
                txtPaymentDate.SelectionStart = txtPaymentDate.Text.Length;
            }
        }

        private bool ValidateTerms(out int creditDays, out decimal discountRate, out decimal interestRate)
        {
            creditDays = 0;
            discountRate = 0;
            interestRate = 0;
            bool valid = int.TryParse(txtCreditDays.Text, out creditDays) &&
                         decimal.TryParse(txtDiscountRate.Text, out discountRate) &&
                         decimal.TryParse(txtInterestRate.Text, out interestRate);
            if (!valid)
            {
                MessageBox.Show("Please enter valid numeric values for all reconciliation terms.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return valid;
        }

        private bool ValidatePayment(out decimal paymentAmount, out DateTime paymentDate)
        {
            paymentAmount = 0;
            paymentDate = DateTime.MinValue;

            // Check if either party or broker is selected
            bool partySelected = cmbParty.SelectedValue != null && (int)cmbParty.SelectedValue > 0;
            bool brokerSelected = cmbBroker.SelectedValue != null && (int)cmbBroker.SelectedValue > 0;

            if (!partySelected && !brokerSelected)
            {
                MessageBox.Show("Please select either a party or a broker.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!decimal.TryParse(txtPaymentAmount.Text, out paymentAmount) || paymentAmount < 0)
            {
                MessageBox.Show("Payment amount is invalid.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!DateTime.TryParseExact(txtPaymentDate.Text, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out paymentDate))
            {
                MessageBox.Show("Please enter a valid payment date in dd-mm-yyyy format.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPaymentDate.Focus();
                return false;
            }
            return true;
        }

        #endregion
    }
}
