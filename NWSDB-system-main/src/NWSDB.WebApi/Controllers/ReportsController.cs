using Microsoft.AspNetCore.Mvc;
using NWSDB.Services.Interfaces;

namespace NWSDB.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("payments/excel")]
    public async Task<IActionResult> DownloadPaymentsExcel()
    {
        var bytes = await _reportService.GeneratePaymentReportExcelAsync();
        string filename = $"NWSDB_Payment_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
    }

    [HttpGet("billing/excel")]
    public async Task<IActionResult> DownloadBillingExcel()
    {
        var bytes = await _reportService.GenerateBillingSummaryExcelAsync();
        string filename = $"NWSDB_Billing_Summary_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
    }

    [HttpGet("usage/{accountNumber}/excel")]
    public async Task<IActionResult> DownloadUsageExcel(string accountNumber)
    {
        var bytes = await _reportService.GenerateCustomerUsageExcelAsync(accountNumber);
        string filename = $"NWSDB_Usage_{accountNumber}_{DateTime.Now:yyyyMMdd}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
    }
}
