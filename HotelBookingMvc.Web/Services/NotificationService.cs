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
        DateTime todayDt = DateTime.Today;

        var bookings = await _context.TblBookings
            .Include(x => x.Customer)
            .Include(x => x.TblBookingDetails)
                .ThenInclude(d => d.Room)
            .Where(x => x.Status != "Cancelled")
            .ToListAsync();

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
            string roomNumber = firstDetail?.Room?.RoomNo ?? (firstDetail != null ? firstDetail.RoomId.ToString() : "N/A");

            string customerName = booking.Customer?.Name ?? "Unknown Guest";
            string customerPhone = booking.Customer?.Phone ?? "-";

            DateOnly checkInDate = firstDetail != null ? DateOnly.FromDateTime(firstDetail.CheckInDateTime) : today;
            DateOnly checkOutDate = firstDetail != null ? DateOnly.FromDateTime(firstDetail.CheckOutDateTime) : today.AddDays(1);

            decimal totalAmount = booking.TblBookingDetails.Sum(d => d.TotalRoomPrice);
            decimal paidAmount = paymentsGrouped.TryGetValue(booking.BookingId, out var pAmt) ? pAmt : 0m;
            decimal balanceAmount = totalAmount - paidAmount;
            if (balanceAmount < 0) balanceAmount = 0;

            // 1. TODAY CHECK-IN
            if (booking.Status == "Confirmed" && checkInDate <= today && checkOutDate > today)
            {
                todayCheckInsCount++;
                bool isOverdue = checkInDate < today;
                items.Add(new NotificationItemViewModel
                {
                    BookingId = booking.BookingId,
                    BookingNo = booking.BookingNo ?? string.Empty,
                    CustomerName = customerName,
                    CustomerPhone = customerPhone,
                    RoomNumber = roomNumber,
                    Category = "CheckIn",
                    Title = isOverdue ? "Overdue Check-In" : "Check-In Scheduled Today",
                    Message = isOverdue 
                        ? $"Guest {customerName} was scheduled to check into Room {roomNumber} on {checkInDate:yyyy-MM-dd}."
                        : $"Guest {customerName} is scheduled to check into Room {roomNumber} today.",
                    CheckInDate = checkInDate,
                    CheckOutDate = checkOutDate,
                    TotalAmount = totalAmount,
                    PaidAmount = paidAmount,
                    BalanceAmount = balanceAmount,
                    ActionUrl = "/CheckIn",
                    ActionText = "Check In Guest",
                    IsOverdue = isOverdue
                });
            }

            // 2. TODAY CHECK-OUT
            if (booking.Status == "CheckedIn" && checkOutDate <= today)
            {
                todayCheckOutsCount++;
                bool isOverdue = checkOutDate < today;
                items.Add(new NotificationItemViewModel
                {
                    BookingId = booking.BookingId,
                    BookingNo = booking.BookingNo ?? string.Empty,
                    CustomerName = customerName,
                    CustomerPhone = customerPhone,
                    RoomNumber = roomNumber,
                    Category = "CheckOut",
                    Title = isOverdue ? "Overdue Check-Out" : "Check-Out Scheduled Today",
                    Message = isOverdue 
                        ? $"Guest {customerName} in Room {roomNumber} was scheduled to check out on {checkOutDate:yyyy-MM-dd}."
                        : $"Guest {customerName} in Room {roomNumber} is scheduled for check-out today.",
                    CheckInDate = checkInDate,
                    CheckOutDate = checkOutDate,
                    TotalAmount = totalAmount,
                    PaidAmount = paidAmount,
                    BalanceAmount = balanceAmount,
                    ActionUrl = $"/CheckOut/Details/{booking.BookingId}",
                    ActionText = "Process Check-Out",
                    IsOverdue = isOverdue
                });
            }

            // 3. PENDING PAYMENT
            if (booking.Status != "CheckedOut" && balanceAmount > 0)
            {
                pendingPaymentsCount++;
                items.Add(new NotificationItemViewModel
                {
                    BookingId = booking.BookingId,
                    BookingNo = booking.BookingNo ?? string.Empty,
                    CustomerName = customerName,
                    CustomerPhone = customerPhone,
                    RoomNumber = roomNumber,
                    Category = "Payment",
                    Title = "Pending Booking Payment",
                    Message = $"Booking #{booking.BookingNo} ({customerName}) has an outstanding balance of MMK {balanceAmount:N2}.",
                    CheckInDate = checkInDate,
                    CheckOutDate = checkOutDate,
                    TotalAmount = totalAmount,
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