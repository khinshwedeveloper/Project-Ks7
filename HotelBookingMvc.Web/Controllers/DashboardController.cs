using HotelBookingMvc.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingMvc.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly DashboardService _dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetData()
        {
            var result = await _dashboardService.GetDashboardAsync();

            return Json(result);
        }
    }
}