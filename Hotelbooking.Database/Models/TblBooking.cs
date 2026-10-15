using System;
using System.Collections.Generic;

namespace Hotelbooking.Database.Models;

public partial class TblBooking
{
    public int BookingId { get; set; }

    public string? BookingNo { get; set; }
 
    public int CustomerId { get; set; }

    public DateTime BookingDate { get; set; }

    public string? Status { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual TblCustomer? Customer { get; set; }

    public virtual ICollection<TblBookingDetail> TblBookingDetails { get; set; } = new List<TblBookingDetail>();

    public virtual ICollection<TblBookingService> TblBookingServices { get; set; } = new List<TblBookingService>();

    public virtual ICollection<TblPayment> TblPayments { get; set; } = new List<TblPayment>();
}