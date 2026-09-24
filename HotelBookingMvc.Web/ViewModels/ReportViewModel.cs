using System;

namespace HotelBookingMvc.Web.ViewModels;

public class ReportViewModel
{
    // ================================
    // FILTER
    // ================================

    public DateOnly? FromDate { get; set; }

    public DateOnly? ToDate { get; set; }


    // ================================
    // SUMMARY
    // ================================

    public int TotalBookings { get; set; }

    public int ConfirmedBookings { get; set; }

    public int CheckedInBookings { get; set; }

    public int CheckedOutBookings { get; set; }

    public int CancelledBookings { get; set; }


    // ================================
    // REVENUE
    // ================================

    public decimal TotalBookingAmount { get; set; }

    public decimal TotalPaidAmount { get; set; }

    public decimal TotalOutstandingAmount { get; set; }


    // ================================
    // ROOM STATUS
    // ================================

    public int TotalRooms { get; set; }

    public int AvailableRooms { get; set; }

    public int ReservedRooms { get; set; }

    public int OccupiedRooms { get; set; }

    public int MaintenanceRooms { get; set; }


    // ================================
    // BOOKING REPORT
    // ================================

    public List<BookingReportItem> Bookings { get; set; }
        = new();


    // ================================
    // PAYMENT REPORT
    // ================================

    public List<PaymentReportItem> Payments { get; set; }
        = new();
}


// ====================================
// BOOKING REPORT ITEM
// ====================================

public class BookingReportItem
{
    public int BookingId { get; set; }

    public string BookingNo { get; set; }
        = string.Empty;

    public string CustomerName { get; set; }
        = string.Empty;

    public string CustomerPhone { get; set; }
        = string.Empty;

    public string RoomNumber { get; set; }
        = string.Empty;

    public string Status { get; set; }
        = string.Empty;

    public DateOnly CheckInDate { get; set; }

    public DateOnly CheckOutDate { get; set; }

    public int NumberOfNights { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal BalanceAmount { get; set; }
}


// ====================================
// PAYMENT REPORT ITEM
// ====================================

public class PaymentReportItem
{
    public int PaymentId { get; set; }

    public int BookingId { get; set; }

    public string BookingNo { get; set; }
        = string.Empty;

    public string CustomerName { get; set; }
        = string.Empty;

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; }
        = string.Empty;

    public string PaymentStatus { get; set; }
        = string.Empty;

    public DateTime PaymentDate { get; set; }
}