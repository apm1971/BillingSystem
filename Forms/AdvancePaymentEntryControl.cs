using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class AdvancePaymentEntryControl : UserControl
    {
        public event EventHandler? CloseRequested;

        private List<Party> _parties = new List<Party>();
        private List<Broker> _brokers = new List<Broker>();
        private List<AdvancePayment> _advancePayments = new List<AdvancePayment>();
        private List<AdvancePayment> _allAdvancePayments = new List<AdvancePayment>();
        private bool _dateFilterApplied = false;
        private string? _sessionDate = null; // Persists payment date across saves within a session

        public AdvancePaymentEntryControl()
        {
            InitializeComponent();
        }

        private void AdvancePaymentEntryControl_Load(object sender, EventArgs e)
        {
            SetupForm();
            LoadInitialData();
            SetupEventHandlers();
            ClearForm();
        }

        #region Initial Setup

        private void SetupForm()
        {
            this.Dock = DockStyle.Fill;
            
            // Set default values
            txtPaymentDate.Text = DateTime.Now.ToString("dd-MM-yyyy");
            
            // Setup date filter controls with null checks
            if (dtpFromDate != null)
            {
                dtpFromDate.Value = DateTime.Now.AddDays(-30); // Default to last 30 days
            }
            if (dtpToDate != null)
            {
                dtpToDate.Value = DateTime.Now;
            }
            
            // Setup payment method combo box
            cmbPaymentMethod.Items.AddRange(new string[] 
            { 
                "Cash", "Cheque"
            });
            cmbPaymentMethod.SelectedIndex = 0; // Default to Cash
            
            // Setup payment method filter combo box
            if (cmbPaymentMethodFilter != null)
            {
                cmbPaymentMethodFilter.Items.AddRange(new string[] 
                { 
                    "All", "Cash", "Cheque", "Reversal"
                });
                cmbPaymentMethodFilter.SelectedIndex = 0; // Default to All
            }
            
            SetupDataGridView();
        }

        private void SetupDataGridView()
        {
            dgvAdvancePayments.AutoGenerateColumns = false;
            dgvAdvancePayments.AllowUserToAddRows = false;
            dgvAdvancePayments.AllowUserToDeleteRows = false;
            dgvAdvancePayments.ReadOnly = true;
            dgvAdvancePayments.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAdvancePayments.MultiSelect = false;
            dgvAdvancePayments.RowHeadersVisible = false;
            dgvAdvancePayments.BackgroundColor = Color.White;
            dgvAdvancePayments.DefaultCellStyle.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular);
            dgvAdvancePayments.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvAdvancePayments.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvAdvancePayments.RowTemplate.Height = 28;

            dgvAdvancePayments.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            dgvAdvancePayments.DefaultCellStyle.SelectionForeColor = Color.White;

            dgvAdvancePayments.Columns.Clear();

            // Define columns
            dgvAdvancePayments.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "AdvanceID", 
                HeaderText = "ID", 
                Visible = false, 
                ReadOnly = true 
            });
            
            dgvAdvancePayments.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "PaymentDate", 
                HeaderText = "Date", 
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd-MMM-yyyy" }, 
                Width = 120, 
                ReadOnly = true 
            });
            
            dgvAdvancePayments.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "PartyName", 
                HeaderText = "Party", 
                Width = 200, 
                ReadOnly = true 
            });
            
            dgvAdvancePayments.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "BrokerName", 
                HeaderText = "Broker", 
                Width = 150, 
                ReadOnly = true 
            });
            
            dgvAdvancePayments.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "Amount", 
                HeaderText = "Available", 
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Format = "N2", 
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Font = new Font("Segoe UI", 9.75F, FontStyle.Bold),
                    ForeColor = Color.Green
                }, 
                Width = 120, 
                ReadOnly = true 
            });
            
            dgvAdvancePayments.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "OriginalAmount", 
                HeaderText = "Original", 
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Format = "N2", 
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    ForeColor = Color.Blue
                }, 
                Width = 100, 
                ReadOnly = true 
            });
            
            dgvAdvancePayments.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "UtilizedAmount", 
                HeaderText = "Used", 
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Format = "N2", 
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    ForeColor = Color.Red
                }, 
                Width = 100, 
                ReadOnly = true 
            });
            
            dgvAdvancePayments.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "PaymentMethod", 
                HeaderText = "Method", 
                Width = 100, 
                ReadOnly = true 
            });
            
            dgvAdvancePayments.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "ChequeAmountFirm1", 
                HeaderText = "Firm 1", 
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Format = "N2", 
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    BackColor = Color.LightCyan
                }, 
                Width = 100, 
                ReadOnly = true 
            });
            
            dgvAdvancePayments.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "ChequeAmountFirm2", 
                HeaderText = "Firm 2", 
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Format = "N2", 
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    BackColor = Color.LightCyan
                }, 
                Width = 100, 
                ReadOnly = true 
            });
            
            dgvAdvancePayments.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                DataPropertyName = "Reference", 
                HeaderText = "Reference", 
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, 
                ReadOnly = true 
            });
        }

        private void LoadInitialData()
        {
            try
            {
                // Initialize advance payment system
                DatabaseManager.InitializeAdvancePaymentSystem();
                
                // Load parties and brokers with autocomplete
                LoadParties();
                LoadBrokers();
                LoadAdvancePayments();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading initial data: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadParties()
        {
            try
            {
                _parties = PartyService.GetAllParties();
                
                cmbParty.DataSource = _parties;
                cmbParty.DisplayMember = "PartyName";
                cmbParty.ValueMember = "PartyID";
                cmbParty.SelectedIndex = -1; // No selection by default
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading parties: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadBrokers()
        {
            try
            {
                _brokers = BrokerService.GetAllBrokers();
                
                cmbBroker.DataSource = _brokers;
                cmbBroker.DisplayMember = "BrokerName";
                cmbBroker.ValueMember = "BrokerID";
                cmbBroker.SelectedIndex = -1; // No selection by default
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading brokers: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadAdvancePayments()
        {
            try
            {
                // Load all advance payments but don't display them initially
                _allAdvancePayments = AdvancePaymentService.GetAllAdvancePayments();
                
                // Show empty grid initially
                _advancePayments = new List<AdvancePayment>();
                dgvAdvancePayments.DataSource = _advancePayments;
                
                // Update total label to show instruction
                lblTotalAdvances.Text = "Select date range and click 'Apply Filter' to view payments";
                _dateFilterApplied = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading advance payments: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupEventHandlers()
        {
            btnSave.Click += BtnSave_Click;
            btnClear.Click += BtnClear_Click;
            btnDelete.Click += BtnDelete_Click;
            
            // Add event handler for date filter button with null check
            if (btnApplyDateFilter != null)
            {
                btnApplyDateFilter.Click += BtnApplyDateFilter_Click;
            }
            
            // Add event handler for payment method filter with null check
            if (cmbPaymentMethodFilter != null)
            {
                cmbPaymentMethodFilter.SelectedIndexChanged += CmbPaymentMethodFilter_SelectedIndexChanged;
            }
            
            cmbParty.SelectedIndexChanged += CmbParty_SelectedIndexChanged;
            cmbBroker.SelectedIndexChanged += CmbBroker_SelectedIndexChanged;
            txtPaymentDate.TextChanged += TxtPaymentDate_TextChanged;
            cmbPaymentMethod.SelectedIndexChanged += CmbPaymentMethod_SelectedIndexChanged;
            txtChequeAmountFirm1.TextChanged += TxtChequeAmount_TextChanged;
            txtChequeAmountFirm2.TextChanged += TxtChequeAmount_TextChanged;
            dgvAdvancePayments.KeyDown += DgvAdvancePayments_KeyDown;
        }

        private void ClearForm()
        {
            cmbParty.SelectedIndex = -1;
            cmbBroker.SelectedIndex = -1;
            
            // Reset payment method filter to "All"
            if (cmbPaymentMethodFilter != null)
            {
                cmbPaymentMethodFilter.SelectedIndex = 0; // "All"
            }
            
            txtPaymentDate.Text = _sessionDate ?? DateTime.Now.ToString("dd-MM-yyyy");
            nudAmount.Value = 0;
            cmbPaymentMethod.SelectedIndex = 0;
            txtReference.Clear();
            txtChequeAmountFirm1.Text = "0.00";
            txtChequeAmountFirm2.Text = "0.00";
            pnlChequeDetails.Visible = false;
            cmbParty.Focus();
        }

        #endregion

        #region Event Handlers

        private void TxtPaymentDate_TextChanged(object sender, EventArgs e)
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

        private void CmbParty_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterAdvancePayments();
        }

        private void CmbBroker_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterAdvancePayments();
        }

        private void CmbPaymentMethodFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterAdvancePayments();
        }

        private void FilterAdvancePayments()
        {
            try
            {
                // Only filter if date filter has been applied
                if (!_dateFilterApplied)
                {
                    lblTotalAdvances.Text = "Select date range and click 'Apply Filter' to view payments";
                    return;
                }

                int? partyId = cmbParty.SelectedValue as int?;
                int? brokerId = cmbBroker.SelectedValue as int?;
                string selectedPaymentMethod = cmbPaymentMethodFilter?.SelectedItem?.ToString() ?? "All";

                var filteredPayments = _advancePayments.AsEnumerable();

                // Filter by Party
                if (partyId.HasValue && partyId.Value > 0)
                {
                    filteredPayments = filteredPayments.Where(ap => ap.PartyID == partyId.Value);
                }

                // Filter by Broker
                if (brokerId.HasValue && brokerId.Value > 0)
                {
                    filteredPayments = filteredPayments.Where(ap => ap.BrokerID == brokerId.Value);
                }

                // Filter by Payment Method
                if (selectedPaymentMethod != "All")
                {
                    filteredPayments = filteredPayments.Where(ap => 
                        string.Equals(ap.PaymentMethod?.Trim(), selectedPaymentMethod, StringComparison.OrdinalIgnoreCase));
                }

                var result = filteredPayments.ToList();
                dgvAdvancePayments.DataSource = result;
                
                // Update filtered total
                decimal filteredTotal = result.Sum(ap => ap.Amount);
                string filterText = "";
                if (partyId.HasValue && partyId.Value > 0 || 
                    brokerId.HasValue && brokerId.Value > 0 || 
                    selectedPaymentMethod != "All")
                {
                    filterText = "Filtered ";
                }
                string dateRangeText = "";
                if (dtpFromDate != null && dtpToDate != null)
                {
                    dateRangeText = $" ({dtpFromDate.Value:dd-MMM-yyyy} to {dtpToDate.Value:dd-MMM-yyyy})";
                }
                lblTotalAdvances.Text = $"{filterText}Total: ₹{filteredTotal:N2}{dateRangeText}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filtering advance payments: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnApplyDateFilter_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if date picker controls are available
                if (dtpFromDate == null || dtpToDate == null)
                {
                    MessageBox.Show("Date filter controls are not available.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Validate date range
                if (dtpFromDate.Value > dtpToDate.Value)
                {
                    MessageBox.Show("From Date cannot be greater than To Date.", "Invalid Date Range", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dtpFromDate.Focus();
                    return;
                }

                // Filter payments by date range
                DateTime fromDate = dtpFromDate.Value.Date;
                DateTime toDate = dtpToDate.Value.Date.AddDays(1).AddTicks(-1); // End of day

                _advancePayments = _allAdvancePayments
                    .Where(ap => ap.PaymentDate >= fromDate && ap.PaymentDate <= toDate)
                    .ToList();

                _dateFilterApplied = true;

                // Apply any existing party/broker filters
                FilterAdvancePayments();

                // Update button text to show filter is applied
                if (btnApplyDateFilter != null)
                {
                    btnApplyDateFilter.Text = "Refresh";
                    btnApplyDateFilter.BackColor = Color.LightGreen;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying date filter: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbPaymentMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedMethod = cmbPaymentMethod.SelectedItem?.ToString() ?? "Cash";
            
            // Show/hide the cheque details panel based on payment method
            pnlChequeDetails.Visible = selectedMethod == "Cheque";
            
            // If switching to non-Cheque, reset the cheque amounts
            if (selectedMethod != "Cheque")
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
                nudAmount.Value = totalChequeAmount;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                // Parse the payment date
                if (!DateTime.TryParseExact(txtPaymentDate.Text, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime paymentDate))
                {
                    MessageBox.Show("Please enter a valid payment date in dd-mm-yyyy format.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPaymentDate.Focus();
                    return;
                }

                var advancePayment = new AdvancePayment
                {
                    PaymentDate = paymentDate.Date,
                    Amount = nudAmount.Value,
                    PaymentMethod = cmbPaymentMethod.Text.Trim(),
                    Reference = txtReference.Text.Trim().ToUpper(),
                    CompanyID = 1 // Default company ID
                };

                // Set cheque amounts if payment method is Cheque
                if (cmbPaymentMethod.Text.Trim() == "Cheque")
                {
                    if (decimal.TryParse(txtChequeAmountFirm1.Text, out decimal firm1Amount))
                    {
                        advancePayment.ChequeAmountFirm1 = firm1Amount;
                    }
                    
                    if (decimal.TryParse(txtChequeAmountFirm2.Text, out decimal firm2Amount))
                    {
                        advancePayment.ChequeAmountFirm2 = firm2Amount;
                    }
                }

                // Set party if selected
                if (cmbParty.SelectedValue is int partyId && partyId > 0)
                {
                    advancePayment.PartyID = partyId;
                }

                // Set broker if selected
                if (cmbBroker.SelectedValue is int brokerId && brokerId > 0)
                {
                    advancePayment.BrokerID = brokerId;
                }

                if (AdvancePaymentService.AddAdvancePayment(advancePayment))
                {
                    MessageBox.Show("Advance payment added successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Remember the payment date for this session
                    _sessionDate = txtPaymentDate.Text;
                    
                    // Refresh the data - reload all payments and reapply current filter if active
                    _allAdvancePayments = AdvancePaymentService.GetAllAdvancePayments();
                    if (_dateFilterApplied)
                    {
                        BtnApplyDateFilter_Click(sender, e); // Reapply current date filter
                    }
                    ClearForm();
                }
                else
                {
                    MessageBox.Show("Failed to add advance payment. Please try again.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving advance payment: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            _sessionDate = null; // Reset session so date goes back to today
            ClearForm();
            // Reset the date filter
            _dateFilterApplied = false;
            _advancePayments = new List<AdvancePayment>();
            dgvAdvancePayments.DataSource = _advancePayments;
            lblTotalAdvances.Text = "Select date range and click 'Apply Filter' to view payments";
            
            // Reset date filter button with null check
            if (btnApplyDateFilter != null)
            {
                btnApplyDateFilter.Text = "Apply Filter";
                btnApplyDateFilter.BackColor = Color.LightBlue;
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            // Check if any row is selected
            if (dgvAdvancePayments.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an advance payment to delete.", "No Selection", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Get the selected advance payment
            var selectedAdvancePayment = dgvAdvancePayments.SelectedRows[0].DataBoundItem as AdvancePayment;
            if (selectedAdvancePayment == null)
            {
                MessageBox.Show("Unable to determine selected advance payment.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check permissions before allowing delete
            if (!PermissionManager.ValidateDeleteOperation(ModuleType.Payments, "advance payment"))
                return;

            // Show confirmation dialog
            string confirmMessage = $"Are you sure you want to delete the advance payment?\n\n" +
                                  $"Date: {selectedAdvancePayment.PaymentDate:dd-MMM-yyyy}\n" +
                                  $"Amount: ₹{selectedAdvancePayment.Amount:N2}\n" +
                                  $"Party: {selectedAdvancePayment.PartyName ?? "N/A"}\n" +
                                  $"Broker: {selectedAdvancePayment.BrokerName ?? "N/A"}\n" +
                                  $"Method: {selectedAdvancePayment.PaymentMethod}\n\n" +
                                  $"This action cannot be undone!";

            var result = MessageBox.Show(confirmMessage, "Confirm Delete", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
                return;

            try
            {
                // Disable delete button to prevent multiple clicks
                btnDelete.Enabled = false;
                btnDelete.Text = "Deleting...";
                
                // Delete the advance payment
                if (AdvancePaymentService.DeleteAdvancePayment(selectedAdvancePayment.AdvanceID))
                {
                    MessageBox.Show("Advance payment deleted successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Refresh the data - reload all payments and reapply current filter if active
                    _allAdvancePayments = AdvancePaymentService.GetAllAdvancePayments();
                    if (_dateFilterApplied)
                    {
                        BtnApplyDateFilter_Click(sender, EventArgs.Empty); // Reapply current date filter
                    }
                    else
                    {
                        LoadAdvancePayments(); // Show empty grid with instruction
                    }
                }
                else
                {
                    MessageBox.Show("Failed to delete advance payment. Please try again.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting advance payment: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Re-enable delete button
                btnDelete.Enabled = true;
                btnDelete.Text = "&Delete";
            }
        }

        private void DgvAdvancePayments_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle Delete key press
            if (e.KeyCode == Keys.Delete)
            {
                BtnDelete_Click(sender, EventArgs.Empty);
                e.Handled = true; // Prevent the default key handling
            }
        }

        #endregion

        #region Validation

        private bool ValidateForm()
        {
            // Check if at least one party or broker is selected
            bool hasParty = cmbParty.SelectedValue is int partyId && partyId > 0;
            bool hasBroker = cmbBroker.SelectedValue is int brokerId && brokerId > 0;

            if (!hasParty && !hasBroker)
            {
                MessageBox.Show("Please select either a Party or Broker (or both)", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbParty.Focus();
                return false;
            }

            // Check payment date
            if (!DateTime.TryParseExact(txtPaymentDate.Text, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime paymentDate))
            {
                MessageBox.Show("Please enter a valid payment date in dd-mm-yyyy format.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPaymentDate.Focus();
                return false;
            }

            // Check amount
            if (nudAmount.Value <= 0)
            {
                MessageBox.Show("Please enter a valid advance amount", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nudAmount.Focus();
                return false;
            }

            // Check payment method
            if (string.IsNullOrWhiteSpace(cmbPaymentMethod.Text))
            {
                MessageBox.Show("Please select a payment method", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbPaymentMethod.Focus();
                return false;
            }

            // Check cheque amounts if payment method is Cheque
            if (cmbPaymentMethod.Text.Trim() == "Cheque")
            {
                if (!decimal.TryParse(txtChequeAmountFirm1.Text, out decimal firm1Amount) || firm1Amount < 0)
                {
                    MessageBox.Show("Firm 1 cheque amount is invalid.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtChequeAmountFirm1.Focus();
                    return false;
                }
                
                if (!decimal.TryParse(txtChequeAmountFirm2.Text, out decimal firm2Amount) || firm2Amount < 0)
                {
                    MessageBox.Show("Firm 2 cheque amount is invalid.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtChequeAmountFirm2.Focus();
                    return false;
                }
                
                // Verify that at least one firm amount is greater than zero
                if (firm1Amount == 0 && firm2Amount == 0)
                {
                    MessageBox.Show("At least one firm cheque amount must be greater than zero.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtChequeAmountFirm1.Focus();
                    return false;
                }
                
                // Verify that the sum matches the total amount
                if (Math.Abs((firm1Amount + firm2Amount) - nudAmount.Value) > 0.01m)
                {
                    MessageBox.Show("The sum of Firm 1 and Firm 2 amounts must equal the total amount.", 
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtChequeAmountFirm1.Focus();
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}