using System;
using System.Collections.Generic;

namespace Hotelbooking.Database.Models;

public partial class TblRoomPrice
{
    public int RoomPriceId { get; set; }

    public int RoomTypeId { get; set; }

    public string PriceName { get; set; } = null!;

    public DateOnly EffectiveDate { get; set; }

    public bool Status { get; set; }

    public virtual TblRoomType RoomType { get; set; } = null!;

    public virtual ICollection<TblRoomPriceDetail> TblRoomPriceDetails { get; set; } = new List<TblRoomPriceDetail>();
}
