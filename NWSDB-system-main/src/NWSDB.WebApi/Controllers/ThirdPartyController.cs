using Microsoft.AspNetCore.Mvc;
using NWSDB.Core.DTOs;
using NWSDB.Services.Interfaces;

namespace NWSDB.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ThirdPartyController : ControllerBase
{
    private readonly IThirdPartyPartnerService _partnerService;

    public ThirdPartyController(IThirdPartyPartnerService partnerService)
    {
        _partnerService = partnerService;
    }

    [HttpPost("pay")]
    public async Task<IActionResult> PartnerPayment(
        [FromHeader(Name = "X-API-KEY")] string? apiKeyHeader,
        [FromQuery] string? apiKey,
        [FromBody] ThirdPartyPaymentRequest request)
    {
        string key = !string.IsNullOrWhiteSpace(apiKeyHeader) ? apiKeyHeader : (apiKey ?? string.Empty);
        if (string.IsNullOrWhiteSpace(key))
        {
            return Unauthorized(ApiResponse<ThirdPartyPaymentResponse>.Fail("X-API-KEY header or apiKey query parameter is missing."));
        }

        var res = await _partnerService.ProcessThirdPartyPaymentAsync(key, request);
        return res.Success ? Ok(res) : BadRequest(res);
    }

    [HttpGet("partners")]
    public async Task<IActionResult> GetAllPartners()
    {
        var list = await _partnerService.GetAllPartnersAsync();
        return Ok(ApiResponse<List<ThirdPartyPartnerDto>>.Ok(list));
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterPartner([FromBody] RegisterPartnerRequest request)
    {
        var res = await _partnerService.RegisterPartnerAsync(request);
        return res.Success ? Ok(res) : BadRequest(res);
    }

    [HttpPut("{partnerId:int}/toggle")]
    public async Task<IActionResult> ToggleStatus(int partnerId)
    {
        var res = await _partnerService.TogglePartnerStatusAsync(partnerId);
        return res.Success ? Ok(res) : BadRequest(res);
    }
}
