using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CourseReqModel;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface ICartService
    {
        public Guid GetCurrentUserId();
        Task<IActionResult> AddCourseToCartAsync(Guid userId, AddToCartRequest request);
        Task<IActionResult> GetUserCartAsync(Guid userId);
        Task<IActionResult> RemoveCartItemAsync(Guid userId, Guid cartItemId);
        Task<IActionResult> ClearUserCartAsync(Guid userId);
    }
}
