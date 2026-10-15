using System;
using System.Collections.Generic;

namespace HotelBookingMvc.Web.ViewModels;

public class DashboardViewModel
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public string DateRangeDescription { get; set; } = string.Empty;

    // Room status summary
    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
    public int ReservedRooms { get; set; }
    public int OccupiedRooms { get; set; }
    public int MaintenanceRooms { get; set; }

    public int ActiveGuests { get; set; }

    // Period / Filtered Booking Stats
    public int TotalBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int CheckedInBookings { get; set; }
    public int CheckedOutBookings { get; set; }
    public int CancelledBookings { get; set; }

    // Check-in / Check-out counts for the selected date range
    public int TodayCheckIns { get; set; }
    public int TodayCheckOuts { get; set; }

    // Revenue in the selected date range
    public decimal TotalRevenue { get; set; }
    public decimal TotalBookingAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal TotalOutstandingAmount { get; set; }

    public decimal OccupancyRate { get; set; }

    public List<BookingSummaryItem> RecentBookings { get; set; } = new();
}

public class BookingSummaryItem
{
    public int BookingId { get; set; }
    public string BookingNo { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public DateTime BookingDate { get; set; }
}