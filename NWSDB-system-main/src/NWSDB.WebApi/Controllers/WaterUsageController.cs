using Microsoft.AspNetCore.Mvc;
using NWSDB.Core.DTOs;
using NWSDB.Services.Interfaces;

namespace NWSDB.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WaterUsageController : ControllerBase
{
    private readonly IWaterUsageService _waterUsageService;

    public WaterUsageController(IWaterUsageService waterUsageService)
    {
        _waterUsageService = waterUsageService;
    }

    [HttpGet("{accountNumber}")]
    public async Task<IActionResult> GetHistory(string accountNumber)
    {
        var list = await _waterUsageService.GetUsageHistoryAsync(accountNumber);
        return Ok(ApiResponse<List<WaterUsageDto>>.Ok(list));
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var list = await _waterUsageService.GetAllUsagesAsync();
        return Ok(ApiResponse<List<WaterUsageDto>>.Ok(list));
    }

    [HttpGet("{accountNumber}/latest")]
    public async Task<IActionResult> GetLatest(string accountNumber)
    {
        var latest = await _waterUsageService.GetLatestUsageAsync(accountNumber);
        return latest != null ? Ok(ApiResponse<WaterUsageDto>.Ok(latest)) : NotFound(ApiResponse<WaterUsageDto>.Fail("No usage recorded yet."));
    }

    [HttpPost("record")]
    public async Task<IActionResult> Record([FromBody] RecordUsageRequest request)
    {
        var res = await _waterUsageService.RecordMeterReadingAsync(request);
        return res.Success ? Ok(res) : BadRequest(res);
    }
}
