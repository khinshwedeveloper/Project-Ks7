using Hotelbooking.Database.Context;
using HotelBookingMvc.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingMvc.Web.Services;

public class Dashboard_Service
{
    private readonly AppDbContext _context;

    public Dashboard_Service(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardViewModel> GetDashboardAsync(DateOnly? fromDate = null, DateOnly? toDate = null)
    {
        // 1. Room Inventory Snapshot
        var totalRooms = await _context.TblRooms.CountAsync();

        var available = await _context.TblRooms
            .CountAsync(x => x.Status != null && x.Status.StatusName == "Available");

        var reserved = await _context.TblRooms
            .CountAsync(x => x.Status != null && x.Status.StatusName == "Reserved");

        var occupied = await _context.TblRooms
            .CountAsync(x => x.Status != null && x.Status.StatusName == "Occupied");

        var maintenance = await _context.TblRooms
            .CountAsync(x => x.Status != null && x.Status.StatusName == "Maintenance");

        decimal occupancy = 0;
        if (totalRooms > 0)
        {
            occupancy = (decimal)occupied / totalRooms * 100;
        }

        // 2. Determine Date Boundaries
        DateTime fromDt;
        DateTime toDt;
        string dateRangeDesc;

        if (fromDate.HasValue && toDate.HasValue)
        {
            fromDt = fromDate.Value.ToDateTime(TimeOnly.MinValue);
            toDt = toDate.Value.ToDateTime(TimeOnly.MaxValue);
            dateRangeDesc = $"{fromDate.Value:dd MMM yyyy} - {toDate.Value:dd MMM yyyy}";
        }
        else if (fromDate.HasValue)
        {
            fromDt = fromDate.Value.ToDateTime(TimeOnly.MinValue);
            toDt = fromDate.Value.ToDateTime(TimeOnly.MaxValue);
            toDate = fromDate;
            dateRangeDesc = $"{fromDate.Value:dd MMM yyyy}";
        }
        else if (toDate.HasValue)
        {
            fromDt = toDate.Value.ToDateTime(TimeOnly.MinValue);
            toDt = toDate.Value.ToDateTime(TimeOnly.MaxValue);
            fromDate = toDate;
            dateRangeDesc = $"{toDate.Value:dd MMM yyyy}";
        }
        else
        {
            // Default to today's date
            var today = DateOnly.FromDateTime(DateTime.Today);
            fromDate = today;
            toDate = today;
            fromDt = today.ToDateTime(TimeOnly.MinValue);
            toDt = today.ToDateTime(TimeOnly.MaxValue);
            dateRangeDesc = $"Today ({today:dd MMM yyyy})";
        }

        // 3. Check-ins in selected period
        int periodCheckIns = await _context.TblBookingDetails
            .CountAsync(d => d.CheckInDateTime >= fromDt && d.CheckInDateTime <= toDt);

        // 4. Check-outs in selected period
        int periodCheckOuts = await _context.TblBookingDetails
            .CountAsync(d => d.CheckOutDateTime >= fromDt && d.CheckOutDateTime <= toDt);

        // 5. Revenue collected in selected period
        var revenue = await _context.TblPayments
            .Where(x => (x.PaymentStatus == "Paid" || x.PaymentStatus == "Partial") &&
                        x.PaymentDate >= fromDt && x.PaymentDate <= toDt)
            .SumAsync(x => (decimal?)x.Amount) ?? 0;

        // 6. Bookings active/created in selected period
        var bookingQuery = _context.TblBookings
            .Include(x => x.Customer)
            .Include(x => x.TblBookingDetails)
                .ThenInclude(d => d.Room)
            .Include(x => x.TblPayments)
            .Where(x => x.TblBookingDetails.Any(d => d.CheckInDateTime >= fromDt && d.CheckInDateTime <= toDt)
                     || (x.BookingDate >= fromDt && x.BookingDate <= toDt));

        var bookings = await bookingQuery
            .OrderByDescending(x => x.BookingDate)
            .ToListAsync();

        int totalBookings = bookings.Count;
        int confirmedBookings = bookings.Count(x => x.Status == "Confirmed");
        int checkedInBookings = bookings.Count(x => x.Status == "CheckedIn");
        int checkedOutBookings = bookings.Count(x => x.Status == "CheckedOut");
        int cancelledBookings = bookings.Count(x => x.Status == "Cancelled");

        decimal totalBookingAmount = bookings.Sum(x => x.TblBookingDetails.Sum(d => d.TotalRoomPrice));
        decimal totalPaidAmount = bookings.Sum(x => x.TblPayments.Where(p => p.PaymentStatus != "Refunded").Sum(p => p.Amount));
        decimal outstandingAmount = totalBookingAmount - totalPaidAmount;
        if (outstandingAmount < 0) outstandingAmount = 0;

        var recentBookings = bookings.Take(10).Select(b =>
        {
            var detail = b.TblBookingDetails.FirstOrDefault();
            var bPaid = b.TblPayments.Where(p => p.PaymentStatus != "Refunded").Sum(p => p.Amount);
            var bTotal = b.TblBookingDetails.Sum(d => d.TotalRoomPrice);
            var bBalance = bTotal - bPaid;
            if (bBalance < 0) bBalance = 0;

            return new BookingSummaryItem
            {
                BookingId = b.BookingId,
                BookingNo = b.BookingNo ?? "-",
                CustomerName = b.Customer?.Name ?? "Unknown",
                CustomerPhone = b.Customer?.Phone ?? "-",
                RoomNumber = detail?.Room?.RoomNo ?? (detail != null ? detail.RoomId.ToString() : "-"),
                Status = b.Status ?? "-",
                CheckInDate = detail != null ? DateOnly.FromDateTime(detail.CheckInDateTime) : DateOnly.FromDateTime(b.BookingDate),
                CheckOutDate = detail != null ? DateOnly.FromDateTime(detail.CheckOutDateTime) : DateOnly.FromDateTime(b.BookingDate.AddDays(1)),
                TotalAmount = bTotal,
                PaidAmount = bPaid,
                BalanceAmount = bBalance,
                BookingDate = b.BookingDate
            };
        }).ToList();

        return new DashboardViewModel
        {
            FromDate = fromDate,
            ToDate = toDate,
            DateRangeDescription = dateRangeDesc,
            TotalRooms = totalRooms,
            AvailableRooms = available,
            ReservedRooms = reserved,
            OccupiedRooms = occupied,
            MaintenanceRooms = maintenance,
            ActiveGuests = occupied,
            TodayCheckIns = periodCheckIns,
            TodayCheckOuts = periodCheckOuts,
            TotalRevenue = revenue,
            TotalBookings = totalBookings,
            ConfirmedBookings = confirmedBookings,
            CheckedInBookings = checkedInBookings,
            CheckedOutBookings = checkedOutBookings,
            CancelledBookings = cancelledBookings,
            TotalBookingAmount = totalBookingAmount,
            TotalPaidAmount = totalPaidAmount,
            TotalOutstandingAmount = outstandingAmount,
            OccupancyRate = occupancy,
            RecentBookings = recentBookings
        };
    }
}