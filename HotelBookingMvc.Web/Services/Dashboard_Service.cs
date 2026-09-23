using Hotelbooking.Database.Context;
using HotelBookingMvc.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingMvc.Web.Services;

public class Dashboard_Service
{
    private readonly AppDbContext _context;

    public Dashboard_Service(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        var totalRooms =
            await _context.TblRooms
                .Where(x => !x.IsDelete)
                .CountAsync();

        var available =
            await _context.TblRooms
                .CountAsync(x => x.Status == "Available");

        var reserved =
            await _context.TblRooms
                .CountAsync(x => x.Status == "Reserved");

        var occupied =
            await _context.TblRooms
                .CountAsync(x => x.Status == "Occupied");

        var maintenance =
            await _context.TblRooms
                .CountAsync(x => x.Status == "Maintenance");

        decimal occupancy = 0;

        if (totalRooms > 0)
        {
            occupancy =
                (decimal)occupied / totalRooms * 100;
        }

        var revenue =
            await _context.TblPayments
                .Where(x => x.PaymentStatus == "Paid")
                .SumAsync(x => (decimal?)x.Amount) ?? 0;

        return new DashboardViewModel
        {
            TotalRooms = totalRooms,
            AvailableRooms = available,
            ReservedRooms = reserved,
            OccupiedRooms = occupied,
            MaintenanceRooms = maintenance,

            ActiveGuests = occupied,

            TodayCheckIns =
                await _context.TblBookings
                    .CountAsync(x =>
                        x.CheckInDate ==
                        DateOnly.FromDateTime(DateTime.Today)),

            TodayCheckOuts =
                await _context.TblBookings
                    .CountAsync(x =>
                        x.CheckOutDate ==
                        DateOnly.FromDateTime(DateTime.Today)),

            TotalRevenue = revenue,

            OccupancyRate = occupancy
        };
    }
}