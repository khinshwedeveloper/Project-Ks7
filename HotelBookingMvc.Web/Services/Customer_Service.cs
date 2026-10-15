
using Hotelbooking.Database.Context;
using Hotelbooking.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelBookingMvc.Web.Services;

public class Customer_Service
{
    private readonly AppDbContext _db;
    private readonly ILogger<Customer_Service> _logger;

    public Customer_Service(
        AppDbContext db,
        ILogger<Customer_Service> logger)
    {
        _db = db;
        _logger = logger;
    }

    // Get all customers
    public async Task<List<TblCustomer>> GetCustomersAsync()
    {
        return await _db.TblCustomers
            .AsNoTracking()
            .OrderByDescending(x => x.CustomerId)
            .ToListAsync();
    }

    // Get customer by ID
    public async Task<TblCustomer?> GetCustomerByIdAsync(int id)
    {
        return await _db.TblCustomers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CustomerId == id);
    }

    // Create customer
    public async Task<bool> CreateCustomerAsync(TblCustomer customer)
    {
        try
        {
            // Generate Customer Code if empty
            if (string.IsNullOrWhiteSpace(customer.CustomerCode))
            {
                int maxId = await _db.TblCustomers
                    .Select(x => (int?)x.CustomerId)
                    .MaxAsync() ?? 0;

                customer.CustomerCode = $"CUST-{(maxId + 1):D4}";
            }

            // Check duplicate Customer Code
            bool codeExists = await _db.TblCustomers
                .AnyAsync(x => x.CustomerCode == customer.CustomerCode);

            if (codeExists)
            {
                return false;
            }

            customer.CreatedDate = DateTime.Now;

            _db.TblCustomers.Add(customer);

            await _db.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating customer. CustomerCode: {CustomerCode}",
                customer.CustomerCode);

            throw;
        }
    }

    // Update customer
    public async Task<bool> UpdateCustomerAsync(TblCustomer customer)
    {
        try
        {
            var existingCustomer = await _db.TblCustomers
                .FirstOrDefaultAsync(x => x.CustomerId == customer.CustomerId);

            if (existingCustomer == null)
            {
                return false;
            }

            existingCustomer.Name = customer.Name;
            existingCustomer.Phone = customer.Phone;
            existingCustomer.Email = customer.Email;
            existingCustomer.Address = customer.Address;
            existingCustomer.Nrc = customer.Nrc;
            existingCustomer.Gender = customer.Gender;
            existingCustomer.DateOfBirth = customer.DateOfBirth;

            await _db.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error updating customer. CustomerId: {CustomerId}",
                customer.CustomerId);

            throw;
        }
    }

    // Delete customer
    public async Task<bool> DeleteCustomerAsync(int id)
    {
        try
        {
            var customer = await _db.TblCustomers
                .FirstOrDefaultAsync(x => x.CustomerId == id);

            if (customer == null)
            {
                return false;
            }

            _db.TblCustomers.Remove(customer);

            await _db.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error deleting customer. CustomerId: {CustomerId}",
                id);

            throw;
        }
    }
}

