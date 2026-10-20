using NWSDB.Core.DTOs;
using NWSDB.Services.Implementations;
using Xunit;

namespace NWSDB.Tests;

public class ThirdPartyPartnerTests
{
    [Fact]
    public async Task ProcessThirdPartyPayment_ValidApiKey_ProcessesPaymentSuccessfully()
    {
        // Arrange
        using var context = TestDbContextFactory.Create(nameof(ProcessThirdPartyPayment_ValidApiKey_ProcessesPaymentSuccessfully));
        var audit = new TransactionAuditService(context);
        var service = new ThirdPartyPartnerService(context, audit);

        var request = new ThirdPartyPaymentRequest
        {
            AccountNumber = "ACC10001",
            Amount = 1200m,
            ExternalTransactionId = "TXN-BANK-9988",
            Channel = "ComBank Online"
        };

        // Act - API key seeded for Commercial Bank is NWSDB-COMBANK-KEY-778899
        var result = await service.ProcessThirdPartyPaymentAsync("NWSDB-COMBANK-KEY-778899", request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("SETTLED", result.Data.ConfirmationStatus);
        Assert.StartsWith("NWSDB-EXT-COMBANK", result.Data.TransactionReference);
    }

    [Fact]
    public async Task ProcessThirdPartyPayment_InvalidApiKey_RejectsUnauthorized()
    {
        // Arrange
        using var context = TestDbContextFactory.Create(nameof(ProcessThirdPartyPayment_InvalidApiKey_RejectsUnauthorized));
        var audit = new TransactionAuditService(context);
        var service = new ThirdPartyPartnerService(context, audit);

        var request = new ThirdPartyPaymentRequest
        {
            AccountNumber = "ACC10001",
            Amount = 500m,
            ExternalTransactionId = "TXN-FAKE",
            Channel = "FakeBank"
        };

        // Act
        var result = await service.ProcessThirdPartyPaymentAsync("INVALID-KEY-12345", request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Unauthorized", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}
