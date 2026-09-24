using HotelBookingMvc.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingMvc.Web.Controllers
{
    public class ReportController : Controller
    {
        private readonly Report_Service report_Service;

        public ReportController(Report_Service report_Service)
        {
            this.report_Service = report_Service;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            DateOnly? fromDate,
            DateOnly? toDate)
        {
            if (fromDate.HasValue &&
                toDate.HasValue &&
                fromDate.Value > toDate.Value)
            {
                TempData["Error"] =
                    "From date cannot be later To Date.";

                fromDate = null;
                toDate = null;
            }

            var model =
                await report_Service.GetReportAsync(
                    fromDate,
                    toDate);
            return View(model);
        }
    }
}
