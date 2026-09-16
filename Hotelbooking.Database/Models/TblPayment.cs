using System;
using System.Collections.Generic;

namespace Hotelbooking.Database.Models;

public partial class TblPayment
{
    public int PaymentId { get; set; }

    public int BookingId { get; set; }

    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;
}
