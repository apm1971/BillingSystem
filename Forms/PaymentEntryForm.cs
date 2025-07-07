using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class PaymentEntryForm : Form
    {
        private List<Party> parties;
        private List<Broker> brokers;
        private List<Bill> outstandingBills;
        private Payment currentPayment;

        public PaymentEntryForm()
        {
            InitializeComponent();
            LoadData();
            SetupForm();
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

        private void SetupForm()
        {
            this.Text = "Payment Entry";
            
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
                Width = 100,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
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
                outstandingBills = new List<Bill>();

                if (rbFilterByParty.Checked && cmbParty.SelectedValue is int partyId && partyId > 0)
                {
                    outstandingBills = PaymentService.GetOutstandingBillsByParty(partyId);
                }
                else if (rbFilterByBroker.Checked && cmbBroker.SelectedValue is int brokerId && brokerId > 0)
                {
                    outstandingBills = PaymentService.GetOutstandingBillsByBroker(brokerId);
                }

                // Create display data with payment amount column
                var billData = outstandingBills.Select(b => new
                {
                    BillID = b.BillID,
                    BillNo = b.BillNo,
                    BillDate = b.BillDate,
                    PartyName = b.PartyName,
                    NetAmount = b.NetAmount,
                    PaidAmount = b.PaidAmount,
                    BalanceAmount = b.BalanceAmount,
                    PaymentAmount = 0.0
                }).ToList();

                dgvBills.DataSource = billData;
                groupBoxBills.Enabled = outstandingBills.Count > 0;
                
                lblTotalBills.Text = $"Outstanding Bills: {outstandingBills.Count}";
                lblTotalOutstanding.Text = $"Total Outstanding: ₹{outstandingBills.Sum(b => b.BalanceAmount):N2}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bills: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PaymentAmount_TextChanged(object sender, EventArgs e)
        {
            AutoAllocatePayment();
        }

        private void AutoAllocatePayment()
        {
            if (!double.TryParse(txtPaymentAmount.Text, out double paymentAmount) || paymentAmount <= 0)
                return;

            double remainingAmount = paymentAmount;

            // Auto-allocate payment to bills (FIFO - oldest first)
            foreach (DataGridViewRow row in dgvBills.Rows)
            {
                if (remainingAmount <= 0) break;

                double balanceAmount = Convert.ToDouble(row.Cells["BalanceAmount"].Value);
                double allocationAmount = Math.Min(remainingAmount, balanceAmount);
                
                row.Cells["PaymentAmount"].Value = allocationAmount;
                remainingAmount -= allocationAmount;
            }

            UpdatePaymentSummary();
        }

        private void DgvBills_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0 && dgvBills.Columns[e.ColumnIndex].Name == "PaymentAmount")
            {
                UpdatePaymentSummary();
            }
        }

        private void UpdatePaymentSummary()
        {
            double totalAllocated = 0;
            foreach (DataGridViewRow row in dgvBills.Rows)
            {
                if (row.Cells["PaymentAmount"].Value != null && 
                    double.TryParse(row.Cells["PaymentAmount"].Value.ToString(), out double amount))
                {
                    totalAllocated += amount;
                }
            }

            lblAllocatedAmount.Text = $"Allocated: ₹{totalAllocated:N2}";
            
            if (double.TryParse(txtPaymentAmount.Text, out double paymentAmount))
            {
                double unallocated = paymentAmount - totalAllocated;
                lblUnallocatedAmount.Text = $"Unallocated: ₹{unallocated:N2}";
                lblUnallocatedAmount.ForeColor = unallocated >= 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidatePayment())
                return;

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
                    if (row.Cells["PaymentAmount"].Value != null && 
                        double.TryParse(row.Cells["PaymentAmount"].Value.ToString(), out double amount) && 
                        amount > 0)
                    {
                        int billId = Convert.ToInt32(row.Cells["BillID"].Value);
                        Bill bill = outstandingBills.First(b => b.BillID == billId);

                        PaymentDetail detail = new PaymentDetail
                        {
                            BillID = billId,
                            BillNo = bill.BillNo,
                            PartyName = bill.PartyName,
                            BillAmount = bill.NetAmount,
                            PreviousPaid = bill.PaidAmount,
                            BalanceBefore = bill.BalanceAmount,
                            AllocatedAmount = amount,
                            BalanceAfter = bill.BalanceAmount - amount
                        };

                        currentPayment.PaymentDetails.Add(detail);
                    }
                }

                if (PaymentService.SavePayment(currentPayment))
                {
                    MessageBox.Show("Payment saved successfully!", "Success", 
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

        private void btnClearAllocation_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvBills.Rows)
            {
                row.Cells["PaymentAmount"].Value = 0.0;
            }
            UpdatePaymentSummary();
        }

        private void btnAutoAllocate_Click(object sender, EventArgs e)
        {
            AutoAllocatePayment();
        }
    }
} 