using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.AuthModel;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface IBlogService
    {
        Task<IActionResult> GetBlogsByPageAsync(int pageNumber, int pageSize);
        Task<IActionResult> CreateBlogAsync();
    }
}
