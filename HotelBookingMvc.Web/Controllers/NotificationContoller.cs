using HotelBookingMvc.Web.Services;
using Microsoft.AspNetCore.Mvc; 
namespace Hotelbooking.Web.Controllers;

public class NotificationContoller : Controller
{
    private readonly NotificationService _notificationService;
    public NotificationContoller(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }
    public async Task <IActionResult> Index()
    {
        var notification=await _notificationService.GetNotificationAsync();
        return View(notification);
    }
    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        var result = await _notificationService.GetNotificationAsync();
        return Json(result);
    }
}