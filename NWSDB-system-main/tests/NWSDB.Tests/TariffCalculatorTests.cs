using NWSDB.Core.Logic;
using Xunit;

namespace NWSDB.Tests;

public class TariffCalculatorTests
{
    [Fact]
    public void CalculateDomesticBill_ZeroUnits_ReturnsFixedChargeAndVat()
    {
        // Act
        var result = NwsdbTariffCalculator.CalculateDomesticBill(0);

        // Assert
        Assert.Equal(0m, result.UnitsConsumed);
        Assert.Equal(0m, result.WaterCharge);
        Assert.Equal(300m, result.FixedServiceCharge);
        Assert.Equal(7.50m, result.VatAmount); // 300 * 0.025
        Assert.Equal(307.50m, result.TotalAmount);
    }

    [Fact]
    public void CalculateDomesticBill_FiveUnits_CalculatesTier1Correctly()
    {
        // Act: 5 units @ Rs. 60 = 300. Fixed = 300. Subtotal = 600. VAT 2.5% = 15. Total = 615.
        var result = NwsdbTariffCalculator.CalculateDomesticBill(5);

        // Assert
        Assert.Equal(5m, result.UnitsConsumed);
        Assert.Equal(300m, result.WaterCharge);
        Assert.Equal(300m, result.FixedServiceCharge);
        Assert.Equal(15m, result.VatAmount);
        Assert.Equal(615m, result.TotalAmount);
    }

    [Fact]
    public void CalculateDomesticBill_TenUnits_CalculatesTier2Correctly()
    {
        // Act: 5*60 (300) + 5*80 (400) = 700 water charge. Fixed = 300. Subtotal = 1000. VAT = 25. Total = 1025.
        var result = NwsdbTariffCalculator.CalculateDomesticBill(10);

        // Assert
        Assert.Equal(10m, result.UnitsConsumed);
        Assert.Equal(700m, result.WaterCharge);
        Assert.Equal(300m, result.FixedServiceCharge);
        Assert.Equal(25m, result.VatAmount);
        Assert.Equal(1025m, result.TotalAmount);
    }

    [Fact]
    public void CalculateDomesticBill_FifteenUnits_CalculatesTier3Correctly()
    {
        // Act: 5*60 (300) + 5*80 (400) + 5*100 (500) = 1200 water charge. Fixed = 400. Subtotal = 1600. VAT = 40. Total = 1640.
        var result = NwsdbTariffCalculator.CalculateDomesticBill(15);

        // Assert
        Assert.Equal(15m, result.UnitsConsumed);
        Assert.Equal(1200m, result.WaterCharge);
        Assert.Equal(400m, result.FixedServiceCharge);
        Assert.Equal(40m, result.VatAmount);
        Assert.Equal(1640m, result.TotalAmount);
    }

    [Fact]
    public void CalculateDomesticBill_NegativeUnits_ThrowsArgumentException()
    {
        // Assert
        Assert.Throws<ArgumentException>(() => NwsdbTariffCalculator.CalculateDomesticBill(-5));
    }
}
