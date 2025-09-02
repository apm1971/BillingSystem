using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class AdvancePaymentSelectionForm : Form
    {
        private List<AdvancePayment> _allAdvancePayments;
        private List<AdvancePayment> _selectedAdvancePayments;
        private int? _brokerId;
        private int? _partyId;

        public List<AdvancePayment> SelectedAdvancePayments => _selectedAdvancePayments;

        // Additional buttons for better user control
        private Button btnSelectAll = null!;
        private Button btnClearAll = null!;
        private Button btnSelectNone = null!;

        // Class to hold advance payment data with selection state
        private class AdvancePaymentRow
        {
            public int AdvanceID { get; set; }
            public DateTime PaymentDate { get; set; }
            public decimal Amount { get; set; }
            public string PaymentMethod { get; set; } = string.Empty;
            public string Reference { get; set; } = string.Empty;
            public string PartyName { get; set; } = string.Empty;
            public string BrokerName { get; set; } = string.Empty;
            public bool IsSelected { get; set; }
            public int? PartyID { get; set; }
            public int? BrokerID { get; set; }
        }

        public AdvancePaymentSelectionForm(int? brokerId = null, int? partyId = null)
        {
            InitializeComponent();
            _brokerId = brokerId;
            _partyId = partyId;
            _selectedAdvancePayments = new List<AdvancePayment>();
            _allAdvancePayments = new List<AdvancePayment>(); // Initialize to prevent null reference warnings
            LoadAdvancePayments();
        }

        private void InitializeComponent()
        {
            this.chkSelectAll = new CheckBox();
            this.dgvAdvancePayments = new DataGridView();
            this.btnOK = new Button();
            this.btnCancel = new Button();
            this.lblTitle = new Label();
            this.panel1 = new Panel();
            this.panel2 = new Panel();
            this.btnSelectAll = new Button();
            this.btnClearAll = new Button();
            this.btnSelectNone = new Button();
            
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblTitle);
            this.panel1.Controls.Add(this.chkSelectAll);
            this.panel1.Controls.Add(this.btnSelectAll);
            this.panel1.Controls.Add(this.btnClearAll);
            this.panel1.Controls.Add(this.btnSelectNone);
            this.panel1.Dock = DockStyle.Top;
            this.panel1.Location = new Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(800, 60);
            this.panel1.TabIndex = 0;
            this.panel1.BackColor = Color.FromArgb(240, 240, 240);
            
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.lblTitle.Location = new Point(20, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(200, 21);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Select Advance Payments";
            
            // 
            // chkSelectAll
            // 
            this.chkSelectAll.AutoSize = true;
            this.chkSelectAll.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.chkSelectAll.Location = new Point(250, 20);
            this.chkSelectAll.Name = "chkSelectAll";
            this.chkSelectAll.Size = new Size(100, 21);
            this.chkSelectAll.TabIndex = 1;
            this.chkSelectAll.Text = "Select All";
            this.chkSelectAll.UseVisualStyleBackColor = true;
            this.chkSelectAll.CheckedChanged += new EventHandler(this.ChkSelectAll_CheckedChanged);
            
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.BackColor = Color.FromArgb(0, 120, 215);
            this.btnSelectAll.FlatStyle = FlatStyle.Flat;
            this.btnSelectAll.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnSelectAll.ForeColor = Color.White;
            this.btnSelectAll.Location = new Point(370, 18);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new Size(80, 25);
            this.btnSelectAll.TabIndex = 2;
            this.btnSelectAll.Text = "Select All";
            this.btnSelectAll.UseVisualStyleBackColor = false;
            this.btnSelectAll.Click += new EventHandler(this.BtnSelectAll_Click);
            
            // 
            // btnClearAll
            // 
            this.btnClearAll.BackColor = Color.FromArgb(220, 53, 69);
            this.btnClearAll.FlatStyle = FlatStyle.Flat;
            this.btnClearAll.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnClearAll.ForeColor = Color.White;
            this.btnClearAll.Location = new Point(460, 18);
            this.btnClearAll.Name = "btnClearAll";
            this.btnClearAll.Size = new Size(80, 25);
            this.btnClearAll.TabIndex = 3;
            this.btnClearAll.Text = "Clear All";
            this.btnClearAll.UseVisualStyleBackColor = false;
            this.btnClearAll.Click += new EventHandler(this.BtnClearAll_Click);
            
            // 
            // btnSelectNone
            // 
            this.btnSelectNone.BackColor = Color.FromArgb(108, 117, 125);
            this.btnSelectNone.FlatStyle = FlatStyle.Flat;
            this.btnSelectNone.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnSelectNone.ForeColor = Color.White;
            this.btnSelectNone.Location = new Point(550, 18);
            this.btnSelectNone.Name = "btnSelectNone";
            this.btnSelectNone.Size = new Size(80, 25);
            this.btnSelectNone.TabIndex = 4;
            this.btnSelectNone.Text = "Select None";
            this.btnSelectNone.UseVisualStyleBackColor = false;
            this.btnSelectNone.Click += new EventHandler(this.BtnSelectNone_Click);
            
            // 
            // dgvAdvancePayments
            // 
            this.dgvAdvancePayments.AllowUserToAddRows = false;
            this.dgvAdvancePayments.AllowUserToDeleteRows = false;
            this.dgvAdvancePayments.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvAdvancePayments.BackgroundColor = Color.White;
            this.dgvAdvancePayments.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAdvancePayments.Dock = DockStyle.Fill;
            this.dgvAdvancePayments.Location = new Point(0, 0);
            this.dgvAdvancePayments.Name = "dgvAdvancePayments";
            this.dgvAdvancePayments.RowTemplate.Height = 25;
            this.dgvAdvancePayments.Size = new Size(800, 400);
            this.dgvAdvancePayments.TabIndex = 0;
            this.dgvAdvancePayments.CellValueChanged += new DataGridViewCellEventHandler(this.DgvAdvancePayments_CellValueChanged);
            
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnCancel);
            this.panel2.Controls.Add(this.btnOK);
            this.panel2.Dock = DockStyle.Bottom;
            this.panel2.Location = new Point(0, 400);
            this.panel2.Name = "panel2";
            this.panel2.Size = new Size(800, 60);
            this.panel2.TabIndex = 1;
            this.panel2.BackColor = Color.FromArgb(240, 240, 240);
            
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnOK.BackColor = Color.FromArgb(0, 120, 215);
            this.btnOK.FlatStyle = FlatStyle.Flat;
            this.btnOK.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            this.btnOK.ForeColor = Color.White;
            this.btnOK.Location = new Point(600, 15);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(80, 30);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = false;
            this.btnOK.Click += new EventHandler(this.BtnOK_Click);
            
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnCancel.BackColor = Color.Gray;
            this.btnCancel.FlatStyle = FlatStyle.Flat;
            this.btnCancel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            this.btnCancel.ForeColor = Color.White;
            this.btnCancel.Location = new Point(700, 15);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(80, 30);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new EventHandler(this.BtnCancel_Click);
            
            // 
            // AdvancePaymentSelectionForm
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(800, 460);
            this.Controls.Add(this.dgvAdvancePayments);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.MinimumSize = new Size(800, 460);
            this.Name = "AdvancePaymentSelectionForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Select Advance Payments";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ResumeLayout(false);
        }

        private void LoadAdvancePayments()
        {
            try
            {
                // Load advance payments based on broker/party selection
                if (_brokerId.HasValue && _brokerId.Value > 0)
                {
                    _allAdvancePayments = AdvancePaymentService.GetAvailableAdvancePayments(null, _brokerId);
                    lblTitle.Text = $"Select Advance Payments - Broker ID: {_brokerId}";
                }
                else if (_partyId.HasValue && _partyId.Value > 0)
                {
                    _allAdvancePayments = AdvancePaymentService.GetAvailableAdvancePayments(_partyId, null);
                    lblTitle.Text = $"Select Advance Payments - Party ID: {_partyId}";
                }
                else
                {
                    _allAdvancePayments = AdvancePaymentService.GetAvailableAdvancePayments(null, null);
                    lblTitle.Text = "Select Advance Payments - All";
                }

                SetupDataGridView();
                PopulateDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading advance payments: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupDataGridView()
        {
            dgvAdvancePayments.AutoGenerateColumns = false;
            dgvAdvancePayments.Columns.Clear();
            dgvAdvancePayments.ReadOnly = false;
            dgvAdvancePayments.AllowUserToAddRows = false;
            dgvAdvancePayments.AllowUserToDeleteRows = false;

            // Add checkbox column for selection
            var chkColumn = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "IsSelected",
                HeaderText = "Select",
                Width = 60,
                ReadOnly = false
            };
            dgvAdvancePayments.Columns.Add(chkColumn);

            // Add other columns
            dgvAdvancePayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AdvanceID",
                HeaderText = "ID",
                Width = 60,
                ReadOnly = true
            });

            dgvAdvancePayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PaymentDate",
                HeaderText = "Payment Date",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd-MMM-yyyy" },
                Width = 120,
                ReadOnly = true
            });

            dgvAdvancePayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Amount",
                HeaderText = "Amount",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight },
                Width = 120,
                ReadOnly = true
            });

            dgvAdvancePayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PaymentMethod",
                HeaderText = "Payment Method",
                Width = 120,
                ReadOnly = true
            });

            dgvAdvancePayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Reference",
                HeaderText = "Reference",
                Width = 200,
                ReadOnly = true
            });

            dgvAdvancePayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PartyName",
                HeaderText = "Party",
                Width = 150,
                ReadOnly = true
            });

            dgvAdvancePayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "BrokerName",
                HeaderText = "Broker",
                Width = 150,
                ReadOnly = true
            });
        }

        private void PopulateDataGridView()
        {
            // Create a list with selection property using our custom class
            // Sort by payment date in descending order (newest first)
            var advancePaymentsWithSelection = _allAdvancePayments
                .OrderBy(ap => ap.PaymentDate)
                .Select(ap => new AdvancePaymentRow
                {
                    AdvanceID = ap.AdvanceID,
                    PaymentDate = ap.PaymentDate,
                    Amount = ap.Amount,
                    PaymentMethod = ap.PaymentMethod ?? string.Empty,
                    Reference = ap.Reference ?? string.Empty,
                    PartyName = GetPartyName(ap.PartyID),
                    BrokerName = GetBrokerName(ap.BrokerID),
                    IsSelected = false, // Default to not selected
                    PartyID = ap.PartyID,
                    BrokerID = ap.BrokerID
                }).ToList();

            dgvAdvancePayments.DataSource = advancePaymentsWithSelection;
        }

        private string GetPartyName(int? partyId)
        {
            if (!partyId.HasValue || partyId.Value <= 0) return "N/A";
            try
            {
                var party = PartyService.GetPartyByID(partyId.Value);
                return party?.PartyName ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private string GetBrokerName(int? brokerId)
        {
            if (!brokerId.HasValue || brokerId.Value <= 0) return "N/A";
            try
            {
                var broker = BrokerService.GetBrokerByID(brokerId.Value);
                return broker?.BrokerName ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private void ChkSelectAll_CheckedChanged(object? sender, EventArgs e)
        {
            bool selectAll = chkSelectAll.Checked;
            
            // Update the data source directly
            if (dgvAdvancePayments.DataSource is List<AdvancePaymentRow> dataSource)
            {
                foreach (var row in dataSource)
                {
                    row.IsSelected = selectAll;
                }
                
                // Refresh the DataGridView to show the changes
                dgvAdvancePayments.Refresh();
            }
        }

        private void DgvAdvancePayments_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0) // Checkbox column
            {
                // Update the data source when checkbox is clicked
                if (dgvAdvancePayments.DataSource is List<AdvancePaymentRow> dataSource && e.RowIndex < dataSource.Count)
                {
                    var row = dataSource[e.RowIndex];
                    if (dgvAdvancePayments.Rows[e.RowIndex].Cells[0] is DataGridViewCheckBoxCell checkBoxCell)
                    {
                        row.IsSelected = checkBoxCell.Value as bool? ?? false;
                    }
                }
                
                UpdateSelectAllCheckbox();
            }
        }

        private void UpdateSelectAllCheckbox()
        {
            if (dgvAdvancePayments.DataSource is List<AdvancePaymentRow> dataSource)
            {
                bool allSelected = dataSource.All(row => row.IsSelected);
                bool anySelected = dataSource.Any(row => row.IsSelected);

                // Temporarily remove the event handler to avoid infinite loop
                chkSelectAll.CheckedChanged -= ChkSelectAll_CheckedChanged;
                chkSelectAll.Checked = allSelected;
                chkSelectAll.CheckedChanged += ChkSelectAll_CheckedChanged;
            }
        }

        private void BtnOK_Click(object? sender, EventArgs e)
        {
            try
            {
                _selectedAdvancePayments.Clear();

                // Get the data source and check which items are selected
                if (dgvAdvancePayments.DataSource is List<AdvancePaymentRow> dataSource)
                {
                    var selectedRows = dataSource.Where(row => row.IsSelected).ToList();
                    
                    foreach (var selectedRow in selectedRows)
                    {
                        var advance = _allAdvancePayments.FirstOrDefault(ap => ap.AdvanceID == selectedRow.AdvanceID);
                        if (advance != null)
                        {
                            _selectedAdvancePayments.Add(advance);
                        }
                    }
                }

                if (_selectedAdvancePayments.Any())
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Please select at least one advance payment.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing selection: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void BtnSelectAll_Click(object? sender, EventArgs e)
        {
            SelectAllPayments();
        }

        private void BtnClearAll_Click(object? sender, EventArgs e)
        {
            ClearAllPayments();
        }

        private void BtnSelectNone_Click(object? sender, EventArgs e)
        {
            SelectNonePayments();
        }

        private void SelectAllPayments()
        {
            if (dgvAdvancePayments.DataSource is List<AdvancePaymentRow> dataSource)
            {
                foreach (var row in dataSource)
                {
                    row.IsSelected = true;
                }
                
                // Update checkbox state
                chkSelectAll.Checked = true;
                
                // Refresh the DataGridView to show the changes
                dgvAdvancePayments.Refresh();
            }
        }

        private void ClearAllPayments()
        {
            if (dgvAdvancePayments.DataSource is List<AdvancePaymentRow> dataSource)
            {
                foreach (var row in dataSource)
                {
                    row.IsSelected = false;
                }
                
                // Update checkbox state
                chkSelectAll.Checked = false;
                
                // Refresh the DataGridView to show the changes
                dgvAdvancePayments.Refresh();
            }
        }

        private void SelectNonePayments()
        {
            ClearAllPayments(); // Same functionality as Clear All
        }

        // Designer components
        private CheckBox chkSelectAll = null!;
        private DataGridView dgvAdvancePayments = null!;
        private Button btnOK = null!;
        private Button btnCancel = null!;
        private Label lblTitle = null!;
        private Panel panel1 = null!;
        private Panel panel2 = null!;
    }
}
