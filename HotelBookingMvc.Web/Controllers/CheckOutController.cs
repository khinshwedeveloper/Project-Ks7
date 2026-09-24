using HotelBookingMvc.Web.Hubs;
using HotelBookingMvc.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace HotelBookingMvc.Web.Controllers;

public class CheckOutController : Controller
{
    private readonly CheckOut_Service _checkOutService;
    private readonly IHubContext<HotelHub> _hubContext;

    public CheckOutController(
        CheckOut_Service checkOutService,
        IHubContext<HotelHub> hubContext)
    {
        _checkOutService = checkOutService;
        _hubContext = hubContext;
    }


    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var bookings =
            await _checkOutService.GetCheckOutBookingsAsync();

        return View(bookings);
    }


    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var booking =
            await _checkOutService.GetCheckOutBookingAsync(id);

        if (booking == null)
        {
            TempData["Error"] =
                "Checked-in booking could not be found.";

            return RedirectToAction(nameof(Index));
        }

        return View(booking);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(int id)
    {
        bool result =
            await _checkOutService.CheckOutAsync(id);

        if (result)
        {
            TempData["Success"] =
                "Guest checked out successfully.";

            await _hubContext.Clients.All
                .SendAsync("HotelUpdated");
        }
        else
        {
            TempData["Error"] =
                "Unable to check out. Please make sure the booking is checked in and the payment balance is zero.";
        }

        return RedirectToAction(nameof(Index));
    }
}