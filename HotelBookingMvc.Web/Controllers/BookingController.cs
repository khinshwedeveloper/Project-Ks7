using HotelBookingMvc.Web.Hubs;
using HotelBookingMvc.Web.Services;
using HotelBookingMvc.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;

namespace HotelBookingMvc.Web.Controllers;

public class BookingController : Controller
{
    private readonly Booking_Service _bookingService;
    private readonly IHubContext<HotelHub> _hubContext;

    public BookingController(
        Booking_Service bookingService,
        IHubContext<HotelHub> hubContext)
    {
        _bookingService = bookingService;
        _hubContext = hubContext;
    }

    // =========================
    // BOOKING LIST
    // =========================
    public async Task<IActionResult> Index()
    {
        var bookings = await _bookingService.GetBookingsAsync();
        return View(bookings);
    }

    // =========================
    // CREATE GET
    // =========================
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new BookingCreateViewModel
        {
            CheckInDate = DateOnly.FromDateTime(DateTime.Today),
            CheckOutDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        };

        await LoadCustomers(model);
        await LoadRooms(model);

        return View(model);
    }

    // =========================
    // CREATE POST
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingCreateViewModel model)
    {
        if (model.CheckOutDate <= model.CheckInDate)
        {
            ModelState.AddModelError(
                "CheckOutDate",
                "Check-out date must be after check-in date.");
        }

        if (!ModelState.IsValid)
        {
            await LoadCustomers(model);
            await LoadRooms(model);
            return View(model);
        }

        bool result = await _bookingService.CreateBookingAsync(model);

        if (result)
        {
            TempData["Success"] = "Booking Created Successfully!";
            await _hubContext.Clients.All.SendAsync("HotelUpdated");
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(
            "",
            "Unable to create booking. The selected room may already be booked.");

        await LoadCustomers(model);
        await LoadRooms(model);
        return View(model);
    }

    // =========================
    // DETAILS
    // =========================
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);
        if (booking == null)
            return NotFound();

        var detail = booking.TblBookingDetails.FirstOrDefault();
        if (detail == null)
            return NotFound();

        var room = detail.Room ?? await _bookingService.GetRoomByIdAsync(detail.RoomId);
        string roomTypeName = room?.RoomType?.RoomTypeName ?? "Standard Room";

        int nights = (detail.CheckOutDateTime - detail.CheckInDateTime).Days;
        if (nights <= 0) nights = 1;

        var model = new BookingDetailViewModel
        {
            BookingId = booking.BookingId,
            BookingNo = booking.BookingNo ?? string.Empty,
            CustomerName = booking.Customer?.Name ?? "Unknown",
            CustomerPhone = booking.Customer?.Phone,
            RoomNumber = room?.RoomNo ?? "Unknown",
            RoomTypeName = roomTypeName,
            CheckInDate = DateOnly.FromDateTime(detail.CheckInDateTime),
            CheckOutDate = DateOnly.FromDateTime(detail.CheckOutDateTime),
            NumberOfNights = nights,
            PricePerNight = detail.RoomPrice,
            ExtraBedQty = detail.ExtraBedQty,
            ExtraBedPrice = detail.ExtraBedPrice,
            Amount = detail.TotalRoomPrice,
            Status = booking.Status ?? string.Empty,
            BookingDate = booking.BookingDate
        };

        return View(model);
    }

    // =========================
    // CONFIRM
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(int id)
    {
        bool result = await _bookingService.ConfirmBookingAsync(id);

        if (result)
        {
            TempData["Success"] = "Booking Confirmed Successfully!";
            await _hubContext.Clients.All.SendAsync("HotelUpdated");
        }
        else
        {
            TempData["Error"] = "Unable to confirm booking.";
        }

        return RedirectToAction(nameof(Index));
    }

    // =========================
    // CANCEL
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        bool result = await _bookingService.CancelBookingAsync(id);

        if (result)
        {
            TempData["Success"] = "Booking Cancelled Successfully!";
            await _hubContext.Clients.All.SendAsync("HotelUpdated");
        }
        else
        {
            TempData["Error"] = "Unable to cancel booking.";
        }

        return RedirectToAction(nameof(Index));
    }

    // =========================
    // AJAX: AVAILABLE ROOMS
    // =========================
    [HttpGet]
    public async Task<IActionResult> GetAvailableRooms(
        string checkIn,
        string checkOut)
    {
        if (!DateOnly.TryParse(checkIn, out DateOnly inDate))
            return Json(new List<object>());

        if (!DateOnly.TryParse(checkOut, out DateOnly outDate))
            return Json(new List<object>());

        if (outDate <= inDate)
            return Json(new List<object>());

        var rooms = await _bookingService.GetAvailableRoomsAsync(inDate, outDate);

        var result = rooms.Select(x => new
        {
            id = x.RoomId,
            roomNumber = x.RoomNo,
            floor = x.Floor?.FloorName ?? $"Floor {x.FloorId}"
        });

        return Json(result);
    }

    // =========================
    // AJAX: ROOM PRICE
    // =========================
    [HttpGet]
    public async Task<IActionResult> GetRoomPrice(int roomId)
    {
        var price = await _bookingService.GetRoomPriceAsync(roomId);

        if (price == null)
            return Json(new { success = false });

        return Json(new
        {
            success = true,
            price = price.Value
        });
    }

    // =========================
    // LOAD CUSTOMERS
    // =========================
    private async Task LoadCustomers(BookingCreateViewModel model)
    {
        var customers = await _bookingService.GetCustomersAsync();

        model.Customers = customers
            .Select(x => new SelectListItem
            {
                Value = x.CustomerId.ToString(),
                Text = $"{x.Name} ({x.CustomerCode}) - {x.Phone}"
            })
            .ToList();
    }

    // =========================
    // LOAD ROOMS
    // =========================
    private async Task LoadRooms(BookingCreateViewModel model)
    {
        var rooms = await _bookingService.GetAvailableRoomsAsync(
            model.CheckInDate,
            model.CheckOutDate);

        model.Rooms = rooms
            .Select(x => new SelectListItem
            {
                Value = x.RoomId.ToString(),
                Text = $"Room {x.RoomNo} - {x.Floor?.FloorName ?? $"Floor {x.FloorId}"} ({x.RoomType?.RoomTypeName})"
            })
            .ToList();
    }
}