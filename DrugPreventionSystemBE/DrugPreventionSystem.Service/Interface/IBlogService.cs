using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.AuthModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.BlogReqModel;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface IBlogService
    {
        Task<IActionResult> GetBlogsByPageAsync(int pageNumber, int pageSize, string? filterByContent);
        Task<IActionResult> CreateBlogAsync(CreateBlogRequest request);
        Task<IActionResult> GetBlogByIdAsync(Guid blogId);
        Task<IActionResult> GetBlogsByUserAsync(Guid userId, int pageNumber = 1, int pageSize = 12);
        Task<IActionResult> UpdateBlogAsync(Guid blogId, UpdateBlogRequest request);
        Task<IActionResult> SoftDeleteBlogAsync(Guid blogId);
    }
}
