namespace HotelBookingMvc.Web.ViewModels;

public class NotificationViewModel
{
    public int TodayCheckIns { get; set; }

    public int TodayCheckOuts { get; set; }

    public int PendingPayments { get; set; }

    public int TotalNotifications { get; set; }
}