namespace NWSDB.Core.Entities; // Defining the namespace for the ThirdPartyPartner entity

public class ThirdPartyPartner // Represents a third-party partner involved in payment processing
{
    public int Id { get; set; } // Unique identifier for the third-party partner
    public string PartnerCode { get; set; } = string.Empty; // A unique code representing the third-party partner
    public string PartnerName { get; set; } = string.Empty; // The name of the third-party partner
    public string ApiKey { get; set; } = string.Empty; // The API key used for authentication with the third-party partner
    public string ContactEmail { get; set; } = string.Empty; // The contact email of the third-party partner
    public string ContactPhone { get; set; } = string.Empty; // The contact phone number of the third-party partner
    public bool IsActive { get; set; } = true; // Indicates whether the third-party partner is currently active
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // The date and time when the third-party partner was created

    public ICollection<Payment> Payments { get; set; } = new List<Payment>(); // A collection of payments associated with the third-party partner
}
