using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

        public async Task<IActionResult> CreateOrderFromCartAsync(Guid userId)
        {
            var cartItems = await _context.Carts
                .Where(c => c.UserId == userId && !c.IsDeleted && c.Status == Enum.CartStatus.Pending)
                .Include(c => c.Course)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return new BadRequestObjectResult("Giỏ hàng của bạn trống. Không thể tạo đơn hàng.");
            }

            var totalAmount = cartItems.Sum(c => c.Price - c.Discount);

            var newOrder = new Order
            {
                Id = _idServices.GenerateNextId(),
                CartId = cartItems.First().Id, // Lưu ID của một cart item bất kỳ (hoặc có thể tạo một bảng cart header riêng)
                TotalAmount = totalAmount,
                OrderDate = DateTime.UtcNow,
                Status = Enum.OrderStatus.Pending, // Đơn hàng đang chờ thanh toán
            };

            _context.Orders.Add(newOrder);

            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    Id = _idServices.GenerateNextId(),
                    OrderId = newOrder.Id,
                    ServiceType = Enum.ServiceType.CourseSale,
                    Amount = item.Price - item.Discount,
                    // TransactionId sẽ được điền sau khi thanh toán thành công
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                _context.OrderDetails.Add(orderDetail);

                // Đánh dấu các mục trong giỏ hàng là đã được chuyển đổi thành đơn hàng
                item.Status = Enum.CartStatus.Completed;
                item.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new OkObjectResult(new { Message = "Đơn hàng đã được tạo thành công.", OrderId = newOrder.Id });
        }

        public async Task<IActionResult> GetUserOrdersAsync(Guid userId)
        {
            var orders = await _context.Orders
                .Where(o => o.Cart.UserId == userId) // Giả định CartId trong Order liên kết với userId
                .Include(o => o.OrderDetails) // Kéo theo chi tiết đơn hàng
                .ThenInclude(od => od.Transaction) // Kéo theo giao dịch nếu có
                .Select(o => new OrderResponse
                {
                    OrderId = o.Id ?? Guid.Empty, // Explicitly handle nullable Guid
                    TotalAmount = o.TotalAmount,
                    OrderDate = o.OrderDate ?? DateTime.MinValue, // Handle nullable DateTime
                    Status = o.Status.ToString(),
                    Items = o.OrderDetails.Select(od => new OrderDetailResponse
                    {
                        OrderDetailId = od.Id,
                        ServiceType = od.ServiceType.ToString(),
                        ServiceName = od.ServiceType.ToString(), 
                        Amount = od.Amount ?? 0 // Handle nullable decimal
                    }).ToList()
                })
                .ToListAsync();

            if (!orders.Any())
            {
                return new NotFoundObjectResult("Bạn chưa có đơn hàng nào.");
            }

            return new OkObjectResult(orders);
        }

        public async Task<IActionResult> GetOrderDetailAsync(Guid orderId)
        {
            var order = await _context.Orders
                .Where(o => o.Id == orderId)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Transaction)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return new NotFoundObjectResult("Không tìm thấy đơn hàng.");
            }

            var orderResponse = new OrderResponse
            {
                OrderId = order.Id ?? Guid.Empty, // Explicitly handle nullable Guid
                TotalAmount = order.TotalAmount,
                OrderDate = order.OrderDate ?? DateTime.MinValue, // Handle nullable DateTime
                Status = order.Status.ToString(),
                Items = order.OrderDetails.Select(od => new OrderDetailResponse
                {
                    OrderDetailId = od.Id,
                    ServiceType = od.ServiceType.ToString(),
                    ServiceName = od.Transaction?.Course?.Name ?? od.ServiceType.ToString(),
                    Amount = od.Amount ?? 0 // Handle nullable decimal
                }).ToList()
            };

            return new OkObjectResult(orderResponse);
        }
    }
}
