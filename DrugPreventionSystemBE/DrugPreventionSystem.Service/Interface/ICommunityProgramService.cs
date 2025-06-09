using DrugPreventionSystemBE.DrugPreventionSystem.Enity;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Services
{
    public interface ICommunityProgramService
    {
        Task<IEnumerable<CommunityProgram>> GetAllProgramsAsync();
        Task<CommunityProgram?> GetProgramByIdAsync(Guid id);
        Task<CommunityProgram> CreateCommunityProgramAsync(CommunityProgram program);
        Task<bool> UpdateCommunityProgramAsync(Guid id, CommunityProgram updatedProgram);
        Task<bool> DeleteProgramAsync(Guid id);
    }
}
