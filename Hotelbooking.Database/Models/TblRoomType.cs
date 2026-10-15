using System;
using System.Collections.Generic;

namespace Hotelbooking.Database.Models;

public partial class TblRoomType
{
    public int RoomTypeId { get; set; }

    public string RoomTypeName { get; set; } = null!;

    public string? Description { get; set; }

    public int MaxGuest { get; set; }

    public bool Status { get; set; }

    public virtual ICollection<TblRoom> TblRooms { get; set; } = new List<TblRoom>();

    public virtual ICollection<TblRoomPrice> TblRoomPrices { get; set; } = new List<TblRoomPrice>();

    public virtual ICollection<TblExtraBedPrice> TblExtraBedPrices { get; set; } = new List<TblExtraBedPrice>();
}
