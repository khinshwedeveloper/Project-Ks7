using Microsoft.AspNetCore.Mvc;

namespace HotelBookingMvc.Web.Controllers;

public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}