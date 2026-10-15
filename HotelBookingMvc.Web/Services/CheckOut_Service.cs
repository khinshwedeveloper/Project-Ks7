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
                .ThenInclude(d => d.Room)
            .Where(x => x.Status == "CheckedIn")
            .OrderBy(x => x.BookingDate)
            .ToListAsync();

        var result = new List<CheckOutViewModel>();

        foreach (var booking in bookings)
        {
            var detail = booking.TblBookingDetails.FirstOrDefault();
            if (detail == null)
                continue;

            var room = detail.Room;
            if (room == null)
                continue;

            decimal totalAmount = booking.TblBookingDetails.Sum(d => d.TotalRoomPrice);

            decimal paidAmount = await _db.TblPayments
                .Where(x => x.BookingId == booking.BookingId && x.PaymentStatus != "Refunded")
                .SumAsync(x => (decimal?)x.Amount) ?? 0m;

            decimal balance = totalAmount - paidAmount;
            if (balance < 0) balance = 0;

            result.Add(new CheckOutViewModel
            {
                BookingId = booking.BookingId,
                BookingNo = booking.BookingNo ?? string.Empty,
                CustomerName = booking.Customer?.Name ?? "Unknown",
                CustomerPhone = booking.Customer?.Phone ?? "-",
                RoomNumber = room.RoomNo,
                CheckInDate = DateOnly.FromDateTime(detail.CheckInDateTime),
                CheckOutDate = DateOnly.FromDateTime(detail.CheckOutDateTime),
                TotalAmount = totalAmount,
                PaidAmount = paidAmount,
                BalanceAmount = balance,
                Status = booking.Status ?? string.Empty
            });
        }

        return result;
    }

    public async Task<CheckOutViewModel?> GetCheckOutBookingAsync(int bookingId)
    {
        var booking = await _db.TblBookings
            .Include(x => x.Customer)
            .Include(x => x.TblBookingDetails)
                .ThenInclude(d => d.Room)
            .FirstOrDefaultAsync(x => x.BookingId == bookingId && x.Status == "CheckedIn");

        if (booking == null)
            return null;

        var detail = booking.TblBookingDetails.FirstOrDefault();
        if (detail == null)
            return null;

        var room = detail.Room ?? await _db.TblRooms.FirstOrDefaultAsync(r => r.RoomId == detail.RoomId);
        if (room == null)
            return null;

        decimal totalAmount = booking.TblBookingDetails.Sum(d => d.TotalRoomPrice);

        decimal paidAmount = await _db.TblPayments
            .Where(x => x.BookingId == booking.BookingId && x.PaymentStatus != "Refunded")
            .SumAsync(x => (decimal?)x.Amount) ?? 0m;

        decimal balance = totalAmount - paidAmount;
        if (balance < 0) balance = 0;

        return new CheckOutViewModel
        {
            BookingId = booking.BookingId,
            BookingNo = booking.BookingNo ?? string.Empty,
            CustomerName = booking.Customer?.Name ?? "Unknown",
            CustomerPhone = booking.Customer?.Phone ?? "-",
            RoomNumber = room.RoomNo,
            CheckInDate = DateOnly.FromDateTime(detail.CheckInDateTime),
            CheckOutDate = DateOnly.FromDateTime(detail.CheckOutDateTime),
            TotalAmount = totalAmount,
            PaidAmount = paidAmount,
            BalanceAmount = balance,
            Status = booking.Status ?? string.Empty
        };
    }

    public async Task<bool> CheckOutAsync(int bookingId)
    {
        try
        {
            var booking = await _db.TblBookings
                .Include(x => x.TblBookingDetails)
                    .ThenInclude(d => d.Room)
                .FirstOrDefaultAsync(x => x.BookingId == bookingId);

            if (booking == null || booking.Status != "CheckedIn")
                return false;

            decimal totalAmount = booking.TblBookingDetails.Sum(d => d.TotalRoomPrice);

            decimal paidAmount = await _db.TblPayments
                .Where(x => x.BookingId == booking.BookingId && x.PaymentStatus != "Refunded")
                .SumAsync(x => (decimal?)x.Amount) ?? 0m;

            decimal balance = totalAmount - paidAmount;
            if (balance > 0)
                return false;

            booking.Status = "CheckedOut";

            var availableStatus = await _db.TblRoomStatuses.FirstOrDefaultAsync(s => s.StatusName == "Available");

            foreach (var detail in booking.TblBookingDetails)
            {
                var room = detail.Room ?? await _db.TblRooms.FirstOrDefaultAsync(r => r.RoomId == detail.RoomId);
                if (room != null && availableStatus != null)
                {
                    room.StatusId = availableStatus.StatusId;
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