namespace PmHrmsAPI.PmHrmsDAL.DbEntities
{
    public class DesignationWorkPolicyMapping
    {
        public int Id { get; set; }

        public int DesignationId { get; set; }
        public int WorkPolicyId { get; set; }

        public virtual Designation Designation { get; set; }
        public virtual WorkPolicy WorkPolicy { get; set; }
    }
}
