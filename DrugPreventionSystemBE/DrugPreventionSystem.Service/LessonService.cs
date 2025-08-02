using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
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

        public async Task<IActionResult> GetLessonsByPageAsync(
            int pageNumber,
            int pageSize,
            string? filterByName,
            Guid? courseId,
            Guid? sessionId,
            LessonType? lessonType)
        {
            var safePageNumber = pageNumber < 1 ? 1 : pageNumber;
            var safePageSize = pageSize < 1 ? 12 : pageSize;

            var query = _context.Lessons
                .Include(l => l.User)
                .Include(l => l.Course)
                .Include(l => l.Session)
                .Where(l => !l.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filterByName))
            {
                query = query.Where(l => l.Name != null && EF.Functions.Like(l.Name, $"%{filterByName}%"));
            }

            if (courseId.HasValue)
            {
                query = query.Where(l => l.CourseId == courseId.Value);
            }

            if (sessionId.HasValue)
            {
                query = query.Where(l => l.SessionId == sessionId.Value);
            }

            if (lessonType.HasValue)
            {
                query = query.Where(l => l.LessonType == lessonType.Value);
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
                    UserAvatar = l.User?.ProfilePicUrl,
                    CourseId = l.Course.Id,
                    SessionId = l.Session.Id
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
        .Include(l => l.Course)
        .Include(l => l.Session) //
        .FirstOrDefaultAsync(l => l.Id == lessonId && !l.IsDeleted); 

            if (lesson == null)
                return new NotFoundObjectResult("Không tìm thấy bài học trong phiên hoặc khóa học này."); 

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
                UserAvatar = lesson.User?.ProfilePicUrl,
                CourseId = lesson.Course.Id,
                SessionId = lesson.Session.Id 
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
                .Where(l => l.SessionId == sessionId && !l.IsDeleted);

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

        public async Task<IActionResult> UpdateLessonAsync(Guid id, UpdateLessonRequest request)
        {
            var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);
            if (lesson == null)
                return new NotFoundObjectResult("Không tìm thấy bài học.");

            // Validate and update CourseId
            if (request.CourseId.HasValue && request.CourseId != lesson.CourseId)
            {
                var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == request.CourseId.Value && !c.IsDeleted);
                if (course == null)
                    return new BadRequestObjectResult("Khóa học không tồn tại.");

                lesson.CourseId = request.CourseId.Value;
            }

            // Validate and update SessionId
            if (request.SessionId.HasValue && request.SessionId != lesson.SessionId)
            {
                var session = await _context.Sessions.FirstOrDefaultAsync(s => s.Id == request.SessionId.Value && !s.IsDeleted);
                if (session == null)
                    return new BadRequestObjectResult("Buổi học không tồn tại.");

                lesson.SessionId = request.SessionId.Value;
            }

            // Update other fields
            lesson.Name = request.Name ?? lesson.Name;
            lesson.Content = request.Content ?? lesson.Content;
            lesson.LessonType = request.LessonType ?? lesson.LessonType;
            lesson.VideoUrl = request.VideoUrl ?? lesson.VideoUrl;
            lesson.ImageUrl = request.ImageUrl ?? lesson.ImageUrl;
            lesson.FullTime = request.FullTime ?? lesson.FullTime;
            lesson.PositionOrder = request.PositionOrder ?? lesson.PositionOrder;
            lesson.UpdatedAt = DateTime.UtcNow;

            _context.Lessons.Update(lesson);
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
