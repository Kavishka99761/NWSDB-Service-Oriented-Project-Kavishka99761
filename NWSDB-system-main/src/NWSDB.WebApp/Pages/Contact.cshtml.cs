using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NWSDB.WebApp.Pages;

public class ContactModel : PageModel
{
    [BindProperty]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    public string AccountNumber { get; set; } = string.Empty;

    [BindProperty]
    public string ContactPhone { get; set; } = string.Empty;

    [BindProperty]
    public string Subject { get; set; } = string.Empty;

    [BindProperty]
    public string Message { get; set; } = string.Empty;

    public string? FeedbackMessage { get; set; }

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        FeedbackMessage = "Thank you! Your inquiry / water service report has been registered. Reference: TKT-" + new Random().Next(10000, 99999) + ". An officer will contact you shortly.";
        return Page();
    }
}
