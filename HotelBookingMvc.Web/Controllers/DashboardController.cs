using HotelBookingMvc.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingMvc.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly Dashboard_Service _service;

    public DashboardController(Dashboard_Service service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DateOnly? fromDate, DateOnly? toDate)
    {
        if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
        {
            TempData["Error"] = "From date cannot be later than To date.";
            var temp = fromDate;
            fromDate = toDate;
            toDate = temp;
        }

        var model = await _service.GetDashboardAsync(fromDate, toDate);

        return View(model);
    }
}