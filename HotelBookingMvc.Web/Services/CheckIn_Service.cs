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
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        var bookings = await _db.TblBookings
            .Include(x => x.Customer)
            .Include(x => x.TblBookingDetails)
            .Where(x =>
                x.Status == "Confirmed" &&
                x.CheckInDate <= today &&
                x.CheckOutDate > today)
            .OrderBy(x => x.CheckInDate)
            .ToListAsync();

        var rooms = await _db.TblRooms
            .Where(x => !x.IsDelete)
            .ToListAsync();

        var result = new List<CheckInViewModel>();

        foreach (var booking in bookings)
        {
            var detail = booking.TblBookingDetails.FirstOrDefault();

            if (detail == null)
                continue;

            var room = rooms.FirstOrDefault(
                x => x.RoomId == detail.Roomid);

            if (room == null)
                continue;

            result.Add(new CheckInViewModel
            {
                BookingId = booking.BookingId,
                BookingNo = booking.BookingNo,

                CustomerName =
                    booking.Customer?.CustomerName ?? "Unknown",

                CustomerPhone =
                    booking.Customer?.Phone ?? "-",

                RoomNumber =
                    room.RoomNumber,

                CheckInDate =
                    booking.CheckInDate,

                CheckOutDate =
                    booking.CheckOutDate,

                TotalAmount =
                    booking.TotalAmount,

                Status =
                    booking.Status
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
            .FirstOrDefaultAsync(x =>
                x.BookingId == bookingId);
    }


  
    public async Task<bool> CheckInAsync(int bookingId)
    {
        try
        {
            DateOnly today =
                DateOnly.FromDateTime(DateTime.Today);

            var booking = await _db.TblBookings
                .Include(x => x.TblBookingDetails)
                .FirstOrDefaultAsync(x =>
                    x.BookingId == bookingId);

            if (booking == null)
                return false;

           
            if (booking.Status != "Confirmed")
                return false;

         
            if (today < booking.CheckInDate)
                return false;

           
            if (today >= booking.CheckOutDate)
                return false;

            var detail =
                booking.TblBookingDetails.FirstOrDefault();

            if (detail == null)
                return false;

            var room = await _db.TblRooms
                .FirstOrDefaultAsync(x =>
                    x.RoomId == detail.Roomid &&
                    !x.IsDelete);

            if (room == null)
                return false;

            
            if (room.Status == "Maintenance")
                return false;

            
            booking.Status = "CheckedIn";

          
            room.Status = "Occupied";

            await _db.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }
}