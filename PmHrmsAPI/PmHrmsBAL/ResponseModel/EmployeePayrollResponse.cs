using System.Collections.Generic;
using DocumentFormat.OpenXml.Bibliography;

namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class EmployeePayrollResponse
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode {get; set; } 
        public string DepartmentName {get; set; } 
        public byte TotalWorkingDays { get; set; }
        public decimal PresentDays { get; set; }
        public decimal AbsentDays { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPayable { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Remarks { get; set; }
        public List<ComponentResult> Components { get; set; } = new();
        public System.DateTime CreatedAt { get; set; }
        public System.DateTime UpdatedAt { get; set; }

        public string? BankName { get; set; }
         public string? AccountHolderName { get; set; }
         public string? AccountNumber { get; set; }
         public string? IFSCCode { get; set; }
    }
}
