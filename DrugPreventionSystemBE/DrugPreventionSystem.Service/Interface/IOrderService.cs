using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface IOrderService
    {
        Task<IActionResult> CreateOrderFromCartAsync();

        Task<IActionResult> GetOrderByIdAsync(Guid orderId);

        Task<IActionResult> GetUserOrdersAsync(Guid userId);

        Task<IActionResult> GetAllOrdersAsync();

        Task<IActionResult> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus);
    }
}
