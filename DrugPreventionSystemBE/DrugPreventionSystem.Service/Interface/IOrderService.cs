using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CourseReqModel;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface IOrderService
    {
        Task<IActionResult> CreateOrderFromCartAsync(CreateOrderFromCartReq request);

        Task<IActionResult> GetOrderByIdAsync(Guid orderId);

        Task<IActionResult> GetUserOrdersAsync(Guid userId);

        Task<IActionResult> GetAllOrdersAsync();

        Task<IActionResult> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus);
    }
}
