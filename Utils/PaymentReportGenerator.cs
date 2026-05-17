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
        /// Generates a compact slip report optimized for 3-inch thermal printer (72mm width)
        /// Shows complete bill details including party, amounts, interest, discount, brokerage
        /// </summary>
        public static string GenerateSlipReport(PaymentReportData reportData)
        {
            var html = new StringBuilder();
            
            // Start HTML document with thermal printer specific styles
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='en'>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='UTF-8'>");
            html.AppendLine("    <title>Payment Slip</title>");
            html.AppendLine(GetSlipCSSStyles());
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            // Compact slip content
            html.AppendLine("<div class='slip-container'>");
            
            // Header
            html.AppendLine("<div class='slip-header'>");
            html.AppendLine("<div class='title'>PAYMENT SLIP</div>");
            html.AppendLine($"<div class='date'>{reportData.PaymentDate:dd-MMM-yyyy}</div>");
            html.AppendLine("</div>");
            
            // Divider
            html.AppendLine("<div class='divider'>================================</div>");
            
            // Party and Broker Info
            if (!string.IsNullOrEmpty(reportData.PartyName))
                html.AppendLine($"<div class='info-row'><span class='label'>Party:</span><span class='value'>{TruncateText(reportData.PartyName, 20)}</span></div>");
            
            if (!string.IsNullOrEmpty(reportData.BrokerName))
                html.AppendLine($"<div class='info-row'><span class='label'>Broker:</span><span class='value'>{TruncateText(reportData.BrokerName, 19)}</span></div>");
            
            html.AppendLine($"<div class='info-row'><span class='label'>Method:</span><span class='value'>{reportData.PaymentMethod}</span></div>");
            
            // Divider
            html.AppendLine("<div class='divider'>--------------------------------</div>");
            
            // Bill Details Section - Compact layout
            if (reportData.BillDetails.Count > 0)
            {
                html.AppendLine("<div class='section-title'>BILLS SETTLED</div>");
                html.AppendLine("<div class='bill-list'>");
                
                foreach (var bill in reportData.BillDetails)
                {
                    html.AppendLine("<div class='bill-item-compact'>");
                    
                    // Line 1: Bill No + Date + Party (all on one line)
                    var partyText = !string.IsNullOrEmpty(bill.PartyName) ? $" {TruncateText(bill.PartyName, 14)}" : "";
                    html.AppendLine($"<div class='bill-hdr'><span class='bill-no'>{bill.BillNo}</span><span>{bill.BillDate:dd-MM-yy}{partyText}</span></div>");
                    
                    // Line 2: Original + Balance + Paid side by side
                    html.AppendLine($"<div class='bill-amounts'><span>Org:₹{bill.OriginalAmount:N0}</span><span>Bal:₹{bill.BalanceDue:N0}</span><span class='paid'>Pd:₹{bill.AmountPaid:N0}</span></div>");
                    
                    // Line 3: Adjustments inline (only non-zero values)
                    var adjustments = new List<string>();
                    if (bill.InterestCharged > 0) adjustments.Add($"Int:₹{bill.InterestCharged:N0}");
                    if (bill.DiscountEarned > 0) adjustments.Add($"Dis:₹{bill.DiscountEarned:N0}");
                    if (bill.Brokerage > 0) adjustments.Add($"Brk:₹{bill.Brokerage:N0}");
                    if (bill.AdvanceUsed > 0) adjustments.Add($"Adv:₹{bill.AdvanceUsed:N0}");
                    if (bill.CashUsed > 0) adjustments.Add($"Csh:₹{bill.CashUsed:N0}");
                    
                    if (adjustments.Count > 0)
                        html.AppendLine($"<div class='bill-adj'>{string.Join(" ", adjustments)}</div>");
                    
                    html.AppendLine("</div>"); // bill-item-compact
                }
                html.AppendLine("</div>"); // bill-list
            }
            
            // Divider
            html.AppendLine("<div class='divider'>================================</div>");
            
            // Advance Utilizations Section
            if (reportData.AdvanceUtilizations.Count > 0)
            {
                html.AppendLine("<div class='section-title'>PAYMENTS USED</div>");
                html.AppendLine("<div class='advance-list'>");
                
                foreach (var adv in reportData.AdvanceUtilizations)
                {
                    html.AppendLine("<div class='adv-item'>");
                    html.AppendLine($"<div class='adv-row'><span>ID:{adv.AdvanceID}</span><span>{adv.AdvanceDate:dd-MM-yy}</span></div>");
                    html.AppendLine($"<div class='adv-row'><span>Used:</span><span>₹{adv.AmountUsed:N0}</span></div>");
                    if (adv.UsedForBills.Count > 0)
                    {
                        html.AppendLine($"<div class='adv-bills'>For: {string.Join(",", adv.UsedForBills)}</div>");
                    }
                    html.AppendLine("</div>");
                }
                html.AppendLine("</div>");
                html.AppendLine("<div class='divider'>--------------------------------</div>");
            }
            
            // Summary Section
            html.AppendLine("<div class='section-title'>SUMMARY</div>");
            html.AppendLine($"<div class='amount-row'><span class='label'>Amount Due:</span><span class='value'>₹{reportData.TotalAmountDue:N0}</span></div>");
            
            if (reportData.TotalAdvanceUsed > 0)
                html.AppendLine($"<div class='amount-row'><span class='label'>Advance Used:</span><span class='value'>₹{reportData.TotalAdvanceUsed:N0}</span></div>");
            
            if (reportData.TotalInterest > 0)
                html.AppendLine($"<div class='amount-row negative'><span class='label'>+Interest:</span><span class='value'>₹{reportData.TotalInterest:N0}</span></div>");
            
            if (reportData.TotalDiscount > 0)
                html.AppendLine($"<div class='amount-row positive'><span class='label'>-Discount:</span><span class='value'>₹{reportData.TotalDiscount:N0}</span></div>");
            
            if (reportData.TotalBrokerage > 0)
                html.AppendLine($"<div class='amount-row'><span class='label'>Brokerage:</span><span class='value'>₹{reportData.TotalBrokerage:N0}</span></div>");
            
            // Divider
            html.AppendLine("<div class='divider'>--------------------------------</div>");
            
            // Total
            html.AppendLine($"<div class='total-row'><span class='label'>CASH PAID:</span><span class='value'>₹{reportData.TotalCashNeeded:N0}</span></div>");
            html.AppendLine($"<div class='total-row grand'><span class='label'>TOTAL:</span><span class='value'>₹{reportData.TotalPaymentAmount:N0}</span></div>");
            
            if (reportData.UnusedAdvance > 0)
            {
                html.AppendLine($"<div class='amount-row'><span class='label'>Unused Adv:</span><span class='value'>₹{reportData.UnusedAdvance:N0}</span></div>");
            }
            
            // Cheque details if any
            if (reportData.ChequeAmountFirm1 > 0 || reportData.ChequeAmountFirm2 > 0)
            {
                html.AppendLine("<div class='divider'>--------------------------------</div>");
                html.AppendLine("<div class='section-title'>CHEQUE DETAILS</div>");
                if (reportData.ChequeAmountFirm1 > 0)
                    html.AppendLine($"<div class='amount-row'><span class='label'>Firm1 Chq:</span><span class='value'>₹{reportData.ChequeAmountFirm1:N0}</span></div>");
                if (reportData.ChequeAmountFirm2 > 0)
                    html.AppendLine($"<div class='amount-row'><span class='label'>Firm2 Chq:</span><span class='value'>₹{reportData.ChequeAmountFirm2:N0}</span></div>");
            }
            
            // Divider
            html.AppendLine("<div class='divider'>================================</div>");
            
            // Payment Terms (compact)
            html.AppendLine("<div class='terms-row'>");
            html.AppendLine($"<span>Int:{reportData.PaymentTerms.InterestDays}d@{reportData.PaymentTerms.InterestRate}%</span>");
            html.AppendLine($"<span>Disc:{reportData.PaymentTerms.DiscountDays}d@{reportData.PaymentTerms.DiscountRate}%</span>");
            html.AppendLine("</div>");
            if (reportData.PaymentTerms.BrokerageRate > 0)
            {
                html.AppendLine($"<div class='terms-row'><span>Brokerage: {reportData.PaymentTerms.BrokerageRate}%</span></div>");
            }
            
            // Footer
            html.AppendLine("<div class='divider'>================================</div>");
            html.AppendLine("<div class='footer'>");
            html.AppendLine("<div>Thank You!</div>");
            html.AppendLine($"<div class='bill-count'>{reportData.BillDetails.Count} Bill(s) Settled</div>");
            html.AppendLine($"<div class='timestamp'>{DateTime.Now:dd-MMM-yy HH:mm}</div>");
            html.AppendLine("</div>");
            
            html.AppendLine("</div>"); // slip-container
            
            // Print script
            html.AppendLine("<script>");
            html.AppendLine("window.onload = function() { window.print(); };");
            html.AppendLine("</script>");
            
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return html.ToString();
        }

        /// <summary>
        /// Truncates text to fit within thermal printer width
        /// </summary>
        private static string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Length <= maxLength ? text : text.Substring(0, maxLength - 2) + "..";
        }

        /// <summary>
        /// CSS styles for 3-inch thermal printer slip (72mm width)
        /// </summary>
        private static string GetSlipCSSStyles()
        {
            return @"
    <style>
        @page {
            size: 72mm auto;
            margin: 0;
        }
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        body {
            font-family: 'Courier New', Courier, monospace;
            font-size: 10px;
            line-height: 1.2;
            background: white;
            color: black;
            width: 72mm;
            margin: 0 auto;
        }
        .slip-container {
            width: 100%;
            padding: 2mm;
        }
        .slip-header {
            text-align: center;
            margin-bottom: 2mm;
        }
        .slip-header .title {
            font-size: 14px;
            font-weight: bold;
            letter-spacing: 1px;
        }
        .slip-header .date {
            font-size: 10px;
            margin-top: 1mm;
        }
        .divider {
            text-align: center;
            font-size: 8px;
            margin: 1mm 0;
            letter-spacing: -0.5px;
        }
        .info-row, .amount-row {
            display: flex;
            justify-content: space-between;
            font-size: 9px;
            padding: 0.5mm 0;
        }
        .info-row .label, .amount-row .label {
            font-weight: bold;
        }
        .amount-row.negative {
            color: #c00;
        }
        .amount-row.positive {
            color: #090;
        }
        .total-row {
            display: flex;
            justify-content: space-between;
            font-size: 11px;
            font-weight: bold;
            padding: 1mm 0;
            border-top: 1px dashed black;
            margin: 1mm 0;
        }
        .total-row.grand {
            font-size: 12px;
            border-bottom: 1px dashed black;
        }
        .section-title {
            font-size: 9px;
            font-weight: bold;
            text-align: center;
            margin: 1mm 0;
            text-decoration: underline;
        }
        .bill-list {
            font-size: 8px;
        }
        .bill-item-compact {
            padding: 0.5mm 0;
            border-bottom: 1px dotted #999;
            margin-bottom: 0.5mm;
        }
        .bill-item-compact:last-child {
            border-bottom: none;
        }
        .bill-hdr {
            display: flex;
            justify-content: space-between;
            font-weight: bold;
            font-size: 8px;
        }
        .bill-no {
            font-weight: bold;
        }
        .bill-amounts {
            display: flex;
            justify-content: space-between;
            font-size: 8px;
            padding: 0 1mm;
        }
        .bill-amounts .paid {
            font-weight: bold;
        }
        .bill-adj {
            font-size: 7px;
            padding: 0 1mm;
            color: #333;
        }
        .advance-list {
            font-size: 8px;
        }
        .adv-item {
            padding: 0.5mm 0;
            border-bottom: 1px dotted #ccc;
        }
        .adv-row {
            display: flex;
            justify-content: space-between;
            font-size: 8px;
        }
        .adv-bills {
            font-size: 7px;
            color: #000;
        }
        .terms-row {
            display: flex;
            justify-content: space-between;
            font-size: 7px;
            padding: 1mm 0;
        }
        .footer {
            text-align: center;
            font-size: 9px;
            margin-top: 2mm;
        }
        .footer .bill-count {
            font-size: 8px;
            margin-top: 1mm;
        }
        .footer .timestamp {
            font-size: 7px;
            color: #000;
            margin-top: 1mm;
        }
        @media print {
            body {
                width: 72mm;
            }
            .slip-container {
                page-break-after: always;
            }
        }
        @media screen {
            body {
                border: 1px dashed #ccc;
                margin: 10px auto;
            }
        }
    </style>";
        }

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
            html.AppendLine("    <title>Settlement Report</title>");
            html.AppendLine(GetCSSStyles());
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("    <button class='print-button' onclick='window.print()'>🖨️ Print</button>");
            
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
            
            // Unused advance reversals are now shown as "Reverted" status in advance utilizations table
            
            // Payment Terms
            html.AppendLine(GeneratePaymentTerms(reportData));
            
            // Report Footer
            // html.AppendLine(GenerateReportFooter(reportData));
            
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
            padding: 8px;
            background-color: white;
            color: black;
            font-size: 11px;
            max-width: 100%;
        }
        .report-container {
            max-width: 210mm;
            margin: 0 auto;
        }
        .header {
            text-align: center;
            border-bottom: 2px solid black;
            padding: 3px 0;
            margin-bottom: 8px;
        }
        .header h1 {
            margin: 0;
            font-size: 14px;
            font-weight: bold;
        }
        .header .subtitle {
            margin: 2px 0 0 0;
            font-size: 10px;
        }
        .company-info {
            text-align: center;
            margin-bottom: 8px;
            font-size: 10px;
        }
        .company-info h2 {
            margin: 0;
            font-size: 12px;
        }
        .company-info p {
            margin: 1px 0;
        }
        .section {
            margin-bottom: 8px;
            border-bottom: 1px solid #ddd;
            padding-bottom: 5px;
        }
        .section-title {
            font-size: 12px;
            font-weight: bold;
            margin-bottom: 4px;
            text-align: center;
            text-decoration: underline;
        }
        .summary-grid {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 8px;
            margin-bottom: 5px;
        }
        .summary-card {
            border: 1px solid #ddd;
            padding: 4px;
            border-radius: 3px;
            text-align: center;
        }
        .summary-card h3 {
            margin: 0 0 2px 0;
            font-size: 10px;
            font-weight: bold;
            color: #666;
        }
        .summary-card .value {
            font-size: 11px;
            font-weight: bold;
        }
        .horizontal-row {
            display: flex;
            justify-content: space-between;
            margin-bottom: 2px;
            font-size: 10px;
        }
        .horizontal-row .label {
            font-weight: bold;
            min-width: 80px;
        }
        .horizontal-row .value {
            text-align: right;
            flex: 1;
        }
        .two-column {
            display: flex;
            justify-content: space-between;
            gap: 15px;
        }
        .column {
            flex: 1;
        }
        table {
            width: 100%;
            border-collapse: collapse;
            margin: 3px 0;
            font-size: 9px;
        }
        th, td {
            padding: 2px 3px;
            text-align: left;
            border: 1px solid #ccc;
            vertical-align: top;
        }
        th {
            background-color: #f0f0f0;
            font-weight: bold;
            font-size: 9px;
        }
        .amount {
            text-align: right;
            font-weight: 500;
        }
        .amount.positive {
            color: #28a745;
        }
        .amount.negative {
            color: #dc3545;
        }
        .total-row {
            font-weight: bold;
            background-color: #f0f0f0;
        }
        .status {
            padding: 1px 4px;
            border-radius: 2px;
            font-size: 8px;
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
            background-color: #f8f9fa;
            padding: 4px;
            border-radius: 3px;
            margin: 2px 0;
            font-size: 8px;
        }
        .interest-period h4 {
            margin: 0 0 3px 0;
            color: #495057;
            font-size: 9px;
        }
        .interest-details {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(100px, 1fr));
            gap: 3px;
            font-size: 8px;
        }
        .footer {
            background-color: #f8f9fa;
            padding: 5px;
            text-align: center;
            color: #6c757d;
            font-size: 9px;
            margin-top: 10px;
        }
        .print-button {
            position: fixed;
            top: 10px;
            right: 10px;
            background-color: #007bff;
            color: white;
            border: none;
            padding: 5px 10px;
            border-radius: 3px;
            cursor: pointer;
            font-size: 10px;
            z-index: 1000;
        }
        .print-button:hover {
            background-color: #0056b3;
        }
        
        /* Slip-like compact styling */
        .slip-header {
            border: 2px solid black;
            padding: 5px;
            margin-bottom: 5px;
        }
        
        .compact-info {
            display: flex;
            justify-content: flex-start;
            align-items: center;
            flex-wrap: wrap;
            gap: 15px;
            font-size: 10px;
            margin-bottom: 5px;
            padding: 3px 0;
            border-bottom: 1px solid #eee;
        }
        .compact-info span {
            white-space: nowrap;
        }
        
        .info-group {
            display: flex;
            flex-direction: column;
            gap: 2px;
        }
        
        .info-item {
            display: flex;
            justify-content: space-between;
            min-width: 150px;
        }
        
        .info-item .label {
            font-weight: bold;
        }
        
        @media print {
            body { 
                background-color: white; 
                font-size: 10px;
            }
            .report-container { 
                box-shadow: none; 
                max-width: 100%;
                margin: 0;
                padding: 0;
            }
            .print-button { display: none; }
            .section { 
                page-break-inside: avoid; 
                margin-bottom: 5px;
            }
            table {
                font-size: 8px;
            }
            th, td {
                padding: 1px 2px;
            }
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
            html.AppendLine("        <div class='compact-info'>");
            
            // html.AppendLine($"            <span><strong>Payment ID:</strong> {data.PaymentID}</span>");
            html.AppendLine($"            <span><strong>Date:</strong> {data.PaymentDate:dd-MMM-yyyy}</span>");
            html.AppendLine($"            <span><strong>Method:</strong> {data.PaymentMethod}</span>");
            html.AppendLine($"            <span><strong>Amount:</strong> ₹{data.TotalPaymentAmount:N2}</span>");
            
            if (!string.IsNullOrEmpty(data.PartyName))
                html.AppendLine($"            <span><strong>Party:</strong> {data.PartyName}</span>");
            
            if (!string.IsNullOrEmpty(data.BrokerName))
                html.AppendLine($"            <span><strong>Broker:</strong> {data.BrokerName}</span>");
            
            html.AppendLine("        </div>");
            
            return html.ToString();
        }

        /// <summary>
        /// Generates settlement summary section
        /// </summary>
        private static string GenerateSettlementSummary(PaymentReportData data)
        {
            var html = new StringBuilder();
            html.AppendLine("        <div class='compact-info'>");
            
            // Calculate total original amount from bill details
            decimal totalOriginalAmount = 0;
            foreach (var bill in data.BillDetails)
            {
                totalOriginalAmount += bill.OriginalAmount;
            }
            
            html.AppendLine($"            <span><strong>Original Amount:</strong> ₹{totalOriginalAmount:N0}</span>");
            html.AppendLine($"            <span><strong>Amount Due:</strong> ₹{data.TotalAmountDue:N0}</span>");
            html.AppendLine($"            <span><strong>Payments Used:</strong> ₹{data.TotalAdvanceUsed:N0}</span>");
            html.AppendLine($"            <span><strong>Cash Needed:</strong> ₹{data.TotalCashNeeded:N0}</span>");
            html.AppendLine($"            <span><strong>Interest:</strong> ₹{data.TotalInterest:N0}</span>");
            html.AppendLine($"            <span><strong>Discount:</strong> ₹{data.TotalDiscount:N0}</span>");
            html.AppendLine($"            <span><strong>Brokerage:</strong> ₹{data.TotalBrokerage:N0}</span>");
            
            if (data.UnusedAdvance > 0)
                html.AppendLine($"            <span><strong>Unused Advance:</strong> ₹{data.UnusedAdvance:N0}</span>");
            
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
            html.AppendLine("                        <th>Party Name</th>");
            html.AppendLine("                        <th>Date</th>");
            html.AppendLine("                        <th>Original Amount</th>");
            html.AppendLine("                        <th>Balance Due</th>");
            html.AppendLine("                        <th>Amount Paid</th>");
            html.AppendLine("                        <th>Interest</th>");
            html.AppendLine("                        <th>Discount</th>");
            html.AppendLine("                        <th>Brokerage</th>");
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
                html.AppendLine($"                        <td>{bill.PartyName}</td>");
                html.AppendLine($"                        <td>{bill.BillDate:dd-MMM-yyyy}</td>");
                html.AppendLine($"                        <td class='amount'>₹{bill.OriginalAmount:N2}</td>");
                html.AppendLine($"                        <td class='amount'>₹{bill.BalanceDue:N2}</td>");
                html.AppendLine($"                        <td class='amount positive'>₹{bill.AmountPaid:N2}</td>");
                html.AppendLine($"                        <td class='amount negative'>₹{bill.InterestCharged:N2}</td>");
                html.AppendLine($"                        <td class='amount positive'>₹{bill.DiscountEarned:N2}</td>");
                html.AppendLine($"                        <td class='amount'>₹{bill.Brokerage:N2}</td>");
                html.AppendLine($"                        <td class='amount'>₹{bill.AdvanceUsed:N2}</td>");
                html.AppendLine($"                        <td class='amount'>₹{bill.CashUsed:N2}</td>");
                html.AppendLine($"                        <td><span class='status {bill.Status.ToLower()}'>{bill.Status}</span></td>");
                html.AppendLine("                    </tr>");
                
                // Add interest period details if any
                if (bill.InterestPeriods.Count > 0)
                {
                    html.AppendLine("                    <tr>");
                    html.AppendLine("                        <td colspan='12'>");
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
            
            // Calculate totals for all numeric columns
            decimal totalOriginalAmount = 0, totalBalanceDue = 0, totalAmountPaid = 0;
            decimal totalInterest = 0, totalDiscount = 0, totalBrokerage = 0;
            decimal totalAdvanceUsed = 0, totalCashUsed = 0;
            foreach (var bill in data.BillDetails)
            {
                totalOriginalAmount += bill.OriginalAmount;
                totalBalanceDue += bill.BalanceDue;
                totalAmountPaid += bill.AmountPaid;
                totalInterest += bill.InterestCharged;
                totalDiscount += bill.DiscountEarned;
                totalBrokerage += bill.Brokerage;
                totalAdvanceUsed += bill.AdvanceUsed;
                totalCashUsed += bill.CashUsed;
            }
            
            // Total row
            html.AppendLine("                    <tr class='total-row'>");
            html.AppendLine("                        <td colspan='3'><strong>Total</strong></td>");
            html.AppendLine($"                        <td class='amount'><strong>₹{totalOriginalAmount:N2}</strong></td>");
            html.AppendLine($"                        <td class='amount'><strong>₹{totalBalanceDue:N2}</strong></td>");
            html.AppendLine($"                        <td class='amount positive'><strong>₹{totalAmountPaid:N2}</strong></td>");
            html.AppendLine($"                        <td class='amount negative'><strong>₹{totalInterest:N2}</strong></td>");
            html.AppendLine($"                        <td class='amount positive'><strong>₹{totalDiscount:N2}</strong></td>");
            html.AppendLine($"                        <td class='amount'><strong>₹{totalBrokerage:N2}</strong></td>");
            html.AppendLine($"                        <td class='amount'><strong>₹{totalAdvanceUsed:N2}</strong></td>");
            html.AppendLine($"                        <td class='amount'><strong>₹{totalCashUsed:N2}</strong></td>");
            html.AppendLine("                        <td></td>");
            html.AppendLine("                    </tr>");
            
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
            html.AppendLine("            <h2 class='section-title'>Payments Utilizations</h2>");
            
            html.AppendLine("            <table>");
            html.AppendLine("                <thead>");
            html.AppendLine("                    <tr>");
            html.AppendLine("                        <th>Payment ID</th>");
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
                if (data.UnusedAdvanceReversals.Count > 0 && advance.RemainingAmount > 0)
                {
                    html.AppendLine($"                        <td><span class='status reverted'>Reverted</span></td>");
                }
                else
                {
                    html.AppendLine($"                        <td><span class='status {advance.Status.ToLower()}'>{advance.Status}</span></td>");
                }
                html.AppendLine($"                        <td>{string.Join(", ", advance.UsedForBills)}</td>");
                html.AppendLine("                    </tr>");
            }
            
            // Calculate totals for payment utilizations
            decimal totalPmtOriginal = 0, totalPmtUsed = 0, totalPmtRemaining = 0;
            foreach (var advance in data.AdvanceUtilizations)
            {
                totalPmtOriginal += advance.OriginalAmount;
                totalPmtUsed += advance.AmountUsed;
                totalPmtRemaining += advance.RemainingAmount;
            }
            
            // Total row
            html.AppendLine("                    <tr class='total-row'>");
            html.AppendLine("                        <td colspan='2'><strong>Total</strong></td>");
            html.AppendLine($"                        <td class='amount'><strong>₹{totalPmtOriginal:N2}</strong></td>");
            html.AppendLine($"                        <td class='amount positive'><strong>₹{totalPmtUsed:N2}</strong></td>");
            html.AppendLine($"                        <td class='amount'><strong>₹{totalPmtRemaining:N2}</strong></td>");
            html.AppendLine("                        <td colspan='3'></td>");
            html.AppendLine("                    </tr>");
            
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
            html.AppendLine("        <div class='compact-info'>");
            
            html.AppendLine($"            <span><strong>Interest:</strong> {data.PaymentTerms.InterestDays}d @ {data.PaymentTerms.InterestRate}%</span>");
            html.AppendLine($"            <span><strong>Discount:</strong> {data.PaymentTerms.DiscountDays}d @ {data.PaymentTerms.DiscountRate}%</span>");
            html.AppendLine($"            <span><strong>Brokerage:</strong> {data.PaymentTerms.BrokerageRate}%</span>");
            html.AppendLine($"            <span><strong>Terms:</strong> {data.PaymentTerms.TermsSource}</span>");
            
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
