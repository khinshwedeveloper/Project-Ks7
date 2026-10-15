using Hotelbooking.Database.Context;
using Hotelbooking.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingMvc.Web.Services;

public class Floor_Service
{
    private readonly AppDbContext _db;

    public Floor_Service(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<TblFloor>> GetFloorsAsync()
    {
        return await _db.TblFloors
            .OrderByDescending(x => x.FloorId)
            .ToListAsync();
    }

    public async Task<TblFloor?> GetFloorByIdAsync(int id)
    {
        return await _db.TblFloors
            .FirstOrDefaultAsync(x => x.FloorId == id);
    }

    public async Task<bool> CreateFloorAsync(TblFloor floor)
    {
        try
        {
            bool exists = await _db.TblFloors
                .AnyAsync(x => x.FloorName == floor.FloorName);

            if (exists)
                return false;

            floor.Status = true;

            _db.TblFloors.Add(floor);
            await _db.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateFloorAsync(TblFloor floor)
    {
        try
        {
            var existingFloor = await _db.TblFloors
                .FirstOrDefaultAsync(x => x.FloorId == floor.FloorId);

            if (existingFloor == null)
                return false;

            bool duplicate = await _db.TblFloors
                .AnyAsync(x =>
                    x.FloorName == floor.FloorName &&
                    x.FloorId != floor.FloorId);

            if (duplicate)
                return false;

            existingFloor.FloorName = floor.FloorName;
            existingFloor.Description = floor.Description;
            existingFloor.Status = floor.Status;

            await _db.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteFloorAsync(int id)
    {
        try
        {
            var floor = await _db.TblFloors
                .FirstOrDefaultAsync(x => x.FloorId == id);

            if (floor == null)
                return false;

            _db.TblFloors.Remove(floor);
            await _db.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }
}

