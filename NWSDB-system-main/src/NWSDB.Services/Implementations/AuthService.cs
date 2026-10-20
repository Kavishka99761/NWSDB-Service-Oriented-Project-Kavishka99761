using Microsoft.EntityFrameworkCore;
using NWSDB.Core.DTOs;
using NWSDB.Core.Entities;
using NWSDB.Core.Enums;
using NWSDB.Data;
using NWSDB.Services.Interfaces;

namespace NWSDB.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly NwsdbDbContext _context;
    private readonly ITransactionAuditService _auditService;

    public AuthService(NwsdbDbContext context, ITransactionAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return ApiResponse<LoginResponse>.Fail("Username and password are required.");
        }

        var user = await _context.Users
            .Include(u => u.CustomerProfile)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.Trim().ToLower());

        if (user == null || user.PasswordHash != request.Password)
        {
            return ApiResponse<LoginResponse>.Fail("Invalid username or password.");
        }

        if (!user.IsActive)
        {
            return ApiResponse<LoginResponse>.Fail("This account has been deactivated.");
        }

        string? accountNo = user.CustomerProfile?.AccountNumber;
        if (string.IsNullOrEmpty(accountNo) && user.Role == UserRole.Customer)
        {
            var cust = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            accountNo = cust?.AccountNumber;
        }

        await _auditService.LogAuditAsync(
            "UserLogin",
            user.Username,
            accountNo,
            $"User-{user.Id}",
            $"User {user.Username} ({user.Role}) logged in successfully."
        );

        var response = new LoginResponse
        {
            UserId = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            AccountNumber = accountNo,
            Token = $"NWSDB-SESSION-{Guid.NewGuid():N}"
        };

        return ApiResponse<LoginResponse>.Ok(response, "Login successful.");
    }

    public async Task<ApiResponse<LoginResponse>> RegisterCustomerAsync(RegisterCustomerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return ApiResponse<LoginResponse>.Fail("Username and password are required.");
        }

        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.NIC))
        {
            return ApiResponse<LoginResponse>.Fail("Full name and NIC are required.");
        }

        bool userExists = await _context.Users.AnyAsync(u => u.Username.ToLower() == request.Username.Trim().ToLower());
        if (userExists)
        {
            return ApiResponse<LoginResponse>.Fail("Username is already taken.");
        }

        // Generate next account number: ACC10001 + count
        int count = await _context.Customers.CountAsync();
        string newAccountNo = $"ACC{10001 + count}";
        string meterNo = string.IsNullOrWhiteSpace(request.MeterNumber)
            ? $"MTR-{new Random().Next(1000, 9999)}"
            : request.MeterNumber;

        var user = new User
        {
            Username = request.Username.Trim(),
            PasswordHash = request.Password, // plain or hashed
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            PhoneNumber = request.Phone.Trim(),
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var customer = new Customer
        {
            AccountNumber = newAccountNo,
            FullName = request.FullName.Trim(),
            NIC = request.NIC.Trim(),
            Address = request.Address.Trim(),
            AreaBranch = string.IsNullOrWhiteSpace(request.AreaBranch) ? "Colombo Central" : request.AreaBranch.Trim(),
            Phone = request.Phone.Trim(),
            Email = request.Email.Trim(),
            MeterNumber = meterNo,
            ConnectionDate = DateTime.UtcNow,
            IsActive = true,
            UserId = user.Id
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        await _auditService.LogAuditAsync(
            "CustomerRegistered",
            user.Username,
            newAccountNo,
            $"Customer-{customer.Id}",
            $"Customer {customer.FullName} registered with account {newAccountNo}."
        );

        var response = new LoginResponse
        {
            UserId = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Role = UserRole.Customer.ToString(),
            AccountNumber = newAccountNo,
            Token = $"NWSDB-SESSION-{Guid.NewGuid():N}"
        };

        return ApiResponse<LoginResponse>.Ok(response, "Registration successful.");
    }
}
