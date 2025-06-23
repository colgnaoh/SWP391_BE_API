using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
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
            try
            {
                int currentPage = pageNumber < 1 ? 1 : pageNumber;
                int currentPageSize = pageSize < 1 ? 12 : pageSize;

                var query = _context.Programs
                    .Where(p => !p.IsDeleted);

                if (!string.IsNullOrEmpty(filterByName))
                {
                    query = query.Where(p => EF.Functions.Like(p.Name!, $"%{filterByName}%"));
                }

                int totalCount = await query.CountAsync();

                var programs = await query
                    .OrderBy(p => p.Id)
                    .Skip((currentPage - 1) * currentPageSize)
                    .Take(currentPageSize)
                    .ToListAsync();

                var programDtos = programs.Select(p => new CommunityProgramResponseModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList();

                return new OkObjectResult(new GetProgramsByPageResponse
                {
                    Success = true,
                    Data = programDtos,
                    TotalCount = totalCount,
                    PageNumber = currentPage,
                    PageSize = currentPageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / currentPageSize)
                });
            }
            catch (Exception)
            {
                return new ObjectResult("Lỗi khi truy xuất danh sách chương trình.") { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> GetCommunityProgramByIdAsync(Guid programId)
        {
            try
            {
                var program = await _context.Programs
                    .FirstOrDefaultAsync(p => p.Id == programId && !p.IsDeleted);

                if (program == null)
                {
                    return new NotFoundObjectResult("Không tìm thấy chương trình.");
                }

                var response = new CommunityProgramResponseModel
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
                    Data = response
                });
            }
            catch (Exception)
            {
                return new ObjectResult("Lỗi khi truy xuất chương trình.") { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> CreateCommunityProgramAsync(CommunityProgram program)
        {
            try
            {
                program.Id = Guid.NewGuid();
                program.CreatedAt = DateTime.UtcNow;

                _context.Programs.Add(program);
                await _context.SaveChangesAsync();

                return new OkObjectResult(new
                {
                    Success = true,
                    Message = "Tạo chương trình thành công.",
                    Data = program
                });
            }
            catch (Exception)
            {
                return new ObjectResult("Lỗi khi tạo chương trình.") { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> UpdateCommunityProgramAsync(Guid id, CommunityProgram updated)
        {
            try
            {
                var program = await _context.Programs
                    .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

                if (program == null)
                {
                    return new NotFoundObjectResult("Không tìm thấy chương trình.");
                }

                program.Name = updated.Name;
                program.Description = updated.Description;
                program.Location = updated.Location;
                program.Type = updated.Type;
                program.StartDate = updated.StartDate;
                program.EndDate = updated.EndDate;
                program.ProgramImgUrl = updated.ProgramImgUrl;
                program.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new OkObjectResult("Cập nhật chương trình thành công.");
            }
            catch (Exception)
            {
                return new ObjectResult("Lỗi khi cập nhật chương trình.") { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> DeleteProgramAsync(Guid id)
        {
            try
            {
                var program = await _context.Programs
                    .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

                if (program == null)
                {
                    return new NotFoundObjectResult("Chương trình không tồn tại hoặc đã bị xóa.");
                }

                program.IsDeleted = true;
                program.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new OkObjectResult("Xóa mềm chương trình thành công.");
            }
            catch (Exception)
            {
                return new ObjectResult("Lỗi khi xóa chương trình.") { StatusCode = 500 };
            }
        }
    }
}
