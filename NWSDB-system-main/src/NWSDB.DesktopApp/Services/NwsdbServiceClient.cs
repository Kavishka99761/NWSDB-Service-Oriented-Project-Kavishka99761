using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using NWSDB.Core.DTOs;
using NWSDB.Core.Entities;
using NWSDB.Data;
using NWSDB.Services.Implementations;
using NWSDB.Services.Interfaces;

namespace NWSDB.DesktopApp.Services;

public class NwsdbServiceClient
{
    private readonly HttpClient _http;
    private readonly string _apiBaseUrl = "http://localhost:5000/api/";

    // Direct service fallbacks to guarantee 100% offline & seamless testability
    private readonly NwsdbDbContext _localContext;
    private readonly IAuthService _authService;
    private readonly ICustomerService _customerService;
    private readonly IWaterUsageService _usageService;
    private readonly IBillingService _billingService;
    private readonly IPaymentService _paymentService;
    private readonly IThirdPartyPartnerService _partnerService;
    private readonly ITransactionAuditService _auditService;
    private readonly IReportService _reportService;

    public bool IsApiConnected { get; private set; }

    public NwsdbServiceClient()
    {
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };

        // Initialize local service provider for fallback
        var options = new DbContextOptionsBuilder<NwsdbDbContext>()
            .UseSqlite("Data Source=nwsdb_system.db")
            .Options;

        _localContext = new NwsdbDbContext(options);
        DbInitializer.Initialize(_localContext);

        _auditService = new TransactionAuditService(_localContext);
        _authService = new AuthService(_localContext, _auditService);
        _customerService = new CustomerService(_localContext, _auditService);
        _usageService = new WaterUsageService(_localContext, _auditService);
        _billingService = new BillingService(_localContext, _auditService);
        _paymentService = new PaymentService(_localContext, _auditService);
        _partnerService = new ThirdPartyPartnerService(_localContext, _auditService);
        _reportService = new ReportService(_localContext);
    }

    public async Task<bool> CheckApiConnectionAsync()
    {
        try
        {
            var res = await _http.GetAsync(_apiBaseUrl + "customer/all");
            IsApiConnected = res.IsSuccessStatusCode;
        }
        catch
        {
            IsApiConnected = false;
        }
        return IsApiConnected;
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(string username, string password)
    {
        if (await CheckApiConnectionAsync())
        {
            try
            {
                var response = await _http.PostAsJsonAsync(_apiBaseUrl + "auth/login", new LoginRequest { Username = username, Password = password });
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
                if (result != null) return result;
            }
            catch { }
        }

        // Direct Service fallback
        return await _authService.LoginAsync(new LoginRequest { Username = username, Password = password });
    }

    public async Task<AdminDashboardDto> GetDashboardAsync()
    {
        if (await CheckApiConnectionAsync())
        {
            try
            {
                var res = await _http.GetFromJsonAsync<ApiResponse<AdminDashboardDto>>(_apiBaseUrl + "monitoring/admin-dashboard");
                if (res?.Data != null) return res.Data;
            }
            catch { }
        }

        return await _auditService.GetAdminDashboardSummaryAsync();
    }

    public async Task<List<CustomerDto>> GetAllCustomersAsync()
    {
        if (await CheckApiConnectionAsync())
        {
            try
            {
                var res = await _http.GetFromJsonAsync<ApiResponse<List<CustomerDto>>>(_apiBaseUrl + "customer/all");
                if (res?.Data != null) return res.Data;
            }
            catch { }
        }

        return await _customerService.GetAllCustomersAsync();
    }

    public async Task<ApiResponse<CustomerDto>> CreateCustomerAsync(CustomerDto dto)
    {
        if (await CheckApiConnectionAsync())
        {
            try
            {
                var res = await _http.PostAsJsonAsync(_apiBaseUrl + "customer", dto);
                var apiRes = await res.Content.ReadFromJsonAsync<ApiResponse<CustomerDto>>();
                if (apiRes != null) return apiRes;
            }
            catch { }
        }

        return await _customerService.CreateCustomerAsync(dto);
    }

    public async Task<ApiResponse<CustomerDto>> UpdateCustomerAsync(string accountNumber, CustomerDto dto)
    {
        if (await CheckApiConnectionAsync())
        {
            try
            {
                var res = await _http.PutAsJsonAsync(_apiBaseUrl + $"customer/{accountNumber}", dto);
                var apiRes = await res.Content.ReadFromJsonAsync<ApiResponse<CustomerDto>>();
                if (apiRes != null) return apiRes;
            }
            catch { }
        }

        return await _customerService.UpdateCustomerAsync(accountNumber, dto);
    }

    public async Task<List<WaterUsageDto>> GetAllUsagesAsync()
    {
        if (await CheckApiConnectionAsync())
        {
            try
            {
                var res = await _http.GetFromJsonAsync<ApiResponse<List<WaterUsageDto>>>(_apiBaseUrl + "waterusage/all");
                if (res?.Data != null) return res.Data;
            }
            catch { }
        }

        return await _usageService.GetAllUsagesAsync();
    }

    public async Task<ApiResponse<WaterUsageDto>> RecordUsageAsync(RecordUsageRequest req)
    {
        if (await CheckApiConnectionAsync())
        {
            try
            {
                var res = await _http.PostAsJsonAsync(_apiBaseUrl + "waterusage/record", req);
                var apiRes = await res.Content.ReadFromJsonAsync<ApiResponse<WaterUsageDto>>();
                if (apiRes != null) return apiRes;
            }
            catch { }
        }

        return await _usageService.RecordMeterReadingAsync(req);
    }

    public async Task<List<BillDto>> GetAllBillsAsync()
    {
        if (await CheckApiConnectionAsync())
        {
            try
            {
                var res = await _http.GetFromJsonAsync<ApiResponse<List<BillDto>>>(_apiBaseUrl + "bills/all");
                if (res?.Data != null) return res.Data;
            }
            catch { }
        }

        return await _billingService.GetAllBillsAsync();
    }

    public async Task<ApiResponse<int>> BatchGenerateBillsAsync(string monthYear)
    {
        if (await CheckApiConnectionAsync())
        {
            try
            {
                var res = await _http.PostAsync(_apiBaseUrl + $"bills/batch-generate?monthYear={monthYear}", null);
                var apiRes = await res.Content.ReadFromJsonAsync<ApiResponse<int>>();
                if (apiRes != null) return apiRes;
            }
            catch { }
        }

        return await _billingService.GenerateMonthlyBillsForAllAsync(monthYear);
    }

    public async Task<List<PaymentHistoryDto>> GetAllPaymentsAsync()
    {
        if (await CheckApiConnectionAsync())
        {
            try
            {
                var res = await _http.GetFromJsonAsync<ApiResponse<List<PaymentHistoryDto>>>(_apiBaseUrl + "payments/all");
                if (res?.Data != null) return res.Data;
            }
            catch { }
        }

        return await _paymentService.GetAllPaymentsAsync();
    }

    public async Task<ApiResponse<PaymentReceiptDto>> ProcessPaymentAsync(MakePaymentRequest req)
    {
        if (await CheckApiConnectionAsync())
        {
            try
            {
                var res = await _http.PostAsJsonAsync(_apiBaseUrl + "payments/pay", req);
                var apiRes = await res.Content.ReadFromJsonAsync<ApiResponse<PaymentReceiptDto>>();
                if (apiRes != null) return apiRes;
            }
            catch { }
        }

        return await _paymentService.MakePaymentAsync(req, "DesktopConsole");
    }

    public async Task<List<ThirdPartyPartnerDto>> GetAllPartnersAsync()
    {
        if (await CheckApiConnectionAsync())
        {
            try
            {
                var res = await _http.GetFromJsonAsync<ApiResponse<List<ThirdPartyPartnerDto>>>(_apiBaseUrl + "thirdparty/partners");
                if (res?.Data != null) return res.Data;
            }
            catch { }
        }

        return await _partnerService.GetAllPartnersAsync();
    }

    public async Task<ApiResponse<ThirdPartyPartnerDto>> RegisterPartnerAsync(RegisterPartnerRequest req)
    {
        if (await CheckApiConnectionAsync())
        {
            try
            {
                var res = await _http.PostAsJsonAsync(_apiBaseUrl + "thirdparty/register", req);
                var apiRes = await res.Content.ReadFromJsonAsync<ApiResponse<ThirdPartyPartnerDto>>();
                if (apiRes != null) return apiRes;
            }
            catch { }
        }

        return await _partnerService.RegisterPartnerAsync(req);
    }

    public async Task<ApiResponse<bool>> TogglePartnerStatusAsync(int partnerId)
    {
        if (await CheckApiConnectionAsync())
        {
            try
            {
                var res = await _http.PutAsync(_apiBaseUrl + $"thirdparty/{partnerId}/toggle", null);
                var apiRes = await res.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                if (apiRes != null) return apiRes;
            }
            catch { }
        }

        return await _partnerService.TogglePartnerStatusAsync(partnerId);
    }

    public async Task<List<TransactionAudit>> GetRecentAuditsAsync(int count = 50)
    {
        return await _auditService.GetRecentAuditsAsync(count);
    }

    public async Task<byte[]> ExportPaymentsExcelAsync()
    {
        return await _reportService.GeneratePaymentReportExcelAsync();
    }

    public async Task<byte[]> ExportBillsExcelAsync()
    {
        return await _reportService.GenerateBillingSummaryExcelAsync();
    }
}
