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


    // =========================================================
    // MAIN REPORT
    // =========================================================

    public async Task<ReportViewModel> GetReportAsync(
        DateOnly? fromDate,
        DateOnly? toDate)
    {
        var model = new ReportViewModel
        {
            FromDate = fromDate,
            ToDate = toDate
        };


        // =====================================================
        // ROOM SUMMARY
        // =====================================================

        model.TotalRooms =
            await _db.TblRooms
                .CountAsync(x => !x.IsDelete);

        model.AvailableRooms =
            await _db.TblRooms
                .CountAsync(x =>
                    !x.IsDelete &&
                    x.Status == "Available");

        model.ReservedRooms =
            await _db.TblRooms
                .CountAsync(x =>
                    !x.IsDelete &&
                    x.Status == "Reserved");

        model.OccupiedRooms =
            await _db.TblRooms
                .CountAsync(x =>
                    !x.IsDelete &&
                    x.Status == "Occupied");

        model.MaintenanceRooms =
            await _db.TblRooms
                .CountAsync(x =>
                    !x.IsDelete &&
                    x.Status == "Maintenance");


        // =====================================================
        // BOOKING QUERY
        // =====================================================

        var bookingQuery =
            _db.TblBookings
                .Include(x => x.Customer)
                .Include(x => x.TblBookingDetails)
                .AsQueryable();


        // =====================================================
        // DATE FILTER
        // =====================================================

        if (fromDate.HasValue)
        {
            bookingQuery =
                bookingQuery.Where(x =>
                    x.CheckInDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            bookingQuery =
                bookingQuery.Where(x =>
                    x.CheckInDate <= toDate.Value);
        }


        // =====================================================
        // GET BOOKINGS
        // =====================================================

        var bookings =
            await bookingQuery
                .OrderByDescending(x => x.BookingDate)
                .ToListAsync();


        // =====================================================
        // BOOKING SUMMARY
        // =====================================================

        model.TotalBookings =
            bookings.Count;

        model.ConfirmedBookings =
            bookings.Count(x =>
                x.Status == "Confirmed");

        model.CheckedInBookings =
            bookings.Count(x =>
                x.Status == "CheckedIn");

        model.CheckedOutBookings =
            bookings.Count(x =>
                x.Status == "CheckedOut");

        model.CancelledBookings =
            bookings.Count(x =>
                x.Status == "Cancelled");


        // =====================================================
        // PAYMENT DATA
        // =====================================================

        var bookingIds =
            bookings
                .Select(x => x.BookingId)
                .ToList();

        var payments =
            await _db.TblPayments
                .Where(x =>
                    bookingIds.Contains(x.BookingId))
                .ToListAsync();


        // =====================================================
        // TOTAL BOOKING AMOUNT
        // =====================================================

        model.TotalBookingAmount =
            bookings.Sum(x => x.TotalAmount);


        // =====================================================
        // TOTAL PAID AMOUNT
        // =====================================================

        model.TotalPaidAmount =
            payments
                .Where(x => x.PaymentStatus != "Refunded")
                .Sum(x => x.Amount);


        // =====================================================
        // OUTSTANDING
        // =====================================================

        model.TotalOutstandingAmount =
            model.TotalBookingAmount -
            model.TotalPaidAmount;

        if (model.TotalOutstandingAmount < 0)
        {
            model.TotalOutstandingAmount = 0;
        }


        // =====================================================
        // BOOKING REPORT
        // =====================================================

        foreach (var booking in bookings)
        {
            var detail =
                booking.TblBookingDetails.FirstOrDefault();

            var roomNumber = "-";

            if (detail != null)
            {
                var room =
                    await _db.TblRooms
                        .FirstOrDefaultAsync(x =>
                            x.RoomId == detail.Roomid);

                if (room != null)
                {
                    roomNumber = room.RoomNumber;
                }
            }


            // ---------------------------------------------
            // PAYMENT FOR THIS BOOKING
            // ---------------------------------------------

            decimal paidAmount =
                payments
                    .Where(x =>
                        x.BookingId == booking.BookingId &&
                        x.PaymentStatus != "Refunded")
                    .Sum(x => x.Amount);


            decimal balance =
                booking.TotalAmount -
                paidAmount;

            if (balance < 0)
            {
                balance = 0;
            }


            // ---------------------------------------------
            // NUMBER OF NIGHTS
            // ---------------------------------------------

            int nights =
                booking.CheckOutDate.DayNumber -
                booking.CheckInDate.DayNumber;

            if (nights < 0)
            {
                nights = 0;
            }


            model.Bookings.Add(
                new BookingReportItem
                {
                    BookingId =
                        booking.BookingId,

                    BookingNo =
                        booking.BookingNo,

                    CustomerName =
                        booking.Customer?.CustomerName
                        ?? "Unknown",

                    CustomerPhone =
                        booking.Customer?.Phone
                        ?? "-",

                    RoomNumber =
                        roomNumber,

                    Status =
                        booking.Status,

                    CheckInDate =
                        booking.CheckInDate,

                    CheckOutDate =
                        booking.CheckOutDate,

                    NumberOfNights =
                        nights,

                    TotalAmount =
                        booking.TotalAmount,

                    PaidAmount =
                        paidAmount,

                    BalanceAmount =
                        balance
                });
        }


        // =====================================================
        // PAYMENT REPORT
        // =====================================================

        var customerLookup =
            bookings.ToDictionary(
                x => x.BookingId,
                x => x.Customer?.CustomerName ?? "Unknown");


        var bookingNoLookup =
            bookings.ToDictionary(
                x => x.BookingId,
                x => x.BookingNo);


        foreach (var payment in payments
            .OrderByDescending(x => x.PaymentDate))
        {
            model.Payments.Add(
                new PaymentReportItem
                {
                    PaymentId =
                        payment.PaymentId,

                    BookingId =
                        payment.BookingId,

                    BookingNo =
                        bookingNoLookup.TryGetValue(
                            payment.BookingId,
                            out var bookingNo)
                            ? bookingNo
                            : "-",

                    CustomerName =
                        customerLookup.TryGetValue(
                            payment.BookingId,
                            out var customerName)
                            ? customerName
                            : "Unknown",

                    Amount =
                        payment.Amount,

                    PaymentMethod =
                        payment.PaymentMethod,

                    PaymentStatus =
                        payment.PaymentStatus,

                    PaymentDate =
                        payment.PaymentDate
                });
        }


        return model;
    }
}