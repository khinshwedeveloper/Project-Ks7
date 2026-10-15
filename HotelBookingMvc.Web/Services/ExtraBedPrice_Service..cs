
using Hotelbooking.Database.Context;
using Hotelbooking.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingMvc.Web.Services
{
    public class ExtraBedPrice_Service
    {
        private readonly AppDbContext _context;

        public ExtraBedPrice_Service(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TblExtraBedPrice>> GetAllAsync()
        {
            return await _context.TblExtraBedPrices
                .Include(x => x.RoomType)
                .AsNoTracking()
                .OrderByDescending(x => x.EffectiveDate)
                .ToListAsync();
        }

        public async Task<TblExtraBedPrice?> GetByIdAsync(int id)
        {
            return await _context.TblExtraBedPrices
                .Include(x => x.RoomType)
                .FirstOrDefaultAsync(x => x.ExtraBedPriceId == id);
        }

        public async Task<List<TblRoomType>> GetRoomTypesAsync()
        {
            return await _context.TblRoomTypes
                .AsNoTracking()
                .OrderBy(x => x.RoomTypeName)
                .ToListAsync();
        }

        public async Task CreateAsync(TblExtraBedPrice item)
        {
            _context.TblExtraBedPrices.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(TblExtraBedPrice item)
        {
            var existing = await _context.TblExtraBedPrices
                .FindAsync(item.ExtraBedPriceId);

            if (existing == null)
                return false;

            existing.RoomTypeId = item.RoomTypeId;
            existing.Price = item.Price;
            existing.EffectiveDate = item.EffectiveDate;
            existing.Status = item.Status;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _context.TblExtraBedPrices.FindAsync(id);

            if (item == null)
                return false;

            _context.TblExtraBedPrices.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
