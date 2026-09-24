namespace HotelBookingMvc.Web.ViewModels;

public class NotificationViewModel
{
    public int TodayCheckIns { get; set; }

    public int TodayCheckOuts { get; set; }

    public int PendingPayments { get; set; }

    public int TotalNotifications { get; set; }

    public List<NotificationItemViewModel> Items { get; set; } = new();
}

public class NotificationItemViewModel
{
    public int BookingId { get; set; }
    public string BookingNo { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // CheckIn, CheckOut, Payment
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public string ActionUrl { get; set; } = string.Empty;
    public string ActionText { get; set; } = string.Empty;
    public bool IsOverdue { get; set; }
}