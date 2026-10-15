using Hotelbooking.Database.Context;
using Hotelbooking.Database.Models;
using HotelBookingMvc.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingMvc.Web.Services;

public class CheckIn_Service
{
    private readonly AppDbContext _db;

    public CheckIn_Service(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CheckInViewModel>> GetCheckInBookingsAsync()
    {
        DateTime today = DateTime.Today;
        DateTime tomorrow = today.AddDays(1);

        var bookings = await _db.TblBookings
            .Include(x => x.Customer)
            .Include(x => x.TblBookingDetails)
                .ThenInclude(d => d.Room)
            .Where(x => x.Status == "Confirmed")
            .OrderBy(x => x.BookingDate)
            .ToListAsync();

        var result = new List<CheckInViewModel>();

        foreach (var booking in bookings)
        {
            var detail = booking.TblBookingDetails.FirstOrDefault();
            if (detail == null)
                continue;

            var room = detail.Room;
            if (room == null)
                continue;

            result.Add(new CheckInViewModel
            {
                BookingId = booking.BookingId,
                BookingNo = booking.BookingNo ?? string.Empty,
                CustomerName = booking.Customer?.Name ?? "Unknown",
                CustomerPhone = booking.Customer?.Phone ?? "-",
                RoomNumber = room.RoomNo,
                CheckInDate = DateOnly.FromDateTime(detail.CheckInDateTime),
                CheckOutDate = DateOnly.FromDateTime(detail.CheckOutDateTime),
                TotalAmount = detail.TotalRoomPrice,
                Status = booking.Status ?? string.Empty
            });
        }

        return result;
    }

    // Get one booking
    public async Task<TblBooking?> GetBookingAsync(int bookingId)
    {
        return await _db.TblBookings
            .Include(x => x.Customer)
            .Include(x => x.TblBookingDetails)
                .ThenInclude(d => d.Room)
            .FirstOrDefaultAsync(x => x.BookingId == bookingId);
    }

    public async Task<bool> CheckInAsync(int bookingId)
    {
        try
        {
            var booking = await _db.TblBookings
                .Include(x => x.TblBookingDetails)
                    .ThenInclude(d => d.Room)
                .FirstOrDefaultAsync(x => x.BookingId == bookingId);

            if (booking == null)
                return false;

            if (booking.Status != "Confirmed")
                return false;

            var occupiedStatus = await _db.TblRoomStatuses.FirstOrDefaultAsync(s => s.StatusName == "Occupied");

            booking.Status = "CheckedIn";

            foreach (var detail in booking.TblBookingDetails)
            {
                var room = detail.Room ?? await _db.TblRooms.FirstOrDefaultAsync(r => r.RoomId == detail.RoomId);
                if (room != null && occupiedStatus != null)
                {
                    room.StatusId = occupiedStatus.StatusId;
                }
            }

            await _db.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}