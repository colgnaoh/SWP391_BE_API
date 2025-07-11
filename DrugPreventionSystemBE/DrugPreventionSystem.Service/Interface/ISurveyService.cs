using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyResModel;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface ISurveyService
    {
        Task<IActionResult> CreateSurveyAsync(SurveyCreateModel model);
        Task<IActionResult> UpdateSurveyAsync(Guid id, SurveyUpdateModel model);
        Task<IActionResult> DeleteSurveyAsync(Guid id);
        Task<SurveyDetailModel?> GetSurveyDetailAsync(Guid id);
        Task<SurveyResultResponseModel> SubmitSurveyAsync(SurveySubmitRequestModel model);
        Task<IActionResult> GetSurveysByPageWithStatusAsync(
                Guid userId,
                string? role,
                int pageNumber,
                int pageSize,
                string? filterByName);
    }
}
