using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class Designation
{
    public int DesignationId { get; set; }
     public string DesignationName { get; set; } = null!;
               
     public int? HierarchyLevel { get; set; }                                                                   
                           
    public int? DepartmentId { get; set; }                       
     public bool IsSystemDefault { get; set;  }                                                                    
 
    public bool IsActive { get; set; } 

    public virtual Department? Department { get; set; }                         
}                
                                       