using Microsoft.EntityFrameworkCore; //import the Entity Framework Core namespace for database context and entity configurations
using NWSDB.Core.Entities; //import the namespace containing the entity classes for the application

namespace NWSDB.Data; //define the namespace for the data access layer of the application

public class NwsdbDbContext : DbContext //define the database context class that inherits from DbContext
{
    public NwsdbDbContext(DbContextOptions<NwsdbDbContext> options) : base(options) //constructor that accepts DbContextOptions
                                                                                    //and passes them to the base DbContext class
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<WaterUsage> WaterUsages => Set<WaterUsage>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ThirdPartyPartner> ThirdPartyPartners => Set<ThirdPartyPartner>();
    public DbSet<TransactionAudit> TransactionAudits => Set<TransactionAudit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) //override the OnModelCreating method to configure entity mappings and relationships
    {
        base.OnModelCreating(modelBuilder); //call the base implementation of OnModelCreating

        // User indexes
        modelBuilder.Entity<User>() //configure the User entity
            .HasIndex(u => u.Username)
            .IsUnique();

        // Customer indexes
        modelBuilder.Entity<Customer>() //configure the Customer entity
            .HasIndex(c => c.AccountNumber)
            .IsUnique();

        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.NIC);

        // Third party partner indexes
        modelBuilder.Entity<ThirdPartyPartner>() //configure the ThirdPartyPartner entity
            .HasIndex(p => p.PartnerCode)
            .IsUnique();

        modelBuilder.Entity<ThirdPartyPartner>()
            .HasIndex(p => p.ApiKey)
            .IsUnique();

        // Decimal precisions
        modelBuilder.Entity<WaterUsage>() //configure the WaterUsage entity
            .Property(u => u.PreviousReading).HasPrecision(18, 2);
        modelBuilder.Entity<WaterUsage>()
            .Property(u => u.CurrentReading).HasPrecision(18, 2);
        modelBuilder.Entity<WaterUsage>()
            .Property(u => u.UnitsConsumed).HasPrecision(18, 2);

        modelBuilder.Entity<Bill>() //configure the Bill entity
            .Property(b => b.UnitsConsumed).HasPrecision(18, 2);
        modelBuilder.Entity<Bill>()
            .Property(b => b.WaterCharge).HasPrecision(18, 2);
        modelBuilder.Entity<Bill>()
            .Property(b => b.FixedServiceCharge).HasPrecision(18, 2);
        modelBuilder.Entity<Bill>()
            .Property(b => b.VatAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Bill>()
            .Property(b => b.TotalAmount).HasPrecision(18, 2);

        modelBuilder.Entity<Payment>() //configure the Payment entity
            .Property(p => p.Amount).HasPrecision(18, 2);
    }
}
