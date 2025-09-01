using System;
using System.Collections.Generic;
using System.Text;
using SaleBillSystem.NET.Models;

namespace SaleBillSystem.NET.Utils
{
    /// <summary>
    /// Generates HTML reports for payment settlements
    /// </summary>
    public class PaymentReportGenerator
    {
        /// <summary>
        /// Generates a complete HTML report from payment data
        /// </summary>
        public static string GeneratePaymentReport(PaymentReportData reportData)
        {
            var html = new StringBuilder();
            
            // Start HTML document
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='en'>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='UTF-8'>");
            html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine("    <title>FIFO Settlement Report</title>");
            html.AppendLine(GetCSSStyles());
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            // Report Header
            html.AppendLine(GenerateReportHeader(reportData));
            
            // Payment Summary
            html.AppendLine(GeneratePaymentSummary(reportData));
            
            // Settlement Summary
            html.AppendLine(GenerateSettlementSummary(reportData));
            
            // Bill Details
            html.AppendLine(GenerateBillDetails(reportData));
            
            // Advance Utilizations
            html.AppendLine(GenerateAdvanceUtilizations(reportData));
            
            // Unused Advance Reversals (if any)
            if (reportData.UnusedAdvanceReversals.Count > 0)
            {
                html.AppendLine(GenerateUnusedAdvanceReversals(reportData));
            }
            
            // Payment Terms
            html.AppendLine(GeneratePaymentTerms(reportData));
            
            // Report Footer
            html.AppendLine(GenerateReportFooter(reportData));
            
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return html.ToString();
        }

        /// <summary>
        /// CSS styles for the report
        /// </summary>
        private static string GetCSSStyles()
        {
            return @"
    <style>
        body {
            font-family: Arial, sans-serif;
            margin: 0;
            padding: 10px;
            background-color: white;
            color: black;
            font-size: 12px;
        }
        .header {
            text-align: center;
            border-bottom: 2px solid black;
            padding: 5px 0;
            margin-bottom: 10px;
        }
        .header h1 {
            margin: 0;
            font-size: 16px;
            font-weight: bold;
        }
        .section {
            margin-bottom: 10px;
            border-bottom: 1px solid black;
            padding-bottom: 5px;
        }
        .section-title {
            font-size: 14px;
            font-weight: bold;
            margin-bottom: 5px;
            text-decoration: underline;
        }
        .horizontal-row {
            display: flex;
            justify-content: space-between;
            margin-bottom: 2px;
        }
        .horizontal-row .label {
            font-weight: bold;
            min-width: 120px;
        }
        .horizontal-row .value {
            text-align: right;
            flex: 1;
        }
        .two-column {
            display: flex;
            justify-content: space-between;
            gap: 20px;
        }
        .column {
            flex: 1;
        }
        table {
            width: 100%;
            border-collapse: collapse;
            margin: 5px 0;
            font-size: 11px;
        }
        th, td {
            padding: 3px 5px;
            text-align: left;
            border: 1px solid black;
        }
        th {
            background-color: #f0f0f0;
            font-weight: bold;
        }
        .amount {
            text-align: right;
        }
        .total-row {
            font-weight: bold;
            background-color: #f0f0f0;
        }
        }
        tr:hover {
            background-color: #f8f9fa;
        }
        .amount {
            text-align: right;
            font-weight: 600;
        }
        .amount.positive {
            color: #28a745;
        }
        .amount.negative {
            color: #dc3545;
        }
        .status {
            padding: 4px 8px;
            border-radius: 4px;
            font-size: 12px;
            font-weight: 600;
            text-transform: uppercase;
        }
        .status.paid {
            background-color: #d4edda;
            color: #155724;
        }
        .status.partial {
            background-color: #fff3cd;
            color: #856404;
        }
        .status.unpaid {
            background-color: #f8d7da;
            color: #721c24;
        }
        .interest-period {
            background-color: #e3f2fd;
            padding: 15px;
            border-radius: 6px;
            margin: 10px 0;
        }
        .interest-period h4 {
            margin: 0 0 10px 0;
            color: #1976d2;
            font-size: 14px;
        }
        .interest-details {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
            gap: 10px;
            font-size: 13px;
        }
        .footer {
            background-color: #f8f9fa;
            padding: 20px;
            text-align: center;
            color: #6c757d;
            font-size: 14px;
        }
        .print-button {
            position: fixed;
            top: 20px;
            right: 20px;
            background-color: #007bff;
            color: white;
            border: none;
            padding: 10px 20px;
            border-radius: 5px;
            cursor: pointer;
            font-size: 14px;
        }
        .print-button:hover {
            background-color: #0056b3;
        }
        @media print {
            body { background-color: white; }
            .report-container { box-shadow: none; }
            .print-button { display: none; }
            .section { page-break-inside: avoid; }
        }
    </style>";
        }

        /// <summary>
        /// Generates the report header
        /// </summary>
        private static string GenerateReportHeader(PaymentReportData data)
        {
            var html = new StringBuilder();
            html.AppendLine("    <div class='report-container'>");
            html.AppendLine("        <div class='header'>");
            html.AppendLine($"            <h1>{data.ReportTitle}</h1>");
            html.AppendLine($"            <p class='subtitle'>Generated on {data.ReportDate:dddd, MMMM dd, yyyy 'at' h:mm tt}</p>");
            html.AppendLine("        </div>");
            
            if (!string.IsNullOrEmpty(data.CompanyName))
            {
                html.AppendLine("        <div class='company-info'>");
                html.AppendLine($"            <h2>{data.CompanyName}</h2>");
                if (!string.IsNullOrEmpty(data.CompanyAddress))
                    html.AppendLine($"            <p>{data.CompanyAddress}</p>");
                if (!string.IsNullOrEmpty(data.CompanyPhone))
                    html.AppendLine($"            <p>Phone: {data.CompanyPhone}</p>");
                html.AppendLine("        </div>");
            }
            
            return html.ToString();
        }

        /// <summary>
        /// Generates payment summary section
        /// </summary>
        private static string GeneratePaymentSummary(PaymentReportData data)
        {
            var html = new StringBuilder();
            html.AppendLine("        <div class='section'>");
            html.AppendLine("            <h2 class='section-title'>Payment Information</h2>");
            html.AppendLine("            <div class='summary-grid'>");
            
            html.AppendLine("                <div class='summary-card'>");
            html.AppendLine("                    <h3>Payment ID</h3>");
            html.AppendLine($"                    <div class='value'>{data.PaymentID}</div>");
            html.AppendLine("                </div>");
            
            html.AppendLine("                <div class='summary-card'>");
            html.AppendLine("                    <h3>Payment Date</h3>");
            html.AppendLine($"                    <div class='value'>{data.PaymentDate:dd-MMM-yyyy}</div>");
            html.AppendLine("                </div>");
            
            html.AppendLine("                <div class='summary-card'>");
            html.AppendLine("                    <h3>Payment Method</h3>");
            html.AppendLine($"                    <div class='value'>{data.PaymentMethod}</div>");
            html.AppendLine("                </div>");
            
            html.AppendLine("                <div class='summary-card'>");
            html.AppendLine("                    <h3>Total Amount</h3>");
            html.AppendLine($"                    <div class='value amount positive'>₹{data.TotalPaymentAmount:N2}</div>");
            html.AppendLine("                </div>");
            
            if (!string.IsNullOrEmpty(data.PartyName))
            {
                html.AppendLine("                <div class='summary-card'>");
                html.AppendLine("                    <h3>Party</h3>");
                html.AppendLine($"                    <div class='value'>{data.PartyName}</div>");
                html.AppendLine("                </div>");
            }
            
            if (!string.IsNullOrEmpty(data.BrokerName))
            {
                html.AppendLine("                <div class='summary-card'>");
                html.AppendLine("                    <h3>Broker</h3>");
                html.AppendLine($"                    <div class='value'>{data.BrokerName}</div>");
                html.AppendLine("                </div>");
            }
            
            html.AppendLine("            </div>");
            html.AppendLine("        </div>");
            
            return html.ToString();
        }

        /// <summary>
        /// Generates settlement summary section
        /// </summary>
        private static string GenerateSettlementSummary(PaymentReportData data)
        {
            var html = new StringBuilder();
            html.AppendLine("        <div class='section'>");
            html.AppendLine("            <h2 class='section-title'>Settlement Summary</h2>");
            html.AppendLine("            <div class='summary-grid'>");
            
            html.AppendLine("                <div class='summary-card'>");
            html.AppendLine("                    <h3>Total Amount Due</h3>");
            html.AppendLine($"                    <div class='value amount'>{data.TotalAmountDue:N2}</div>");
            html.AppendLine("                </div>");
            
            html.AppendLine("                <div class='summary-card'>");
            html.AppendLine("                    <h3>Advance Used</h3>");
            html.AppendLine($"                    <div class='value amount positive'>₹{data.TotalAdvanceUsed:N2}</div>");
            html.AppendLine("                </div>");
            
            html.AppendLine("                <div class='summary-card'>");
            html.AppendLine("                    <h3>Cash Needed</h3>");
            html.AppendLine($"                    <div class='value amount'>{data.TotalCashNeeded:N2}</div>");
            html.AppendLine("                </div>");
            
            html.AppendLine("                <div class='summary-card'>");
            html.AppendLine("                    <h3>Interest Charged</h3>");
            html.AppendLine($"                    <div class='value amount negative'>₹{data.TotalInterest:N2}</div>");
            html.AppendLine("                </div>");
            
            html.AppendLine("                <div class='summary-card'>");
            html.AppendLine("                    <h3>Discount Earned</h3>");
            html.AppendLine($"                    <div class='value amount positive'>₹{data.TotalDiscount:N2}</div>");
            html.AppendLine("                </div>");
            
            html.AppendLine("                <div class='summary-card'>");
            html.AppendLine("                    <h3>Brokerage</h3>");
            html.AppendLine($"                    <div class='value amount'>{data.TotalBrokerage:N2}</div>");
            html.AppendLine("                </div>");
            
            if (data.UnusedAdvance > 0)
            {
                html.AppendLine("                <div class='summary-card warning'>");
                html.AppendLine("                    <h3>Unused Advance</h3>");
                html.AppendLine($"                    <div class='value amount'>₹{data.UnusedAdvance:N2}</div>");
                html.AppendLine("                </div>");
            }
            
            html.AppendLine("            </div>");
            html.AppendLine("        </div>");
            
            return html.ToString();
        }

        /// <summary>
        /// Generates bill details section
        /// </summary>
        private static string GenerateBillDetails(PaymentReportData data)
        {
            var html = new StringBuilder();
            html.AppendLine("        <div class='section'>");
            html.AppendLine("            <h2 class='section-title'>Bill Details</h2>");
            
            html.AppendLine("            <table>");
            html.AppendLine("                <thead>");
            html.AppendLine("                    <tr>");
            html.AppendLine("                        <th>Bill No</th>");
            html.AppendLine("                        <th>Date</th>");
            html.AppendLine("                        <th>Original Amount</th>");
            html.AppendLine("                        <th>Balance Due</th>");
            html.AppendLine("                        <th>Amount Paid</th>");
            html.AppendLine("                        <th>Interest</th>");
            html.AppendLine("                        <th>Discount</th>");
            html.AppendLine("                        <th>Advance Used</th>");
            html.AppendLine("                        <th>Cash Used</th>");
            html.AppendLine("                        <th>Status</th>");
            html.AppendLine("                    </tr>");
            html.AppendLine("                </thead>");
            html.AppendLine("                <tbody>");
            
            foreach (var bill in data.BillDetails)
            {
                html.AppendLine("                    <tr>");
                html.AppendLine($"                        <td>{bill.BillNo}</td>");
                html.AppendLine($"                        <td>{bill.BillDate:dd-MMM-yyyy}</td>");
                html.AppendLine($"                        <td class='amount'>₹{bill.OriginalAmount:N2}</td>");
                html.AppendLine($"                        <td class='amount'>₹{bill.BalanceDue:N2}</td>");
                html.AppendLine($"                        <td class='amount positive'>₹{bill.AmountPaid:N2}</td>");
                html.AppendLine($"                        <td class='amount negative'>₹{bill.InterestCharged:N2}</td>");
                html.AppendLine($"                        <td class='amount positive'>₹{bill.DiscountEarned:N2}</td>");
                html.AppendLine($"                        <td class='amount'>₹{bill.AdvanceUsed:N2}</td>");
                html.AppendLine($"                        <td class='amount'>₹{bill.CashUsed:N2}</td>");
                html.AppendLine($"                        <td><span class='status {bill.Status.ToLower()}'>{bill.Status}</span></td>");
                html.AppendLine("                    </tr>");
                
                // Add interest period details if any
                if (bill.InterestPeriods.Count > 0)
                {
                    html.AppendLine("                    <tr>");
                    html.AppendLine("                        <td colspan='10'>");
                    html.AppendLine("                            <div class='interest-period'>");
                    html.AppendLine("                                <h4>Interest Calculation Details</h4>");
                    foreach (var period in bill.InterestPeriods)
                    {
                        html.AppendLine("                                <div class='interest-details'>");
                        html.AppendLine($"                                    <div><strong>Period:</strong> {period.StartDate:dd-MMM-yyyy} to {period.EndDate:dd-MMM-yyyy}</div>");
                        html.AppendLine($"                                    <div><strong>Days:</strong> {period.Days}</div>");
                        html.AppendLine($"                                    <div><strong>Principal:</strong> ₹{period.Principal:N2}</div>");
                        html.AppendLine($"                                    <div><strong>Rate:</strong> {period.InterestRate}%</div>");
                        html.AppendLine($"                                    <div><strong>Interest:</strong> ₹{period.InterestAmount:N2}</div>");
                        html.AppendLine("                                </div>");
                    }
                    html.AppendLine("                            </div>");
                    html.AppendLine("                        </td>");
                    html.AppendLine("                    </tr>");
                }
            }
            
            html.AppendLine("                </tbody>");
            html.AppendLine("            </table>");
            html.AppendLine("        </div>");
            
            return html.ToString();
        }

        /// <summary>
        /// Generates advance utilizations section
        /// </summary>
        private static string GenerateAdvanceUtilizations(PaymentReportData data)
        {
            var html = new StringBuilder();
            html.AppendLine("        <div class='section'>");
            html.AppendLine("            <h2 class='section-title'>Advance Payment Utilizations</h2>");
            
            html.AppendLine("            <table>");
            html.AppendLine("                <thead>");
            html.AppendLine("                    <tr>");
            html.AppendLine("                        <th>Advance ID</th>");
            html.AppendLine("                        <th>Date</th>");
            html.AppendLine("                        <th>Original Amount</th>");
            html.AppendLine("                        <th>Amount Used</th>");
            html.AppendLine("                        <th>Remaining</th>");
            html.AppendLine("                        <th>Payment Method</th>");
            html.AppendLine("                        <th>Status</th>");
            html.AppendLine("                        <th>Used For Bills</th>");
            html.AppendLine("                    </tr>");
            html.AppendLine("                </thead>");
            html.AppendLine("                <tbody>");
            
            foreach (var advance in data.AdvanceUtilizations)
            {
                html.AppendLine("                    <tr>");
                html.AppendLine($"                        <td>{advance.AdvanceID}</td>");
                html.AppendLine($"                        <td>{advance.AdvanceDate:dd-MMM-yyyy}</td>");
                html.AppendLine($"                        <td class='amount'>₹{advance.OriginalAmount:N2}</td>");
                html.AppendLine($"                        <td class='amount positive'>₹{advance.AmountUsed:N2}</td>");
                html.AppendLine($"                        <td class='amount'>₹{advance.RemainingAmount:N2}</td>");
                html.AppendLine($"                        <td>{advance.PaymentMethod}</td>");
                html.AppendLine($"                        <td><span class='status {advance.Status.ToLower()}'>{advance.Status}</span></td>");
                html.AppendLine($"                        <td>{string.Join(", ", advance.UsedForBills)}</td>");
                html.AppendLine("                    </tr>");
            }
            
            html.AppendLine("                </tbody>");
            html.AppendLine("            </table>");
            html.AppendLine("        </div>");
            
            return html.ToString();
        }

        /// <summary>
        /// Generates unused advance reversals section
        /// </summary>
        private static string GenerateUnusedAdvanceReversals(PaymentReportData data)
        {
            var html = new StringBuilder();
            html.AppendLine("        <div class='section'>");
            html.AppendLine("            <h2 class='section-title'>Unused Advance Reversals</h2>");
            
            html.AppendLine("            <table>");
            html.AppendLine("                <thead>");
            html.AppendLine("                    <tr>");
            html.AppendLine("                        <th>Original Advance ID</th>");
            html.AppendLine("                        <th>Reversal Advance ID</th>");
            html.AppendLine("                        <th>Broker</th>");
            html.AppendLine("                        <th>Original Amount</th>");
            html.AppendLine("                        <th>Used Amount</th>");
            html.AppendLine("                        <th>Unused Amount</th>");
            html.AppendLine("                        <th>Reversal Date</th>");
            html.AppendLine("                    </tr>");
            html.AppendLine("                </thead>");
            html.AppendLine("                <tbody>");
            
            foreach (var reversal in data.UnusedAdvanceReversals)
            {
                html.AppendLine("                    <tr>");
                html.AppendLine($"                        <td>{reversal.OriginalAdvanceID}</td>");
                html.AppendLine($"                        <td>{reversal.ReversalAdvanceID}</td>");
                html.AppendLine($"                        <td>{reversal.BrokerName}</td>");
                html.AppendLine($"                        <td class='amount'>₹{reversal.OriginalAmount:N2}</td>");
                html.AppendLine($"                        <td class='amount positive'>₹{reversal.UsedAmount:N2}</td>");
                html.AppendLine($"                        <td class='amount negative'>₹{reversal.UnusedAmount:N2}</td>");
                html.AppendLine($"                        <td>{reversal.ReversalDate:dd-MMM-yyyy}</td>");
                html.AppendLine("                    </tr>");
            }
            
            html.AppendLine("                </tbody>");
            html.AppendLine("            </table>");
            html.AppendLine("        </div>");
            
            return html.ToString();
        }

        /// <summary>
        /// Generates payment terms section
        /// </summary>
        private static string GeneratePaymentTerms(PaymentReportData data)
        {
            var html = new StringBuilder();
            html.AppendLine("        <div class='section'>");
            html.AppendLine("            <h2 class='section-title'>Payment Terms Used</h2>");
            html.AppendLine("            <div class='summary-grid'>");
            
            html.AppendLine("                <div class='summary-card'>");
            html.AppendLine("                    <h3>Interest Days</h3>");
            html.AppendLine($"                    <div class='value'>{data.PaymentTerms.InterestDays} days</div>");
            html.AppendLine("                </div>");
            
            html.AppendLine("                <div class='summary-card'>");
            html.AppendLine("                    <h3>Interest Rate</h3>");
            html.AppendLine($"                    <div class='value'>{data.PaymentTerms.InterestRate}%</div>");
            html.AppendLine("                </div>");
            
            html.AppendLine("                <div class='summary-card'>");
            html.AppendLine("                    <h3>Discount Days</h3>");
            html.AppendLine($"                    <div class='value'>{data.PaymentTerms.DiscountDays} days</div>");
            html.AppendLine("                </div>");
            
            html.AppendLine("                <div class='summary-card'>");
            html.AppendLine("                    <h3>Discount Rate</h3>");
            html.AppendLine($"                    <div class='value'>{data.PaymentTerms.DiscountRate}%</div>");
            html.AppendLine("                </div>");
            
            html.AppendLine("                <div class='summary-card'>");
            html.AppendLine("                    <h3>Brokerage Rate</h3>");
            html.AppendLine($"                    <div class='value'>{data.PaymentTerms.BrokerageRate}%</div>");
            html.AppendLine("                </div>");
            
            html.AppendLine("                <div class='summary-card'>");
            html.AppendLine("                    <h3>Terms Source</h3>");
            html.AppendLine($"                    <div class='value'>{data.PaymentTerms.TermsSource}</div>");
            html.AppendLine("                </div>");
            
            html.AppendLine("            </div>");
            html.AppendLine("        </div>");
            
            return html.ToString();
        }

        /// <summary>
        /// Generates the report footer
        /// </summary>
        private static string GenerateReportFooter(PaymentReportData data)
        {
            var html = new StringBuilder();
            html.AppendLine("        <div class='footer'>");
            html.AppendLine($"            <p>Report generated on {data.GeneratedAt:dddd, MMMM dd, yyyy 'at' h:mm tt}</p>");
            if (!string.IsNullOrEmpty(data.GeneratedBy))
            {
                html.AppendLine($"            <p>Generated by: {data.GeneratedBy}</p>");
            }
            html.AppendLine("            <p>This is a computer-generated report. No signature required.</p>");
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
            
            return html.ToString();
        }
    }
}
