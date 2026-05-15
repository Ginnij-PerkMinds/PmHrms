using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsDAL.Models;

public interface ISalaryStructureBAL
{
    Task<PagedResult<SalaryStructureDTO>> GetAll(int page, int size, string? search);
    Task<SalaryStructureDTO> Create(SalaryStructureModel model);
    Task<SalaryStructureDTO?> Update(int id, SalaryStructureModel model);
    Task<bool> Delete(int id);
    Task AssignToDesignation(int designationId, int salaryStructureId);
    Task<List<DesignationSalaryMappingDTO>> GetDesignationMappings();
    Task<List<SalaryComponentMaster>> LoadMaster();           // ✅ replaces broken GetAll()
    Task<SalaryResult?> GetSalaryByEmployeeId(int employeeId);
}