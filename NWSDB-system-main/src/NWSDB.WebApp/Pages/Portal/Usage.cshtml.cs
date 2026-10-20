using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NWSDB.Core.DTOs;
using NWSDB.Services.Interfaces;

namespace NWSDB.WebApp.Pages.Portal;

public class UsageModel : PageModel
{
    private readonly IWaterUsageService _usageService;
    private readonly ICustomerService _customerService;

    public UsageModel(IWaterUsageService usageService, ICustomerService customerService)
    {
        _usageService = usageService;
        _customerService = customerService;
    }

    public List<WaterUsageDto> Usages { get; set; } = new();
    public CustomerDto? Customer { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var accountNo = HttpContext.Session.GetString("AccountNumber") ?? "ACC10001";
        Customer = await _customerService.GetByAccountNumberAsync(accountNo);
        Usages = await _usageService.GetUsageHistoryAsync(accountNo);
        return Page();
    }
}
