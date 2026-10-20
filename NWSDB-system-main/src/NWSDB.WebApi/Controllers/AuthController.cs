using Microsoft.AspNetCore.Mvc;
using NWSDB.Core.DTOs;
using NWSDB.Services.Interfaces;

namespace NWSDB.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCustomerRequest request)
    {
        var result = await _authService.RegisterCustomerAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
