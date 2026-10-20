using Microsoft.EntityFrameworkCore;
using NWSDB.Core.DTOs;
using NWSDB.Core.Entities;
using NWSDB.Data;
using NWSDB.Services.Interfaces;

namespace NWSDB.Services.Implementations;

public class WaterUsageService : IWaterUsageService
{
    private readonly NwsdbDbContext _context;
    private readonly ITransactionAuditService _auditService;

    public WaterUsageService(NwsdbDbContext context, ITransactionAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<List<WaterUsageDto>> GetUsageHistoryAsync(string accountNumber)
    {
        var usages = await _context.WaterUsages
            .Where(u => u.AccountNumber.ToUpper() == accountNumber.Trim().ToUpper())
            .OrderByDescending(u => u.MonthYear)
            .ToListAsync();

        return usages.Select(u => new WaterUsageDto
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
        }).ToList();
    }

    public async Task<List<WaterUsageDto>> GetAllUsagesAsync()
    {
        var usages = await _context.WaterUsages
            .OrderByDescending(u => u.ReadingDate)
            .ToListAsync();

        return usages.Select(u => new WaterUsageDto
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
        }).ToList();
    }

    public async Task<WaterUsageDto?> GetLatestUsageAsync(string accountNumber)
    {
        var u = await _context.WaterUsages
            .Where(u => u.AccountNumber.ToUpper() == accountNumber.Trim().ToUpper())
            .OrderByDescending(u => u.MonthYear)
            .FirstOrDefaultAsync();

        if (u == null) return null;

        return new WaterUsageDto
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
        };
    }

    public async Task<ApiResponse<WaterUsageDto>> RecordMeterReadingAsync(RecordUsageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AccountNumber))
        {
            return ApiResponse<WaterUsageDto>.Fail("Account number is required.");
        }

        if (request.CurrentReading < request.PreviousReading)
        {
            return ApiResponse<WaterUsageDto>.Fail("Current reading cannot be lower than previous reading.");
        }

        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.AccountNumber == request.AccountNumber.Trim().ToUpper());
        if (customer == null)
        {
            return ApiResponse<WaterUsageDto>.Fail($"Customer account {request.AccountNumber} not found.");
        }

        string monthYear = string.IsNullOrWhiteSpace(request.MonthYear)
            ? DateTime.UtcNow.ToString("yyyy-MM")
            : request.MonthYear.Trim();

        // Check for existing reading for same month
        var existing = await _context.WaterUsages
            .FirstOrDefaultAsync(u => u.AccountNumber == customer.AccountNumber && u.MonthYear == monthYear);

        decimal units = request.CurrentReading - request.PreviousReading;

        if (existing != null)
        {
            existing.PreviousReading = request.PreviousReading;
            existing.CurrentReading = request.CurrentReading;
            existing.UnitsConsumed = units;
            existing.ReadingDate = DateTime.UtcNow;
            existing.MeterReaderName = string.IsNullOrWhiteSpace(request.MeterReaderName) ? "Officer" : request.MeterReaderName.Trim();
            existing.Remarks = request.Remarks;

            await _context.SaveChangesAsync();
            await _auditService.LogAuditAsync("MeterReadingUpdated", "Admin", customer.AccountNumber, $"Usage-{existing.Id}", $"Updated meter reading for {monthYear}: {units} units.");

            return ApiResponse<WaterUsageDto>.Ok(new WaterUsageDto
            {
                Id = existing.Id,
                AccountNumber = existing.AccountNumber,
                MonthYear = existing.MonthYear,
                PreviousReading = existing.PreviousReading,
                CurrentReading = existing.CurrentReading,
                UnitsConsumed = existing.UnitsConsumed,
                ReadingDate = existing.ReadingDate,
                MeterReaderName = existing.MeterReaderName,
                Remarks = existing.Remarks
            }, "Meter reading updated.");
        }

        var usage = new WaterUsage
        {
            AccountNumber = customer.AccountNumber,
            MonthYear = monthYear,
            PreviousReading = request.PreviousReading,
            CurrentReading = request.CurrentReading,
            UnitsConsumed = units,
            ReadingDate = DateTime.UtcNow,
            MeterReaderName = string.IsNullOrWhiteSpace(request.MeterReaderName) ? "Officer" : request.MeterReaderName.Trim(),
            Remarks = request.Remarks
        };

        _context.WaterUsages.Add(usage);
        await _context.SaveChangesAsync();

        await _auditService.LogAuditAsync("MeterReadingRecorded", "Admin", customer.AccountNumber, $"Usage-{usage.Id}", $"Recorded reading for {monthYear}: {units} units.");

        var dto = new WaterUsageDto
        {
            Id = usage.Id,
            AccountNumber = usage.AccountNumber,
            MonthYear = usage.MonthYear,
            PreviousReading = usage.PreviousReading,
            CurrentReading = usage.CurrentReading,
            UnitsConsumed = usage.UnitsConsumed,
            ReadingDate = usage.ReadingDate,
            MeterReaderName = usage.MeterReaderName,
            Remarks = usage.Remarks
        };

        return ApiResponse<WaterUsageDto>.Ok(dto, "Meter reading recorded successfully.");
    }
}
