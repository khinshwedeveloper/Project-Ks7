using System;
using System.Collections.Generic;

namespace Hotelbooking.Database.Context;

public partial class TblCustomer
{
    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Nrc { get; set; } = null!;

    public DateTime CreateDatetime { get; set; }
}
