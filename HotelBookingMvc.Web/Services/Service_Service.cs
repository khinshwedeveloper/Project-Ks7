using Hotelbooking.Database.Context;
using Hotelbooking.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingMvc.Web.Services
{
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
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.ServiceName)
                .ToListAsync();
        }

        public async Task<TblService?> GetServiceByIdAsync(int id)
        {
            return await _db.TblServices.FirstOrDefaultAsync(x => x.ServiceId == id && !x.IsDelete);
        }

        // Create service
        public async Task<bool> CreateServiceAsync(TblService service)
        {
            try
            {
                int maxId = await _db.TblServices
                    .Select(x => (int?)x.ServiceId)
                    .MaxAsync() ?? 0;

                service.ServiceId = maxId + 1;

                service.IsDelete = false;

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
                var existing = await _db.TblServices.FirstOrDefaultAsync(x => x.ServiceId == service.ServiceId && !x.IsDelete);
                if (existing == null)
                    return false;

                existing.ServiceName = service.ServiceName;
                existing.Description = service.Description;
                existing.Price = service.Price;
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
                var service = await _db.TblServices.FirstAsync(x => x.ServiceId == id && !x.IsDelete);
                if (service == null)
                    return false;
                service.IsDelete = true;
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
