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
        
        // User-selected advance payments
        private List<AdvancePayment> _userSelectedAdvancePayments = new List<AdvancePayment>();
        private SettlementCalculationResult? _lastSettlementResult = null;
        
        // Unused advance reversal tracking
        private bool _shouldRevertUnusedAdvances = false;
        private List<UnusedAdvanceDetail> _unusedAdvancesToRevert = new List<UnusedAdvanceDetail>();

        // Background loading state
        private System.ComponentModel.BackgroundWorker _advanceLoadingWorker;
        private bool _isLoadingAdvances = false;
        public class PaymentAllocationResult
        {
            public decimal AdvanceUsed { get; set; }
            public decimal CashUsed { get; set; }
            public decimal DiscountEarned { get; set; }
            public decimal InterestCharged { get; set; }
            public decimal BrokerageCharged { get; set; }
            public decimal NetSettlement { get; set; }
            public List<AdvanceUtilization> AdvanceBreakdown { get; set; }
        }

        /// <summary>
        /// Complete payment allocation summary for multiple bills
        /// </summary>
        public class PaymentAllocationSummary
        {
            public decimal TotalPaymentAmount { get; set; }
            public decimal TotalAdvanceUsed { get; set; }
            public decimal TotalCashUsed { get; set; }
            public decimal TotalDiscountEarned { get; set; }
            public decimal TotalInterestCharged { get; set; }
            public decimal TotalBrokerageCharged { get; set; }
            public decimal ExcessAmount { get; set; }
            public List<BillPaymentResult> BillResults { get; set; } = new List<BillPaymentResult>();
            public List<AdvanceUtilization> AdvanceUtilizations { get; set; } = new List<AdvanceUtilization>();
        }
        /// <summary>
        /// Individual bill payment result
        /// </summary>
        public class AdvanceAllocation
{
    public int AdvanceID { get; set; }
    public decimal AmountUsed { get; set; }
    public DateTime AdvanceDate { get; set; }
}
        public class BillPaymentResult
        {
            public int BillID { get; set; }
            public string BillNo { get; set; }
            public decimal OriginalBalance { get; set; }
            public decimal PaymentAllocated { get; set; }
            public decimal RemainingBalance { get; set; }
            public decimal AdvanceUsed { get; set; }
            public decimal CashUsed { get; set; }
            public decimal InterestCharged { get; set; }
            public decimal DiscountEarned { get; set; }
            public decimal BrokerageCharged { get; set; }
            public PaymentStatus Status { get; set; }
            public List<AdvanceAllocation> AdvanceAllocations { get; set; } = new List<AdvanceAllocation>();
        }
        public enum PaymentStatus
        {
            FullyPaid,
            PartiallyPaid,
            NothingPaid
        }
        private PaymentAllocationSummary _currentAllocationSummary;
        /// <summary>
        /// Payment terms for calculations
        /// </summary>
        public class PaymentTerms
        {
            public int InterestDays { get; set; }
            public decimal InterestRate { get; set; }
            public int DiscountDays { get; set; }
            public decimal DiscountRate { get; set; }
            public decimal BrokerageRate { get; set; }
            public DateTime PaymentDate { get; set; }
        }

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
                HeaderText = "Calculated Amount",
                Name = "PaymentAllocation",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, BackColor = Color.LightYellow },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
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

        public class PaymentEvent
        {
            public DateTime Date { get; set; }
            public decimal Amount { get; set; }
            public string Type { get; set; } // "Advance", "Payment"
            public bool IsHistorical { get; set; } // True for existing transactions, false for new ones
            public int AdvanceId { get; set; } // For tracking advance utilization
        }

        public class PaymentCalculationSummary
        {
            public decimal TotalAmountDue { get; set; }
            public decimal AdvanceAvailable { get; set; }
            public decimal AdvanceUsable { get; set; }
            public decimal CashRequired { get; set; }
            public decimal InterestCharged { get; set; }
            public decimal DiscountEarned { get; set; }
            public decimal BrokerageCharged { get; set; }
            public bool CanFullySettle { get; set; }
        }

        private PaymentCalculationSummary CalculateBillRequirementOnly(
    Bill bill, 
    BillViewModel billVm,
    List<AdvancePayment> availableAdvances,
    DateTime paymentDate,
    int interestDays, 
    decimal interestRate,
    int discountDays, 
    decimal discountRate,
    decimal brokerageRate)
{
    // Step 1: Get historical transactions only
    var historicalTransactions = LedgerService.GetTransactionsForBill(bill.BillID);
    
    // Step 2: Create timeline with ONLY historical payments (no new advances)
    var historicalEvents = new List<PaymentEvent>();
    foreach (var transaction in historicalTransactions)
    {
        if (transaction.TransactionType == "Payment" || transaction.TransactionType == "Advance")
        {
            historicalEvents.Add(new PaymentEvent
            {
                Date = transaction.TransactionDate,
                Amount = transaction.CreditAmount,
                Type = transaction.TransactionType,
                IsHistorical = true,
                AdvanceId = transaction.PaymentID ?? 0
            });
        }
    }

    // Step 3: Calculate timeline with historical payments only
    var timeline = CalculateCompleteTimeline(
        bill, 
        historicalEvents.OrderBy(e => e.Date).ToList(), 
        interestDays, 
        interestRate, 
        discountDays, 
        discountRate, 
        brokerageRate,
        paymentDate);
    
    // Step 4: Calculate what's needed vs what's available
    decimal totalNeeded = timeline.RemainingBalance + timeline.TotalInterest - timeline.TotalDiscount;
    decimal totalAdvanceAvailable = availableAdvances.Sum(a => a.Amount);
    decimal advanceUsable = Math.Min(totalAdvanceAvailable, totalNeeded);
    decimal cashRequired = Math.Max(0, totalNeeded - totalAdvanceAvailable);
    
    return new PaymentCalculationSummary
    {
        TotalAmountDue = totalNeeded,
        AdvanceAvailable = totalAdvanceAvailable,
        AdvanceUsable = advanceUsable,
        CashRequired = cashRequired,
        InterestCharged = timeline.TotalInterest,
        DiscountEarned = timeline.TotalDiscount,
        BrokerageCharged = timeline.Brokerage,
        CanFullySettle = cashRequired <= 0.01m
    };
}

        private TimelineCalculationResult CalculateCompleteTimeline(
        Bill bill, 
        List<PaymentEvent> sortedEvents,
        int interestDays, 
        decimal interestRate, 
        int discountDays, 
        decimal discountRate, 
        decimal brokerageRate,
        DateTime paymentDate)
        {
            var result = new TimelineCalculationResult();
            
            // Key dates
            DateTime billDate = bill.BillDate;
            DateTime dueDate = billDate.AddDays(interestDays);
            DateTime discountDueDate = billDate.AddDays(discountDays);
            
            // Initialize
            decimal originalAmount = bill.TotalAmount;
            decimal brokerage = originalAmount * brokerageRate / 100m;
            decimal principalBalance = originalAmount - brokerage;
            decimal nonHistoricalAdvanceAmount = 0;
            decimal totalDiscount = 0;
            decimal totalInterest = 0;
            
            DateTime currentInterestDate = dueDate;
            
            // Process each payment event chronologically
            foreach (var paymentEvent in sortedEvents)
            {
                // Calculate interest from last interest date to this payment date
                if (paymentEvent.Date > dueDate && principalBalance > 0)
                {
                    int interestDaysThisPeriod = (paymentEvent.Date.Date - currentInterestDate.Date).Days;
                    if (interestDaysThisPeriod > 0)
                    {
                        decimal periodInterest = principalBalance * (interestRate / 100m) * (interestDaysThisPeriod / 365m);
                        totalInterest += periodInterest;
                        
                        // Store period details for debugging
                        result.InterestPeriods.Add(new InterestPeriod
                        {
                            StartDate = currentInterestDate,
                            EndDate = paymentEvent.Date,
                            Days = interestDaysThisPeriod,
                            Principal = principalBalance,
                            Interest = periodInterest
                        });
                    }
                    currentInterestDate = paymentEvent.Date;
                }
                
                // Calculate discount if payment is within discount period
                if (paymentEvent.Date <= discountDueDate)
                {
                    decimal paymentDiscount = Math.Min(paymentEvent.Amount, principalBalance) * (discountRate / 100m);
                    totalDiscount += paymentDiscount;
                    
                    result.DiscountDetails.Add(new DiscountDetail
                    {
                        PaymentDate = paymentEvent.Date,
                        PaymentAmount = paymentEvent.Amount,
                        DiscountEarned = paymentDiscount
                    });
                }
                
                // Reduce principal balance
                if(paymentEvent.Type == "Advance" && !paymentEvent.IsHistorical){
                    nonHistoricalAdvanceAmount += Math.Min(paymentEvent.Amount, principalBalance);
                }
                principalBalance -= Math.Min(paymentEvent.Amount, principalBalance);
                
                // Store payment allocation
                result.PaymentAllocations.Add(new PaymentAllocation
                {
                    Date = paymentEvent.Date,
                    Amount = paymentEvent.Amount,
                    Type = paymentEvent.Type,
                    RemainingBalance = Math.Max(0, principalBalance)
                });
                MessageBox.Show($"Debug: Payment Allocation: {paymentEvent.Type} {paymentEvent.Amount} on {paymentEvent.Date:dd-MM-yyyy}", "Debug: Timeline Calculation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Stop if fully paid
                if (principalBalance <= 0) break;
            }
            
            if (principalBalance > 0)
    {
        DateTime lastEventDate = sortedEvents.Any() 
            ? sortedEvents.Max(e => e.Date) 
            : dueDate;
        
        // DateTime paymentDate = paymentDate; // You need to pass this as parameter
        
        if (paymentDate > lastEventDate && paymentDate > dueDate)
        {
            DateTime interestStartDate = lastEventDate > dueDate ? lastEventDate : dueDate;
            int remainingDays = (paymentDate.Date - interestStartDate.Date).Days;
            
            if (remainingDays > 0)
            {
                decimal finalInterest = principalBalance * (interestRate / 100m) * (remainingDays / 365m);
                totalInterest += finalInterest;
                
                // Store this period for debugging
                result.InterestPeriods.Add(new InterestPeriod
                {
                    StartDate = interestStartDate,
                    EndDate = paymentDate,
                    Days = remainingDays,
                    Principal = principalBalance,
                    Interest = finalInterest
                });
            }
        }
    }
            result.TotalInterest = Math.Round(totalInterest, 2);
            result.TotalDiscount = Math.Round(totalDiscount, 2);
            result.Brokerage = Math.Round(brokerage, 2);
            result.RemainingBalance = Math.Max(0, principalBalance);
            result.NetAmount = Math.Round(result.RemainingBalance + result.TotalInterest - result.TotalDiscount , 2);
            MessageBox.Show($"Debug: Total Interest: {result.TotalInterest}, Total Discount: {result.TotalDiscount}, Brokerage: {result.Brokerage}, Remaining Balance: {result.RemainingBalance}, Net Amount: {result.NetAmount}, Non Historical Advance Amount: {nonHistoricalAdvanceAmount}", "Debug: Timeline Calculation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            return result;
        }

        // private PaymentAllocationResult AllocateNewPayments(
        //     Bill bill, 
        //     TimelineCalculationResult timelineResult,
        //     List<AdvancePayment> newAdvances, 
        //     decimal cashUsed)
        // {
        //     var result = new PaymentAllocationResult();
        //     result.AdvanceBreakdown = new List<AdvanceUtilization>();
            
        //     decimal totalAmountNeeded = timelineResult.NetAmount;
        //     decimal totalAllocated = 0;
            
        //     // Step 1: Allocate new advances first (in chronological order)
        //     foreach (var advance in newAdvances.OrderBy(a => a.PaymentDate))
        //     {
        //         decimal remainingNeeded = totalAmountNeeded - totalAllocated;
        //         if (remainingNeeded <= 0) break;
                
        //         decimal amountToUse = Math.Min(advance.Amount, remainingNeeded);
                
        //         if (amountToUse > 0)
        //         {
        //             result.AdvanceUsed += amountToUse;
        //             result.AdvanceBreakdown.Add(new AdvanceUtilization
        //             {
        //                 AdvanceID = advance.AdvanceID,
        //                 AmountUsed = amountToUse,
        //                 CreatedDate = advance.PaymentDate, // Use PaymentDate for calculations
        //                 UtilizedDate = DateTime.Now
        //             });
        //             totalAllocated += amountToUse;
        //         }
        //     }
            
        //     // Step 2: Use cash for any remaining amount
        //     decimal remainingAfterAdvances = totalAmountNeeded - result.AdvanceUsed;
        //     if (remainingAfterAdvances > 0 && cashUsed > 0)
        //     {
        //         result.CashUsed = Math.Min(cashUsed, remainingAfterAdvances);
        //     }
            
        //     // Step 3: Set final results
        //     result.InterestCharged = timelineResult.TotalInterest;
        //     result.DiscountEarned = timelineResult.TotalDiscount;
        //     result.BrokerageCharged = timelineResult.Brokerage;
        //     result.NetSettlement = Math.Round(result.AdvanceUsed + result.CashUsed, 2);
            
        //     return result;
        // }

        public class TimelineCalculationResult
        {
            public decimal TotalInterest { get; set; }
            public decimal TotalDiscount { get; set; }
            public decimal Brokerage { get; set; }
            public decimal RemainingBalance { get; set; }
            public decimal NetAmount { get; set; }
            public List<InterestPeriod> InterestPeriods { get; set; } = new List<InterestPeriod>();
            public List<DiscountDetail> DiscountDetails { get; set; } = new List<DiscountDetail>();
            public List<PaymentAllocation> PaymentAllocations { get; set; } = new List<PaymentAllocation>();
        }

        // public class InterestPeriod
        // {
        //     public DateTime StartDate { get; set; }
        //     public DateTime EndDate { get; set; }
        //     public int Days { get; set; }
        //     public decimal Principal { get; set; }
        //     public decimal Interest { get; set; }
        // }

        public class DiscountDetail
        {
            public DateTime PaymentDate { get; set; }
            public decimal PaymentAmount { get; set; }
            public decimal DiscountEarned { get; set; }
        }

        public class PaymentAllocation
        {
            public DateTime Date { get; set; }
            public decimal Amount { get; set; }
            public string Type { get; set; }
            public decimal RemainingBalance { get; set; }
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

            btnSave.Click += BtnSave_Click;
            btnClear.Click += BtnClear_Click;
            // Note: btnClose doesn't exist in the designer, removed the event handler

            // PaymentAllocation column is now read-only, no need for CellValueChanged event
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

            // Clear user-selected advance payments
            _userSelectedAdvancePayments.Clear();

            // Clear unused advance reversal tracking
            _shouldRevertUnusedAdvances = false;
            _unusedAdvancesToRevert.Clear();

            // Hide advance payment display
            // UpdateAdvancePaymentDisplay();
            
            // Hide Select Payments button
            btnSelectPayments.Visible = false;
            
            // Hide Generate Report button
            btnGenerateReport.Visible = false;

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
                
                // Show/hide Select Payments button based on whether advance payments are available
                UpdateSelectPaymentsButtonVisibility();
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
                
                // Show/hide Select Payments button based on whether advance payments are available
                UpdateSelectPaymentsButtonVisibility();
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

         private void ConsumeAdvances(List<AdvancePayment> advances, decimal 
        amountToConsume)
        {
            decimal remaining = amountToConsume;

            // Sort by payment date (FIFO - oldest first)
            var sortedAdvances = advances.OrderBy(a => a.PaymentDate).ToList();

            foreach (var advance in sortedAdvances)
            {
                if (remaining <= 0) break;

                decimal consumeFromThis = Math.Min(advance.Amount, remaining);
                advance.Amount -= consumeFromThis;
                remaining -= consumeFromThis;

                // Remove fully consumed advances
                if (advance.Amount <= 0)
                {
                    advances.Remove(advance);
                }
            }
        }

        private void ConsumeAdvancesFromCalculation(List<AdvancePayment> advances, decimal amountToConsume)
{
    decimal remaining = amountToConsume;
    var sortedAdvances = advances.OrderBy(a => a.PaymentDate).ToList();

    foreach (var advance in sortedAdvances)
    {
        if (remaining <= 0) break;

        decimal consumeFromThis = Math.Min(advance.Amount, remaining);
        advance.Amount -= consumeFromThis;
        remaining -= consumeFromThis;

        // Remove fully consumed advances
        if (advance.Amount <= 0.01m)
        {
            advances.Remove(advance);
        }
    }
}
       private void BtnCalculate_Click(object? sender, EventArgs e)
{
    if (!ValidateTerms(out int interestDays, out int discountDays, out decimal discountRate, out decimal interestRate, out decimal brokerageRate)) return;
    if (!DateTime.TryParseExact(txtPaymentDate.Text, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime paymentDate))
    {
        MessageBox.Show("Please enter a valid payment date in dd-mm-yyyy format.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    var billsToProcess = GetSelectedBillsFromGrid();
    if (!billsToProcess.Any())
    {
        MessageBox.Show("Please select one or more bills to calculate.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    // Clear previously allocated amounts in grid before new calculation
    foreach (var bill in _outstandingBills)
    {
        bill.PaymentAllocation = 0;
    }
    
    // Reset grid styles to remove previous highlighting
    ResetGridStyles();

    // Show advance payment status
    if (_userSelectedAdvancePayments.Any())
    {
        decimal totalSelected = _userSelectedAdvancePayments.Sum(ap => ap.Amount);
        MessageBox.Show($"Using {_userSelectedAdvancePayments.Count} selected advance payments (₹{totalSelected:N2}) for calculation.", 
            "Advance Payments Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    else
    {
        MessageBox.Show("No advance payments selected. Calculation will use cash only.", 
            "Cash Only Calculation", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // PaymentAllocation column is now read-only, no need for CellValueChanged event
    ResetGridStyles();

    int? partyId = cmbParty.SelectedValue as int?;
    int? brokerId = cmbBroker.SelectedValue as int?;
    
    // Use user-selected advance payments if any, otherwise use empty list
    var availableAdvances = _userSelectedAdvancePayments.Any() 
        ? _userSelectedAdvancePayments 
        : new List<AdvancePayment>();
    
    // Use the new settlement calculation logic
    var settlementResult = CalculateSettlementRequirement(
        billsToProcess, availableAdvances, paymentDate,
            interestDays, interestRate, discountDays, discountRate, brokerageRate);
                _lastSettlementResult = settlementResult;
            
            // Set PartyID and BrokerID from form selections
            _lastSettlementResult.PartyID = cmbParty.SelectedValue as int?;
            _lastSettlementResult.BrokerID = cmbBroker.SelectedValue as int?;
            
            // DEBUG: Show calculation settlement result
            var debugMessage = $"DEBUG - Payment Calculation Results:\n" +
                             $"Party ID: {_lastSettlementResult.PartyID}\n" +
                             $"Broker ID: {_lastSettlementResult.BrokerID}\n" +
                             $"Total Cash Needed: {_lastSettlementResult.TotalCashNeeded:C}\n" +
                             $"Total Advance Used: {_lastSettlementResult.TotalAdvanceUsed:C}\n" +
                             $"Total Amount Due: {_lastSettlementResult.TotalAmountDue:C}\n" +
                             $"Total Interest: {_lastSettlementResult.TotalInterest:C}\n" +
                             $"Total Discount: {_lastSettlementResult.TotalDiscount:C}\n" +
                             $"Total Brokerage: {_lastSettlementResult.TotalBrokerage:C}\n\n" +
                             $"Settlement Breakdown:\n";
            
            if (_lastSettlementResult.BillBreakdowns != null)
            {
                foreach (var breakdown in _lastSettlementResult.BillBreakdowns)
                {
                    debugMessage += $"Bill {breakdown.BillNo}: Amount Due={breakdown.AmountDue:C}, " +
                                  $"Interest={breakdown.Interest:C}, " +
                                  $"Discount={breakdown.Discount:C}, " +
                                  $"Brokerage={breakdown.Brokerage:C}, " +
                                  $"Advance Used={breakdown.AdvanceUsed:C}, " +
                                  $"Cash Needed={breakdown.CashNeeded:C}\n";
                    
                    if (breakdown.AdvanceUtilizations != null && breakdown.AdvanceUtilizations.Any())
                    {
                        debugMessage += $"  Advance Utilizations: ";
                        foreach (var util in breakdown.AdvanceUtilizations)
                        {
                            debugMessage += $"AdvanceID={util.AdvanceID}, Amount={util.AmountUsed:C}; ";
                        }
                        debugMessage += "\n";
                    }
                }
            }
            else
            {
                debugMessage += "No settlement breakdown available\n";
            }
            
            System.Windows.Forms.MessageBox.Show(debugMessage, "DEBUG - Payment Calculation", 
                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);

    // Update grid with settlement results
    foreach (var billBreakdown in settlementResult.BillBreakdowns)
    {
        var billVm = billsToProcess.FirstOrDefault(b => b.BillID == billBreakdown.BillID);
        if (billVm != null)
        {
        // Set calculated amount in grid - this shows what the bill needs
            billVm.PaymentAllocation = billBreakdown.AdvanceUsed + billBreakdown.CashNeeded;

        // Highlight the row (different color to show this is calculation, not allocation)
        foreach (DataGridViewRow row in dgvOutstandingBills.Rows)
        {
            if ((row.DataBoundItem as BillViewModel)?.BillID == billVm.BillID)
            {
                row.DefaultCellStyle.BackColor = Color.LightBlue; // Blue for calculation
                break;
                }
            }
        }
    }

    // PaymentAllocation column is now read-only, no need for CellValueChanged event
    dgvOutstandingBills.Refresh();

    // Update display with settlement calculation results
    txtPaymentAmount.Text = Math.Round(settlementResult.TotalCashNeeded).ToString("F0"); // Show cash needed
    lblDiscountValue.Text = $"Discount Available: ₹{Math.Round(settlementResult.TotalDiscount):N0}";
    lblInterestValue.Text = $"Interest Due: ₹{Math.Round(settlementResult.TotalInterest):N0}";
    lblBrokerageValue.Text = $"Brokerage: ₹{Math.Round(settlementResult.TotalBrokerage):N0}";
    lblFinalAmount.Text = $"Total Due: ₹{Math.Round(settlementResult.TotalAmountDue):N0}";

                // Show settlement summary
            ShowSettlementSummary(settlementResult);
            
            // Show information about advance payments used
            if (_userSelectedAdvancePayments.Any())
            {
                decimal totalSelected = _userSelectedAdvancePayments.Sum(ap => ap.Amount);
                decimal totalUsed = settlementResult.TotalAdvanceUsed;
                decimal unused = totalSelected - totalUsed;
                
                string advanceInfo = $"=== ADVANCE PAYMENT USAGE ===\n\n" +
                                   $"Total Selected: ₹{totalSelected:N2}\n" +
                                   $"Total Used: ₹{totalUsed:N2}\n" +
                                   $"Unused: ₹{unused:N2}";
                
                MessageBox.Show(advanceInfo, "Advance Payment Usage", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
            // Show the Generate Report button after successful calculation
            btnGenerateReport.Visible = true;
}

// Add this helper method
private void ShowCalculationSummary(PaymentCalculationSummary calculation, string billNo)
{
    var details = $"=== CALCULATION SUMMARY FOR BILL {billNo} ===\n\n" +
                  $"Total Amount Due: ₹{calculation.TotalAmountDue:N2}\n" +
                  $"Available Advance: ₹{calculation.AdvanceAvailable:N2}\n" +
                  $"Advance Usable: ₹{calculation.AdvanceUsable:N2}\n" +
                  $"Cash Required: ₹{calculation.CashRequired:N2}\n" +
                  $"Interest Charged: ₹{calculation.InterestCharged:N2}\n" +
                  $"Discount Available: ₹{calculation.DiscountEarned:N2}\n" +
                  $"Can Fully Settle: {(calculation.CanFullySettle ? "Yes" : "No")}";
    
    MessageBox.Show(details, $"Calculation - Bill {billNo}", MessageBoxButtons.OK, MessageBoxIcon.Information);
}

        /// <summary>
        /// Shows settlement summary for all bills
        /// </summary>
        private void ShowSettlementSummary(SettlementCalculationResult settlementResult)
        {
            string advanceStatus = _userSelectedAdvancePayments.Any() 
                ? $"Advance Payments: {_userSelectedAdvancePayments.Count} selected (₹{_userSelectedAdvancePayments.Sum(ap => ap.Amount):N2})"
                : "Advance Payments: None selected (calculation uses cash only)";
            
            var summary = $"=== SETTLEMENT CALCULATION SUMMARY ===\n\n" +
                          $"{advanceStatus}\n\n" +
                          $"Total Amount Due: ₹{settlementResult.TotalAmountDue:N2}\n" +
                          $"Total Advance Used: ₹{settlementResult.TotalAdvanceUsed:N2}\n" +
                          $"Total Cash Needed: ₹{settlementResult.TotalCashNeeded:N2}\n" +
                          $"Total Interest: ₹{settlementResult.TotalInterest:N2}\n" +
                          $"Total Discount: ₹{settlementResult.TotalDiscount:N2}\n" +
                          $"Total Brokerage: ₹{settlementResult.TotalBrokerage:N2}\n" +
                          $"Unused Advance: ₹{settlementResult.UnusedAdvance:N2}\n" +
                          $"Can Fully Settle: {(settlementResult.CanFullySettle ? "Yes" : "No")}";
            
            MessageBox.Show(summary, "Settlement Calculation Summary", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Opens the advance payment selection form
        /// </summary>
        private void ShowAdvancePaymentSelectionForm()
        {
            try
            {
            int? partyId = cmbParty.SelectedValue as int?;
            int? brokerId = cmbBroker.SelectedValue as int?;

                if (!partyId.HasValue && !brokerId.HasValue)
            {
                    MessageBox.Show("Please select either a party or broker first.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

                using (var selectionForm = new AdvancePaymentSelectionForm(brokerId, partyId))
                {
                    if (selectionForm.ShowDialog() == DialogResult.OK)
                    {
                        _userSelectedAdvancePayments = selectionForm.SelectedAdvancePayments.ToList();
                        
                        // Show confirmation of selected payments
                        decimal totalSelected = _userSelectedAdvancePayments.Sum(ap => ap.Amount);
                        string message = $"Selected {_userSelectedAdvancePayments.Count} advance payment(s) with total amount: ₹{totalSelected:N2}";
                        
                        if (brokerId.HasValue)
                        {
                            message += $"\nBroker ID: {brokerId}";
                        }
                        if (partyId.HasValue)
                        {
                            message += $"\nParty ID: {partyId}";
                        }
                        
                        MessageBox.Show(message, "Advance Payments Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        // Update the UI to show selected payments are available
                        UpdateSelectedAdvancePaymentsDisplay();
                        
                        // Update button to show that payments are selected
                        btnSelectPayments.Text = $"Selected ({_userSelectedAdvancePayments.Count})";
                        btnSelectPayments.BackColor = Color.LightGreen;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening advance payment selection: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Updates the display to show selected advance payments
        /// </summary>
        private void UpdateSelectedAdvancePaymentsDisplay()
        {
            if (_userSelectedAdvancePayments.Any())
            {
                decimal totalSelected = _userSelectedAdvancePayments.Sum(ap => ap.Amount);
                // You can add a label or update existing UI elements to show this information
                // For now, we'll just show it in a tooltip or status message
                System.Diagnostics.Debug.WriteLine($"User selected {_userSelectedAdvancePayments.Count} advance payments with total: ₹{totalSelected:N2}");
            }
        }

        /// <summary>
        /// Clears all selected advance payments and updates the button state
        /// </summary>
        public void ClearSelectedAdvancePayments()
        {
            _userSelectedAdvancePayments.Clear();
            UpdateSelectPaymentsButtonVisibility();
        }

        /// <summary>
        /// Updates the visibility of the Select Payments button based on whether advance payments are available
        /// </summary>
        private void UpdateSelectPaymentsButtonVisibility()
        {
            try
            {
                bool hasAdvancePayments = HasAdvancePaymentsAvailable();
                btnSelectPayments.Visible = hasAdvancePayments;
                
                if (_userSelectedAdvancePayments.Any())
                {
                    // Show selected payments count
                    btnSelectPayments.Text = $"Selected ({_userSelectedAdvancePayments.Count})";
                    btnSelectPayments.BackColor = Color.LightGreen;
                }
                else if (hasAdvancePayments)
                {
                    btnSelectPayments.Text = "Select Payments (None Selected)";
                    btnSelectPayments.BackColor = Color.LightBlue;
                }
                else
                {
                    btnSelectPayments.Text = "No Payments Available";
                    btnSelectPayments.BackColor = Color.LightGray;
                }
        }
        catch (Exception ex)
        {
                System.Diagnostics.Debug.WriteLine($"Error updating Select Payments button visibility: {ex.Message}");
                btnSelectPayments.Visible = false;
            }
        }

        /// <summary>
        /// Checks if advance payments are available for the selected broker/party
        /// </summary>
        public bool HasAdvancePaymentsAvailable()
        {
            int? partyId = cmbParty.SelectedValue as int?;
            int? brokerId = cmbBroker.SelectedValue as int?;
            
            if (!partyId.HasValue && !brokerId.HasValue)
                return false;

            try
            {
                var availableAdvances = AdvancePaymentService.GetAvailableAdvancePayments(partyId, brokerId);
                return availableAdvances != null && availableAdvances.Any();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Public method to open advance payment selection (can be called from designer)
        /// </summary>
        public void OpenAdvancePaymentSelection()
        {
            ShowAdvancePaymentSelectionForm();
        }

        /// <summary>
        /// Event handler for the Select Payments button click
        /// </summary>
        private void BtnSelectPayments_Click(object? sender, EventArgs e)
        {
            OpenAdvancePaymentSelection();
        }

        /// <summary>
        /// Event handler for the Generate Report button click
        /// </summary>
        private void BtnGenerateReport_Click(object? sender, EventArgs e)
        {
            GeneratePaymentReport();
        }

        /// <summary>
        /// Generates and displays the payment report
        /// </summary>
        private void GeneratePaymentReport()
        {
            try
            {
                if (_lastSettlementResult == null)
                {
                    MessageBox.Show("No calculation data available. Please calculate first.", "No Data", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create report data from current calculation
                var reportData = CreatePaymentReportData();
                
                // Show the report form
                var reportForm = new PaymentReportForm(reportData);
                reportForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", "Report Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Creates PaymentReportData from current settlement calculation
        /// </summary>
        private PaymentReportData CreatePaymentReportData()
        {
            var reportData = new PaymentReportData
            {
                // Header Information
                ReportTitle = "Payment Settlement Report",
                ReportDate = DateTime.Now,
                
                // Payment Information
                PaymentID = 0, // Will be set after save
                PaymentDate = _lastSettlementResult.PaymentDate,
                PaymentMethod = cmbPaymentMethod.Text ?? "Cash",
                Reference = txtReference.Text ?? "",
                TotalPaymentAmount = _lastSettlementResult.TotalCashNeeded,
                
                // Party/Broker Information
                PartyID = _lastSettlementResult.PartyID,
                BrokerID = _lastSettlementResult.BrokerID,
                
                // Settlement Summary
                TotalAmountDue = _lastSettlementResult.TotalAmountDue,
                TotalAdvanceUsed = _lastSettlementResult.TotalAdvanceUsed,
                TotalCashNeeded = _lastSettlementResult.TotalCashNeeded,
                TotalInterest = _lastSettlementResult.TotalInterest,
                TotalDiscount = _lastSettlementResult.TotalDiscount,
                TotalBrokerage = _lastSettlementResult.TotalBrokerage,
                UnusedAdvance = _lastSettlementResult.UnusedAdvance,
                CanFullySettle = _lastSettlementResult.CanFullySettle,
                
                // Cheque Details
                ChequeAmountFirm1 = decimal.TryParse(txtChequeAmountFirm1.Text, out decimal firm1) ? firm1 : 0,
                ChequeAmountFirm2 = decimal.TryParse(txtChequeAmountFirm2.Text, out decimal firm2) ? firm2 : 0,
                TotalChequeAmount = decimal.TryParse(txtChequeAmountFirm1.Text, out decimal f1) && 
                                   decimal.TryParse(txtChequeAmountFirm2.Text, out decimal f2) ? f1 + f2 : 0,
                
                // Generated By
                GeneratedBy = "System User", // Could be enhanced to get actual user
                GeneratedAt = DateTime.Now
            };

            // Get party and broker names
            if (reportData.PartyID.HasValue)
            {
                var party = _parties.FirstOrDefault(p => p.PartyID == reportData.PartyID.Value);
                if (party != null)
                    reportData.PartyName = party.PartyName;
            }

            if (reportData.BrokerID.HasValue)
            {
                var broker = _brokers.FirstOrDefault(b => b.BrokerID == reportData.BrokerID.Value);
                if (broker != null)
                    reportData.BrokerName = broker.BrokerName;
            }

            // Convert bill breakdowns to report details
            foreach (var breakdown in _lastSettlementResult.BillBreakdowns)
            {
                var billDetail = new BillReportDetail
                {
                    BillID = breakdown.BillID,
                    BillNo = breakdown.BillNo,
                    BillDate = DateTime.Now, // Could be enhanced to get actual bill date
                    OriginalAmount = breakdown.AmountDue + breakdown.Interest - breakdown.Discount,
                    BalanceDue = breakdown.AmountDue,
                    AmountPaid = breakdown.AdvanceUsed + breakdown.CashNeeded,
                    InterestCharged = breakdown.Interest,
                    DiscountEarned = breakdown.Discount,
                    Brokerage = breakdown.Brokerage,
                    AdvanceUsed = breakdown.AdvanceUsed,
                    CashUsed = breakdown.CashNeeded,
                    Status = breakdown.CashNeeded == 0 ? "Paid" : "Partial"
                };

                // Convert interest periods
                foreach (var period in breakdown.InterestPeriods)
                {
                    billDetail.InterestPeriods.Add(new InterestPeriodReport
                    {
                        StartDate = period.StartDate,
                        EndDate = period.EndDate,
                        Days = period.Days,
                        Principal = period.Principal,
                        InterestRate = 0, // Could be enhanced to get actual rate
                        InterestAmount = period.Interest,
                        Description = $"Interest for {period.Days} days"
                    });
                }

                // Convert advance utilizations
                foreach (var util in breakdown.AdvanceUtilizations)
                {
                    billDetail.AdvanceUtilizations.Add(new Models.AdvanceUtilizationDetail
                    {
                        AdvanceID = util.AdvanceID,
                        AdvanceDate = util.AdvanceDate,
                        AmountUsed = util.AmountUsed,
                        PaymentMethod = util.PaymentMethod,
                        Reference = util.Reference
                    });
                }

                reportData.BillDetails.Add(billDetail);
            }

            // Convert advance utilizations
            foreach (var advance in _userSelectedAdvancePayments)
            {
                var advanceUtil = new AdvanceUtilizationReport
                {
                    AdvanceID = advance.AdvanceID,
                    AdvanceDate = advance.PaymentDate,
                    OriginalAmount = advance.Amount,
                    AmountUsed = 0, // Could be enhanced to calculate actual usage
                    RemainingAmount = advance.Amount,
                    PaymentMethod = advance.PaymentMethod,
                    Reference = advance.Reference,
                    Status = "Available"
                };

                // Find which bills use this advance
                foreach (var breakdown in _lastSettlementResult.BillBreakdowns)
                {
                    var util = breakdown.AdvanceUtilizations.FirstOrDefault(u => u.AdvanceID == advance.AdvanceID);
                    if (util != null)
                    {
                        advanceUtil.AmountUsed += util.AmountUsed;
                        advanceUtil.RemainingAmount -= util.AmountUsed;
                        advanceUtil.UsedForBills.Add(breakdown.BillNo);
                    }
                }

                if (advanceUtil.AmountUsed > 0)
                {
                    advanceUtil.Status = advanceUtil.RemainingAmount > 0 ? "Partial" : "Used";
                }

                reportData.AdvanceUtilizations.Add(advanceUtil);
            }

            // Add payment terms
            reportData.PaymentTerms = new PaymentTermsReport
            {
                InterestDays = int.TryParse(txtInterestDays.Text, out int interestDays) ? interestDays : 0,
                InterestRate = decimal.TryParse(txtInterestRate.Text, out decimal interestRate) ? interestRate : 0,
                DiscountDays = int.TryParse(txtDiscountDays.Text, out int discountDays) ? discountDays : 0,
                DiscountRate = decimal.TryParse(txtDiscountRate.Text, out decimal discountRate) ? discountRate : 0,
                BrokerageRate = decimal.TryParse(txtBrokerageRate.Text, out decimal brokerageRate) ? brokerageRate : 0,
                TermsSource = "Manual Entry"
            };

            return reportData;
        }

        /// <summary>
        /// Gets details of unused advance payments for reversal dialog
        /// </summary>
        private List<UnusedAdvanceDetail> GetUnusedAdvanceDetails()
        {
            var unusedAdvances = new List<UnusedAdvanceDetail>();
            
            if (_lastSettlementResult == null || _userSelectedAdvancePayments == null || !_userSelectedAdvancePayments.Any())
                return unusedAdvances;

            // Get all advance utilizations from the settlement result
            var allUtilizations = new List<AdvanceUtilizationDetail>();
            foreach (var breakdown in _lastSettlementResult.BillBreakdowns)
            {
                if (breakdown.AdvanceUtilizations != null)
                {
                    allUtilizations.AddRange(breakdown.AdvanceUtilizations);
                }
            }

            // Group utilizations by AdvanceID to get total used amount
            var utilizationTotals = allUtilizations
                .GroupBy(u => u.AdvanceID)
                .ToDictionary(g => g.Key, g => g.Sum(u => u.AmountUsed));

            // Check each selected advance payment for unused amounts
            foreach (var advance in _userSelectedAdvancePayments)
            {
                decimal usedAmount = utilizationTotals.ContainsKey(advance.AdvanceID) 
                    ? utilizationTotals[advance.AdvanceID] 
                    : 0;
                
                decimal unusedAmount = advance.Amount - usedAmount;
                
                if (unusedAmount > 0.01m)
                {
                    // Get broker name
                    string brokerName = "Unknown Broker";
                    if (advance.BrokerID.HasValue)
                    {
                        var broker = _brokers.FirstOrDefault(b => b.BrokerID == advance.BrokerID.Value);
                        brokerName = broker?.BrokerName ?? "Unknown Broker";
                    }

                    unusedAdvances.Add(new UnusedAdvanceDetail
                    {
                        AdvanceID = advance.AdvanceID,
                        BrokerName = brokerName,
                        OriginalAmount = advance.Amount,
                        UsedAmount = usedAmount,
                        UnusedAmount = unusedAmount,
                        PaymentDate = advance.PaymentDate,
                        Reference = advance.Reference ?? "",
                        BrokerID = advance.BrokerID ?? 0,
                        PartyID = advance.PartyID ?? 0
                    });
                }
            }

            return unusedAdvances;
        }



        // PaymentAllocation column is now read-only, no need for CellValueChanged event handler

        private void UpdateTotalPaymentFromGrid()
        {
            decimal totalAllocated = _outstandingBills.Sum(b => b.PaymentAllocation);
            
            // Only update payment amount if user hasn't entered a larger amount (preserve excess)
            if (decimal.TryParse(txtPaymentAmount.Text, out decimal currentPaymentAmount))
            {
                if (totalAllocated > currentPaymentAmount)
                {
                    // If allocated amount is greater than what user entered, update the field
            // txtPaymentAmount.Text = Math.Round(totalAllocated).ToString("F0");
                }
                // If user entered more than allocated (excess), keep the original amount and show excess in advance display
                else if (currentPaymentAmount > totalAllocated)
                {
                    // Refresh advance display to show the potential excess as advance
                    // UpdateAdvancePaymentDisplay();
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

            // Check for unused advance payments and ask user if they want to revert them
            if (_lastSettlementResult != null && _lastSettlementResult.UnusedAdvance > 0.01m)
            {
                var unusedAdvances = GetUnusedAdvanceDetails();
                if (unusedAdvances.Any())
                {
                    using (var reversalForm = new UnusedAdvanceReversalForm(unusedAdvances))
                    {
                        var dialogResult = reversalForm.ShowDialog();
                        if (dialogResult == DialogResult.Cancel)
                        {
                            return; // User cancelled the save operation
                        }
                        
                        // Store the user's decision for use during save
                        _shouldRevertUnusedAdvances = reversalForm.ShouldRevertUnusedAdvances;
                        _unusedAdvancesToRevert = reversalForm.UnusedAdvances;
                    }
                }
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
                IsAdvancePayment = false,
                AllocationSummary = _currentAllocationSummary
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
                    (paymentId, totalAvailableAdvance) = SavePaymentInBackground();
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
                    // UpdateAdvancePaymentDisplay();
                    
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
            public PaymentAllocationSummary AllocationSummary { get; set; }
        }

        /// <summary>
        /// Saves payment entry with all related transactions and advance utilizations
        /// </summary>
        private (int, decimal) SavePaymentInBackground()
        {
            // Validate required data before proceeding
            if (_lastSettlementResult == null)
            {
                throw new InvalidOperationException("No settlement calculation available. Please calculate first.");
            }

            using (var conn = DatabaseManager.GetConnection())
            {
                conn.Open();
                var dbTransaction = conn.BeginTransaction();
                
                try
                {
                    // STEP 1: Save the main payment record (cash payment if any)
                    int? cashPaymentId = null;
                    if (_lastSettlementResult.TotalCashNeeded > 0)
                    {
                        // Get form values with null checks
                        var partyId = cmbParty.SelectedValue as int? ?? 0;
                        var brokerId = cmbBroker.SelectedValue as int? ?? 0;
                        var paymentMethod = cmbPaymentMethod.Text ?? "Cash";
                        var reference = txtReference.Text ?? "";

                        // Handle cash and cheque allocation based on payment method
                        decimal chequeAmountFirm1 = 0;
                        decimal chequeAmountFirm2 = 0;
                        
                        if (paymentMethod.Equals("Cheque", StringComparison.OrdinalIgnoreCase))
                        {
                            // For cheque payments, use the amounts from the form fields
                            if (!decimal.TryParse(txtChequeAmountFirm1.Text, out chequeAmountFirm1))
                                chequeAmountFirm1 = 0;
                            if (!decimal.TryParse(txtChequeAmountFirm2.Text, out chequeAmountFirm2))
                                chequeAmountFirm2 = 0;
                            
                            // Validate that the sum matches the cash needed
                            decimal totalChequeAmount = chequeAmountFirm1 + chequeAmountFirm2;
                            if (Math.Round(totalChequeAmount) != Math.Round(_lastSettlementResult.TotalCashNeeded))
                            {
                                throw new InvalidOperationException($"Cheque amounts (₹{totalChequeAmount:N2}) do not match cash needed (₹{_lastSettlementResult.TotalCashNeeded:N2}). Please adjust the cheque amounts.");
                            }
                        }
                        else
                        {
                            // For cash payments, the full amount goes to cash (no cheque allocation)
                            chequeAmountFirm1 = 0;
                            chequeAmountFirm2 = 0;
                        }

                        // Create advance payment record for cash payment
                        var cashPayment = new AdvancePayment
                        {
                            PartyID = partyId,
                            BrokerID = brokerId,
                            PaymentDate = _lastSettlementResult.PaymentDate,
                            Amount = _lastSettlementResult.TotalCashNeeded,
                            PaymentMethod = paymentMethod,
                            Reference = reference,
                            ChequeAmountFirm1 = chequeAmountFirm1,
                            ChequeAmountFirm2 = chequeAmountFirm2,
                            CompanyID = 1,
                            CreatedDate = DateTime.Now
                        };

                        // Save cash payment directly using the existing transaction
                        string insertCashPaymentSql = @"
                            INSERT INTO AdvancePayments (PartyID, BrokerID, PaymentDate, Amount, PaymentMethod, Reference, ChequeAmountFirm1, ChequeAmountFirm2, CompanyID, CreatedDate)
                            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                        var cashPaymentParams = new OleDbParameter[]
                        {
                            new OleDbParameter("PartyID", OleDbType.Integer) { Value = partyId },
                            new OleDbParameter("BrokerID", OleDbType.Integer) { Value = brokerId },
                            new OleDbParameter("PaymentDate", OleDbType.Date) { Value = cashPayment.PaymentDate },
                            new OleDbParameter("Amount", OleDbType.Currency) { Value = cashPayment.Amount },
                            new OleDbParameter("PaymentMethod", OleDbType.VarChar, 50) { Value = cashPayment.PaymentMethod },
                            new OleDbParameter("Reference", OleDbType.VarChar, 255) { Value = cashPayment.Reference },
                            new OleDbParameter("ChequeAmountFirm1", OleDbType.Currency) { Value = cashPayment.ChequeAmountFirm1 },
                            new OleDbParameter("ChequeAmountFirm2", OleDbType.Currency) { Value = cashPayment.ChequeAmountFirm2 },
                            new OleDbParameter("CompanyID", OleDbType.Integer) { Value = cashPayment.CompanyID },
                            new OleDbParameter("CreatedDate", OleDbType.Date) { Value = cashPayment.CreatedDate }
                        };

                        using (var insertCmd = new OleDbCommand(insertCashPaymentSql, conn, dbTransaction))
                        {
                            insertCmd.Parameters.AddRange(cashPaymentParams);
                            insertCmd.ExecuteNonQuery();
                        }

                        // Get the ID of the saved cash payment
                        string getCashPaymentIdSql = "SELECT @@IDENTITY";
                        using (var cmd = new OleDbCommand(getCashPaymentIdSql, conn, dbTransaction))
                        {
                            cashPaymentId = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                        
                        // IMMEDIATELY UTILIZE THE CASH PAYMENT
                        var cashPaymentUtilization = new AdvanceUtilization
                        {
                            AdvanceID = cashPaymentId.Value,
                            PaymentID = 0,
                            AmountUsed = _lastSettlementResult.TotalCashNeeded,
                            UtilizedDate = DateTime.Now,
                            PartyID = partyId,
                            BrokerID = brokerId,
                            CompanyID = 1
                        };
                        
                        // Save the utilization record
                        AdvanceUtilizationService.AddUtilization(cashPaymentUtilization, conn, dbTransaction);
                    }

                    // STEP 2: Save all ledger transactions
                    if (_lastSettlementResult.BillBreakdowns == null || !_lastSettlementResult.BillBreakdowns.Any())
                    {
                        throw new InvalidOperationException("No bill breakdowns available for saving.");
                    }

                    foreach (var breakdown in _lastSettlementResult.BillBreakdowns)
                    {
                        // Get form values with null checks (reuse from above)
                        var partyId = cmbParty.SelectedValue as int? ?? 0;
                        var brokerId = cmbBroker.SelectedValue as int? ?? 0;
                        var paymentMethod = cmbPaymentMethod.Text ?? "Cash";
                        var reference = txtReference.Text ?? "";

                        // Save interest transaction
                        if (breakdown.Discount > 0)
                        {
                            var discountTransaction = new Transaction
                            {
                                PartyID = partyId,
                                BillID = breakdown.BillID,
                                PaymentID = cashPaymentId ?? 0,
                                TransactionDate = _lastSettlementResult.PaymentDate,
                                TransactionType = "Discount",
                                Description = $"Discount on Bill {breakdown.BillNo}",
                                DebitAmount = 0,
                                CreditAmount = breakdown.Discount,
                                PaymentMethod = paymentMethod,
                                Reference = reference,
                                CompanyID = 1
                            };
                            LedgerService.AddTransaction(discountTransaction, conn, dbTransaction);
                        }
                        if (breakdown.Interest > 0)
                        {
                            var interestTransaction = new Transaction
                            {
                                PartyID = partyId,
                                BillID = breakdown.BillID,
                                PaymentID = cashPaymentId ?? 0,
                                TransactionDate = _lastSettlementResult.PaymentDate,
                                TransactionType = "Interest",
                                Description = $"Interest on Bill {breakdown.BillNo}",
                                DebitAmount = breakdown.Interest,
                                CreditAmount = 0,
                                PaymentMethod = paymentMethod,
                                Reference = reference,
                                CompanyID = 1
                            };
                            LedgerService.AddTransaction(interestTransaction, conn, dbTransaction);
                        }

                        // Save brokerage transaction
                        if (breakdown.Brokerage > 0)
                        {
                            var brokerageTransaction = new Transaction
                            {
                                PartyID = partyId,
                                BillID = breakdown.BillID,
                                PaymentID = cashPaymentId ?? 0,
                                TransactionDate = _lastSettlementResult.PaymentDate,
                                TransactionType = "Brokerage",
                                Description = $"Brokerage on Bill {breakdown.BillNo}",
                                DebitAmount = 0,
                                CreditAmount = breakdown.Brokerage,
                                PaymentMethod = paymentMethod,
                                Reference = reference,
                                CompanyID = 1
                            };
                            LedgerService.AddTransaction(brokerageTransaction, conn, dbTransaction);
                        }

                        // Save payment transaction (cash needed)
                        if (breakdown.CashNeeded > 0)
                        {
                            var paymentTransaction = new Transaction
                            {
                                PartyID = partyId,
                                BillID = breakdown.BillID,
                                PaymentID = cashPaymentId ?? 0,
                                TransactionDate = _lastSettlementResult.PaymentDate,
                                TransactionType = "Payment",
                                Description = $"Payment against Bill {breakdown.BillNo}",
                                DebitAmount = 0,
                                CreditAmount = breakdown.CashNeeded,
                                PaymentMethod = paymentMethod,
                                Reference = reference,
                                CompanyID = 1
                            };
                            LedgerService.AddTransaction(paymentTransaction, conn, dbTransaction);
                        }

                        // Save advance utilization records
                        if (breakdown.AdvanceUtilizations != null && breakdown.AdvanceUtilizations.Any())
                        {
                            foreach (var utilization in breakdown.AdvanceUtilizations)
                            {
                                // Save utilization record
                                var advanceUtilization = new AdvanceUtilization
                                {
                                    AdvanceID = utilization.AdvanceID,
                                    PaymentID = cashPaymentId ?? 0,
                                    AmountUsed = utilization.AmountUsed,
                                    UtilizedDate = _lastSettlementResult.PaymentDate,
                                    PartyID = partyId,
                                    BrokerID = brokerId,
                                    CompanyID = 1
                                };
                                AdvanceUtilizationService.AddUtilization(advanceUtilization, conn, dbTransaction);

                                // ALSO save advance utilization as a ledger transaction
                                var advancePaymentTransaction = new Transaction
                                {
                                    PartyID = partyId,
                                    BillID = breakdown.BillID,
                                    PaymentID = utilization.AdvanceID, // Use the advance payment ID
                                    TransactionDate = utilization.AdvanceDate,
                                    TransactionType = "Payment",
                                    Description = $"Payment utilized for Bill {breakdown.BillNo}",
                                    DebitAmount = 0,
                                    CreditAmount = utilization.AmountUsed,
                                    PaymentMethod = "Advance Payment",
                                    Reference = $"Advance ID: {utilization.AdvanceID}",
                                    CompanyID = 1
                                };
                                LedgerService.AddTransaction(advancePaymentTransaction, conn, dbTransaction);
                            }
                        }
                    }

                    // STEP 3: Update bill statuses to "Paid"
                    var paidBills = _lastSettlementResult.BillBreakdowns
                        .Select(b => new BillViewModel { BillID = b.BillID, BillNo = b.BillNo })
                        .ToList();
                    UpdateBillStatuses(paidBills);

                    // STEP 4: Handle unused advance reversals if user requested
                    if (_shouldRevertUnusedAdvances && _unusedAdvancesToRevert.Any())
                    {
                        try
                        {
                            ProcessUnusedAdvanceReversals(_unusedAdvancesToRevert, conn, dbTransaction);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Failed to process unused advance reversals: {ex.Message}", ex);
                        }
                    }

                    dbTransaction.Commit();
                    
                    // Return the cash payment ID and total amount
                    return (cashPaymentId ?? 0, _lastSettlementResult.TotalCashNeeded);
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    MessageBox.Show($"Error saving payment: {ex.Message}", "Save Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw;
                }
            }
        }

        /// <summary>
        /// Saves advance utilization records (which advances were used for which bills)
        /// </summary>



        private void HandleCashChequeDistribution(AdvancePayment cashAdvancePayment)
        {
            if (decimal.TryParse(txtChequeAmountFirm1.Text, out decimal firm1Amount) && 
                decimal.TryParse(txtChequeAmountFirm2.Text, out decimal firm2Amount))
            {
                decimal totalCheque = firm1Amount + firm2Amount;
                if (totalCheque > 0)
                {
                    decimal firm1Ratio = firm1Amount / totalCheque;
                    decimal firm2Ratio = firm2Amount / totalCheque;
                    
                    cashAdvancePayment.ChequeAmountFirm1 = Math.Round(cashAdvancePayment.Amount * firm1Ratio);
                    cashAdvancePayment.ChequeAmountFirm2 = Math.Round(cashAdvancePayment.Amount * firm2Ratio);
                }
            }
        }
        private void SaveLedgerTransactions(OleDbConnection conn, OleDbTransaction transaction, int? cashAdvanceId)
        {
            if (_lastSettlementResult == null)
            {
                throw new Exception("No settlement calculation available. Please calculate first.");
            }
            
            // Get all bill details in one query for efficiency
            var billIds = _outstandingBills.Where(b => b.PaymentAllocation > 0).Select(b => b.BillID).ToList();
            var billDetails = BillService.GetBillsByIDs(billIds);
            
            foreach (var breakdown in _lastSettlementResult.BillBreakdowns)
            {
                var fullBill = billDetails.FirstOrDefault(b => b.BillID == breakdown.BillID);
                if (fullBill == null) continue;
                
                // Save interest transaction if applicable
                if (breakdown.Interest > 0)
                {
                    var interestTransaction = new Transaction
                    {
                        PaymentID = cashAdvanceId, // Use cash advance ID if cash was needed
                        PartyID = fullBill.PartyID,
                        BillID = fullBill.BillID,
                        TransactionDate = DateTime.Parse(txtPaymentDate.Text),
                        TransactionType = "Interest",
                        Description = $"Interest on Bill No: {fullBill.BillNo}",
                        DebitAmount = Math.Round(breakdown.Interest),
                        UserID = 1,
                        CompanyID = 1
                    };
                    LedgerService.AddTransaction(interestTransaction, conn, transaction);
                }
                
                // Save brokerage transaction if applicable
                if (breakdown.Brokerage > 0)
                {
                    var brokerageTransaction = new Transaction
                    {
                        PaymentID = cashAdvanceId, // Use cash advance ID if cash was needed
                        PartyID = fullBill.PartyID,
                        BillID = fullBill.BillID,
                        TransactionDate = DateTime.Parse(txtPaymentDate.Text),
                        TransactionType = "Brokerage",
                        Description = $"Brokerage on Bill No: {fullBill.BillNo}",
                        CreditAmount = Math.Round(breakdown.Brokerage),
                        UserID = 1,
                        CompanyID = 1
                    };
                    LedgerService.AddTransaction(brokerageTransaction, conn, transaction);
                }
                
                // Save payment transaction (cash needed) if applicable
                if (breakdown.CashNeeded > 0)
                {
                    var paymentTransaction = new Transaction
                    {
                        PaymentID = cashAdvanceId, // Use cash advance ID for cash payments
                        PartyID = fullBill.PartyID,
                        BillID = fullBill.BillID,
                        TransactionDate = DateTime.Parse(txtPaymentDate.Text),
                        TransactionType = "Payment",
                        Description = $"Payment against Bill No: {fullBill.BillNo}",
                        CreditAmount = Math.Round(breakdown.CashNeeded),
                        PaymentMethod = cmbPaymentMethod.SelectedItem?.ToString() ?? "Cash",
                        Reference = txtReference.Text,
                        UserID = 1,
                        CompanyID = 1
                    };
                    LedgerService.AddTransaction(paymentTransaction, conn, transaction);
                }
                
                // Save advance utilization transactions
                if (breakdown.AdvanceUtilizations != null)
                {
                    foreach (var advanceUtil in breakdown.AdvanceUtilizations)
                    {
                        var advanceTransaction = new Transaction
                        {
                            PaymentID = advanceUtil.AdvanceID, // Use the advance payment ID
                            PartyID = fullBill.PartyID,
                            BillID = fullBill.BillID,
                            TransactionDate = DateTime.Parse(txtPaymentDate.Text),
                            TransactionType = "Advance",
                            Description = $"Advance utilization on Bill No: {fullBill.BillNo}",
                            CreditAmount = Math.Round(advanceUtil.AmountUsed),
                            PaymentMethod = cmbPaymentMethod.SelectedItem?.ToString() ?? "Cash",
                            Reference = $"Advance ID: {advanceUtil.AdvanceID}",
                            UserID = 1,
                            CompanyID = 1
                        };
                        LedgerService.AddTransaction(advanceTransaction, conn, transaction);
                    }
                }
            }
        }

        /// <summary>
        /// Updates advance payment entries to mark them as utilized
        /// </summary>
        private void UpdateAdvancePaymentEntries(OleDbConnection conn, OleDbTransaction transaction)
        {
            if (_userSelectedAdvancePayments == null || !_userSelectedAdvancePayments.Any())
                return;
            
            // Get the settlement result from the last calculation
            if (_lastSettlementResult == null)
            {
                throw new Exception("No settlement calculation available. Please calculate first.");
            }
            
            // Collect all advance utilizations from all bills
            var allAdvanceUtilizations = new List<AdvanceUtilizationDetail>();
            foreach (var breakdown in _lastSettlementResult.BillBreakdowns)
            {
                if (breakdown.AdvanceUtilizations != null)
                {
                    allAdvanceUtilizations.AddRange(breakdown.AdvanceUtilizations);
                }
            }
            
            // Group utilizations by AdvanceID to get total amount used from each advance
            var advanceUtilizations = allAdvanceUtilizations
                .GroupBy(u => u.AdvanceID)
                .Select(g => new { AdvanceID = g.Key, TotalUsed = g.Sum(u => u.AmountUsed) })
                .ToList();
            
            foreach (var advanceUtil in advanceUtilizations)
            {
                // Update the advance payment entry to mark it as fully utilized
                string updateQuery = @"UPDATE AdvancePayments 
                                     SET UtilizationStatus = 'Full', 
                                         RemainingAmount = 0, 
                                         LastUtilizedDate = ? 
                                     WHERE AdvanceID = ?";
                
                using (var cmd = new OleDbCommand(updateQuery, conn, transaction))
                {
                    cmd.Parameters.Add(new OleDbParameter("LastUtilizedDate", DateTime.Now));
                    cmd.Parameters.Add(new OleDbParameter("AdvanceID", advanceUtil.AdvanceID));
                    cmd.ExecuteNonQuery();
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
        /// Processes unused advance reversals by creating negative advance payments and ledger transactions
        /// </summary>
        private void ProcessUnusedAdvanceReversals(List<UnusedAdvanceDetail> unusedAdvances, OleDbConnection conn, OleDbTransaction transaction)
        {
            foreach (var unusedAdvance in unusedAdvances)
            {
                // Create a negative advance payment record (reversal)
                var reversalAdvance = new AdvancePayment
                {
                    PartyID = unusedAdvance.PartyID,
                    BrokerID = unusedAdvance.BrokerID,
                    PaymentDate = _lastSettlementResult.PaymentDate,
                    Amount = -unusedAdvance.UnusedAmount, // Negative amount for reversal
                    PaymentMethod = "Reversal",
                    Reference = $"Reversal of unused advance from AdvanceID: {unusedAdvance.AdvanceID}",
                    CompanyID = 1,
                    CreatedDate = DateTime.Now
                };

                // Insert the reversal advance payment
                string insertReversalSql = @"
                    INSERT INTO AdvancePayments (PartyID, BrokerID, PaymentDate, Amount, PaymentMethod, Reference, ChequeAmountFirm1, ChequeAmountFirm2, CompanyID, CreatedDate)
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                var reversalParams = new OleDbParameter[]
                {
                    new OleDbParameter("PartyID", OleDbType.Integer) { Value = reversalAdvance.PartyID },
                    new OleDbParameter("BrokerID", OleDbType.Integer) { Value = reversalAdvance.BrokerID },
                    new OleDbParameter("PaymentDate", OleDbType.Date) { Value = reversalAdvance.PaymentDate },
                    new OleDbParameter("Amount", OleDbType.Currency) { Value = reversalAdvance.Amount },
                    new OleDbParameter("PaymentMethod", OleDbType.VarChar, 50) { Value = reversalAdvance.PaymentMethod },
                    new OleDbParameter("Reference", OleDbType.VarChar, 255) { Value = reversalAdvance.Reference },
                    new OleDbParameter("ChequeAmountFirm1", OleDbType.Currency) { Value = 0m },
                    new OleDbParameter("ChequeAmountFirm2", OleDbType.Currency) { Value = 0m },
                    new OleDbParameter("CompanyID", OleDbType.Integer) { Value = reversalAdvance.CompanyID },
                    new OleDbParameter("CreatedDate", OleDbType.Date) { Value = reversalAdvance.CreatedDate }
                };

                using (var insertCmd = new OleDbCommand(insertReversalSql, conn, transaction))
                {
                    insertCmd.Parameters.AddRange(reversalParams);
                    insertCmd.ExecuteNonQuery();
                }

                // Get the ID of the reversal advance payment
                string getReversalIdSql = "SELECT @@IDENTITY";
                int reversalAdvanceId;
                using (var cmd = new OleDbCommand(getReversalIdSql, conn, transaction))
                {
                    reversalAdvanceId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Note: No ledger transaction needed for reversal - just the negative advance payment

                // Create utilization record for the unused portion of the ORIGINAL advance
                var originalAdvanceUtilization = new AdvanceUtilization
                {
                    AdvanceID = unusedAdvance.AdvanceID, // Original advance ID
                    PaymentID = 0,
                    AmountUsed = unusedAdvance.UnusedAmount, // Unused amount from original advance
                    UtilizedDate = _lastSettlementResult.PaymentDate,
                    PartyID = unusedAdvance.PartyID,
                    BrokerID = unusedAdvance.BrokerID,
                    CompanyID = 1
                };

                AdvanceUtilizationService.AddUtilization(originalAdvanceUtilization, conn, transaction);

                // Create utilization record for the reversal (marking it as used immediately)
                var reversalUtilization = new AdvanceUtilization
                {
                    AdvanceID = reversalAdvanceId,
                    PaymentID = 0,
                    AmountUsed = Math.Abs(reversalAdvance.Amount), // Positive amount for utilization
                    UtilizedDate = _lastSettlementResult.PaymentDate,
                    PartyID = unusedAdvance.PartyID,
                    BrokerID = unusedAdvance.BrokerID,
                    CompanyID = 1
                };

                AdvanceUtilizationService.AddUtilization(reversalUtilization, conn, transaction);
            }
        }

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
                            // OPTIMIZATION: Update all bills in one batch operation
                            var billIds = paidBills.Select(b => b.BillID).ToList();
                            var placeholders = string.Join(",", billIds.Select((_, i) => "?"));
                            
                            string updateSql = $"UPDATE BillMaster SET Status = 'Paid' WHERE BillID IN ({placeholders})";
                            
                            using (var cmd = new OleDbCommand(updateSql, conn, trans))
                            {
                                cmd.CommandTimeout = 30;
                                
                                // Add parameters for all bill IDs
                                for (int i = 0; i < billIds.Count; i++)
                                {
                                    cmd.Parameters.Add(new OleDbParameter($"BillID{i}", billIds[i]));
                                }
                                
                                cmd.ExecuteNonQuery();
                            }
                            
                            trans.Commit();
                            
                            System.Diagnostics.Debug.WriteLine($"Updated {billIds.Count} bills to 'Paid' status");
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
            
            // CRITICAL: Sort selected bills by date (oldest first) to maintain proper order for calculation
            // This ensures that when processing bills for payment allocation, older bills are processed first
            // which is important for proper advance payment allocation and interest/discount calculations
            return selectedBills.OrderBy(b => b.BillDate).ToList();
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
                
            // Get the target payment amount
            if (!decimal.TryParse(txtPaymentAmount.Text, out decimal targetPaymentAmount))
                return;
                
            // Store the current cursor position for the field being edited
            TextBox currentTextBox = sender as TextBox;
            int cursorPosition = currentTextBox?.SelectionStart ?? 0;
                
            // Determine which field was changed and auto-calculate the other
            if (sender == txtChequeAmountFirm1)
            {
                // Firm1 was changed, calculate Firm2
                if (decimal.TryParse(txtChequeAmountFirm1.Text, out decimal firm1Amount))
                {
                    decimal remainingAmount = targetPaymentAmount - firm1Amount;
                    if (remainingAmount >= 0)
                    {
                        txtChequeAmountFirm2.Text = remainingAmount.ToString("F2");
                    }
                    else
                    {
                        // Firm1 amount exceeds target, clear Firm2 and show warning
                        txtChequeAmountFirm2.Text = "0.00";
                        // Optional: Show a subtle warning or change color
                    }
                }
                
                // Restore cursor position for Firm1
                if (currentTextBox != null)
                {
                    currentTextBox.SelectionStart = Math.Min(cursorPosition, currentTextBox.Text.Length);
                }
            }
            else if (sender == txtChequeAmountFirm2)
            {
                // Firm2 was changed, calculate Firm1
                if (decimal.TryParse(txtChequeAmountFirm2.Text, out decimal firm2Amount))
                {
                    decimal remainingAmount = targetPaymentAmount - firm2Amount;
                    if (remainingAmount >= 0)
                    {
                        txtChequeAmountFirm1.Text = remainingAmount.ToString("F2");
                    }
                    else
                    {
                        // Firm2 amount exceeds target, clear Firm1 and show warning
                        txtChequeAmountFirm1.Text = "0.00";
                        // Optional: Show a subtle warning or change color
                    }
                }
                
                // Restore cursor position for Firm2
                if (currentTextBox != null)
                {
                    currentTextBox.SelectionStart = Math.Min(cursorPosition, currentTextBox.Text.Length);
                }
            }
        }

        #region Advance Payment Display

        /// <summary>
        /// Updates the advance payment display based on selected party and broker
        /// </summary>
        // private void UpdateAdvancePaymentDisplay()
        // {
        //     try
        //     {
        //         int? partyId = cmbParty.SelectedValue as int?;
        //         int? brokerId = cmbBroker.SelectedValue as int?;

        //         // Hide panel if nothing is selected
        //         if ((!partyId.HasValue || partyId.Value <= 0) && (!brokerId.HasValue || brokerId.Value <= 0))
        //         {
        //             pnlAdvanceDisplay.Visible = false;
        //             return;
        //         }

        //         // Get advance amounts by payment method
        //         var advanceAmounts = GetAdvanceAmountsByPaymentMethod(partyId, brokerId);
                
        //         // Calculate excess amount using the same logic as save process
        //         decimal excessAmount = 0;
        //         string selectedPaymentMethod = cmbPaymentMethod.SelectedItem?.ToString() ?? "Cash";
                
        //         if (decimal.TryParse(txtPaymentAmount.Text, out decimal paymentAmount))
        //         {
        //             decimal totalAllocated = _outstandingBills.Sum(b => b.PaymentAllocation);
                    
        //             if (paymentAmount > 0 && totalAllocated > 0)
        //             {
        //                 // Calculate how much advance would be used against bills (FIFO simulation)
        //                 decimal totalAdvanceAvailable = advanceAmounts.Cash + advanceAmounts.Firm1 + advanceAmounts.Firm2;
        //                 decimal advanceUsedAgainstBills = Math.Min(totalAdvanceAvailable, totalAllocated);
                        
        //                 // Remove used advances from display (they will be consumed)
        //                 if (advanceUsedAgainstBills > 0)
        //                 {
        //                     // Simulate FIFO consumption to remove used amounts from display
        //                     decimal remainingToRemove = advanceUsedAgainstBills;
                            
        //                     // Remove from Cash first
        //                     if (remainingToRemove > 0 && advanceAmounts.Cash > 0)
        //                     {
        //                         decimal removeFromCash = Math.Min(advanceAmounts.Cash, remainingToRemove);
        //                         advanceAmounts.Cash -= removeFromCash;
        //                         remainingToRemove -= removeFromCash;
        //                     }
                            
        //                     // Remove from Firm1 next
        //                     if (remainingToRemove > 0 && advanceAmounts.Firm1 > 0)
        //                     {
        //                         decimal removeFromFirm1 = Math.Min(advanceAmounts.Firm1, remainingToRemove);
        //                         advanceAmounts.Firm1 -= removeFromFirm1;
        //                         remainingToRemove -= removeFromFirm1;
        //                     }
                            
        //                     // Remove from Firm2 last
        //                     if (remainingToRemove > 0 && advanceAmounts.Firm2 > 0)
        //                     {
        //                         decimal removeFromFirm2 = Math.Min(advanceAmounts.Firm2, remainingToRemove);
        //                         advanceAmounts.Firm2 -= removeFromFirm2;
        //                         remainingToRemove -= removeFromFirm2;
        //                     }
        //                 }
                        
        //                 // Calculate actual cash that would be used for bills
        //                 decimal actualCashUsed = Math.Max(0, totalAllocated - advanceUsedAgainstBills);
                        
        //                 // Excess = Payment Entered - Cash Actually Used
        //                 if (paymentAmount > actualCashUsed)
        //                 {
        //                     excessAmount = paymentAmount - actualCashUsed;
                            
        //                     System.Diagnostics.Debug.WriteLine($"Display calculation: Payment {paymentAmount:C}, Bills {totalAllocated:C}, Advance used {advanceUsedAgainstBills:C}, Cash used {actualCashUsed:C}, Excess {excessAmount:C}");
        //                 }
        //             }
        //             else if (paymentAmount > totalAllocated)
        //             {
        //                 // Fallback for when no bills are allocated
        //                 excessAmount = paymentAmount - totalAllocated;
                        
        //             }
                    
        //             // Add excess to the appropriate advance category based on payment method (if any excess exists)
        //             if (excessAmount > 0)
        //             {
        //                 if (selectedPaymentMethod.Equals("Cash", StringComparison.OrdinalIgnoreCase))
        //                 {
        //                     advanceAmounts.Cash += excessAmount;
        //                 }
        //                 else if (selectedPaymentMethod.Equals("Cheque", StringComparison.OrdinalIgnoreCase))
        //                 {
        //                     // For cheque payments, distribute excess based on firm amounts ratio
        //                     if (decimal.TryParse(txtChequeAmountFirm1.Text, out decimal firm1) && 
        //                         decimal.TryParse(txtChequeAmountFirm2.Text, out decimal firm2))
        //                     {
        //                         decimal totalCheque = firm1 + firm2;
        //                         if (totalCheque > 0 && Math.Abs(totalCheque - paymentAmount) < 0.01m)
        //                         {
        //                             // Distribute excess proportionally
        //                             decimal firm1Ratio = firm1 / totalCheque;
        //                             decimal firm2Ratio = firm2 / totalCheque;
        //                             advanceAmounts.Firm1 += excessAmount * firm1Ratio;
        //                             advanceAmounts.Firm2 += excessAmount * firm2Ratio;
        //                         }
        //                         else
        //                         {
        //                             // Default to equal split or add to Firm1
        //                             advanceAmounts.Firm1 += excessAmount;
        //                         }
        //                     }
        //                     else
        //                     {
        //                         // Default to adding excess to Firm1
        //                         advanceAmounts.Firm1 += excessAmount;
        //                     }
        //                 }
        //             }
        //         }
                
        //         // Update labels
        //         lblAdvanceCash.Text = $"Cash: ₹{advanceAmounts.Cash:N2}";
        //         lblAdvanceFirm1.Text = $"Firm1: ₹{advanceAmounts.Firm1:N2}";
        //         lblAdvanceFirm2.Text = $"Firm2: ₹{advanceAmounts.Firm2:N2}";

        //         // Update title to show if there's excess
        //         if (excessAmount > 0)
        //         {
        //             lblAdvanceTitle.Text = $"Advance Avail (+ ₹{excessAmount:N2} excess):";
        //             lblAdvanceTitle.ForeColor = Color.Red; // Highlight excess
        //         }
        //         else
        //         {
        //             lblAdvanceTitle.Text = "Advance Avail:";
        //             lblAdvanceTitle.ForeColor = Color.Black; // Default color
        //         }

        //         // Show panel if there are any advances or excess
        //         bool hasAdvances = advanceAmounts.Cash > 0 || advanceAmounts.Firm1 > 0 || advanceAmounts.Firm2 > 0;
        //         pnlAdvanceDisplay.Visible = hasAdvances;
        //     }
        //     catch (Exception ex)
        //     {
        //         System.Diagnostics.Debug.WriteLine($"Error updating advance payment display: {ex.Message}");
        //         pnlAdvanceDisplay.Visible = false;
        //     }
        // }

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

        /// <summary>
        /// Main payment allocation orchestrator - coordinates the entire payment allocation process
        /// </summary>
        // public PaymentAllocationSummary AllocatePaymentToBills(
        // decimal totalPaymentAmount,
        // List<BillViewModel> billsToProcess,
        // List<AdvancePayment> availableAdvances,
        // DateTime paymentDate,
        // PaymentTerms terms)
        // {
        //     var summary = new PaymentAllocationSummary
        //     {
        //         TotalPaymentAmount = totalPaymentAmount,
        //         BillResults = new List<BillPaymentResult>()
        //     };

        //     // Validate inputs
        //     ValidateInputs(totalPaymentAmount, billsToProcess);

        //     // Create working copies to avoid modifying original data
        //     var workingAdvances = CloneAdvances(availableAdvances);
        //     var sortedBills = billsToProcess.OrderBy(b => b.BillDate).ToList();
            
        //     // Get full bill details efficiently
        //     var billIds = sortedBills.Select(b => b.BillID).ToList();
        //     var fullBillDetails = BillService.GetBillsByIDs(billIds);
            
        //     decimal remainingPaymentAmount = totalPaymentAmount;

        //     // Process each bill in chronological order
        //     foreach (var billVm in sortedBills)
        //     {
        //         if (remainingPaymentAmount <= 0.01m) break;

        //         var fullBill = GetFullBillDetails(billVm, fullBillDetails);
        //         if (fullBill == null) continue;

        //         var billResult = ProcessSingleBillPayment(
        //             billVm, 
        //             fullBill, 
        //             remainingPaymentAmount, 
        //             workingAdvances, 
        //             paymentDate, 
        //             terms);

        //         summary.BillResults.Add(billResult);
        //         remainingPaymentAmount -= billResult.PaymentAllocated;

        //         // Update summary totals
        //         UpdateSummaryTotals(summary, billResult);
        //     }

        //     // Handle any excess payment amount
        //     if (remainingPaymentAmount > 0.01m)
        //     {
        //         summary.ExcessAmount = remainingPaymentAmount;
        //     }

        //     // Create advance utilization records for successfully processed advances
        //     summary.AdvanceUtilizations = CreateAdvanceUtilizationRecords(summary.BillResults, paymentDate);

        //     return summary;
        // }


        /// <summary>
        /// Processes payment allocation for a single bill using your existing calculation logic
        /// </summary>
        // private BillPaymentResult ProcessSingleBillPayment(
        //     BillViewModel billVm,
        //     Bill fullBill,
        //     decimal availablePaymentAmount,
        //     List<AdvancePayment> workingAdvances,
        //     DateTime paymentDate,
        //     PaymentTerms terms)
        // {
        //     // Step 1: Calculate what this bill actually needs using your proven method
        //     var calculationResult = CalculateMixedPaymentAllocation(
        //         fullBill, 
        //         billVm, 
        //         workingAdvances.ToList(), // Pass copy to avoid modification during calculation
        //         0, // Let it auto-calculate cash requirement
        //         paymentDate,
        //         terms.InterestDays,
        //         terms.InterestRate,
        //         terms.DiscountDays,
        //         terms.DiscountRate,
        //         terms.BrokerageRate);

        //     decimal totalAmountNeeded = calculationResult.NetSettlement;
            
        //     // Step 2: Determine how much we can actually pay
        //     decimal amountWeCanPay = Math.Min(availablePaymentAmount, totalAmountNeeded);
            
        //     // Step 3: Create the result based on payment capacity
        //     var result = CreateBillPaymentResult(billVm, totalAmountNeeded, amountWeCanPay, calculationResult);
            
        //     // Step 4: Only consume advances if we can make meaningful payment
        //     if (result.Status == PaymentStatus.FullyPaid)
        //     {
        //         // Full payment - consume advances as calculated
        //         result.AdvanceAllocations = ConsumeAdvancesForFullPayment(
        //             workingAdvances, calculationResult.AdvanceBreakdown);
        //     }
        //     else if (result.Status == PaymentStatus.PartiallyPaid)
        //     {
        //         // Partial payment - consume advances proportionally
        //         result.AdvanceAllocations = ConsumeAdvancesProportionally(
        //             workingAdvances, calculationResult.AdvanceBreakdown, 
        //             amountWeCanPay / totalAmountNeeded);
        //     }
        //     // For NothingPaid status, don't consume any advances

        //     return result;
        // }

        // private BillPaymentResult CreateBillPaymentResult(
        // BillViewModel billVm, 
        // decimal totalNeeded, 
        // decimal amountPaying, 
        // PaymentAllocationResult calculationResult)
        // {
        //     var result = new BillPaymentResult
        //     {
        //         BillID = billVm.BillID,
        //         BillNo = billVm.BillNo,
        //         OriginalBalance = billVm.BalanceDue,
        //         PaymentAllocated = amountPaying
        //     };

        //     // Determine payment status
        //     if (amountPaying <= 0.01m)
        //     {
        //         result.Status = PaymentStatus.NothingPaid;
        //         result.RemainingBalance = totalNeeded;
        //         return result;
        //     }
            
        //     if (amountPaying >= totalNeeded - 0.01m) // Account for rounding
        //     {
        //         result.Status = PaymentStatus.FullyPaid;
        //         result.RemainingBalance = 0;
                
        //         // Use full calculated amounts for complete payment
        //         result.AdvanceUsed = calculationResult.AdvanceUsed;
        //         result.CashUsed = calculationResult.CashUsed;
        //         result.InterestCharged = calculationResult.InterestCharged;
        //         result.DiscountEarned = calculationResult.DiscountEarned;
        //         result.BrokerageCharged = calculationResult.BrokerageCharged;
        //     }
        //     else
        //     {
        //         result.Status = PaymentStatus.PartiallyPaid;
        //         result.RemainingBalance = totalNeeded - amountPaying;
                
        //         // Calculate proportional amounts for partial payment
        //         decimal ratio = amountPaying / totalNeeded;
        //         result.AdvanceUsed = Math.Round(calculationResult.AdvanceUsed * ratio, 2);
        //         result.CashUsed = amountPaying - result.AdvanceUsed;
        //         result.InterestCharged = Math.Round(calculationResult.InterestCharged * ratio, 2);
        //         result.DiscountEarned = Math.Round(calculationResult.DiscountEarned * ratio, 2);
        //         result.BrokerageCharged = Math.Round(calculationResult.BrokerageCharged * ratio, 2);
        //     }

        //     return result;
        // }

        // private List<AdvanceAllocation> ConsumeAdvancesForFullPayment(
        //     List<AdvancePayment> workingAdvances,
        //     List<AdvanceUtilization> advanceBreakdown)
        // {
        //     var allocations = new List<AdvanceAllocation>();
            
        //     if (advanceBreakdown == null) return allocations;

        //     foreach (var breakdown in advanceBreakdown)
        //     {
        //         var advance = workingAdvances.FirstOrDefault(a => a.AdvanceID == breakdown.AdvanceID);
        //         if (advance != null && advance.Amount >= breakdown.AmountUsed)
        //         {
        //             // Create allocation record
        //             allocations.Add(new AdvanceAllocation
        //             {
        //                 AdvanceID = advance.AdvanceID,
        //                 AmountUsed = breakdown.AmountUsed,
        //                 AdvanceDate = breakdown.CreatedDate
        //             });

        //             // Consume from advance
        //             advance.Amount -= breakdown.AmountUsed;
                    
        //             // Remove if fully consumed
        //             if (advance.Amount <= 0.01m)
        //             {
        //                 workingAdvances.Remove(advance);
        //             }
        //         }
        //     }

        //     return allocations;
        // }
                
//         private List<AdvanceAllocation> ConsumeAdvancesProportionally(
//     List<AdvancePayment> workingAdvances,
//     List<AdvanceUtilization> advanceBreakdown,
//     decimal proportionRatio)
// {
//     var allocations = new List<AdvanceAllocation>();
    
//     if (advanceBreakdown == null || proportionRatio <= 0) return allocations;

//     foreach (var breakdown in advanceBreakdown)
//     {
//         var advance = workingAdvances.FirstOrDefault(a => a.AdvanceID == breakdown.AdvanceID);
//         if (advance != null)
//         {
//             decimal proportionalAmount = Math.Round(breakdown.AmountUsed * proportionRatio, 2);
//             decimal actualAmountToUse = Math.Min(proportionalAmount, advance.Amount);

//             if (actualAmountToUse > 0.01m)
//             {
//                 allocations.Add(new AdvanceAllocation
//                 {
//                     AdvanceID = advance.AdvanceID,
//                     AmountUsed = actualAmountToUse,
//                     AdvanceDate = breakdown.CreatedDate
//                 });

//                 advance.Amount -= actualAmountToUse;
                
//                 if (advance.Amount <= 0.01m)
//                 {
//                     workingAdvances.Remove(advance);
//                 }
//             }
//         }
//     }

//     return allocations;
// }

// private void ValidateInputs(decimal totalPaymentAmount, List<BillViewModel> billsToProcess)
// {
//     if (billsToProcess == null || !billsToProcess.Any())
//         throw new ArgumentException("No bills provided for payment allocation");
    
//     if (totalPaymentAmount <= 0)
//         throw new ArgumentException("Payment amount must be greater than zero");
// }

// private List<AdvancePayment> CloneAdvances(List<AdvancePayment> original)
// {
//     return original?.Select(a => new AdvancePayment
//     {
//         AdvanceID = a.AdvanceID,
//         Amount = a.Amount,
//         CreatedDate = a.CreatedDate,
//         PartyID = a.PartyID,
//         BrokerID = a.BrokerID
//         // Copy other necessary properties
//     }).ToList() ?? new List<AdvancePayment>();
// }

// private Bill GetFullBillDetails(BillViewModel billVm, List<Bill> fullBillDetails)
// {
//     var fullBill = fullBillDetails.FirstOrDefault(b => b.BillID == billVm.BillID);
//     if (fullBill == null)
//     {
//         fullBill = BillService.GetBillByID(billVm.BillID);
//     }
//     return fullBill;
// }

// private void UpdateSummaryTotals(PaymentAllocationSummary summary, BillPaymentResult billResult)
// {
//     summary.TotalAdvanceUsed += billResult.AdvanceUsed;
//     summary.TotalCashUsed += billResult.CashUsed;
//     summary.TotalDiscountEarned += billResult.DiscountEarned;
//     summary.TotalInterestCharged += billResult.InterestCharged;
//     summary.TotalBrokerageCharged += billResult.BrokerageCharged;
// }

// private List<AdvanceUtilization> CreateAdvanceUtilizationRecords(
//     List<BillPaymentResult> billResults, 
//     DateTime paymentDate)
// {
//     var utilizations = new List<AdvanceUtilization>();
    
//     foreach (var billResult in billResults)
//     {
//         foreach (var allocation in billResult.AdvanceAllocations)
//         {
//             utilizations.Add(new AdvanceUtilization
//             {
//                 AdvanceID = allocation.AdvanceID,
//                 AmountUsed = allocation.AmountUsed,
//                 UtilizedDate = paymentDate,
//                 // PaymentID will be set when payment record is saved
//                 // Other properties as needed
//             });
//         }
//     }
    
//     return utilizations;
// }
        
        /// <summary>
        /// Enhanced advance utilization tracking and management
        /// </summary>
        // private AdvanceUtilizationResult ProcessAdvanceUtilization(
        //     decimal amountNeeded,
        //     List<AdvancePayment> availableAdvances,
        //     BillViewModel billVm,
        //     DateTime paymentDate,
        //     DateTime interestStartDate,
        //     PaymentTerms terms,
        //     decimal interestBearingPrincipal)
        // {
        //     var result = new AdvanceUtilizationResult();
            
        //     if (availableAdvances == null || !availableAdvances.Any() || amountNeeded <= 0.01m)
        //     {
        //         result.AdvanceUsed = 0;
        //         result.CashNeeded = amountNeeded;
        //         result.Utilizations = new List<AdvanceUtilization>();
        //         return result;
        //     }

        //     // Sort advances by date (FIFO - oldest first)
        //     var sortedAdvances = availableAdvances.OrderBy(a => a.CreatedDate).ToList();
            
        //     decimal remainingAmount = amountNeeded;
        //     decimal advanceUsed = 0;
        //     var utilizations = new List<AdvanceUtilization>();
        //     var advanceAllocations = new List<AdvanceAllocation>();
        //     decimal interest_advance = 0;
        //     decimal discount_advance = 0;
        //     foreach(var adv in sortedAdvances){
        //         if(remainingAmount<=0) break;
        //         decimal amountToUse = Math.Min(adv.Amount, remainingAmount);
        //         if(remainingAmount>0){
        //             if((adv.CreatedDate.Date-billVm.BillDate.Date).Days<=(terms.DiscountDays)){
        //                 discount_advance = discount_advance + (amountToUse*terms.DiscountRate)/100;
        //             }
        //             int overDays = (adv.CreatedDate.Date - interestStartDate.Date).Days ;
        //             if(overDays>0){
        //                 interest_advance +=  interestBearingPrincipal * (terms.InterestRate / 100m) * (overDays / 365m);
        //                 interestStartDate = adv.CreatedDate;
        //             }
        //             result.AdvanceUsed += amountToUse;
        //             interestBearingPrincipal -= amountToUse;
        //             remainingAmount -= amountToUse;
        //             advanceAllocations.Add(new AdvanceAllocation
        //             {
        //                 AdvanceUsed = amountToUse,
        //                 AdvanceStartDate = adv.CreatedDate,
        //                 AdvanceID = adv.AdvanceID
        //             });
        //         }
        //     }
        //     remainingAmount = result.AdvanceUsed;
        //     foreach (var advance in sortedAdvances)
        //     {
        //         if (remainingAmount <= 0.01m || advance.Amount <= 0.01m)
        //             break;

        //         decimal amountToUse = Math.Min(advance.Amount, remainingAmount);
                
        //         if (amountToUse > 0)
        //         {
        //             // Create utilization record
        //             var utilization = new AdvanceUtilization
        //             {
        //                 AdvanceID = advance.AdvanceID,
        //                 PaymentID = 0, // Will be set when payment is saved
        //                 AmountUsed = amountToUse,
        //                 UtilizedDate = paymentDate,
        //                 PartyID = advance.PartyID,
        //                 BrokerID = advance.BrokerID,
        //                 CompanyID = 1, 
        //             };

        //             utilizations.Add(utilization);

        //             // Update advance amount
        //             advance.Amount -= amountToUse;
        //             advanceUsed += amountToUse;
        //             remainingAmount -= amountToUse;

        //             // Remove fully consumed advances
        //             if (advance.Amount <= 0.01m)
        //             {
        //                 availableAdvances.Remove(advance);
        //             }

        //             // Log the utilization for debugging
        //             System.Diagnostics.Debug.WriteLine($"Using {amountToUse:C} from AdvanceID: {advance.AdvanceID} for Bill: {billVm.BillNo}");
        //         }
        //     }
        //     remainingAmount =  (amountNeeded - advanceUsed);

        //     result.AdvanceUsed = advanceUsed;
        //     result.CashNeeded = remainingAmount;
        //     result.Utilizations = utilizations;
        //     result.InterestUsed = interest_advance;
        //     result.DiscountUsed = discount_advance;
        //     result.InterestStartDate = interestStartDate;
        //     result.AdvanceAllocations = advanceAllocations;
        //     return result;
        // }

        /// <summary>
        /// Calculates interest and discount for advance payments based on bill date
        /// </summary>

        /// <summary>
        /// Integrates advance utilization with the main payment allocation
        /// </summary>

        /// <summary>
        /// Result of advance utilization processing
        /// </summary>
        private class AdvanceUtilizationResult
        {
            public decimal AdvanceUsed { get; set; }
            public decimal CashNeeded { get; set; }
            public required List<AdvanceUtilization> Utilizations { get; set; }
            public required List<AdvanceAllocation> AdvanceAllocations { get; set; }
            public decimal InterestUsed { get; set; }
            public decimal DiscountUsed { get; set; }
            public DateTime InterestStartDate { get; set; }
        }

        /// <summary>
        /// Handles excess payment amounts by creating new advance payments
        /// </summary>
        // private AdvancePayment CreateAdvanceFromExcess(
        //     decimal excessAmount,
        //     int? partyId,
        //     int? brokerId,
        //     DateTime paymentDate,
        //     string paymentMethod,
        //     string reference)
        // {
        //     if (excessAmount <= 0.01m)
        //         return null;

        //     // Create new advance payment record
        //     var advancePayment = new AdvancePayment
        //     {
        //         PartyID = partyId ?? -1,
        //         BrokerID = brokerId,
        //         PaymentDate = paymentDate,
        //         Amount = excessAmount,
        //         PaymentMethod = paymentMethod,
        //         Reference = $"Excess from payment: {reference}",
        //         CompanyID = 1, // TODO: Use Program.ActiveCompany.CompanyID
        //         CreatedDate = DateTime.Now,
        //     };

        //     // Log the creation of advance payment
        //     System.Diagnostics.Debug.WriteLine($"Creating advance payment of {excessAmount:C} from excess amount. PartyID: {partyId}, BrokerID: {brokerId}");

        //     return advancePayment;
        // }

        /// <summary>
        /// Processes excess amount and creates advance payment if needed
        /// </summary>
        // private ExcessHandlingResult HandleExcessAmount(
        //     decimal excessAmount,
        //     int? partyId,
        //     int? brokerId,
        //     DateTime paymentDate,
        //     string paymentMethod,
        //     string reference,
        //     PaymentAllocationSummary summary)
        // {
        //     var result = new ExcessHandlingResult();

        //     if (excessAmount <= 0.01m)
        //     {
        //         result.HasExcess = false;
        //         result.ExcessAmount = 0;
        //         result.AdvancePayment = null;
        //         return result;
        //     }

        //     result.HasExcess = true;
        //     result.ExcessAmount = excessAmount;

        //     // Create advance payment from excess
        //     var advancePayment = CreateAdvanceFromExcess(
        //         excessAmount, 
        //         partyId, 
        //         brokerId, 
        //         paymentDate, 
        //         paymentMethod, 
        //         reference);

        //     if (advancePayment != null)
        //     {
        //         result.AdvancePayment = advancePayment;
                
        //         // Add to summary for tracking
        //         summary.ExcessAmount = excessAmount;
                
        //         // Log the excess handling
        //         System.Diagnostics.Debug.WriteLine($"Excess amount {excessAmount:C} converted to advance payment. AdvanceID will be assigned when saved.");
                
        //         // Update the summary to reflect the advance payment
        //         summary.TotalAdvanceUsed += excessAmount; // This represents the new advance created
        //     }

        //     return result;
        // }

        /// <summary>
        /// Validates the complete payment allocation
        /// </summary>

        /// <summary>
        /// Creates a comprehensive summary report for the payment allocation
        /// </summary>
        // private PaymentSummaryReport CreatePaymentSummaryReport(
        //     PaymentAllocationSummary summary,
        //     int? partyId,
        //     int? brokerId,
        //     string paymentMethod,
        //     string reference)
        // {
        //     var report = new PaymentSummaryReport
        //     {
        //         PaymentDate = summary.BillResults.FirstOrDefault()?.PaymentDate ?? DateTime.Now,
        //         PartyID = partyId,
        //         BrokerID = brokerId,
        //         PaymentMethod = paymentMethod,
        //         Reference = reference,
        //         TotalPaymentAmount = summary.TotalPaymentAmount,
        //         TotalAdvanceUsed = summary.TotalAdvanceUsed,
        //         TotalCashUsed = summary.TotalCashUsed,
        //         TotalDiscountEarned = summary.TotalDiscountEarned,
        //         TotalInterestCharged = summary.TotalInterestCharged,
        //         TotalBrokerageCharged = summary.TotalBrokerageCharged,
        //         ExcessAmount = summary.ExcessAmount,
        //         BillCount = summary.BillResults.Count,
        //         FullyPaidBills = summary.BillResults.Count(b => b.IsFullyPaid),
        //         PartiallyPaidBills = summary.BillResults.Count(b => b.IsPartiallyPaid),
        //         AdvanceUtilizationCount = summary.AdvanceUtilizations.Count
        //     };

        //     // Add bill details
        //     report.BillDetails = summary.BillResults.Select(b => new BillSummaryDetail
        //     {
        //         BillNo = b.BillNo,
        //         OriginalBalance = b.OriginalBalance,
        //         PaymentAllocated = b.PaymentAllocated,
        //         RemainingBalance = b.RemainingBalance,
        //         AdvanceUsed = b.AdvanceUsed,
        //         CashUsed = b.CashUsed,
        //         DiscountEarned = b.DiscountEarned,
        //         InterestCharged = b.InterestCharged,
        //         BrokerageCharged = b.BrokerageCharged,
        //         IsFullyPaid = b.IsFullyPaid
        //     }).ToList();

        //     return report;
        // }

        /// <summary>
        /// Result of excess amount handling
        /// </summary>
        private class ExcessHandlingResult
        {
            public bool HasExcess { get; set; }
            public decimal ExcessAmount { get; set; }
            public AdvancePayment? AdvancePayment { get; set; }
        }

        /// <summary>
        /// Result of payment validation
        /// </summary>
        private class PaymentValidationResult
        {
            public bool IsValid { get; set; }
            public required string ValidationMessage { get; set; }
        }

        /// <summary>
        /// Comprehensive payment summary report
        /// </summary>
        private class PaymentSummaryReport
        {
            public DateTime PaymentDate { get; set; }
            public int? PartyID { get; set; }
            public int? BrokerID { get; set; }
            public required string PaymentMethod { get; set; }
            public required string Reference { get; set; }
            public decimal TotalPaymentAmount { get; set; }
            public decimal TotalAdvanceUsed { get; set; }
            public decimal TotalCashUsed { get; set; }
            public decimal TotalDiscountEarned { get; set; }
            public decimal TotalInterestCharged { get; set; }
            public decimal TotalBrokerageCharged { get; set; }
            public decimal ExcessAmount { get; set; }
            public int BillCount { get; set; }
            public int FullyPaidBills { get; set; }
            public int PartiallyPaidBills { get; set; }
            public int AdvanceUtilizationCount { get; set; }
            public required List<BillSummaryDetail> BillDetails { get; set; } = new List<BillSummaryDetail>();
        }

        /// <summary>
        /// Individual bill summary detail
        /// </summary>
        private class BillSummaryDetail
        {
            public required string BillNo { get; set; }
            public decimal OriginalBalance { get; set; }
            public decimal PaymentAllocated { get; set; }
            public decimal RemainingBalance { get; set; }
            public decimal AdvanceUsed { get; set; }
            public decimal CashUsed { get; set; }
            public decimal DiscountEarned { get; set; }
            public decimal InterestCharged { get; set; }
            public decimal BrokerageCharged { get; set; }
            public bool IsFullyPaid { get; set; }
        }

        /// <summary>
        /// DEBUG: Shows detailed payment allocation information
        /// </summary>
        private void ShowPaymentAllocationDetails(PaymentAllocationSummary allocationSummary)
        {
        //     var details = new System.Text.StringBuilder();
        //     details.AppendLine("=== PAYMENT ALLOCATION DETAILS ===\n");
            
        //     details.AppendLine($"Total Payment Amount: ₹{allocationSummary.TotalPaymentAmount:N2}");
        //     details.AppendLine($"Total Cash Used: ₹{allocationSummary.TotalCashUsed:N2}");
        //     details.AppendLine($"Total Advance Used: ₹{allocationSummary.TotalAdvanceUsed:N2}");
        //     details.AppendLine($"Total Discount Earned: ₹{allocationSummary.TotalDiscountEarned:N2}");
        //     details.AppendLine($"Total Interest Charged: ₹{allocationSummary.TotalInterestCharged:N2}");
        //     details.AppendLine($"Total Brokerage Charged: ₹{allocationSummary.TotalBrokerageCharged:N2}");
        //     details.AppendLine($"Excess Amount: ₹{allocationSummary.ExcessAmount:N2}");
        //     details.AppendLine($"Bills Processed: {allocationSummary.BillResults?.Count ?? 0}\n");

        //     if (allocationSummary.BillResults != null && allocationSummary.BillResults.Any())
        //     {
        //         details.AppendLine("=== BILL-WISE BREAKDOWN ===");
        //         foreach (var billResult in allocationSummary.BillResults)
        //         {
        //             details.AppendLine($"\nBill ID: {billResult.BillID}");
        //             details.AppendLine($"  Payment Allocated: ₹{billResult.PaymentAllocated:N2}");
        //             details.AppendLine($"  Cash Used: ₹{billResult.CashUsed:N2}");
        //             details.AppendLine($"  Advance Used: ₹{billResult.AdvanceUsed:N2}");
        //             details.AppendLine($"  Discount Earned: ₹{billResult.DiscountEarned:N2}");
        //             details.AppendLine($"  Interest Charged: ₹{billResult.InterestCharged:N2}");
        //             details.AppendLine($"  Brokerage Charged: ₹{billResult.BrokerageCharged:N2}");
        //             details.AppendLine($"  Remaining Balance: ₹{billResult.RemainingBalance:N2}");
                    
        //             if (billResult.AdvanceAllocations != null && billResult.AdvanceAllocations.Any())
        //             {
        //                 details.AppendLine($"  Advance Allocations ({billResult.AdvanceAllocations.Count}):");
        //                 foreach (var advance in billResult.AdvanceAllocations)
        //                 {
        //                     details.AppendLine($"    - Used: ₹{advance.AdvanceUsed:N2}, Start Date: {advance.AdvanceStartDate:dd-MM-yyyy}");
        //                 }
        //             }
        //         }
        //     }

        //     MessageBox.Show(details.ToString(), "Payment Allocation Debug Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// DEBUG: Shows detailed payment allocation result for individual bills
        /// </summary>
        // private void ShowPaymentAllocationResultDetails(PaymentAllocationResult result, string billNo)
        // {
        //     var details = new System.Text.StringBuilder();
        //     details.AppendLine($"=== PAYMENT ALLOCATION RESULT FOR BILL {billNo} ===\n");
            
        //     details.AppendLine($"Advance Used: ₹{result.AdvanceUsed:N2}");
        //     details.AppendLine($"Cash Used: ₹{result.CashUsed:N2}");
        //     details.AppendLine($"Discount Earned: ₹{result.DiscountEarned:N2}");
        //     details.AppendLine($"Interest Charged: ₹{result.InterestCharged:N2}");
        //     details.AppendLine($"Brokerage Charged: ₹{result.BrokerageCharged:N2}");
        //     details.AppendLine($"Net Settlement: ₹{result.NetSettlement:N2}");
            
        //     if (result.AdvanceBreakdown != null && result.AdvanceBreakdown.Any())
        //     {
        //         details.AppendLine($"\n=== ADVANCE BREAKDOWN ({result.AdvanceBreakdown.Count} utilizations) ===");
        //         foreach (var advance in result.AdvanceBreakdown)
        //         {
        //             details.AppendLine($"- AdvanceID: {advance.AdvanceID}");
        //             details.AppendLine($"  Amount Used: ₹{advance.AmountUsed:N2}");
        //             details.AppendLine($"  Payment Date (used for calc): {advance.CreatedDate:dd-MM-yyyy}");
        //             details.AppendLine($"  Utilized Date: {advance.UtilizedDate:dd-MM-yyyy}");
        //             details.AppendLine();
        //         }
        //     }
        //     else
        //     {
        //         details.AppendLine("\n=== NO ADVANCE BREAKDOWN ===");
        //     }

        //     MessageBox.Show(details.ToString(), $"Payment Allocation Result - Bill {billNo}", MessageBoxButtons.OK, MessageBoxIcon.Information);
        // }


        /// <summary>
        /// Calculates settlement requirements for multiple bills with advance payments
        /// </summary>
        private SettlementCalculationResult CalculateSettlementRequirement(
    List<BillViewModel> billsToSettle,
    List<AdvancePayment> availableAdvances,
    DateTime settlementDate,
    int interestDays,
    decimal interestRate,
    int discountDays,
    decimal discountRate,
    decimal brokerageRate)
{
    decimal totalAdvanceUsed = 0;
    decimal totalCashNeeded = 0;
    decimal totalInterest = 0;
    decimal totalDiscount = 0;
    decimal totalBrokerage = 0;
    var billBreakdowns = new List<BillSettlementBreakdown>();
    
    // Use user-selected advances or all available advances
    var advancesToUse = _userSelectedAdvancePayments.Any() 
        ? _userSelectedAdvancePayments 
        : availableAdvances;
    
    // Create working copy of advances for consumption tracking
    var workingAdvances = advancesToUse.Select(a => new AdvancePayment
    {
        AdvanceID = a.AdvanceID,
        Amount = a.Amount,
        PaymentDate = a.PaymentDate,
        PartyID = a.PartyID,
        BrokerID = a.BrokerID,
        PaymentMethod = a.PaymentMethod,
        Reference = a.Reference
    }).OrderBy(a => a.PaymentDate).ToList(); // FIFO order

    // Process each bill
    foreach (var billVm in billsToSettle.OrderBy(b => b.BillDate))
    {
        var billResult = CalculateIndividualBillSettlement(
            billVm, workingAdvances, settlementDate,
            interestDays, interestRate, discountDays, discountRate, brokerageRate);

        totalAdvanceUsed += billResult.AdvanceUsed;
        totalCashNeeded += billResult.CashNeeded;
        totalInterest += billResult.Interest;
        totalDiscount += billResult.Discount;
        totalBrokerage += billResult.Brokerage;

        billBreakdowns.Add(new BillSettlementBreakdown
        {
            BillID = billVm.BillID,
            BillNo = billVm.BillNo,
            AmountDue = billResult.TotalDue,
            AdvanceUsed = billResult.AdvanceUsed,
            CashNeeded = billResult.CashNeeded,
            Interest = billResult.Interest,
            Discount = billResult.Discount,
            Brokerage = billResult.Brokerage,
            AdvanceUtilizations = billResult.AdvanceUtilizations,
            InterestPeriods = billResult.InterestPeriods
        });
    }

    decimal totalAdvanceAvailable = advancesToUse.Sum(a => a.Amount);
    decimal unusedAdvance = workingAdvances.Sum(a => a.Amount);

    return new SettlementCalculationResult
    {
        TotalAmountDue = billsToSettle.Sum(b => b.TotalAmount) + totalInterest - totalDiscount,
        TotalAdvanceUsed = totalAdvanceUsed,
        TotalCashNeeded = totalCashNeeded,
        TotalInterest = totalInterest,
        TotalDiscount = totalDiscount,
        TotalBrokerage = totalBrokerage,
        AdvanceAvailable = totalAdvanceAvailable,
        UnusedAdvance = unusedAdvance,
        CanFullySettle = totalCashNeeded <= 0.01m,
        BillBreakdowns = billBreakdowns,
        PaymentDate = settlementDate
    };
}

/// <summary>
/// Calculates settlement for an individual bill with advance payments
/// </summary>
private IndividualBillSettlementResult CalculateIndividualBillSettlement(
    BillViewModel billVm,
    List<AdvancePayment> workingAdvances,
    DateTime settlementDate,
    int interestDays,
    decimal interestRate,
    int discountDays,
    decimal discountRate,
    decimal brokerageRate)
{
    var interestPeriods = new List<InterestPeriod>();
    
    // Calculate base amounts
    decimal billAmount = billVm.TotalAmount;
    decimal brokerage = billAmount * brokerageRate / 100m;
    decimal netBillAmount = billAmount - brokerage;
    
    // Calculate interest if settlement is after due date
    DateTime dueDate = billVm.BillDate.AddDays(interestDays);
    decimal interest = 0;
    if (settlementDate > dueDate)
    {
        int overdueDays = (settlementDate.Date - dueDate.Date).Days;
        interest = netBillAmount * (interestRate / 100m) * (overdueDays / 365m);
         interestPeriods.Add(new InterestPeriod
    {
        StartDate = dueDate,
        EndDate = settlementDate,
        Days = overdueDays,
        Principal = netBillAmount,
        Interest = interest
    });
    }
    
    // Calculate discount if settlement is within discount period
    DateTime discountDueDate = billVm.BillDate.AddDays(discountDays);
    decimal discount = 0;
    if (settlementDate <= discountDueDate)
    {
        discount = netBillAmount * (discountRate / 100m);
    }
    
    // Total amount due for this bill
    decimal totalDue = netBillAmount + interest - discount;
    
    // Now allocate advances to this bill
    decimal remainingDue = totalDue;
    decimal totalAdvanceUsed = 0;
    var utilizationDetails = new List<AdvanceUtilizationDetail>();
    
    // Use FIFO to consume advances
    foreach (var advance in workingAdvances.Where(a => a.Amount > 0))
    {
        if (remainingDue <= 0) break;
        
        decimal amountToUse = Math.Min(advance.Amount, remainingDue);
        
        if (amountToUse > 0)
        {
            utilizationDetails.Add(new AdvanceUtilizationDetail
            {
                AdvanceID = advance.AdvanceID,
                BillID = billVm.BillID,
                AmountUsed = amountToUse,
                AdvanceDate = advance.PaymentDate,
                UtilizationDate = settlementDate,
                PaymentMethod = advance.PaymentMethod ?? "Cash",
                Reference = advance.Reference ?? ""
            });
            
            advance.Amount -= amountToUse; // Consume from advance
            remainingDue -= amountToUse;
            totalAdvanceUsed += amountToUse;
        }
    }
    
    return new IndividualBillSettlementResult
    {
        TotalDue = totalDue,
        AdvanceUsed = totalAdvanceUsed,
        CashNeeded = Math.Max(0, remainingDue),
        Interest = interest,
        Discount = discount,
        Brokerage = brokerage,
        InterestPeriods = interestPeriods,
        AdvanceUtilizations = utilizationDetails
    };
}

/// <summary>
/// Result of settlement calculation for an individual bill
/// </summary>
public class IndividualBillSettlementResult
{
    public decimal TotalDue { get; set; }
    public decimal AdvanceUsed { get; set; }
    public decimal CashNeeded { get; set; }
    public decimal Interest { get; set; }
    public decimal Discount { get; set; }
    public decimal Brokerage { get; set; }
    public List<AdvanceUtilizationDetail> AdvanceUtilizations { get; set; } = new List<AdvanceUtilizationDetail>();
    public List<InterestPeriod> InterestPeriods { get; set; } = new List<InterestPeriod>();
}

/// <summary>
/// Complete settlement calculation result for multiple bills
/// </summary>
public class SettlementCalculationResult
{
    public decimal TotalAmountDue { get; set; }
    public decimal TotalAdvanceUsed { get; set; }
    public decimal TotalCashNeeded { get; set; }
    public decimal TotalInterest { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalBrokerage { get; set; }
    public decimal AdvanceAvailable { get; set; }
    public decimal UnusedAdvance { get; set; }
    public bool CanFullySettle { get; set; }
    public DateTime PaymentDate { get; set; }
    public List<BillSettlementBreakdown> BillBreakdowns { get; set; } = new List<BillSettlementBreakdown>();
    public int? PartyID { get; set; }
    public int? BrokerID { get; set; }
}

/// <summary>
/// Breakdown of settlement for a specific bill
/// </summary>
public class BillSettlementBreakdown
{
    public int BillID { get; set; }
    public string BillNo { get; set; }
    public decimal AmountDue { get; set; }
    public decimal AdvanceUsed { get; set; }
    public decimal CashNeeded { get; set; }
    public decimal Interest { get; set; }
    public decimal Discount { get; set; }
    public decimal Brokerage { get; set; }
    public List<AdvanceUtilizationDetail> AdvanceUtilizations { get; set; } = new List<AdvanceUtilizationDetail>();
    public List<InterestPeriod> InterestPeriods { get; set; } = new List<InterestPeriod>();
}

/// <summary>
/// Details of how an advance payment was utilized for a bill
/// </summary>
public class AdvanceUtilizationDetail
{
    public int AdvanceID { get; set; }
    public int BillID { get; set; }
    public decimal AmountUsed { get; set; }
    public DateTime AdvanceDate { get; set; }
    public DateTime UtilizationDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
}
 public class InterestPeriod
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int Days { get; set; }
            public decimal Principal { get; set; }
            public decimal Interest { get; set; }
        }

    }
}
