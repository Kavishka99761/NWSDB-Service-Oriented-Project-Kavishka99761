namespace NWSDB.Core.Entities; // Defining the namespace for the Customer entity

public class Customer // Represents a customer in the water supply system
{
    public int Id { get; set; } // Unique identifier for the customer
    public string AccountNumber { get; set; } = string.Empty; // The account number associated with the customer
    public string FullName { get; set; } = string.Empty; // The full name of the customer
    public string NIC { get; set; } = string.Empty; // National Identity Card
    public string Address { get; set; } = string.Empty; // The address of the customer
    public string AreaBranch { get; set; } = "Colombo Central"; // The area branch the customer belongs to
    public string Phone { get; set; } = string.Empty; // The phone number of the customer
    public string Email { get; set; } = string.Empty; // The email address of the customer
    public string MeterNumber { get; set; } = string.Empty; // The meter number associated with the customer
    public DateTime ConnectionDate { get; set; } = DateTime.UtcNow; // The date the customer was connected to the water supply
    public bool IsActive { get; set; } = true; // Indicates whether the customer is currently active

    // Foreign Key to User
    public int? UserId { get; set; } // Foreign key to the User entity (nullable)
    public User? User { get; set; } // Navigation property to the User entity (nullable)

    // Navigation properties
    public ICollection<WaterUsage> Usages { get; set; } = new List<WaterUsage>(); // A collection of water usage records associated with the customer
    public ICollection<Bill> Bills { get; set; } = new List<Bill>(); // A collection of bills associated with the customer
    public ICollection<Payment> Payments { get; set; } = new List<Payment>(); // A collection of payments made by the customer
}
