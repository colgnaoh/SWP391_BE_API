using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.BlogReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class BlogService : IBlogService
    {
        private readonly DrugPreventionDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IdServices _idServices;


        public BlogService(DrugPreventionDbContext context, IHttpContextAccessor httpContextAccessor, IdServices idServices)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _idServices = idServices;
        }

        public async Task<IActionResult> CreateBlogAsync(CreateBlogRequest request)
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
            var nextId = _idServices.GenerateNextId();
            var newBlog = new Blog
            {
                Id = nextId, // Sử dụng ID được tạo từ IdServices
                UserId = userId, // Gán UserId đã lấy từ token
                Content = request.Content,
                BlogImgUrl = request.BlogImgUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            _context.Blogs.Add(newBlog);
            await _context.SaveChangesAsync();

            return new OkObjectResult("Tạo blog thành công.");
        }

        public async Task<IActionResult> GetBlogsByPageAsync(int pageNumber, int pageSize, string? filterByContent)
        {
            // Đảm bảo pageNumber và pageSize hợp lệ
            var safePageNumber = pageNumber < 1 ? 1 : pageNumber;
            var safePageSize = pageSize < 1 ? 12 : pageSize; // Mặc định pageSize là 12 nếu không hợp lệ

            var query = _context.Blogs.AsQueryable();

            if (!string.IsNullOrEmpty(filterByContent))
            {
                query = query.Where(b => b.Content != null && EF.Functions.Like(b.Content, $"%{filterByContent}%"));
            }

            var blogs = await query
                .OrderBy(c => c.Id)
                .Skip((safePageNumber - 1) * safePageSize)
                .Take(safePageSize)
                .ToListAsync();

            return new OkObjectResult(new GetBlogsByPageResponse
            {
                Success = true,
                Data = blogs.Select(b => new BlogResponseModel
                {
                    Id = b.Id,
                    UserId = (Guid)b.UserId,
                    Content = b.Content,
                    BlogImgUrl = b.BlogImgUrl,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt
                }).ToList()
            });
        }
    }
}
