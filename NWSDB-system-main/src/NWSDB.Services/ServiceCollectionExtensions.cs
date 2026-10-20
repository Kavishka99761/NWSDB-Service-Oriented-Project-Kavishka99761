using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NWSDB.Data;
using NWSDB.Services.Implementations;
using NWSDB.Services.Interfaces;

namespace NWSDB.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNwsdbServices(this IServiceCollection services, string connectionString = "Data Source=nwsdb_system.db")
    {
        services.AddDbContext<NwsdbDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });

        services.AddScoped<ITransactionAuditService, TransactionAuditService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IWaterUsageService, WaterUsageService>();
        services.AddScoped<IBillingService, BillingService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IThirdPartyPartnerService, ThirdPartyPartnerService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
