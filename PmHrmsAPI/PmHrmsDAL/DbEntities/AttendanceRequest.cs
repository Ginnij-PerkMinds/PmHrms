using System.ComponentModel.DataAnnotations.Schema;

[Table("attendance_request")]
public class AttendanceRequest
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("employee_id")]
    public int EmployeeId { get; set; }

    [Column("attendance_id")]
    public int AttendanceId { get; set; }

    [Column("reason")]
    public string Reason { get; set; }

    [Column("status")]
    public string Status { get; set; }

    [Column("admin_remarks")]
    public string? AdminRemarks { get; set; }

    [Column("approved_checkout_time")]
    public DateTime? ApprovedCheckoutTime { get; set; } 

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}