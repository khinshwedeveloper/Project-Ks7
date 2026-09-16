using Microsoft.AspNetCore.Mvc;

namespace HotelBookingMvc.Web.Controllers;

public class NotificationController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult GetCount()
    {
        return Json(new
        {
            count = 0
        });
    }
}