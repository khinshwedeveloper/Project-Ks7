using System;
using System.Collections.Generic;

namespace Hotelbooking.Database.Models;

public partial class TblRoom
{
    public int RoomId { get; set; }

    public string RoomNumber { get; set; } = null!;

    public int RoomTypeId { get; set; }

    public int Floor { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedDateTime { get; set; }

    public bool IsDelete { get; set; }
}
