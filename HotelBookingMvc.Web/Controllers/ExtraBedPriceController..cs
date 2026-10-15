
using HotelBookingMvc.Web.Services;
using Hotelbooking.Database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelBookingMvc.Web.Controllers
{
    public class ExtraBedPriceController : Controller
    {
        private readonly ExtraBedPrice_Service _service;

        public ExtraBedPriceController(ExtraBedPrice_Service service)
        {
            _service = service;
        }

        private async Task LoadRoomTypesAsync(int? selectedId = null)
        {
            var roomTypes = await _service.GetRoomTypesAsync();

            ViewBag.RoomTypes = new SelectList(
                roomTypes,
                "RoomTypeId",
                "RoomTypeName",
                selectedId);
        }

        public async Task<IActionResult> Index()
        {
            return View(await _service.GetAllAsync());
        }

        public async Task<IActionResult> Create()
        {
            await LoadRoomTypesAsync();

            return View(new TblExtraBedPrice
            {
                EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                Status = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TblExtraBedPrice item)
        {
            if (item.Price < 0)
                ModelState.AddModelError(nameof(item.Price), "Price cannot be negative.");

            if (ModelState.IsValid)
            {
                await _service.CreateAsync(item);
                TempData["Success"] = "Extra bed price created successfully.";
                return RedirectToAction(nameof(Index));
            }

            await LoadRoomTypesAsync(item.RoomTypeId);
            return View(item);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _service.GetByIdAsync(id);

            if (item == null)
                return NotFound();

            await LoadRoomTypesAsync(item.RoomTypeId);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TblExtraBedPrice item)
        {
            if (item.Price < 0)
                ModelState.AddModelError(nameof(item.Price), "Price cannot be negative.");

            if (ModelState.IsValid)
            {
                var updated = await _service.UpdateAsync(item);

                if (!updated)
                    return NotFound();

                TempData["Success"] = "Extra bed price updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            await LoadRoomTypesAsync(item.RoomTypeId);
            return View(item);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var item = await _service.GetByIdAsync(id);

            if (item == null)
                return NotFound();

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            TempData["Success"] = "Extra bed price deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
