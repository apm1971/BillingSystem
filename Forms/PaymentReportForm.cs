using System;
using System.IO;
using System.Windows.Forms;
using SaleBillSystem.NET.Models;
using SaleBillSystem.NET.Utils;

namespace SaleBillSystem.NET.Forms
{
    public partial class PaymentReportForm : Form
    {
        private PaymentReportData _reportData;
        private string _htmlContent;

        public PaymentReportForm(PaymentReportData reportData)
        {
            InitializeComponent();
            _reportData = reportData;
            LoadReport();
        }

        private void LoadReport()
        {
            try
            {
                // Generate HTML report
                _htmlContent = PaymentReportGenerator.GeneratePaymentReport(_reportData);
                
                // Load HTML into WebBrowser
                webBrowser.DocumentText = _htmlContent;
                
                // Set form title
                this.Text = $"Payment Report - Payment ID: {_reportData.PaymentID}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading report: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                // Show print dialog
                webBrowser.ShowPrintDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing report: {ex.Message}", "Print Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSaveAs_Click(object sender, EventArgs e)
        {
            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "HTML Files (*.html)|*.html|All Files (*.*)|*.*";
                    saveDialog.FileName = $"PaymentReport_{_reportData.PaymentID}_{DateTime.Now:yyyyMMdd_HHmmss}.html";
                    saveDialog.DefaultExt = "html";
                    
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(saveDialog.FileName, _htmlContent);
                        MessageBox.Show($"Report saved successfully to:\n{saveDialog.FileName}", 
                            "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving report: {ex.Message}", "Save Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadReport();
        }
    }
}
