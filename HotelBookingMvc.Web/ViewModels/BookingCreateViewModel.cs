using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HotelBookingMvc.Web.ViewModels;

public class BookingCreateViewModel
{
    [Required]
    [Display(Name = "Customer")]
    public int CustomerId { get; set; }

    [Required]
    [Display(Name = "Check-in Date")]
    public DateOnly CheckInDate { get; set; }

    [Required]
    [Display(Name = "Check-out Date")]
    public DateOnly CheckOutDate { get; set; }

    [Required]
    [Display(Name = "Room")]
    public int RoomId { get; set; }

    public decimal PricePerNight { get; set; }

    public int ExtraBedQty { get; set; } = 0;

    public decimal ExtraBedPrice { get; set; } = 0;

    public int NumberOfNights { get; set; }

    public decimal Amount { get; set; }

    public string? Remarks { get; set; }

    public List<SelectListItem> Customers { get; set; } = new();
    public List<SelectListItem> Rooms { get; set; } = new();
}
