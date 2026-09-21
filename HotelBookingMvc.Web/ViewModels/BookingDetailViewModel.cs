namespace HotelBookingMvc.Web.ViewModels;

public class BookingDetailViewModel
{
    public int BookingId { get; set; }

    public string BookingNo { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string? CustomerPhone { get; set; }

    public string RoomNumber { get; set; } = string.Empty;

    public string RoomTypeName { get; set; } = string.Empty;

    public DateOnly CheckInDate { get; set; }

    public DateOnly CheckOutDate { get; set; }

    public int NumberOfNights { get; set; }

    public decimal PricePerNight { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime BookingDate { get; set; }
}