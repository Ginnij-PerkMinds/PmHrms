namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class WorkPolicyWeekOff
{
    public int Id { get; set; }

    public int PolicyId { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public bool IsHalfDay { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual WorkPolicy WorkPolicy { get; set; } = null!;
}