using Hotelbooking.Database.Context;
using Hotelbooking.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingMvc.Web.Services;

public class Customer_Service
{
    private readonly AppDbContext _db;

    public Customer_Service(AppDbContext db)
    {
        _db = db;
    }

    // Get all customers
    public async Task<List<TblCustomer>> GetCustomersAsync()
    {
        return await _db.TblCustomers
            .OrderByDescending(x => x.CustomerId)
            .ToListAsync();
    }

    // Get customer by ID
    public async Task<TblCustomer?> GetCustomerByIdAsync(int id)
    {
        return await _db.TblCustomers
            .FirstOrDefaultAsync(x => x.CustomerId == id);
    }

    // Create customer
    public async Task<bool> CreateCustomerAsync(TblCustomer customer)
    {
        try
        {
            customer.CreateDatetime = DateTime.Now;

            _db.TblCustomers.Add(customer);

            await _db.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    // Update customer
    public async Task<bool> UpdateCustomerAsync(TblCustomer customer)
    {
        try
        {
            var existingCustomer = await _db.TblCustomers
                .FirstOrDefaultAsync(
                    x => x.CustomerId == customer.CustomerId);

            if (existingCustomer == null)
            {
                return false;
            }

            existingCustomer.CustomerName = customer.CustomerName;
            existingCustomer.Phone = customer.Phone;
            existingCustomer.Email = customer.Email;
            existingCustomer.Address = customer.Address;
            existingCustomer.Nrc = customer.Nrc;

            await _db.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
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
        catch
        {
            return false;
        }
    }
}