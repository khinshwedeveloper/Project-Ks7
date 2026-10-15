using Hotelbooking.Database.Context;
using Hotelbooking.Database.Models;
using HotelBookingMvc.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingMvc.Web.Services;

public class Payment_Service
{
    private readonly AppDbContext _db;

    public Payment_Service(AppDbContext db)
    {
        _db = db;
    }

    // Get all payments
    public async Task<List<PaymentViewModel>> GetPaymentsAsync()
    {
        var rawPayments = await _db.TblPayments
            .Include(p => p.Booking)
                .ThenInclude(b => b!.Customer)
            .Include(p => p.Booking)
                .ThenInclude(b => b!.TblBookingDetails)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        var payments = new List<PaymentViewModel>();

        foreach (var payment in rawPayments)
        {
            decimal totalAmount = payment.Booking?.TblBookingDetails.Sum(d => d.TotalRoomPrice) ?? 0m;
            decimal paidAmount = await GetPaidAmountAsync(payment.BookingId);

            payments.Add(new PaymentViewModel
            {
                PaymentId = payment.PaymentId,
                BookingId = payment.BookingId,
                BookingNo = payment.Booking?.BookingNo ?? "-",
                CustomerName = payment.Booking?.Customer?.Name ?? "Unknown",
                TotalAmount = totalAmount,
                PaidAmount = paidAmount,
                RemainingAmount = totalAmount - paidAmount < 0 ? 0 : totalAmount - paidAmount,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                PaymentStatus = payment.PaymentStatus,
                PaymentDate = payment.PaymentDate
            });
        }

        return payments;
    }

    // Get payment by ID
    public async Task<TblPayment?> GetPaymentByIdAsync(int id)
    {
        return await _db.TblPayments
            .Include(p => p.Booking)
                .ThenInclude(b => b!.Customer)
            .FirstOrDefaultAsync(x => x.PaymentId == id);
    }

    // Get bookings available for payment
    public async Task<List<TblBooking>> GetPaymentBookingsAsync()
    {
        return await _db.TblBookings
            .Include(x => x.Customer)
            .Include(x => x.TblBookingDetails)
            .Where(x => x.Status != "Cancelled")
            .OrderByDescending(x => x.BookingDate)
            .ToListAsync();
    }

    // Get booking information
    public async Task<TblBooking?> GetBookingAsync(int bookingId)
    {
        return await _db.TblBookings
            .Include(x => x.Customer)
            .Include(x => x.TblBookingDetails)
            .FirstOrDefaultAsync(x => x.BookingId == bookingId);
    }

    // Get total booking amount
    public async Task<decimal> GetBookingTotalAmountAsync(int bookingId)
    {
        var booking = await GetBookingAsync(bookingId);
        if (booking == null) return 0m;
        return booking.TblBookingDetails.Sum(d => d.TotalRoomPrice);
    }

    // Get customer
    public async Task<TblCustomer?> GetCustomerAsync(int customerId)
    {
        return await _db.TblCustomers
            .FirstOrDefaultAsync(x => x.CustomerId == customerId);
    }

    // Get total paid amount for booking
    public async Task<decimal> GetPaidAmountAsync(int bookingId)
    {
        return await _db.TblPayments
            .Where(x => x.BookingId == bookingId && x.PaymentStatus != "Refunded")
            .SumAsync(x => (decimal?)x.Amount) ?? 0m;
    }

    // Get remaining balance
    public async Task<decimal> GetRemainingAmountAsync(int bookingId)
    {
        var booking = await GetBookingAsync(bookingId);
        if (booking == null)
            return 0;

        decimal total = booking.TblBookingDetails.Sum(d => d.TotalRoomPrice);
        decimal paid = await GetPaidAmountAsync(bookingId);
        decimal remaining = total - paid;

        return remaining < 0 ? 0 : remaining;
    }

    // Create payment
    public async Task<(bool Success, string Message)> CreatePaymentAsync(PaymentViewModel model)
    {
        try
        {
            var booking = await GetBookingAsync(model.BookingId);
            if (booking == null)
            {
                return (false, "Booking not found.");
            }

            if (booking.Status == "Cancelled")
            {
                return (false, "Payment cannot be added to a cancelled booking.");
            }

            if (model.Amount <= 0)
            {
                return (false, "Payment amount must be greater than 0.");
            }

            decimal total = booking.TblBookingDetails.Sum(d => d.TotalRoomPrice);
            decimal paidAmount = await GetPaidAmountAsync(model.BookingId);
            decimal remaining = total - paidAmount;

            if (remaining <= 0)
            {
                return (false, "This booking has already been fully paid.");
            }

            if (model.Amount > remaining)
            {
                return (false, $"Payment amount cannot exceed the remaining balance of {remaining:N2}.");
            }

            var payment = new TblPayment
            {
                BookingId = model.BookingId,
                PaymentDate = DateTime.Now,
                Amount = model.Amount,
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = model.Amount >= remaining ? "Paid" : "Partial"
            };

            _db.TblPayments.Add(payment);
            await _db.SaveChangesAsync();

            return (true, payment.PaymentStatus == "Paid"
                ? "Payment completed successfully."
                : "Partial payment recorded successfully.");
        }
        catch
        {
            return (false, "Unable to save payment.");
        }
    }

    // Get booking payment summary
    public async Task<PaymentViewModel?> GetBookingPaymentSummaryAsync(int bookingId)
    {
        var booking = await GetBookingAsync(bookingId);
        if (booking == null)
            return null;

        var customer = await GetCustomerAsync(booking.CustomerId);
        decimal total = booking.TblBookingDetails.Sum(d => d.TotalRoomPrice);
        decimal paid = await GetPaidAmountAsync(bookingId);
        decimal remaining = total - paid;
        if (remaining < 0) remaining = 0;

        return new PaymentViewModel
        {
            BookingId = booking.BookingId,
            BookingNo = booking.BookingNo ?? "-",
            CustomerName = customer?.Name ?? "Unknown",
            TotalAmount = total,
            PaidAmount = paid,
            RemainingAmount = remaining
        };
    }
}