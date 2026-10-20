using Microsoft.EntityFrameworkCore; // Importing Entity Framework Core for database context and operations
using NWSDB.Core.Entities; // Importing the core entities used in the application
using NWSDB.Core.Enums; // Importing the enums used in the application
using NWSDB.Core.Logic; // Importing the logic for tariff calculations and other business rules

namespace NWSDB.Data; // Defining the namespace for the data access layer

public static class DbInitializer // Static class responsible for initializing and seeding the database with initial data
{
    public static void Initialize(NwsdbDbContext context) // Method to initialize the database context
    {
        // Ensure database exists and schema is created
        context.Database.EnsureCreated();

        // Check if already seeded
        if (context.Users.Any())
        {
            return; // DB has been seeded
        }

        // 1. Seed Administrator User
        var adminUser = new User //var used for local variable declaration, infers type from the right-hand side
        {
            Username = "admin",
            PasswordHash = "admin123", // For academic demo, simple string or hash
            FullName = "NWSDB Head Administrator",
            Email = "admin@waterboard.gov.lk",
            PhoneNumber = "+94112638888",
            Role = UserRole.Administrator, // Enum value for Administrator role
            CreatedAt = DateTime.UtcNow.AddMonths(-6), // Setting the creation date to 6 months ago
            IsActive = true // Marking the admin user as active
        };
        context.Users.Add(adminUser); // Adding the admin user to the Users DbSet

        // 2. Seed Customer Users & Profiles
        var customer1User = new User
        {
            Username = "sunil",
            PasswordHash = "sunil123",
            FullName = "Sunil Perera",
            Email = "sunil.perera@gmail.com",
            PhoneNumber = "+94771234567",
            Role = UserRole.Customer, // Enum value for Customer role
            CreatedAt = DateTime.UtcNow.AddMonths(-6) // Setting the creation date to 6 months ago
        };

        //3. Seed additional customer users
        var customer2User = new User
        {
            Username = "kamala",
            PasswordHash = "kamala123",
            FullName = "Kamala Jayasinghe",
            Email = "kamala.j@yahoo.com",
            PhoneNumber = "+94719876543",
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow.AddMonths(-5) // Setting the creation date to 5 months ago
        };

        //4. Seed additional customer users 
        var customer3User = new User
        {
            Username = "nimal",
            PasswordHash = "nimal123",
            FullName = "Nimal Fernando",
            Email = "nimal.fernando@outlook.com",
            PhoneNumber = "+94755554433",
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow.AddMonths(-4) // Setting the creation date to 4 months ago
        };

        //5. Seed additional customer users
        var customer4User = new User
        {
            Username = "fathima",
            PasswordHash = "fathima123",
            FullName = "Fathima Rizna",
            Email = "fathima.rizna@gmail.com",
            PhoneNumber = "+94761122334",
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow.AddMonths(-3) // Setting the creation date to 3 months ago
        };

        context.Users.AddRange(customer1User, customer2User, customer3User, customer4User); // Adding all customer users to the Users DbSet
        context.SaveChanges(); // Saving changes to the database to persist the added users

        // Customers
        var c1 = new Customer //c1 mean customer 1
        {
            AccountNumber = "ACC10001",
            FullName = "Sunil Perera",
            NIC = "198214502341",
            Address = "No. 45/2, Galle Road, Colombo 03",
            AreaBranch = "Colombo Central",
            Phone = "+94771234567",
            Email = "sunil.perera@gmail.com",
            MeterNumber = "MTR-COL-8821",
            ConnectionDate = DateTime.UtcNow.AddMonths(-12), // Setting the connection date to 12 months ago
            IsActive = true, // Marking the customer as active
            UserId = customer1User.Id // Linking the customer profile to the corresponding user by setting the UserId
        };

        var c2 = new Customer
        {
            AccountNumber = "ACC10002",
            FullName = "Kamala Jayasinghe",
            NIC = "197568903412",
            Address = "No. 18, Peradeniya Road, Kandy",
            AreaBranch = "Kandy Regional",
            Phone = "+94719876543",
            Email = "kamala.j@yahoo.com",
            MeterNumber = "MTR-KDY-4490",
            ConnectionDate = DateTime.UtcNow.AddMonths(-10), // Setting the connection date to 10 months ago
            IsActive = true,
            UserId = customer2User.Id
        };

        var c3 = new Customer
        {
            AccountNumber = "ACC10003",
            FullName = "Nimal Fernando",
            NIC = "198923405678",
            Address = "No. 102, Main Street, Galle",
            AreaBranch = "Southern Regional",
            Phone = "+94755554433",
            Email = "nimal.fernando@outlook.com",
            MeterNumber = "MTR-GAL-3312",
            ConnectionDate = DateTime.UtcNow.AddMonths(-8),
            IsActive = true,
            UserId = customer3User.Id
        };

        var c4 = new Customer
        {
            AccountNumber = "ACC10004",
            FullName = "Fathima Rizna",
            NIC = "199478201944",
            Address = "No. 22/B, Kandy Road, Gampaha",
            AreaBranch = "Gampaha Regional",
            Phone = "+94761122334",
            Email = "fathima.rizna@gmail.com",
            MeterNumber = "MTR-GAM-7714",
            ConnectionDate = DateTime.UtcNow.AddMonths(-6),
            IsActive = true,
            UserId = customer4User.Id
        };

        context.Customers.AddRange(c1, c2, c3, c4); // Adding all customer profiles to the Customers DbSet
        context.SaveChanges(); // Saving changes to the database to persist the added customers

        // 3. Seed Water Usage History (Month-by-month meter readings)
        // Sunil: July (14 units), August (18 units), September (12 units)
        var u1 = new WaterUsage { AccountNumber = "ACC10001", MonthYear = "2026-07", PreviousReading = 210, CurrentReading = 224, UnitsConsumed = 14, ReadingDate = DateTime.UtcNow.AddMonths(-2), MeterReaderName = "M. S. Bandara", Remarks = "Normal reading" };
        var u2 = new WaterUsage { AccountNumber = "ACC10001", MonthYear = "2026-08", PreviousReading = 224, CurrentReading = 242, UnitsConsumed = 18, ReadingDate = DateTime.UtcNow.AddMonths(-1), MeterReaderName = "M. S. Bandara", Remarks = "Normal reading" };
        var u3 = new WaterUsage { AccountNumber = "ACC10001", MonthYear = "2026-09", PreviousReading = 242, CurrentReading = 254, UnitsConsumed = 12, ReadingDate = DateTime.UtcNow, MeterReaderName = "M. S. Bandara", Remarks = "Normal reading" };

        // Kamala: July (22 units), August (26 units), September (24 units)
        var u4 = new WaterUsage { AccountNumber = "ACC10002", MonthYear = "2026-07", PreviousReading = 550, CurrentReading = 572, UnitsConsumed = 22, ReadingDate = DateTime.UtcNow.AddMonths(-2), MeterReaderName = "T. Gamage", Remarks = "Normal reading" };
        var u5 = new WaterUsage { AccountNumber = "ACC10002", MonthYear = "2026-08", PreviousReading = 572, CurrentReading = 598, UnitsConsumed = 26, ReadingDate = DateTime.UtcNow.AddMonths(-1), MeterReaderName = "T. Gamage", Remarks = "Normal reading" };
        var u6 = new WaterUsage { AccountNumber = "ACC10002", MonthYear = "2026-09", PreviousReading = 598, CurrentReading = 622, UnitsConsumed = 24, ReadingDate = DateTime.UtcNow, MeterReaderName = "T. Gamage", Remarks = "Normal reading" };

        // Nimal: August (8 units), September (10 units)
        var u7 = new WaterUsage { AccountNumber = "ACC10003", MonthYear = "2026-08", PreviousReading = 140, CurrentReading = 148, UnitsConsumed = 8, ReadingDate = DateTime.UtcNow.AddMonths(-1), MeterReaderName = "K. Premadasa", Remarks = "Normal reading" };
        var u8 = new WaterUsage { AccountNumber = "ACC10003", MonthYear = "2026-09", PreviousReading = 148, CurrentReading = 158, UnitsConsumed = 10, ReadingDate = DateTime.UtcNow, MeterReaderName = "K. Premadasa", Remarks = "Normal reading" };

        // Fathima: August (16 units), September (15 units)
        var u9 = new WaterUsage { AccountNumber = "ACC10004", MonthYear = "2026-08", PreviousReading = 80, CurrentReading = 96, UnitsConsumed = 16, ReadingDate = DateTime.UtcNow.AddMonths(-1), MeterReaderName = "S. Silva", Remarks = "Normal reading" };
        var u10 = new WaterUsage { AccountNumber = "ACC10004", MonthYear = "2026-09", PreviousReading = 96, CurrentReading = 111, UnitsConsumed = 15, ReadingDate = DateTime.UtcNow, MeterReaderName = "S. Silva", Remarks = "Normal reading" };

        context.WaterUsages.AddRange(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10); // Adding all water usage records to the WaterUsages DbSet
        context.SaveChanges(); // Saving changes to the database to persist the added water usage records

        // 4. Seed Bills based on Tariff Calculation
        // Sunil July (Paid), August (Paid), September (Pending)
        var billC1_Jul_Tariff = NwsdbTariffCalculator.CalculateDomesticBill(14); // Calculating the tariff for Sunil's July usage
        var b1 = new Bill // Creating a new bill for Sunil's July usage
        {
            AccountNumber = "ACC10001",
            MonthYear = "2026-07",
            UnitsConsumed = 14,
            WaterCharge = billC1_Jul_Tariff.WaterCharge, // Setting the water charge based on the tariff calculation
            FixedServiceCharge = billC1_Jul_Tariff.FixedServiceCharge, // Setting the fixed service charge based on the tariff calculation
            VatAmount = billC1_Jul_Tariff.VatAmount, // Setting the VAT amount based on the tariff calculation
            TotalAmount = billC1_Jul_Tariff.TotalAmount, // Setting the total amount based on the tariff calculation
            IssueDate = DateTime.UtcNow.AddMonths(-2), // Setting the issue date to 2 months ago
            DueDate = DateTime.UtcNow.AddMonths(-2).AddDays(21), // Setting the due date to 21 days after the issue date
            Status = BillStatus.Paid, // Marking the bill as paid
            PaidDate = DateTime.UtcNow.AddMonths(-2).AddDays(10) // Setting the paid date to 10 days after the issue date
        };

        var billC1_Aug_Tariff = NwsdbTariffCalculator.CalculateDomesticBill(18); // Calculating the tariff for Sunil's August usage
        var b2 = new Bill
        {
            AccountNumber = "ACC10001",
            MonthYear = "2026-08",
            UnitsConsumed = 18,
            WaterCharge = billC1_Aug_Tariff.WaterCharge,
            FixedServiceCharge = billC1_Aug_Tariff.FixedServiceCharge,
            VatAmount = billC1_Aug_Tariff.VatAmount,
            TotalAmount = billC1_Aug_Tariff.TotalAmount,
            IssueDate = DateTime.UtcNow.AddMonths(-1),
            DueDate = DateTime.UtcNow.AddMonths(-1).AddDays(21),
            Status = BillStatus.Paid,
            PaidDate = DateTime.UtcNow.AddMonths(-1).AddDays(15)
        };

        var billC1_Sep_Tariff = NwsdbTariffCalculator.CalculateDomesticBill(12); //
        var b3 = new Bill
        {
            AccountNumber = "ACC10001",
            MonthYear = "2026-09",
            UnitsConsumed = 12,
            WaterCharge = billC1_Sep_Tariff.WaterCharge,
            FixedServiceCharge = billC1_Sep_Tariff.FixedServiceCharge,
            VatAmount = billC1_Sep_Tariff.VatAmount,
            TotalAmount = billC1_Sep_Tariff.TotalAmount,
            IssueDate = DateTime.UtcNow.AddDays(-5),
            DueDate = DateTime.UtcNow.AddDays(16),
            Status = BillStatus.Pending
        };

        // Kamala July (Paid), August (Paid), September (Pending)
        var billC2_Jul_Tariff = NwsdbTariffCalculator.CalculateDomesticBill(22);
        var b4 = new Bill
        {
            AccountNumber = "ACC10002",
            MonthYear = "2026-07",
            UnitsConsumed = 22,
            WaterCharge = billC2_Jul_Tariff.WaterCharge,
            FixedServiceCharge = billC2_Jul_Tariff.FixedServiceCharge,
            VatAmount = billC2_Jul_Tariff.VatAmount,
            TotalAmount = billC2_Jul_Tariff.TotalAmount,
            IssueDate = DateTime.UtcNow.AddMonths(-2),
            DueDate = DateTime.UtcNow.AddMonths(-2).AddDays(21),
            Status = BillStatus.Paid,
            PaidDate = DateTime.UtcNow.AddMonths(-2).AddDays(12)
        };

        var billC2_Aug_Tariff = NwsdbTariffCalculator.CalculateDomesticBill(26);
        var b5 = new Bill
        {
            AccountNumber = "ACC10002",
            MonthYear = "2026-08",
            UnitsConsumed = 26,
            WaterCharge = billC2_Aug_Tariff.WaterCharge,
            FixedServiceCharge = billC2_Aug_Tariff.FixedServiceCharge,
            VatAmount = billC2_Aug_Tariff.VatAmount,
            TotalAmount = billC2_Aug_Tariff.TotalAmount,
            IssueDate = DateTime.UtcNow.AddMonths(-1),
            DueDate = DateTime.UtcNow.AddMonths(-1).AddDays(21),
            Status = BillStatus.Paid,
            PaidDate = DateTime.UtcNow.AddMonths(-1).AddDays(8)
        };

        var billC2_Sep_Tariff = NwsdbTariffCalculator.CalculateDomesticBill(24);
        var b6 = new Bill
        {
            AccountNumber = "ACC10002",
            MonthYear = "2026-09",
            UnitsConsumed = 24,
            WaterCharge = billC2_Sep_Tariff.WaterCharge,
            FixedServiceCharge = billC2_Sep_Tariff.FixedServiceCharge,
            VatAmount = billC2_Sep_Tariff.VatAmount,
            TotalAmount = billC2_Sep_Tariff.TotalAmount,
            IssueDate = DateTime.UtcNow.AddDays(-6),
            DueDate = DateTime.UtcNow.AddDays(15),
            Status = BillStatus.Pending
        };

        // Nimal August (Paid), September (Pending)
        var billC3_Aug_Tariff = NwsdbTariffCalculator.CalculateDomesticBill(8);
        var b7 = new Bill
        {
            AccountNumber = "ACC10003",
            MonthYear = "2026-08",
            UnitsConsumed = 8,
            WaterCharge = billC3_Aug_Tariff.WaterCharge,
            FixedServiceCharge = billC3_Aug_Tariff.FixedServiceCharge,
            VatAmount = billC3_Aug_Tariff.VatAmount,
            TotalAmount = billC3_Aug_Tariff.TotalAmount,
            IssueDate = DateTime.UtcNow.AddMonths(-1),
            DueDate = DateTime.UtcNow.AddMonths(-1).AddDays(21),
            Status = BillStatus.Paid,
            PaidDate = DateTime.UtcNow.AddMonths(-1).AddDays(14)
        };

        var billC3_Sep_Tariff = NwsdbTariffCalculator.CalculateDomesticBill(10);
        var b8 = new Bill
        {
            AccountNumber = "ACC10003",
            MonthYear = "2026-09",
            UnitsConsumed = 10,
            WaterCharge = billC3_Sep_Tariff.WaterCharge,
            FixedServiceCharge = billC3_Sep_Tariff.FixedServiceCharge,
            VatAmount = billC3_Sep_Tariff.VatAmount,
            TotalAmount = billC3_Sep_Tariff.TotalAmount,
            IssueDate = DateTime.UtcNow.AddDays(-4),
            DueDate = DateTime.UtcNow.AddDays(17),
            Status = BillStatus.Pending
        };

        // Fathima September (Pending)
        var billC4_Sep_Tariff = NwsdbTariffCalculator.CalculateDomesticBill(15);
        var b9 = new Bill
        {
            AccountNumber = "ACC10004",
            MonthYear = "2026-09",
            UnitsConsumed = 15,
            WaterCharge = billC4_Sep_Tariff.WaterCharge,
            FixedServiceCharge = billC4_Sep_Tariff.FixedServiceCharge,
            VatAmount = billC4_Sep_Tariff.VatAmount,
            TotalAmount = billC4_Sep_Tariff.TotalAmount,
            IssueDate = DateTime.UtcNow.AddDays(-3),
            DueDate = DateTime.UtcNow.AddDays(18),
            Status = BillStatus.Pending
        };

        context.Bills.AddRange(b1, b2, b3, b4, b5, b6, b7, b8, b9);
        context.SaveChanges();

        // 5. Seed Third-Party Partners
        var p1 = new ThirdPartyPartner
        {
            PartnerCode = "COMBANK",
            PartnerName = "Commercial Bank (ComBank Online)",
            ApiKey = "NWSDB-COMBANK-KEY-778899",
            ContactEmail = "api-integration@combank.net",
            ContactPhone = "+94112486000",
            IsActive = true
        };

        var p2 = new ThirdPartyPartner
        {
            PartnerCode = "SAMPATH",
            PartnerName = "Sampath Bank (Sampath Vishwa)",
            ApiKey = "NWSDB-SAMPATH-KEY-112233",
            ContactEmail = "partners@sampath.lk",
            ContactPhone = "+94112303050",
            IsActive = true
        };

        var p3 = new ThirdPartyPartner
        {
            PartnerCode = "EZCASH",
            PartnerName = "Dialog Axiata (eZ Cash)",
            ApiKey = "NWSDB-EZCASH-KEY-445566",
            ContactEmail = "support@ezcash.lk",
            ContactPhone = "+94777678678",
            IsActive = true
        };

        context.ThirdPartyPartners.AddRange(p1, p2, p3);
        context.SaveChanges();

        // 6. Seed Payment Records
        var pay1 = new Payment
        {
            PaymentReference = "NWSDB-PAY-20260715-001",
            BillId = b1.Id,
            AccountNumber = "ACC10001",
            Amount = b1.TotalAmount,
            PaymentDate = DateTime.UtcNow.AddMonths(-2).AddDays(10),
            Method = PaymentMethod.CreditCard,
            Status = PaymentStatus.Success,
            MaskedCardOrPhone = "**** **** **** 4112",
            TransactionNotes = "Paid via NWSDB Web Portal"
        };

        var pay2 = new Payment
        {
            PaymentReference = "NWSDB-PAY-20260818-002",
            BillId = b2.Id,
            AccountNumber = "ACC10001",
            Amount = b2.TotalAmount,
            PaymentDate = DateTime.UtcNow.AddMonths(-1).AddDays(15),
            Method = PaymentMethod.OnlineBanking,
            Status = PaymentStatus.Success,
            MaskedCardOrPhone = "ACC-****-8812",
            TransactionNotes = "Paid via Internet Banking",
            ThirdPartyPartnerId = p1.Id
        };

        var pay3 = new Payment
        {
            PaymentReference = "NWSDB-PAY-20260714-003",
            BillId = b4.Id,
            AccountNumber = "ACC10002",
            Amount = b4.TotalAmount,
            PaymentDate = DateTime.UtcNow.AddMonths(-2).AddDays(12),
            Method = PaymentMethod.PartnerApp,
            Status = PaymentStatus.Success,
            MaskedCardOrPhone = "+9471****543",
            TransactionNotes = "Paid via eZ Cash Partner App",
            ThirdPartyPartnerId = p3.Id
        };

        var pay4 = new Payment
        {
            PaymentReference = "NWSDB-PAY-20260810-004",
            BillId = b5.Id,
            AccountNumber = "ACC10002",
            Amount = b5.TotalAmount,
            PaymentDate = DateTime.UtcNow.AddMonths(-1).AddDays(8),
            Method = PaymentMethod.PartnerApp,
            Status = PaymentStatus.Success,
            MaskedCardOrPhone = "ACC-****-3050",
            TransactionNotes = "Paid via Sampath Vishwa API",
            ThirdPartyPartnerId = p2.Id
        };

        var pay5 = new Payment
        {
            PaymentReference = "NWSDB-PAY-20260820-005",
            BillId = b7.Id,
            AccountNumber = "ACC10003",
            Amount = b7.TotalAmount,
            PaymentDate = DateTime.UtcNow.AddMonths(-1).AddDays(14),
            Method = PaymentMethod.CreditCard,
            Status = PaymentStatus.Success,
            MaskedCardOrPhone = "**** **** **** 9081",
            TransactionNotes = "Paid via NWSDB Online Payment Portal"
        };

        context.Payments.AddRange(pay1, pay2, pay3, pay4, pay5); // Adding all payment records to the Payments DbSet

        // 7. Seed Initial Transaction Audit Logs
        context.TransactionAudits.AddRange(
            new TransactionAudit { Timestamp = DateTime.UtcNow.AddMonths(-2), Action = "SystemInitialized", PerformedBy = "System", Details = "Initial database configuration and seed data created." },
            new TransactionAudit { Timestamp = DateTime.UtcNow.AddMonths(-2).AddDays(10), Action = "BillPayment", PerformedBy = "ACC10001", AccountNumber = "ACC10001", ReferenceId = pay1.PaymentReference, Details = $"Payment of Rs. {pay1.Amount} received." },
            new TransactionAudit { Timestamp = DateTime.UtcNow.AddMonths(-1).AddDays(15), Action = "PartnerPayment", PerformedBy = "COMBANK", AccountNumber = "ACC10001", ReferenceId = pay2.PaymentReference, Details = $"Partner ComBank forwarded Rs. {pay2.Amount}." },
            new TransactionAudit { Timestamp = DateTime.UtcNow.AddDays(-5), Action = "BillGenerated", PerformedBy = "BillingEngine", AccountNumber = "ACC10001", ReferenceId = $"Bill-{b3.Id}", Details = "Monthly bill generated for 2026-09." }
        );

        context.SaveChanges(); // Saving all changes to the database to persist the added payments and transaction audits
    }
}
