using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;
using System.Drawing; // Added for Color

namespace SaleBillSystem.NET.Forms
{
    public partial class PaymentEntryForm : Form
    {
        private List<Party> parties;
        private List<Broker> brokers;
        private List<Bill> outstandingBills;
        private Payment currentPayment;
        private bool isEditMode = false;
        private bool isAutoAllocating = false; // Flag to prevent multiple updates during auto-allocation

        // Default constructor for new payments
        public PaymentEntryForm()
        {
            InitializeComponent();
            LoadData();
            SetupForm();
        }
        
        // Constructor for editing existing payments
        public PaymentEntryForm(int paymentId)
        {
            InitializeComponent();
            // Set edit mode flag first
            isEditMode = true;
            
            LoadData();
            SetupForm();
            
            // Load the existing payment
            LoadPayment(paymentId);
            
            this.Text = "Edit Payment";
        }

        private void LoadData()
        {
            try
            {
                parties = PartyService.GetAllParties();
                brokers = BrokerService.GetAllBrokers();
                currentPayment = new Payment();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void LoadPayment(int paymentId)
        {
            try
            {
                // Load the payment
                currentPayment = PaymentService.GetPaymentByID(paymentId);
                if (currentPayment == null)
                {
                    MessageBox.Show("Payment not found.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }
                
                // Set form controls with payment data
                dtpPaymentDate.Value = currentPayment.PaymentDate;
                txtPaymentAmount.Text = currentPayment.PaymentAmount.ToString("N2");
                cmbPaymentMethod.Text = currentPayment.PaymentMethod;
                txtReference.Text = currentPayment.Reference;
                txtNotes.Text = currentPayment.Notes;
                
                // Auto-select party or broker based on the payment details
                if (currentPayment.PaymentDetails.Count > 0)
                {
                    var firstDetail = currentPayment.PaymentDetails.First();
                    var bill = BillService.GetBillByID(firstDetail.BillID);
                    
                    if (bill != null)
                    {
                        // Try to select by party first
                        if (bill.PartyID > 0)
                        {
                            rbFilterByParty.Checked = true;
                            cmbParty.SelectedValue = bill.PartyID;
                        }
                        // If no party, try broker
                        else if (bill.BrokerID > 0)
                        {
                            rbFilterByBroker.Checked = true;
                            cmbBroker.SelectedValue = bill.BrokerID;
                        }
                    }
                }
                
                // Load all associated bills
                LoadAssociatedBills();
                
                // Keep payment amount editable in edit mode
                txtPaymentAmount.ReadOnly = false;
                btnAutoAllocate.Enabled = true;
                
                // Update form title
                this.Text = "Edit Payment";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading payment: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }
        
        private void LoadAssociatedBills()
        {
            try
            {
                // Initialize outstandingBills to ensure it's never null
                outstandingBills = new List<Bill>();
                
                if (isEditMode && currentPayment != null && currentPayment.PaymentDetails != null)
                {
                    // In edit mode, load bills that are either:
                    // 1. Already allocated in this payment
                    // 2. Currently outstanding for the selected party/broker
                    
                    // First, get all bills that are already allocated in this payment
                    foreach (var detail in currentPayment.PaymentDetails)
                    {
                        if (detail != null)
                        {
                            Bill bill = BillService.GetBillByID(detail.BillID);
                            if (bill != null)
                            {
                                // Temporarily adjust paid amount to exclude this payment's allocation
                                // so we can see the correct balance for editing
                                bill.PaidAmount -= detail.AllocatedAmount;
                                outstandingBills.Add(bill);
                            }
                        }
                    }
                    
                    // Then, add any other outstanding bills for the same party/broker
                    List<Bill> additionalBills = new List<Bill>();
                    if (rbFilterByParty.Checked && cmbParty.SelectedValue is int partyId && partyId > 0)
                    {
                        var bills = PaymentService.GetOutstandingBillsByParty(partyId);
                        additionalBills = bills ?? new List<Bill>();
                    }
                    else if (rbFilterByBroker.Checked && cmbBroker.SelectedValue is int brokerId && brokerId > 0)
                    {
                        var bills = PaymentService.GetOutstandingBillsByBroker(brokerId);
                        additionalBills = bills ?? new List<Bill>();
                    }
                    
                    // Add bills that aren't already in the list
                    foreach (var bill in additionalBills)
                    {
                        if (bill != null && !outstandingBills.Any(b => b.BillID == bill.BillID))
                        {
                            outstandingBills.Add(bill);
                        }
                    }
                    
                    // Create display data with existing payment allocations
                    var billData = outstandingBills.Select(b => 
                    {
                        var (interestAmount, discountAmount, netPayableAmount) = PaymentService.CalculateInterestAndDiscount(b, dtpPaymentDate.Value);
                        
                        // Safely check for payment details matching this bill
                        double allocatedAmount = 0.0;
                        if (currentPayment != null && currentPayment.PaymentDetails != null)
                        {
                            var paymentDetail = currentPayment.PaymentDetails
                                .FirstOrDefault(pd => pd != null && pd.BillID == b.BillID);
                            if (paymentDetail != null)
                            {
                                allocatedAmount = paymentDetail.AllocatedAmount;
                            }
                        }
                        
                        return new
                        {
                            BillID = b.BillID,
                            BillNo = b.BillNo,
                            BillDate = b.BillDate,
                            DueDate = b.DueDate,
                            PartyName = b.PartyName,
                            NetAmount = b.NetAmount,
                            InterestAmount = interestAmount,
                            DiscountAmount = discountAmount,
                            NetPayableAmount = netPayableAmount,
                            PaidAmount = b.PaidAmount,
                            BalanceAmount = netPayableAmount - b.PaidAmount,
                            PaymentAmount = allocatedAmount
                        };
                    }).ToList();

                    dgvBills.DataSource = billData;
                }
                else
                {
                    // In new payment mode or when payment details are null, load outstanding bills normally
                    if (rbFilterByParty.Checked && cmbParty.SelectedValue is int partyId && partyId > 0)
                    {
                        var bills = PaymentService.GetOutstandingBillsByParty(partyId);
                        outstandingBills = bills ?? new List<Bill>();
                    }
                    else if (rbFilterByBroker.Checked && cmbBroker.SelectedValue is int brokerId && brokerId > 0)
                    {
                        var bills = PaymentService.GetOutstandingBillsByBroker(brokerId);
                        outstandingBills = bills ?? new List<Bill>();
                    }

                    // Ensure outstandingBills is not null before using it
                    if (outstandingBills == null)
                    {
                        outstandingBills = new List<Bill>();
                    }

                    // Create display data with payment amount column
                    var billData = outstandingBills.Select(b => 
                    {
                        var (interestAmount, discountAmount, netPayableAmount) = PaymentService.CalculateInterestAndDiscount(b, dtpPaymentDate.Value);
                        
                        return new
                        {
                            BillID = b.BillID,
                            BillNo = b.BillNo,
                            BillDate = b.BillDate,
                            DueDate = b.DueDate,
                            PartyName = b.PartyName,
                            NetAmount = b.NetAmount,
                            InterestAmount = interestAmount,
                            DiscountAmount = discountAmount,
                            NetPayableAmount = netPayableAmount,
                            PaidAmount = b.PaidAmount,
                            BalanceAmount = netPayableAmount - b.PaidAmount,
                            PaymentAmount = 0.0
                        };
                    }).ToList();

                    dgvBills.DataSource = billData;
                }
                
                groupBoxBills.Enabled = outstandingBills.Count > 0;
                
                lblTotalBills.Text = $"Bills: {outstandingBills.Count}";
                lblTotalOutstanding.Text = $"Total Outstanding: ₹{outstandingBills.Sum(b => b.BalanceAmount):N2}";
                
                UpdatePaymentSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bills: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Ensure outstandingBills is initialized even if an error occurs
                if (outstandingBills == null)
                {
                    outstandingBills = new List<Bill>();
                }
            }
        }

        private void SetupForm()
        {
            this.Text = "Payment Entry";
            this.KeyPreview = true; // Enable form to receive key events first
            
            // Setup party combo box
            SetupPartyComboBox();
            
            // Setup broker combo box
            SetupBrokerComboBox();
            
            // Setup payment method combo box
            SetupPaymentMethodComboBox();
            
            // Setup data grid
            SetupDataGrid();
            
            // Set default values
            dtpPaymentDate.Value = DateTime.Today;
            txtPaymentAmount.Text = "0.00";
            
            // Initially hide bill selection controls
            groupBoxBills.Enabled = false;
            
            // Setup event handlers
            rbFilterByParty.CheckedChanged += FilterType_CheckedChanged;
            rbFilterByBroker.CheckedChanged += FilterType_CheckedChanged;
            cmbParty.SelectedIndexChanged += Party_SelectedIndexChanged;
            cmbBroker.SelectedIndexChanged += Broker_SelectedIndexChanged;
            txtPaymentAmount.TextChanged += PaymentAmount_TextChanged;
            dgvBills.CellValueChanged += DgvBills_CellValueChanged;
            dgvBills.CellEndEdit += DgvBills_CellEndEdit;
            dtpPaymentDate.ValueChanged += DtpPaymentDate_ValueChanged; // Added for date change
            
            // Add KeyDown event handler for keyboard shortcuts
            this.KeyDown += PaymentEntryForm_KeyDown;
            
            // Set tab order for better keyboard navigation
            SetTabOrder();
        }
        
        private void SetTabOrder()
        {
            // Set tab order for logical keyboard navigation
            dtpPaymentDate.TabIndex = 0;
            txtPaymentAmount.TabIndex = 1;
            cmbPaymentMethod.TabIndex = 2;
            txtReference.TabIndex = 3;
            rbFilterByParty.TabIndex = 4;
            rbFilterByBroker.TabIndex = 5;
            cmbParty.TabIndex = 6;
            cmbBroker.TabIndex = 7;
            dgvBills.TabIndex = 8;
            btnAutoAllocate.TabIndex = 9;
            btnClearAllocation.TabIndex = 10;
            txtNotes.TabIndex = 11;
            btnSave.TabIndex = 12;
            btnCancel.TabIndex = 13;
        }
        
        private void PaymentEntryForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                // Ctrl+S: Save
                btnSave_Click(sender, e);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                // Escape: Cancel
                btnCancel_Click(sender, e);
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.A)
            {
                // Ctrl+A: Auto Allocate
                btnAutoAllocate_Click(sender, e);
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.R)
            {
                // Ctrl+R: Clear Allocation
                btnClearAllocation_Click(sender, e);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F1)
            {
                // F1: Help
                ShowPaymentEntryHelp();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                // F5: Refresh bill list
                LoadOutstandingBills();
                e.SuppressKeyPress = true;
            }
            else if (e.Alt && e.KeyCode == Keys.P)
            {
                // Alt+P: Switch to Party filter
                rbFilterByParty.Checked = true;
                cmbParty.Focus();
                e.SuppressKeyPress = true;
            }
            else if (e.Alt && e.KeyCode == Keys.B)
            {
                // Alt+B: Switch to Broker filter
                rbFilterByBroker.Checked = true;
                cmbBroker.Focus();
                e.SuppressKeyPress = true;
            }
        }
        
        private void ShowPaymentEntryHelp()
        {
            MessageBox.Show(
                "Payment Entry Keyboard Shortcuts:\n\n" +
                "Ctrl+S: Save Payment\n" +
                "Escape: Cancel\n" +
                "Ctrl+A: Auto Allocate Payment\n" +
                "Ctrl+R: Clear Allocation\n" +
                "F5: Refresh Bill List\n" +
                "Alt+P: Switch to Party Filter\n" +
                "Alt+B: Switch to Broker Filter\n" +
                "F1: Show this help\n\n" +
                "Navigation:\n" +
                "Tab: Move to next field\n" +
                "Shift+Tab: Move to previous field\n" +
                "Enter: In DataGrid, move to next cell\n" +
                "Arrow Keys: Navigate in DataGrid",
                "Payment Entry Help",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void SetupPartyComboBox()
        {
            var partyList = new List<Party> { new Party { PartyID = 0, PartyName = "-- Select Party --" } };
            partyList.AddRange(parties);

            cmbParty.DataSource = partyList;
            cmbParty.DisplayMember = "PartyName";
            cmbParty.ValueMember = "PartyID";
            cmbParty.SelectedValue = 0;
        }

        private void SetupBrokerComboBox()
        {
            var brokerList = new List<Broker> { new Broker { BrokerID = 0, BrokerName = "-- Select Broker --" } };
            brokerList.AddRange(brokers);

            cmbBroker.DataSource = brokerList;
            cmbBroker.DisplayMember = "BrokerName";
            cmbBroker.ValueMember = "BrokerID";
            cmbBroker.SelectedValue = 0;
        }

        private void SetupPaymentMethodComboBox()
        {
            var methods = new[] { "Cash", "Cheque", "Bank Transfer", "UPI", "Card", "Other" };
            cmbPaymentMethod.DataSource = methods;
            cmbPaymentMethod.SelectedIndex = 0;
        }

        private void SetupDataGrid()
        {
            dgvBills.AutoGenerateColumns = false;
            dgvBills.AllowUserToAddRows = false;
            dgvBills.AllowUserToDeleteRows = false;
            dgvBills.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dgvBills.Columns.Clear();
            
            // Hidden BillID column
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BillID",
                DataPropertyName = "BillID",
                Visible = false
            });

            // Bill No
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BillNo",
                HeaderText = "Bill No",
                DataPropertyName = "BillNo",
                Width = 120,
                ReadOnly = true
            });

            // Bill Date
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BillDate",
                HeaderText = "Bill Date",
                DataPropertyName = "BillDate",
                Width = 100,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            });
            
            // Due Date
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DueDate",
                HeaderText = "Due Date",
                DataPropertyName = "DueDate",
                Width = 100,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            });

            // Party Name
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PartyName",
                HeaderText = "Party",
                DataPropertyName = "PartyName",
                Width = 150,
                ReadOnly = true
            });

            // Bill Amount
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "NetAmount",
                HeaderText = "Bill Amount",
                DataPropertyName = "NetAmount",
                Width = 90,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            // Interest Amount
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "InterestAmount",
                HeaderText = "Interest",
                DataPropertyName = "InterestAmount",
                Width = 80,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, ForeColor = Color.Red }
            });

            // Discount Amount
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DiscountAmount",
                HeaderText = "Discount",
                DataPropertyName = "DiscountAmount",
                Width = 80,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, ForeColor = Color.Green }
            });

            // Net Payable Amount
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "NetPayableAmount",
                HeaderText = "Net Payable",
                DataPropertyName = "NetPayableAmount",
                Width = 100,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold) }
            });

            // Previous Paid
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PaidAmount",
                HeaderText = "Previous Paid",
                DataPropertyName = "PaidAmount",
                Width = 100,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            // Balance Before
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BalanceAmount",
                HeaderText = "Balance",
                DataPropertyName = "BalanceAmount",
                Width = 100,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            // Payment Amount (Editable)
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PaymentAmount",
                HeaderText = "Payment Amount",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
        }

        private void FilterType_CheckedChanged(object sender, EventArgs e)
        {
            if (rbFilterByParty.Checked)
            {
                cmbParty.Enabled = true;
                cmbBroker.Enabled = false;
                cmbBroker.SelectedValue = 0;
            }
            else if (rbFilterByBroker.Checked)
            {
                cmbParty.Enabled = false;
                cmbBroker.Enabled = true;
                cmbParty.SelectedValue = 0;
            }
            
            LoadOutstandingBills();
        }

        private void Party_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadOutstandingBills();
        }

        private void Broker_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadOutstandingBills();
        }

        private void LoadOutstandingBills()
        {
            try
            {
                if (isEditMode)
                {
                    // In edit mode, use the specialized method that handles existing allocations
                    LoadAssociatedBills();
                    return;
                }
                
                // Initialize outstandingBills to ensure it's never null
                outstandingBills = new List<Bill>();

                if (rbFilterByParty.Checked && cmbParty.SelectedValue is int partyId && partyId > 0)
                {
                    var bills = PaymentService.GetOutstandingBillsByParty(partyId);
                    outstandingBills = bills ?? new List<Bill>();
                }
                else if (rbFilterByBroker.Checked && cmbBroker.SelectedValue is int brokerId && brokerId > 0)
                {
                    var bills = PaymentService.GetOutstandingBillsByBroker(brokerId);
                    outstandingBills = bills ?? new List<Bill>();
                }

                // Use the new RefreshBillsList method to calculate interest/discount
                RefreshBillsList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bills: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Ensure outstandingBills is initialized even if an error occurs
                if (outstandingBills == null)
                {
                    outstandingBills = new List<Bill>();
                }
            }
        }

        private void RefreshBillsList()
        {
            try
            {
                // Ensure outstandingBills is not null
                if (outstandingBills == null)
                {
                    outstandingBills = new List<Bill>();
                }

                // Create display data with calculated interest/discount based on payment date
                var billData = outstandingBills.Select(b => 
                {
                    var (interestAmount, discountAmount, netPayableAmount) = PaymentService.CalculateInterestAndDiscount(b, dtpPaymentDate.Value);
                    
                    return new
                    {
                        BillID = b.BillID,
                        BillNo = b.BillNo,
                        BillDate = b.BillDate,
                        DueDate = b.DueDate,
                        PartyName = b.PartyName,
                        NetAmount = b.NetAmount,
                        InterestAmount = interestAmount,
                        DiscountAmount = discountAmount,
                        NetPayableAmount = netPayableAmount,
                        PaidAmount = b.PaidAmount,
                        BalanceAmount = netPayableAmount - b.PaidAmount,
                        PaymentAmount = 0.0
                    };
                }).ToList();

                dgvBills.DataSource = billData;
                groupBoxBills.Enabled = outstandingBills.Count > 0;
                
                lblTotalBills.Text = $"Outstanding Bills: {outstandingBills.Count}";
                lblTotalOutstanding.Text = $"Total Outstanding: ₹{billData.Sum(b => b.BalanceAmount):N2}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bills: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PaymentAmount_TextChanged(object sender, EventArgs e)
        {
            // Skip processing if this change was triggered programmatically
            if (isAutoAllocating)
                return;
            
            // Auto-allocate for both new payments and when editing existing payments
            // The AutoAllocatePayment method will handle preserving allocations in edit mode
            AutoAllocatePayment();
        }

        private void AutoAllocatePayment()
        {
            // Set flag to prevent recursive updates
            isAutoAllocating = true;
            
            try
            {
                if (!double.TryParse(txtPaymentAmount.Text, out double paymentAmount) || paymentAmount <= 0)
                {
                    // Clear all payment amounts if invalid
                    foreach (DataGridViewRow row in dgvBills.Rows)
                    {
                        if (row.Cells["PaymentAmount"] != null)
                        {
                            row.Cells["PaymentAmount"].Value = 0.0;
                        }
                    }
                    UpdatePaymentSummary();
                    return;
                }

                double remainingAmount = paymentAmount;

                // Get current bill data with calculated interest/discount
                var billData = dgvBills.DataSource as IEnumerable<dynamic>;
                if (billData == null) return;

                var billList = billData.ToList();

                if (isEditMode)
                {
                    // In edit mode, try to preserve existing allocations
                    // Calculate total currently allocated
                    double currentlyAllocated = 0;
                    foreach (DataGridViewRow row in dgvBills.Rows)
                    {
                        if (row.Cells["PaymentAmount"].Value != null)
                        {
                            currentlyAllocated += Convert.ToDouble(row.Cells["PaymentAmount"].Value);
                        }
                    }

                    // If the new amount differs from the currently allocated amount
                    if (Math.Abs(paymentAmount - currentlyAllocated) > 0.01)
                    {
                        // If new amount is greater, allocate the difference to oldest bills with balance
                        if (paymentAmount > currentlyAllocated)
                        {
                            double additionalAmount = paymentAmount - currentlyAllocated;
                            
                            // Allocate additional amount to bills with remaining balance
                            for (int i = 0; i < dgvBills.Rows.Count && additionalAmount > 0; i++)
                            {
                                var row = dgvBills.Rows[i];
                                var billInfo = billList[i];
                                
                                double currentPayment = Convert.ToDouble(row.Cells["PaymentAmount"].Value);
                                double balanceAmount = billInfo.BalanceAmount;
                                double remainingBalance = balanceAmount - currentPayment;
                                
                                if (remainingBalance > 0)
                                {
                                    double additionalAllocation = Math.Min(additionalAmount, remainingBalance);
                                    row.Cells["PaymentAmount"].Value = currentPayment + additionalAllocation;
                                    additionalAmount -= additionalAllocation;
                                }
                            }
                        }
                        // If new amount is less, reduce allocations proportionally
                        else
                        {
                            double reductionFactor = paymentAmount / currentlyAllocated;
                            
                            // Apply reduction to all rows with allocations
                            foreach (DataGridViewRow row in dgvBills.Rows)
                            {
                                if (row.Cells["PaymentAmount"].Value != null)
                                {
                                    double currentValue = Convert.ToDouble(row.Cells["PaymentAmount"].Value);
                                    if (currentValue > 0)
                                    {
                                        double newValue = Math.Round(currentValue * reductionFactor, 2);
                                        row.Cells["PaymentAmount"].Value = newValue;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // For new payments, allocate from scratch
                    // Allocate payment to bills in order (oldest first)
                    for (int i = 0; i < dgvBills.Rows.Count && remainingAmount > 0; i++)
                    {
                        var row = dgvBills.Rows[i];
                        var billInfo = billList[i];
                        
                        double balanceAmount = billInfo.BalanceAmount;
                        double allocationAmount = Math.Min(remainingAmount, balanceAmount);
                        
                        row.Cells["PaymentAmount"].Value = allocationAmount;
                        remainingAmount -= allocationAmount;
                    }

                    // Clear remaining rows if payment is less than total outstanding
                    for (int i = 0; i < dgvBills.Rows.Count; i++)
                    {
                        var row = dgvBills.Rows[i];
                        if (row.Cells["PaymentAmount"].Value == null)
                        {
                            row.Cells["PaymentAmount"].Value = 0.0;
                        }
                    }
                }

                UpdatePaymentSummary();
            }
            finally
            {
                isAutoAllocating = false;
            }
        }
        
        private void btnAutoAllocate_Click(object sender, EventArgs e)
        {
            // First clear all existing allocations
            isAutoAllocating = true;
            
            try
            {
                // Clear existing allocations first
                foreach (DataGridViewRow row in dgvBills.Rows)
                {
                    row.Cells["PaymentAmount"].Value = 0.0;
                }
                
                // Get current payment amount - this stays the same
                double currentAmount = 0;
                if (double.TryParse(txtPaymentAmount.Text, out double amount))
                {
                    currentAmount = amount;
                }
                
                // Then perform auto allocation from scratch
                AutoAllocateFromScratch(currentAmount);
            }
            finally
            {
                isAutoAllocating = false;
            }
        }
        
        private void AutoAllocateFromScratch(double paymentAmount)
        {
            if (paymentAmount <= 0)
            {
                UpdatePaymentSummary();
                return;
            }

            double remainingAmount = paymentAmount;

            // Get current bill data with calculated interest/discount
            var billData = dgvBills.DataSource as IEnumerable<dynamic>;
            if (billData == null) return;

            var billList = billData.ToList();

            // Allocate payment to bills in order (oldest first)
            for (int i = 0; i < dgvBills.Rows.Count && remainingAmount > 0; i++)
            {
                var row = dgvBills.Rows[i];
                var billInfo = billList[i];
                
                double balanceAmount = billInfo.BalanceAmount;
                double allocationAmount = Math.Min(remainingAmount, balanceAmount);
                
                row.Cells["PaymentAmount"].Value = allocationAmount;
                remainingAmount -= allocationAmount;
            }

            UpdatePaymentSummary();
        }
        
        private void btnClearAllocation_Click(object sender, EventArgs e)
        {
            isAutoAllocating = true;
            
            try
            {
                // Clear all payment allocations
                foreach (DataGridViewRow row in dgvBills.Rows)
                {
                    row.Cells["PaymentAmount"].Value = 0.0;
                }
                
                // Update the payment amount to zero as well if not in edit mode
                if (!isEditMode)
                {
                    txtPaymentAmount.Text = "0.00";
                }
                
                UpdatePaymentSummary();
            }
            finally
            {
                isAutoAllocating = false;
            }
        }

        private void DgvBills_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (isAutoAllocating) return; // Skip updates during auto-allocation
            
            if (e.ColumnIndex >= 0 && dgvBills.Columns[e.ColumnIndex].Name == "PaymentAmount")
            {
                // Validate the value is numeric
                var cell = dgvBills.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (cell.Value != null)
                {
                    if (!double.TryParse(cell.Value.ToString(), out double amount))
                    {
                        // Reset to 0 if not a valid number
                        cell.Value = 0.0;
                    }
                    else if (amount < 0)
                    {
                        // Ensure payment amount is not negative
                        cell.Value = 0.0;
                    }
                    else
                    {
                        // Ensure payment amount doesn't exceed bill balance
                        var balanceCell = dgvBills.Rows[e.RowIndex].Cells["BalanceAmount"];
                        if (balanceCell != null && double.TryParse(balanceCell.Value.ToString(), out double balance))
                        {
                            if (amount > balance)
                            {
                                cell.Value = balance;
                            }
                        }
                    }
                }
                
                // Update the payment summary and the top payment amount
                double totalAllocated = CalculateTotalAllocated();
                
                // Only update the top payment amount if not in edit mode
                if (!isEditMode)
                {
                    // Update the payment amount textbox with the total allocated
                    // Set isAutoAllocating to prevent recursive updates
                    isAutoAllocating = true;
                    txtPaymentAmount.Text = totalAllocated.ToString("N2");
                    isAutoAllocating = false;
                }
                
                UpdatePaymentSummary();
            }
        }

        private void DgvBills_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // Skip if this is not the payment amount column
            if (e.ColumnIndex < 0 || dgvBills.Columns[e.ColumnIndex].Name != "PaymentAmount")
                return;
                
            // Ensure the value is valid
            var cell = dgvBills.Rows[e.RowIndex].Cells[e.ColumnIndex];
            if (cell.Value == null || string.IsNullOrWhiteSpace(cell.Value.ToString()))
            {
                cell.Value = 0.0;
            }
            
            // Make sure the payment amount is valid and doesn't exceed the balance
            if (double.TryParse(cell.Value.ToString(), out double amount))
            {
                // Get the balance amount for this bill
                var balanceCell = dgvBills.Rows[e.RowIndex].Cells["BalanceAmount"];
                if (balanceCell != null && double.TryParse(balanceCell.Value.ToString(), out double balance))
                {
                    if (amount > balance)
                    {
                        cell.Value = balance;
                        MessageBox.Show(
                            "Payment amount cannot exceed the outstanding balance. Amount has been adjusted.",
                            "Payment Amount Adjusted",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
            }
            else
            {
                cell.Value = 0.0;
            }
        }

        private double CalculateTotalAllocated()
        {
            double totalAllocated = 0;
            
            foreach (DataGridViewRow row in dgvBills.Rows)
            {
                if (row.Cells["PaymentAmount"].Value != null)
                {
                    totalAllocated += Convert.ToDouble(row.Cells["PaymentAmount"].Value);
                }
            }
            
            return totalAllocated;
        }

        private void UpdatePaymentSummary()
        {
            double totalAllocated = CalculateTotalAllocated();
            lblAllocatedAmount.Text = $"Allocated: ₹{totalAllocated:N2}";
            
            if (double.TryParse(txtPaymentAmount.Text, out double paymentAmount))
            {
                double unallocatedAmount = paymentAmount - totalAllocated;
                lblUnallocatedAmount.Text = $"Unallocated: ₹{unallocatedAmount:N2}";
                lblUnallocatedAmount.ForeColor = unallocatedAmount == 0 ? Color.Green : Color.Red;
            }
            else
            {
                lblUnallocatedAmount.Text = "Unallocated: ₹0.00";
                lblUnallocatedAmount.ForeColor = Color.Black;
            }
        }

        private void DtpPaymentDate_ValueChanged(object sender, EventArgs e)
        {
            // Recalculate interest and discount when payment date changes
            RefreshBillsList();
            
            // Re-allocate payment if there's an amount entered
            if (!string.IsNullOrWhiteSpace(txtPaymentAmount.Text))
            {
                PaymentAmount_TextChanged(txtPaymentAmount, EventArgs.Empty);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidatePayment())
                return;

            // Validate payment date is within financial year
            if (!CompanyService.IsDateWithinFinancialYear(dtpPaymentDate.Value.Date))
            {
                MessageBox.Show("Payment date must be within the current financial year.", "Date Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpPaymentDate.Focus();
                return;
            }
            
            try
            {
                // Create payment object
                currentPayment.PaymentDate = dtpPaymentDate.Value;
                currentPayment.PaymentAmount = Convert.ToDouble(txtPaymentAmount.Text);
                currentPayment.PaymentMethod = cmbPaymentMethod.Text;
                currentPayment.Reference = txtReference.Text.Trim();
                currentPayment.Notes = txtNotes.Text.Trim();

                // Create payment details
                currentPayment.PaymentDetails.Clear();
                foreach (DataGridViewRow row in dgvBills.Rows)
                {
                    try
                    {
                        if (row.Cells["PaymentAmount"].Value != null && 
                            double.TryParse(row.Cells["PaymentAmount"].Value.ToString(), out double amount) && 
                            amount > 0)
                        {
                            // Make sure to use the correct case for the BillID column
                            int billId = Convert.ToInt32(row.Cells["BillID"].Value);
                            Bill bill = outstandingBills.First(b => b.BillID == billId);

                            // Calculate interest and discount for this bill
                            var (interestAmount, discountAmount, netPayableAmount) = PaymentService.CalculateInterestAndDiscount(bill, dtpPaymentDate.Value);
                            
                            PaymentDetail detail = new PaymentDetail
                            {
                                BillID = billId,
                                BillNo = bill.BillNo,
                                PartyName = bill.PartyName,
                                BillAmount = netPayableAmount, // Use calculated net payable amount
                                PreviousPaid = bill.PaidAmount,
                                BalanceBefore = netPayableAmount - bill.PaidAmount,
                                AllocatedAmount = amount,
                                BalanceAfter = (netPayableAmount - bill.PaidAmount) - amount
                            };

                            currentPayment.PaymentDetails.Add(detail);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error processing bill row: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                if (currentPayment.PaymentDetails.Count == 0)
                {
                    MessageBox.Show("No payment allocations have been made. Please allocate payments to at least one bill.", 
                        "No Allocations", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Save the payment - either new or existing
                string successMessage = isEditMode ? 
                    "Payment updated successfully!" : 
                    "Payment saved successfully!";
                
                if (PaymentService.SavePayment(currentPayment))
                {
                    MessageBox.Show(successMessage, "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to save payment.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving payment: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidatePayment()
        {
            if (!double.TryParse(txtPaymentAmount.Text, out double amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid payment amount.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPaymentAmount.Focus();
                return false;
            }

            if (currentPayment.PaymentDetails.Count == 0)
            {
                // Check if any bills have payment allocated
                bool hasAllocation = false;
                foreach (DataGridViewRow row in dgvBills.Rows)
                {
                    if (row.Cells["PaymentAmount"].Value != null && 
                        double.TryParse(row.Cells["PaymentAmount"].Value.ToString(), out double rowAmount) && 
                        rowAmount > 0)
                    {
                        hasAllocation = true;
                        break;
                    }
                }

                if (!hasAllocation)
                {
                    MessageBox.Show("Please allocate payment to at least one bill.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            return true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
} 