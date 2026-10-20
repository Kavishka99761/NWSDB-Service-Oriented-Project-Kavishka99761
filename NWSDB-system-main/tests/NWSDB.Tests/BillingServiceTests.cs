using NWSDB.Core.DTOs;
using NWSDB.Core.Enums;
using NWSDB.Services.Implementations;
using Xunit;

namespace NWSDB.Tests;

public class BillingServiceTests
{
    [Fact]
    public async Task GenerateBillForAccount_ValidAccount_CalculatesCorrectCharges()
    {
        // Arrange
        using var context = TestDbContextFactory.Create(nameof(GenerateBillForAccount_ValidAccount_CalculatesCorrectCharges));
        var audit = new TransactionAuditService(context);
        var service = new BillingService(context, audit);

        var request = new GenerateBillRequest
        {
            AccountNumber = "ACC10001",
            MonthYear = "2026-11",
            UnitsConsumed = 10
        };

        // Act
        var result = await service.GenerateBillForAccountAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(10m, result.Data.UnitsConsumed);
        Assert.Equal(1025m, result.Data.TotalAmount); // 5*60 + 5*80 + 300 + 2.5% = 1025
        Assert.Equal(BillStatus.Pending, result.Data.Status);
    }

    [Fact]
    public async Task GenerateBillForAccount_DuplicateMonth_FailsValidation()
    {
        // Arrange
        using var context = TestDbContextFactory.Create(nameof(GenerateBillForAccount_DuplicateMonth_FailsValidation));
        var audit = new TransactionAuditService(context);
        var service = new BillingService(context, audit);

        var request = new GenerateBillRequest
        {
            AccountNumber = "ACC10001",
            MonthYear = "2026-09", // Already exists from seeding
            UnitsConsumed = 12
        };

        // Act
        var result = await service.GenerateBillForAccountAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("already exists", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}
