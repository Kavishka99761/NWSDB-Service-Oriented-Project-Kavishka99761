using Microsoft.EntityFrameworkCore;
using NWSDB.Core.DTOs;
using NWSDB.Core.Entities;
using NWSDB.Core.Enums;
using NWSDB.Data;
using NWSDB.Services.Interfaces;

namespace NWSDB.Services.Implementations;

public class ThirdPartyPartnerService : IThirdPartyPartnerService
{
    private readonly NwsdbDbContext _context;
    private readonly ITransactionAuditService _auditService;

    public ThirdPartyPartnerService(NwsdbDbContext context, ITransactionAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<ThirdPartyPartner?> ValidateApiKeyAsync(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey)) return null;

        return await _context.ThirdPartyPartners
            .FirstOrDefaultAsync(p => p.ApiKey == apiKey.Trim() && p.IsActive);
    }

    public async Task<ApiResponse<ThirdPartyPaymentResponse>> ProcessThirdPartyPaymentAsync(string apiKey, ThirdPartyPaymentRequest request)
    {
        var partner = await ValidateApiKeyAsync(apiKey);
        if (partner == null)
        {
            return ApiResponse<ThirdPartyPaymentResponse>.Fail("Unauthorized: Invalid or inactive Partner API Key.");
        }

        if (string.IsNullOrWhiteSpace(request.AccountNumber))
        {
            return ApiResponse<ThirdPartyPaymentResponse>.Fail("Account number is required.");
        }

        if (request.Amount <= 0)
        {
            return ApiResponse<ThirdPartyPaymentResponse>.Fail("Payment amount must be greater than zero.");
        }

        var customer = await _context.Customers
            .Include(c => c.Bills)
            .FirstOrDefaultAsync(c => c.AccountNumber == request.AccountNumber.Trim().ToUpper());

        if (customer == null)
        {
            return ApiResponse<ThirdPartyPaymentResponse>.Fail($"Customer account '{request.AccountNumber}' not found in NWSDB registry.");
        }

        // Find earliest unpaid bill
        var pendingBill = await _context.Bills
            .Where(b => b.AccountNumber == customer.AccountNumber && b.Status != BillStatus.Paid)
            .OrderBy(b => b.DueDate)
            .FirstOrDefaultAsync();

        string reference = $"NWSDB-EXT-{partner.PartnerCode}-{DateTime.UtcNow:yyyyMMddHHmmss}-{new Random().Next(100, 999)}";

        var payment = new Payment
        {
            PaymentReference = reference,
            BillId = pendingBill?.Id,
            AccountNumber = customer.AccountNumber,
            Amount = request.Amount,
            PaymentDate = DateTime.UtcNow,
            Method = PaymentMethod.PartnerApp,
            Status = PaymentStatus.Success,
            MaskedCardOrPhone = string.IsNullOrWhiteSpace(request.ExternalTransactionId) ? "PARTNER-APP" : $"EXT-{request.ExternalTransactionId}",
            TransactionNotes = $"Payment received via Partner '{partner.PartnerName}'. Channel: {request.Channel}",
            ThirdPartyPartnerId = partner.Id
        };

        _context.Payments.Add(payment);

        if (pendingBill != null && request.Amount >= pendingBill.TotalAmount)
        {
            pendingBill.Status = BillStatus.Paid;
            pendingBill.PaidDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        await _auditService.LogAuditAsync(
            "ThirdPartyPayment",
            partner.PartnerName,
            customer.AccountNumber,
            reference,
            $"Partner {partner.PartnerCode} posted payment of Rs. {request.Amount:F2} for {customer.AccountNumber}."
        );

        var response = new ThirdPartyPaymentResponse
        {
            Success = true,
            TransactionReference = reference,
            AccountNumber = customer.AccountNumber,
            AmountProcessed = request.Amount,
            ProcessedAt = payment.PaymentDate,
            ConfirmationStatus = "SETTLED",
            Message = $"Payment processed successfully by NWSDB for partner {partner.PartnerName}."
        };

        return ApiResponse<ThirdPartyPaymentResponse>.Ok(response, "Payment received and settled.");
    }

    public async Task<List<ThirdPartyPartnerDto>> GetAllPartnersAsync()
    {
        var partners = await _context.ThirdPartyPartners
            .Include(p => p.Payments)
            .OrderBy(p => p.PartnerName)
            .ToListAsync();

        return partners.Select(p => new ThirdPartyPartnerDto
        {
            Id = p.Id,
            PartnerCode = p.PartnerCode,
            PartnerName = p.PartnerName,
            ApiKey = p.ApiKey,
            ContactEmail = p.ContactEmail,
            ContactPhone = p.ContactPhone,
            IsActive = p.IsActive,
            TotalTransactionsProcessed = p.Payments.Count,
            TotalAmountProcessed = p.Payments.Sum(pay => pay.Amount)
        }).ToList();
    }

    public async Task<ApiResponse<ThirdPartyPartnerDto>> RegisterPartnerAsync(RegisterPartnerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PartnerCode) || string.IsNullOrWhiteSpace(request.PartnerName))
        {
            return ApiResponse<ThirdPartyPartnerDto>.Fail("Partner code and name are required.");
        }

        string code = request.PartnerCode.Trim().ToUpper();
        if (await _context.ThirdPartyPartners.AnyAsync(p => p.PartnerCode == code))
        {
            return ApiResponse<ThirdPartyPartnerDto>.Fail($"Partner with code '{code}' already exists.");
        }

        string newApiKey = $"NWSDB-{code}-KEY-{Guid.NewGuid():N}"[..30].ToUpper();

        var partner = new ThirdPartyPartner
        {
            PartnerCode = code,
            PartnerName = request.PartnerName.Trim(),
            ApiKey = newApiKey,
            ContactEmail = request.ContactEmail.Trim(),
            ContactPhone = request.ContactPhone.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ThirdPartyPartners.Add(partner);
        await _context.SaveChangesAsync();

        await _auditService.LogAuditAsync(
            "PartnerRegistered",
            "Admin",
            null,
            $"Partner-{partner.Id}",
            $"New third-party partner '{partner.PartnerName}' registered with code '{partner.PartnerCode}'."
        );

        var dto = new ThirdPartyPartnerDto
        {
            Id = partner.Id,
            PartnerCode = partner.PartnerCode,
            PartnerName = partner.PartnerName,
            ApiKey = partner.ApiKey,
            ContactEmail = partner.ContactEmail,
            ContactPhone = partner.ContactPhone,
            IsActive = partner.IsActive,
            TotalTransactionsProcessed = 0,
            TotalAmountProcessed = 0
        };

        return ApiResponse<ThirdPartyPartnerDto>.Ok(dto, "Partner registered successfully.");
    }

    public async Task<ApiResponse<bool>> TogglePartnerStatusAsync(int partnerId)
    {
        var partner = await _context.ThirdPartyPartners.FindAsync(partnerId);
        if (partner == null)
        {
            return ApiResponse<bool>.Fail("Partner not found.");
        }

        partner.IsActive = !partner.IsActive;
        await _context.SaveChangesAsync();

        await _auditService.LogAuditAsync(
            "PartnerStatusChanged",
            "Admin",
            null,
            $"Partner-{partner.Id}",
            $"Partner {partner.PartnerCode} active status changed to {partner.IsActive}."
        );

        return ApiResponse<bool>.Ok(partner.IsActive, $"Partner is now {(partner.IsActive ? "active" : "inactive")}.");
    }
}
