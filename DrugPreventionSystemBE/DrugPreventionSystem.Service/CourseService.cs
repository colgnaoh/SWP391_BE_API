using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Enity;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CourseReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class CourseService : ICourseService
    {
        private readonly DrugPreventionDbContext _context;
        private readonly IConfiguration _configuration;

        public CourseService(DrugPreventionDbContext context, IConfiguration configuration)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration;
        }

        public async Task<IActionResult> GetCoursesByPageAsync([FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] string? filterByName)
        {
            // Đảm bảo pageNumber và pageSize hợp lệ
            var safePageNumber = pageNumber < 1 ? 1 : pageNumber;
            var safePageSize = pageSize < 1 ? 12 : pageSize; // Mặc định pageSize là 12 nếu không hợp lệ

            var query = _context.Courses.AsQueryable();

            if (!string.IsNullOrEmpty(filterByName))
            {
                query = query.Where(b => b.Name != null && EF.Functions.Like(b.Name, $"%{filterByName}%"));
            }

            var courses = await query
                .OrderBy(c => c.Id)
                .Skip((safePageNumber - 1) * safePageSize)
                .Take(safePageSize)
                .ToListAsync();

            return new OkObjectResult(new GetCoursesByPageResponse
            {
                Success = true,
                Data = courses.Select(b => new CourseResponseModel
                {
                    Id = b.Id,
                    UserId = (Guid)b.UserId,
                    CategoryId = b.CategoryId,
                    Name = b.Name,
                    Content = b.Content,
                    Status = b.Status,
                    TargetAudience = b.TargetAudience,
                    ImageUrl = b.ImageUrl,
                    Price = b.Price,
                    Discount = b.Discount,
                    Slug = b.Slug
                }).ToList()
            });
        }
        public Task<IActionResult> GetCourseByIdAsync(int courseId)
        {
            throw new NotImplementedException();
        }

        public async Task<IActionResult> CreateCourseAsync(CourseCreateRequest request)
        {
            // Kiểm tra dữ liệu đầu vào
            if (request == null || string.IsNullOrEmpty(request.Name) || request.CategoryId == Guid.Empty || string.IsNullOrEmpty(request.token))
            {
                return new BadRequestObjectResult("Dữ liệu đầu vào không hợp lệ");
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);
            try
            {
                tokenHandler.ValidateToken(request.token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false, // Có thể bật nếu dùng Issuer
                    ValidateAudience = false, // Có thể bật nếu dùng Audience
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
                {
                    return new UnauthorizedObjectResult("Token không hợp lệ hoặc không chứa UserId.");
                }

                // Kiểm tra user tồn tại
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userIdGuid && !u.IsDeleted);
                if (user == null)
                {
                    return new BadRequestObjectResult("Người dùng không tồn tại hoặc đã bị xóa.");
                }

                // Kiểm tra user đã xác thực email
                if (!user.IsVerified)
                {
                    return new UnauthorizedObjectResult("Tài khoản chưa được xác thực email.");
                }

                // Kiểm tra quyền (nếu chỉ giáo viên/admin được tạo khóa học)
                if (user.Role != Enum.Role.Manager && user.Role != Enum.Role.Admin)
                {
                    return new ForbidResult("Chỉ nhân viên hoặc admin được phép tạo khóa học.");
                }

                // Kiểm tra CategoryId
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId && !c.IsDeleted);
                if (!categoryExists)
                {
                    return new BadRequestObjectResult("Danh mục không tồn tại hoặc đã bị xóa.");
                }

                // Kiểm tra giá và giảm giá
                if (request.Price.HasValue && request.Price < 0)
                {
                    return new BadRequestObjectResult("Giá khóa học không được âm.");
                }
                if (request.Discount.HasValue && (request.Discount < 0 || (request.Price.HasValue && request.Discount > request.Price)))
                {
                    return new BadRequestObjectResult("Giảm giá không hợp lệ.");
                }

                // Tạo entity Course
                var course = new Course
                {
                    //Id = Guid.NewGuid(),
                    //Name = request.Name,
                    //UserId = userIdGuid, // Lấy từ token
                    //CategoryId = request.CategoryId,
                    //Content = request.Content,
                    //Status = request.Status,
                    //TargetAudience = request.,
                    //Price = request.Price,
                    //Discount = request.Discount,
                    //CreatedAt = DateTime.UtcNow,
                    //UpdatedAt = DateTime.UtcNow,
                    //IsDeleted = false,
                    //CourseImgUrl = request.CourseImgUrl
                };

                // Lưu vào database
                try
                {
                    _context.Courses.Add(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    return new ObjectResult($"Lỗi khi lưu khóa học: {ex.Message}") { StatusCode = 500 };
                }

                // Trả về response theo phong cách AuthenticationService
                return new OkObjectResult(new
                {
                    Success = true,
                    Message = "Tạo khóa học thành công.",
                    Data = new
                    {
                        CourseId = course.Id,
                        Name = course.Name,
                        CreatedAt = course.CreatedAt,
                        UserId = course.UserId
                    }
                });
            }
            catch (SecurityTokenException ex)
            {
                return new UnauthorizedObjectResult($"Token không hợp lệ: {ex.Message}");
            }
            catch (Exception ex)
            {
                return new ObjectResult($"Lỗi hệ thống: {ex.Message}") { StatusCode = 500 };
            }
        }
    }
}
