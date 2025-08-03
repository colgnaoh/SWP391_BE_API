using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CommunityProgramReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CommunityProgramsResModel;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public interface ICommunityProgramService
{
    //Task<IActionResult> GetCommunityProgramsByPageAsync(int pageNumber, int pageSize, string? filterByName);
    Task<IActionResult> GetCommunityProgramByIdAsync(Guid programId);
    Task<IActionResult> CreateCommunityProgramAsync(CommunityProgramCreateRequest request);
    Task<IActionResult> UpdateCommunityProgramAsync(Guid id, CommunityProgramUpdateRequest request);
    Task<EnrollProgramResponse> EnrollToProgramAsync(EnrollProgramRequest request, Guid userId);
    Task<List<EnrollmentHistoryItem>> GetEnrollmentHistoryAsync(Guid userId, ClaimsPrincipal user);
    Task<IActionResult> DeleteProgramAsync(Guid id);
    Task<IActionResult> GetCommunityProgramsByPageAsync(int pageNumber, int pageSize, string? filterByName, RiskLevel? riskLevel, Programtype? programType);
}
