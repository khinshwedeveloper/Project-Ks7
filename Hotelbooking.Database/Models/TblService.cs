using System;
using System.Collections.Generic;

namespace Hotelbooking.Database.Models;

public partial class TblService
{
    public int ServiceId { get; set; }

    public int ServiceCategoryId { get; set; }

    public string ServiceName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public bool Status { get; set; }

    public virtual TblServiceCategory? ServiceCategory { get; set; }

    public virtual ICollection<TblBookingService> TblBookingServices { get; set; } = new List<TblBookingService>();
}
