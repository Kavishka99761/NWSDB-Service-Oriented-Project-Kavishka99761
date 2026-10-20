using Microsoft.AspNetCore.Mvc;
using NWSDB.Core.DTOs;
using NWSDB.Services.Interfaces;

namespace NWSDB.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BillsController : ControllerBase
{
    private readonly IBillingService _billingService;

    public BillsController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpGet("account/{accountNumber}")]
    public async Task<IActionResult> GetByAccount(string accountNumber)
    {
        var list = await _billingService.GetBillsByAccountAsync(accountNumber);
        return Ok(ApiResponse<List<BillDto>>.Ok(list));
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var list = await _billingService.GetPendingBillsAsync();
        return Ok(ApiResponse<List<BillDto>>.Ok(list));
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var list = await _billingService.GetAllBillsAsync();
        return Ok(ApiResponse<List<BillDto>>.Ok(list));
    }

    [HttpGet("{billId:int}")]
    public async Task<IActionResult> GetById(int billId)
    {
        var bill = await _billingService.GetBillByIdAsync(billId);
        return bill != null ? Ok(ApiResponse<BillDto>.Ok(bill)) : NotFound(ApiResponse<BillDto>.Fail("Bill not found."));
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateBillRequest request)
    {
        var res = await _billingService.GenerateBillForAccountAsync(request);
        return res.Success ? Ok(res) : BadRequest(res);
    }

    [HttpPost("batch-generate")]
    public async Task<IActionResult> BatchGenerate([FromQuery] string? monthYear)
    {
        var res = await _billingService.GenerateMonthlyBillsForAllAsync(monthYear ?? DateTime.UtcNow.ToString("yyyy-MM"));
        return res.Success ? Ok(res) : BadRequest(res);
    }
}
