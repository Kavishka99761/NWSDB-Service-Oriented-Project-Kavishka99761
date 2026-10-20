using Microsoft.AspNetCore.Mvc;
using NWSDB.Core.DTOs;
using NWSDB.Services.Interfaces;

namespace NWSDB.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MonitoringController : ControllerBase
{
    private readonly ITransactionAuditService _auditService;

    public MonitoringController(ITransactionAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet("audits")]
    public async Task<IActionResult> GetAudits([FromQuery] int count = 50)
    {
        var logs = await _auditService.GetRecentAuditsAsync(count);
        return Ok(ApiResponse<object>.Ok(logs));
    }

    [HttpGet("admin-dashboard")]
    public async Task<IActionResult> GetAdminDashboard()
    {
        var data = await _auditService.GetAdminDashboardSummaryAsync();
        return Ok(ApiResponse<AdminDashboardDto>.Ok(data));
    }

    [HttpGet("customer-dashboard/{accountNumber}")]
    public async Task<IActionResult> GetCustomerDashboard(string accountNumber)
    {
        var data = await _auditService.GetCustomerDashboardSummaryAsync(accountNumber);
        return data != null ? Ok(ApiResponse<CustomerDashboardDto>.Ok(data)) : NotFound(ApiResponse<CustomerDashboardDto>.Fail("Customer not found."));
    }
}
