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
        var payments =
            await (
                from payment in _db.TblPayments
                join booking in _db.TblBookings
                    on payment.BookingId equals booking.BookingId
                join customer in _db.TblCustomers
                    on booking.CustomerId equals customer.CustomerId
                orderby payment.PaymentDate descending
                select new PaymentViewModel
                {
                    PaymentId = payment.PaymentId,
                    BookingId = payment.BookingId,
                    BookingNo = booking.BookingNo,
                    CustomerName = customer.CustomerName,
                    TotalAmount = booking.TotalAmount,
                    Amount = payment.Amount,
                    PaymentMethod = payment.PaymentMethod,
                    PaymentStatus = payment.PaymentStatus,
                    PaymentDate = payment.PaymentDate
                }
            ).ToListAsync();

        foreach (var payment in payments)
        {
            payment.PaidAmount =
                await GetPaidAmountAsync(payment.BookingId);

            payment.RemainingAmount =
                payment.TotalAmount - payment.PaidAmount;
        }

        return payments;
    }


    // Get payment by ID
    public async Task<TblPayment?> GetPaymentByIdAsync(int id)
    {
        return await _db.TblPayments
            .FirstOrDefaultAsync(x => x.PaymentId == id);
    }


    // Get bookings available for payment
    public async Task<List<TblBooking>> GetPaymentBookingsAsync()
    {
        return await _db.TblBookings
            .Where(x =>
                x.Status != "Cancelled" &&
                x.TotalAmount > 0)
            .OrderByDescending(x => x.BookingDate)
            .ToListAsync();
    }


    // Get booking information
    public async Task<TblBooking?> GetBookingAsync(int bookingId)
    {
        return await _db.TblBookings
            .FirstOrDefaultAsync(x =>
                x.BookingId == bookingId);
    }


    // Get customer
    public async Task<TblCustomer?> GetCustomerAsync(int customerId)
    {
        return await _db.TblCustomers
            .FirstOrDefaultAsync(x =>
                x.CustomerId == customerId);
    }


    // Get total paid amount for booking
    public async Task<decimal> GetPaidAmountAsync(int bookingId)
    {
        return await _db.TblPayments
            .Where(x =>
                x.BookingId == bookingId &&
                x.PaymentStatus != "Refunded")
            .SumAsync(x => (decimal?)x.Amount) ?? 0;
    }


    // Get remaining balance
    public async Task<decimal> GetRemainingAmountAsync(int bookingId)
    {
        var booking =
            await GetBookingAsync(bookingId);

        if (booking == null)
            return 0;

        var paid =
            await GetPaidAmountAsync(bookingId);

        var remaining =
            booking.TotalAmount - paid;

        return remaining < 0 ? 0 : remaining;
    }


    // Create payment
    public async Task<(bool Success, string Message)> CreatePaymentAsync(
        PaymentViewModel model)
    {
        try
        {
            var booking =
                await GetBookingAsync(model.BookingId);

            if (booking == null)
            {
                return (false, "Booking not found.");
            }

            if (booking.Status == "Cancelled")
            {
                return (false,
                    "Payment cannot be added to a cancelled booking.");
            }

            if (model.Amount <= 0)
            {
                return (false,
                    "Payment amount must be greater than 0.");
            }

            var paidAmount =
                await GetPaidAmountAsync(model.BookingId);

            var remaining =
                booking.TotalAmount - paidAmount;

            if (remaining <= 0)
            {
                return (false,
                    "This booking has already been fully paid.");
            }

            if (model.Amount > remaining)
            {
                return (false,
                    $"Payment amount cannot exceed the remaining balance of {remaining:N2}.");
            }


            // Generate Payment ID
            int maxId =
                await _db.TblPayments
                    .Select(x => (int?)x.PaymentId)
                    .MaxAsync() ?? 0;


            var payment = new TblPayment
            {
                PaymentId = maxId + 1,

                BookingId = model.BookingId,

                PaymentDate = DateTime.Now,

                Amount = model.Amount,

                PaymentMethod = model.PaymentMethod,

                PaymentStatus =
                    model.Amount == remaining
                        ? "Paid"
                        : "Partial"
            };


            _db.TblPayments.Add(payment);

            await _db.SaveChangesAsync();


            return (true,
                payment.PaymentStatus == "Paid"
                    ? "Payment completed successfully."
                    : "Partial payment recorded successfully.");
        }
        catch
        {
            return (false,
                "Unable to save payment.");
        }
    }


    // Get booking payment summary
    public async Task<PaymentViewModel?> GetBookingPaymentSummaryAsync(
        int bookingId)
    {
        var booking =
            await GetBookingAsync(bookingId);

        if (booking == null)
            return null;

        var customer =
            await GetCustomerAsync(booking.CustomerId);

        var paid =
            await GetPaidAmountAsync(bookingId);

        var remaining =
            booking.TotalAmount - paid;

        if (remaining < 0)
            remaining = 0;

        return new PaymentViewModel
        {
            BookingId = booking.BookingId,

            BookingNo = booking.BookingNo,

            CustomerName =
                customer?.CustomerName ?? "Unknown",

            TotalAmount =
                booking.TotalAmount,

            PaidAmount =
                paid,

            RemainingAmount =
                remaining
        };
    }
}