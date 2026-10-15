using System;
using System.Collections.Generic;

namespace Hotelbooking.Database.Models;

public partial class TblRoom
{
    public int RoomId { get; set; }

    public string RoomNo { get; set; } = null!;

    public int FloorId { get; set; }

    public int RoomTypeId { get; set; }

    public int StatusId { get; set; }

    public string? Description { get; set; }

    public virtual TblFloor? Floor { get; set; }

    public virtual TblRoomType? RoomType { get; set; }

    public virtual TblRoomStatus? Status { get; set; }

    public virtual ICollection<TblBookingDetail> TblBookingDetails { get; set; } = new List<TblBookingDetail>();

    public virtual ICollection<TblRoomTransfer> TblRoomTransferFromRooms { get; set; } = new List<TblRoomTransfer>();

    public virtual ICollection<TblRoomTransfer> TblRoomTransferToRooms { get; set; } = new List<TblRoomTransfer>();
}
