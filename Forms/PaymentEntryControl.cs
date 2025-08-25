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
        private List<Bill> _allBills = new List<Bill>();
        
        // Cache for advance payments to avoid repeated DB calls
        private Dictionary<string, List<AdvancePayment>> _advancePaymentsCache = new Dictionary<string, List<AdvancePayment>>();
        private (decimal Cash, decimal Firm1, decimal Firm2)? _cachedAdvanceAmounts = null;
        private string _lastCacheKey = string.Empty;
        
        // Background loading state
        private System.ComponentModel.BackgroundWorker _advanceLoadingWorker;
        private bool _isLoadingAdvances = false;

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
                // Temporarily remove event handlers to prevent automatic selection
                cmbParty.SelectedIndexChanged -= CmbParty_SelectedIndexChanged;
                cmbBroker.SelectedIndexChanged -= CmbBroker_SelectedIndexChanged;
                
                // Store the data in the private fields
                _parties = PartyService.GetAllParties();
                _brokers = BrokerService.GetAllBrokers();
                
                cmbParty.DataSource = _parties;
                cmbParty.DisplayMember = "PartyName";
                cmbParty.ValueMember = "PartyID";
                
                cmbBroker.DataSource = _brokers;
                cmbBroker.DisplayMember = "BrokerName";
                cmbBroker.ValueMember = "BrokerID";
                
                // CRITICAL: Set SelectedIndex to -1 to prevent automatic selection
                cmbParty.SelectedIndex = -1;
                cmbBroker.SelectedIndex = -1;
                
                // Restore event handlers
                cmbParty.SelectedIndexChanged += CmbParty_SelectedIndexChanged;
                cmbBroker.SelectedIndexChanged += CmbBroker_SelectedIndexChanged;
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

            // Clear advance payment cache when form is cleared
            ClearAdvancePaymentCache();

            // Hide advance payment display
            UpdateAdvancePaymentDisplay();

            cmbParty.Focus();
        }

        #endregion

        #region Data Loading & UI Updates

        private void CmbParty_SelectedIndexChanged(object? sender, EventArgs e)
        {
            try
            {
                // When party changes, clear broker selection to avoid cascading events
                LoadBillsBasedOnSelection();
                UpdateFieldsBasedOnSelection();
                
                // Clear cache and preload advance payments for selected party/broker
                InvalidateAdvancePaymentCache();
                PreloadAdvancePayments();
                UpdateAdvancePaymentDisplay();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CmbParty_SelectedIndexChanged: {ex.Message}");
                // Still show the error to user but don't crash
                MessageBox.Show($"Error loading party data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CmbBroker_SelectedIndexChanged(object? sender, EventArgs e)
        {
            try
            {
                LoadBillsBasedOnSelection();
                UpdateFieldsBasedOnSelection();
                
                // Clear cache and preload advance payments for selected party/broker
                InvalidateAdvancePaymentCache();
                PreloadAdvancePayments();
                UpdateAdvancePaymentDisplay();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CmbBroker_SelectedIndexChanged: {ex.Message}");
                // Still show the error to user but don't crash
                MessageBox.Show($"Error loading broker data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

         private void LoadBillsBasedOnSelection()
        {
            int? partyId = cmbParty.SelectedValue as int?;
            int? brokerId = cmbBroker.SelectedValue as int?;

            if (partyId.HasValue && partyId.Value > 0)
            {
                LoadBillsForParty(partyId.Value, brokerId);
            }
            else if (brokerId.HasValue && brokerId.Value > 0)
            {
                LoadBillsByBroker(brokerId.Value);
            }
            else
            {
                LoadAllBills();
            }
            
            // After loading bills, update the grid with unpaid bills only
            UpdateGridWithUnpaidBills();
        }
        

        private void LoadAllBills()
        {
            try
            {
                // Use the optimized GetAllBills that includes party names through JOIN
                var allBills = BillService.GetAllBills();
                
                // Get all bill balances in one database query instead of querying individually
                var allBillBalances = LedgerService.GetAllBillBalances();
                
                // Get all brokers once to use for lookups
                var brokers = BrokerService.GetAllBrokers();
                
                // Update status for each bill based on due amount and populate broker names
                foreach (var bill in allBills)
                {
                    // Use pre-calculated balance from the dictionary
                    decimal dueAmount = allBillBalances.ContainsKey(bill.BillID) ? allBillBalances[bill.BillID] : 0m;
                    bill.Balance = dueAmount;

                    // Look up broker name from broker ID using cached brokers list
                    if (bill.BrokerID.HasValue && bill.BrokerID.Value > 0)
                    {
                        var broker = brokers.FirstOrDefault(b => b.BrokerID == bill.BrokerID.Value);
                        bill.BrokerName = broker?.BrokerName ?? "Unknown Broker";
                    }
                    else
                    {
                        bill.BrokerName = "No Broker";
                    }
                }
                
                _allBills = allBills;
            }

            
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading all bills: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
         private void LoadBillsByBroker(int brokerId)
        {
            try
            {
                // Get all bills with joined party names
                var allBills = BillService.GetAllBills();
                
                // Filter by broker
                _allBills = allBills.Where(b => b.BrokerID == brokerId).ToList();
                
                // Get all bill balances in one database query instead of querying individually
                var allBillBalances = LedgerService.GetAllBillBalances();
                
                // Get the broker once
                var broker = BrokerService.GetBrokerByID(brokerId);
                string brokerName = broker?.BrokerName ?? "Unknown Broker";
                
                // Update status for each bill
                foreach (var bill in _allBills)
                {
                    // Set broker name for all bills
                    bill.BrokerName = brokerName;
                    
                    // Use pre-calculated balance from the dictionary
                    decimal dueAmount = allBillBalances.ContainsKey(bill.BillID) ? allBillBalances[bill.BillID] : 0m;
                    bill.Balance = dueAmount;
                    
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bills by broker: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadBillsForParty(int partyId, int? brokerId = null)
        {
            try
            {
                var bills = BillService.GetAllBillsForParty(partyId);
                
                // Filter by broker if specified
                if (brokerId.HasValue && brokerId.Value > 0)
                {
                    bills = bills.Where(b => b.BrokerID == brokerId.Value).ToList();
                }
                
                // Get all bill balances in one database query instead of querying individually
                var allBillBalances = LedgerService.GetAllBillBalances();
                
                // Get all brokers once to use for lookups
                var brokers = BrokerService.GetAllBrokers();
                
                // Get the party once to use for all bills
                var party = PartyService.GetPartyByID(partyId);
                string partyName = party?.PartyName ?? "Unknown Party";
                
                // Update status for each bill
                foreach (var bill in bills)
                {
                    // Set party name for all bills from the same party
                    bill.PartyName = partyName;
                    
                    // Use pre-calculated balance from the dictionary
                    decimal dueAmount = allBillBalances.ContainsKey(bill.BillID) ? allBillBalances[bill.BillID] : 0m;
                    bill.Balance = dueAmount;

                    // Look up broker name from broker ID using cached brokers list
                    if (bill.BrokerID.HasValue && bill.BrokerID.Value > 0)
                    {
                        var broker = brokers.FirstOrDefault(b => b.BrokerID == bill.BrokerID.Value);
                        bill.BrokerName = broker?.BrokerName ?? "Unknown Broker";
                    }
                    else
                    {
                        bill.BrokerName = "No Broker";
                    }

                }
                
                _allBills = bills; // Store all bills for pagination
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bills: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // NEW METHOD: Convert _allBills to _outstandingBills and display in grid
        private void UpdateGridWithUnpaidBills()
        {
            try
            {
                if (_allBills == null || !_allBills.Any())
                {
                    dgvOutstandingBills.DataSource = null;
                    _outstandingBills = new List<BillViewModel>();
                    return;
                }

                // Filter out paid bills (those with balance <= 0) and convert to BillViewModel
                _outstandingBills = _allBills
                    .Where(b => b.Balance > 0.01m) // Only unpaid bills (with small tolerance for floating point)
                    .Select(b => new BillViewModel
                    {
                        BillID = b.BillID,
                        BillNo = b.BillNo,
                        PartyName = b.PartyName ?? "Unknown Party",
                        BrokerName = b.BrokerName ?? "No Broker",
                        BillDate = b.BillDate,
                        OriginalAmount = b.OriginalAmount,
                        AdditionalCharges = b.AdditionalCharges,
                        BalanceDue = b.Balance,
                        PaymentAllocation = 0, // Initialize payment allocation to 0
                        ChequeAmountFirm1 = b.ChequeAmountFirm1,
                        ChequeAmountFirm2 = b.ChequeAmountFirm2
                    })
                    .OrderBy(b => b.BillDate) // Order by bill date (oldest first)
                    .ToList();

                // Update the grid
                dgvOutstandingBills.DataSource = _outstandingBills;
                
                // Reset grid styles
                ResetGridStyles();
                
                // Update summary labels
                UpdateSummaryLabels();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating grid with unpaid bills: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // NEW METHOD: Update summary labels based on current bills
        private void UpdateSummaryLabels()
        {
            if (_outstandingBills == null || !_outstandingBills.Any())
            {
                lblDiscountValue.Text = "Discount: ₹0.00";
                lblInterestValue.Text = "Interest: ₹0.00";
                lblBrokerageValue.Text = "Brokerage: ₹0.00";
                lblFinalAmount.Text = "Amount Due: ₹0.00";
                return;
            }

            decimal totalBalance = _outstandingBills.Sum(b => b.BalanceDue);
            lblFinalAmount.Text = $"Total Outstanding: ₹{totalBalance:N2}";
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
                // Broker selected - load broker data and make READ-ONLY
                var broker = _brokers.FirstOrDefault(b => b.BrokerID == brokerId.Value);
                
                if (broker != null)
                {
                    txtInterestDays.Text = broker.InterestDays.ToString();
                    txtDiscountDays.Text = broker.DiscountDays.ToString();
                    txtDiscountRate.Text = broker.DiscountRate.ToString("F2");
                    txtInterestRate.Text = broker.InterestRate.ToString("F2");
                    txtBrokerageRate.Text = broker.BrokerageRate.ToString("F2");
                    
                    // CRITICAL: Make fields READ-ONLY when broker is selected
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
            
            // Only update payment amount if user hasn't entered a larger amount (preserve excess)
            if (decimal.TryParse(txtPaymentAmount.Text, out decimal currentPaymentAmount))
            {
                if (totalAllocated > currentPaymentAmount)
                {
                    // If allocated amount is greater than what user entered, update the field
                    txtPaymentAmount.Text = Math.Round(totalAllocated).ToString("F0");
                }
                // If user entered more than allocated (excess), keep the original amount and show excess in advance display
                else if (currentPaymentAmount > totalAllocated)
                {
                    // Refresh advance display to show the potential excess as advance
                    UpdateAdvancePaymentDisplay();
                }
            }
            else
            {
                // Fallback to old behavior if payment amount is not a valid number
                txtPaymentAmount.Text = Math.Round(totalAllocated).ToString("F0");
            }
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
                ChequeAmountFirm2Text = txtChequeAmountFirm2.Text,
                AdvanceUsed = 0,
                AdvanceAmount = 0,
                IsAdvancePayment = false
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
                    int paymentId;
                    decimal totalAvailableAdvance;
                    (paymentId, totalAvailableAdvance) = SavePaymentInBackground(paymentData);
                    e.Result = new { Success = true, PaymentId = paymentId, TotalAvailableAdvance = totalAvailableAdvance };
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
                    MessageBox.Show($"Payment(s) saved successfully! Total available advance: {result.TotalAvailableAdvance:C}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Show payment trace with print option
                    ShowPaymentTraceAfterSave(result.PaymentId);
                    
                    // Refresh advance display to show updated amounts after save
                    InvalidateAdvancePaymentCache();
                    UpdateAdvancePaymentDisplay();
                    
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
            public Dictionary<int, List<AdvanceUtilization>> AdvanceUtilizations { get; set; } = new Dictionary<int, List<AdvanceUtilization>>();
            public decimal AdvanceUsed { get; set; }
            public decimal AdvanceAmount { get; set; }
            public bool IsAdvancePayment { get; set; }
        }

        private (int, decimal) SavePaymentInBackground(PaymentSaveData data)
        {
            using (var conn = DatabaseManager.GetConnection())
            {
                conn.Open();
                var dbTransaction = conn.BeginTransaction();
                try
                {
                    // Determine the party ID for the payment master record
                    int? partyId = null;
                    decimal totalAvailableAdvance = 0;
                    List<AdvancePayment> combinedAvailableAdvances = new List<AdvancePayment>();
                    if (data.SelectedPartyId.HasValue && data.SelectedPartyId.Value > 0)
                    {
                        // Party is directly selected
                        partyId = data.SelectedPartyId.Value;
                     combinedAvailableAdvances.AddRange(AdvancePaymentService.GetAvailableAdvancePayments(partyId, null, 1));
                    if (combinedAvailableAdvances.Any())
                    {
                        totalAvailableAdvance = combinedAvailableAdvances.Sum(a => a.Amount);
                        System.Diagnostics.Debug.WriteLine($"Found {combinedAvailableAdvances.Count} advance payments totaling {totalAvailableAdvance:C} for PartyID: {partyId}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"No advance payments found for PartyID: {partyId}");
                    }
                    }else{
                        partyId = -1;
                    }
                    

                    // Determine the broker ID for the payment
                    int? brokerId = null;
                    if (data.SelectedBrokerId.HasValue && data.SelectedBrokerId.Value > 0)
                    {
                        brokerId = data.SelectedBrokerId.Value;
                     combinedAvailableAdvances.AddRange(AdvancePaymentService.GetAvailableAdvancePayments(null, brokerId, 1));
                    if (combinedAvailableAdvances.Any())
                    {
                        totalAvailableAdvance = combinedAvailableAdvances.Sum(a => a.Amount);
                        System.Diagnostics.Debug.WriteLine($"Found {combinedAvailableAdvances.Count} advance payments totaling {totalAvailableAdvance:C} for BrokerID: {brokerId}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"No advance payments found for BrokerID: {brokerId}");
                    }
                    }else{
                        brokerId = -1;
                    }
                    

                    

                    // Step 1.2: Use FIFO logic to consume advances against bills (not payment amount)
                    decimal totalBillAmount = data.PaymentsToSave.Sum(p => p.PaymentAllocation);
                    decimal totalAmountNeeded = totalBillAmount; // Use advances to cover bills, not full payment
                    decimal advanceUsed = 0;
                    
                    if (totalAvailableAdvance > 0 && totalAmountNeeded > 0)
                    {
                        // Sort advances by payment date (FIFO - oldest first)
                        var sortedAdvances = combinedAvailableAdvances.OrderBy(a => a.PaymentDate).ToList();
                        decimal remainingAmountNeeded = totalAmountNeeded; // Amount still needed to cover bills
                        
                        foreach (var advance in sortedAdvances)
                        {
                            if (remainingAmountNeeded <= 0) break;
                            
                            decimal amountToUse = Math.Min(advance.Amount, remainingAmountNeeded);
                            if (amountToUse > 0)
                            {
                                // Create utilization record instead of reducing advance amount
                                var utilization = new AdvanceUtilization
                                {
                                    AdvanceID = advance.AdvanceID,
                                    PaymentID = 0, // Will be set after PaymentMaster is saved
                                    AmountUsed = amountToUse,
                                    UtilizedDate = data.PaymentDate,
                                    PartyID = advance.PartyID,
                                    BrokerID = advance.BrokerID,
                                    CompanyID = 1
                                };
                                
                                // Store for later processing after PaymentMaster is saved
                                if (!data.AdvanceUtilizations.ContainsKey(advance.AdvanceID))
                                {
                                    data.AdvanceUtilizations[advance.AdvanceID] = new List<AdvanceUtilization>();
                                }
                                data.AdvanceUtilizations[advance.AdvanceID].Add(utilization);
                                
                                advanceUsed += amountToUse;
                                remainingAmountNeeded -= amountToUse;
                                
                                System.Diagnostics.Debug.WriteLine($"Will use {amountToUse:C} from AdvanceID: {advance.AdvanceID} to cover bills, remaining bill amount needed: {remainingAmountNeeded:C}");
                            }
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"Total advance used against bills: {advanceUsed:C}, remaining bill amount needed: {remainingAmountNeeded:C}");
                    }

                    // Calculate actual cash payment used (bills - advance used)
                    decimal actualCashUsed = Math.Max(0, totalBillAmount - advanceUsed);
                    
                    // Create a single master record for this payment event (actual cash used, not total payment)
                    var paymentMaster = new PaymentMaster
                    {
                        PartyID = partyId,
                        BrokerID = brokerId,
                        PaymentDate = data.PaymentDate,
                        TotalAmountPaid = Math.Round(data.TotalPaymentAmount), // Only the cash actually used for bills
                        PaymentMethod = data.PaymentMethod,
                        Reference = data.Reference,
                        CompanyID = 1, // Replace with Program.ActiveCompany.CompanyID
                        ChequeAmountFirm1 = 0,
                        ChequeAmountFirm2 = 0,
                        AdvanceUsed = data.AdvanceUsed,
                        AdvanceAmount = data.AdvanceAmount,
                        IsAdvancePayment = data.IsAdvancePayment
                    };
                    
                    // If payment method is Cheque, distribute the actual cash used proportionally
                    if (paymentMaster.PaymentMethod == "Cheque")
                    {
                        if (decimal.TryParse(data.ChequeAmountFirm1Text, out decimal firm1Amount) && 
                            decimal.TryParse(data.ChequeAmountFirm2Text, out decimal firm2Amount))
                        {
                            decimal totalCheque = firm1Amount + firm2Amount;
                            if (totalCheque > 0 && actualCashUsed > 0)
                            {
                                // Distribute actual cash used proportionally based on original firm amounts
                                decimal firm1Ratio = firm1Amount / totalCheque;
                                decimal firm2Ratio = firm2Amount / totalCheque;
                                paymentMaster.ChequeAmountFirm1 = Math.Round(actualCashUsed * firm1Ratio);
                                paymentMaster.ChequeAmountFirm2 = Math.Round(actualCashUsed * firm2Ratio);
                                
                                System.Diagnostics.Debug.WriteLine($"Cheque distribution for actual cash used {actualCashUsed:C}: Firm1={paymentMaster.ChequeAmountFirm1:C}, Firm2={paymentMaster.ChequeAmountFirm2:C}");
                            }
                        }
                    }
                    decimal excessAmount = 0;
                    if(advanceUsed < data.TotalPaymentAmount){
                    excessAmount = data.TotalPaymentAmount - actualCashUsed;
                    paymentMaster.AdvanceAmount = excessAmount;
                    paymentMaster.AdvanceUsed = advanceUsed;
                    paymentMaster.IsAdvancePayment = true;
                    }
                    if(excessAmount == 0 && advanceUsed > 0){
                        paymentMaster.AdvanceAmount = 0;
                        paymentMaster.AdvanceUsed = advanceUsed;
                        paymentMaster.IsAdvancePayment = false;
                    } 
                    if(excessAmount == 0 && advanceUsed == 0){
                        paymentMaster.AdvanceAmount = 0;
                        paymentMaster.AdvanceUsed = 0;
                        paymentMaster.IsAdvancePayment = false;
                    }
                    int paymentId = PaymentService.SavePaymentMaster(paymentMaster, conn, dbTransaction);

                    // Save advance utilization records after payment is saved
                    foreach (var advanceUtilizationGroup in data.AdvanceUtilizations)
                    {
                        foreach (var utilization in advanceUtilizationGroup.Value)
                        {
                            utilization.PaymentID = paymentId; // Now we have the PaymentID
                            bool utilizationSaved = AdvanceUtilizationService.AddUtilization(utilization, conn, dbTransaction);
                            if (!utilizationSaved)
                            {
                                throw new Exception($"Failed to save advance utilization for AdvanceID: {utilization.AdvanceID}");
                            }
                            System.Diagnostics.Debug.WriteLine($"Saved utilization: {utilization.AmountUsed:C} from AdvanceID: {utilization.AdvanceID} for PaymentID: {paymentId}");
                        }
                    }

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

                    // Handle excess amount - create new advance payment
                    // Excess = Total Payment Entered - Actual Cash Used for Bills
                    
                    System.Diagnostics.Debug.WriteLine($"Excess calculation: Payment entered {data.TotalPaymentAmount:C} - Cash used {actualCashUsed:C} = Excess {excessAmount:C}");
                    
                    if (excessAmount > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"Creating advance payment for excess amount: {excessAmount:C}");
                        
                        // Create new advance payment record from excess amount
                        var excessAdvancePayment = new AdvancePayment
                        {
                            PartyID = data.SelectedPartyId > 0 ? data.SelectedPartyId : null,
                            BrokerID = data.SelectedBrokerId > 0 ? data.SelectedBrokerId : null,
                            PaymentDate = data.PaymentDate,
                            Amount = Math.Round(excessAmount),
                            PaymentMethod = data.PaymentMethod,
                            Reference = $"Excess from Payment Ref: {data.Reference}",
                            CompanyID = 1
                        };

                        // Handle cheque amounts for excess
                        if (data.PaymentMethod?.Equals("Cheque", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            if (decimal.TryParse(data.ChequeAmountFirm1Text, out decimal firm1Amount) && 
                                decimal.TryParse(data.ChequeAmountFirm2Text, out decimal firm2Amount))
                            {
                                decimal totalCheque = firm1Amount + firm2Amount;
                                if (totalCheque > 0)
                                {
                                    // Distribute excess proportionally to firm amounts
                                    decimal firm1Ratio = firm1Amount / totalCheque;
                                    decimal firm2Ratio = firm2Amount / totalCheque;
                                    excessAdvancePayment.ChequeAmountFirm1 = Math.Round(excessAmount * firm1Ratio);
                                    excessAdvancePayment.ChequeAmountFirm2 = Math.Round(excessAmount * firm2Ratio);
                                }
                                else
                                {
                                    // Default to Firm1 if no distribution is clear
                                    excessAdvancePayment.ChequeAmountFirm1 = Math.Round(excessAmount);
                                    excessAdvancePayment.ChequeAmountFirm2 = 0;
                                }
                            }
                            else
                            {
                                // Default to Firm1 if parsing fails
                                excessAdvancePayment.ChequeAmountFirm1 = Math.Round(excessAmount);
                                excessAdvancePayment.ChequeAmountFirm2 = 0;
                            }
                        }
                        else
                        {
                            // For cash or other payment methods, set cheque amounts to 0
                            excessAdvancePayment.ChequeAmountFirm1 = 0;
                            excessAdvancePayment.ChequeAmountFirm2 = 0;
                        }

                        // Add advance payment directly in the same transaction to avoid lock issues
                        string advanceSql = @"
                            INSERT INTO AdvancePayments (PartyID, BrokerID, PaymentDate, Amount, PaymentMethod, Reference, ChequeAmountFirm1, ChequeAmountFirm2, CompanyID, CreatedDate)
                            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                        using (var advanceCmd = new OleDbCommand(advanceSql, conn, dbTransaction))
                        {
                            var parameters = new OleDbParameter[]
                            {
                                new OleDbParameter("PartyID", OleDbType.Integer) { Value = excessAdvancePayment.PartyID ?? (object)DBNull.Value },
                                new OleDbParameter("BrokerID", OleDbType.Integer) { Value = excessAdvancePayment.BrokerID ?? (object)DBNull.Value },
                                new OleDbParameter("PaymentDate", OleDbType.Date) { Value = excessAdvancePayment.PaymentDate },
                                new OleDbParameter("Amount", OleDbType.Currency) { Value = excessAdvancePayment.Amount },
                                new OleDbParameter("PaymentMethod", OleDbType.VarChar, 50) { Value = excessAdvancePayment.PaymentMethod ?? (object)DBNull.Value },
                                new OleDbParameter("Reference", OleDbType.VarChar, 255) { Value = excessAdvancePayment.Reference ?? (object)DBNull.Value },
                                new OleDbParameter("ChequeAmountFirm1", OleDbType.Currency) { Value = excessAdvancePayment.ChequeAmountFirm1 },
                                new OleDbParameter("ChequeAmountFirm2", OleDbType.Currency) { Value = excessAdvancePayment.ChequeAmountFirm2 },
                                new OleDbParameter("CompanyID", OleDbType.Integer) { Value = excessAdvancePayment.CompanyID },
                                new OleDbParameter("CreatedDate", OleDbType.Date) { Value = DateTime.Now }
                            };

                            advanceCmd.Parameters.AddRange(parameters);
                            
                            int advanceRows = advanceCmd.ExecuteNonQuery();
                            if (advanceRows > 0)
                            {
                                System.Diagnostics.Debug.WriteLine($"Created advance payment for excess: {excessAmount:C}");
                            }
                        }
                    }

                    // First commit the ledger transactions
                    dbTransaction.Commit();

                    // Now update bill statuses in a new transaction
                    UpdateBillStatuses(data.PaymentsToSave);

                    return (paymentId, totalAvailableAdvance); // Return the payment ID for success handling
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
                // Simple confirmation dialog
                var result = MessageBox.Show(
                    "Payment saved successfully! Would you like to print the payment slip?",
                    "Print Payment Slip",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Simple approach - just show the form without auto-print
                    ShowPaymentTraceForm(paymentId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing payment trace: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowPaymentTraceForm(int paymentId)
        {
            try
            {
                // Get payment details
                var payment = PaymentService.GetPaymentById(paymentId);
                if (payment == null)
                {
                    MessageBox.Show("Could not retrieve payment details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get payment transactions
                var paymentTrace = PaymentService.GetPaymentTrace(paymentId);
                if (!paymentTrace.Any())
                {
                    MessageBox.Show("No transaction details found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create and show the form
                var traceForm = new PaymentTraceForm(payment, paymentTrace);
                traceForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing payment trace form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        #region Advance Payment Display

        /// <summary>
        /// Updates the advance payment display based on selected party and broker
        /// </summary>
        private void UpdateAdvancePaymentDisplay()
        {
            try
            {
                int? partyId = cmbParty.SelectedValue as int?;
                int? brokerId = cmbBroker.SelectedValue as int?;

                // Hide panel if nothing is selected
                if ((!partyId.HasValue || partyId.Value <= 0) && (!brokerId.HasValue || brokerId.Value <= 0))
                {
                    pnlAdvanceDisplay.Visible = false;
                    return;
                }

                // Get advance amounts by payment method
                var advanceAmounts = GetAdvanceAmountsByPaymentMethod(partyId, brokerId);
                
                // Calculate excess amount using the same logic as save process
                decimal excessAmount = 0;
                string selectedPaymentMethod = cmbPaymentMethod.SelectedItem?.ToString() ?? "Cash";
                
                if (decimal.TryParse(txtPaymentAmount.Text, out decimal paymentAmount))
                {
                    decimal totalAllocated = _outstandingBills.Sum(b => b.PaymentAllocation);
                    
                    if (paymentAmount > 0 && totalAllocated > 0)
                    {
                        // Calculate how much advance would be used against bills (FIFO simulation)
                        decimal totalAdvanceAvailable = advanceAmounts.Cash + advanceAmounts.Firm1 + advanceAmounts.Firm2;
                        decimal advanceUsedAgainstBills = Math.Min(totalAdvanceAvailable, totalAllocated);
                        
                        // Remove used advances from display (they will be consumed)
                        if (advanceUsedAgainstBills > 0)
                        {
                            // Simulate FIFO consumption to remove used amounts from display
                            decimal remainingToRemove = advanceUsedAgainstBills;
                            
                            // Remove from Cash first
                            if (remainingToRemove > 0 && advanceAmounts.Cash > 0)
                            {
                                decimal removeFromCash = Math.Min(advanceAmounts.Cash, remainingToRemove);
                                advanceAmounts.Cash -= removeFromCash;
                                remainingToRemove -= removeFromCash;
                            }
                            
                            // Remove from Firm1 next
                            if (remainingToRemove > 0 && advanceAmounts.Firm1 > 0)
                            {
                                decimal removeFromFirm1 = Math.Min(advanceAmounts.Firm1, remainingToRemove);
                                advanceAmounts.Firm1 -= removeFromFirm1;
                                remainingToRemove -= removeFromFirm1;
                            }
                            
                            // Remove from Firm2 last
                            if (remainingToRemove > 0 && advanceAmounts.Firm2 > 0)
                            {
                                decimal removeFromFirm2 = Math.Min(advanceAmounts.Firm2, remainingToRemove);
                                advanceAmounts.Firm2 -= removeFromFirm2;
                                remainingToRemove -= removeFromFirm2;
                            }
                        }
                        
                        // Calculate actual cash that would be used for bills
                        decimal actualCashUsed = Math.Max(0, totalAllocated - advanceUsedAgainstBills);
                        
                        // Excess = Payment Entered - Cash Actually Used
                        if (paymentAmount > actualCashUsed)
                        {
                            excessAmount = paymentAmount - actualCashUsed;
                            
                            System.Diagnostics.Debug.WriteLine($"Display calculation: Payment {paymentAmount:C}, Bills {totalAllocated:C}, Advance used {advanceUsedAgainstBills:C}, Cash used {actualCashUsed:C}, Excess {excessAmount:C}");
                        }
                    }
                    else if (paymentAmount > totalAllocated)
                    {
                        // Fallback for when no bills are allocated
                        excessAmount = paymentAmount - totalAllocated;
                        
                    }
                    
                    // Add excess to the appropriate advance category based on payment method (if any excess exists)
                    if (excessAmount > 0)
                    {
                        if (selectedPaymentMethod.Equals("Cash", StringComparison.OrdinalIgnoreCase))
                        {
                            advanceAmounts.Cash += excessAmount;
                        }
                        else if (selectedPaymentMethod.Equals("Cheque", StringComparison.OrdinalIgnoreCase))
                        {
                            // For cheque payments, distribute excess based on firm amounts ratio
                            if (decimal.TryParse(txtChequeAmountFirm1.Text, out decimal firm1) && 
                                decimal.TryParse(txtChequeAmountFirm2.Text, out decimal firm2))
                            {
                                decimal totalCheque = firm1 + firm2;
                                if (totalCheque > 0 && Math.Abs(totalCheque - paymentAmount) < 0.01m)
                                {
                                    // Distribute excess proportionally
                                    decimal firm1Ratio = firm1 / totalCheque;
                                    decimal firm2Ratio = firm2 / totalCheque;
                                    advanceAmounts.Firm1 += excessAmount * firm1Ratio;
                                    advanceAmounts.Firm2 += excessAmount * firm2Ratio;
                                }
                                else
                                {
                                    // Default to equal split or add to Firm1
                                    advanceAmounts.Firm1 += excessAmount;
                                }
                            }
                            else
                            {
                                // Default to adding excess to Firm1
                                advanceAmounts.Firm1 += excessAmount;
                            }
                        }
                    }
                }
                
                // Update labels
                lblAdvanceCash.Text = $"Cash: ₹{advanceAmounts.Cash:N2}";
                lblAdvanceFirm1.Text = $"Firm1: ₹{advanceAmounts.Firm1:N2}";
                lblAdvanceFirm2.Text = $"Firm2: ₹{advanceAmounts.Firm2:N2}";

                // Update title to show if there's excess
                if (excessAmount > 0)
                {
                    lblAdvanceTitle.Text = $"Advance Avail (+ ₹{excessAmount:N2} excess):";
                    lblAdvanceTitle.ForeColor = Color.Red; // Highlight excess
                }
                else
                {
                    lblAdvanceTitle.Text = "Advance Avail:";
                    lblAdvanceTitle.ForeColor = Color.Black; // Default color
                }

                // Show panel if there are any advances or excess
                bool hasAdvances = advanceAmounts.Cash > 0 || advanceAmounts.Firm1 > 0 || advanceAmounts.Firm2 > 0;
                pnlAdvanceDisplay.Visible = hasAdvances;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating advance payment display: {ex.Message}");
                pnlAdvanceDisplay.Visible = false;
            }
        }

        /// <summary>
        /// Gets advance amounts broken down by payment method using cache
        /// </summary>
        private (decimal Cash, decimal Firm1, decimal Firm2) GetAdvanceAmountsByPaymentMethod(int? partyId, int? brokerId)
        {
            try
            {
                // Generate cache key
                string cacheKey = GetCacheKey(partyId, brokerId);
                
                // Return cached amounts if available and cache key hasn't changed
                if (_cachedAdvanceAmounts.HasValue && _lastCacheKey == cacheKey)
                {
                    return _cachedAdvanceAmounts.Value;
                }

                decimal cashAmount = 0;
                decimal firm1Amount = 0;
                decimal firm2Amount = 0;

                // Get advance payments from cache or database
                var advancePayments = GetAdvancePaymentsFromCache(partyId, brokerId);

                foreach (var advance in advancePayments)
                {
                    if (advance.PaymentMethod?.Trim().Equals("Cash", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        cashAmount += advance.Amount;
                    }
                    else if (advance.PaymentMethod?.Trim().Equals("Cheque", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        firm1Amount += advance.ChequeAmountFirm1;
                        firm2Amount += advance.ChequeAmountFirm2;
                    }
                }

                // Cache the result
                var result = (cashAmount, firm1Amount, firm2Amount);
                _cachedAdvanceAmounts = result;
                _lastCacheKey = cacheKey;

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting advance amounts by payment method: {ex.Message}");
                return (0, 0, 0);
            }
        }

        #endregion

        #region Advance Payment Cache Management

        /// <summary>
        /// Generates a cache key for advance payments based on party and broker IDs
        /// </summary>
        private string GetCacheKey(int? partyId, int? brokerId)
        {
            return $"P{partyId ?? 0}_B{brokerId ?? 0}";
        }

        /// <summary>
        /// Gets advance payments from cache or loads from database if not cached
        /// </summary>
        private List<AdvancePayment> GetAdvancePaymentsFromCache(int? partyId, int? brokerId)
        {
            string cacheKey = GetCacheKey(partyId, brokerId);
            
            // Return from cache if exists
            if (_advancePaymentsCache.TryGetValue(cacheKey, out List<AdvancePayment> cachedPayments))
            {
                System.Diagnostics.Debug.WriteLine($"Using cached advance payments for key: {cacheKey}");
                return cachedPayments;
            }

            // Load from database and cache
            System.Diagnostics.Debug.WriteLine($"Loading advance payments from database for key: {cacheKey}");
            var payments = AdvancePaymentService.GetAvailableAdvancePayments(partyId, brokerId);
            _advancePaymentsCache[cacheKey] = payments;
            
            return payments;
        }

        /// <summary>
        /// Preloads advance payments for the current selection to cache
        /// </summary>
        private void PreloadAdvancePayments()
        {
            try
            {
                int? partyId = cmbParty.SelectedValue as int?;
                int? brokerId = cmbBroker.SelectedValue as int?;
                
                // Only preload if party or broker is selected
                if ((!partyId.HasValue || partyId.Value <= 0) && (!brokerId.HasValue || brokerId.Value <= 0))
                {
                    return;
                }

                // Load synchronously for now to avoid thread issues
                // Background loading can be added later after ensuring thread safety
                GetAdvancePaymentsFromCache(partyId, brokerId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error preloading advance payments: {ex.Message}");
            }
        }

        /// <summary>
        /// Clears the entire advance payment cache
        /// </summary>
        private void ClearAdvancePaymentCache()
        {
            _advancePaymentsCache.Clear();
            _cachedAdvanceAmounts = null;
            _lastCacheKey = string.Empty;
            System.Diagnostics.Debug.WriteLine("Advance payment cache cleared");
        }

        /// <summary>
        /// Invalidates cache for current selection (forces reload on next access)
        /// </summary>
        private void InvalidateAdvancePaymentCache()
        {
            _cachedAdvanceAmounts = null;
            _lastCacheKey = string.Empty;
        }

        #endregion
    }
}
