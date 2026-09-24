using Hotelbooking.Database.Context;
using HotelBookingMvc.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingMvc.Web.Services;

public class NotificationService
{
    private readonly AppDbContext _context;

    public NotificationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationViewModel> GetNotificationsAsync()
    {
        DateOnly today =
            DateOnly.FromDateTime(DateTime.Today);

        // ============================================
        // TODAY CHECK-IN
        // ============================================

        int todayCheckIns =
            await _context.TblBookings
                .CountAsync(x =>
                    x.CheckInDate == today &&
                    x.Status == "Confirmed");


        // ============================================
        // TODAY CHECK-OUT
        // ============================================

        int todayCheckOuts =
            await _context.TblBookings
                .CountAsync(x =>
                    x.CheckOutDate == today &&
                    x.Status == "CheckedIn");


        // ============================================
        // PENDING PAYMENT
        // ============================================

        var bookings =
            await _context.TblBookings
                .Where(x =>
                    x.Status != "Cancelled" &&
                    x.Status != "CheckedOut")
                .Select(x => new
                {
                    x.BookingId,
                    x.TotalAmount
                })
                .ToListAsync();


        int pendingPayments = 0;

        foreach (var booking in bookings)
        {
            decimal paidAmount =
                await _context.TblPayments
                    .Where(x =>
                        x.BookingId == booking.BookingId &&
                        x.PaymentStatus != "Refunded")
                    .SumAsync(x =>
                        (decimal?)x.Amount) ?? 0m;

            if (paidAmount < booking.TotalAmount)
            {
                pendingPayments++;
            }
        }


        // ============================================
        // TOTAL
        // ============================================

        int total =
            todayCheckIns +
            todayCheckOuts +
            pendingPayments;


        return new NotificationViewModel
        {
            TodayCheckIns = todayCheckIns,
            TodayCheckOuts = todayCheckOuts,
            PendingPayments = pendingPayments,
            TotalNotifications = total
        };
    }
}