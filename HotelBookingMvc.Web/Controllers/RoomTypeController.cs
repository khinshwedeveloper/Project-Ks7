using Hotelbooking.Database.Models;
using HotelBookingMvc.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace HotelBookingMvc.Web.Controllers;

public class RoomTypeController : Controller
{
    private readonly RoomType_Service _roomTypeService;

    public RoomTypeController(RoomType_Service roomTypeService)
    {
        _roomTypeService = roomTypeService;
    }

    public async Task<IActionResult> Index()
    {
        var roomTypes = await _roomTypeService.GetRoomTypesAsync();

        return View(roomTypes);
    }
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TblRoomType roomType)
    {
        ModelState.Remove(nameof(roomType.TblRooms));
        ModelState.Remove(nameof(roomType.TblRoomPrices));
        ModelState.Remove(nameof(roomType.TblExtraBedPrices));

        if (!ModelState.IsValid)
        {
            return View(roomType);
        }
        bool result = await _roomTypeService.CreateRoomTypeAsync(roomType);

        if (result)
        {
            TempData["Success"] = "Room Type Created Successfully!";
            return RedirectToAction(nameof(Index));
        }
        TempData["Error"] = "Unable To create roomtype.";
        return View(roomType);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var roomType = await _roomTypeService.GetRoomTypeByIdAsync(id);

        if (roomType == null)
        {
            return NotFound();
        }
        return View(roomType);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit (TblRoomType roomType)
    {
        ModelState.Remove(nameof(roomType.TblRooms));
        ModelState.Remove(nameof(roomType.TblRoomPrices));
        ModelState.Remove(nameof(roomType.TblExtraBedPrices));

        if (!ModelState.IsValid)
        {
            return View(roomType);
        }
        bool result = await _roomTypeService.UpdateRoomTypeAsync(roomType);
        if (result)
        {
            TempData["Success"] = "Room Type Updated Successfully!";
                return RedirectToAction(nameof(Index));
        }
        TempData["Error"] = "Unable to Update room type.";
        return View(roomType);
    }
    
[HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        bool result = await _roomTypeService.DeleteRoomTypeAsync(id);

        if (result)
        {
            TempData["Success"] = "Room Type Deleted Successfully!";
        }
        else
        {
            TempData["Error"] = "Unable to delete room type.";
        }
        return RedirectToAction(nameof(Index));
    }
}
    

