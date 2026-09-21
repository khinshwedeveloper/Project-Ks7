using HotelBookingMvc.Web.Services;
using HotelBookingMvc.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelBookingMvc.Web.Controllers;

public class BookingController : Controller
{
    private readonly Booking_Service _bookingService;

    public BookingController(Booking_Service bookingService)
    {
        _bookingService = bookingService;
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

        model.Rooms = new List<SelectListItem>();

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

        bool result =
            await _bookingService.CreateBookingAsync(model);

        if (result)
        {
            TempData["Success"] =
                "Booking created successfully.";

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
        var booking =
            await _bookingService.GetBookingByIdAsync(id);

        if (booking == null)
            return NotFound();

        var detail = booking.TblBookingDetails.FirstOrDefault();

        if (detail == null)
            return NotFound();

        var room =
            await _bookingService.GetRoomByIdAsync(detail.Roomid);

        string roomTypeName = "Unknown";

        if (room != null)
        {
            var roomType =
                await _bookingService.GetRoomTypeByIdAsync(
                    room.RoomTypeId);

            if (roomType != null)
                roomTypeName = roomType.RoomTypeName;
        }

        var model = new BookingDetailViewModel
        {
            BookingId = booking.BookingId,
            BookingNo = booking.BookingNo,
            CustomerName =
                booking.Customer?.CustomerName ?? "Unknown",
            CustomerPhone =
                booking.Customer?.Phone,
            RoomNumber =
                room?.RoomNumber ?? "Unknown",
            RoomTypeName = roomTypeName,
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            NumberOfNights = detail.NumberOfNights,
            PricePerNight = detail.PricePerNight,
            Amount = detail.Amount,
            Status = booking.Status,
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
        bool result =
            await _bookingService.ConfirmBookingAsync(id);

        if (result)
            TempData["Success"] = "Booking confirmed successfully.";
        else
            TempData["Error"] = "Unable to confirm booking.";

        return RedirectToAction(nameof(Index));
    }

    // =========================
    // CANCEL
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        bool result =
            await _bookingService.CancelBookingAsync(id);

        if (result)
            TempData["Success"] = "Booking cancelled successfully.";
        else
            TempData["Error"] = "Unable to cancel booking.";

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

        var rooms =
            await _bookingService.GetAvailableRoomsAsync(
                inDate,
                outDate);

        var result = rooms.Select(x => new
        {
            id = x.RoomId,
            roomNumber = x.RoomNumber,
            floor = x.Floor
        });

        return Json(result);
    }

    // =========================
    // AJAX: ROOM PRICE
    // =========================
    [HttpGet]
    public async Task<IActionResult> GetRoomPrice(int roomId)
    {
        var price =
            await _bookingService.GetRoomPriceAsync(roomId);

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
    private async Task LoadCustomers(
        BookingCreateViewModel model)
    {
        var customers =
            await _bookingService.GetCustomersAsync();

        model.Customers = customers
            .Select(x => new SelectListItem
            {
                Value = x.CustomerId.ToString(),
                Text = $"{x.CustomerName} - {x.Phone}"
            })
            .ToList();
    }

    // =========================
    // LOAD ROOMS
    // =========================
    private async Task LoadRooms(
        BookingCreateViewModel model)
    {
        var rooms =
            await _bookingService.GetAvailableRoomsAsync(
                model.CheckInDate,
                model.CheckOutDate);

        model.Rooms = rooms
            .Select(x => new SelectListItem
            {
                Value = x.RoomId.ToString(),
                Text = $"Room {x.RoomNumber} - Floor {x.Floor}"
            })
            .ToList();
    }
}