using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NWSDB.Core.DTOs;
using NWSDB.Services.Interfaces;

namespace NWSDB.WebApp.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IAuthService _authService;

    public LoginModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public void OnGet(string? demoUser)
    {
        if (demoUser == "admin")
        {
            Username = "admin";
            Password = "admin123";
        }
        else if (demoUser == "customer")
        {
            Username = "sunil";
            Password = "sunil123";
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _authService.LoginAsync(new LoginRequest
        {
            Username = Username,
            Password = Password
        });

        if (!result.Success || result.Data == null)
        {
            ErrorMessage = result.Message;
            return Page();
        }

        // Store login in session
        HttpContext.Session.SetString("Username", result.Data.Username);
        HttpContext.Session.SetString("FullName", result.Data.FullName);
        HttpContext.Session.SetString("Role", result.Data.Role);
        if (!string.IsNullOrEmpty(result.Data.AccountNumber))
        {
            HttpContext.Session.SetString("AccountNumber", result.Data.AccountNumber);
        }

        if (result.Data.Role == "Administrator")
        {
            return RedirectToPage("/Index");
        }

        return RedirectToPage("/Portal/Dashboard");
    }
}
