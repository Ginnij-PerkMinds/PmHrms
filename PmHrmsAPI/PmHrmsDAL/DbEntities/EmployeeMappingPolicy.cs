//using System.ComponentModel.DataAnnotations.Schema;
//namespace PmHrmsAPI.PmHrmsDAL.DbEntities
//{
    

//    [Table("employee_policy_mapping")]
//    public class EmployeePolicyMapping
//    {
//        [Column("mapping_id")]
//        public int MappingId { get; set; }

//        [Column("employee_id")]
//        public int EmployeeId { get; set; }

//        [Column("policy_id")]
//        public int PolicyId { get; set; }

//        [Column("organization_id")]
//        public int OrganizationId { get; set; }

//        [Column("effective_from")]
//        public DateTime EffectiveFrom { get; set; }

//        [Column("effective_to")]
//        public DateTime? EffectiveTo { get; set; }

//        [Column("is_active")]
//        public bool IsActive { get; set; }

//        [Column("created_at")]
//        public DateTime CreatedAt { get; set; }

//        // Navigation Properties (Optional)
//        public virtual WorkPolicy Policy { get; set; } = null!;
//    }
//}
