using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NWSDB.Core.DTOs;
using NWSDB.Services.Interfaces;

namespace NWSDB.WebApp.Pages.Portal;

public class BillsModel : PageModel
{
    private readonly IBillingService _billingService;
    private readonly ICustomerService _customerService;

    public BillsModel(IBillingService billingService, ICustomerService customerService)
    {
        _billingService = billingService;
        _customerService = customerService;
    }

    public List<BillDto> Bills { get; set; } = new();
    public CustomerDto? Customer { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var accountNo = HttpContext.Session.GetString("AccountNumber") ?? "ACC10001";
        Customer = await _customerService.GetByAccountNumberAsync(accountNo);
        Bills = await _billingService.GetBillsByAccountAsync(accountNo);
        return Page();
    }
}
