using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CommunityProgramReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CommunityProgramResModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CommunityProgramsResModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
                    Location = p.Location,
                    Type = p.Type,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    ProgramImgUrl = p.ProgramImgUrl,
                    ProgramVidUrl = p.ProgramVidUrl,
                    RiskLevel = p.RiskLevel,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Message = ""
                }).ToList();

                return new OkObjectResult(new GetProgramsByPageResponse
                {
                    Success = true,
                    Message = "Lấy danh sách thành công.",
                    Data = programDtos,
                    TotalCount = totalCount,
                    PageNumber = currentPage,
                    PageSize = currentPageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / currentPageSize)
                }); ;
            }
            catch (Exception)
            {
                return new ObjectResult(new GetProgramsByPageResponse
                {
                    Success = false,
                    Message = "Lỗi khi truy xuất danh sách chương trình."
                })
                { StatusCode = 500 };
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
                    return new NotFoundObjectResult(new SingleProgramResponseModel
                    {
                        Success = false,
                        Message = "Không tìm thấy chương trình."
                    });
                }

                var response = new CommunityProgramResponseModel
                {
                    Id = program.Id,
                    Name = program.Name,
                    Description = program.Description,
                    Location = program.Location,
                    Type = program.Type,
                    StartDate = program.StartDate,
                    EndDate = program.EndDate,
                    ProgramImgUrl = program.ProgramImgUrl,
                    ProgramVidUrl = program.ProgramVidUrl,
                    RiskLevel = program.RiskLevel,
                    CreatedAt = program.CreatedAt,
                    UpdatedAt = program.UpdatedAt,
                    Message = "Lấy chương trình thành công."
                };

                return new OkObjectResult(response);
            }
            catch (Exception)
            {
                return new ObjectResult(new SingleProgramResponseModel
                {
                    Success = false,
                    Message = "Lỗi khi truy xuất chương trình."
                })
                { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> CreateCommunityProgramAsync(CommunityProgramCreateRequest request)
        {
            try
            {
                var program = new CommunityProgram
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description,
                    Location = request.Location,
                    Type = request.Type,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    ProgramImgUrl = request.ProgramImgUrl,
                    ProgramVidUrl = request.ProgramVidUrl, 
                    RiskLevel = request.RiskLevel,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                _context.Programs.Add(program);
                await _context.SaveChangesAsync();

                var response = new CommunityProgramResponseModel
                {
                    Id = program.Id,
                    Name = program.Name,
                    Description = program.Description,
                    Location = program.Location,
                    Type = program.Type,
                    StartDate = program.StartDate,
                    EndDate = program.EndDate,
                    ProgramImgUrl = program.ProgramImgUrl,
                    ProgramVidUrl = program.ProgramVidUrl,
                    RiskLevel = program.RiskLevel,
                    CreatedAt = program.CreatedAt,
                    UpdatedAt = program.UpdatedAt,
                    Message = "Tạo chương trình thành công."
                };

                return new OkObjectResult(response);
            }
            catch (Exception)
            {
                return new ObjectResult(new SingleProgramResponseModel
                {
                    Success = false,
                    Message = "Lỗi khi tạo chương trình."
                })
                { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> UpdateCommunityProgramAsync(Guid id, CommunityProgramUpdateRequest request)
        {
            try
            {
                var program = await _context.Programs
                    .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

                if (program == null)
                {
                    return new NotFoundObjectResult(new SingleProgramResponseModel
                    {
                        Success = false,
                        Message = "Không tìm thấy chương trình."
                    });
                }

                // Update fields
                program.Name = request.Name;
                program.Description = request.Description;
                program.Location = request.Location;
                program.Type = request.Type;
                program.StartDate = request.StartDate;
                program.EndDate = request.EndDate;
                program.ProgramImgUrl = request.ProgramImgUrl;
                program.ProgramVidUrl = request.ProgramVidUrl;
                program.RiskLevel = request.RiskLevel;
                program.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                //Return updated data
                return new OkObjectResult(new CommunityProgramResponseModel
                {
                    Id = program.Id,
                    Name = program.Name,
                    Description = program.Description,
                    Location = program.Location,
                    Type = program.Type,
                    StartDate = program.StartDate,
                    EndDate = program.EndDate,
                    ProgramImgUrl = program.ProgramImgUrl,
                    ProgramVidUrl = program.ProgramVidUrl,
                    RiskLevel = program.RiskLevel,
                    CreatedAt = program.CreatedAt,
                    UpdatedAt = program.UpdatedAt,
                    Message = "Cập nhật chương trình thành công."
                });
            }
            catch (Exception)
            {
                return new ObjectResult(new SingleProgramResponseModel
                {
                    Success = false,
                    Message = "Lỗi khi cập nhật chương trình."
                })
                { StatusCode = 500 };
            }
        }


        public async Task<EnrollProgramResponse> EnrollToProgramAsync(EnrollProgramRequest request, Guid userId)
        {
            var program = await _context.Programs
                .FirstOrDefaultAsync(p => p.Id == request.ProgramId && !p.IsDeleted);

            if (program == null)
            {
                return new EnrollProgramResponse
                {
                    Success = false,
                    Message = "Chương trình không tồn tại."
                };
            }

            bool alreadyEnrolled = await _context.ProgramRegistrations
                .AnyAsync(r => r.UserId == userId && r.ProgramId == request.ProgramId && !r.IsDeleted);

            if (alreadyEnrolled)
            {
                return new EnrollProgramResponse
                {
                    Success = false,
                    Message = "Bạn đã tham gia chương trình này rồi."
                };
            }

            var registration = new ProgramRegistration
            {
                UserId = userId,
                ProgramId = request.ProgramId,
                JoinDate = DateTime.UtcNow
            };

            _context.ProgramRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            return new EnrollProgramResponse
            {
                Success = true,
                Message = "Tham gia chương trình thành công."
            };
        }

        public async Task<List<EnrollmentHistoryItem>> GetEnrollmentHistoryAsync(Guid userId, ClaimsPrincipal user)
{
    var isAdminOrManager = user.IsInRole("Admin") || user.IsInRole("Manager");

    if (isAdminOrManager)
    {
        var programs = await _context.Programs
            .Where(p => !p.IsDeleted)
            .Select(p => new EnrollmentHistoryItem
            {
                ProgramId = p.Id,
                Name = p.Name,
                Description = p.Description,
                Location = p.Location,
                Type = p.Type,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                ProgramImgUrl = p.ProgramImgUrl,
                ProgramVidUrl = p.ProgramVidUrl,
                RiskLevel = p.RiskLevel,
                JoinDate = null,
                UserCount = _context.ProgramRegistrations.Count(r => r.ProgramId == p.Id && !r.IsDeleted)
            })
            .ToListAsync();

        return programs;
    }
    else
    {
        var joinedPrograms = await _context.ProgramRegistrations
            .Include(r => r.Program)
            .Where(r => r.UserId == userId && !r.IsDeleted && !r.Program.IsDeleted)
            .OrderByDescending(r => r.JoinDate)
            .ToListAsync();

        var result = joinedPrograms.Select(r => new EnrollmentHistoryItem
        {
            ProgramId = r.Program.Id,
            Name = r.Program.Name,
            Description = r.Program.Description,
            Location = r.Program.Location,
            Type = r.Program.Type,
            StartDate = r.Program.StartDate,
            EndDate = r.Program.EndDate,
            ProgramImgUrl = r.Program.ProgramImgUrl,
            ProgramVidUrl = r.Program.ProgramVidUrl,
            RiskLevel = r.Program.RiskLevel,
            JoinDate = r.JoinDate,
            UserCount = null // Không hiển thị
        }).ToList();

        return result;
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
                    return new NotFoundObjectResult(new SingleProgramResponseModel
                    {
                        Success = false,
                        Message = "Chương trình không tồn tại hoặc đã bị xóa."
                    });
                }

                program.IsDeleted = true;
                program.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new OkObjectResult(new SingleProgramResponseModel
                {
                    Success = true,
                    Message = "Xóa mềm chương trình thành công."
                });
            }
            catch (Exception)
            {
                return new ObjectResult(new SingleProgramResponseModel
                {
                    Success = false,
                    Message = "Lỗi khi xóa chương trình."
                })
                { StatusCode = 500 };
            }
        }
    }
}
