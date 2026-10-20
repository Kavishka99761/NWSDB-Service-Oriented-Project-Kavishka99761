using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NWSDB.Core.DTOs;
using NWSDB.Services.Interfaces;

namespace NWSDB.WebApp.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IAuthService _authService;

    public RegisterModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public RegisterCustomerRequest Input { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _authService.RegisterCustomerAsync(Input);
        if (!result.Success || result.Data == null)
        {
            ErrorMessage = result.Message;
            return Page();
        }

        // Set session
        HttpContext.Session.SetString("Username", result.Data.Username);
        HttpContext.Session.SetString("FullName", result.Data.FullName);
        HttpContext.Session.SetString("Role", result.Data.Role);
        if (!string.IsNullOrEmpty(result.Data.AccountNumber))
        {
            HttpContext.Session.SetString("AccountNumber", result.Data.AccountNumber);
        }

        return RedirectToPage("/Portal/Dashboard");
    }
}
