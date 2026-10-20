using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using NWSDB.Core.Enums;
using NWSDB.Data;
using NWSDB.Services.Interfaces;

namespace NWSDB.Services.Implementations;

public class ReportService : IReportService
{
    private readonly NwsdbDbContext _context;

    public ReportService(NwsdbDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> GeneratePaymentReportExcelAsync()
    {
        var payments = await _context.Payments
            .Include(p => p.Customer)
            .Include(p => p.ThirdPartyPartner)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Payment Records");

        // Set Title & Branding
        ws.Cell(1, 1).Value = "NATIONAL WATER SUPPLY AND DRAINAGE BOARD (NWSDB)";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 16;
        ws.Cell(1, 1).Style.Font.FontColor = XLColor.FromArgb(0, 51, 102);

        ws.Cell(2, 1).Value = $"Official Payment Transactions Report | Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        ws.Cell(2, 1).Style.Font.Italic = true;
        ws.Cell(2, 1).Style.Font.FontSize = 10;
        ws.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;

        // Table Headers
        string[] headers =
        {
            "No", "Payment Reference", "Account No", "Customer Name",
            "Payment Date", "Payment Method", "Account/Card", "Amount (Rs.)",
            "Status", "Channel / Partner"
        };

        int headerRow = 4;
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(headerRow, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 51, 102);
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Data Rows
        int row = 5;
        decimal totalAmount = 0m;
        int index = 1;

        foreach (var p in payments)
        {
            ws.Cell(row, 1).Value = index++;
            ws.Cell(row, 2).Value = p.PaymentReference;
            ws.Cell(row, 3).Value = p.AccountNumber;
            ws.Cell(row, 4).Value = p.Customer?.FullName ?? "N/A";
            ws.Cell(row, 5).Value = p.PaymentDate.ToString("yyyy-MM-dd HH:mm");
            ws.Cell(row, 6).Value = p.Method.ToString();
            ws.Cell(row, 7).Value = p.MaskedCardOrPhone;

            var amountCell = ws.Cell(row, 8);
            amountCell.Value = p.Amount;
            amountCell.Style.NumberFormat.Format = "#,##0.00";
            amountCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            var statusCell = ws.Cell(row, 9);
            statusCell.Value = p.Status.ToString();
            statusCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            if (p.Status == PaymentStatus.Success)
            {
                statusCell.Style.Font.FontColor = XLColor.DarkGreen;
                statusCell.Style.Font.Bold = true;
            }

            ws.Cell(row, 10).Value = p.ThirdPartyPartner?.PartnerName ?? "Direct Web Portal";

            if (row % 2 == 0)
            {
                ws.Range(row, 1, row, headers.Length).Style.Fill.BackgroundColor = XLColor.FromArgb(240, 245, 250);
            }

            totalAmount += p.Amount;
            row++;
        }

        // Total Row
        ws.Cell(row, 7).Value = "TOTAL COLLECTIONS:";
        ws.Cell(row, 7).Style.Font.Bold = true;
        ws.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

        var totalCell = ws.Cell(row, 8);
        totalCell.Value = totalAmount;
        totalCell.Style.Font.Bold = true;
        totalCell.Style.NumberFormat.Format = "#,##0.00";
        totalCell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        totalCell.Style.Border.BottomBorder = XLBorderStyleValues.Double;

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> GenerateBillingSummaryExcelAsync()
    {
        var bills = await _context.Bills
            .Include(b => b.Customer)
            .OrderByDescending(b => b.MonthYear)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Billing Summary");

        ws.Cell(1, 1).Value = "NATIONAL WATER SUPPLY AND DRAINAGE BOARD (NWSDB)";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 16;
        ws.Cell(1, 1).Style.Font.FontColor = XLColor.FromArgb(0, 51, 102);

        ws.Cell(2, 1).Value = $"Customer Billing Ledger | Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        ws.Cell(2, 1).Style.Font.Italic = true;
        ws.Cell(2, 1).Style.Font.FontSize = 10;
        ws.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;

        string[] headers =
        {
            "Bill ID", "Account No", "Customer Name", "Month/Year",
            "Units", "Water Charge (Rs.)", "Fixed Charge (Rs.)", "VAT (Rs.)",
            "Total Amount (Rs.)", "Due Date", "Status"
        };

        int headerRow = 4;
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(headerRow, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 102, 153);
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        int row = 5;
        decimal sumTotal = 0m;
        foreach (var b in bills)
        {
            ws.Cell(row, 1).Value = b.Id;
            ws.Cell(row, 2).Value = b.AccountNumber;
            ws.Cell(row, 3).Value = b.Customer?.FullName ?? "";
            ws.Cell(row, 4).Value = b.MonthYear;
            ws.Cell(row, 5).Value = b.UnitsConsumed;
            ws.Cell(row, 6).Value = b.WaterCharge;
            ws.Cell(row, 7).Value = b.FixedServiceCharge;
            ws.Cell(row, 8).Value = b.VatAmount;

            var totCell = ws.Cell(row, 9);
            totCell.Value = b.TotalAmount;
            totCell.Style.NumberFormat.Format = "#,##0.00";

            ws.Cell(row, 10).Value = b.DueDate.ToString("yyyy-MM-dd");

            var stCell = ws.Cell(row, 11);
            stCell.Value = b.Status.ToString();
            if (b.Status == BillStatus.Paid)
            {
                stCell.Style.Font.FontColor = XLColor.DarkGreen;
            }
            else
            {
                stCell.Style.Font.FontColor = XLColor.Crimson;
                stCell.Style.Font.Bold = true;
            }

            if (row % 2 == 0)
            {
                ws.Range(row, 1, row, headers.Length).Style.Fill.BackgroundColor = XLColor.FromArgb(245, 250, 255);
            }

            sumTotal += b.TotalAmount;
            row++;
        }

        ws.Cell(row, 8).Value = "TOTAL BILLED:";
        ws.Cell(row, 8).Style.Font.Bold = true;
        var summaryCell = ws.Cell(row, 9);
        summaryCell.Value = sumTotal;
        summaryCell.Style.Font.Bold = true;
        summaryCell.Style.NumberFormat.Format = "#,##0.00";
        summaryCell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        summaryCell.Style.Border.BottomBorder = XLBorderStyleValues.Double;

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> GenerateCustomerUsageExcelAsync(string accountNumber)
    {
        var usages = await _context.WaterUsages
            .Where(u => u.AccountNumber.ToUpper() == accountNumber.Trim().ToUpper())
            .OrderByDescending(u => u.MonthYear)
            .ToListAsync();

        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.AccountNumber == accountNumber.Trim().ToUpper());

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Usage History");

        ws.Cell(1, 1).Value = $"NWSDB - Water Consumption Ledger for {accountNumber}";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;

        ws.Cell(2, 1).Value = $"Customer: {customer?.FullName} | Meter: {customer?.MeterNumber}";
        ws.Cell(2, 1).Style.Font.Italic = true;

        string[] headers = { "Month/Year", "Previous Reading", "Current Reading", "Units Consumed", "Reading Date", "Officer", "Remarks" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(4, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(30, 80, 130);
            cell.Style.Font.FontColor = XLColor.White;
        }

        int row = 5;
        foreach (var u in usages)
        {
            ws.Cell(row, 1).Value = u.MonthYear;
            ws.Cell(row, 2).Value = u.PreviousReading;
            ws.Cell(row, 3).Value = u.CurrentReading;
            ws.Cell(row, 4).Value = u.UnitsConsumed;
            ws.Cell(row, 5).Value = u.ReadingDate.ToString("yyyy-MM-dd");
            ws.Cell(row, 6).Value = u.MeterReaderName;
            ws.Cell(row, 7).Value = u.Remarks;
            row++;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}
