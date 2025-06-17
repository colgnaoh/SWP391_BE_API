using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ReviewReqModel;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface IReviewService
    {
        Task<List<ReviewResModel>> GetAllReviewsAsync();
        Task<ReviewResModel?> GetReviewByIdAsync(Guid id);
        Task<ReviewResModel> CreateReviewAsync(CreateReviewReqModel request);
        Task<bool> UpdateReviewAsync(Guid id, UpdateReviewReqModel request);
        Task<bool> DeleteReviewAsync(Guid id);
    }
}
