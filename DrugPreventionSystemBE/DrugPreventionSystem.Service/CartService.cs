using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ApiResponse;
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
            var course = await _context.Courses
                                       .FirstOrDefaultAsync(c => c.Id == request.CourseId && !c.IsDeleted);
            if (course == null)
            {
                return new NotFoundObjectResult(new BaseResponse { Success = false, Message = "Id khóa học không tồn tại." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null)
            {
                return new UnauthorizedObjectResult(new BaseResponse { Success = false, Message = "Người dùng không tồn tại hoặc đã bị vô hiệu hóa." });
            }

            if (course.TargetAudience != CourseTargetAudience.GeneralPublic)
            {
                CourseTargetAudience userMappedTargetAudience;

                switch (user.AgeGroup)
                {
                    case AgeGroup.Student:
                        userMappedTargetAudience = CourseTargetAudience.Student;
                        break;
                    case AgeGroup.UniversityStudent:
                        userMappedTargetAudience = CourseTargetAudience.UniversityStudent;
                        break;
                    case AgeGroup.Parent:
                        userMappedTargetAudience = CourseTargetAudience.Parent; 
                        break;
                    default:
                        return new BadRequestObjectResult(new BaseResponse { Success = false, Message = "Nhóm tuổi của bạn không phù hợp để thêm khóa học này vào giỏ hàng." });
                }

                if (course.TargetAudience != userMappedTargetAudience)
                {
                    return new BadRequestObjectResult(new BaseResponse { Success = false, Message = $"Khóa học này chỉ dành cho đối tượng '{course.TargetAudience.ToString()}', không phù hợp với nhóm tuổi của bạn." });
                }
            }
            var existingCartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.CourseId == request.CourseId && !c.IsDeleted && c.Status == CartStatus.Pending);

            if (existingCartItem != null)
            {
                return new BadRequestObjectResult(new BaseResponse { Success = false, Message = "Khóa học này đã có trong giỏ hàng của bạn." });
            }

            var nextCartId = _idServices.GenerateNextId();

            var cartItem = new Cart
            {
                Id = nextCartId,
                UserId = userId,
                CourseId = request.CourseId,
                CartNo = $"CART-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                Status = CartStatus.Pending,
                Price = (decimal)course.Price,
                Discount = (decimal)course.Discount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Carts.Add(cartItem);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new BaseResponse { Success = true, Message = "Khóa học đã được thêm vào giỏ hàng thành công." });
        }

        public async Task<IActionResult> GetUserCartAsync(Guid userId)
        {
            // Only display cart items that are in Pending status
            var cartItems = await _context.Carts
                .Where(c => c.UserId == userId && !c.IsDeleted && c.Status == CartStatus.Pending)
                .Include(c => c.Course)
                .Select(c => new CartItemResponse
                {
                    CartId = c.Id,
                    CourseId = c.CourseId ?? Guid.Empty,
                    CourseName = c.Course.Name,
                    CourseImageUrl = c.Course.ImageUrls != null && c.Course.ImageUrls.Any()
                        ? c.Course.ImageUrls.First()
                        : null,
                    Price = c.Price,
                    Discount = c.Discount,
                    Status = c.Status,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            if (!cartItems.Any())
            {
                return new OkObjectResult(new BaseResponse { Success = false, Message = "Giỏ hàng của bạn trống." });
            }

            return new OkObjectResult(new BaseResponse
            {
                Success = true,
                Data = cartItems,
                Message = "Lấy thông tin giỏ hàng thành công."
            });
        }
        public async Task<IActionResult> RemoveCartItemAsync(Guid userId, Guid cartItemId)
        {
            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId && !c.IsDeleted && c.Status == CartStatus.Pending);

            if (cartItem == null)
            {
                return new NotFoundObjectResult(new BaseResponse { Success = false, Message = "Mục giỏ hàng không tồn tại hoặc không thuộc về bạn." });
            }

            cartItem.IsDeleted = true;
            cartItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new OkObjectResult(new BaseResponse { Success = true, Message = "Mục giỏ hàng đã được xóa thành công." });
        }

        public async Task<IActionResult> ClearUserCartAsync(Guid userId)
        {
            var cartItems = await _context.Carts
                .Where(c => c.UserId == userId && !c.IsDeleted && c.Status == CartStatus.Pending)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return new OkObjectResult(new BaseResponse { Success = true, Message = "Giỏ hàng của bạn đã trống." });
            }

            foreach (var item in cartItems)
            {
                item.IsDeleted = true;
                item.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new OkObjectResult(new BaseResponse { Success = true, Message = "Giỏ hàng của bạn đã được xóa sạch." });
        }
    }
}