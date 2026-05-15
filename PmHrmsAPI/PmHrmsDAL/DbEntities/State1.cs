using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class State1
{
    public long Id { get; set; }

    public long JobId { get; set; }

    public string Name { get; set; } = null!;

    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Data { get; set; }

    public virtual Job Job { get; set; } = null!;
}
