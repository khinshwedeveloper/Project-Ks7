//using HotelBookingMvc.Web.ViewModels;

//namespace HotelBookingMvc.Web.Services
//{
//    public class CheckOut_Service
//    {
//        private readonly AppDbContext _db;

//        public CheckOut_Service(AppDbContext db)
//        {
//            _db = db;
//        }

//        public async Task<List<CheckOutViewModel>> GetCheckBookingsAsync()
//        {
//            var bookings = await _db.TblBookings
//                .Include(x => x.Customer)
//                .Include(x => x.TblBookingDetails)
//                .Where(x => x.Status == "CheckOutDate")
//                .ToListAsync();

//            var result = new List<CheckOutViewModel>();

//            foreach (var booking in bookings)
//            {
//                var detail = booking.TblBookingDetails.FirstOrDefault();
//                if (detail == null)
//                    continue;
//                var room = rooms.FirstOrDefault(
//               x => x.RoomId == detail.Roomid);

//                if (room == null)
//                    continue;

//                decimal paidAmount =
//                    await _db.TblPayments.Where(x => x.BookingId == booking.BookingId)
//                    .SumAsync(x => (decimal?)x.Amount) ?? 0m;

//                decimal balance =
//                    booking.TotalAmount - paidAmount;
//                if (balance)
//            }
//        }
//    }
//}
