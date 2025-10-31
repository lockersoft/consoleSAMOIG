using System;
using System.Collections.Generic;

namespace consoleSAMOIG.Models;

public partial class ApiKey
{
    public int ApiKeyIdpk { get; set; }

    public string Api { get; set; } = null!;

    public string ApiKey1 { get; set; } = null!;

    public DateTime? DateUpdated { get; set; }
}
