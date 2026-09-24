namespace HotelBookingMvc.Web.ViewModels;

public class CheckOutViewModel
{
    public int BookingId { get; set; }

    public string BookingNo { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerPhone { get; set; } = string.Empty;

    public string RoomNumber { get; set; } = string.Empty;

    public DateOnly CheckInDate { get; set; }

    public DateOnly CheckOutDate { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal BalanceAmount { get; set; }

    public string Status { get; set; } = string.Empty;
}