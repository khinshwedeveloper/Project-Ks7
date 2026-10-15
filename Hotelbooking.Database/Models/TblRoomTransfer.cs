using System;
using System.Collections.Generic;

namespace Hotelbooking.Database.Models;

public partial class TblRoomTransfer
{
    public int RoomTransferId { get; set; }

    public int BookingDetailId { get; set; }

    public int FromRoomId { get; set; }

    public int ToRoomId { get; set; }

    public DateTime TransferDate { get; set; }

    public string? Reason { get; set; }

    public string? Remarks { get; set; }

    public virtual TblBookingDetail BookingDetail { get; set; } = null!;

    public virtual TblRoom FromRoom { get; set; } = null!;

    public virtual TblRoom ToRoom { get; set; } = null!;
}
