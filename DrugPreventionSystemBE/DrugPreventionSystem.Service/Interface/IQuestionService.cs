using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyResModel;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface IQuestionService
    {
        Task<IActionResult> CreateQuestionAsync(QuestionCreateModel model);
        Task<IActionResult> UpdateQuestionAsync(Guid id, QuestionUpdateModel model);
        Task<IActionResult> DeleteQuestionAsync(Guid id);
        Task<List<QuestionResponseModel>> GetQuestionsBySurveyIdAsync(Guid surveyId);
        Task<IActionResult> GetQuestionsByPageAsync(Guid? surveyId, int pageNumber, int pageSize, string? filter);
    }

}
