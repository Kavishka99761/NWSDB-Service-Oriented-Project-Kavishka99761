using NWSDB.Core.DTOs;
using NWSDB.Core.Entities;

namespace NWSDB.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<LoginResponse>> RegisterCustomerAsync(RegisterCustomerRequest request);
}

public interface ICustomerService
{
    Task<CustomerDto?> GetByAccountNumberAsync(string accountNumber);
    Task<List<CustomerDto>> GetAllCustomersAsync();
    Task<ApiResponse<CustomerDto>> CreateCustomerAsync(CustomerDto dto);
    Task<ApiResponse<CustomerDto>> UpdateCustomerAsync(string accountNumber, CustomerDto dto);
    Task<decimal> GetOutstandingBalanceAsync(string accountNumber);
}

public interface IWaterUsageService
{
    Task<List<WaterUsageDto>> GetUsageHistoryAsync(string accountNumber);
    Task<List<WaterUsageDto>> GetAllUsagesAsync();
    Task<WaterUsageDto?> GetLatestUsageAsync(string accountNumber);
    Task<ApiResponse<WaterUsageDto>> RecordMeterReadingAsync(RecordUsageRequest request);
}

public interface IBillingService
{
    Task<List<BillDto>> GetBillsByAccountAsync(string accountNumber);
    Task<List<BillDto>> GetPendingBillsAsync();
    Task<List<BillDto>> GetAllBillsAsync();
    Task<BillDto?> GetBillByIdAsync(int billId);
    Task<ApiResponse<BillDto>> GenerateBillForAccountAsync(GenerateBillRequest request);
    Task<ApiResponse<int>> GenerateMonthlyBillsForAllAsync(string monthYear);
}

public interface IPaymentService
{
    Task<ApiResponse<PaymentReceiptDto>> MakePaymentAsync(MakePaymentRequest request, string performedBy = "Customer");
    Task<List<PaymentHistoryDto>> GetPaymentHistoryAsync(string accountNumber);
    Task<List<PaymentHistoryDto>> GetAllPaymentsAsync();
    Task<PaymentReceiptDto?> GetReceiptAsync(string paymentReference);
}

public interface IThirdPartyPartnerService
{
    Task<ThirdPartyPartner?> ValidateApiKeyAsync(string apiKey);
    Task<ApiResponse<ThirdPartyPaymentResponse>> ProcessThirdPartyPaymentAsync(string apiKey, ThirdPartyPaymentRequest request);
    Task<List<ThirdPartyPartnerDto>> GetAllPartnersAsync();
    Task<ApiResponse<ThirdPartyPartnerDto>> RegisterPartnerAsync(RegisterPartnerRequest request);
    Task<ApiResponse<bool>> TogglePartnerStatusAsync(int partnerId);
}

public interface ITransactionAuditService
{
    Task LogAuditAsync(string action, string performedBy, string? accountNumber, string? referenceId, string details, string clientIp = "Local");
    Task<List<TransactionAudit>> GetRecentAuditsAsync(int count = 50);
    Task<AdminDashboardDto> GetAdminDashboardSummaryAsync();
    Task<CustomerDashboardDto?> GetCustomerDashboardSummaryAsync(string accountNumber);
}

public interface IReportService
{
    Task<byte[]> GeneratePaymentReportExcelAsync();
    Task<byte[]> GenerateBillingSummaryExcelAsync();
    Task<byte[]> GenerateCustomerUsageExcelAsync(string accountNumber);
}
