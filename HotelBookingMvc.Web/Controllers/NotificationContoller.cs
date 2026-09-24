using HotelBookingMvc.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingMvc.Web.Controllers;

public class NotificationController : Controller
{
    private readonly NotificationService _service;

    public NotificationController(NotificationService service)
    {
        _service = service;
    }

    // Notification page
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = await _service.GetNotificationsAsync();

        return View(model);
    }

    // Notification badge count
    [HttpGet]
    public async Task<IActionResult> Count()
    {
        var model = await _service.GetNotificationsAsync();

        return Json(model.TotalNotifications);
    }

    // Keep this because your old Layout may still call GetCount
    [HttpGet]
    public async Task<IActionResult> GetCount()
    {
        var model = await _service.GetNotificationsAsync();

        return Json(model.TotalNotifications);
    }
}