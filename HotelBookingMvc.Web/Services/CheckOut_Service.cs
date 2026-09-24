using Hotelbooking.Database.Context;
using HotelBookingMvc.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingMvc.Web.Services;

public class CheckOut_Service
{
    private readonly AppDbContext _db;

    public CheckOut_Service(AppDbContext db)
    {
        _db = db;
    }


    public async Task<List<CheckOutViewModel>> GetCheckOutBookingsAsync()
    {
        var bookings = await _db.TblBookings
            .Include(x => x.Customer)
            .Include(x => x.TblBookingDetails)
            .Where(x => x.Status == "CheckedIn")
            .OrderBy(x => x.CheckOutDate)
            .ToListAsync();

        var rooms = await _db.TblRooms
            .Where(x => !x.IsDelete)
            .ToListAsync();

        var result = new List<CheckOutViewModel>();

        foreach (var booking in bookings)
        {
            var detail = booking.TblBookingDetails.FirstOrDefault();

            if (detail == null)
                continue;

            var room = rooms.FirstOrDefault(
                x => x.RoomId == detail.Roomid);

            if (room == null)
                continue;

            decimal paidAmount =
                await _db.TblPayments
                    .Where(x => x.BookingId == booking.BookingId)
                    .SumAsync(x => (decimal?)x.Amount) ?? 0m;

            decimal balance =
                booking.TotalAmount - paidAmount;

            if (balance < 0)
                balance = 0;

            result.Add(new CheckOutViewModel
            {
                BookingId = booking.BookingId,
                BookingNo = booking.BookingNo,

                CustomerName =
                    booking.Customer?.CustomerName ?? "Unknown",

                CustomerPhone =
                    booking.Customer?.Phone ?? "-",

                RoomNumber = room.RoomNumber,

                CheckInDate = booking.CheckInDate,

                CheckOutDate = booking.CheckOutDate,

                TotalAmount = booking.TotalAmount,

                PaidAmount = paidAmount,

                BalanceAmount = balance,

                Status = booking.Status
            });
        }

        return result;
    }


   
    public async Task<CheckOutViewModel?> GetCheckOutBookingAsync(
        int bookingId)
    {
        var booking = await _db.TblBookings
            .Include(x => x.Customer)
            .Include(x => x.TblBookingDetails)
            .FirstOrDefaultAsync(x =>
                x.BookingId == bookingId &&
                x.Status == "CheckedIn");

        if (booking == null)
            return null;

        var detail = booking.TblBookingDetails.FirstOrDefault();

        if (detail == null)
            return null;

        var room = await _db.TblRooms
            .FirstOrDefaultAsync(x =>
                x.RoomId == detail.Roomid &&
                !x.IsDelete);

        if (room == null)
            return null;

        decimal paidAmount =
            await _db.TblPayments
                .Where(x => x.BookingId == booking.BookingId)
                .SumAsync(x => (decimal?)x.Amount) ?? 0m;

        decimal balance =
            booking.TotalAmount - paidAmount;

        if (balance < 0)
            balance = 0;

        return new CheckOutViewModel
        {
            BookingId = booking.BookingId,
            BookingNo = booking.BookingNo,

            CustomerName =
                booking.Customer?.CustomerName ?? "Unknown",

            CustomerPhone =
                booking.Customer?.Phone ?? "-",

            RoomNumber = room.RoomNumber,

            CheckInDate = booking.CheckInDate,

            CheckOutDate = booking.CheckOutDate,

            TotalAmount = booking.TotalAmount,

            PaidAmount = paidAmount,

            BalanceAmount = balance,

            Status = booking.Status
        };
    }



    public async Task<bool> CheckOutAsync(int bookingId)
    {
        try
        {
            var booking = await _db.TblBookings
                .Include(x => x.TblBookingDetails)
                .FirstOrDefaultAsync(x =>
                    x.BookingId == bookingId);

            if (booking == null)
                return false;

            if (booking.Status != "CheckedIn")
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

            decimal paidAmount =
                await _db.TblPayments
                    .Where(x => x.BookingId == booking.BookingId)
                    .SumAsync(x => (decimal?)x.Amount) ?? 0m;

            decimal balance =
                booking.TotalAmount - paidAmount;

         
            if (balance > 0)
                return false;

            booking.Status = "CheckedOut";

            room.Status = "Available";

            await _db.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }
}