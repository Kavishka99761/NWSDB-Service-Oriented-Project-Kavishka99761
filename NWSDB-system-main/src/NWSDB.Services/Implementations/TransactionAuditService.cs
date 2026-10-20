using Microsoft.EntityFrameworkCore;
using NWSDB.Core.DTOs;
using NWSDB.Core.Entities;
using NWSDB.Core.Enums;
using NWSDB.Data;
using NWSDB.Services.Interfaces;

namespace NWSDB.Services.Implementations;

public class TransactionAuditService : ITransactionAuditService
{
    private readonly NwsdbDbContext _context;

    public TransactionAuditService(NwsdbDbContext context)
    {
        _context = context;
    }

    public async Task LogAuditAsync(string action, string performedBy, string? accountNumber, string? referenceId, string details, string clientIp = "Local")
    {
        try
        {
            var audit = new TransactionAudit
            {
                Timestamp = DateTime.UtcNow,
                Action = action,
                PerformedBy = performedBy,
                AccountNumber = accountNumber,
                ReferenceId = referenceId,
                Details = details,
                SourceIpOrClient = clientIp
            };

            _context.TransactionAudits.Add(audit);
            await _context.SaveChangesAsync();
        }
        catch
        {
            // Logging shouldn't crash the calling operation
        }
    }

    public async Task<List<TransactionAudit>> GetRecentAuditsAsync(int count = 50)
    {
        return await _context.TransactionAudits
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<AdminDashboardDto> GetAdminDashboardSummaryAsync()
    {
        int totalCust = await _context.Customers.CountAsync(c => c.IsActive);
        var pendingBills = await _context.Bills.Where(b => b.Status != BillStatus.Paid).ToListAsync();
        int pendingCount = pendingBills.Count;
        decimal pendingAmount = pendingBills.Sum(b => b.TotalAmount);

        decimal totalRevenue = await _context.Payments
            .Where(p => p.Status == PaymentStatus.Success)
            .SumAsync(p => p.Amount);

        var today = DateTime.UtcNow.Date;
        var todayPayments = await _context.Payments
            .Where(p => p.PaymentDate >= today && p.Status == PaymentStatus.Success)
            .ToListAsync();

        int activePartners = await _context.ThirdPartyPartners.CountAsync(p => p.IsActive);

        var recentPayments = await _context.Payments
            .Include(p => p.ThirdPartyPartner)
            .OrderByDescending(p => p.PaymentDate)
            .Take(10)
            .Select(p => new PaymentHistoryDto
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
                PartnerName = p.ThirdPartyPartner != null ? p.ThirdPartyPartner.PartnerName : null
            })
            .ToListAsync();

        return new AdminDashboardDto
        {
            TotalCustomers = totalCust,
            TotalPendingBills = pendingCount,
            TotalPendingAmount = pendingAmount,
            TotalRevenueCollected = totalRevenue,
            TotalTransactionsToday = todayPayments.Count,
            TotalAmountToday = todayPayments.Sum(p => p.Amount),
            ActivePartners = activePartners,
            RecentPayments = recentPayments
        };
    }

    public async Task<CustomerDashboardDto?> GetCustomerDashboardSummaryAsync(string accountNumber)
    {
        var customer = await _context.Customers
            .Include(c => c.Bills)
            .FirstOrDefaultAsync(c => c.AccountNumber.ToUpper() == accountNumber.Trim().ToUpper());

        if (customer == null) return null;

        decimal outstanding = customer.Bills
            .Where(b => b.Status != BillStatus.Paid)
            .Sum(b => b.TotalAmount);

        var custDto = new CustomerDto
        {
            Id = customer.Id,
            AccountNumber = customer.AccountNumber,
            FullName = customer.FullName,
            NIC = customer.NIC,
            Address = customer.Address,
            AreaBranch = customer.AreaBranch,
            Phone = customer.Phone,
            Email = customer.Email,
            MeterNumber = customer.MeterNumber,
            ConnectionDate = customer.ConnectionDate,
            IsActive = customer.IsActive,
            CurrentOutstandingBalance = outstanding
        };

        var latestUsage = await _context.WaterUsages
            .Where(u => u.AccountNumber == customer.AccountNumber)
            .OrderByDescending(u => u.MonthYear)
            .Select(u => new WaterUsageDto
            {
                Id = u.Id,
                AccountNumber = u.AccountNumber,
                MonthYear = u.MonthYear,
                PreviousReading = u.PreviousReading,
                CurrentReading = u.CurrentReading,
                UnitsConsumed = u.UnitsConsumed,
                ReadingDate = u.ReadingDate,
                MeterReaderName = u.MeterReaderName,
                Remarks = u.Remarks
            })
            .FirstOrDefaultAsync();

        var currentBill = await _context.Bills
            .Where(b => b.AccountNumber == customer.AccountNumber)
            .OrderByDescending(b => b.MonthYear)
            .Select(b => new BillDto
            {
                Id = b.Id,
                AccountNumber = b.AccountNumber,
                CustomerName = customer.FullName,
                MonthYear = b.MonthYear,
                UnitsConsumed = b.UnitsConsumed,
                WaterCharge = b.WaterCharge,
                FixedServiceCharge = b.FixedServiceCharge,
                VatAmount = b.VatAmount,
                TotalAmount = b.TotalAmount,
                IssueDate = b.IssueDate,
                DueDate = b.DueDate,
                Status = b.Status,
                PaidDate = b.PaidDate
            })
            .FirstOrDefaultAsync();

        var unpaidBills = await _context.Bills
            .Where(b => b.AccountNumber == customer.AccountNumber && b.Status != BillStatus.Paid)
            .OrderBy(b => b.DueDate)
            .Select(b => new BillDto
            {
                Id = b.Id,
                AccountNumber = b.AccountNumber,
                CustomerName = customer.FullName,
                MonthYear = b.MonthYear,
                UnitsConsumed = b.UnitsConsumed,
                WaterCharge = b.WaterCharge,
                FixedServiceCharge = b.FixedServiceCharge,
                VatAmount = b.VatAmount,
                TotalAmount = b.TotalAmount,
                IssueDate = b.IssueDate,
                DueDate = b.DueDate,
                Status = b.Status,
                PaidDate = b.PaidDate
            })
            .ToListAsync();

        var recentPayments = await _context.Payments
            .Include(p => p.ThirdPartyPartner)
            .Where(p => p.AccountNumber == customer.AccountNumber)
            .OrderByDescending(p => p.PaymentDate)
            .Take(5)
            .Select(p => new PaymentHistoryDto
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
                PartnerName = p.ThirdPartyPartner != null ? p.ThirdPartyPartner.PartnerName : null
            })
            .ToListAsync();

        return new CustomerDashboardDto
        {
            Customer = custDto,
            CurrentBill = currentBill,
            LatestUsage = latestUsage,
            UnpaidBills = unpaidBills,
            RecentPayments = recentPayments
        };
    }
}
