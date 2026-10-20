using NWSDB.Core.DTOs;
using NWSDB.Services.Implementations;
using Xunit;

namespace NWSDB.Tests;

public class WaterUsageServiceTests
{
    [Fact]
    public async Task RecordMeterReading_ValidReadings_CalculatesConsumedUnitsCorrectly()
    {
        // Arrange
        using var context = TestDbContextFactory.Create(nameof(RecordMeterReading_ValidReadings_CalculatesConsumedUnitsCorrectly));
        var audit = new TransactionAuditService(context);
        var service = new WaterUsageService(context, audit);

        var request = new RecordUsageRequest
        {
            AccountNumber = "ACC10001",
            MonthYear = "2026-10",
            PreviousReading = 254,
            CurrentReading = 274, // 20 units
            MeterReaderName = "Tester",
            Remarks = "Test reading"
        };

        // Act
        var result = await service.RecordMeterReadingAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(20m, result.Data.UnitsConsumed);
        Assert.Equal("2026-10", result.Data.MonthYear);
    }

    [Fact]
    public async Task RecordMeterReading_CurrentReadingLessThanPrevious_FailsValidation()
    {
        // Arrange
        using var context = TestDbContextFactory.Create(nameof(RecordMeterReading_CurrentReadingLessThanPrevious_FailsValidation));
        var audit = new TransactionAuditService(context);
        var service = new WaterUsageService(context, audit);

        var request = new RecordUsageRequest
        {
            AccountNumber = "ACC10001",
            MonthYear = "2026-10",
            PreviousReading = 300,
            CurrentReading = 200, // Invalid: decreased
            MeterReaderName = "Tester"
        };

        // Act
        var result = await service.RecordMeterReadingAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("lower than previous", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}
