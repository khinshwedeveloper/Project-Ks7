using HotelBookingMvc.Web.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace HotelBookingMvc.Web.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AppDbContext _db;


        private readonly IHubContext<HotelHub> _hub;
        public CustomerController(
    AppDbContext db,
    IHubContext<HotelHub> hub)
        {
            _db = db;
            _hub = hub;
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TblCustomer customer)
        {
            if (!ModelState.IsValid)
            {
                return View(customer);
            }
            customer .CreateDatetime = DateTime.Now;
            _db.TblCustomers.Add(customer);
            await _db.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("Customer created Successfully");
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Index()
        {
            return View();
        }
    }
}
