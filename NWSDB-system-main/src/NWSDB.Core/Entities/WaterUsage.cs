namespace NWSDB.Core.Entities; // Defining the namespace for the WaterUsage entity

public class WaterUsage // Represents a water usage record for a specific account and billing period
{
    public int Id { get; set; } // Unique identifier for the water usage record
    public string AccountNumber { get; set; } = string.Empty; // The account number associated with the water usage record
    public string MonthYear { get; set; } = string.Empty; // The month and year for which the water usage is recorded, e.g., "January 2024"
    public decimal PreviousReading { get; set; } // The previous meter reading for the billing period
    public decimal CurrentReading { get; set; } // The current meter reading for the billing period
    public decimal UnitsConsumed { get; set; } // CurrentReading - PreviousReading = UnitsConsumed 
    public DateTime ReadingDate { get; set; } = DateTime.UtcNow; // The date when the current meter reading was taken
    public string MeterReaderName { get; set; } = "Field Officer"; // The name of the meter reader who recorded the current reading
    public string Remarks { get; set; } = string.Empty; // Additional remarks or comments about the water usage record

    // Navigation
    public Customer? Customer { get; set; } // The customer associated with the water usage record (nullable)
}
