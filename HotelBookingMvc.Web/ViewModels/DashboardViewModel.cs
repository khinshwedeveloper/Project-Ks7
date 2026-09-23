namespace HotelBookingMvc.Web.ViewModels;

public class DashboardViewModel
{
    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
    public int ReservedRooms { get; set; }
    public int OccupiedRooms { get; set; }
    public int MaintenanceRooms { get; set; }

    public int ActiveGuests { get; set; }

    public int TodayCheckIns { get; set; }
    public int TodayCheckOuts { get; set; }

    public decimal TotalRevenue { get; set; }

    public decimal OccupancyRate { get; set; }
}