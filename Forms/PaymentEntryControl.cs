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
            dgvOutstandingBills.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BrokerName", HeaderText = "Broker Name", Width = 120, ReadOnly = true });
            dgvOutstandingBills.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BillDate", HeaderText = "Bill Date", DefaultCellStyle = new DataGridViewCellStyle { Format = "dd-MMM-yyyy" }, Width = 120, ReadOnly = true });
            dgvOutstandingBills.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalAmount", HeaderText = "Total Amount", DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }, Width = 150, ReadOnly = true });
            dgvOutstandingBills.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BalanceDue", HeaderText = "Balance Due", Name = "BalanceDue", DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9.75F, FontStyle.Bold), ForeColor = Color.Red }, Width = 150, ReadOnly = true });

            dgvOutstandingBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PaymentAllocation",
                HeaderText = "Allocated Payment",
                Name = "PaymentAllocation",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, BackColor = Color.LightYellow },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = false
            });

            // Add Cheque Amount Firm1 column
            dgvOutstandingBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ChequeAmountFirm1",
                HeaderText = "Cheque Amt Firm1",
                Name = "ChequeAmountFirm1",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, BackColor = Color.LightCyan },
                Width = 120,
                ReadOnly = true
            });

            // Add Cheque Amount Firm2 column
            dgvOutstandingBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ChequeAmountFirm2",
                HeaderText = "Cheque Amt Firm2",
                Name = "ChequeAmountFirm2",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, BackColor = Color.LightCyan },
                Width = 120,
                ReadOnly = true
            });
        }

        private void LoadInitialData()
        {
            try
            {
                _parties = PartyService.GetAllParties();
                _brokers = BrokerService.GetAllBrokers();

                // Set up autocomplete for comboboxes
                SetupPartyComboBox();
                SetupBrokerComboBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading initial data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupPartyComboBox()
        {
            // Temporarily remove event handler
            cmbParty.SelectedIndexChanged -= CmbParty_SelectedIndexChanged;
            
            // Configure combobox for optimal performance
            cmbParty.BeginUpdate();
            cmbParty.DropDownStyle = ComboBoxStyle.DropDown;
            
            // Use a binding source for better performance
            var partyBindingSource = new BindingSource();
            partyBindingSource.DataSource = _parties;
            
            cmbParty.DataSource = partyBindingSource;
            cmbParty.DisplayMember = "PartyName";
            cmbParty.ValueMember = "PartyID";
            
            // Set up autocomplete
            cmbParty.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbParty.AutoCompleteSource = AutoCompleteSource.ListItems;
            
            cmbParty.EndUpdate();
            
            // Restore event handler
            cmbParty.SelectedIndexChanged += CmbParty_SelectedIndexChanged;
        }

        private void SetupBrokerComboBox()
        {
            // Temporarily remove event handler
            cmbBroker.SelectedIndexChanged -= CmbBroker_SelectedIndexChanged;
            
            // Configure combobox for optimal performance
            cmbBroker.BeginUpdate();
            cmbBroker.DropDownStyle = ComboBoxStyle.DropDown;
            
            // Use a binding source for better performance
            var brokerBindingSource = new BindingSource();
            brokerBindingSource.DataSource = _brokers;
            
            cmbBroker.DataSource = brokerBindingSource;
            cmbBroker.DisplayMember = "BrokerName";
            cmbBroker.ValueMember = "BrokerID";
            
            // Set up autocomplete
            cmbBroker.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbBroker.AutoCompleteSource = AutoCompleteSource.ListItems;
            
            cmbBroker.EndUpdate();
            
            // Restore event handler
            cmbBroker.SelectedIndexChanged += CmbBroker_SelectedIndexChanged;
        }

        private void SetupEventHandlers()
        {
            cmbParty.SelectedIndexChanged += CmbParty_SelectedIndexChanged;
            cmbBroker.SelectedIndexChanged += CmbBroker_SelectedIndexChanged;
            btnCalculate.Click += BtnCalculate_Click;
            btnAutoAllocate.Click += BtnAutoAllocate_Click;
            btnSave.Click += BtnSave_Click;
            btnClear.Click += BtnClear_Click;
            // Note: btnClose doesn't exist in the designer, removed the event handler

            dgvOutstandingBills.CellValueChanged += DgvOutstandingBills_CellValueChanged;
            txtPaymentDate.TextChanged += TxtPaymentDate_TextChanged;
            cmbPaymentMethod.SelectedIndexChanged += CmbPaymentMethod_SelectedIndexChanged;
            
            // Add event handlers for cheque amount fields
            txtChequeAmountFirm1.TextChanged += TxtChequeAmount_TextChanged;
            txtChequeAmountFirm2.TextChanged += TxtChequeAmount_TextChanged;
        }

        private void ClearForm()
        {
            cmbParty.SelectedIndex = -1;
            cmbBroker.SelectedIndex = -1;
            dgvOutstandingBills.DataSource = null;
            _outstandingBills = new List<BillViewModel>();

            // Set default values from settings
            txtInterestDays.Text = SettingsService.GetDefaultInterestDays().ToString();
            txtDiscountDays.Text = SettingsService.GetDefaultDiscountDays().ToString();
            txtDiscountRate.Text = SettingsService.GetDefaultDiscountRate().ToString("F2");
            txtInterestRate.Text = SettingsService.GetDefaultInterestRate().ToString("F2");
            txtBrokerageRate.Text = SettingsService.GetDefaultBrokerageRate().ToString("F2");

            lblDiscountValue.Text = "Discount: ₹0.00";
            lblInterestValue.Text = "Interest: ₹0.00";
            lblBrokerageValue.Text = "Brokerage: ₹0.00";
            lblFinalAmount.Text = "Amount Due: ₹0.00";

            txtPaymentAmount.Text = "0.00";
            txtPaymentDate.Text = DateTime.Now.ToString("dd-MM-yyyy");
            cmbPaymentMethod.SelectedIndex = 0;
            txtReference.Clear();
            
            // Reset cheque amount fields
            txtChequeAmountFirm1.Text = "0.00";
            txtChequeAmountFirm2.Text = "0.00";
            pnlChequeDetails.Visible = false;

            // Make all fields editable by default
            SetFieldsEditable(true);

            cmbParty.Focus();
        }

        #endregion

        #region Data Loading & UI Updates

        private void CmbParty_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // When party changes, clear broker selection to avoid cascading events
            if (cmbBroker.SelectedIndex > 0)
            {
                cmbBroker.SelectedIndexChanged -= CmbBroker_SelectedIndexChanged;
                cmbBroker.SelectedIndex = 0;
                cmbBroker.SelectedIndexChanged += CmbBroker_SelectedIndexChanged;
            }
            
            // Use background worker for bill loading to prevent UI hanging
            LoadBillsBasedOnSelectionAsync();
            UpdateFieldsBasedOnSelection();
        }

        private void CmbBroker_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // When broker changes, clear party selection to avoid cascading events
            if (cmbParty.SelectedIndex > 0 && cmbBroker.SelectedIndex > 0)
            {
                cmbParty.SelectedIndexChanged -= CmbParty_SelectedIndexChanged;
                cmbParty.SelectedIndex = 0;
                cmbParty.SelectedIndexChanged += CmbParty_SelectedIndexChanged;
            }
            
            // Use background worker for bill loading to prevent UI hanging
            LoadBillsBasedOnSelectionAsync();
            UpdateFieldsBasedOnSelection();
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

        private void LoadBillsBasedOnSelectionAsync()
        {
            int? partyId = cmbParty.SelectedValue as int?;
            int? brokerId = cmbBroker.SelectedValue as int?;

            // Show loading cursor
            Cursor.Current = Cursors.WaitCursor;
            
            if (!partyId.HasValue || partyId.Value <= 0)
            {
                if (!brokerId.HasValue || brokerId.Value <= 0)
                {
                    // No selection - clear grid immediately
                    Cursor.Current = Cursors.Default;
                    dgvOutstandingBills.DataSource = null;
                    _outstandingBills.Clear();
                    return;
                }
            }

            var backgroundWorker = new System.ComponentModel.BackgroundWorker();
            backgroundWorker.DoWork += (sender, e) =>
            {
                try
                {
                    if (partyId.HasValue && partyId.Value > 0)
                    {
                        var bills = BillService.GetAllBillsForParty(partyId.Value);
                        if (brokerId.HasValue && brokerId.Value > 0)
                        {
                            bills = bills.Where(b => b.BrokerID == brokerId.Value).ToList();
                        }
                        e.Result = new { Success = true, Bills = bills, IsPartySelection = true, PartyId = partyId.Value, BrokerId = brokerId };
                    }
                    else if (brokerId.HasValue && brokerId.Value > 0)
                    {
                        var allBills = BillService.GetAllBills();
                        var bills = allBills.Where(b => b.BrokerID == brokerId.Value).ToList();
                        e.Result = new { Success = true, Bills = bills, IsPartySelection = false, BrokerId = brokerId.Value };
                    }
                    else
                    {
                        e.Result = new { Success = true, Bills = new List<Bill>(), IsPartySelection = false };
                    }
                }
                catch (Exception ex)
                {
                    e.Result = new { Success = false, Error = ex.Message };
                }
            };

            backgroundWorker.RunWorkerCompleted += (sender, e) =>
            {
                Cursor.Current = Cursors.Default;
                
                dynamic result = e.Result;
                if (result.Success)
                {
                    ProcessLoadedBills(result.Bills, result.IsPartySelection);
                }
                else
                {
                    MessageBox.Show($"Error loading bills: {result.Error}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    dgvOutstandingBills.DataSource = null;
                    _outstandingBills.Clear();
                }
            };

            backgroundWorker.RunWorkerAsync();
        }

        private void ProcessLoadedBills(List<Bill> bills, bool isPartySelection)
        {
            try
            {
                // Get all bill balances in one query rather than individually
                var allBalances = LedgerService.GetAllBillBalances();
                
                // Cache broker data to avoid repeated lookups
                var brokerCache = new Dictionary<int, string>();
                
                // Use efficient mapping with cached data
                _outstandingBills = bills
                    .Select(b =>
                    {
                        // Look up broker name from cache or add it
                        string brokerName = string.Empty;
                        if (b.BrokerID.HasValue && b.BrokerID.Value > 0)
                        {
                            if (!brokerCache.TryGetValue(b.BrokerID.Value, out brokerName))
                            {
                                var broker = _brokers.FirstOrDefault(br => br.BrokerID == b.BrokerID.Value);
                                brokerName = broker?.BrokerName ?? "Unknown Broker";
                                brokerCache[b.BrokerID.Value] = brokerName;
                            }
                        }
                        
                        // Get balance from the pre-fetched dictionary
                        decimal balance = allBalances.TryGetValue(b.BillID, out decimal dueAmount) ? dueAmount : 0;
                        
                        return new BillViewModel
                        {
                            BillID = b.BillID,
                            BillNo = b.BillNo,
                            PartyName = b.PartyName,
                            BrokerName = brokerName,
                            BillDate = b.BillDate,
                            OriginalAmount = b.OriginalAmount,
                            AdditionalCharges = b.AdditionalCharges,
                            BalanceDue = balance,
                            PaymentAllocation = 0,
                            ChequeAmountFirm1 = b.ChequeAmountFirm1,
                            ChequeAmountFirm2 = b.ChequeAmountFirm2
                        };
                    })
                    .Where(b => b.BalanceDue > 0.01m)
                    .OrderBy(b => b.BillDate)
                    .ToList();

                dgvOutstandingBills.DataSource = _outstandingBills;
                ResetGridStyles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing bills: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateFieldsBasedOnSelection()
        {
            int? partyId = cmbParty.SelectedValue as int?;
            int? brokerId = cmbBroker.SelectedValue as int?;

            if (partyId.HasValue && partyId.Value > 0 && (!brokerId.HasValue || brokerId.Value == 0))
            {
                // Only party selected - use settings defaults and make editable
                txtInterestDays.Text = SettingsService.GetDefaultInterestDays().ToString();
                txtDiscountDays.Text = SettingsService.GetDefaultDiscountDays().ToString();
                txtDiscountRate.Text = SettingsService.GetDefaultDiscountRate().ToString("F2");
                txtInterestRate.Text = SettingsService.GetDefaultInterestRate().ToString("F2");
                txtBrokerageRate.Text = SettingsService.GetDefaultBrokerageRate().ToString("F2");
                SetFieldsEditable(true);
            }
            else if (brokerId.HasValue && brokerId.Value > 0)
            {
                // Broker selected - load broker data and make editable
                // Use the cached broker list instead of making a database call
                var broker = _brokers.FirstOrDefault(b => b.BrokerID == brokerId.Value);
                
                if (broker != null)
                {
                    txtInterestDays.Text = broker.InterestDays.ToString();
                    txtDiscountDays.Text = broker.DiscountDays.ToString();
                    txtDiscountRate.Text = broker.DiscountRate.ToString("F2");
                    txtInterestRate.Text = broker.InterestRate.ToString("F2");
                    txtBrokerageRate.Text = broker.BrokerageRate.ToString("F2");
                    SetFieldsEditable(true);
                }
            }
            else
            {
                // Nothing selected - use defaults and make editable
                txtInterestDays.Text = SettingsService.GetDefaultInterestDays().ToString();
                txtDiscountDays.Text = SettingsService.GetDefaultDiscountDays().ToString();
                txtDiscountRate.Text = SettingsService.GetDefaultDiscountRate().ToString("F2");
                txtInterestRate.Text = SettingsService.GetDefaultInterestRate().ToString("F2");
                txtBrokerageRate.Text = SettingsService.GetDefaultBrokerageRate().ToString("F2");
                SetFieldsEditable(true);
            }
        }

        private void SetFieldsEditable(bool editable)
        {
            txtInterestDays.ReadOnly = !editable;
            txtDiscountDays.ReadOnly = !editable;
            txtDiscountRate.ReadOnly = !editable;
            txtInterestRate.ReadOnly = !editable;
            txtBrokerageRate.ReadOnly = !editable;
        }

        private void LoadOutstandingBills(int partyId, int? brokerId = null)
        {
            try
            {
                // Show loading indicator or cursor
                Cursor.Current = Cursors.WaitCursor;
                
                var bills = BillService.GetAllBillsForParty(partyId);

                // Filter by broker if specified
                if (brokerId.HasValue && brokerId.Value > 0)
                {
                    bills = bills.Where(b => b.BrokerID == brokerId.Value).ToList();
                }
                
                // Get all bill balances in one query rather than individually
                var allBalances = LedgerService.GetAllBillBalances();
                
                // Cache broker data to avoid repeated lookups
                var brokerCache = new Dictionary<int, string>();
                
                // Use efficient mapping with cached data
                _outstandingBills = bills
                    .Select(b =>
                    {
                        // Look up broker name from cache or add it
                        string brokerName = string.Empty;
                        if (b.BrokerID.HasValue && b.BrokerID.Value > 0)
                        {
                            if (!brokerCache.TryGetValue(b.BrokerID.Value, out brokerName))
                            {
                                var broker = _brokers.FirstOrDefault(br => br.BrokerID == b.BrokerID.Value);
                                brokerName = broker?.BrokerName ?? "Unknown Broker";
                                brokerCache[b.BrokerID.Value] = brokerName;
                            }
                        }
                        
                        // Get balance from the pre-fetched dictionary
                        decimal balance = allBalances.TryGetValue(b.BillID, out decimal dueAmount) ? dueAmount : 0;
                        
                        return new BillViewModel
                        {
                            BillID = b.BillID,
                            BillNo = b.BillNo,
                            PartyName = b.PartyName,
                            BrokerName = brokerName,
                            BillDate = b.BillDate,
                            OriginalAmount = b.OriginalAmount,
                            AdditionalCharges = b.AdditionalCharges,
                            BalanceDue = balance,
                            PaymentAllocation = 0,
                            ChequeAmountFirm1 = b.ChequeAmountFirm1,
                            ChequeAmountFirm2 = b.ChequeAmountFirm2
                        };
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
            finally
            {
                // Restore cursor
                Cursor.Current = Cursors.Default;
            }
        }

        private void LoadOutstandingBillsByBroker(int brokerId)
        {
            try
            {
                // Show loading indicator or cursor
                Cursor.Current = Cursors.WaitCursor;
                
                var allBills = BillService.GetAllBills();
                var bills = allBills.Where(b => b.BrokerID == brokerId).ToList();
                
                // Get all bill balances in one query rather than individually
                var allBalances = LedgerService.GetAllBillBalances();
                
                // Get broker name once from cache
                var broker = _brokers.FirstOrDefault(b => b.BrokerID == brokerId);
                string brokerName = broker?.BrokerName ?? "Unknown Broker";

                _outstandingBills = bills
                    .Select(b => new BillViewModel
                    {
                        BillID = b.BillID,
                        BillNo = b.BillNo,
                        PartyName = b.PartyName,
                        BrokerName = brokerName,
                        BillDate = b.BillDate,
                        OriginalAmount = b.OriginalAmount,
                        AdditionalCharges = b.AdditionalCharges,
                        BalanceDue = allBalances.TryGetValue(b.BillID, out decimal balance) ? balance : 0,
                        PaymentAllocation = 0,
                        ChequeAmountFirm1 = b.ChequeAmountFirm1,
                        ChequeAmountFirm2 = b.ChequeAmountFirm2
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
            finally
            {
                // Restore cursor
                Cursor.Current = Cursors.Default;
            }
        }

        #endregion

        #region Core Logic

        private void BtnCalculate_Click(object? sender, EventArgs e)
        {
            if (!ValidateTerms(out int interestDays, out int discountDays, out decimal discountRate, out decimal interestRate, out decimal brokerageRate)) return;
            if (!DateTime.TryParseExact(txtPaymentDate.Text, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime paymentDate))
            {
                MessageBox.Show("Please enter a valid payment date in dd-mm-yyyy format.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal totalDiscount = 0;
            decimal totalInterest = 0;
            decimal totalBrokerage = 0;
            decimal totalAmountDue = 0;

            var billsToProcess = GetSelectedBillsFromGrid();
            if (!billsToProcess.Any())
            {
                MessageBox.Show("Please select one or more bills to reconcile, or check 'Apply to all'.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dgvOutstandingBills.CellValueChanged -= DgvOutstandingBills_CellValueChanged;
            ResetGridStyles();
            // OPTIMIZATION: Get all bill details in one query instead of individual calls
            var billIds = billsToProcess.Select(b => b.BillID).ToList();
            var billDetails = BillService.GetBillsByIDs(billIds);

            foreach (var billVm in billsToProcess)
            {
                // Use cached bill data instead of individual database calls
                var fullBill = billDetails.FirstOrDefault(b => b.BillID == billVm.BillID);
                if (fullBill == null) 
                {
                    // Fallback to individual call if batch didn't work
                    fullBill = BillService.GetBillByID(billVm.BillID);
                    if (fullBill == null) continue;
                }

                var (interest, discount, finalAmount) = LedgerService.CalculateFinalSettlement(fullBill, interestDays, interestRate, discountDays, discountRate, paymentDate);
                var brokerageAmount = LedgerService.CalculateBrokerage(fullBill, brokerageRate);
                totalBrokerage += brokerageAmount;
                totalInterest += interest;
                totalDiscount += discount;
                totalAmountDue += finalAmount;
                finalAmount -= brokerageAmount;
                // Set the payment allocation to the calculated final amount
                billVm.PaymentAllocation = finalAmount;

                // Highlight the row
                foreach (DataGridViewRow row in dgvOutstandingBills.Rows)
                {
                    if ((row.DataBoundItem as BillViewModel)?.BillID == billVm.BillID)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightGreen;
                        break;
                    }
                }
            }

            // Calculate brokerage on total bill amount
            totalAmountDue -= totalBrokerage;

            dgvOutstandingBills.CellValueChanged += DgvOutstandingBills_CellValueChanged;

            dgvOutstandingBills.Refresh();

            // Update payment amount to include brokerage
            txtPaymentAmount.Text = Math.Round(totalAmountDue).ToString("F0");

            lblDiscountValue.Text = $"Earned Discount: ₹{Math.Round(totalDiscount):N0}";
            lblInterestValue.Text = $"Accrued Interest: ₹{Math.Round(totalInterest):N0}";
            lblBrokerageValue.Text = $"Brokerage: ₹{Math.Round(totalBrokerage):N0}";
            lblFinalAmount.Text = $"Amount Due: ₹{Math.Round(totalAmountDue):N0}";
        }

        private void BtnAutoAllocate_Click(object? sender, EventArgs e)
        {
            if (!ValidateTerms(out int interestDays, out int discountDays, out decimal discountRate, out decimal interestRate, out decimal brokerageRate)) return;
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

            // Calculate brokerage on total bill amount
            // decimal totalBillAmount = billsToProcess.Sum(b => b.BalanceDue);
            // decimal totalBrokerage = totalBillAmount * (brokerageRate / 100m);

            // Subtract brokerage from payment amount for allocation
            decimal remainingAmount = paymentAmount;

            // OPTIMIZATION: Get all bill details in one query instead of individual calls
            var billIds = billsToProcess.Select(b => b.BillID).ToList();
            var billDetails = BillService.GetBillsByIDs(billIds);

            foreach (var billVm in billsToProcess.OrderBy(b => b.BillDate)) // Ensure FIFO on selected bills
            {
                if (remainingAmount <= 0)
                {
                    billVm.PaymentAllocation = 0;
                    continue;
                }

                // Use cached bill data instead of individual database calls
                var fullBill = billDetails.FirstOrDefault(b => b.BillID == billVm.BillID);
                if (fullBill == null) 
                {
                    // Fallback to individual call if batch didn't work
                    fullBill = BillService.GetBillByID(billVm.BillID);
                    if (fullBill == null) continue;
                }

                // Calculate the true amount needed to settle this bill
                var (_, _, settlementAmount) = LedgerService.CalculateFinalSettlement(fullBill, interestDays, interestRate, discountDays, discountRate, paymentDate);
                var brokerageAmount = LedgerService.CalculateBrokerage(fullBill, brokerageRate);
                settlementAmount -= brokerageAmount;
                decimal amountToAllocate = Math.Min(remainingAmount, settlementAmount);
                billVm.PaymentAllocation = amountToAllocate;
                remainingAmount -= amountToAllocate;
            }

            dgvOutstandingBills.CellValueChanged += DgvOutstandingBills_CellValueChanged;
            dgvOutstandingBills.Refresh();
            UpdateTotalPaymentFromGrid();

            // Update summary to show brokerage
            // lblBrokerageValue.Text = $"Brokerage: ₹{totalBrokerage:N2}";
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
            txtPaymentAmount.Text = Math.Round(totalAllocated).ToString("F0");
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
            if (!ValidateTerms(out int interestDays, out int discountDays, out decimal discountRate, out decimal interestRate, out decimal brokerageRate)) return;

            var paymentsToSave = _outstandingBills.Where(b => b.PaymentAllocation > 0).ToList();
            if (!paymentsToSave.Any())
            {
                MessageBox.Show("No payments have been allocated to any bills.", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // CRITICAL FIX: Capture all UI values BEFORE starting background thread
            var paymentData = new PaymentSaveData
            {
                PaymentsToSave = paymentsToSave,
                TotalPaymentAmount = totalPaymentAmount,
                PaymentDate = paymentDate,
                InterestDays = interestDays,
                DiscountDays = discountDays,
                DiscountRate = discountRate,
                InterestRate = interestRate,
                BrokerageRate = brokerageRate,
                SelectedPartyId = cmbParty.SelectedValue as int?,
                SelectedBrokerId = cmbBroker.SelectedValue as int?,
                PaymentMethod = cmbPaymentMethod.SelectedItem?.ToString() ?? "Cash",
                Reference = txtReference.Text,
                ChequeAmountFirm1Text = txtChequeAmountFirm1.Text,
                ChequeAmountFirm2Text = txtChequeAmountFirm2.Text
            };

            // Show loading indicator and disable save button to prevent double-clicking
            Cursor.Current = Cursors.WaitCursor;
            btnSave.Enabled = false;
            btnSave.Text = "Saving...";
            
            // Use background worker for the save operation
            var backgroundWorker = new System.ComponentModel.BackgroundWorker();
            backgroundWorker.DoWork += (sender, e) =>
            {
                try
                {
                    int paymentId = SavePaymentInBackground(paymentData);
                    e.Result = new { Success = true, PaymentId = paymentId };
                }
                catch (Exception ex)
                {
                    e.Result = new { Success = false, Error = ex.Message };
                }
            };

            backgroundWorker.RunWorkerCompleted += (sender, e) =>
            {
                // Restore UI state
                Cursor.Current = Cursors.Default;
                btnSave.Enabled = true;
                btnSave.Text = "Save";
                
                dynamic result = e.Result;
                if (result.Success)
                {
                    MessageBox.Show("Payment(s) saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Show payment trace with print option
                    ShowPaymentTraceAfterSave(result.PaymentId);
                    
                    ClearForm();
                }
                else
                {
                    MessageBox.Show($"Failed to save payment: {result.Error}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            backgroundWorker.RunWorkerAsync();
        }

        // Helper class to pass data to background thread without UI access
        private class PaymentSaveData
        {
            public List<BillViewModel> PaymentsToSave { get; set; }
            public decimal TotalPaymentAmount { get; set; }
            public DateTime PaymentDate { get; set; }
            public int InterestDays { get; set; }
            public int DiscountDays { get; set; }
            public decimal DiscountRate { get; set; }
            public decimal InterestRate { get; set; }
            public decimal BrokerageRate { get; set; }
            public int? SelectedPartyId { get; set; }
            public int? SelectedBrokerId { get; set; }
            public string PaymentMethod { get; set; }
            public string Reference { get; set; }
            public string ChequeAmountFirm1Text { get; set; }
            public string ChequeAmountFirm2Text { get; set; }
        }

        private int SavePaymentInBackground(PaymentSaveData data)
        {
            using (var conn = DatabaseManager.GetConnection())
            {
                conn.Open();
                var dbTransaction = conn.BeginTransaction();
                try
                {
                    // Determine the party ID for the payment master record
                    int partyId;
                    if (data.SelectedPartyId.HasValue && data.SelectedPartyId.Value > 0)
                    {
                        // Party is directly selected
                        partyId = data.SelectedPartyId.Value;
                    }
                    else
                    {
                        // Only broker is selected, get party from the first bill being paid
                        var firstBill = BillService.GetBillByID(data.PaymentsToSave.First().BillID);
                        if (firstBill == null)
                        {
                            throw new Exception("Unable to determine party for payment. Please select a party.");
                        }
                        partyId = firstBill.PartyID;
                    }

                    // Determine the broker ID for the payment
                    int? brokerId = null;
                    if (data.SelectedBrokerId.HasValue && data.SelectedBrokerId.Value > 0)
                    {
                        brokerId = data.SelectedBrokerId.Value;
                    }
                    else if (data.PaymentsToSave.Any())
                    {
                        // Get broker ID from the first bill being paid
                        var firstBill = BillService.GetBillByID(data.PaymentsToSave.First().BillID);
                        if (firstBill?.BrokerID.HasValue == true)
                        {
                            brokerId = firstBill.BrokerID.Value;
                        }
                    }

                    // Create a single master record for this payment event
                    var paymentMaster = new PaymentMaster
                    {
                        PartyID = partyId,
                        BrokerID = brokerId,
                        PaymentDate = data.PaymentDate,
                        TotalAmountPaid = Math.Round(data.TotalPaymentAmount),
                        PaymentMethod = data.PaymentMethod,
                        Reference = data.Reference,
                        CompanyID = 1, // Replace with Program.ActiveCompany.CompanyID
                        ChequeAmountFirm1 = 0,
                        ChequeAmountFirm2 = 0
                    };
                    
                    // If payment method is Cheque, get the firm amounts
                    if (paymentMaster.PaymentMethod == "Cheque")
                    {
                        if (decimal.TryParse(data.ChequeAmountFirm1Text, out decimal firm1Amount))
                        {
                            paymentMaster.ChequeAmountFirm1 = Math.Round(firm1Amount);
                        }
                        
                        if (decimal.TryParse(data.ChequeAmountFirm2Text, out decimal firm2Amount))
                        {
                            paymentMaster.ChequeAmountFirm2 = Math.Round(firm2Amount);
                        }
                        
                        // Validate that the sum matches the total payment amount
                        if (Math.Abs((paymentMaster.ChequeAmountFirm1 + paymentMaster.ChequeAmountFirm2) - data.TotalPaymentAmount) > 0.01m)
                        {
                            throw new Exception("The sum of Firm 1 and Firm 2 amounts must equal the total payment amount.");
                        }
                    }
                    int paymentId = PaymentService.SavePaymentMaster(paymentMaster, conn, dbTransaction);

                    // OPTIMIZATION: Get all bill details in one query instead of individual calls
                    var billIds = data.PaymentsToSave.Select(b => b.BillID).ToList();
                    var billDetails = BillService.GetBillsByIDs(billIds);

                    foreach (var billVm in data.PaymentsToSave)
                    {
                        // Use cached bill data instead of individual database calls
                        var fullBill = billDetails.FirstOrDefault(b => b.BillID == billVm.BillID);
                        if (fullBill == null) 
                        {
                            // Fallback to individual call if batch didn't work
                            fullBill = BillService.GetBillByID(billVm.BillID);
                            if (fullBill == null) continue;
                        }

                        var brokerageAmount = LedgerService.CalculateBrokerage(fullBill, data.BrokerageRate);
                        var (interest, discount, finalAmount) = LedgerService.CalculateFinalSettlement(fullBill, data.InterestDays, data.InterestRate, data.DiscountDays, data.DiscountRate, data.PaymentDate);
                        bool isFinalSettlement = billVm.PaymentAllocation >= (billVm.BalanceDue + interest - discount - brokerageAmount);

                        if (isFinalSettlement)
                        {
                            if (interest > 0)
                            {
                                var interestTx = new Transaction
                                {
                                    PaymentID = paymentId,
                                    PartyID = fullBill.PartyID,
                                    BillID = fullBill.BillID,
                                    TransactionDate = data.PaymentDate,
                                    TransactionType = "Interest",
                                    Description = $"Interest on Bill No: {fullBill.BillNo}",
                                    DebitAmount = Math.Round(interest),
                                    UserID = 1,
                                    CompanyID = 1
                                };
                                LedgerService.AddTransaction(interestTx, conn, dbTransaction);
                            }

                            if (discount > 0)
                            {
                                var discountTx = new Transaction
                                {
                                    PaymentID = paymentId,
                                    PartyID = fullBill.PartyID,
                                    BillID = fullBill.BillID,
                                    TransactionDate = data.PaymentDate,
                                    TransactionType = "Discount",
                                    Description = $"Discount on Bill No: {fullBill.BillNo}",
                                    CreditAmount = Math.Round(discount),
                                    UserID = 1,
                                    CompanyID = 1
                                };
                                LedgerService.AddTransaction(discountTx, conn, dbTransaction);
                            }
                            if (brokerageAmount > 0)
                            {
                                var brokerageTx = new Transaction
                                {
                                    PaymentID = paymentId,
                                    PartyID = fullBill.PartyID,
                                    BillID = fullBill.BillID,
                                    TransactionDate = data.PaymentDate,
                                    TransactionType = "Brokerage",
                                    Description = $"Brokerage on Bill No: {fullBill.BillNo}",
                                    CreditAmount = Math.Round(brokerageAmount),
                                    UserID = 1,
                                    CompanyID = 1
                                };
                                LedgerService.AddTransaction(brokerageTx, conn, dbTransaction);
                            }
                        }

                        var paymentTx = new Transaction
                        {
                            PartyID = fullBill.PartyID,
                            BillID = fullBill.BillID,
                            PaymentID = paymentId,
                            TransactionDate = data.PaymentDate,
                            TransactionType = "Payment",
                            Description = $"Payment against Bill No: {fullBill.BillNo}",
                            CreditAmount = Math.Round(billVm.PaymentAllocation),
                            PaymentMethod = data.PaymentMethod,
                            Reference = data.Reference,
                            UserID = 1, // Replace with Program.CurrentUser.UserID
                            CompanyID = 1 // Replace with Program.ActiveCompany.CompanyID
                        };
                        LedgerService.AddTransaction(paymentTx, conn, dbTransaction);
                    }

                    // First commit the ledger transactions
                    dbTransaction.Commit();

                    // Now update bill statuses in a new transaction
                    UpdateBillStatuses(data.PaymentsToSave);

                    return paymentId; // Return the payment ID for success handling
                }
                catch (Exception ex)
                {
                    try
                    {
                        dbTransaction.Rollback();
                    }
                    catch
                    {
                        // Ignore rollback errors
                    }
                    throw; // Re-throw to be caught by background worker
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
        /// Updates the status of bills after payment transactions are saved - OPTIMIZED
        /// </summary>
        private void UpdateBillStatuses(List<BillViewModel> paidBills)
        {
            if (paidBills == null || !paidBills.Any())
                return;
                
            try
            {
                using (var conn = DatabaseManager.GetConnection())
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // OPTIMIZATION: Get all due amounts in one query instead of individual calls
                            var billIds = paidBills.Select(b => b.BillID).ToList();
                            var currentBalances = LedgerService.GetAllBillBalances();

                            foreach (var billVm in paidBills)
                            {
                                // Use pre-calculated balance from the batch query
                                decimal dueAmount = currentBalances.ContainsKey(billVm.BillID) ? currentBalances[billVm.BillID] : 0;

                                // Determine new status based on balance
                                string newStatus;
                                if (dueAmount <= 0)
                                {
                                    newStatus = "Paid";
                                }
                                else if (Math.Round(dueAmount) >= Math.Round(billVm.TotalAmount))
                                {
                                    newStatus = "Unpaid";
                                }
                                else
                                {
                                    newStatus = "Partial";
                                }

                                // Update the bill status in the database
                                string updateSql = "UPDATE BillMaster SET Status = ? WHERE BillID = ?";
                                using (var cmd = new OleDbCommand(updateSql, conn, trans))
                                {
                                    cmd.CommandTimeout = 30; // Add timeout
                                    cmd.Parameters.Add(new OleDbParameter("Status", newStatus));
                                    cmd.Parameters.Add(new OleDbParameter("BillID", billVm.BillID));
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                trans.Rollback();
                            }
                            catch
                            {
                                // Ignore rollback errors
                            }
                            throw;
                        }
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
            // Note: chkApplyToAll checkbox doesn't exist in the designer
            // For now, return all bills if no rows are selected, otherwise return selected rows
            if (dgvOutstandingBills.SelectedRows.Count == 0)
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

        private bool ValidateTerms(out int interestDays, out int discountDays, out decimal discountRate, out decimal interestRate, out decimal brokerageRate)
        {
            interestDays = 0;
            discountDays = 0;
            discountRate = 0;
            interestRate = 0;
            brokerageRate = 0;
            bool valid = int.TryParse(txtInterestDays.Text, out interestDays) &&
                         int.TryParse(txtDiscountDays.Text, out discountDays) &&
                         decimal.TryParse(txtDiscountRate.Text, out discountRate) &&
                         decimal.TryParse(txtInterestRate.Text, out interestRate) &&
                         decimal.TryParse(txtBrokerageRate.Text, out brokerageRate);
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
            
            // Check if payment method is Cheque and validate firm amounts
            string paymentMethod = cmbPaymentMethod.SelectedItem?.ToString() ?? "Cash";
            if (paymentMethod == "Cheque")
            {
                if (!decimal.TryParse(txtChequeAmountFirm1.Text, out decimal firm1Amount) || firm1Amount < 0)
                {
                    MessageBox.Show("Firm 1 cheque amount is invalid.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtChequeAmountFirm1.Focus();
                    return false;
                }
                
                if (!decimal.TryParse(txtChequeAmountFirm2.Text, out decimal firm2Amount) || firm2Amount < 0)
                {
                    MessageBox.Show("Firm 2 cheque amount is invalid.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtChequeAmountFirm2.Focus();
                    return false;
                }
                
                // Verify that at least one firm amount is greater than zero
                if (firm1Amount == 0 && firm2Amount == 0)
                {
                    MessageBox.Show("At least one firm cheque amount must be greater than zero.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtChequeAmountFirm1.Focus();
                    return false;
                }
                
                // Verify that the sum matches the total payment amount
                if (Math.Abs((firm1Amount + firm2Amount) - paymentAmount) > 0.01m)
                {
                    MessageBox.Show("The sum of Firm 1 and Firm 2 amounts must equal the total payment amount.", 
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtChequeAmountFirm1.Focus();
                    return false;
                }
            }
            
            return true;
        }

        #endregion

        #region Payment Trace After Save

        private void ShowPaymentTraceAfterSave(int paymentId)
        {
            try
            {
                // Ask user if they want to print BEFORE loading data
                var result = MessageBox.Show(
                    "Payment saved successfully! Would you like to print the payment slip?",
                    "Print Payment Slip",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Show loading cursor
                    Cursor.Current = Cursors.WaitCursor;
                    
                    // Load data on background thread to prevent UI hanging
                    var backgroundWorker = new System.ComponentModel.BackgroundWorker();
                    backgroundWorker.DoWork += (sender, e) =>
                    {
                        try
                        {
                            // Get the payment details
                            var payment = PaymentService.GetPaymentById(paymentId);
                            if (payment == null)
                            {
                                e.Result = new { Success = false, Message = "Could not retrieve payment details for printing." };
                                return;
                            }

                            // Get the payment trace
                            var paymentTrace = PaymentService.GetPaymentTrace(paymentId);
                            if (!paymentTrace.Any())
                            {
                                e.Result = new { Success = false, Message = "No transaction details found for printing." };
                                return;
                            }

                            e.Result = new { Success = true, Payment = payment, PaymentTrace = paymentTrace };
                        }
                        catch (Exception ex)
                        {
                            e.Result = new { Success = false, Message = ex.Message };
                        }
                    };

                    backgroundWorker.RunWorkerCompleted += (sender, e) =>
                    {
                        Cursor.Current = Cursors.Default;
                        
                        dynamic result_data = e.Result;
                        if (result_data.Success)
                        {
                            // Create and show the payment trace form
                            var traceForm = new PaymentTraceForm(result_data.Payment, result_data.PaymentTrace);
                            // Auto-print the payment slip
                            traceForm.AutoPrint();
                            traceForm.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show(result_data.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    };

                    backgroundWorker.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show($"Error showing payment trace: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        private void btnCalculate_Click_1(object sender, EventArgs e)
        {

        }

        private void CmbPaymentMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedMethod = cmbPaymentMethod.SelectedItem?.ToString() ?? "Cash";
            
            // Show/hide the cheque details panel based on payment method
            pnlChequeDetails.Visible = selectedMethod == "Cheque";
            
            // If switching to Cash, reset the cheque amounts
            if (selectedMethod == "Cash")
            {
                txtChequeAmountFirm1.Text = "0.00";
                txtChequeAmountFirm2.Text = "0.00";
            }
            
            // If switching to Cheque, set focus to the first firm amount field
            if (selectedMethod == "Cheque")
            {
                txtChequeAmountFirm1.Focus();
            }
        }

        private void gbReconciliation_Enter(object sender, EventArgs e)
        {

        }

        private void btnCalculate_Click_2(object sender, EventArgs e)
        {

        }
        
        private void TxtChequeAmount_TextChanged(object sender, EventArgs e)
        {
            // Only process if payment method is Cheque
            if (cmbPaymentMethod.SelectedItem?.ToString() != "Cheque")
                return;
                
            // Calculate the total of the two firm amounts
            if (decimal.TryParse(txtChequeAmountFirm1.Text, out decimal firm1Amount) && 
                decimal.TryParse(txtChequeAmountFirm2.Text, out decimal firm2Amount))
            {
                decimal totalChequeAmount = firm1Amount + firm2Amount;
                txtPaymentAmount.Text = totalChequeAmount.ToString("F2");
            }
        }
    }
}
