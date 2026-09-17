using HotelBookingMvc.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingMvc.Web.Controllers
{
    public class RoomController : Controller
    {
        private readonly Room_Service _roomService;
        private readonly AppDbContext _db;

        public RoomController(Room_Service roomService)
        {
            _roomService = roomService;
        }
    }
    public async Task<IActionResult> Index()
        {
            var roooms = await _roomService.GetRoomsAsync();
            ViewBag.RoomTypes = await _db.TblRoomTypes
                .OrderBy(x => x.RoomTypeName)
                .ToListAsync();
            return View(rooms);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.RoomTypes = await _db.TblRoomTypes
                .(x=> x.RoomTypeName)
                .ToListAsync()



                return View();
        }


}
