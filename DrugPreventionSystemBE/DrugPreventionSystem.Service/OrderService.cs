using CloudinaryDotNet.Actions;
using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ApiResponse;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Role = DrugPreventionSystemBE.DrugPreventionSystem.Enum.Role;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class OrderService : IOrderService
    {
        private readonly DrugPreventionDbContext _context;
        private readonly IdServices _idServices;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderService(DrugPreventionDbContext context, IdServices idServices, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _idServices = idServices;
            _httpContextAccessor = httpContextAccessor;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out Guid userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID not found in token.");
        }

        private string GetCurrentUserRole()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
        }

        public async Task<IActionResult> CreateOrderFromCartAsync(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null)
            {
                return new BadRequestObjectResult(new BaseResponse { Success = false, Message = "Người dùng không tồn tại." });
            }

            // Lấy Cart, chỉ cần Include Course vì chỉ có dịch vụ CourseSale
            var cart = await _context.Carts
                .Include(c => c.Course)          // Load Course
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == CartStatus.Pending && !c.IsDeleted);

            if (cart == null)
            {
                return new NotFoundObjectResult(new BaseResponse { Success = false, Message = "Không tìm thấy giỏ hàng hoạt động hoặc giỏ hàng trống." });
            }

            // Xác định thông tin của mục trong giỏ hàng (chỉ Course)
            bool hasValidItem = false;
            decimal itemPrice = 0;
            string itemName = string.Empty;
            Guid? itemId = null;
            ServiceType? serviceType = null; // Sẽ luôn là CourseSale

            // Chỉ xử lý nếu ServiceType là CourseSale
            if (cart.CourseId.HasValue && cart.Course != null)
            {
                hasValidItem = true;
                itemPrice = (decimal)(cart.Course.Price - cart.Course.Discount);
                itemName = cart.Course.Name;
                itemId = cart.CourseId;
                serviceType = DrugPreventionSystemBE.DrugPreventionSystem.Enum.ServiceType.CourseSale;
            }
            

            if (!hasValidItem)
            {
                return new BadRequestObjectResult(new BaseResponse { Success = false, Message = "Giỏ hàng không chứa khóa học hợp lệ để tạo đơn hàng." });
            }

            // Tạo Order mới
            var newOrderId = _idServices.GenerateNextId();
            var order = new Order
            {
                Id = newOrderId,
                UserId = userId,
                CartId = cart.Id,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending, // Đơn hàng mới tạo đang chờ thanh toán
                TotalAmount = itemPrice, // Tổng tiền là giá của khóa học
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrderDetails = new List<OrderDetail>() // Khởi tạo danh sách OrderDetails
            };

            // Tạo OrderDetail duy nhất cho khóa học
            var orderDetailId = _idServices.GenerateNextId();
            var orderDetail = new OrderDetail
            {
                Id = orderDetailId,
                OrderId = newOrderId,
                ServiceType = serviceType, // Sẽ là CourseSale
                CourseId = itemId,         // Chỉ gán CourseId
                Amount = itemPrice,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            order.OrderDetails.Add(orderDetail);

            // Đặt giỏ hàng về trạng thái Inactive sau khi tạo đơn hàng
            cart.Status = CartStatus.Pending;
            cart.UpdatedAt = DateTime.UtcNow;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var orderResponse = new OrderResponse
            {
                OrderId = order.Id,
                UserId = order.UserId,
                UserName = $"{user.LastName} {user.FirstName}".Trim(),
                TotalAmount = order.TotalAmount,
                OrderDate = order.OrderDate,
                OrderStatus = order.Status,
                OrderDetails = order.OrderDetails.Select(od => new OrderDetailResponse
                {
                    OrderDetailId = od.Id,
                    CourseId = od.CourseId,
                    CourseName = itemName, // Sử dụng itemName đã tính toán từ giỏ hàng (tên khóa học)
                    Amount = od.Amount
                }).ToList()
            };

            return new OkObjectResult(new BaseResponse { Success = true, Message = "Đơn hàng khóa học đã được tạo thành công từ giỏ hàng.", Data = orderResponse });
        }

        public async Task<IActionResult> GetOrderByIdAsync(Guid orderId)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Course) 
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

            if (order == null)
            {
                return new NotFoundObjectResult(new BaseResponse { Success = false, Message = "Không tìm thấy đơn hàng." });
            }

            // Admin có thể xem tất cả, người dùng chỉ xem đơn hàng của mình
            if (currentUserRole != Role.Admin.ToString() && order.UserId != currentUserId)
            {
                return new UnauthorizedObjectResult(new BaseResponse { Success = false, Message = "Bạn không có quyền truy cập đơn hàng này." });
            }

            var orderResponse = new OrderResponse
            {
                OrderId = order.Id,
                UserId = order.UserId,
                UserName = $"{order.User.LastName} {order.User.FirstName}".Trim(),
                TotalAmount = order.TotalAmount,
                OrderDate = order.OrderDate,
                OrderStatus = order.Status,
                PaymentStatus = order.Payment?.Status,
                PaymentId = order.Payment?.Id,
                OrderDetails = order.OrderDetails.Select(od => new OrderDetailResponse
                {
                    OrderDetailId = od.Id,
                    CourseId = od.CourseId,
                    CourseName = od.Course?.Name,
                    Amount = od.Amount
                }).ToList()
            };

            return new OkObjectResult(new BaseResponse { Success = true, Data = orderResponse });
        }

        public async Task<IActionResult> GetUserOrdersAsync(Guid userId)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            // Đảm bảo người dùng chỉ có thể lấy order của chính họ trừ khi là Admin
            if (currentUserRole != Role.Admin.ToString() && userId != currentUserId)
            {
                return new UnauthorizedObjectResult(new BaseResponse { Success = false, Message = "Bạn không có quyền truy cập đơn hàng của người dùng này." });
            }

            var orders = await _context.Orders
                .Where(o => o.UserId == userId && !o.IsDeleted)
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Course) 
                .Include(o => o.Payment)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderResponse
                {
                    OrderId = o.Id,
                    UserId = o.UserId,
                    UserName = $"{o.User.LastName} {o.User.FirstName}".Trim(),
                    TotalAmount = o.TotalAmount,
                    OrderDate = o.OrderDate,
                    OrderStatus = o.Status,
                    PaymentStatus = (PaymentStatus)(o.Payment != null ? o.Payment.Status : (PaymentStatus?)null),
                    PaymentId = o.Payment.Id,
                    OrderDetails = o.OrderDetails.Select(od => new OrderDetailResponse
                    {
                        OrderDetailId = od.Id,
                        CourseId = od.CourseId,
                        CourseName = od.Course != null ? od.Course.Name : null,
                        Amount = od.Amount
                    }).ToList()
                })
                .ToListAsync();

            if (!orders.Any())
            {
                return new NotFoundObjectResult(new BaseResponse { Success = false, Message = "Không tìm thấy đơn hàng nào cho người dùng này." });
            }

            return new OkObjectResult(new BaseResponse { Success = true, Data = orders });
        }

        public async Task<IActionResult> GetAllOrdersAsync()
        {
            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole != Role.Admin.ToString())
            {
                return new UnauthorizedObjectResult(new BaseResponse { Success = false, Message = "Bạn không có quyền truy cập tất cả đơn hàng." });
            }

            var orders = await _context.Orders
                .Where(o => !o.IsDeleted)
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Course)
                .Include(o => o.Payment)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderResponse
                {
                    OrderId = o.Id,
                    UserId = o.UserId,
                    UserName = $"{o.User.LastName} {o.User.FirstName}".Trim(),
                    TotalAmount = o.TotalAmount,
                    OrderDate = o.OrderDate,
                    OrderStatus = o.Status,
                    //PaymentStatus = o.Payment.Status,
                    PaymentStatus = (PaymentStatus)(o.Payment != null ? o.Payment.Status : (PaymentStatus?)null),
                    PaymentId = o.Payment != null ? o.Payment.Id : null,
                    OrderDetails = o.OrderDetails.Select(od => new OrderDetailResponse
                    {
                        OrderDetailId = od.Id,
                        CourseId = od.CourseId,
                        CourseName = od.Course.Name,
                        Amount = od.Amount
                    }).ToList()
                })
                .ToListAsync();

            if (!orders.Any())
            {
                return new NotFoundObjectResult(new BaseResponse { Success = false, Message = "Không tìm thấy đơn hàng nào." });
            }

            return new OkObjectResult(new BaseResponse { Success = true, Data = orders });
        }

        public async Task<IActionResult> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus)
        {
            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole != Role.Admin.ToString()) // Chỉ admin mới được cập nhật trạng thái Order
            {
                return new UnauthorizedObjectResult(new BaseResponse { Success = false, Message = "Bạn không có quyền cập nhật trạng thái đơn hàng." });
            }

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);
            if (order == null)
            {
                return new NotFoundObjectResult(new BaseResponse { Success = false, Message = "Không tìm thấy đơn hàng." });
            }

            // Logic kiểm tra chuyển trạng thái hợp lệ
            if (order.Status == OrderStatus.Paid)
            {
                return new BadRequestObjectResult(new BaseResponse { Success = false, Message = $"Đơn hàng đã ở trạng thái '{order.Status}', không thể cập nhật." });
            }

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new OkObjectResult(new BaseResponse { Success = true, Message = $"Trạng thái đơn hàng '{order.Id}' đã được cập nhật thành '{newStatus}'." });
        }

    }
}
