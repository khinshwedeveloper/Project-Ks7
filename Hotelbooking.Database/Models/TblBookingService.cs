using System;
using System.Collections.Generic;

namespace Hotelbooking.Database.Models;

public partial class TblBookingService
{
    public int BookingserviceId { get; set; }

    public int BookingId { get; set; }

    public int ServiceId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Amount { get; set; }

    public DateTime ServiceDate { get; set; }

    public DateTime CreatedDateTime { get; set; }
}
