using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotelbooking.Database.Models;

public partial class TblBookingDetail
{
    public int BookingDetailId { get; set; }

    public int Bookingid { get; set; }

    public int Roomid { get; set; }

    public decimal PricePerNight { get; set; }

    public int NumberOfNights { get; set; }

    public decimal Amount { get; set; }

    [ForeignKey("Bookingid")]
    public virtual TblBooking? Booking { get; set; }

    [ForeignKey("Roomid")]
    public virtual TblRoom? Room { get; set; }
}