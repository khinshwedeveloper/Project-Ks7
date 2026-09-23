using HotelBookingMvc.Web.Hubs;
using HotelBookingMvc.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace HotelBookingMvc.Web.Controllers;

public class CheckInController : Controller
{
    private readonly CheckIn_Service _checkInService;
    private readonly IHubContext<HotelHub> _hubContext;

    public CheckInController(
        CheckIn_Service checkInService,
        IHubContext<HotelHub> hubContext)
    {
        _checkInService = checkInService;
        _hubContext = hubContext;
    }


    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var bookings =
            await _checkInService.GetCheckInBookingsAsync();

        return View(bookings);
    }


  
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(int id)
    {
        bool result =
            await _checkInService.CheckInAsync(id);

        if (result)
        {
            TempData["Success"] =
                "Guest checked in successfully.";

            await _hubContext.Clients.All
                .SendAsync("HotelUpdated");
        }
        else
        {
            TempData["Error"] =
                "Unable to check in. Please check booking date, booking status, and room status.";
        }

        return RedirectToAction(nameof(Index));
    }
}