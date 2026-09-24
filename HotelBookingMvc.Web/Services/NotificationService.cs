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
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        // Fetch active bookings (not cancelled) with Customer and Details
        var bookings = await _context.TblBookings
            .Include(x => x.Customer)
            .Include(x => x.TblBookingDetails)
            .Where(x => x.Status != "Cancelled")
            .ToListAsync();

        var roomIds = bookings
            .SelectMany(b => b.TblBookingDetails)
            .Select(d => d.Roomid)
            .Distinct()
            .ToList();

        var rooms = await _context.TblRooms
            .Where(r => roomIds.Contains(r.RoomId) && !r.IsDelete)
            .ToDictionaryAsync(r => r.RoomId, r => r.RoomNumber);

        var bookingIds = bookings.Select(b => b.BookingId).ToList();

        var paymentsGrouped = await _context.TblPayments
            .Where(p => bookingIds.Contains(p.BookingId) && p.PaymentStatus != "Refunded")
            .GroupBy(p => p.BookingId)
            .Select(g => new
            {
                BookingId = g.Key,
                PaidAmount = g.Sum(p => (decimal?)p.Amount) ?? 0m
            })
            .ToDictionaryAsync(x => x.BookingId, x => x.PaidAmount);

        var items = new List<NotificationItemViewModel>();

        int todayCheckInsCount = 0;
        int todayCheckOutsCount = 0;
        int pendingPaymentsCount = 0;

        foreach (var booking in bookings)
        {
            var firstDetail = booking.TblBookingDetails.FirstOrDefault();
            string roomNumber = firstDetail != null && rooms.TryGetValue(firstDetail.Roomid, out var rNum)
                ? rNum
                : "N/A";

            string customerName = booking.Customer?.CustomerName ?? "Unknown Guest";
            string customerPhone = booking.Customer?.Phone ?? "-";

            decimal paidAmount = paymentsGrouped.TryGetValue(booking.BookingId, out var pAmt) ? pAmt : 0m;
            decimal balanceAmount = booking.TotalAmount - paidAmount;
            if (balanceAmount < 0) balanceAmount = 0;

            // 1. TODAY CHECK-IN: Confirmed bookings scheduled for check-in today or past-due
            if (booking.Status == "Confirmed" && booking.CheckInDate <= today && booking.CheckOutDate > today)
            {
                todayCheckInsCount++;
                bool isOverdue = booking.CheckInDate < today;
                items.Add(new NotificationItemViewModel
                {
                    BookingId = booking.BookingId,
                    BookingNo = booking.BookingNo,
                    CustomerName = customerName,
                    CustomerPhone = customerPhone,
                    RoomNumber = roomNumber,
                    Category = "CheckIn",
                    Title = isOverdue ? "Overdue Check-In" : "Check-In Scheduled Today",
                    Message = isOverdue 
                        ? $"Guest {customerName} was scheduled to check into Room {roomNumber} on {booking.CheckInDate:yyyy-MM-dd}."
                        : $"Guest {customerName} is scheduled to check into Room {roomNumber} today.",
                    CheckInDate = booking.CheckInDate,
                    CheckOutDate = booking.CheckOutDate,
                    TotalAmount = booking.TotalAmount,
                    PaidAmount = paidAmount,
                    BalanceAmount = balanceAmount,
                    ActionUrl = "/CheckIn",
                    ActionText = "Check In Guest",
                    IsOverdue = isOverdue
                });
            }

            // 2. TODAY CHECK-OUT: CheckedIn bookings scheduled for check-out today or past-due
            if (booking.Status == "CheckedIn" && booking.CheckOutDate <= today)
            {
                todayCheckOutsCount++;
                bool isOverdue = booking.CheckOutDate < today;
                items.Add(new NotificationItemViewModel
                {
                    BookingId = booking.BookingId,
                    BookingNo = booking.BookingNo,
                    CustomerName = customerName,
                    CustomerPhone = customerPhone,
                    RoomNumber = roomNumber,
                    Category = "CheckOut",
                    Title = isOverdue ? "Overdue Check-Out" : "Check-Out Scheduled Today",
                    Message = isOverdue 
                        ? $"Guest {customerName} in Room {roomNumber} was scheduled to check out on {booking.CheckOutDate:yyyy-MM-dd}."
                        : $"Guest {customerName} in Room {roomNumber} is scheduled for check-out today.",
                    CheckInDate = booking.CheckInDate,
                    CheckOutDate = booking.CheckOutDate,
                    TotalAmount = booking.TotalAmount,
                    PaidAmount = paidAmount,
                    BalanceAmount = balanceAmount,
                    ActionUrl = $"/CheckOut/Details/{booking.BookingId}",
                    ActionText = "Process Check-Out",
                    IsOverdue = isOverdue
                });
            }

            // 3. PENDING PAYMENT: Active bookings (Confirmed or CheckedIn) with remaining balance
            if (booking.Status != "CheckedOut" && balanceAmount > 0)
            {
                pendingPaymentsCount++;
                items.Add(new NotificationItemViewModel
                {
                    BookingId = booking.BookingId,
                    BookingNo = booking.BookingNo,
                    CustomerName = customerName,
                    CustomerPhone = customerPhone,
                    RoomNumber = roomNumber,
                    Category = "Payment",
                    Title = "Pending Booking Payment",
                    Message = $"Booking #{booking.BookingNo} ({customerName}) has an outstanding balance of MMK {balanceAmount:N2}.",
                    CheckInDate = booking.CheckInDate,
                    CheckOutDate = booking.CheckOutDate,
                    TotalAmount = booking.TotalAmount,
                    PaidAmount = paidAmount,
                    BalanceAmount = balanceAmount,
                    ActionUrl = $"/Payment/Create?bookingId={booking.BookingId}",
                    ActionText = "Collect Payment",
                    IsOverdue = false
                });
            }
        }

        int total = todayCheckInsCount + todayCheckOutsCount + pendingPaymentsCount;

        return new NotificationViewModel
        {
            TodayCheckIns = todayCheckInsCount,
            TodayCheckOuts = todayCheckOutsCount,
            PendingPayments = pendingPaymentsCount,
            TotalNotifications = total,
            Items = items.OrderByDescending(x => x.IsOverdue).ThenBy(x => x.Category).ToList()
        };
    }
}