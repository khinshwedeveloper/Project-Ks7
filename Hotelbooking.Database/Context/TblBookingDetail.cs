using System;
using System.Collections.Generic;

namespace Hotelbooking.Database.Context;

public partial class TblBookingDetail
{
    public int BookingDetailId { get; set; }

    public int Bookingid { get; set; }

    public int Roomid { get; set; }

    public decimal PricePerNight { get; set; }

    public int NumberOfNights { get; set; }

    public decimal Amount { get; set; }
}
