using DrugPreventionSystemBE.DrugPreventionSystem.Enity;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Services
{
    public interface ICommunityProgramService
    {
        Task<IEnumerable<Program>> GetAllProgramsAsync();
        Task<Program?> GetProgramByIdAsync(Guid id);
        Task<Program> CreateProgramAsync(Program program);
        Task<bool> UpdateProgramAsync(Guid id, Program updatedProgram);
        Task<bool> DeleteProgramAsync(Guid id);
    }
}
