using NWSDB.Services.Implementations;
using Xunit;

namespace NWSDB.Tests;

public class ReportServiceTests
{
    [Fact]
    public async Task GeneratePaymentReportExcel_ReturnsValidNonEmptyExcelBytes()
    {
        // Arrange
        using var context = TestDbContextFactory.Create(nameof(GeneratePaymentReportExcel_ReturnsValidNonEmptyExcelBytes));
        var service = new ReportService(context);

        // Act
        byte[] bytes = await service.GeneratePaymentReportExcelAsync();

        // Assert
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 1000, "Excel spreadsheet bytes should be substantial and formatted.");

        // Check ZIP / OpenXML magic header (PK\x03\x04)
        Assert.Equal((byte)'P', bytes[0]);
        Assert.Equal((byte)'K', bytes[1]);
        Assert.Equal(3, bytes[2]);
        Assert.Equal(4, bytes[3]);
    }

    [Fact]
    public async Task GenerateBillingSummaryExcel_ReturnsValidNonEmptyExcelBytes()
    {
        // Arrange
        using var context = TestDbContextFactory.Create(nameof(GenerateBillingSummaryExcel_ReturnsValidNonEmptyExcelBytes));
        var service = new ReportService(context);

        // Act
        byte[] bytes = await service.GenerateBillingSummaryExcelAsync();

        // Assert
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 1000);
        Assert.Equal((byte)'P', bytes[0]);
        Assert.Equal((byte)'K', bytes[1]);
    }
}
