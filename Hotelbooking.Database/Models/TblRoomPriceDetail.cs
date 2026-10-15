using System;
using System.Collections.Generic;

namespace Hotelbooking.Database.Models;

public partial class TblRoomPriceDetail
{
    public int RoomPriceDetailId { get; set; }

    public int RoomPriceId { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public decimal Price { get; set; }

    public bool Status { get; set; }

    public virtual TblRoomPrice RoomPrice { get; set; } = null!;
}
