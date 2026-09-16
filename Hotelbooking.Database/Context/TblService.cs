using System;
using System.Collections.Generic;

namespace Hotelbooking.Database.Context;

public partial class TblService
{
    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = null!;

    public decimal Price { get; set; }

    public string Description { get; set; } = null!;

    public bool IsDelete { get; set; }
}
