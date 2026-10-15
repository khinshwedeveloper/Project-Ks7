using System;
using System.Collections.Generic;

namespace Hotelbooking.Database.Models;

public partial class TblExtraBedPrice
{
    public int ExtraBedPriceId { get; set; }

    public int RoomTypeId { get; set; }

    public decimal Price { get; set; }

    public DateOnly EffectiveDate { get; set; }

    public bool Status { get; set; }

    public virtual TblRoomType RoomType { get; set; } = null!;
}
