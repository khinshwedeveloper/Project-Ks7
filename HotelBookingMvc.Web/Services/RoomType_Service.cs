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
        return await _db.TblRoomTypes.OrderBy(x => x.RoomTypeId).ToListAsync();
    }
    public async Task<TblRoomType> GetRoomTypeByIdAsync(int id)
    {
        return await _db.TblRoomTypes.FirstOrDefaultAsync(x => x.RoomTypeId == id);
    }
    public async Task<bool> CreateRoomTypeAsync(TblRoomType roomType)
    {
        try
        {
            int maxId = await _db.TblRoomTypes.Select(x => (int?)x.RoomTypeId).MaxAsync() ?? 0;

            roomType.RoomTypeId = maxId + 1;
            roomType.CreatedDateTime = DateTime.Now;
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
            var existing = await _db.TblRoomTypes.FirstOrDefaultAsync(x => x.RoomTypeId == roomType.RoomTypeId);
            if (existing == null)
                return false;

            existing.RoomTypeName = roomType.RoomTypeName;
            existing.Description = roomType.Description;
            existing.PricePerNight = roomType.PricePerNight;

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
            var roomtype = await _db.TblRoomTypes.FirstOrDefaultAsync(x => x.RoomTypeId == id);
            if (roomtype == null)
                return false;
            _db.TblRoomTypes.Remove(roomtype);
            await _db.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }
}
