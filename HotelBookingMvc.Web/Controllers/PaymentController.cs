using HotelBookingMvc.Web.Services;
using HotelBookingMvc.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelBookingMvc.Web.Controllers;

public class PaymentController : Controller
{
    private readonly Payment_Service _paymentService;

    public PaymentController(Payment_Service paymentService)
    {
        _paymentService = paymentService;
    }


    // Payment List
    public async Task<IActionResult> Index()
    {
        var payments =
            await _paymentService.GetPaymentsAsync();

        return View(payments);
    }


    // Create Payment
    [HttpGet]
    public async Task<IActionResult> Create(int? bookingId)
    {
        var model = new PaymentViewModel();

        if (bookingId.HasValue)
        {
            var summary =
                await _paymentService
                    .GetBookingPaymentSummaryAsync(
                        bookingId.Value);

            if (summary != null)
            {
                model = summary;
            }
        }

        await LoadBookings(model);

        return View(model);
    }


    // Save Payment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        PaymentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadBookings(model);
            return View(model);
        }


        var result =
            await _paymentService
                .CreatePaymentAsync(model);


        if (result.Success)
        {
            TempData["Success"] =
                result.Message;

            return RedirectToAction(nameof(Index));
        }


        ModelState.AddModelError(
            "",
            result.Message);

        var summary =
            await _paymentService
                .GetBookingPaymentSummaryAsync(
                    model.BookingId);

        if (summary != null)
        {
            model.BookingNo =
                summary.BookingNo;

            model.CustomerName =
                summary.CustomerName;

            model.TotalAmount =
                summary.TotalAmount;

            model.PaidAmount =
                summary.PaidAmount;

            model.RemainingAmount =
                summary.RemainingAmount;
        }

        await LoadBookings(model);

        return View(model);
    }


    // AJAX - Get booking payment information
    [HttpGet]
    public async Task<IActionResult> GetBookingSummary(
        int bookingId)
    {
        var summary =
            await _paymentService
                .GetBookingPaymentSummaryAsync(
                    bookingId);

        if (summary == null)
        {
            return Json(new
            {
                success = false
            });
        }

        return Json(new
        {
            success = true,

            bookingNo = summary.BookingNo,

            customerName =
                summary.CustomerName,

            totalAmount =
                summary.TotalAmount,

            paidAmount =
                summary.PaidAmount,

            remainingAmount =
                summary.RemainingAmount
        });
    }


    private async Task LoadBookings(
        PaymentViewModel model)
    {
        var bookings =
            await _paymentService
                .GetPaymentBookingsAsync();

        model.Bookings =
            bookings.Select(x => new SelectListItem
            {
                Value =
                    x.BookingId.ToString(),

                Text =
                    $"{x.BookingNo} - {x.TotalAmount:N2} MMK",

                Selected =
                    x.BookingId == model.BookingId

            }).ToList();
    }
}