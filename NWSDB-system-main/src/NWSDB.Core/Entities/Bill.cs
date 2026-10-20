using NWSDB.Core.Enums;  // Importing the enums used in the Bill entity

namespace NWSDB.Core.Entities; // Defining the namespace for the Bill entity

public class Bill // Represents a water bill issued to a customer
{
    public int Id { get; set; } // Unique identifier for the bill
    public string AccountNumber { get; set; } = string.Empty; // The account number associated with the bill
    public string MonthYear { get; set; } = string.Empty; // The billing period in "MM/yyyy" format
    public decimal UnitsConsumed { get; set; } // The number of water units consumed during the billing period
    public decimal WaterCharge { get; set; } // The charge for the water consumed based on the units consumed
    public decimal FixedServiceCharge { get; set; } // The fixed service charge for the billing period
    public decimal VatAmount { get; set; } // The VAT amount applied to the bill
    public decimal TotalAmount { get; set; } // The total amount due for the bill
    public DateTime IssueDate { get; set; } = DateTime.UtcNow; // The date the bill was issued
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(21); // The date the bill is due
    public BillStatus Status { get; set; } = BillStatus.Pending; // The current status of the bill (Pending, Paid, Overdue, etc.)
    public DateTime? PaidDate { get; set; } // The date the bill was paid, if applicable

    // Navigation
    public Customer? Customer { get; set; } // The customer associated with the bill (nullable)
    public ICollection<Payment> Payments { get; set; } = new List<Payment>(); // A collection of payments made towards the bill
}
