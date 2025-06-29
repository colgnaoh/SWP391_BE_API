using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CourseReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class CartService : ICartService
    {
        private readonly DrugPreventionDbContext _context;
        private readonly IdServices _idServices;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(DrugPreventionDbContext context, IdServices idServices, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _idServices = idServices;
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out Guid userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID not found in token.");
        }

        public async Task<IActionResult> AddCourseToCartAsync(Guid userId, AddToCartRequest request)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == request.CourseId && !c.IsDeleted);
            if (course == null)
            {
                return new NotFoundObjectResult("Id khóa học không tồn tại.");
            }

            // Kiểm tra xem khóa học đã có trong giỏ hàng của người dùng chưa
            var existingCartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.CourseId == request.CourseId && !c.IsDeleted);

            if (existingCartItem != null)
            {
                return new BadRequestObjectResult("Khóa học này đã có trong giỏ hàng của bạn.");
            }

            var nextCartId = _idServices.GenerateNextId(); // Sử dụng IdServices để tạo ID

            var cartItem = new Cart
            {
                Id = nextCartId,
                UserId = userId,
                CourseId = request.CourseId,
                CartNo = $"CART-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}", // Mã giỏ hàng đơn giản
                Status = Enum.CartStatus.Pending, // Có thể là "Pending", "Active"
                Price = course.Price ?? 0m, // Default to 0 if Price is null
                Discount = course.Discount ?? 0m, // Default to 0 if Discount is null
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Carts.Add(cartItem);
            await _context.SaveChangesAsync();

            return new OkObjectResult("Khóa học đã được thêm vào giỏ hàng thành công.");
        }

        public async Task<IActionResult> GetUserCartAsync(Guid userId)
        {
            var cartItems = await _context.Carts
                .Where(c => c.UserId == userId && !c.IsDeleted && c.Status == Enum.CartStatus.Pending) // Chỉ lấy các mục đang hoạt động
                .Include(c => c.Course) // Kéo theo thông tin khóa học
                .Select(c => new CartItemResponse
                {
                    CartId = c.Id,
                    CourseId = c.CourseId ?? Guid.Empty, 
                    CourseName = c.Course.Name,
                    CourseImageUrl = c.Course.ImageUrl,
                    Price = c.Price,
                    Discount = c.Discount,
                    Status = c.Status,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            if (!cartItems.Any())
            {
                return new NotFoundObjectResult("Giỏ hàng của bạn trống.");
            }

            return new OkObjectResult(new CartListResponse
            {
                Success = true,
                Data = cartItems,
            });
        }

        public async Task<IActionResult> RemoveCartItemAsync(Guid userId, Guid cartItemId)
        {
            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId && !c.IsDeleted);

            if (cartItem == null)
            {
                return new NotFoundObjectResult("Mục giỏ hàng không tồn tại hoặc không thuộc về bạn.");
            }

            cartItem.IsDeleted = true; // Đánh dấu xóa mềm
            cartItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new OkObjectResult("Mục giỏ hàng đã được xóa thành công.");
        }

        public async Task<IActionResult> ClearUserCartAsync(Guid userId)
        {
            var cartItems = await _context.Carts
                .Where(c => c.UserId == userId && !c.IsDeleted && c.Status == Enum.CartStatus.Pending)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return new OkObjectResult("Giỏ hàng của bạn đã trống.");
            }

            foreach (var item in cartItems)
            {
                item.IsDeleted = true;
                item.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new OkObjectResult("Giỏ hàng của bạn đã được xóa sạch.");
        }
    }
}
