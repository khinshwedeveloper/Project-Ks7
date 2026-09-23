using HotelBookingMvc.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingMvc.Web.Controllers;

public class DashboardController : Controller
{
    private readonly Dashboard_Service _service;

    public DashboardController(Dashboard_Service service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var model =
            await _service.GetDashboardAsync();

        return View(model);
    }
}