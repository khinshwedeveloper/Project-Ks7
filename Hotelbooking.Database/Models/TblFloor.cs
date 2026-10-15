using System;
using System.Collections.Generic;

namespace Hotelbooking.Database.Models;

public partial class TblFloor
{
    public int FloorId { get; set; }

    public string FloorName { get; set; } = null!;

    public string? Description { get; set; }

    public bool Status { get; set; }

    public virtual ICollection<TblRoom> TblRooms { get; set; } = new List<TblRoom>();
}
