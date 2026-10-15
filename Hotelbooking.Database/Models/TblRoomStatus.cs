using System;
using System.Collections.Generic;

namespace Hotelbooking.Database.Models;

public partial class TblRoomStatus
{
    public int StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsAvailable { get; set; }

    public virtual ICollection<TblRoom> TblRooms { get; set; } = new List<TblRoom>();
}
