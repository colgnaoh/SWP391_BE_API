using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Helpers;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SessionReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class SessionService : ISessionService
    {
        private readonly DrugPreventionDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionService(DrugPreventionDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> CreateAsync(SessionCreateModelView request)
        {
            var userPrincipal = _httpContextAccessor.HttpContext?.User;
            if (userPrincipal == null)
                return new UnauthorizedResult();

            var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                return new BadRequestObjectResult("Không tìm thấy ID người dùng.");

            var baseSlug = string.IsNullOrWhiteSpace(request.Slug)
                ? SlugGeneratorHelper.GenerateSlug(request.Name)
                : SlugGeneratorHelper.GenerateSlug(request.Slug);

            var uniqueSlug = await GenerateUniqueSlugAsync(baseSlug);

            var session = new Session
            {
                Id = Guid.NewGuid(),
                CourseId = request.CourseId,
                Name = request.Name,
                UserId = userId,
                Slug = uniqueSlug,
                Content = request.Content,
                PositionOrder = request.PositionOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new SessionViewModel
            {
                Id = session.Id,
                CourseId = session.CourseId,
                Name = session.Name,
                UserId = session.UserId,
                Slug = session.Slug,
                Content = session.Content,
                PositionOrder = session.PositionOrder
            });
        }

        public async Task<IActionResult> GetAllAsync(string? name = null, Guid? courseId = null, int pageNumber = 1, int pageSize = 12)
        {
            var safePageNumber = pageNumber < 1 ? 1 : pageNumber;
            var safePageSize = pageSize < 1 ? 12 : pageSize;

            var query = _context.Sessions
                .Include(s => s.Course)
                .Where(s => !s.IsDeleted)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(s => s.Name.Contains(name));
            }

            if (courseId.HasValue)
            {
                query = query.Where(s => s.CourseId == courseId.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / safePageSize);

            var sessions = await query
                .OrderBy(s => s.PositionOrder)
                .Skip((safePageNumber - 1) * safePageSize)
                .Take(safePageSize)
                .Select(s => new SessionViewModel
                {
                    Id = s.Id,
                    CourseId = s.CourseId,
                    Name = s.Name,
                    UserId = s.UserId,
                    Slug = s.Slug,
                    Content = s.Content,
                    PositionOrder = s.PositionOrder,
                    Course = new Course
                    {
                        Id = s.Course.Id,
                        Name = s.Course.Name,
                        Slug = s.Course.Slug,
                        CreatedAt = s.Course.CreatedAt,
                        UpdatedAt = s.Course.UpdatedAt
                    }
                })
                .ToListAsync();

            return new OkObjectResult(new GetAllSessionResponse
            {
                Success = true,
                Data = sessions,
                TotalCount = totalCount,
                PageNumber = safePageNumber,
                PageSize = safePageSize,
                TotalPages = totalPages
            });
        }



        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var s = await _context.Sessions
                .Where(x => x.Id == id && !x.IsDeleted)
                .FirstOrDefaultAsync();

            if (s == null)
                return new NotFoundObjectResult("Không tìm thấy buổi học.");

            return new OkObjectResult(new SessionSingleResponse
            {
                Success = true,
                Data = new SessionViewModel
                {
                    Id = s.Id,
                    CourseId = s.CourseId,
                    Name = s.Name,
                    UserId = s.UserId,
                    Slug = s.Slug,
                    Content = s.Content,
                    PositionOrder = s.PositionOrder
                }
            });
        }

        public async Task<IActionResult> GetSessionByPageAsync(Guid courseId, int pageNumber = 1, int pageSize = 12)
        {
            var query = _context.Sessions
                .Where(s => s.CourseId == courseId && !s.IsDeleted)
                .AsQueryable();

            var totalCount = await query.CountAsync();
            var sessions = await query
                .OrderBy(s => s.PositionOrder)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new OkObjectResult(new GetSessionsByPageResponse
            {
                Success = true,
                Data = sessions.Select(s => new SessionViewModel
                {
                    Id = s.Id,
                    CourseId = s.CourseId,
                    Name = s.Name,
                    UserId = s.UserId,
                    Slug = s.Slug,
                    Content = s.Content,
                    PositionOrder = s.PositionOrder
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

        public async Task<IActionResult> UpdateAsync(Guid id, SessionUpdateModelView request)
        {
            var s = await _context.Sessions.FindAsync(id);
            if (s == null || s.IsDeleted)
                return new NotFoundObjectResult("Không tìm thấy buổi học.");

            // Check and update CourseId if provided and different
            if (request.CourseId.HasValue && request.CourseId != s.CourseId)
            {
                var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == request.CourseId.Value && !c.IsDeleted);
                if (course == null)
                    return new BadRequestObjectResult("Khóa học mới không tồn tại.");

                s.CourseId = request.CourseId.Value;
            }



            s.Name = request.Name ?? s.Name;
            s.Content = request.Content ?? s.Content;
            s.PositionOrder = request.PositionOrder ?? s.PositionOrder;
            s.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(request.Slug))
            {
                var baseSlug = SlugGeneratorHelper.GenerateSlug(request.Slug);
                s.Slug = await GenerateUniqueSlugAsync(baseSlug, id);
            }

            _context.Sessions.Update(s);
            await _context.SaveChangesAsync();
            return new OkObjectResult("Cập nhật buổi học thành công.");
        }

        public async Task<IActionResult> SoftDeleteAsync(Guid id) 
        {
            var session = await _context.Sessions.FindAsync(id);
            if (session == null || session.IsDeleted)
                return new NotFoundObjectResult("Không tìm thấy buổi học.");

            session.IsDeleted = true;
            session.UpdatedAt = DateTime.UtcNow;

            //get all related lessons
            var relatedLessons = await _context.Lessons
                .Where(l => l.SessionId == id && !l.IsDeleted)
                .ToListAsync();

            foreach (var lesson in relatedLessons)
            {
                lesson.IsDeleted = true;
                lesson.UpdatedAt = DateTime.UtcNow;
            }

            _context.Sessions.Update(session);
            await _context.SaveChangesAsync();

            return new OkObjectResult("Xóa mềm buổi học thành công.");
        }

        private async Task<string> GenerateUniqueSlugAsync(string baseSlug, Guid? excludeId = null)
        {
            var slug = baseSlug;
            var suffix = 1;

            while (await _context.Sessions.AnyAsync(s =>
                s.Slug == slug &&
                !s.IsDeleted &&
                (excludeId == null || s.Id != excludeId)))
            {
                slug = $"{baseSlug}-{suffix++}";
            }

            return slug;
        }
    }
}
