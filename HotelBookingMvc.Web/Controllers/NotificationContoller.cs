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

    public async Task<IActionResult> Index()
    {
        var model =
            await _service.GetNotificationsAsync();

        return View(model);
    }

    public async Task<IActionResult> Count()
    {
        var model =
            await _service.GetNotificationsAsync();

        return Json(model.TotalNotifications);
    }
}