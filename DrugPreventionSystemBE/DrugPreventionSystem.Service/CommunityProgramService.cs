using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Enity;
using Microsoft.EntityFrameworkCore;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Services
{
    public class CommunityProgramService : ICommunityProgramService
    {
        private readonly DrugPreventionDbContext _context;

        public CommunityProgramService(DrugPreventionDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CommunityProgram>> GetAllProgramsAsync()
        {
            return await _context.Programs.ToListAsync();
        }

        public async Task<CommunityProgram?> GetProgramByIdAsync(Guid id)
        {
            return await _context.Programs.FindAsync(id);
        }

        public async Task<CommunityProgram> CreateCommunityProgramAsync(CommunityProgram program)
        {
            program.Id = Guid.NewGuid();
            _context.Programs.Add(program);
            await _context.SaveChangesAsync();
            return program;
        }

        public async Task<bool> UpdateCommunityProgramAsync(Guid id, CommunityProgram updatedProgram)
        {
            var program = await _context.Programs.FindAsync(id);
            if (program == null) return false;

            program.Name = updatedProgram.Name;
            program.Description = updatedProgram.Description;
            program.Location = updatedProgram.Location;
            program.Type = updatedProgram.Type;
            program.StartDate = updatedProgram.StartDate;
            program.EndDate = updatedProgram.EndDate;
            program.ProgramImgUrl = updatedProgram.ProgramImgUrl;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProgramAsync(Guid id)
        {
            var program = await _context.Programs.FindAsync(id);
            if (program == null) return false;

            program.IsDeleted = true; //Soft delete
            _context.Programs.Update(program);

            await _context.SaveChangesAsync();
            return true;
        }


        Task<IEnumerable<CommunityProgram>> ICommunityProgramService.GetAllProgramsAsync()
        {
            throw new NotImplementedException();
        }

        Task<CommunityProgram?> ICommunityProgramService.GetProgramByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<CommunityProgram> CreateProgramAsync(Program program)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateProgramAsync(Guid id, Program updatedProgram)
        {
            throw new NotImplementedException();
        }

        public Task<CommunityProgram> CreateCommunityProgramAsync(Program program)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateCommunityProgramAsync(Guid id, Program updatedProgram)
        {
            throw new NotImplementedException();
        }

        Task<CommunityProgram> ICommunityProgramService.CreateCommunityProgramAsync(CommunityProgram program)
        {
            throw new NotImplementedException();
        }
    }
}
