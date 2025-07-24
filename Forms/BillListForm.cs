using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SaleBillSystem.NET.Data;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class BillListForm : Form
    {
        private List<Bill> bills = new List<Bill>();
        private List<Bill> filteredBills = new List<Bill>();
        private Button btnViewPayments; // Add new button field
        private string currentSortColumn = "BillDate"; // Default sort column
        private SortOrder currentSortOrder = SortOrder.Descending; // Default sort order

        public BillListForm()
        {
            InitializeComponent();
            SetupDataGridView();
            SetupEventHandlers();
            AddViewPaymentsButton(); // Add new method call
            LoadBills();
            this.KeyPreview = true; // Enable form to receive key events first
            this.KeyDown += BillListForm_KeyDown;
        }
        
        private void SetupEventHandlers()
        {
            // Setup search event handler
            txtSearch.TextChanged += txtSearch_TextChanged;
            
            // Configure date pickers
            dtpFromDate.Format = DateTimePickerFormat.Custom;
            dtpFromDate.CustomFormat = "dd-MM-yyyy";
            dtpToDate.Format = DateTimePickerFormat.Custom;
            dtpToDate.CustomFormat = "dd-MM-yyyy";
            
            dtpFromDate.ValueChanged += DateFilter_Changed;
            dtpToDate.ValueChanged += DateFilter_Changed;
            
            // Setup grid event handlers
            dgvBills.SelectionChanged += dgvBills_SelectionChanged;
            dgvBills.CellDoubleClick += dgvBills_CellDoubleClick;
            dgvBills.KeyDown += dgvBills_KeyDown;
            dgvBills.ColumnHeaderMouseClick += DgvBills_ColumnHeaderMouseClick;
        }

        private void DgvBills_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Get the column that was clicked
            DataGridViewColumn column = dgvBills.Columns[e.ColumnIndex];
            
            // Skip sorting for non-sortable columns or if this is the summary row
            if (column.Name == "ItemCount" || column.Name == "InterestDiscountInfo")
                return;
                
            // Toggle sort order if clicking on the same column
            if (column.Name == currentSortColumn)
            {
                currentSortOrder = currentSortOrder == SortOrder.Ascending ? 
                    SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                currentSortColumn = column.Name;
                currentSortOrder = SortOrder.Ascending; // Default to ascending for new column
            }
            
            // Apply the sort
            ApplySorting();
            
            // Update the column header text to show sort direction
            UpdateColumnHeaderSortIndicator();
        }
        
        private void UpdateColumnHeaderSortIndicator()
        {
            // Reset all column headers
            foreach (DataGridViewColumn column in dgvBills.Columns)
            {
                if (column.HeaderText.EndsWith(" ▲") || column.HeaderText.EndsWith(" ▼"))
                {
                    column.HeaderText = column.HeaderText.Substring(0, column.HeaderText.Length - 2);
                }
            }
            
            // Add indicator to current sort column
            DataGridViewColumn sortColumn = dgvBills.Columns[currentSortColumn];
            if (sortColumn != null)
            {
                sortColumn.HeaderText += currentSortOrder == SortOrder.Ascending ? " ▲" : " ▼";
            }
        }
        
        private void ApplySorting()
        {
            if (dgvBills.DataSource is BindingList<BillDisplayData> dataSource)
            {
                // Create a new sorted list
                List<BillDisplayData> sortedList = new List<BillDisplayData>();
                
                // Extract the summary row if it exists
                BillDisplayData summaryRow = null;
                foreach (BillDisplayData item in dataSource)
                {
                    if (item.IsSummaryRow)
                    {
                        summaryRow = item;
                    }
                    else
                    {
                        sortedList.Add(item);
                    }
                }
                
                // Sort the list based on the current sort column and order
                switch (currentSortColumn)
                {
                    case "BillNo":
                        sortedList = currentSortOrder == SortOrder.Ascending ?
                            sortedList.OrderBy(b => b.BillNo).ToList() :
                            sortedList.OrderByDescending(b => b.BillNo).ToList();
                        break;
                    case "BillDate":
                        sortedList = currentSortOrder == SortOrder.Ascending ?
                            sortedList.OrderBy(b => b.BillDate).ToList() :
                            sortedList.OrderByDescending(b => b.BillDate).ToList();
                        break;
                    case "DueDate":
                        sortedList = currentSortOrder == SortOrder.Ascending ?
                            sortedList.OrderBy(b => b.DueDate).ToList() :
                            sortedList.OrderByDescending(b => b.DueDate).ToList();
                        break;
                    case "PartyName":
                        sortedList = currentSortOrder == SortOrder.Ascending ?
                            sortedList.OrderBy(b => b.PartyName).ToList() :
                            sortedList.OrderByDescending(b => b.PartyName).ToList();
                        break;
                    case "BrokerName":
                        sortedList = currentSortOrder == SortOrder.Ascending ?
                            sortedList.OrderBy(b => b.BrokerName).ToList() :
                            sortedList.OrderByDescending(b => b.BrokerName).ToList();
                        break;
                    case "TotalAmount":
                        sortedList = currentSortOrder == SortOrder.Ascending ?
                            sortedList.OrderBy(b => b.TotalAmount).ToList() :
                            sortedList.OrderByDescending(b => b.TotalAmount).ToList();
                        break;
                    case "TotalCharges":
                        sortedList = currentSortOrder == SortOrder.Ascending ?
                            sortedList.OrderBy(b => b.TotalCharges).ToList() :
                            sortedList.OrderByDescending(b => b.TotalCharges).ToList();
                        break;
                    case "NetAmount":
                        sortedList = currentSortOrder == SortOrder.Ascending ?
                            sortedList.OrderBy(b => b.NetAmount).ToList() :
                            sortedList.OrderByDescending(b => b.NetAmount).ToList();
                        break;
                    case "AdjustedNetAmount":
                        sortedList = currentSortOrder == SortOrder.Ascending ?
                            sortedList.OrderBy(b => b.AdjustedNetAmount).ToList() :
                            sortedList.OrderByDescending(b => b.AdjustedNetAmount).ToList();
                        break;
                    case "PaidAmount":
                        sortedList = currentSortOrder == SortOrder.Ascending ?
                            sortedList.OrderBy(b => b.PaidAmount).ToList() :
                            sortedList.OrderByDescending(b => b.PaidAmount).ToList();
                        break;
                    case "BalanceAmount":
                        sortedList = currentSortOrder == SortOrder.Ascending ?
                            sortedList.OrderBy(b => b.BalanceAmount).ToList() :
                            sortedList.OrderByDescending(b => b.BalanceAmount).ToList();
                        break;
                    case "PaymentStatusText":
                        sortedList = currentSortOrder == SortOrder.Ascending ?
                            sortedList.OrderBy(b => b.PaymentStatusText).ToList() :
                            sortedList.OrderByDescending(b => b.PaymentStatusText).ToList();
                        break;
                    default:
                        // Default to bill date if column not recognized
                        sortedList = currentSortOrder == SortOrder.Ascending ?
                            sortedList.OrderBy(b => b.BillDate).ToList() :
                            sortedList.OrderByDescending(b => b.BillDate).ToList();
                        break;
                }
                
                // Add the summary row back at the end if it exists
                if (summaryRow != null)
                {
                    sortedList.Add(summaryRow);
                }
                
                // Update the data source
                dgvBills.DataSource = null;
                dgvBills.DataSource = new BindingList<BillDisplayData>(sortedList);
            }
        }
        
        private void BillListForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                // F5: Refresh
                LoadBills();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.F2)
            {
                // Enter or F2: Edit selected bill
                if (dgvBills.SelectedRows.Count > 0)
                {
                    EditBill();
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.Shift && e.KeyCode == Keys.Delete)
            {
                // Shift+Delete: Delete selected bill
                if (dgvBills.SelectedRows.Count > 0)
                {
                    DeleteBill();
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.Control && e.KeyCode == Keys.N)
            {
                // Ctrl+N: New bill
                NewBill();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                // Escape: Close
                this.Close();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.F)
            {
                // Ctrl+F: Focus on search
                txtSearch.Focus();
                txtSearch.SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F1)
            {
                // F1: Help
                ShowBillListHelp();
                e.SuppressKeyPress = true;
            }
        }
        
        private void ShowBillListHelp()
        {
            MessageBox.Show(
                "Bill List Keyboard Shortcuts:\n\n" +
                "F5: Refresh List\n" +
                "Enter or F2: Edit Selected Bill\n" +
                "Shift+Delete: Delete Selected Bill\n" +
                "Ctrl+N: New Bill\n" +
                "Ctrl+F: Focus on Search\n" +
                "Escape: Close\n" +
                "F1: Show this help\n\n" +
                "Navigation:\n" +
                "Arrow Keys: Navigate in list\n" +
                "Page Up/Down: Scroll list\n" +
                "Home/End: Go to first/last item\n" +
                "Tab: Move between controls",
                "Bill List Help",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
        
        private void EditBill()
        {
            if (dgvBills.SelectedRows.Count > 0)
            {
                int billId = Convert.ToInt32(dgvBills.SelectedRows[0].Cells["BillID"].Value);
                
                // Skip if this is the summary row
                if (billId == -1)
                    return;
                    
                Bill bill = BillService.GetBillByID(billId);
                if (bill != null)
                {
                    var form = new SaleBillForm(bill);
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadBills();
                    }
                }
            }
        }
        
        private void DeleteBill()
        {
            if (dgvBills.SelectedRows.Count > 0)
            {
                int billId = Convert.ToInt32(dgvBills.SelectedRows[0].Cells["BillID"].Value);
                
                // Skip if this is the summary row
                if (billId == -1)
                    return;
                    
                string billNo = dgvBills.SelectedRows[0].Cells["BillNo"].Value.ToString();
                
                var result = MessageBox.Show(
                    $"Are you sure you want to delete Bill No: {billNo}?\n\nThis action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );
                
                if (result == DialogResult.Yes)
                {
                    if (BillService.DeleteBill(billId))
                    {
                        MessageBox.Show("Bill deleted successfully.", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadBills();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete bill.", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        
        private void NewBill()
        {
            var form = new SaleBillForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadBills();
            }
        }

        private void ConfigureControls()
        {
            // Set form properties
            this.Text = "Bill List";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Setup date filters
            dtpFromDate.Value = DateTime.Today.AddMonths(-1);
            dtpToDate.Value = DateTime.Today;

            // Setup search functionality
            txtSearch.TextChanged += txtSearch_TextChanged;
            dtpFromDate.ValueChanged += DateFilter_Changed;
            dtpToDate.ValueChanged += DateFilter_Changed;
        }

        private void SetupDataGridView()
        {
            dgvBills.AutoGenerateColumns = false;
            dgvBills.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBills.MultiSelect = false;
            dgvBills.ReadOnly = true;
            dgvBills.AllowUserToAddRows = false;
            dgvBills.AllowUserToDeleteRows = false;
            dgvBills.RowHeadersVisible = true; // Enable row headers for better visibility
            dgvBills.RowHeadersWidth = 40;
            dgvBills.BackgroundColor = Color.White;
            dgvBills.BorderStyle = BorderStyle.Fixed3D;
            dgvBills.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvBills.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
            dgvBills.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvBills.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(64, 64, 64);
            dgvBills.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvBills.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            dgvBills.EnableHeadersVisualStyles = false;
            dgvBills.GridColor = Color.LightGray;
            dgvBills.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            
            // Clear existing columns
            dgvBills.Columns.Clear();

            // Bill ID (Hidden)
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BillID",
                HeaderText = "Bill ID",
                DataPropertyName = "BillID",
                Visible = false
            });

            // Bill Number
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BillNo",
                HeaderText = "Bill No",
                DataPropertyName = "BillNo",
                Width = 100
            });

            // Bill Date
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BillDate",
                HeaderText = "Bill Date",
                DataPropertyName = "BillDate",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy", NullValue = null }
            });

            // Due Date
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DueDate",
                HeaderText = "Due Date",
                DataPropertyName = "DueDate",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy", NullValue = null }
            });

            // Party Name
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PartyName",
                HeaderText = "Party",
                DataPropertyName = "PartyName",
                Width = 180
            });

            // Broker Name
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BrokerName",
                HeaderText = "Broker",
                DataPropertyName = "BrokerName",
                Width = 120
            });

            // Total Amount
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TotalAmount",
                HeaderText = "Total Amount",
                DataPropertyName = "TotalAmount",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            // Total Charges
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TotalCharges",
                HeaderText = "Total Charges",
                DataPropertyName = "TotalCharges",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            // Net Amount
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "NetAmount",
                HeaderText = "Net Amount",
                DataPropertyName = "NetAmount",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold) }
            });
            
            // Adjusted Net Amount (visible for paid/partial payments)
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AdjustedNetAmount",
                HeaderText = "Adj. Net Amount",
                DataPropertyName = "AdjustedNetAmount",
                Width = 110,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold) }
            });
            
            // Interest/Discount Info
            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "InterestDiscountInfo",
                HeaderText = "Int/Disc",
                DataPropertyName = "InterestDiscountInfo",
                Width = 110,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ItemCount",
                HeaderText = "Items",
                DataPropertyName = "ItemCount",
                Width = 60,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PaidAmount",
                HeaderText = "Paid Amount",
                DataPropertyName = "PaidAmount",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BalanceAmount",
                HeaderText = "Balance",
                DataPropertyName = "BalanceAmount",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold) }
            });

            dgvBills.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PaymentStatusText",
                HeaderText = "Status",
                DataPropertyName = "PaymentStatusText",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            
            // Add CellFormatting event to colorize status column
            dgvBills.CellFormatting += DgvBills_CellFormatting;
        }

        private void AddViewPaymentsButton()
        {
            // Create View Payments button
            btnViewPayments = new Button
            {
                Text = "View Payments",
                BackColor = Color.FromArgb(255, 223, 186), // Light orange color
                Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold),
                Size = new Size(120, 40),
                Location = new Point(660, 15), // Position after Close button
                Enabled = false // Initially disabled until a bill is selected
            };
            btnViewPayments.Click += BtnViewPayments_Click;

            // Add to panel
            panel1.Controls.Add(btnViewPayments);
        }

        private void BtnViewPayments_Click(object sender, EventArgs e)
        {
            var selectedBill = GetSelectedBill();
            if (selectedBill == null) return;

            try
            {
                // Get all payments for this bill
                var payments = PaymentService.GetAllPayments()
                    .Where(p => p.PaymentDetails.Any(pd => pd.BillID == selectedBill.BillID))
                    .OrderByDescending(p => p.PaymentDate)
                    .ToList();

                if (payments.Count == 0)
                {
                    MessageBox.Show("No payments found for this bill.", "Payment Details",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Create and show payment details form
                using (var form = new Form())
                {
                    form.Text = $"Payment Details - Bill #{selectedBill.BillNo}";
                    form.StartPosition = FormStartPosition.CenterParent;
                    form.Size = new Size(800, 400);
                    form.MinimizeBox = false;
                    form.MaximizeBox = false;
                    form.FormBorderStyle = FormBorderStyle.FixedDialog;

                    // Create DataGridView for payments
                    var dgvPayments = new DataGridView
                    {
                        Dock = DockStyle.Fill,
                        AutoGenerateColumns = false,
                        AllowUserToAddRows = false,
                        AllowUserToDeleteRows = false,
                        ReadOnly = true,
                        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                        BackgroundColor = Color.White,
                        BorderStyle = BorderStyle.Fixed3D,
                        ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                        {
                            BackColor = Color.FromArgb(64, 64, 64),
                            ForeColor = Color.White,
                            Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold)
                        }
                    };

                    // Add columns
                    dgvPayments.Columns.AddRange(new DataGridViewColumn[]
                    {
                        new DataGridViewTextBoxColumn
                        {
                            Name = "PaymentDate",
                            HeaderText = "Payment Date",
                            DataPropertyName = "PaymentDate",
                            Width = 120,
                            DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
                        },
                        new DataGridViewTextBoxColumn
                        {
                            Name = "PaymentMethod",
                            HeaderText = "Payment Mode",
                            DataPropertyName = "PaymentMethod",
                            Width = 120
                        },
                        new DataGridViewTextBoxColumn
                        {
                            Name = "Reference",
                            HeaderText = "Reference",
                            DataPropertyName = "Reference",
                            Width = 120
                        },
                        new DataGridViewTextBoxColumn
                        {
                            Name = "AllocatedAmount",
                            HeaderText = "Amount",
                            Width = 120,
                            DefaultCellStyle = new DataGridViewCellStyle 
                            { 
                                Format = "N2",
                                Alignment = DataGridViewContentAlignment.MiddleRight
                            }
                        },
                        new DataGridViewTextBoxColumn
                        {
                            Name = "Interest",
                            HeaderText = "Interest",
                            Width = 100,
                            DefaultCellStyle = new DataGridViewCellStyle 
                            { 
                                Format = "N2",
                                Alignment = DataGridViewContentAlignment.MiddleRight
                            }
                        },
                        new DataGridViewTextBoxColumn
                        {
                            Name = "Discount",
                            HeaderText = "Discount",
                            Width = 100,
                            DefaultCellStyle = new DataGridViewCellStyle 
                            { 
                                Format = "N2",
                                Alignment = DataGridViewContentAlignment.MiddleRight
                            }
                        }
                    });

                    // Add rows
                    foreach (var payment in payments)
                    {
                        var detail = payment.PaymentDetails.First(pd => pd.BillID == selectedBill.BillID);
                        var (interest, discount, _) = PaymentService.CalculateInterestAndDiscount(selectedBill, payment.PaymentDate);
                        
                        dgvPayments.Rows.Add(
                            payment.PaymentDate,
                            payment.PaymentMethod,
                            payment.Reference,
                            detail.AllocatedAmount,
                            interest,
                            discount
                        );
                    }

                    // Add total row
                    var totalRow = dgvPayments.Rows[dgvPayments.Rows.Add()];
                    totalRow.DefaultCellStyle.BackColor = Color.LightGray;
                    totalRow.DefaultCellStyle.Font = new Font(dgvPayments.Font, FontStyle.Bold);
                    totalRow.Cells[0].Value = "Total";
                    totalRow.Cells[3].Value = payments.Sum(p => p.PaymentDetails.First(pd => pd.BillID == selectedBill.BillID).AllocatedAmount);
                    totalRow.Cells[4].Value = payments.Sum(p => PaymentService.CalculateInterestAndDiscount(selectedBill, p.PaymentDate).interestAmount);
                    totalRow.Cells[5].Value = payments.Sum(p => PaymentService.CalculateInterestAndDiscount(selectedBill, p.PaymentDate).discountAmount);

                    form.Controls.Add(dgvPayments);
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error viewing payments: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Flag to track if summary row has been added
        private bool isSummaryRowAdded = false;

        private void LoadBills()
        {
            try
            {
                bills = BillService.GetAllBills();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bills: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilters()
        {
            try
            {
                filteredBills = bills.Where(b => 
                    b.BillDate.Date >= dtpFromDate.Value.Date && 
                    b.BillDate.Date <= dtpToDate.Value.Date).ToList();

                // Apply search filter
                string searchText = txtSearch.Text.ToLower().Trim();
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    filteredBills = filteredBills.Where(b =>
                        b.BillNo.ToLower().Contains(searchText) ||
                        b.PartyName.ToLower().Contains(searchText) ||
                        (!string.IsNullOrEmpty(b.BrokerName) && b.BrokerName.ToLower().Contains(searchText))
                    ).ToList();
                }

                // Reset summary row flag
                isSummaryRowAdded = false;

                // Calculate totals for summary row
                double totalAmount = filteredBills.Sum(b => b.TotalAmount);
                double totalCharges = filteredBills.Sum(b => b.TotalCharges);
                double netAmount = filteredBills.Sum(b => b.NetAmount);
                double adjustedNetAmount = filteredBills.Sum(b => b.AdjustedNetAmount);
                double paidAmount = filteredBills.Sum(b => b.PaidAmount);
                double balanceAmount = filteredBills.Sum(b => b.BalanceAmount);
                int totalItems = filteredBills.Sum(b => b.BillItems.Count);

                // Create a list of display data
                var displayList = new List<BillDisplayData>();
                
                // Add regular bill rows
                foreach (var b in filteredBills)
                {
                    displayList.Add(new BillDisplayData
                    {
                        BillID = b.BillID,
                        BillNo = b.BillNo,
                        BillDate = b.BillDate,
                        DueDate = b.DueDate,
                        PartyName = b.PartyName,
                        BrokerName = string.IsNullOrEmpty(b.BrokerName) ? "No Broker" : b.BrokerName,
                        TotalAmount = b.TotalAmount,
                        TotalCharges = b.TotalCharges,
                        NetAmount = b.NetAmount,
                        AdjustedNetAmount = b.AdjustedNetAmount,
                        InterestDiscountInfo = b.GetInterestDiscountInfo(),
                        PaidAmount = b.PaidAmount,
                        BalanceAmount = b.BalanceAmount,
                        PaymentStatusText = b.PaymentStatusText,
                        ItemCount = b.BillItems.Count,
                        IsSummaryRow = false
                    });
                }

                // Add summary row if there are bills
                if (filteredBills.Count > 0)
                {
                    displayList.Add(new BillDisplayData
                    {
                        BillID = -1, // Use -1 to identify the summary row
                        BillNo = "TOTAL",
                        BillDate = null, // Null for summary row
                        DueDate = null, // Null for summary row
                        PartyName = $"{filteredBills.Count} bills",
                        BrokerName = "",
                        TotalAmount = totalAmount,
                        TotalCharges = totalCharges,
                        NetAmount = netAmount,
                        AdjustedNetAmount = adjustedNetAmount,
                        InterestDiscountInfo = "",
                        PaidAmount = paidAmount,
                        BalanceAmount = balanceAmount,
                        PaymentStatusText = "",
                        ItemCount = totalItems,
                        IsSummaryRow = true
                    });
                    
                    isSummaryRowAdded = true;
                }

                // Create a binding list from the display list
                var bindingList = new BindingList<BillDisplayData>(displayList);
                
                // Clear the data source first
                dgvBills.DataSource = null;
                
                // Set the new data source
                dgvBills.DataSource = bindingList;
                
                // Apply current sorting
                ApplySorting();
                
                // Update column headers to show sort indicators
                UpdateColumnHeaderSortIndicator();
                
                lblTotalBills.Text = $"Total Bills: {filteredBills.Count}";
                lblTotalAmount.Text = $"Total Amount: {netAmount:N2}";

                // Enable/disable buttons based on selection
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying filters: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Class to hold bill display data including summary row
        private class BillDisplayData
        {
            public int BillID { get; set; }
            public string BillNo { get; set; } = string.Empty;
            public DateTime? BillDate { get; set; }
            public DateTime? DueDate { get; set; }
            public string PartyName { get; set; } = string.Empty;
            public string BrokerName { get; set; } = string.Empty;
            public double TotalAmount { get; set; }
            public double TotalCharges { get; set; }
            public double NetAmount { get; set; }
            public double AdjustedNetAmount { get; set; }
            public string InterestDiscountInfo { get; set; } = string.Empty;
            public double PaidAmount { get; set; }
            public double BalanceAmount { get; set; }
            public string PaymentStatusText { get; set; } = string.Empty;
            public int ItemCount { get; set; }
            public bool IsSummaryRow { get; set; } = false;
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = dgvBills.SelectedRows.Count > 0;
            
            // Don't enable buttons for the summary row
            if (hasSelection && dgvBills.SelectedRows[0].DataBoundItem is BillDisplayData data && data.IsSummaryRow)
            {
                btnEdit.Enabled = false;
                btnDelete.Enabled = false;
                btnView.Enabled = false;
                btnViewPayments.Enabled = false; // Add this line
                return;
            }
            
            btnEdit.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;
            btnView.Enabled = hasSelection;
            btnViewPayments.Enabled = hasSelection; // Add this line
        }

        private Bill GetSelectedBill()
        {
            if (dgvBills.SelectedRows.Count > 0)
            {
                // Skip if summary row is selected
                if (dgvBills.SelectedRows[0].DataBoundItem is BillDisplayData data && data.IsSummaryRow)
                    return null;
                    
                int billId = Convert.ToInt32(dgvBills.SelectedRows[0].Cells["BillID"].Value);
                return filteredBills.FirstOrDefault(b => b.BillID == billId);
            }
            return null;
        }

        private void EditSelectedBill()
        {
            var selectedBill = GetSelectedBill();
            if (selectedBill != null)
            {
                var editForm = new SaleBillForm(selectedBill);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadBills(); // Refresh the list
                }
            }
        }

        private void ViewSelectedBill()
        {
            var selectedBill = GetSelectedBill();
            if (selectedBill != null)
            {
                // Create a read-only view or detailed view of the bill
                string paymentStatus = selectedBill.PaymentStatusText;
                string dueDateInfo = selectedBill.DaysUntilDue > 0 ? $" (Due in {selectedBill.DaysUntilDue} days)" : 
                                    selectedBill.IsOverdue ? $" (Overdue by {selectedBill.DaysOverdue} days)" : " (Due today)";
                
                string billDetails = $"Bill Details:\n\n" +
                    $"Bill No: {selectedBill.BillNo}\n" +
                    $"Bill Date: {selectedBill.BillDate:dd/MM/yyyy}\n" +
                    $"Due Date: {selectedBill.DueDate:dd/MM/yyyy}{dueDateInfo}\n" +
                    $"Party: {selectedBill.PartyName}\n" +
                    $"Total Amount: {selectedBill.TotalAmount:N2}\n" +
                    $"Total Charges: {selectedBill.TotalCharges:N2}\n" +
                    $"Net Amount: {selectedBill.NetAmount:N2}";

                // Show interest and discount for paid or partial payments
                if (selectedBill.PaidAmount > 0)
                {
                    if (selectedBill.InterestAmount > 0)
                    {
                        billDetails += $"\nAdjustment: +₹{selectedBill.InterestAmount:N2}";
                    }
                    else if (selectedBill.DiscountAmount > 0)
                    {
                        billDetails += $"\nAdjustment: -₹{selectedBill.DiscountAmount:N2}";
                    }
                    
                    if (selectedBill.InterestAmount > 0 || selectedBill.DiscountAmount > 0)
                    {
                        billDetails += $"\nAdjusted Net Amount: {selectedBill.AdjustedNetAmount:N2}";
                    }
                }

                billDetails += $"\nPaid Amount: {selectedBill.PaidAmount:N2}\n" +
                    $"Balance: {selectedBill.BalanceAmount:N2}\n" +
                    $"Status: {paymentStatus}\n" +
                    $"Number of Items: {selectedBill.BillItems.Count}\n\n";

                if (selectedBill.BillItems.Count > 0)
                {
                    billDetails += "Items:\n";
                    foreach (var item in selectedBill.BillItems)
                    {
                        billDetails += $"• {item.ItemName} - Qty: {item.Quantity:N2}, Rate: {item.Rate:N2}, Amount: {item.TotalAmount:N2}\n";
                    }
                }

                MessageBox.Show(billDetails, "Bill Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DeleteSelectedBill()
        {
            var selectedBill = GetSelectedBill();
            if (selectedBill != null)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete Bill No: {selectedBill.BillNo}?\n\n" +
                    $"This action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        if (BillService.DeleteBill(selectedBill.BillID))
                        {
                            MessageBox.Show("Bill deleted successfully!", "Success", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadBills(); // Refresh the list
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete bill. Please try again.", "Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting bill: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        #region Event Handlers

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadBills();
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            var newBillForm = new SaleBillForm();
            if (newBillForm.ShowDialog() == DialogResult.OK)
            {
                LoadBills(); // Refresh the list
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            EditSelectedBill();
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            ViewSelectedBill();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DeleteSelectedBill();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dgvBills_SelectionChanged(object sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void dgvBills_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Skip if this is the summary row
                if (dgvBills.Rows[e.RowIndex].DataBoundItem is BillDisplayData data && data.IsSummaryRow)
                    return;
                    
                EditBill();
            }
        }

        private void dgvBills_KeyDown(object sender, KeyEventArgs e)
        {
            if (dgvBills.SelectedRows.Count > 0)
            {
                // Skip if this is the summary row
                if (dgvBills.SelectedRows[0].DataBoundItem is BillDisplayData data && data.IsSummaryRow)
                    return;
                    
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.F2)
                {
                    // Enter or F2: Edit selected bill
                    EditBill();
                    e.SuppressKeyPress = true;
                }
                else if (e.Shift && e.KeyCode == Keys.Delete)
                {
                    // Shift+Delete: Delete selected bill
                    DeleteBill();
                    e.SuppressKeyPress = true;
                }
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void DateFilter_Changed(object sender, EventArgs e)
        {
            ApplyFilters();
        }
        
        #endregion
        
        // Cell formatting event handler to colorize payment status
        private void DgvBills_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Value == null)
                return;
                
            // Check if this is the summary row
            if (dgvBills.Rows[e.RowIndex].DataBoundItem is BillDisplayData data && data.IsSummaryRow)
            {
                // Style the summary row
                e.CellStyle.Font = new Font(dgvBills.Font, FontStyle.Bold);
                e.CellStyle.BackColor = Color.LightGray;
                
                // Date columns should already be null for summary row, but just in case
                if (dgvBills.Columns[e.ColumnIndex].Name == "BillDate" || 
                    dgvBills.Columns[e.ColumnIndex].Name == "DueDate")
                {
                    if (e.Value != null)
                    {
                        e.Value = null;
                        e.FormattingApplied = true;
                    }
                }
                
                return;
            }

            // Original cell formatting for regular rows
            var columnName = dgvBills.Columns[e.ColumnIndex].Name;
            
            if (columnName == "PaymentStatusText")
            {
                string status = e.Value.ToString();
                
                if (status == "Paid")
                {
                    e.CellStyle.ForeColor = Color.Green;
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
                else if (status == "Partial")
                {
                    e.CellStyle.ForeColor = Color.Blue;
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
                else if (status == "Unpaid")
                {
                    e.CellStyle.ForeColor = Color.Red;
                }
            }
            else if (columnName == "InterestDiscountInfo")
            {
                string value = e.Value.ToString();
                
                if (value.StartsWith("+"))
                {
                    e.CellStyle.ForeColor = Color.Red; // Interest is red
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
                else if (value.StartsWith("-"))
                {
                    e.CellStyle.ForeColor = Color.Green; // Discount is green
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
            }
            // Format Balance Amount column
            else if (columnName == "BalanceAmount")
            {
                if (double.TryParse(e.Value.ToString(), out double balanceAmount))
                {
                    if (balanceAmount <= 0.01) // Fully paid
                    {
                        e.CellStyle.ForeColor = Color.Green;
                    }
                    else // Outstanding balance
                    {
                        e.CellStyle.ForeColor = Color.Red;
                    }
                }
            }
            // Format Adjusted Net Amount column - only show for paid/partial payments
            else if (columnName == "AdjustedNetAmount")
            {
                // Get payment status from the same row
                var statusCell = dgvBills.Rows[e.RowIndex].Cells["PaymentStatusText"];
                if (statusCell != null && statusCell.Value != null)
                {
                    string status = statusCell.Value.ToString();
                    if (status == "Unpaid")
                    {
                        // Hide adjusted amount for unpaid bills by making it same as regular net amount
                        var netAmountCell = dgvBills.Rows[e.RowIndex].Cells["NetAmount"];
                        if (netAmountCell != null && netAmountCell.Value != null)
                        {
                            e.Value = netAmountCell.Value;
                        }
                    }
                    else
                    {
                        // For paid/partial: show in bold with appropriate color
                        e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                        
                        // Get interest/discount info to determine color
                        var infoCell = dgvBills.Rows[e.RowIndex].Cells["InterestDiscountInfo"];
                        if (infoCell != null && infoCell.Value != null)
                        {
                            string info = infoCell.Value.ToString();
                            if (info.StartsWith("+"))
                            {
                                e.CellStyle.ForeColor = Color.Firebrick; // Higher amount due to interest
                            }
                            else if (info.StartsWith("-"))
                            {
                                e.CellStyle.ForeColor = Color.DarkGreen; // Lower amount due to discount
                            }
                        }
                    }
                }
            }
        }
    }
} 