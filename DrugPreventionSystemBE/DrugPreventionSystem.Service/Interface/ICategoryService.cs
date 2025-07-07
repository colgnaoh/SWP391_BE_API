using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.BlogReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CategoryReqModel;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface ICategoryService
    {
        Task<IActionResult> CreateCategoryAsync(CreateCategoryRequest request);
        Task<IActionResult> ListCategories(string? filterByName = null);
        Task<IActionResult> SoftDeleteCategoryAsync(Guid CategoryId);
        Task<IActionResult> GetCategoryByIdAsync(Guid categoryId);
        Task<IActionResult> UpdateCategoryAsync(Guid categoryId, CreateCategoryRequest request);
    }
}
