using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class EmployeeDocument
{
    public int DocumentId { get; set; }

    public int EmployeeId { get; set; }

    public string? DocumentType { get; set; }

    public string? DocumentPath { get; set; }

    public DateTime? UploadDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public string? VerificationStatus { get; set; }

    public int? VerifiedById { get; set; }

    public DateTime? VerifiedDate { get; set; }

    public string? HrRemarks { get; set; }

    public int DocumentMasterId { get; set; }

    public virtual DocumentMaster DocumentMaster { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;

    public virtual Employee? VerifiedBy { get; set; }
}
