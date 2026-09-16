using Hotelbooking.Database.Context;
using HotelBookingMvc.Web.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingMvc.Web.Controllers
{
    public class CheckInController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<HotelHub> _hub;

        public CheckInController(
            AppDbContext db,
            IHubContext<HotelHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        public async Task<IActionResult> Index()
        {
            var bookings = await _db.TblBookings
                .Where(x => x.Status == "Confirmed")
                .OrderBy(x => x.CheckInDate)
                .ToListAsync();

            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int bookingId)
        {
            var booking = await _db.TblBookings
                .FirstOrDefaultAsync(x => x.BookingId == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = "CheckedIn";

            var bookingDetails = await _db.TblBookingDetails
                .Where(x => x.Bookingid == booking.BookingId)
                .ToListAsync();

            foreach (var detail in bookingDetails)
            {
                var room = await _db.TblRooms
                    .FirstOrDefaultAsync(x => x.RoomId == detail.Roomid);

                if (room != null)
                {
                    room.Status = "Occupied";
                }
            }

            await _db.SaveChangesAsync();

            await _hub.Clients.All
                .SendAsync("HotelUpdated");

            return RedirectToAction(nameof(Index));
        }
    }
}