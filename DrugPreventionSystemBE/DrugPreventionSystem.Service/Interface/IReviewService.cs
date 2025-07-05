using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ReviewReqModel;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface IReviewService
    {
        Task<IActionResult> GetAllReviewsAsync();
        Task<IActionResult> GetReviewByIdAsync(Guid id);
        Task<IActionResult> CreateReviewAsync(CreateReviewReqModel request);
        Task<IActionResult> UpdateReviewAsync(Guid id, UpdateReviewReqModel request);
        Task<IActionResult> DeleteReviewAsync(Guid id);
        Task<IActionResult> GetReviewsByUserIdAsync(Guid userId);
        Task<IActionResult> GetReviewsByCourseIdAsync(Guid courseId);
    }
}
