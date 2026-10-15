using Hotelbooking.Database.Context;
using Hotelbooking.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingMvc.Web.Services;

public class Room_Service
{
    private readonly AppDbContext _db;

    public Room_Service(AppDbContext db)
    {
        _db = db;
    }

    // Get all rooms
    public async Task<List<TblRoom>> GetRoomsAsync()
    {
        return await _db.TblRooms
            .Include(x => x.Floor)
            .Include(x => x.RoomType)
            .Include(x => x.Status)
            .OrderBy(x => x.RoomNo)
            .ToListAsync();
    }

    // Get room by ID
    public async Task<TblRoom?> GetRoomByIdAsync(int id)
    {
        return await _db.TblRooms
            .Include(x => x.Floor)
            .Include(x => x.RoomType)
            .Include(x => x.Status)
            .FirstOrDefaultAsync(x => x.RoomId == id);
    }

    // Create room
    public async Task<bool> CreateRoomAsync(TblRoom room)
    {
        try
        {
            if (room.StatusId == 0)
            {
                var defaultStatus = await _db.TblRoomStatuses.FirstOrDefaultAsync(s => s.StatusName == "Available");
                if (defaultStatus != null)
                {
                    room.StatusId = defaultStatus.StatusId;
                }
            }

            _db.TblRooms.Add(room);
            await _db.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    // Update room
    public async Task<bool> UpdateRoomAsync(TblRoom room)
    {
        try
        {
            var existing = await _db.TblRooms
                .FirstOrDefaultAsync(x => x.RoomId == room.RoomId);

            if (existing == null)
                return false;

            existing.RoomNo = room.RoomNo;
            existing.FloorId = room.FloorId;
            existing.RoomTypeId = room.RoomTypeId;
            existing.StatusId = room.StatusId;
            existing.Description = room.Description;

            await _db.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    // Delete room
    public async Task<bool> DeleteRoomAsync(int id)
    {
        try
        {
            var room = await _db.TblRooms
                .FirstOrDefaultAsync(x => x.RoomId == id);

            if (room == null)
                return false;

            _db.TblRooms.Remove(room);
            await _db.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }
}