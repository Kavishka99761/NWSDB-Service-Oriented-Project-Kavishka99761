using Microsoft.AspNetCore.Mvc;
using NWSDB.Core.DTOs;
using NWSDB.Services.Interfaces;

namespace NWSDB.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("pay")]
    public async Task<IActionResult> MakePayment([FromBody] MakePaymentRequest request)
    {
        var res = await _paymentService.MakePaymentAsync(request, "OnlinePortal");
        return res.Success ? Ok(res) : BadRequest(res);
    }

    [HttpGet("history/{accountNumber}")]
    public async Task<IActionResult> GetHistory(string accountNumber)
    {
        var list = await _paymentService.GetPaymentHistoryAsync(accountNumber);
        return Ok(ApiResponse<List<PaymentHistoryDto>>.Ok(list));
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var list = await _paymentService.GetAllPaymentsAsync();
        return Ok(ApiResponse<List<PaymentHistoryDto>>.Ok(list));
    }

    [HttpGet("receipt/{reference}")]
    public async Task<IActionResult> GetReceipt(string reference)
    {
        var receipt = await _paymentService.GetReceiptAsync(reference);
        return receipt != null ? Ok(ApiResponse<PaymentReceiptDto>.Ok(receipt)) : NotFound(ApiResponse<PaymentReceiptDto>.Fail("Receipt not found."));
    }
}
