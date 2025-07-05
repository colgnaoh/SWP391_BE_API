using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Helpers;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ApiResponse;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CourseReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class CourseService : ICourseService
    {
        private readonly DrugPreventionDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CourseService(DrugPreventionDbContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> GetCoursesByPageAsync(int pageNumber, int pageSize, string? filterByName)
        {
            try
            {
                var safePageNumber = pageNumber < 1 ? 1 : pageNumber;
                var safePageSize = pageSize < 1 ? 12 : pageSize;

                var query = _context.Courses.Where(c => !c.IsDeleted).AsQueryable();

                if (!string.IsNullOrEmpty(filterByName))
                {
                    query = query.Where(b => b.Name != null && EF.Functions.Like(b.Name, $"%{filterByName}%"));
                }

                var totalCount = await query.CountAsync();

                var courses = await query
                    .OrderBy(c => c.Id)
                    .Skip((safePageNumber - 1) * safePageSize)
                    .Take(safePageSize)
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling((double)totalCount / safePageSize);

                return new OkObjectResult(new GetCoursesByPageResponse
                {
                    Success = true,
                    Data = courses.Select(c => new CourseResponseModel
                    {
                        Id = c.Id,
                        UserId = (Guid)c.UserId,
                        CategoryId = c.CategoryId,
                        Name = c.Name,
                        Content = c.Content,
                        Status = c.Status,
                        TargetAudience = c.TargetAudience,
                        ImageUrls = c.ImageUrls,
                        VideoUrls = c.VideoUrls,
                        Price = c.Price,
                        Discount = c.Discount,
                        CreatedAt = c.CreatedAt,
                        Slug = c.Slug
                    }).ToList(),
                    TotalCount = totalCount,
                    PageNumber = safePageNumber,
                    PageSize = safePageSize,
                    TotalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                return new ObjectResult("Lỗi khi lấy danh sách khóa học.") { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> GetCourseByIdAsync(Guid courseId)
        {
            try
            {
                var course = await _context.Courses
                    .Include(c => c.User) 
                    .Include(c => c.Category)
                    .Include(c => c.Sessions.OrderBy(s => s.PositionOrder)) 
                    .ThenInclude(s => s.Lessons.OrderBy(l => l.PositionOrder))
                    .Include(c => c.Reviews) 
                    .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted);

                if (course == null)
                    return new NotFoundObjectResult("Không tìm thấy khóa học.");
                bool isInCart = false;
                bool isPurchased = false;
                var courseResponse = new CourseResponseModel
                {
                    Id = course.Id,
                    UserId = (Guid)course.UserId,
                    CategoryId = course.CategoryId,
                    Name = course.Name,
                    Content = course.Content,
                    Status = course.Status,
                    TargetAudience = course.TargetAudience,
                    ImageUrls = course.ImageUrls,
                    VideoUrls = course.VideoUrls,
                    Price = course.Price,
                    Discount = course.Discount,
                    CreatedAt = course.CreatedAt,
                    Slug = course.Slug,
                    IsInCart = isInCart,
                    IsPurchased = isPurchased,
                    SessionList = course.Sessions.Select(s => new SessionResponseModel
                    {
                        Id = s.Id,
                        Name = s.Name,
                        PositionOrder = s.PositionOrder,
                        Content = s.Content,
                        LessonList = s.Lessons.Select(l => new LessonResponseModel
                        {
                            Id = l.Id,
                            Name = l.Name,
                            Content = l.Content,
                            ImageUrl = l.ImageUrl,
                            VideoUrl = l.VideoUrl,
                            LessonType = l.LessonType, // Chuyển Enum sang string
                            PositionOrder = l.PositionOrder,
                            FullTime = l.FullTime
                        }).ToList()
                    }).ToList()
                };
                return new OkObjectResult(new CourseSingleItemRes
                {
                    Success = true,
                    Data = courseResponse
                });
            }
            catch (Exception)
            {
                return new ObjectResult("Lỗi khi truy xuất khóa học.") { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> CreateCourseAsync(CourseCreateModel CourseCreateRequest)
        {
            var userPrincipal = _httpContextAccessor.HttpContext?.User;
            if (userPrincipal == null)
            {
                return new UnauthorizedResult();
            }
            var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return new BadRequestObjectResult("Không tìm thấy ID người dùng.");
            }
            if (CourseCreateRequest.CategoryId != Guid.Empty)
            {
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == CourseCreateRequest.CategoryId);
                if (!categoryExists)
                {
                    return new BadRequestObjectResult("CategoryId không hợp lệ. Category này không tồn tại.");
                }
            }
            try
            {
                var course = new Course
                {
                    Id = Guid.NewGuid(),
                    Name = CourseCreateRequest.Name,
                    UserId = userId, 
                    CategoryId = CourseCreateRequest.CategoryId,
                    Content = CourseCreateRequest.Content,
                    Status = CourseCreateRequest.Status,
                    TargetAudience = CourseCreateRequest.TargetAudience,
                    RiskLevel = CourseCreateRequest.RiskLevel,
                    ImageUrls = CourseCreateRequest.ImageUrls,
                    VideoUrls = CourseCreateRequest.VideoUrls, 
                    Price = CourseCreateRequest.Price,
                    Discount = CourseCreateRequest.Discount,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                var baseSlug = string.IsNullOrWhiteSpace(CourseCreateRequest.Slug)
                    ? SlugGeneratorHelper.GenerateSlug(CourseCreateRequest.Name)
                    : SlugGeneratorHelper.GenerateSlug(CourseCreateRequest.Slug);

                course.Slug = await GenerateUniqueSlugAsync(baseSlug);

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                return new OkObjectResult(new CourseResponseModel
                {
                    Id = course.Id,
                    Name = course.Name,
                    Content = course.Content,
                    ImageUrls = CourseCreateRequest.ImageUrls,
                    VideoUrls = CourseCreateRequest.VideoUrls,
                    Price = course.Price,
                    Discount = course.Discount,
                    Status = course.Status,
                    TargetAudience = course.TargetAudience,
                    Slug = course.Slug,
                });
            }
            catch (Exception)
            {
                return new ObjectResult("Lỗi khi tạo khóa học.") { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> UpdateCourseAsync(Guid id, CourseUpdateModel model)
        {
            try
            {
                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

                if (course == null)
                {
                    return new NotFoundObjectResult("Không tìm thấy khóa học.");
                }


                // Name: string?
                if (!string.IsNullOrWhiteSpace(model.Name))
                {
                    course.Name = model.Name;
                }

                if (model.CategoryId != null && model.CategoryId != Guid.Empty)
                {
                    var categoryExists = await _context.Categories.AnyAsync(c => c.Id == model.CategoryId);
                    if (!categoryExists)
                    {
                        return new BadRequestObjectResult("CategoryId không hợp lệ. Category này không tồn tại.");
                    }
                    course.CategoryId = model.CategoryId;
                }
                if (model.UserId != null && model.UserId != Guid.Empty)
                {
                    var userExists = await _context.Users.AnyAsync(u => u.Id == model.UserId);
                    if (!userExists)
                    {
                        return new BadRequestObjectResult("UserId không hợp lệ. Người dùng này không tồn tại.");
                    }
                    course.UserId = model.UserId;
                }
                if (model.Content != null)
                {
                    course.Content = model.Content;
                }

                // Status: CourseStatus?
                if (model.Status != null)
                {
                    course.Status = model.Status;
                }

                // TargetAudience: targetAudience?
                if (model.TargetAudience != null)
                {
                    course.TargetAudience = model.TargetAudience;
                }


                if(model.RiskLevel != null)
{
                    course.RiskLevel = model.RiskLevel.Value;
                }

                /// ImageUrls: List<string>?
                if (model.ImageUrls != null && model.ImageUrls.Any())
                {
                    course.ImageUrls = model.ImageUrls;
                }

                // VideoUrls: List<string>?
                if (model.VideoUrls != null && model.VideoUrls.Any())
                {
                    course.VideoUrls = model.VideoUrls;
                }

                // Price: decimal?
                if (model.Price != null)
                {
                    course.Price = (decimal)model.Price;
                }

                // Discount: decimal?
                if (model.Discount != null)
                {
                    course.Discount = (decimal)model.Discount;
                }

                course.UpdatedAt = DateTime.UtcNow; // Luôn cập nhật thời gian thay đổi

                // Xử lý Slug: string?
                string desiredSlug = null;
                var newSlugProvided = !string.IsNullOrWhiteSpace(model.Slug);
                var newNameProvided = !string.IsNullOrWhiteSpace(model.Name);

                if (newSlugProvided)
                {
                    desiredSlug = SlugGeneratorHelper.GenerateSlug(model.Slug);
                }
                else if (newNameProvided) // Nếu không có slug mới nhưng có tên mới được cung cấp
                {
                    desiredSlug = SlugGeneratorHelper.GenerateSlug(model.Name);
                }

                if (desiredSlug != null && course.Slug != desiredSlug)
                {
                    course.Slug = await GenerateUniqueSlugAsync(desiredSlug, course.Id);
                }

                _context.Courses.Update(course);
                await _context.SaveChangesAsync();

                return new OkObjectResult("Cập nhật khóa học thành công.");
            }
            catch (Exception ex)
            {
                return new ObjectResult($"Lỗi khi cập nhật khóa học: {ex.Message}") { StatusCode = 500 };
            }
        }
        public async Task<IActionResult> SoftDeleteCourseAsync(Guid CourseId)
        {
            try
            {
                var course = await _context.Courses.FindAsync(CourseId);
                if (course == null || course.IsDeleted)
                {
                    return new NotFoundResult();
                }

                course.IsDeleted = true;
                course.UpdatedAt = DateTime.UtcNow;

                //Get all sessions related to the course
                var sessions = await _context.Sessions
                    .Where(s => s.CourseId == CourseId && !s.IsDeleted )
                    .ToListAsync();

                foreach (var session in sessions)
                {
                    session.IsDeleted = true;
                    session.UpdatedAt = DateTime.UtcNow;

                    //delete lessons related to sessions
                    var lessons = await _context.Lessons
                        .Where(l => l.SessionId == session.Id && !l.IsDeleted)
                        .ToListAsync();
                    foreach (var  lesson in lessons)
                    {
                        lesson.IsDeleted = true;
                        lesson.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                return new OkObjectResult("Khóa học đã được xóa mềm.");
            }
            catch (Exception)
            {
                return new ObjectResult("Lỗi khi xóa mềm khóa học.") { StatusCode = 500 };
            }
        }

        private async Task<string> GenerateUniqueSlugAsync(string baseSlug, Guid? excludeId = null)
        {
            var slug = baseSlug;
            var suffix = 1;

            while (await _context.Courses.AnyAsync(c =>
                c.Slug == slug &&
                !c.IsDeleted &&
                (excludeId == null || c.Id != excludeId)))
            {
                slug = $"{baseSlug}-{suffix++}";
            }

            return slug;
        }

        public async Task<IActionResult> GetPurchasedCoursesAsync()
        {
            try
            {
                // 1. Lấy User ID của người dùng đang đăng nhập
                Guid? userId = GetCurrentUserId();

                if (!userId.HasValue)
                {
                    return new UnauthorizedObjectResult(new BaseResponse(false, "Người dùng chưa đăng nhập hoặc không xác định được ID.", null));
                }

                var purchasedCourseIds = await _context.OrderLogs
                    .Where(ol => ol.UserId == userId.Value && // Kiểm tra hành động cụ thể khi giỏ hàng đã hoàn tất
                                 ol.CourseId.HasValue)
                    .Select(ol => ol.CourseId.Value)
                    .Distinct() // Đảm bảo chỉ lấy các CourseId duy nhất
                    .ToListAsync();

                if (!purchasedCourseIds.Any())
                {
                    // Trả về thành công nhưng không có dữ liệu nếu người dùng chưa mua khóa học nào
                    return new OkObjectResult(new BaseResponse(true, "Bạn chưa mua khóa học nào.", new List<CourseResponseModel>()));
                }

                // 3. Lấy thông tin chi tiết các khóa học dựa trên danh sách CourseId
                var purchasedCourses = await _context.Courses
                    .Where(c => purchasedCourseIds.Contains(c.Id) && !c.IsDeleted)
                    .Select(c => new CourseResponseModel
                    {
                        Id = c.Id,
                        UserId = (Guid)c.UserId,
                        CategoryId = c.CategoryId,
                        Name = c.Name,
                        Content = c.Content,
                        Status = c.Status,
                        TargetAudience = c.TargetAudience,
                        ImageUrls = c.ImageUrls,
                        VideoUrls = c.VideoUrls,
                        Price = c.Price,
                        Discount = c.Discount,
                        CreatedAt = c.CreatedAt,
                        Slug = c.Slug
                    })
                    .ToListAsync();

                return new OkObjectResult(new BaseResponse(true, "Lấy danh sách khóa học đã mua thành công.", purchasedCourses));
            }
            catch (Exception ex)
            {
                return new ObjectResult(new BaseResponse(false, $"Lỗi khi lấy danh sách khóa học đã mua: {ex.Message}", null)) { StatusCode = 500 };
            }
        }
        private Guid? GetCurrentUserId()
        {
            var userPrincipal = _httpContextAccessor.HttpContext?.User;
            if (userPrincipal == null)
            {
                return null;
            }
            var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return userId;
            }
            return null;
        }

    }
}
