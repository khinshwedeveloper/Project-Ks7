using System;
using System.Collections.Generic;

namespace Hotelbooking.Database.Models;

public partial class TblCustomer
{
    public int CustomerId { get; set; }

    public string? CustomerCode { get; set; }

    public string Name { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Nrc { get; set; }

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<TblBooking> TblBookings { get; set; } = new List<TblBooking>();
}
