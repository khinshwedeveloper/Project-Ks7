using Hotelbooking.Database.Models;
using HotelBookingMvc.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hotelbooking.Database.Context;

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


    public async Task<IActionResult> Index()
    {
        var rooms = await _roomService.GetRoomsAsync();

        ViewBag.RoomTypes = await _db.TblRoomTypes
            .OrderBy(x => x.RoomTypeName)
            .ToListAsync();

        return View(rooms);
    }

    
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.RoomTypes = await _db.TblRoomTypes
            .OrderBy(x => x.RoomTypeName)
            .ToListAsync();

        return View();
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TblRoom room)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.RoomTypes = await _db.TblRoomTypes
                .OrderBy(x => x.RoomTypeName)
                .ToListAsync();

            return View(room);
        }

        bool result = await _roomService.CreateRoomAsync(room);
        if (result)
        {
            TempData["Success"] = "Room created successfully.";

            return RedirectToAction(nameof(Index));
        }

        TempData["Error"] = "Unable to create room.";

        ViewBag.RoomTypes = await  _db.TblRoomTypes
            .OrderBy(x => x.RoomTypeName)
            .ToListAsync();

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
        ViewBag.RoomTypes = await _db.TblRoomTypes
            .OrderBy(x => x.RoomTypeName)
            .ToListAsync();
        return View(room);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult>Edit(TblRoom room)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.RoomTypes = await _db.TblRoomTypes
                .OrderBy(x => x.RoomTypeName)
                .ToListAsync();
            return View(room);
        }

        bool result = await _roomService.UpdateRoomAsync(room);
        if(result)
        {
            TempData["Success"] = "Room Update Succesfully.";
            return RedirectToAction(nameof(Index));
        }
        TempData["Error"] = "Unable to update room.";

        ViewBag.RoomTypes = await _db.TblRoomTypes
            .OrderBy(x => x.RoomTypeName)
            .ToListAsync();
        return View(room);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult>Delete(int id)
    {
        bool result = await _roomService.DeleteRoomAsync(id);

        if(result)
        {
            TempData["Success"] = "Room delete succesfully.";
        }
        else
        {
            TempData["Error"] = "Unable to delete";
        }

        return RedirectToAction(nameof(Index));
    }
}