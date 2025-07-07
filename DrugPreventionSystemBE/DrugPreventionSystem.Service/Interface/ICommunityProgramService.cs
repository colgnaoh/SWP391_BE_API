using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CommunityProgramReqModel;
using Microsoft.AspNetCore.Mvc;

public interface ICommunityProgramService
{
    Task<IActionResult> GetCommunityProgramsByPageAsync(int pageNumber, int pageSize, string? filterByName);
    Task<IActionResult> GetCommunityProgramByIdAsync(Guid programId);
    Task<IActionResult> CreateCommunityProgramAsync(CommunityProgramCreateRequest request);
    Task<IActionResult> UpdateCommunityProgramAsync(Guid id, CommunityProgramUpdateRequest request);
    Task<IActionResult> DeleteProgramAsync(Guid id);
}
