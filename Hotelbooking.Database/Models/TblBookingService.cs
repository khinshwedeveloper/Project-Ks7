using System;
using System.Collections.Generic;

namespace Hotelbooking.Database.Models;

public partial class TblBookingService
{
    public int BookingServiceId { get; set; }

    public int BookingId { get; set; }

    public int? BookingDetailId { get; set; }

    public int ServiceId { get; set; }

    public decimal Qty { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime ServiceDate { get; set; }

    public virtual TblBooking Booking { get; set; } = null!;

    public virtual TblBookingDetail? BookingDetail { get; set; }

    public virtual TblService Service { get; set; } = null!;
}
