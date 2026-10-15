
using HotelBookingMvc.Web.Services;
using Hotelbooking.Database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelBookingMvc.Web.Controllers
{
    public class RoomPriceController : Controller
    {
        private readonly RoomPrice_service _service;

        public RoomPriceController(RoomPrice_service service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var prices = await _service.GetPriceAsync();
            return View(prices);
        }

        public async Task<IActionResult> Create()
        {
            await LoadRoomTypes();
            return View(new TblRoomPrice
            {
                EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                Status = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TblRoomPrice price)
        {
            ModelState.Remove(nameof(price.RoomType));
            ModelState.Remove(nameof(price.TblRoomPriceDetails));

            if (!ModelState.IsValid)
            {
                await LoadRoomTypes();
                return View(price);
            }

            try
            {
                await _service.AddPriceAsync(price);
                TempData["Success"] = "Room price created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Could not save room price.");
                await LoadRoomTypes();
                return View(price);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var price = await _service.GetPriceAsync(id);

            if (price == null)
                return NotFound();

            await LoadRoomTypes();
            return View(price);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TblRoomPrice price)
        {
            if (id != price.RoomPriceId)
                return BadRequest();

            ModelState.Remove(nameof(price.RoomType));
            ModelState.Remove(nameof(price.TblRoomPriceDetails));

            if (!ModelState.IsValid)
            {
                await LoadRoomTypes();
                return View(price);
            }

            try
            {
                await _service.UpdatePriceAsync(price);
                TempData["Success"] = "Room price updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Could not update room price.");
                await LoadRoomTypes();
                return View(price);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var price = await _service.GetPriceAsync(id);

            if (price == null)
                return NotFound();

            return View(price);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _service.DeletePriceAsync(id);
                TempData["Success"] = "Room price deleted successfully.";
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Could not delete this room price.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadRoomTypes()
        {
            var types = await _service.GetRoomTypesAsync();

            ViewBag.RoomTypes = new SelectList(
                types,
                "RoomTypeId",
                "RoomTypeName");
        }
    }
}
