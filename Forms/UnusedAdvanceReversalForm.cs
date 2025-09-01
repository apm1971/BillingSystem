using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Forms
{
    public partial class UnusedAdvanceReversalForm : Form
    {
        public bool ShouldRevertUnusedAdvances { get; private set; }
        public List<UnusedAdvanceDetail> UnusedAdvances { get; private set; }

        public UnusedAdvanceReversalForm(List<UnusedAdvanceDetail> unusedAdvances)
        {
            InitializeComponent();
            UnusedAdvances = unusedAdvances ?? new List<UnusedAdvanceDetail>();
            LoadUnusedAdvances();
        }

        private void LoadUnusedAdvances()
        {
            if (UnusedAdvances == null || !UnusedAdvances.Any())
            {
                lblMessage.Text = "No unused advance payments found.";
                btnYes.Enabled = false;
                return;
            }

            decimal totalUnused = UnusedAdvances.Sum(u => u.UnusedAmount);
            lblMessage.Text = $"Found {UnusedAdvances.Count} advance payment(s) with unused amounts totaling ₹{totalUnused:N2}.\n\nDo you want to revert these excess amounts back to the brokers?";

            // Populate the data grid
            dgvUnusedAdvances.DataSource = UnusedAdvances.Select(u => new
            {
                AdvanceID = u.AdvanceID,
                BrokerName = u.BrokerName,
                OriginalAmount = u.OriginalAmount,
                UsedAmount = u.UsedAmount,
                UnusedAmount = u.UnusedAmount,
                PaymentDate = u.PaymentDate.ToString("dd-MMM-yyyy"),
                Reference = u.Reference
            }).ToList();

            // Format the grid
            dgvUnusedAdvances.Columns["AdvanceID"].HeaderText = "Advance ID";
            dgvUnusedAdvances.Columns["BrokerName"].HeaderText = "Broker";
            dgvUnusedAdvances.Columns["OriginalAmount"].HeaderText = "Original Amount";
            dgvUnusedAdvances.Columns["UsedAmount"].HeaderText = "Used Amount";
            dgvUnusedAdvances.Columns["UnusedAmount"].HeaderText = "Unused Amount";
            dgvUnusedAdvances.Columns["PaymentDate"].HeaderText = "Payment Date";
            dgvUnusedAdvances.Columns["Reference"].HeaderText = "Reference";

            // Format currency columns
            dgvUnusedAdvances.Columns["OriginalAmount"].DefaultCellStyle.Format = "N2";
            dgvUnusedAdvances.Columns["UsedAmount"].DefaultCellStyle.Format = "N2";
            dgvUnusedAdvances.Columns["UnusedAmount"].DefaultCellStyle.Format = "N2";
            dgvUnusedAdvances.Columns["UnusedAmount"].DefaultCellStyle.ForeColor = Color.Red;

            // Auto-size columns
            dgvUnusedAdvances.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void btnYes_Click(object sender, EventArgs e)
        {
            ShouldRevertUnusedAdvances = true;
            DialogResult = DialogResult.Yes;
            Close();
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            ShouldRevertUnusedAdvances = false;
            DialogResult = DialogResult.No;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    public class UnusedAdvanceDetail
    {
        public int AdvanceID { get; set; }
        public string BrokerName { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal UsedAmount { get; set; }
        public decimal UnusedAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Reference { get; set; }
        public int BrokerID { get; set; }
        public int PartyID { get; set; }
    }
}
