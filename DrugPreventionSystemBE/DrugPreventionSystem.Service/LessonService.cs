using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.LessonReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using DrugPreventionSystemBE.DrugPreventionSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class LessonService : ILessonService
    {
        private readonly DrugPreventionDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IdServices _idServices;
        private readonly IUserService _userService;

        public LessonService(DrugPreventionDbContext context, IHttpContextAccessor httpContextAccessor, IdServices idServices, IUserService userService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _idServices = idServices;
            _userService = userService;
        }

        public async Task<IActionResult> CreateLessonAsync(CreateLessonRequest request)
        {
            var userPrincipal = _httpContextAccessor.HttpContext?.User;
            if (userPrincipal == null)
                return new UnauthorizedResult();

            var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                return new BadRequestObjectResult("Không tìm thấy ID người dùng.");

            var newLesson = new Lesson
            {
                Id = _idServices.GenerateNextId(),
                Name = request.Name,
                Content = request.Content,
                LessonType = request.LessonType,
                VideoUrl = request.VideoUrl,
                ImageUrl = request.ImageUrl,
                FullTime = request.FullTime,
                PositionOrder = request.PositionOrder,
                SessionId = request.SessionId,
                CourseId = request.CourseId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Lessons.Add(newLesson);
            await _context.SaveChangesAsync();
            return new OkObjectResult("Tạo bài học thành công.");
        }

        public async Task<IActionResult> GetLessonsByPageAsync(int pageNumber, int pageSize, string? filterByName)
        {
            var safePageNumber = pageNumber < 1 ? 1 : pageNumber;
            var safePageSize = pageSize < 1 ? 12 : pageSize;

            var query = _context.Lessons
                .Include(l => l.User)
                .Where(l => !l.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filterByName))
            {
                query = query.Where(l => l.Name != null && EF.Functions.Like(l.Name, $"%{filterByName}%"));
            }

            var totalCount = await query.CountAsync();
            var lessons = await query
                .OrderBy(l => l.PositionOrder)
                .Skip((safePageNumber - 1) * safePageSize)
                .Take(safePageSize)
                .ToListAsync();

            return new OkObjectResult(new GetLessonsByPageResponse
            {
                Success = true,
                Data = lessons.Select(l => new LessonResponseModel
                {
                    Id = l.Id,
                    Name = l.Name,
                    Content = l.Content,
                    LessonType = l.LessonType,
                    VideoUrl = l.VideoUrl,
                    ImageUrl = l.ImageUrl,
                    FullTime = l.FullTime,
                    PositionOrder = l.PositionOrder,
                    CreatedAt = l.CreatedAt,
                    UpdatedAt = l.UpdatedAt,
                    UserId = l.UserId,
                    FullName = $"{l.User?.LastName} {l.User?.FirstName}".Trim(),
                    UserAvatar = l.User?.ProfilePicUrl
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = safePageNumber,
                PageSize = safePageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / safePageSize)
            });
        }

        public async Task<IActionResult> GetLessonByIdAsync(Guid lessonId)
        {
            var lesson = await _context.Lessons
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == lessonId && !l.IsDeleted);

            if (lesson == null)
                return new NotFoundObjectResult("Không tìm thấy bài học.");

            var lessonResponse = new LessonResponseModel
            {
                Id = lesson.Id,
                Name = lesson.Name,
                Content = lesson.Content,
                LessonType = lesson.LessonType,
                VideoUrl = lesson.VideoUrl,
                ImageUrl = lesson.ImageUrl,
                FullTime = lesson.FullTime,
                PositionOrder = lesson.PositionOrder,
                CreatedAt = lesson.CreatedAt,
                UpdatedAt = lesson.UpdatedAt,
                UserId = lesson.UserId,
                FullName = $"{lesson.User?.LastName} {lesson.User?.FirstName}".Trim(),
                UserAvatar = lesson.User?.ProfilePicUrl
            };

            return new OkObjectResult(new SingleLessonResponse
            {
                Success = true,
                Data = lessonResponse
            });
        }

        public async Task<IActionResult> GetLessonsByUserAsync(Guid sessionId, int pageNumber = 1, int pageSize = 12)
        {
            var safePageNumber = pageNumber < 1 ? 1 : pageNumber;
            var safePageSize = pageSize < 1 ? 12 : pageSize;

            var query = _context.Lessons
                .Where(l => l.Id == sessionId && !l.IsDeleted);

            var totalCount = await query.CountAsync();
            var lessons = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((safePageNumber - 1) * safePageSize)
                .Take(safePageSize)
                .ToListAsync();

            return new OkObjectResult(new GetLessonsByPageResponse
            {
                Success = true,
                Data = lessons.Select(l => new LessonResponseModel
                {
                    Id = l.Id,
                    Name = l.Name,
                    Content = l.Content,
                    LessonType = l.LessonType,
                    VideoUrl = l.VideoUrl,
                    ImageUrl = l.ImageUrl,
                    FullTime = l.FullTime,
                    PositionOrder = l.PositionOrder,
                    CreatedAt = l.CreatedAt,
                    UpdatedAt = l.UpdatedAt,
                    UserId = l.UserId
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = safePageNumber,
                PageSize = safePageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / safePageSize)
            });
        }

        public async Task<IActionResult> UpdateLessonAsync(Guid lessonId, UpdateLessonRequest request)
        {
            var userPrincipal = _httpContextAccessor.HttpContext?.User;
            if (userPrincipal == null)
                return new UnauthorizedResult();

            var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                return new BadRequestObjectResult("Không tìm thấy ID người dùng.");

            var lesson = await _context.Lessons.FindAsync(lessonId);
            if (lesson == null || lesson.IsDeleted)
                return new NotFoundObjectResult("Không tìm thấy bài học.");

            if (lesson.UserId != userId)
                return new ForbidResult();

            lesson.Name = request.Name;
            lesson.Content = request.Content;
            lesson.LessonType = request.LessonType;
            lesson.VideoUrl = request.VideoUrl;
            lesson.ImageUrl = request.ImageUrl;
            lesson.FullTime = request.FullTime;
            lesson.PositionOrder = request.PositionOrder;
            lesson.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return new OkObjectResult("Cập nhật bài học thành công.");
        }

        public async Task<IActionResult> SoftDeleteLessonAsync(Guid lessonId)
        {
            var lesson = await _context.Lessons.FindAsync(lessonId);
            if (lesson == null || lesson.IsDeleted)
                return new NotFoundResult();

            lesson.IsDeleted = true;
            lesson.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return new OkObjectResult("Đã xóa mềm bài học thành công.");
        }
    }
}
