using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NWSDB.Core.Logic;
using NWSDB.Services.Interfaces;

namespace NWSDB.WebApp.Pages;

public class IndexModel : PageModel
{
    private readonly ICustomerService _customerService;

    public IndexModel(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [BindProperty]
    public decimal CalculatorUnits { get; set; } = 15;

    public TariffCalculationResult? CalculationResult { get; set; }

    public void OnGet()
    {
        CalculationResult = NwsdbTariffCalculator.CalculateDomesticBill(CalculatorUnits);
    }

    public IActionResult OnPostCalculate()
    {
        if (CalculatorUnits < 0) CalculatorUnits = 0;
        CalculationResult = NwsdbTariffCalculator.CalculateDomesticBill(CalculatorUnits);
        return Page();
    }
}
