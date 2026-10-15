using Hotelbooking.Database.Context;
using Hotelbooking.Database.Models;
using HotelBookingMvc.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingMvc.Web.Controllers;

public class RoomController : Controller
{
    private readonly Room_Service _roomService;
    private readonly AppDbContext _db;

    public RoomController(
        Room_Service roomService,
        AppDbContext db)
    {
        _roomService = roomService;
        _db = db;
    }

    private async Task LoadDropdowns()
    {
        ViewBag.Floors = await _db.TblFloors
            .Where(x => x.Status)
            .OrderBy(x => x.FloorName)
            .ToListAsync();

        ViewBag.RoomTypes = await _db.TblRoomTypes
            .Where(x => x.Status)
            .OrderBy(x => x.RoomTypeName)
            .ToListAsync();

        ViewBag.RoomStatuses = await _db.TblRoomStatuses
            .OrderBy(x => x.StatusName)
            .ToListAsync();
    }

    public async Task<IActionResult> Index()
    {
        var rooms = await _roomService.GetRoomsAsync();
        await LoadDropdowns();
        return View(rooms);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadDropdowns();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TblRoom room)
    {
        ModelState.Remove(nameof(room.Floor));
        ModelState.Remove(nameof(room.RoomType));
        ModelState.Remove(nameof(room.Status));
        ModelState.Remove(nameof(room.TblBookingDetails));
        ModelState.Remove(nameof(room.TblRoomTransferFromRooms));
        ModelState.Remove(nameof(room.TblRoomTransferToRooms));

        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            return View(room);
        }

        bool result = await _roomService.CreateRoomAsync(room);
        if (result)
        {
            TempData["Success"] = "Room Created Successfully!";
            return RedirectToAction(nameof(Index));
        }

        TempData["Error"] = "Unable to create room.";
        await LoadDropdowns();
        return View(room);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);
        if (room == null)
        {
            return NotFound();
        }
        await LoadDropdowns();
        return View(room);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TblRoom room)
    {
        ModelState.Remove(nameof(room.Floor));
        ModelState.Remove(nameof(room.RoomType));
        ModelState.Remove(nameof(room.Status));
        ModelState.Remove(nameof(room.TblBookingDetails));
        ModelState.Remove(nameof(room.TblRoomTransferFromRooms));
        ModelState.Remove(nameof(room.TblRoomTransferToRooms));

        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            return View(room);
        }

        bool result = await _roomService.UpdateRoomAsync(room);
        if (result)
        {
            TempData["Success"] = "Room Updated Successfully!";
            return RedirectToAction(nameof(Index));
        }
        TempData["Error"] = "Unable to update room.";
        await LoadDropdowns();
        return View(room);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        bool result = await _roomService.DeleteRoomAsync(id);

        if (result)
        {
            TempData["Success"] = "Room Deleted Successfully!";
        }
        else
        {
            TempData["Error"] = "Unable to delete room.";
        }

        return RedirectToAction(nameof(Index));
    }
}