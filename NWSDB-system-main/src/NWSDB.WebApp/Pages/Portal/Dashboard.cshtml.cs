using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NWSDB.Core.DTOs;
using NWSDB.Services.Interfaces;

namespace NWSDB.WebApp.Pages.Portal;

public class DashboardModel : PageModel
{
    private readonly ITransactionAuditService _auditService;

    public DashboardModel(ITransactionAuditService auditService)
    {
        _auditService = auditService;
    }

    public CustomerDashboardDto? DashboardData { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var accountNo = HttpContext.Session.GetString("AccountNumber");
        if (string.IsNullOrEmpty(accountNo))
        {
            // Fallback to demo account ACC10001 if testing without login
            accountNo = "ACC10001";
            HttpContext.Session.SetString("AccountNumber", accountNo);
            HttpContext.Session.SetString("Username", "sunil");
            HttpContext.Session.SetString("FullName", "Sunil Perera");
            HttpContext.Session.SetString("Role", "Customer");
        }

        DashboardData = await _auditService.GetCustomerDashboardSummaryAsync(accountNo);
        if (DashboardData == null)
        {
            return RedirectToPage("/Account/Login");
        }

        return Page();
    }
}
