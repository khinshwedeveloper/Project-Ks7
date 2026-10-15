using System;
using System.Collections.Generic;

namespace Hotelbooking.Database.Models;

public partial class TblBookingDetail
{
    public int BookingDetailId { get; set; }

    public int BookingId { get; set; }

    public int RoomId { get; set; }

    public DateTime CheckInDateTime { get; set; }

    public DateTime CheckOutDateTime { get; set; }

    public decimal RoomPrice { get; set; }

    public int ExtraBedQty { get; set; }

    public decimal ExtraBedPrice { get; set; }

    public decimal TotalRoomPrice { get; set; }

    public string? Remarks { get; set; }

    public virtual TblBooking? Booking { get; set; }

    public virtual TblRoom? Room { get; set; }

    public virtual ICollection<TblBookingService> TblBookingServices { get; set; } = new List<TblBookingService>();

    public virtual ICollection<TblRoomTransfer> TblRoomTransfers { get; set; } = new List<TblRoomTransfer>();
}