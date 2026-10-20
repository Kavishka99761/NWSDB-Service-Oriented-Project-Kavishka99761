using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NWSDB.Core.DTOs;
using NWSDB.Core.Enums;
using NWSDB.Services.Interfaces;

namespace NWSDB.WebApp.Pages;

public class PaymentModel : PageModel
{
    private readonly ICustomerService _customerService;
    private readonly IBillingService _billingService;
    private readonly IPaymentService _paymentService;

    public PaymentModel(
        ICustomerService customerService,
        IBillingService billingService,
        IPaymentService paymentService)
    {
        _customerService = customerService;
        _billingService = billingService;
        _paymentService = paymentService;
    }

    [BindProperty(SupportsGet = true)]
    public string AccountNumber { get; set; } = string.Empty;

    public CustomerDto? Customer { get; set; }
    public List<BillDto> UnpaidBills { get; set; } = new();
    public decimal TotalOutstanding { get; set; }

    [BindProperty]
    public int? SelectedBillId { get; set; }

    [BindProperty]
    public decimal PaymentAmount { get; set; }

    [BindProperty]
    public PaymentMethod Method { get; set; } = PaymentMethod.CreditCard;

    [BindProperty]
    public string CardOrAccountNumber { get; set; } = string.Empty;

    [BindProperty]
    public string CardExpiry { get; set; } = string.Empty;

    [BindProperty]
    public string CardCvv { get; set; } = string.Empty;

    [BindProperty]
    public string Notes { get; set; } = string.Empty;

    public PaymentReceiptDto? Receipt { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        if (!string.IsNullOrWhiteSpace(AccountNumber))
        {
            await LoadCustomerBillingInfoAsync(AccountNumber);
        }
    }

    public async Task<IActionResult> OnPostLookupAsync()
    {
        if (string.IsNullOrWhiteSpace(AccountNumber))
        {
            ErrorMessage = "Please enter an account number to search.";
            return Page();
        }

        await LoadCustomerBillingInfoAsync(AccountNumber);
        if (Customer == null)
        {
            ErrorMessage = $"Account '{AccountNumber}' not found in NWSDB records. Please check the account number.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostPayAsync()
    {
        if (string.IsNullOrWhiteSpace(AccountNumber))
        {
            ErrorMessage = "Account number is required.";
            return Page();
        }

        await LoadCustomerBillingInfoAsync(AccountNumber);
        if (Customer == null)
        {
            ErrorMessage = $"Account '{AccountNumber}' not found.";
            return Page();
        }

        if (PaymentAmount <= 0)
        {
            ErrorMessage = "Please enter a valid payment amount greater than zero.";
            return Page();
        }

        var request = new MakePaymentRequest
        {
            AccountNumber = AccountNumber.Trim(),
            BillId = SelectedBillId,
            Amount = PaymentAmount,
            Method = Method,
            CardOrAccountNumber = CardOrAccountNumber,
            CardExpiry = CardExpiry,
            CardCvv = CardCvv,
            Notes = Notes
        };

        var result = await _paymentService.MakePaymentAsync(request, "NWSDB_WebPortal");
        if (!result.Success)
        {
            ErrorMessage = result.Message;
            return Page();
        }

        Receipt = result.Data;
        // Reload customer info with updated balances
        await LoadCustomerBillingInfoAsync(AccountNumber);
        return Page();
    }

    private async Task LoadCustomerBillingInfoAsync(string accNo)
    {
        Customer = await _customerService.GetByAccountNumberAsync(accNo);
        if (Customer != null)
        {
            var bills = await _billingService.GetBillsByAccountAsync(accNo);
            UnpaidBills = bills.Where(b => b.Status != BillStatus.Paid).ToList();
            TotalOutstanding = UnpaidBills.Sum(b => b.TotalAmount);
            if (PaymentAmount == 0 && TotalOutstanding > 0)
            {
                PaymentAmount = TotalOutstanding;
            }
        }
    }
}
