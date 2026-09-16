using Hotelbooking.Database.Context;
using Microsoft.EntityFrameworkCore;
namespace HotelBookingMvc.Web.Services
{
    public class NotificationService
    {
        private readonly AppDbContext _db;

        public NotificationService(AppDbContext db)
        {
            _db = db;
        }
        public async Task<object> GetNotificationAsync()
        {
            var today = DateTime.Today;

            var peendingPayments=await _db.TblPayments.Where(x=>x.PaymentStatus=="Partial").CountAsync();

            var todayCheckouts=await _db.TblBookings.Where(x=>x.CheckOutDate==DateOnly.FromDateTime(today)&&x.Status== "checkedin").CountAsync();
            
            var todayCheckins=await _db.TblBookings.Where(x => x.CheckInDate == DateOnly.FromDateTime(today) && x.Status == "booked").CountAsync();

            return new
            {
                PendingPayments = peendingPayments,
                TodayCheckouts = todayCheckouts,
                TodayCheckins = todayCheckins
            };
        }
    }
}

