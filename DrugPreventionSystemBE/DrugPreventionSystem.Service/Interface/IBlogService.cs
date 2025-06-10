using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.AuthModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.BlogReqModel;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface IBlogService
    {
        Task<IActionResult> GetBlogsByPageAsync(int pageNumber, int pageSize, string? filterByContent);
        Task<IActionResult> CreateBlogAsync(CreateBlogRequest request);
    }
}
