using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ApiResponse;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controller
{
    [ApiController]
    [Route("api/order")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [Authorize] // Chỉ học viên mới tạo Order
        [HttpPost("from-cart")]
        public async Task<IActionResult> CreateOrderFromCart()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized(new BaseResponse { Success = false, Message = "Không tìm thấy User ID trong token." });
            }

            var result = await _orderService.CreateOrderFromCartAsync(userId);
            return result;
        }

        [Authorize]
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            var result = await _orderService.GetOrderByIdAsync(orderId);
            return result;
        }

        [Authorize]
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized(new BaseResponse { Success = false, Message = "Không tìm thấy User ID trong token." });
            }

            var result = await _orderService.GetUserOrdersAsync(userId);
            return result;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllOrders()
        {
            var result = await _orderService.GetAllOrdersAsync();
            return result;
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("status/{orderId}/{newStatus}")]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, OrderStatus newStatus)
        {
            var result = await _orderService.UpdateOrderStatusAsync(orderId, newStatus);
            return result;
        }
    }
}
