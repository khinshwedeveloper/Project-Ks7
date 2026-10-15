using System;
using System.Collections.Generic;

namespace Hotelbooking.Database.Models;

public partial class TblServiceCategory
{
    public int ServiceCategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public bool Status { get; set; }

    public virtual ICollection<TblService> TblServices { get; set; } = new List<TblService>();
}
