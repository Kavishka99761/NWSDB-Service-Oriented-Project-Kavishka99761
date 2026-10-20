using Microsoft.EntityFrameworkCore;
using NWSDB.Core.DTOs;
using NWSDB.Core.Entities;
using NWSDB.Core.Enums;
using NWSDB.Core.Logic;
using NWSDB.Data;
using NWSDB.Services.Interfaces;

namespace NWSDB.Services.Implementations;

public class BillingService : IBillingService
{
    private readonly NwsdbDbContext _context;
    private readonly ITransactionAuditService _auditService;

    public BillingService(NwsdbDbContext context, ITransactionAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<List<BillDto>> GetBillsByAccountAsync(string accountNumber)
    {
        var bills = await _context.Bills
            .Include(b => b.Customer)
            .Where(b => b.AccountNumber.ToUpper() == accountNumber.Trim().ToUpper())
            .OrderByDescending(b => b.MonthYear)
            .ToListAsync();

        return bills.Select(b => MapToDto(b, b.Customer?.FullName ?? "")).ToList();
    }

    public async Task<List<BillDto>> GetPendingBillsAsync()
    {
        var bills = await _context.Bills
            .Include(b => b.Customer)
            .Where(b => b.Status != BillStatus.Paid)
            .OrderBy(b => b.DueDate)
            .ToListAsync();

        return bills.Select(b => MapToDto(b, b.Customer?.FullName ?? "")).ToList();
    }

    public async Task<List<BillDto>> GetAllBillsAsync()
    {
        var bills = await _context.Bills
            .Include(b => b.Customer)
            .OrderByDescending(b => b.IssueDate)
            .ToListAsync();

        return bills.Select(b => MapToDto(b, b.Customer?.FullName ?? "")).ToList();
    }

    public async Task<BillDto?> GetBillByIdAsync(int billId)
    {
        var b = await _context.Bills
            .Include(b => b.Customer)
            .FirstOrDefaultAsync(b => b.Id == billId);

        return b == null ? null : MapToDto(b, b.Customer?.FullName ?? "");
    }

    public async Task<ApiResponse<BillDto>> GenerateBillForAccountAsync(GenerateBillRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AccountNumber))
        {
            return ApiResponse<BillDto>.Fail("Account number is required.");
        }

        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.AccountNumber == request.AccountNumber.Trim().ToUpper());
        if (customer == null)
        {
            return ApiResponse<BillDto>.Fail($"Customer {request.AccountNumber} not found.");
        }

        string monthYear = string.IsNullOrWhiteSpace(request.MonthYear)
            ? DateTime.UtcNow.ToString("yyyy-MM")
            : request.MonthYear.Trim();

        var existing = await _context.Bills
            .FirstOrDefaultAsync(b => b.AccountNumber == customer.AccountNumber && b.MonthYear == monthYear);

        if (existing != null)
        {
            return ApiResponse<BillDto>.Fail($"Bill for {customer.AccountNumber} for month {monthYear} already exists (Status: {existing.Status}).");
        }

        var tariff = NwsdbTariffCalculator.CalculateDomesticBill(request.UnitsConsumed);

        var bill = new Bill
        {
            AccountNumber = customer.AccountNumber,
            MonthYear = monthYear,
            UnitsConsumed = request.UnitsConsumed,
            WaterCharge = tariff.WaterCharge,
            FixedServiceCharge = tariff.FixedServiceCharge,
            VatAmount = tariff.VatAmount,
            TotalAmount = tariff.TotalAmount,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(21),
            Status = BillStatus.Pending
        };

        _context.Bills.Add(bill);
        await _context.SaveChangesAsync();

        await _auditService.LogAuditAsync(
            "BillGenerated",
            "BillingService",
            customer.AccountNumber,
            $"Bill-{bill.Id}",
            $"Generated bill of Rs. {bill.TotalAmount:F2} for {customer.AccountNumber} ({monthYear}, {request.UnitsConsumed} units)."
        );

        return ApiResponse<BillDto>.Ok(MapToDto(bill, customer.FullName), "Bill generated successfully.");
    }

    public async Task<ApiResponse<int>> GenerateMonthlyBillsForAllAsync(string monthYear)
    {
        if (string.IsNullOrWhiteSpace(monthYear))
        {
            monthYear = DateTime.UtcNow.ToString("yyyy-MM");
        }

        var customers = await _context.Customers.Where(c => c.IsActive).ToListAsync();
        int generatedCount = 0;

        foreach (var customer in customers)
        {
            bool alreadyBilled = await _context.Bills
                .AnyAsync(b => b.AccountNumber == customer.AccountNumber && b.MonthYear == monthYear);

            if (alreadyBilled) continue;

            // Fetch usage for this month or fallback to latest usage or average 15 units
            var usage = await _context.WaterUsages
                .FirstOrDefaultAsync(u => u.AccountNumber == customer.AccountNumber && u.MonthYear == monthYear);

            decimal units = usage?.UnitsConsumed ?? 15.0m;

            var tariff = NwsdbTariffCalculator.CalculateDomesticBill(units);

            var bill = new Bill
            {
                AccountNumber = customer.AccountNumber,
                MonthYear = monthYear,
                UnitsConsumed = units,
                WaterCharge = tariff.WaterCharge,
                FixedServiceCharge = tariff.FixedServiceCharge,
                VatAmount = tariff.VatAmount,
                TotalAmount = tariff.TotalAmount,
                IssueDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(21),
                Status = BillStatus.Pending
            };

            _context.Bills.Add(bill);
            generatedCount++;
        }

        if (generatedCount > 0)
        {
            await _context.SaveChangesAsync();
            await _auditService.LogAuditAsync(
                "BatchBillingRun",
                "Admin",
                null,
                $"Batch-{monthYear}",
                $"Generated {generatedCount} monthly bills for {monthYear}."
            );
        }

        return ApiResponse<int>.Ok(generatedCount, $"Successfully generated {generatedCount} bills for period {monthYear}.");
    }

    private static BillDto MapToDto(Bill b, string customerName) => new()
    {
        Id = b.Id,
        AccountNumber = b.AccountNumber,
        CustomerName = customerName,
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
    };
}
