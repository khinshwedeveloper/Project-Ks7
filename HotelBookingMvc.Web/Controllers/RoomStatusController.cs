using HotelBookingMvc.Web.Services;
using Hotelbooking.Database.Models;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingMvc.Web.Controllers;

public class RoomStatusController : Controller
{
    private readonly RoomStatus_Service _roomStatusService;

    public RoomStatusController(RoomStatus_Service roomStatusService)
    {
        _roomStatusService = roomStatusService;
    }

    public async Task<IActionResult> Index()
    {
        var statuses = await _roomStatusService.GetRoomStatusesAsync();

        return View(statuses);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TblRoomStatus roomStatus)
    {
        if (!ModelState.IsValid)
        {
            return View(roomStatus);
        }

        var result = await _roomStatusService
            .CreateRoomStatusAsync(roomStatus);

        if (!result)
        {
            ModelState.AddModelError(
                "StatusName",
                "Room status already exists or could not be saved.");

            return View(roomStatus);
        }

        TempData["Success"] = "Room status created successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var roomStatus = await _roomStatusService
            .GetRoomStatusByIdAsync(id);

        if (roomStatus == null)
        {
            return NotFound();
        }

        return View(roomStatus);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TblRoomStatus roomStatus)
    {
        if (!ModelState.IsValid)
        {
            return View(roomStatus);
        }

        var result = await _roomStatusService
            .UpdateRoomStatusAsync(roomStatus);

        if (!result)
        {
            ModelState.AddModelError(
                "StatusName",
                "Room status already exists or could not be updated.");

            return View(roomStatus);
        }

        TempData["Success"] = "Room status updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var roomStatus = await _roomStatusService
            .GetRoomStatusByIdAsync(id);

        if (roomStatus == null)
        {
            return NotFound();
        }

        return View(roomStatus);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _roomStatusService
            .DeleteRoomStatusAsync(id);

        if (!result)
        {
            TempData["Error"] =
                "Room status could not be deleted. It may already be used by a room.";

            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Room status deleted successfully.";

        return RedirectToAction(nameof(Index));
    }
}