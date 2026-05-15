public class AttendanceRequestResponse
{
    public Guid Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; }

    public int AttendanceId { get; set; }
    public string Reason { get; set; }
    public string Status { get; set; }

    public string? AdminRemarks { get; set; }
    public DateTime? ApprovedCheckoutTime { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public DateOnly AttendanceDate { get; set; }
    public DateTime? CheckInTime { get; set; }
}