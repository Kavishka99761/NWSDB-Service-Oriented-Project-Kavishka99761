using NWSDB.Core.DTOs;
using NWSDB.Core.Enums;
using NWSDB.Services.Implementations;
using Xunit;

namespace NWSDB.Tests;

public class PaymentServiceTests
{
    [Fact]
    public async Task MakePayment_ValidPayment_SucceedsAndGeneratesOfficialReference()
    {
        // Arrange
        using var context = TestDbContextFactory.Create(nameof(MakePayment_ValidPayment_SucceedsAndGeneratesOfficialReference));
        var audit = new TransactionAuditService(context);
        var service = new PaymentService(context, audit);

        var request = new MakePaymentRequest
        {
            AccountNumber = "ACC10001",
            Amount = 1500m,
            Method = PaymentMethod.CreditCard,
            CardOrAccountNumber = "4111222233334444",
            CardExpiry = "12/28",
            CardCvv = "123",
            Notes = "Unit test payment"
        };

        // Act
        var result = await service.MakePaymentAsync(request, "UnitTest");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.StartsWith("NWSDB-PAY-", result.Data.PaymentReference);
        Assert.Equal("SUCCESS", result.Data.Status);
        Assert.Equal("**** **** **** 4444", result.Data.MaskedCardOrPhone);
        Assert.Equal(1500m, result.Data.AmountPaid);
    }

    [Fact]
    public async Task MakePayment_ZeroOrNegativeAmount_FailsValidation()
    {
        // Arrange
        using var context = TestDbContextFactory.Create(nameof(MakePayment_ZeroOrNegativeAmount_FailsValidation));
        var audit = new TransactionAuditService(context);
        var service = new PaymentService(context, audit);

        var request = new MakePaymentRequest
        {
            AccountNumber = "ACC10001",
            Amount = -100m,
            Method = PaymentMethod.CashCounter
        };

        // Act
        var result = await service.MakePaymentAsync(request, "UnitTest");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("greater than zero", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}
