namespace NWSDB.Core.Enums; // Defining the namespace for the application enums

public enum UserRole // Represents the different roles a user can have in the system
{
    Customer, 
    Administrator
}

public enum BillStatus // Represents the different statuses a bill can have in the system
{
    Pending,
    Paid,
    Overdue
}

public enum PaymentMethod // Represents the different payment methods available in the system
{
    CreditCard,
    DebitCard,
    OnlineBanking,
    PartnerApp,
    CashCounter
}

public enum PaymentStatus // Represents the different statuses a payment can have in the system
{
    Pending,
    Success,
    Failed
}
