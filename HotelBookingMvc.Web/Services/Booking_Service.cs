using Hotelbooking.Database.Context;
using Hotelbooking.Database.Models;
using HotelBookingMvc.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingMvc.Web.Services;

public class Booking_Service
{
    private readonly AppDbContext _db;

    public Booking_Service(AppDbContext db)
    {
        _db = db;
    }

    // =========================
    // GET BOOKING LIST
    // =========================
    public async Task<List<TblBooking>> GetBookingsAsync()
    {
        return await _db.TblBookings
            .Include(x => x.Customer)
            .OrderByDescending(x => x.BookingId)
            .ToListAsync();
    }

    // =========================
    // GET BOOKING BY ID
    // =========================
    public async Task<TblBooking?> GetBookingByIdAsync(int id)
    {
        return await _db.TblBookings
            .Include(x => x.Customer)
            .Include(x => x.TblBookingDetails)
            .FirstOrDefaultAsync(x => x.BookingId == id);
    }

    // =========================
    // GET CUSTOMERS
    // =========================
    public async Task<List<TblCustomer>> GetCustomersAsync()
    {
        return await _db.TblCustomers
            .OrderBy(x => x.CustomerName)
            .ToListAsync();
    }

    // =========================
    // GET AVAILABLE ROOMS
    // =========================
    public async Task<List<TblRoom>> GetAvailableRoomsAsync(
        DateOnly checkIn,
        DateOnly checkOut)
    {
        var bookedRoomIds = await _db.TblBookingDetails
            .Where(detail =>
                detail.Booking != null &&
                detail.Booking.Status != "Cancelled" &&
                detail.Booking.CheckInDate < checkOut &&
                detail.Booking.CheckOutDate > checkIn)
            .Select(detail => detail.Roomid)
            .Distinct()
            .ToListAsync();

        return await _db.TblRooms
            .Where(room =>
                !room.IsDelete &&
                room.Status != "Maintenance" &&
                !bookedRoomIds.Contains(room.RoomId))
            .OrderBy(room => room.RoomNumber)
            .ToListAsync();
    }

    // =========================
    // GET ROOM PRICE
    // =========================
    public async Task<decimal?> GetRoomPriceAsync(int roomId)
    {
        var room = await _db.TblRooms
            .FirstOrDefaultAsync(x =>
                x.RoomId == roomId &&
                !x.IsDelete);

        if (room == null)
            return null;

        var roomType = await _db.TblRoomTypes
            .FirstOrDefaultAsync(x =>
                x.RoomTypeId == room.RoomTypeId);

        return roomType?.PricePerNight;
    }

    // =========================
    // CREATE BOOKING
    // =========================
    public async Task<bool> CreateBookingAsync(
        BookingCreateViewModel model)
    {
        try
        {
            if (model.CheckOutDate <= model.CheckInDate)
                return false;

            int nights =
                model.CheckOutDate.DayNumber -
                model.CheckInDate.DayNumber;

            if (nights <= 0)
                return false;

            var room = await _db.TblRooms
                .FirstOrDefaultAsync(x =>
                    x.RoomId == model.RoomId &&
                    !x.IsDelete);

            if (room == null)
                return false;

            if (room.Status == "Maintenance")
                return false;

            // Check overlapping booking
            bool alreadyBooked = await _db.TblBookingDetails
                .AnyAsync(detail =>
                    detail.Roomid == model.RoomId &&
                    detail.Booking != null &&
                    detail.Booking.Status != "Cancelled" &&
                    detail.Booking.CheckInDate < model.CheckOutDate &&
                    detail.Booking.CheckOutDate > model.CheckInDate);

            if (alreadyBooked)
                return false;

            var roomType = await _db.TblRoomTypes
                .FirstOrDefaultAsync(x =>
                    x.RoomTypeId == room.RoomTypeId);

            if (roomType == null)
                return false;

            decimal amount = roomType.PricePerNight * nights;

            int bookingId = await _db.TblBookings
                .Select(x => (int?)x.BookingId)
                .MaxAsync() ?? 0;

            bookingId++;

            string bookingNo =
                "BK-" +
                DateTime.Now.ToString("yyyyMMdd") +
                "-" +
                bookingId.ToString("D4");

            var booking = new TblBooking
            {
                BookingId = bookingId,
                BookingNo = bookingNo,
                CustomerId = model.customerId,
                BookingDate = DateTime.Now,
                CheckInDate = model.CheckInDate,
                CheckOutDate = model.CheckOutDate,
                Status = "Pending",
                TotalAmount = amount,
                CreateDateTime = DateTime.Now
            };

            _db.TblBookings.Add(booking);

            int detailId = await _db.TblBookingDetails
                .Select(x => (int?)x.BookingDetailId)
                .MaxAsync() ?? 0;

            detailId++;

            var detail = new TblBookingDetail
            {
                BookingDetailId = detailId,
                Bookingid = bookingId,
                Roomid = model.RoomId,
                PricePerNight = roomType.PricePerNight,
                NumberOfNights = nights,
                Amount = amount
            };

            _db.TblBookingDetails.Add(detail);

            // Reserve room
            room.Status = "Reserved";

            await _db.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    // =========================
    // CANCEL BOOKING
    // =========================
    public async Task<bool> CancelBookingAsync(int id)
    {
        try
        {
            var booking = await _db.TblBookings
                .Include(x => x.TblBookingDetails)
                .FirstOrDefaultAsync(x => x.BookingId == id);

            if (booking == null)
                return false;

            if (booking.Status == "CheckedIn" ||
                booking.Status == "CheckedOut")
            {
                return false;
            }

            booking.Status = "Cancelled";

            foreach (var detail in booking.TblBookingDetails)
            {
                var room = await _db.TblRooms
                    .FirstOrDefaultAsync(x =>
                        x.RoomId == detail.Roomid);

                if (room != null && room.Status == "Reserved")
                {
                    room.Status = "Available";
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

    // =========================
    // CONFIRM BOOKING
    // =========================
    public async Task<bool> ConfirmBookingAsync(int id)
    {
        try
        {
            var booking = await _db.TblBookings
                .FirstOrDefaultAsync(x => x.BookingId == id);

            if (booking == null)
                return false;

            if (booking.Status != "Pending")
                return false;

            booking.Status = "Confirmed";

            await _db.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }
}