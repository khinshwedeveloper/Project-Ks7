using System;
using System.Collections.Generic;

namespace Hotelbooking.Database.Models;

public partial class TblBanking
{
    public int BankingId { get; set; }

    public string BankName { get; set; } = null!;

    public string AccountName { get; set; } = null!;

    public string AccountNumber { get; set; } = null!;

    public string? BranchName { get; set; }

    public string? AccountType { get; set; }

    public string? Description { get; set; }

    public bool Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<TblPayment> TblPayments { get; set; } = new List<TblPayment>();
}
