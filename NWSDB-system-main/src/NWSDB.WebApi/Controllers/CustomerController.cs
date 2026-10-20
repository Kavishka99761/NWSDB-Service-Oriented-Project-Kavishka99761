using Microsoft.AspNetCore.Mvc;
using NWSDB.Core.DTOs;
using NWSDB.Services.Interfaces;

namespace NWSDB.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet("{accountNumber}")]
    public async Task<IActionResult> GetByAccount(string accountNumber)
    {
        var cust = await _customerService.GetByAccountNumberAsync(accountNumber);
        return cust != null ? Ok(ApiResponse<CustomerDto>.Ok(cust)) : NotFound(ApiResponse<CustomerDto>.Fail("Customer not found."));
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var list = await _customerService.GetAllCustomersAsync();
        return Ok(ApiResponse<List<CustomerDto>>.Ok(list));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CustomerDto dto)
    {
        var res = await _customerService.CreateCustomerAsync(dto);
        return res.Success ? Ok(res) : BadRequest(res);
    }

    [HttpPut("{accountNumber}")]
    public async Task<IActionResult> Update(string accountNumber, [FromBody] CustomerDto dto)
    {
        var res = await _customerService.UpdateCustomerAsync(accountNumber, dto);
        return res.Success ? Ok(res) : BadRequest(res);
    }

    [HttpGet("{accountNumber}/balance")]
    public async Task<IActionResult> GetBalance(string accountNumber)
    {
        var balance = await _customerService.GetOutstandingBalanceAsync(accountNumber);
        return Ok(ApiResponse<decimal>.Ok(balance));
    }
}
