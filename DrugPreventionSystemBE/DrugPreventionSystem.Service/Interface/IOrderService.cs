using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface IOrderService
    {
        Task<IActionResult> CreateOrderFromCartAsync(Guid userId);
        Task<IActionResult> GetUserOrdersAsync(Guid userId);
        Task<IActionResult> GetOrderDetailAsync(Guid orderId);
    }
}
