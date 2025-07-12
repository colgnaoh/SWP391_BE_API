using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.BlogReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using DrugPreventionSystemBE.DrugPreventionSystem.Services;
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
        private readonly IUserService _userService;

        public BlogService(DrugPreventionDbContext context, IHttpContextAccessor httpContextAccessor, IdServices idServices, IUserService userService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _idServices = idServices;
            _userService = userService;
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
                Title = request.Title,
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

            var query = _context.Blogs
                                .Include(b => b.User) 
                                .Where(b => !b.IsDeleted)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(filterByContent))
            {
                query = query.Where(b => b.Content != null && EF.Functions.Like(b.Content, $"%{filterByContent}%"));
            }
            var totalCount = await query.CountAsync();

            var blogs = await query
                .OrderBy(c => c.Id)
                .Skip((safePageNumber - 1) * safePageSize)
                .Take(safePageSize)
                .ToListAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / safePageSize);

            return new OkObjectResult(new GetBlogsByPageResponse
            {
                Success = true,
                Data = blogs.Select(b => new BlogResponseModel
                {
                    Id = b.Id,
                    UserId = (Guid)b.UserId,
                    Title = b.Title,
                    Content = b.Content,
                    BlogImgUrl = b.BlogImgUrl,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    IsDeleted = b.IsDeleted,
                    FullName = $"{b.User?.LastName} {b.User?.FirstName}".Trim(),
                    UserAvatar = b.User?.ProfilePicUrl ?? b.User?.ProfilePicUrl
                }).ToList(),
                TotalCount = totalCount,    
                PageNumber = safePageNumber,
                PageSize = safePageSize,
                TotalPages = totalPages
            });
        }

        public async Task<IActionResult> GetBlogByIdAsync(Guid blogId)
        {
            var blog = await _context.Blogs
                .Include(b => b.User) // Include User to get user details
                .Where(b => b.Id == blogId)                 
                .FirstOrDefaultAsync();

            if (blog == null)
            {
                return new NotFoundObjectResult("Không tìm thấy blog.");
            }

            var blogResponse = new BlogResponseModel
            {
                Id = blog.Id,
                UserId = (Guid)blog.UserId,
                Title = blog.Title,
                Content = blog.Content,
                BlogImgUrl = blog.BlogImgUrl,
                CreatedAt = blog.CreatedAt,
                UpdatedAt = blog.UpdatedAt,
                IsDeleted = blog.IsDeleted,
                FullName = $"{blog.User?.LastName} {blog.User?.FirstName}".Trim(),
                UserAvatar = blog.User?.ProfilePicUrl
            };

            return new OkObjectResult(new SingleBlogResponse
            {
                Success = true,
                Data = blogResponse
            });
        }

        public async Task<IActionResult> GetBlogsByUserAsync(Guid userId, int pageNumber = 1, int pageSize = 12)
        {
            var safePageNumber = pageNumber < 1 ? 1 : pageNumber;
            var safePageSize = pageSize < 1 ? 12 : pageSize;

            var query = _context.Blogs
                .Where(b => b.UserId == userId && !b.IsDeleted);

            var totalCount = await query.CountAsync();
            var blogs = await query
                .OrderByDescending(b => b.CreatedAt)
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
                    Title = b.Title,
                    Content = b.Content,
                    BlogImgUrl = b.BlogImgUrl,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    IsDeleted = b.IsDeleted
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = safePageNumber,
                PageSize = safePageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / safePageSize)
            });
        }

        public async Task<IActionResult> UpdateBlogAsync(Guid blogId, UpdateBlogRequest request)
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

            var blog = await _context.Blogs.FindAsync(blogId);
            if (blog == null || blog.IsDeleted)
            {
                return new NotFoundObjectResult("Không tìm thấy blog.");
            }

            // Check if user owns the blog
            if (blog.UserId != userId)
            {
                return new ForbidResult();
            }

            // Update fields
            blog.Title = request.Title;
            blog.Content = request.Content;
            blog.BlogImgUrl = request.BlogImgUrl;
            blog.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return new OkObjectResult("Cập nhật blog thành công.");
        }
        public async Task<IActionResult> SoftDeleteBlogAsync(Guid blogId)
        {
            var blog = await _context.Blogs.FindAsync(blogId);
            if (blog == null || blog.IsDeleted)
            {
                return new NotFoundResult();
            }

            blog.IsDeleted = true;
            blog.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return new OkObjectResult("Blog soft-deleted successfully.");
        }

    }
}
