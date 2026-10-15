using Hotelbooking.Database.Context;
using Hotelbooking.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingMvc.Web.Services;

public class Service_Service
{
    private readonly AppDbContext _db;

    public Service_Service(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<TblService>> GetServicesAsync()
    {
        return await _db.TblServices
            .Include(x => x.ServiceCategory)
            .OrderBy(x => x.ServiceName)
            .ToListAsync();
    }

    public async Task<TblService?> GetServiceByIdAsync(int id)
    {
        return await _db.TblServices
            .Include(x => x.ServiceCategory)
            .FirstOrDefaultAsync(x => x.ServiceId == id);
    }

    public async Task<bool> CreateServiceAsync(TblService service)
    {
        try
        {
            service.Status = true;
            _db.TblServices.Add(service);
            await _db.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateServiceAsync(TblService service)
    {
        try
        {
            var existing = await _db.TblServices.FirstOrDefaultAsync(x => x.ServiceId == service.ServiceId);
            if (existing == null)
                return false;

            existing.ServiceName = service.ServiceName;
            existing.ServiceCategoryId = service.ServiceCategoryId;
            existing.Description = service.Description;
            existing.Price = service.Price;
            existing.Status = service.Status;

            await _db.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteServiceAsync(int id)
    {
        try
        {
            var service = await _db.TblServices.FirstOrDefaultAsync(x => x.ServiceId == id);
            if (service == null)
                return false;

            _db.TblServices.Remove(service);
            await _db.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
