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
            .Include(x => x.TblBookingDetails)
                .ThenInclude(d => d.Room)
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
                .ThenInclude(d => d.Room)
                    .ThenInclude(r => r!.RoomType)
            .Include(x => x.TblPayments)
            .FirstOrDefaultAsync(x => x.BookingId == id);
    }

    // =========================
    // GET CUSTOMERS
    // =========================
    public async Task<List<TblCustomer>> GetCustomersAsync()
    {
        return await _db.TblCustomers
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    // =========================
    // GET AVAILABLE ROOMS
    // =========================
    public async Task<List<TblRoom>> GetAvailableRoomsAsync(
        DateOnly checkIn,
        DateOnly checkOut)
    {
        DateTime checkInDt = checkIn.ToDateTime(TimeOnly.MinValue);
        DateTime checkOutDt = checkOut.ToDateTime(TimeOnly.MinValue);

        var bookedRoomIds = await _db.TblBookingDetails
            .Where(detail =>
                detail.Booking != null &&
                detail.Booking.Status != "Cancelled" &&
                detail.CheckInDateTime < checkOutDt &&
                detail.CheckOutDateTime > checkInDt)
            .Select(detail => detail.RoomId)
            .Distinct()
            .ToListAsync();

        return await _db.TblRooms
            .Include(r => r.Floor)
            .Include(r => r.RoomType)
            .Include(r => r.Status)
            .Where(room =>
                (room.Status == null || room.Status.StatusName != "Maintenance") &&
                !bookedRoomIds.Contains(room.RoomId))
            .OrderBy(room => room.RoomNo)
            .ToListAsync();
    }

    // =========================
    // GET ROOM PRICE
    // =========================
    public async Task<decimal?> GetRoomPriceAsync(int roomId)
    {
        var room = await _db.TblRooms
            .FirstOrDefaultAsync(x => x.RoomId == roomId);

        if (room == null)
            return null;

        // Try lookup from TblRoomPrice & TblRoomPriceDetail
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        var priceMaster = await _db.TblRoomPrices
            .Include(p => p.TblRoomPriceDetails)
            .Where(p => p.RoomTypeId == room.RoomTypeId && p.Status && p.EffectiveDate <= today)
            .OrderByDescending(p => p.EffectiveDate)
            .FirstOrDefaultAsync();

        if (priceMaster != null && priceMaster.TblRoomPriceDetails.Any())
        {
            return priceMaster.TblRoomPriceDetails.Average(d => d.Price);
        }

        return 50000m; // Default standard fallback
    }

    // =========================
    // CREATE BOOKING
    // =========================
    public async Task<bool> CreateBookingAsync(BookingCreateViewModel model)
    {
        try
        {
            if (model.CheckOutDate <= model.CheckInDate)
                return false;

            int nights = model.CheckOutDate.DayNumber - model.CheckInDate.DayNumber;
            if (nights <= 0)
                return false;

            var room = await _db.TblRooms
                .Include(r => r.Status)
                .FirstOrDefaultAsync(x => x.RoomId == model.RoomId);

            if (room == null)
                return false;

            if (room.Status != null && room.Status.StatusName == "Maintenance")
                return false;

            DateTime checkInDt = model.CheckInDate.ToDateTime(TimeOnly.MinValue);
            DateTime checkOutDt = model.CheckOutDate.ToDateTime(TimeOnly.MinValue);

            // Check overlapping booking
            bool alreadyBooked = await _db.TblBookingDetails
                .AnyAsync(detail =>
                    detail.RoomId == model.RoomId &&
                    detail.Booking != null &&
                    detail.Booking.Status != "Cancelled" &&
                    detail.CheckInDateTime < checkOutDt &&
                    detail.CheckOutDateTime > checkInDt);

            if (alreadyBooked)
                return false;

            decimal roomPricePerNight = model.PricePerNight > 0 
                ? model.PricePerNight 
                : (await GetRoomPriceAsync(model.RoomId) ?? 50000m);

            decimal extraBedTotal = model.ExtraBedQty * model.ExtraBedPrice * nights;
            decimal totalRoomPrice = (roomPricePerNight * nights) + extraBedTotal;

            string bookingNo = $"BK-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}";

            var booking = new TblBooking
            {
                BookingNo = bookingNo,
                CustomerId = model.CustomerId,
                BookingDate = DateTime.Now,
                Status = "Pending",
                Remarks = model.Remarks,
                CreatedDate = DateTime.Now
            };

            _db.TblBookings.Add(booking);
            await _db.SaveChangesAsync();

            var detail = new TblBookingDetail
            {
                BookingId = booking.BookingId,
                RoomId = model.RoomId,
                CheckInDateTime = checkInDt,
                CheckOutDateTime = checkOutDt,
                RoomPrice = roomPricePerNight,
                ExtraBedQty = model.ExtraBedQty,
                ExtraBedPrice = model.ExtraBedPrice,
                TotalRoomPrice = totalRoomPrice,
                Remarks = model.Remarks
            };

            _db.TblBookingDetails.Add(detail);

            // Update room status to Reserved
            var reservedStatus = await _db.TblRoomStatuses.FirstOrDefaultAsync(s => s.StatusName == "Reserved");
            if (reservedStatus != null)
            {
                room.StatusId = reservedStatus.StatusId;
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

            if (booking.Status == "CheckedIn" || booking.Status == "CheckedOut")
                return false;

            booking.Status = "Cancelled";

            var availableStatus = await _db.TblRoomStatuses.FirstOrDefaultAsync(s => s.StatusName == "Available");
            var reservedStatus = await _db.TblRoomStatuses.FirstOrDefaultAsync(s => s.StatusName == "Reserved");

            if (availableStatus != null)
            {
                foreach (var detail in booking.TblBookingDetails)
                {
                    var room = await _db.TblRooms.FirstOrDefaultAsync(x => x.RoomId == detail.RoomId);
                    if (room != null && reservedStatus != null && room.StatusId == reservedStatus.StatusId)
                    {
                        room.StatusId = availableStatus.StatusId;
                    }
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
            var booking = await _db.TblBookings.FirstOrDefaultAsync(x => x.BookingId == id);
            if (booking == null || booking.Status != "Pending")
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

    // =========================
    // GET ROOM BY ID
    // =========================
    public async Task<TblRoom?> GetRoomByIdAsync(int id)
    {
        return await _db.TblRooms
            .Include(r => r.Floor)
            .Include(r => r.RoomType)
            .Include(r => r.Status)
            .FirstOrDefaultAsync(x => x.RoomId == id);
    }

    // =========================
    // GET ROOM TYPE BY ID
    // =========================
    public async Task<TblRoomType?> GetRoomTypeByIdAsync(int id)
    {
        return await _db.TblRoomTypes
            .FirstOrDefaultAsync(x => x.RoomTypeId == id);
    }
}