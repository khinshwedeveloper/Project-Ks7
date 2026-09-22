using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelBookingMvc.Web.ViewModels;

public class PaymentViewModel
{
    public int PaymentId { get; set; }

    [Required(ErrorMessage = "Please select a booking.")]
    [Display(Name = "Booking")]
    public int BookingId { get; set; }

    public string BookingNo { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal RemainingAmount { get; set; }

    [Required(ErrorMessage = "Please enter payment amount.")]
    [Range(0.01, double.MaxValue,
        ErrorMessage = "Payment amount must be greater than 0.")]
    [Display(Name = "Payment Amount")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Please select payment method.")]
    [Display(Name = "Payment Method")]
    public string PaymentMethod { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = string.Empty;

    public DateTime PaymentDate { get; set; }

    public List<SelectListItem> Bookings { get; set; } = new();
}