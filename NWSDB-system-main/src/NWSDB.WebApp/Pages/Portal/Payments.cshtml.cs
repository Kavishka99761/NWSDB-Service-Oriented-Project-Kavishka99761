using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NWSDB.Core.DTOs;
using NWSDB.Services.Interfaces;

namespace NWSDB.WebApp.Pages.Portal;

public class PaymentsModel : PageModel
{
    private readonly IPaymentService _paymentService;
    private readonly ICustomerService _customerService;

    public PaymentsModel(IPaymentService paymentService, ICustomerService customerService)
    {
        _paymentService = paymentService;
        _customerService = customerService;
    }

    public List<PaymentHistoryDto> Payments { get; set; } = new();
    public CustomerDto? Customer { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var accountNo = HttpContext.Session.GetString("AccountNumber") ?? "ACC10001";
        Customer = await _customerService.GetByAccountNumberAsync(accountNo);
        Payments = await _paymentService.GetPaymentHistoryAsync(accountNo);
        return Page();
    }
}
