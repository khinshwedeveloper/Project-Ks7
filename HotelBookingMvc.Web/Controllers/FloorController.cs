using HotelBookingMvc.Web.Services;
using Hotelbooking.Database.Models;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingMvc.Web.Controllers;

public class FloorController : Controller
{
    private readonly Floor_Service _floorService;

    public FloorController(Floor_Service floorService)
    {
        _floorService = floorService;
    }

    public async Task<IActionResult> Index()
    {
        var floors = await _floorService.GetFloorsAsync();

        return View(floors);
    }


    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TblFloor floor)
    {
        if (!ModelState.IsValid)
        {
            return View(floor);
        }

        var result = await _floorService.CreateFloorAsync(floor);

        if (!result)
        {
            ModelState.AddModelError(
                "FloorName",
                "Floor name already exists or could not be saved.");

            return View(floor);
        }

        TempData["Success"] = "Floor created successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var floor = await _floorService.GetFloorByIdAsync(id);

        if (floor == null)
        {
            return NotFound();
        }

        return View(floor);
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TblFloor floor)
    {
        if (!ModelState.IsValid)
        {
            return View(floor);
        }

        var result = await _floorService.UpdateFloorAsync(floor);

        if (!result)
        {
            ModelState.AddModelError(
                "FloorName",
                "Floor name already exists or could not be updated.");

            return View(floor);
        }

        TempData["Success"] = "Floor updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var floor = await _floorService.GetFloorByIdAsync(id);

        if (floor == null)
        {
            return NotFound();
        }

        return View(floor);
    }

 
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _floorService.DeleteFloorAsync(id);

        if (!result)
        {
            TempData["Error"] = "Floor could not be deleted.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Floor deleted successfully.";

        return RedirectToAction(nameof(Index));
    }
}

