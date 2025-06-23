using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface ICommunityProgramService
    {
        Task<IActionResult> GetCommunityProgramsByPageAsync(int pageNumber, int pageSize, string? filterByName);
        Task<IActionResult> GetCommunityProgramByIdAsync(Guid programId);
        Task<IActionResult> CreateCommunityProgramAsync(CommunityProgram program);
        Task<IActionResult> UpdateCommunityProgramAsync(Guid id, CommunityProgram updatedProgram);
        Task<IActionResult> DeleteProgramAsync(Guid id);
    }
}
