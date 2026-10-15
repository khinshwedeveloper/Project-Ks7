using Hotelbooking.Database.Context;
using Hotelbooking.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingMvc.Web.Services;

public class RoomType_Service
{
    private readonly AppDbContext _db;

    public RoomType_Service(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<TblRoomType>> GetRoomTypesAsync()
    {
        return await _db.TblRoomTypes
            .Include(x => x.TblRoomPrices)
                .ThenInclude(p => p.TblRoomPriceDetails)
            .OrderBy(x => x.RoomTypeId)
            .ToListAsync();
    }

    public async Task<TblRoomType?> GetRoomTypeByIdAsync(int id)
    {
        return await _db.TblRoomTypes
            .Include(x => x.TblRoomPrices)
                .ThenInclude(p => p.TblRoomPriceDetails)
            .FirstOrDefaultAsync(x => x.RoomTypeId == id);
    }

    public async Task<bool> CreateRoomTypeAsync(TblRoomType roomType)
    {
        try
        {
            roomType.Status = true;
            _db.TblRoomTypes.Add(roomType);
            await _db.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateRoomTypeAsync(TblRoomType roomType)
    {
        try
        {
            var existing = await _db.TblRoomTypes
                .FirstOrDefaultAsync(x => x.RoomTypeId == roomType.RoomTypeId);

            if (existing == null)
                return false;

            existing.RoomTypeName = roomType.RoomTypeName;
            existing.Description = roomType.Description;
            existing.MaxGuest = roomType.MaxGuest;
            existing.Status = roomType.Status;

            await _db.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteRoomTypeAsync(int id)
    {
        try
        {
            var roomType = await _db.TblRoomTypes
                .FirstOrDefaultAsync(x => x.RoomTypeId == id);

            if (roomType == null)
                return false;

            _db.TblRoomTypes.Remove(roomType);
            await _db.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }
}
