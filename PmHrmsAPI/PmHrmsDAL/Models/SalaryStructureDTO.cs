
public class SalaryComponentDTO
{
    public int SalaryComponentId { get; set; }
    public int SalaryStructureId { get; set; }
    public int ComponentMasterId { get; set; }
    public string ComponentName { get; set; } = null!;
    public decimal Amount { get; set; }
    public bool IsEarning { get; set; }
}

/// <summary>
/// Response DTO for Salary Structure - used to prevent circular references
/// Does NOT include back-reference to SalaryStructure from Components
/// </summary>
public class SalaryStructureDTO
{
    public int SalaryStructureId { get; set; }
    public int OrganizationId { get; set; }
    public string StructureName { get; set; } = null!;
    public string PayType { get; set; } = "Monthly";
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Components for this structure - uses DTO to avoid circular reference
    /// </summary>
    public List<SalaryComponentDTO> Components { get; set; } = new();
}

/// <summary>
/// DTO for Designation Salary Mapping
/// </summary>
public class DesignationSalaryMappingDTO
{
    public int DesignationSalaryMappingId { get; set; }
    public int DesignationId { get; set; }
    public string DesignationName { get; set; } = null!;
    public int SalaryStructureId { get; set; }
    public string SalaryStructureName { get; set; } = null!;
}

/// <summary>
/// DTO for Employee Salary Mapping
/// </summary>
public class EmployeeSalaryMappingDTO
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = null!;
    public int SalaryStructureId { get; set; }
    public string SalaryStructureName { get; set; } = null!;
}
