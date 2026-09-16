using System;
using System.Collections.Generic;

namespace Hotelbooking.Database.Models;

public partial class TblRoomType
{
    public int RoomTypeId { get; set; }

    public string RoomTypeName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal PricePerNight { get; set; }

    public DateTime CreatedDateTime { get; set; }

    public bool IsDelete { get; set; }
}
