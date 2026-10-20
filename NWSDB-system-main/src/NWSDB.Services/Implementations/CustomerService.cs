using Microsoft.EntityFrameworkCore;
using NWSDB.Core.DTOs;
using NWSDB.Core.Entities;
using NWSDB.Core.Enums;
using NWSDB.Data;
using NWSDB.Services.Interfaces;

namespace NWSDB.Services.Implementations;

public class CustomerService : ICustomerService
{
    private readonly NwsdbDbContext _context;
    private readonly ITransactionAuditService _auditService;

    public CustomerService(NwsdbDbContext context, ITransactionAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<CustomerDto?> GetByAccountNumberAsync(string accountNumber)
    {
        var c = await _context.Customers
            .Include(c => c.Bills)
            .FirstOrDefaultAsync(c => c.AccountNumber.ToUpper() == accountNumber.Trim().ToUpper());

        if (c == null) return null;

        decimal outstanding = c.Bills
            .Where(b => b.Status != BillStatus.Paid)
            .Sum(b => b.TotalAmount);

        return new CustomerDto
        {
            Id = c.Id,
            AccountNumber = c.AccountNumber,
            FullName = c.FullName,
            NIC = c.NIC,
            Address = c.Address,
            AreaBranch = c.AreaBranch,
            Phone = c.Phone,
            Email = c.Email,
            MeterNumber = c.MeterNumber,
            ConnectionDate = c.ConnectionDate,
            IsActive = c.IsActive,
            CurrentOutstandingBalance = outstanding
        };
    }

    public async Task<List<CustomerDto>> GetAllCustomersAsync()
    {
        var customers = await _context.Customers
            .Include(c => c.Bills)
            .OrderBy(c => c.AccountNumber)
            .ToListAsync();

        return customers.Select(c => new CustomerDto
        {
            Id = c.Id,
            AccountNumber = c.AccountNumber,
            FullName = c.FullName,
            NIC = c.NIC,
            Address = c.Address,
            AreaBranch = c.AreaBranch,
            Phone = c.Phone,
            Email = c.Email,
            MeterNumber = c.MeterNumber,
            ConnectionDate = c.ConnectionDate,
            IsActive = c.IsActive,
            CurrentOutstandingBalance = c.Bills
                .Where(b => b.Status != BillStatus.Paid)
                .Sum(b => b.TotalAmount)
        }).ToList();
    }

    public async Task<ApiResponse<CustomerDto>> CreateCustomerAsync(CustomerDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.NIC))
        {
            return ApiResponse<CustomerDto>.Fail("Full name and NIC are required.");
        }

        int count = await _context.Customers.CountAsync();
        string newAccountNo = string.IsNullOrWhiteSpace(dto.AccountNumber)
            ? $"ACC{10001 + count}"
            : dto.AccountNumber.Trim().ToUpper();

        if (await _context.Customers.AnyAsync(c => c.AccountNumber == newAccountNo))
        {
            return ApiResponse<CustomerDto>.Fail($"Account number {newAccountNo} already exists.");
        }

        var customer = new Customer
        {
            AccountNumber = newAccountNo,
            FullName = dto.FullName.Trim(),
            NIC = dto.NIC.Trim(),
            Address = dto.Address.Trim(),
            AreaBranch = string.IsNullOrWhiteSpace(dto.AreaBranch) ? "Colombo Central" : dto.AreaBranch.Trim(),
            Phone = dto.Phone.Trim(),
            Email = dto.Email.Trim(),
            MeterNumber = string.IsNullOrWhiteSpace(dto.MeterNumber) ? $"MTR-{new Random().Next(1000, 9999)}" : dto.MeterNumber.Trim(),
            ConnectionDate = DateTime.UtcNow,
            IsActive = true
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        await _auditService.LogAuditAsync("CustomerCreated", "Admin", newAccountNo, $"Customer-{customer.Id}", $"Customer {customer.FullName} created.");

        dto.Id = customer.Id;
        dto.AccountNumber = customer.AccountNumber;
        dto.MeterNumber = customer.MeterNumber;
        dto.IsActive = customer.IsActive;

        return ApiResponse<CustomerDto>.Ok(dto, "Customer created successfully.");
    }

    public async Task<ApiResponse<CustomerDto>> UpdateCustomerAsync(string accountNumber, CustomerDto dto)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.AccountNumber == accountNumber);
        if (customer == null)
        {
            return ApiResponse<CustomerDto>.Fail("Customer account not found.");
        }

        customer.FullName = dto.FullName.Trim();
        customer.Address = dto.Address.Trim();
        customer.Phone = dto.Phone.Trim();
        customer.Email = dto.Email.Trim();
        customer.AreaBranch = dto.AreaBranch.Trim();
        customer.MeterNumber = dto.MeterNumber.Trim();
        customer.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        await _auditService.LogAuditAsync("CustomerUpdated", "Admin", accountNumber, $"Customer-{customer.Id}", $"Customer {customer.AccountNumber} details updated.");

        return ApiResponse<CustomerDto>.Ok(dto, "Customer updated successfully.");
    }

    public async Task<decimal> GetOutstandingBalanceAsync(string accountNumber)
    {
        return await _context.Bills
            .Where(b => b.AccountNumber == accountNumber && b.Status != BillStatus.Paid)
            .SumAsync(b => b.TotalAmount);
    }
}
