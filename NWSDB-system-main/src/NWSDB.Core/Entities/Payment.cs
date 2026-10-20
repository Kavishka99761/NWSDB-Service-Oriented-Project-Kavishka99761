using NWSDB.Core.Enums; // Importing the enums used in the Payment entity

namespace NWSDB.Core.Entities; // Defining the namespace for the Payment entity

public class Payment // Represents a payment made by a customer for a water bill
{
    public int Id { get; set; }
    public string PaymentReference { get; set; } = string.Empty; // Unique reference for the payment transaction
    public int? BillId { get; set; } // Foreign key to the Bill entity (nullable)
    public string AccountNumber { get; set; } = string.Empty; // The account number associated with the payment
    public decimal Amount { get; set; } // The amount of the payment
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow; // The date the payment was made
    public PaymentMethod Method { get; set; } = PaymentMethod.CreditCard; // The method used for the payment
    public PaymentStatus Status { get; set; } = PaymentStatus.Success; // The current status of the payment (Success, Failed, Pending, etc.)
    public string MaskedCardOrPhone { get; set; } = string.Empty; // The masked card number or phone number used for the payment
    public string TransactionNotes { get; set; } = string.Empty; // Additional notes or details about the transaction
    public int? ThirdPartyPartnerId { get; set; } // Foreign key to the ThirdPartyPartner entity (nullable)

    // Navigation
    public Bill? Bill { get; set; } // The bill associated with the payment (nullable)
    public Customer? Customer { get; set; } // The customer who made the payment (nullable)
    public ThirdPartyPartner? ThirdPartyPartner { get; set; } // The third-party partner involved in the payment (nullable)
}
