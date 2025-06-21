using DrugPreventionSystemBE.DrugPreventionSystem.Enity;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface ICommunityProgramService
    {
        Task<IActionResult> GetCommunityProgramsByPageAsync(int pageNumber, int pageSize, string? filterByName);
        Task<IActionResult> GetCommunityProgramByIdAsync(Guid programId);
        Task<CommunityProgram> CreateCommunityProgramAsync(CommunityProgram program);
        Task<bool> UpdateCommunityProgramAsync(Guid id, CommunityProgram updatedProgram);
        Task<bool> DeleteProgramAsync(Guid id);
    }
}

