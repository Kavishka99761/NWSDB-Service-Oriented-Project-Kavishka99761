using Microsoft.EntityFrameworkCore;
using NWSDB.Core.DTOs;
using NWSDB.Core.Entities;
using NWSDB.Core.Enums;
using NWSDB.Data;
using NWSDB.Services.Interfaces;

namespace NWSDB.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly NwsdbDbContext _context;
    private readonly ITransactionAuditService _auditService;

    public PaymentService(NwsdbDbContext context, ITransactionAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<ApiResponse<PaymentReceiptDto>> MakePaymentAsync(MakePaymentRequest request, string performedBy = "Customer")
    {
        // 1. Validation Phase (<<Include>> Validate Payment)
        if (string.IsNullOrWhiteSpace(request.AccountNumber))
        {
            return ApiResponse<PaymentReceiptDto>.Fail("Customer account number is required.");
        }

        if (request.Amount <= 0)
        {
            return ApiResponse<PaymentReceiptDto>.Fail("Payment amount must be greater than zero.");
        }

        var customer = await _context.Customers
            .Include(c => c.Bills)
            .FirstOrDefaultAsync(c => c.AccountNumber == request.AccountNumber.Trim().ToUpper());

        if (customer == null)
        {
            return ApiResponse<PaymentReceiptDto>.Fail($"Account '{request.AccountNumber}' not found.");
        }

        // Validate Card Details if payment method is Credit/Debit Card
        string maskedNumber;
        if (request.Method == PaymentMethod.CreditCard || request.Method == PaymentMethod.DebitCard)
        {
            string cleanCard = request.CardOrAccountNumber.Replace(" ", "").Replace("-", "");
            if (string.IsNullOrWhiteSpace(cleanCard) || cleanCard.Length < 12)
            {
                return ApiResponse<PaymentReceiptDto>.Fail("Invalid Card Number. Must be at least 12 digits.");
            }
            maskedNumber = $"**** **** **** {cleanCard[^4..]}";
        }
        else
        {
            maskedNumber = string.IsNullOrWhiteSpace(request.CardOrAccountNumber)
                ? "N/A"
                : $"ACC-****-{request.CardOrAccountNumber[^Math.Min(4, request.CardOrAccountNumber.Length)..]}";
        }

        // 2. Identify Bill to Settle
        Bill? targetBill = null;
        if (request.BillId.HasValue && request.BillId.Value > 0)
        {
            targetBill = await _context.Bills.FirstOrDefaultAsync(b => b.Id == request.BillId.Value && b.AccountNumber == customer.AccountNumber);
        }

        if (targetBill == null)
        {
            // Pick earliest pending bill
            targetBill = await _context.Bills
                .Where(b => b.AccountNumber == customer.AccountNumber && b.Status != BillStatus.Paid)
                .OrderBy(b => b.DueDate)
                .FirstOrDefaultAsync();
        }

        // 3. Record Payment (<<Include>> Record Payment)
        string refId = $"NWSDB-PAY-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";

        var payment = new Payment
        {
            PaymentReference = refId,
            BillId = targetBill?.Id,
            AccountNumber = customer.AccountNumber,
            Amount = request.Amount,
            PaymentDate = DateTime.UtcNow,
            Method = request.Method,
            Status = PaymentStatus.Success,
            MaskedCardOrPhone = maskedNumber,
            TransactionNotes = string.IsNullOrWhiteSpace(request.Notes)
                ? $"Online Payment of Rs. {request.Amount:F2} for {customer.AccountNumber}"
                : request.Notes
        };

        _context.Payments.Add(payment);

        // Update target bill status if paid in full
        if (targetBill != null && request.Amount >= targetBill.TotalAmount)
        {
            targetBill.Status = BillStatus.Paid;
            targetBill.PaidDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // 4. Audit Trail
        await _auditService.LogAuditAsync(
            "BillPayment",
            performedBy,
            customer.AccountNumber,
            refId,
            $"Payment of Rs. {request.Amount:F2} processed via {request.Method}."
        );

        // 5. Remaining Balance Calculation
        decimal remaining = await _context.Bills
            .Where(b => b.AccountNumber == customer.AccountNumber && b.Status != BillStatus.Paid)
            .SumAsync(b => b.TotalAmount);

        // 6. Confirmation Receipt
        var receipt = new PaymentReceiptDto
        {
            PaymentReference = payment.PaymentReference,
            PaymentId = payment.Id,
            BillId = payment.BillId,
            AccountNumber = customer.AccountNumber,
            CustomerName = customer.FullName,
            AmountPaid = payment.Amount,
            PaymentDate = payment.PaymentDate,
            PaymentMethod = payment.Method.ToString(),
            Status = "SUCCESS",
            MaskedCardOrPhone = payment.MaskedCardOrPhone,
            RemainingBalance = remaining,
            Message = "Payment successfully verified and processed by NWSDB billing gateway."
        };

        return ApiResponse<PaymentReceiptDto>.Ok(receipt, "Payment successfully processed.");
    }

    public async Task<List<PaymentHistoryDto>> GetPaymentHistoryAsync(string accountNumber)
    {
        var payments = await _context.Payments
            .Include(p => p.ThirdPartyPartner)
            .Where(p => p.AccountNumber.ToUpper() == accountNumber.Trim().ToUpper())
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        return payments.Select(p => MapToHistoryDto(p)).ToList();
    }

    public async Task<List<PaymentHistoryDto>> GetAllPaymentsAsync()
    {
        var payments = await _context.Payments
            .Include(p => p.ThirdPartyPartner)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        return payments.Select(p => MapToHistoryDto(p)).ToList();
    }

    public async Task<PaymentReceiptDto?> GetReceiptAsync(string paymentReference)
    {
        var p = await _context.Payments
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.PaymentReference == paymentReference.Trim());

        if (p == null) return null;

        decimal remaining = await _context.Bills
            .Where(b => b.AccountNumber == p.AccountNumber && b.Status != BillStatus.Paid)
            .SumAsync(b => b.TotalAmount);

        return new PaymentReceiptDto
        {
            PaymentReference = p.PaymentReference,
            PaymentId = p.Id,
            BillId = p.BillId,
            AccountNumber = p.AccountNumber,
            CustomerName = p.Customer?.FullName ?? "Valued Customer",
            AmountPaid = p.Amount,
            PaymentDate = p.PaymentDate,
            PaymentMethod = p.Method.ToString(),
            Status = p.Status.ToString().ToUpper(),
            MaskedCardOrPhone = p.MaskedCardOrPhone,
            RemainingBalance = remaining,
            Message = "Official NWSDB Electronic Payment Confirmation."
        };
    }

    private static PaymentHistoryDto MapToHistoryDto(Payment p) => new()
    {
        Id = p.Id,
        PaymentReference = p.PaymentReference,
        BillId = p.BillId,
        AccountNumber = p.AccountNumber,
        Amount = p.Amount,
        PaymentDate = p.PaymentDate,
        Method = p.Method.ToString(),
        Status = p.Status.ToString(),
        MaskedAccount = p.MaskedCardOrPhone,
        PartnerName = p.ThirdPartyPartner?.PartnerName
    };
}
