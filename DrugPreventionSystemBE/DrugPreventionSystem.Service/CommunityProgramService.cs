using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Enity;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CommunityProgramResModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<IActionResult> GetCommunityProgramsByPageAsync(int pageNumber, int pageSize, string? filterByName)
        {
            var safePageNumber = pageNumber < 1 ? 1 : pageNumber;
            var safePageSize = pageSize < 1 ? 12 : pageSize;

            var query = _context.Programs.AsQueryable();

            if (!string.IsNullOrEmpty(filterByName))
            {
                query = query.Where(p => p.Name != null && EF.Functions.Like(p.Name, $"%{filterByName}%"));
            }

            var totalCount = await query.CountAsync();

            var programs = await query
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.Id)
                .Skip((safePageNumber - 1) * safePageSize)
                .Take(safePageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalCount / safePageSize);

            return new OkObjectResult(new GetProgramsByPageResponse
            {
                Success = true,
                Data = programs.Select(p => new CommunityProgramResponseModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = safePageNumber,
                PageSize = safePageSize,
                TotalPages = totalPages
            });
        }

        public async Task<IActionResult> GetCommunityProgramByIdAsync(Guid programId)
        {
            var program = await _context.Programs
                .Where(p => p.Id == programId)
                .FirstOrDefaultAsync();

            if (program == null)
            {
                return new NotFoundObjectResult("Không tìm thấy chương trình.");
            }

            var programResponse = new CommunityProgramResponseModel
            {
                Id = program.Id,
                Name = program.Name,
                Description = program.Description,
                StartDate = program.StartDate,
                EndDate = program.EndDate,
                CreatedAt = program.CreatedAt,
                UpdatedAt = program.UpdatedAt
            };

            return new OkObjectResult(new SingleCommunityProgramResponse
            {
                Success = true,
                Data = programResponse
            });
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

            program.IsDeleted = true;
            _context.Programs.Update(program);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
