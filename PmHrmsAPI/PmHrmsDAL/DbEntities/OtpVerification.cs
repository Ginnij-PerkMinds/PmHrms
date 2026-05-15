using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class OtpVerification
{
    public string Target { get; set; } = null!;

    public string OtpCode { get; set; } = null!;

    public DateTime OtpExpiry { get; set; }

    public bool? IsVerified { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? VerificationType { get; set; }
}
