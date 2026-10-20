namespace NWSDB.Core.Logic; // Defining the namespace for the NWSDB tariff calculation logic

public class TariffCalculationResult // Represents the result of a tariff calculation, including charges and tier information
{
    public decimal UnitsConsumed { get; set; } // The number of units of water consumed ,decimal value to accommodate fractional units
    public decimal WaterCharge { get; set; } // The charge for the water consumed
    public decimal FixedServiceCharge { get; set; } // The fixed service charge
    public decimal VatAmount { get; set; } // The amount of VAT applied
    public decimal TotalAmount { get; set; } // The total amount due, including water charge, fixed service charge, and VAT
    public string TierDescription { get; set; } = string.Empty; // A description of the tariff tier applied based on units consumed
}

public static class NwsdbTariffCalculator // A static class that provides methods to calculate water bill charges based on the NWSDB tiered tariff structure
{
    /// <summary>
    /// Calculates domestic water bill charges based on official NWSDB tiered tariff structure.
    /// </summary>
    public static TariffCalculationResult CalculateDomesticBill(decimal unitsConsumed, decimal vatRate = 0.025m) 
        // Calculates the domestic water bill based on units consumed and an optional VAT rate (default is 2.5%)
    {
        if (unitsConsumed < 0) // Validates that the number of units consumed is not negative, throwing an exception if it is
        {
            throw new ArgumentException("Units consumed cannot be negative.", nameof(unitsConsumed)); // Throws an exception if the input is invalid
        }

        decimal waterCharge = 0m; // Initializes the water charge to zero
        decimal fixedCharge = 0m; // Initializes the fixed service charge to zero
        string tier = string.Empty; // Initializes the tier description to an empty string

        if (unitsConsumed <= 5) // Determines the water charge, fixed service charge, and tier description based on the number of units consumed
        {
            waterCharge = unitsConsumed * 60m; // Calculates the water charge for Tier 1 (0-5 units @ Rs. 60)
            fixedCharge = 300m; // Sets the fixed service charge for Tier 1
            tier = "Tier 1 (0-5 units @ Rs. 60)"; // Sets the tier description for Tier 1
        }
        else if (unitsConsumed <= 10) // Determines the water charge, fixed service charge, and tier description for Tier 2 (6-10 units @ Rs. 80)
        {
            waterCharge = (5 * 60m) + ((unitsConsumed - 5) * 80m); // Calculates the water charge for Tier 2 (6-10 units @ Rs. 80)
            fixedCharge = 300m; // Sets the fixed service charge for Tier 2
            tier = "Tier 2 (6-10 units @ Rs. 80)"; // Sets the tier description for Tier 2
        }
        else if (unitsConsumed <= 15) // Determines the water charge, fixed service charge, and tier description for Tier 3 (11-15 units @ Rs. 100)
        {
            waterCharge = (5 * 60m) + (5 * 80m) + ((unitsConsumed - 10) * 100m); // Calculates the water charge for Tier 3 (11-15 units @ Rs. 100)
            fixedCharge = 400m; // Sets the fixed service charge for Tier 3
            tier = "Tier 3 (11-15 units @ Rs. 100)"; // Sets the tier description for Tier 3
        }
        else if (unitsConsumed <= 20) // Determines the water charge, fixed service charge, and tier description for Tier 4 (16-20 units @ Rs. 130)
        {
            waterCharge = (5 * 60m) + (5 * 80m) + (5 * 100m) + ((unitsConsumed - 15) * 130m); // Calculates the water charge for Tier 4 (16-20 units @ Rs. 130)
            fixedCharge = 500m; // Sets the fixed service charge for Tier 4
            tier = "Tier 4 (16-20 units @ Rs. 130)"; // Sets the tier description for Tier 4
        }
        else if (unitsConsumed <= 25) // Determines the water charge, fixed service charge, and tier description for Tier 5 (21-25 units @ Rs. 160)
        {
            waterCharge = (5 * 60m) + (5 * 80m) + (5 * 100m) + (5 * 130m) + ((unitsConsumed - 20) * 160m); // Calculates the water charge for Tier 5 (21-25 units @ Rs. 160)
            fixedCharge = 600m; // Sets the fixed service charge for Tier 5
            tier = "Tier 5 (21-25 units @ Rs. 160)"; // Sets the tier description for Tier 5
        }
        else if (unitsConsumed <= 30) // Determines the water charge, fixed service charge, and tier description for Tier 6 (26-30 units @ Rs. 200)
        {
            waterCharge = (5 * 60m) + (5 * 80m) + (5 * 100m) + (5 * 130m) + (5 * 160m) + ((unitsConsumed - 25) * 200m); // Calculates the water charge for Tier 6 (26-30 units @ Rs. 200)
            fixedCharge = 800m; // Sets the fixed service charge for Tier 6
            tier = "Tier 6 (26-30 units @ Rs. 200)"; // Sets the tier description for Tier 6
        }
        else if (unitsConsumed <= 40) // Determines the water charge, fixed service charge, and tier description for Tier 7 (31-40 units @ Rs. 240)
        {
            waterCharge = (5 * 60m) + (5 * 80m) + (5 * 100m) + (5 * 130m) + (5 * 160m) + (5 * 200m) + ((unitsConsumed - 30) * 240m); // Calculates the water charge for Tier 7 (31-40 units @ Rs. 240)
            fixedCharge = 1000m; // Sets the fixed service charge for Tier 7
            tier = "Tier 7 (31-40 units @ Rs. 240)"; // Sets the tier description for Tier 7
        }
        else if (unitsConsumed <= 50) // Determines the water charge, fixed service charge, and tier description for Tier 8 (41-50 units @ Rs. 280)
        {
            waterCharge = (5 * 60m) + (5 * 80m) + (5 * 100m) + (5 * 130m) + (5 * 160m) + (5 * 200m) + (10 * 240m) + ((unitsConsumed - 40) * 280m); // Calculates the water charge for Tier 8 (41-50 units @ Rs. 280)
            fixedCharge = 1200m; // Sets the fixed service charge for Tier 8
            tier = "Tier 8 (41-50 units @ Rs. 280)"; // Sets the tier description for Tier 8
        }
        else
        {
            waterCharge = (5 * 60m) + (5 * 80m) + (5 * 100m) + (5 * 130m) + (5 * 160m) + (5 * 200m) + (10 * 240m) + (10 * 280m) + ((unitsConsumed - 50) * 320m); // Calculates the water charge for Tier 9 (> 50 units @ Rs. 320)
            fixedCharge = 1500m; // Sets the fixed service charge for Tier 9
            tier = "Tier 9 (> 50 units @ Rs. 320)"; // Sets the tier description for Tier 9
        }

        decimal subTotal = waterCharge + fixedCharge; // Calculates the subtotal before VAT by summing the water charge and fixed service charge
        decimal vatAmount = Math.Round(subTotal * vatRate, 2); // Calculates the VAT amount by applying the VAT rate to the subtotal and rounding to 2 decimal places
        decimal totalAmount = Math.Round(subTotal + vatAmount, 2); // Calculates the total amount due by adding the subtotal and VAT amount, rounding to 2 decimal places

        return new TariffCalculationResult // Returns a new instance of TariffCalculationResult with the calculated values
        {
            UnitsConsumed = unitsConsumed, // Sets the number of units consumed in the result
            WaterCharge = Math.Round(waterCharge, 2), // Sets the water charge in the result, rounded to 2 decimal places
            FixedServiceCharge = Math.Round(fixedCharge, 2), // Sets the fixed service charge in the result, rounded to 2 decimal places
            VatAmount = vatAmount, // Sets the VAT amount in the result
            TotalAmount = totalAmount, // Sets the total amount due in the result
            TierDescription = tier // Sets the tier description in the result
        };
    }
}
