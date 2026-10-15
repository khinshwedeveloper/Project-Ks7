
using HotelBookingMvc.Web.Services;
using Hotelbooking.Database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelBookingMvc.Web.Controllers
{
    public class RoomPriceDetailController : Controller
    {
        private readonly RoomPrice_service _service;

        public RoomPriceDetailController(RoomPrice_service service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(int priceId)
        {
            var price = await _service.GetPriceAsync(priceId);

            if (price == null)
                return NotFound();

            ViewBag.PriceId = priceId;
            ViewBag.PriceName = price.PriceName;
            ViewBag.RoomTypeName = price.RoomType?.RoomTypeName;

            var details = await _service.GetDetailsAsync(priceId);
            return View(details);
        }

        public async Task<IActionResult> Create(int priceId)
        {
            var price = await _service.GetPriceAsync(priceId);

            if (price == null)
                return NotFound();

            ViewBag.PriceId = priceId;
            ViewBag.PriceName = price.PriceName;

            return View(new TblRoomPriceDetail
            {
                RoomPriceId = priceId,
                StartTime = new TimeOnly(6, 0),
                EndTime = new TimeOnly(12, 0),
                Status = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TblRoomPriceDetail detail)
        {
            ModelState.Remove(nameof(detail.RoomPrice));

            var price = await _service.GetPriceAsync(detail.RoomPriceId);

            if (price == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.PriceId = detail.RoomPriceId;
                ViewBag.PriceName = price.PriceName;
                return View(detail);
            }

            try
            {
                await _service.AddDetailAsync(detail);
                TempData["Success"] = "Time slot added successfully.";

                return RedirectToAction(
                    nameof(Index),
                    new { priceId = detail.RoomPriceId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.PriceId = detail.RoomPriceId;
                ViewBag.PriceName = price.PriceName;
                return View(detail);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var detail = await _service.GetDetailAsync(id);

            if (detail == null)
                return NotFound();

            ViewBag.PriceId = detail.RoomPriceId;
            ViewBag.PriceName = detail.RoomPrice?.PriceName;

            return View(detail);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, TblRoomPriceDetail detail)
        {
            if (id != detail.RoomPriceDetailId)
                return BadRequest();

            ModelState.Remove(nameof(detail.RoomPrice));

            var price = await _service.GetPriceAsync(detail.RoomPriceId);

            if (price == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.PriceId = detail.RoomPriceId;
                ViewBag.PriceName = price.PriceName;
                return View(detail);
            }

            try
            {
                await _service.UpdateDetailAsync(detail);
                TempData["Success"] = "Time slot updated successfully.";

                return RedirectToAction(
                    nameof(Index),
                    new { priceId = detail.RoomPriceId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.PriceId = detail.RoomPriceId;
                ViewBag.PriceName = price.PriceName;
                return View(detail);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var detail = await _service.GetDetailAsync(id);

            if (detail == null)
                return NotFound();

            return View(detail);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var detail = await _service.GetDetailAsync(id);

            if (detail == null)
                return NotFound();

            int priceId = detail.RoomPriceId;

            try
            {
                await _service.DeleteDetailAsync(id);
                TempData["Success"] = "Time slot deleted successfully.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Could not delete this time slot.";
            }

            return RedirectToAction(
                nameof(Index),
                new { priceId });
        }

        private async Task LoadRoomPrices(int selectedId)
        {
            var prices = await _service.GetPriceAsync();

            ViewBag.RoomPrices = new SelectList(
                prices,
                "RoomPriceId",
                "PriceName",
                selectedId);
        }
    }
}
