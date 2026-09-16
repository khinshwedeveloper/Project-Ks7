using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotelbooking.Database.Models;

public partial class TblBooking
{
    public int BookingId { get; set; }

    public string BookingNo { get; set; } = null!;

    public int CustomerId { get; set; }

    public DateTime BookingDate { get; set; }

    public DateOnly CheckInDate { get; set; }

    public DateOnly CheckOutDate { get; set; }

    public string Status { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public DateTime CreateDateTime { get; set; }

    [ForeignKey("CustomerId")]
    public virtual TblCustomer? Customer { get; set; }

    public virtual ICollection<TblBookingDetail> TblBookingDetails { get; set; } = new List<TblBookingDetail>();
}