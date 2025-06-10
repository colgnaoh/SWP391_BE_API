using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.BlogReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CategoryReqModel;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface ICategoryService
    {
        Task<IActionResult> CreateCategoryAsync(CreateCategoryRequest request);
    }
}
