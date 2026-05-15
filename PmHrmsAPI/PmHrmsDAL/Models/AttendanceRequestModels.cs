public class SubmitAttendanceRequestModel
{
    public int EmployeeId { get; set; }
    public int AttendanceId { get; set; }
    public string Reason { get; set; }
}

public class ApproveAttendanceRequestModel
{
    public Guid RequestId { get; set; }
    public DateTime CheckoutTime { get; set; }
}

public class RejectAttendanceRequestModel
{
    public Guid RequestId { get; set; }
    public string Remarks { get; set; }
}