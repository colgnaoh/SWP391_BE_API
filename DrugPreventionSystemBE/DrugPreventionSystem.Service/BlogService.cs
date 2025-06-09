using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.BlogReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class BlogService : IBlogService
    {
        private readonly DrugPreventionDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BlogService(DrugPreventionDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
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
            var newBlog = new Blog
            {
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

        public Task<IActionResult> GetBlogsByPageAsync(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}
