using Hotelbooking.Database.Context;
using HotelBookingMvc.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingMvc.Web.Services;

public class Report_Service
{
    private readonly AppDbContext _db;

    public Report_Service(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ReportViewModel> GetReportAsync(
        DateOnly? fromDate,
        DateOnly? toDate)
    {
        var model = new ReportViewModel
        {
            FromDate = fromDate,
            ToDate = toDate
        };

        // ROOM SUMMARY
        model.TotalRooms = await _db.TblRooms.CountAsync();

        model.AvailableRooms = await _db.TblRooms
            .CountAsync(x => x.Status != null && x.Status.StatusName == "Available");

        model.ReservedRooms = await _db.TblRooms
            .CountAsync(x => x.Status != null && x.Status.StatusName == "Reserved");

        model.OccupiedRooms = await _db.TblRooms
            .CountAsync(x => x.Status != null && x.Status.StatusName == "Occupied");

        model.MaintenanceRooms = await _db.TblRooms
            .CountAsync(x => x.Status != null && x.Status.StatusName == "Maintenance");

        // BOOKINGS QUERY
        var bookingQuery = _db.TblBookings
            .Include(x => x.Customer)
            .Include(x => x.TblBookingDetails)
                .ThenInclude(d => d.Room)
            .AsQueryable();

        if (fromDate.HasValue)
        {
            DateTime fromDt = fromDate.Value.ToDateTime(TimeOnly.MinValue);
            bookingQuery = bookingQuery.Where(x =>
                x.TblBookingDetails.Any(d => d.CheckInDateTime >= fromDt));
        }

        if (toDate.HasValue)
        {
            DateTime toDt = toDate.Value.ToDateTime(TimeOnly.MaxValue);
            bookingQuery = bookingQuery.Where(x =>
                x.TblBookingDetails.Any(d => d.CheckInDateTime <= toDt));
        }

        var bookings = await bookingQuery
            .OrderByDescending(x => x.BookingDate)
            .ToListAsync();

        model.TotalBookings = bookings.Count;
        model.ConfirmedBookings = bookings.Count(x => x.Status == "Confirmed");
        model.CheckedInBookings = bookings.Count(x => x.Status == "CheckedIn");
        model.CheckedOutBookings = bookings.Count(x => x.Status == "CheckedOut");
        model.CancelledBookings = bookings.Count(x => x.Status == "Cancelled");

        var bookingIds = bookings.Select(x => x.BookingId).ToList();

        var payments = await _db.TblPayments
            .Where(x => bookingIds.Contains(x.BookingId))
            .ToListAsync();

        model.TotalBookingAmount = bookings.Sum(x => x.TblBookingDetails.Sum(d => d.TotalRoomPrice));

        model.TotalPaidAmount = payments
            .Where(x => x.PaymentStatus != "Refunded")
            .Sum(x => x.Amount);

        model.TotalOutstandingAmount = model.TotalBookingAmount - model.TotalPaidAmount;
        if (model.TotalOutstandingAmount < 0)
        {
            model.TotalOutstandingAmount = 0;
        }

        // BOOKING REPORT ITEMS
        foreach (var booking in bookings)
        {
            var detail = booking.TblBookingDetails.FirstOrDefault();
            var roomNumber = detail?.Room?.RoomNo ?? (detail != null ? detail.RoomId.ToString() : "-");

            decimal totalAmount = booking.TblBookingDetails.Sum(d => d.TotalRoomPrice);
            decimal paidAmount = payments
                .Where(x => x.BookingId == booking.BookingId && x.PaymentStatus != "Refunded")
                .Sum(x => x.Amount);

            decimal balance = totalAmount - paidAmount;
            if (balance < 0) balance = 0;

            DateOnly checkIn = detail != null ? DateOnly.FromDateTime(detail.CheckInDateTime) : DateOnly.FromDateTime(DateTime.Today);
            DateOnly checkOut = detail != null ? DateOnly.FromDateTime(detail.CheckOutDateTime) : DateOnly.FromDateTime(DateTime.Today.AddDays(1));

            int nights = detail != null ? (detail.CheckOutDateTime - detail.CheckInDateTime).Days : 1;
            if (nights <= 0) nights = 1;

            model.Bookings.Add(new BookingReportItem
            {
                BookingId = booking.BookingId,
                BookingNo = booking.BookingNo ?? string.Empty,
                CustomerName = booking.Customer?.Name ?? "Unknown",
                CustomerPhone = booking.Customer?.Phone ?? "-",
                RoomNumber = roomNumber,
                Status = booking.Status ?? string.Empty,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                NumberOfNights = nights,
                TotalAmount = totalAmount,
                PaidAmount = paidAmount,
                BalanceAmount = balance
            });
        }

        // PAYMENT REPORT ITEMS
        var customerLookup = bookings.ToDictionary(
            x => x.BookingId,
            x => x.Customer?.Name ?? "Unknown");

        var bookingNoLookup = bookings.ToDictionary(
            x => x.BookingId,
            x => x.BookingNo ?? string.Empty);

        foreach (var payment in payments.OrderByDescending(x => x.PaymentDate))
        {
            model.Payments.Add(new PaymentReportItem
            {
                PaymentId = payment.PaymentId,
                BookingId = payment.BookingId,
                BookingNo = bookingNoLookup.TryGetValue(payment.BookingId, out var bNo) ? (bNo ?? "-") : "-",
                CustomerName = customerLookup.TryGetValue(payment.BookingId, out var cName) ? cName : "Unknown",
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod ?? "-",
                PaymentStatus = payment.PaymentStatus ?? "-",
                PaymentDate = payment.PaymentDate
            });
        }

        return model;
    }
}