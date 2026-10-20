using NWSDB.Core.Enums; // Importing the enums used in the User entity

namespace NWSDB.Core.Entities; // Defining the namespace for the User entity

public class User // Represents a user in the system, which can be an admin, staff, or customer
{
    public int Id { get; set; } // Unique identifier for the user
    public string Username { get; set; } = string.Empty; // The username used for login
    public string PasswordHash { get; set; } = string.Empty; // The hashed password for the user
    public string FullName { get; set; } = string.Empty; // The full name of the user
    public string Email { get; set; } = string.Empty; // The email address of the user
    public string PhoneNumber { get; set; } = string.Empty; // The phone number of the user
    public UserRole Role { get; set; } = UserRole.Customer; // The role of the user (Admin, Staff, Customer)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // The date and time when the user was created
    public bool IsActive { get; set; } = true; // Indicates whether the user account is currently active

    // Optional link to Customer profile if role is Customer
    public Customer? CustomerProfile { get; set; } // Navigation property to the Customer entity (nullable)
}
