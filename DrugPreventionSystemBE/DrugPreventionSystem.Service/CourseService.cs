using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Helpers;
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
                        ImageUrl = c.ImageUrl,
                        Price = c.Price,
                        Discount = c.Discount,
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
                    .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted);

                if (course == null)
                    return new NotFoundObjectResult("Không tìm thấy khóa học.");

                return new OkObjectResult(new CourseResponseModel
                {
                    Id = course.Id,
                    Name = course.Name,
                    Content = course.Content,
                    ImageUrl = course.ImageUrl,
                    Price = course.Price,
                    Discount = course.Discount,
                    Status = course.Status,
                    TargetAudience = course.TargetAudience
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
            try
            {
                var course = new Course
                {
                    Id = Guid.NewGuid(),
                    Name = CourseCreateRequest.Name,
                    UserId = userId, // Fixed: Assigning the parsed Guid value
                    CategoryId = CourseCreateRequest.CategoryId,
                    Content = CourseCreateRequest.Content,
                    Status = CourseCreateRequest.Status,
                    TargetAudience = CourseCreateRequest.TargetAudience,
                    ImageUrl = CourseCreateRequest.ImageUrl,
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
                    ImageUrl = course.ImageUrl,
                    Price = course.Price,
                    Discount = course.Discount,
                    Status = course.Status,
                    TargetAudience = course.TargetAudience
                });
            }
            catch (Exception)
            {
                return new ObjectResult("Lỗi khi tạo khóa học.") { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> UpdateCourseAsync(CourseUpdateModel model)
        {
            try
            {
                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.Id == model.Id && !c.IsDeleted);

                if (course == null)
                    return new NotFoundObjectResult("Không tìm thấy khóa học.");

                course.Name = model.Name;
                course.UserId = Guid.NewGuid(); // TODO: Replace with actual user ID
                course.CategoryId = model.CategoryId;
                course.Content = model.Content;
                course.Status = model.Status;
                course.TargetAudience = model.TargetAudience;
                course.ImageUrl = model.ImageUrl;
                course.Price = model.Price;
                course.Discount = model.Discount;
                course.UpdatedAt = DateTime.UtcNow;

                var desiredSlug = string.IsNullOrWhiteSpace(model.Slug)
                    ? SlugGeneratorHelper.GenerateSlug(model.Name)
                    : SlugGeneratorHelper.GenerateSlug(model.Slug);

                if (course.Slug != desiredSlug)
                {
                    course.Slug = await GenerateUniqueSlugAsync(desiredSlug, course.Id);
                }

                _context.Courses.Update(course);
                await _context.SaveChangesAsync();

                return new OkObjectResult("Cập nhật khóa học thành công.");
            }
            catch (Exception)
            {
                return new ObjectResult("Lỗi khi cập nhật khóa học.") { StatusCode = 500 };
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
    }
}
