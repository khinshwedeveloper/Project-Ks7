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

    // Get all active rooms
    public async Task<List<TblRoom>> GetRoomsAsync()
    {
        return await _db.TblRooms
            .Where(x => !x.IsDelete)
            .OrderBy(x => x.RoomNumber)
            .ToListAsync();
    }

    // Get room by ID
    public async Task<TblRoom?> GetRoomByIdAsync(int id)
    {
        return await _db.TblRooms
            .FirstOrDefaultAsync(x =>
                x.RoomId == id &&
                !x.IsDelete);
    }

    // Create room
    public async Task<bool> CreateRoomAsync(TblRoom room)
    {
        try
        {
            int maxId = await _db.TblRooms
                .Select(x => (int?)x.RoomId)
                .MaxAsync() ?? 0;

            room.RoomId = maxId + 1;

            room.CreatedDateTime = DateTime.Now;

            room.IsDelete = false;

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
                .FirstOrDefaultAsync(x =>
                    x.RoomId == room.RoomId &&
                    !x.IsDelete);

            if (existing == null)
                return false;

            existing.RoomNumber = room.RoomNumber;
            existing.RoomTypeId = room.RoomTypeId;
            existing.Floor = room.Floor;
            existing.Status = room.Status;

            await _db.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    // Soft delete room
    public async Task<bool> DeleteRoomAsync(int id)
    {
        try
        {
            var room = await _db.TblRooms
                .FirstOrDefaultAsync(x =>
                    x.RoomId == id &&
                    !x.IsDelete);

            if (room == null)
                return false;

            room.IsDelete = true;

            await _db.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }
}