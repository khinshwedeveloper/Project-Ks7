using Hotelbooking.Database.Context;
using Hotelbooking.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingMvc.Web.Services;

public class RoomStatus_Service
{
    private readonly AppDbContext _db;

    public RoomStatus_Service(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<TblRoomStatus>> GetRoomStatusesAsync()
    {
        return await _db.TblRoomStatuses
            .OrderByDescending(x => x.StatusId)
            .ToListAsync();
    }

    public async Task<TblRoomStatus?> GetRoomStatusByIdAsync(int id)
    {
        return await _db.TblRoomStatuses
            .FirstOrDefaultAsync(x => x.StatusId == id);
    }

    public async Task<bool> CreateRoomStatusAsync(TblRoomStatus roomStatus)
    {
        try
        {
            bool exists = await _db.TblRoomStatuses
                .AnyAsync(x => x.StatusName == roomStatus.StatusName);

            if (exists)
                return false;

            _db.TblRoomStatuses.Add(roomStatus);

            await _db.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateRoomStatusAsync(TblRoomStatus roomStatus)
    {
        try
        {
            var existingStatus = await _db.TblRoomStatuses
                .FirstOrDefaultAsync(x => x.StatusId == roomStatus.StatusId);

            if (existingStatus == null)
                return false;

            bool duplicate = await _db.TblRoomStatuses
                .AnyAsync(x =>
                    x.StatusName == roomStatus.StatusName &&
                    x.StatusId != roomStatus.StatusId);

            if (duplicate)
                return false;

            existingStatus.StatusName = roomStatus.StatusName;
            existingStatus.Description = roomStatus.Description;
            existingStatus.IsAvailable = roomStatus.IsAvailable;

            await _db.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteRoomStatusAsync(int id)
    {
        try
        {
            var roomStatus = await _db.TblRoomStatuses
                .FirstOrDefaultAsync(x => x.StatusId == id);

            if (roomStatus == null)
                return false;

            _db.TblRoomStatuses.Remove(roomStatus);

            await _db.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }
}