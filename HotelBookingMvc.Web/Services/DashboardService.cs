namespace HotelBookingMvc.Web.Services;

public class DashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<object> GetDashboardAsync()
    {
        var today = DateTime.Today;

        var totalRooms = await _db.TblRooms
            .CountAsync(x => !x.IsDelete);

        var availableRooms = await _db.TblRooms
            .CountAsync(x =>
                !x.IsDelete &&
                x.Status == "Available");

        var occupiedRooms = await _db.TblRooms
            .CountAsync(x =>
                !x.IsDelete &&
                x.Status == "Occupied");

        var reservedRooms = await _db.TblRooms
            .CountAsync(x =>
                !x.IsDelete &&
                x.Status == "Reserved");

        var maintenanceRooms = await _db.TblRooms
            .CountAsync(x =>
                !x.IsDelete &&
                x.Status == "Maintenance");

        var activeGuests = await _db.TblBookings
            .CountAsync(x => x.Status == "CheckedIn");

        var revenue = await _db.TblPayments
            .Where(x => x.PaymentStatus == "Paid")
            .SumAsync(x => (decimal?)x.Amount) ?? 0;

        var todayCheckIns = await _db.TblBookings
            .CountAsync(x =>
                x.CheckInDate ==
                DateOnly.FromDateTime(today) &&
                x.Status == "Confirmed");

        var todayCheckOuts = await _db.TblBookings
            .CountAsync(x =>
                x.CheckOutDate ==
                DateOnly.FromDateTime(today) &&
                x.Status == "CheckedIn");

        var occupancy = totalRooms == 0
            ? 0
            : Math.Round(
                (decimal)occupiedRooms /
                totalRooms * 100, 2);

        return new
        {
            totalRooms,
            availableRooms,
            occupiedRooms,
            reservedRooms,
            maintenanceRooms,
            activeGuests,
            revenue,
            todayCheckIns,
            todayCheckOuts,
            occupancy
        };
    }
}