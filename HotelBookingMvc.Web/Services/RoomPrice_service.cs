using Hotelbooking.Database.Context;
using Hotelbooking.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingMvc.Web.Services;


public class RoomPrice_service
    {
    private readonly AppDbContext _context;
    public RoomPrice_service(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TblRoomPrice>> GetPriceAsync()
    {
        return await _context.TblRoomPrices
            .Include(x => x.RoomType)
            .Include(x=>x.TblRoomPriceDetails)
            .OrderByDescending(x=>x.EffectiveDate)
            .ToListAsync();
    }

    public async Task<TblRoomPrice?> GetPriceAsync(int id)
    {
        return await _context.TblRoomPrices
            .Include(x => x.RoomType)
            .Include(x => x.TblRoomPriceDetails)
            .FirstOrDefaultAsync(x => x.RoomPriceId == id);
    }
    public async Task<List<TblRoomType>> GetRoomTypesAsync()
    {
        return await _context.TblRoomTypes
            .Where(x => x.Status)
            .OrderBy(x => x.RoomTypeName)
            .ToListAsync();
    }
    public async Task AddPriceAsync(TblRoomPrice price)
    {
        _context.TblRoomPrices.Add(price);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePriceAsync(TblRoomPrice price)
    {
        var existing= await _context.TblRoomPrices.FindAsync(price.RoomPriceId);
        if (existing == null)
            throw new Exception(" Room Price not found");
        existing.RoomTypeId = price.RoomTypeId;
        existing.PriceName = price.PriceName;
        existing.EffectiveDate = price.EffectiveDate;
        existing.Status = price.Status;
         
        await _context.SaveChangesAsync();
    }
    public async Task DeletePriceAsync(int id)
    {
        var price = await _context.TblRoomPrices.FindAsync(id);
        if (price == null)
            throw new Exception(" Room Price not found");
        _context.TblRoomPrices.Remove(price);
        await _context.SaveChangesAsync();
    }

    public async Task<List<TblRoomPriceDetail>> GetDetailsAsync(int priceId)
    {
        return await _context.TblRoomPriceDetails
            .Include(x => x.RoomPrice)
            .Where(x => x.RoomPriceId == priceId)
            .OrderBy(x=> x.StartTime)
            .ToListAsync();
        
    }

    public async Task<TblRoomPriceDetail?> GetDetailAsync(int id)
    {
        return await _context.TblRoomPriceDetails
            .Include(x => x.RoomPrice)
            .FirstOrDefaultAsync(x => x.RoomPriceDetailId == id);
    }
    public async Task AddDetailAsync(TblRoomPriceDetail detail)
    {
        if (detail.EndTime == detail.StartTime)
            throw new InvalidOperationException("Start time and end time cannot be the same.");

        if (detail.Price < 0)
            throw new InvalidOperationException("Price cannot be negative");

        _context.TblRoomPriceDetails.Add(detail);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateDetailAsync(TblRoomPriceDetail detail)
    {
        var existing = await _context.TblRoomPriceDetails.FindAsync(detail.RoomPriceDetailId);
        if (existing == null)
            throw new Exception(" Room Price Detail not found");
        if (detail.EndTime == detail.StartTime)
            throw new InvalidOperationException("Start time and end time cannot be the same.");
        if (detail.Price < 0)
            throw new InvalidOperationException("Price cannot be negative");
        existing.StartTime = detail.StartTime;
        existing.EndTime = detail.EndTime;
        existing.Price = detail.Price;
        await _context.SaveChangesAsync();
    }

    internal async Task DeleteDetailAsync(int id)
    {
        throw new NotImplementedException();
    }
}

