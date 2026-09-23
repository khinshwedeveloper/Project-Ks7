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
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Today's check-ins
        int todayCheckIns =
            await _context.TblBookings
                .CountAsync(x =>
                    x.CheckInDate == today);

        // Today's check-outs
        int todayCheckOuts =
            await _context.TblBookings
                .CountAsync(x =>
                    x.CheckOutDate == today);

        // Find bookings where paid amount is less than booking total
        var bookings = await _context.TblBookings
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
                    .Where(x => x.BookingId == booking.BookingId)
                    .SumAsync(x => (decimal?)x.Amount) ?? 0m;

            if (paidAmount < booking.TotalAmount)
            {
                pendingPayments++;
            }
        }

        return new NotificationViewModel
        {
            TodayCheckIns = todayCheckIns,
            TodayCheckOuts = todayCheckOuts,
            PendingPayments = pendingPayments
        };
    }
}