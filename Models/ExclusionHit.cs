using System;
using System.Collections.Generic;

namespace consoleSAMOIG.Models;

public partial class ExclusionHit
{
    public int ExclusionHitIdpk { get; set; }

    public DateOnly? DateRun { get; set; }

    public int ContactIdfk { get; set; }

    public string NameInput { get; set; } = null!;

    public string NameHit { get; set; } = null!;

    public string TableHit { get; set; } = null!;

    public string? Address { get; set; }

    public string City { get; set; } = null!;

    public string State { get; set; } = null!;

    public string? Zip { get; set; }

    public DateOnly? Dob { get; set; }

    public string Message { get; set; } = null!;

    public string LikelyMatch { get; set; } = null!;
}
