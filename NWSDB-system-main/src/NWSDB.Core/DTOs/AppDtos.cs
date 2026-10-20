using NWSDB.Core.Enums; //enums for BillStatus, PaymentMethod, etc.

namespace NWSDB.Core.DTOs; // Data Transfer Objects for the Water Billing System

public class ApiResponse<T> // Generic API response wrapper
{
    public bool Success { get; set; } = true; // Indicates if the API call was successful
    public string Message { get; set; } = string.Empty; // Message providing additional information about the API response
    public T? Data { get; set; } // The actual data returned by the API, can be of any type

    public static ApiResponse<T> Ok(T data, string message = "Success") => // Factory method to create a successful API response
        new() { Success = true, Message = message, Data = data }; // Uses target-typed new expression (C# 9.0 feature) to create
                                                                  // a new instance of ApiResponse<T>

    public static ApiResponse<T> Fail(string message) => // Factory method to create a failed API response
        new() { Success = false, Message = message, Data = default }; // Uses target-typed new expression to create a new
                                                                      // instance of ApiResponse<T> with default data
}

public class LoginRequest // Represents the data required for a user to log in
{
    public string Username { get; set; } = string.Empty; // The username of the user attempting to log in by name authentication
    public string Password { get; set; } = string.Empty; // The password of the user attempting to log in by password authentication
}

//{ get; set; } used for automatic property generation in C#. It allows the compiler to create a private backing field for
//the property, enabling you to get and set the value of the property without explicitly defining a field.
//This simplifies code and reduces boilerplate, making it easier to read and maintain.


//(string.Empty;) used for empty string initialization ensures that the properties are never null,
//which can help prevent null reference exceptions during validation or processing.

public class LoginResponse // Represents the data returned to the user after a successful login
{
    public int UserId { get; set; } // The unique identifier of the user who has logged in
    public string Username { get; set; } = string.Empty; // The username of the user who has logged in
    public string FullName { get; set; } = string.Empty; // The full name of the user who has logged in
    public string Role { get; set; } = string.Empty; // The role of the user who has logged in
    public string? AccountNumber { get; set; } // The account number of the user who has logged in, if applicable
                                               // string? used for indicates that the property can be null,
                                               // which is useful for users who may not have an associated account number.
    public string Token { get; set; } = string.Empty; // Mock JWT/Session token
}

public class RegisterCustomerRequest // Represents the data required to register a new customer
{
    public string Username { get; set; } = string.Empty; // The username for the new customer account
    public string Password { get; set; } = string.Empty; // The password for the new customer account
    public string FullName { get; set; } = string.Empty; // The full name of the new customer
    public string NIC { get; set; } = string.Empty; // The National Identity Card number of the new customer
    public string Address { get; set; } = string.Empty; // The address of the new customer
    public string Phone { get; set; } = string.Empty; // The phone number of the new customer
    public string Email { get; set; } = string.Empty; // The email address of the new customer
    public string AreaBranch { get; set; } = "Colombo Central"; // The area branch of the new customer
    public string MeterNumber { get; set; } = string.Empty; // The meter number of the new customer
}

public class CustomerDto // Represents the data of a customer in the water billing system
{
    public int Id { get; set; }  // The unique identifier of the customer
    public string AccountNumber { get; set; } = string.Empty; // The account number of the customer
    public string FullName { get; set; } = string.Empty; // The full name of the customer
    public string NIC { get; set; } = string.Empty; // The National Identity Card number of the customer
    public string Address { get; set; } = string.Empty; // The address of the customer
    public string AreaBranch { get; set; } = string.Empty; // The area branch of the customer
    public string Phone { get; set; } = string.Empty; // The phone number of the customer
    public string Email { get; set; } = string.Empty; // The email address of the customer
    public string MeterNumber { get; set; } = string.Empty; // The meter number of the customer
    public DateTime ConnectionDate { get; set; } // The date when the customer's water connection was established
    public bool IsActive { get; set; } // Indicates whether the customer's account is active
    public decimal CurrentOutstandingBalance { get; set; } // The current outstanding balance of the customer
}

public class WaterUsageDto // Represents the water usage data for a customer
{
    public int Id { get; set; } // The unique identifier of the water usage record
    public string AccountNumber { get; set; } = string.Empty; // The account number of the customer associated with this water usage record
    public string MonthYear { get; set; } = string.Empty; // The month and year of the water usage record, formatted as "YYYY-MM"
    public decimal PreviousReading { get; set; } // The previous meter reading
    public decimal CurrentReading { get; set; } // The current meter reading
    public decimal UnitsConsumed { get; set; } // The number of units of water consumed
    public DateTime ReadingDate { get; set; } // The date when the meter reading was taken
    public string MeterReaderName { get; set; } = string.Empty; // The name of the meter reader
    public string Remarks { get; set; } = string.Empty; // Any additional remarks
}

public class RecordUsageRequest // Represents the data required to record water usage for a customer
{
    public string AccountNumber { get; set; } = string.Empty; // The account number of the customer for whom the water usage is being recorded
    public string MonthYear { get; set; } = string.Empty; // The month and year of the water usage record, formatted as "YYYY-MM"
    public decimal PreviousReading { get; set; } // The previous meter reading
    public decimal CurrentReading { get; set; } // The current meter reading
    public string MeterReaderName { get; set; } = "Officer"; // The name of the meter reader
    public string Remarks { get; set; } = string.Empty; // Any additional remarks
}

public class BillDto // Represents the bill details for a customer
{
    public int Id { get; set; } // The unique identifier of the bill
    public string AccountNumber { get; set; } = string.Empty; // The account number of the customer associated with this bill
    public string CustomerName { get; set; } = string.Empty; // The name of the customer
    public string MonthYear { get; set; } = string.Empty; // The month and year of the bill, formatted as "YYYY-MM"
    public decimal UnitsConsumed { get; set; } // The number of units of water consumed
    public decimal WaterCharge { get; set; } // The charge for the water consumed
    public decimal FixedServiceCharge { get; set; } // The fixed service charge
    public decimal VatAmount { get; set; } // The VAT amount
    public decimal TotalAmount { get; set; } // The total amount of the bill
    public DateTime IssueDate { get; set; } // The date when the bill was issued
    public DateTime DueDate { get; set; } // The date when the bill is due
    public BillStatus Status { get; set; } // The status of the bill
    public DateTime? PaidDate { get; set; } // The date when the bill was paid, if applicable
}

public class GenerateBillRequest // Represents the data required to generate a bill for a customer
{
    public string AccountNumber { get; set; } = string.Empty; // The account number of the customer for whom the bill is being generated
    public string MonthYear { get; set; } = string.Empty; // The month and year of the bill, formatted as "YYYY-MM"
    public decimal UnitsConsumed { get; set; } // The number of units of water consumed
}

public class MakePaymentRequest // Represents the data required to make a payment for a bill
{
    public string AccountNumber { get; set; } = string.Empty; // The account number of the customer making the payment
    public int? BillId { get; set; } // The unique identifier of the bill being paid, if applicable
    public decimal Amount { get; set; } // The amount to be paid
    public PaymentMethod Method { get; set; } = PaymentMethod.CreditCard; // The payment method used for the payment
                                                                          // , defaulting to CreditCard
    public string CardOrAccountNumber { get; set; } = string.Empty; // The card or account number used for the payment
    public string CardExpiry { get; set; } = string.Empty; // The expiry date of the card, if applicable
    public string CardCvv { get; set; } = string.Empty; // The CVV of the card, if applicable
    public string Notes { get; set; } = string.Empty; // Any additional notes for the payment
}

public class PaymentReceiptDto // Represents the receipt details for a payment made by a customer
{
    public string PaymentReference { get; set; } = string.Empty; // The unique reference for the payment
    public int PaymentId { get; set; } // The unique identifier of the payment
    public int? BillId { get; set; } // The unique identifier of the bill associated with the payment, if applicable
                                     //int? used for nullable int, which allows the property to hold either an
                                     //integer value or null.
    public string AccountNumber { get; set; } = string.Empty; // The account number of the customer who made the payment
    public string CustomerName { get; set; } = string.Empty; // The name of the customer who made the payment
    public decimal AmountPaid { get; set; } // The amount paid by the customer
    public DateTime PaymentDate { get; set; } // The date and time when the payment was made
    public string PaymentMethod { get; set; } = string.Empty; // The payment method used for the payment (e.g., CreditCard, BankTransfer, etc.)
    public string Status { get; set; } = string.Empty; // The status of the payment (e.g., Successful, Failed, Pending, etc.)
    public string MaskedCardOrPhone { get; set; } = string.Empty; // The masked card or phone number used for the payment, for security purposes
    public decimal RemainingBalance { get; set; } // The remaining balance on the customer's account after the payment
    public string Message { get; set; } = string.Empty; // Any additional message or note related to the payment receipt
}

public class PaymentHistoryDto // Represents the history of payments made by a customer
{
    public int Id { get; set; } // The unique identifier of the payment history record
    public string PaymentReference { get; set; } = string.Empty; // The unique reference for the payment
    public int? BillId { get; set; } // The unique identifier of the bill associated with the payment, if applicable
    public string AccountNumber { get; set; } = string.Empty; // The account number of the customer who made the payment
    public decimal Amount { get; set; } // The amount paid by the customer
    public DateTime PaymentDate { get; set; } // The date and time when the payment was made
    public string Method { get; set; } = string.Empty; // The payment method used for the payment (e.g., CreditCard, BankTransfer, etc.)
    public string Status { get; set; } = string.Empty; // The status of the payment (e.g., Successful, Failed, Pending, etc.)
    public string MaskedAccount { get; set; } = string.Empty; // The masked account number used for the payment, for security purposes
    public string? PartnerName { get; set; } // The name of the third-party partner involved in the payment, if applicable
}

public class ThirdPartyPaymentRequest // Represents the data required for a third-party partner to initiate a payment on behalf of a customer
{
    public string AccountNumber { get; set; } = string.Empty; // The account number of the customer for whom the payment is being initiated
    public decimal Amount { get; set; } // The amount to be paid
    public string ExternalTransactionId { get; set; } = string.Empty; // The unique identifier for the external transaction
    public string Channel { get; set; } = "BankApi"; // The channel through which the payment is being made (e.g. "ComBank App", "Sampath Vishwa", "eZ Cash")
}

public class ThirdPartyPaymentResponse // Represents the response returned by a third-party partner after processing a payment
{
    public bool Success { get; set; } // Indicates whether the payment was successful
    public string TransactionReference { get; set; } = string.Empty; // The unique reference for the transaction
    public string AccountNumber { get; set; } = string.Empty; // The account number of the customer for whom the payment was processed
    public decimal AmountProcessed { get; set; } // The amount that was processed
    public DateTime ProcessedAt { get; set; } // The date and time when the payment was processed
    public string ConfirmationStatus { get; set; } = string.Empty; // The confirmation status of the payment
    public string Message { get; set; } = string.Empty; // A message providing additional information about the payment
}

public class ThirdPartyPartnerDto // Represents the data of a third-party partner integrated with the water billing system
{
    public int Id { get; set; } // The unique identifier of the third-party partner
    public string PartnerCode { get; set; } = string.Empty; // The code of the third-party partner
    public string PartnerName { get; set; } = string.Empty; // The name of the third-party partner
    public string ApiKey { get; set; } = string.Empty; // The API key used for authenticating with the third-party partner
    public string ContactEmail { get; set; } = string.Empty; // The contact email of the third-party partner
    public string ContactPhone { get; set; } = string.Empty; // The contact phone number of the third-party partner
    public bool IsActive { get; set; } // Indicates whether the third-party partner is active
    public int TotalTransactionsProcessed { get; set; } // The total number of transactions processed by the third-party partner
    public decimal TotalAmountProcessed { get; set; } // The total amount processed by the third-party partner
}

public class RegisterPartnerRequest // Represents the data required to register a new third-party partner with the water billing system
{
    public string PartnerCode { get; set; } = string.Empty; // The code of the third-party partner
    public string PartnerName { get; set; } = string.Empty; // The name of the third-party partner
    public string ContactEmail { get; set; } = string.Empty; // The contact email of the third-party partner
    public string ContactPhone { get; set; } = string.Empty; // The contact phone number of the third-party partner
}

public class AdminDashboardDto // Represents the data displayed on the admin dashboard of the water billing system
{
    public int TotalCustomers { get; set; } // The total number of customers in the system
    public int TotalPendingBills { get; set; } // The total number of pending bills in the system
    public decimal TotalPendingAmount { get; set; } // The total amount of pending bills in the system
    public decimal TotalRevenueCollected { get; set; } // The total revenue collected by the system
    public int TotalTransactionsToday { get; set; } // The total number of transactions processed today
    public decimal TotalAmountToday { get; set; } // The total amount processed today
    public int ActivePartners { get; set; } // The total number of active third-party partners
    public List<PaymentHistoryDto> RecentPayments { get; set; } = new(); // A list of recent payment transactions
}

public class CustomerDashboardDto // Represents the data displayed on the customer dashboard of the water billing system
{
    public CustomerDto Customer { get; set; } = new(); // The customer details
    public BillDto? CurrentBill { get; set; } // The current bill of the customer
    public WaterUsageDto? LatestUsage { get; set; } // The latest water usage of the customer
    public List<BillDto> UnpaidBills { get; set; } = new(); // A list of unpaid bills of the customer
    public List<PaymentHistoryDto> RecentPayments { get; set; } = new(); // A list of recent payment transactions of the customer
}
