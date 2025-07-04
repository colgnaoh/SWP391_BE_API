using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyResModel;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface IAnswerOptionService
    {
        Task<IActionResult> CreateAnswerOptionAsync(AnswerOptionCreateModel model);
        Task<IActionResult> UpdateAnswerOptionAsync(Guid id, AnswerOptionUpdateModel model);
        Task<IActionResult> DeleteAnswerOptionAsync(Guid id);
        Task<List<AnswerOptionResponseModel>> GetAnswerOptionsByQuestionIdAsync(Guid questionId);
        Task<IActionResult> GetAnswerOptionsByPageAsync(Guid? questionId, int pageNumber, int pageSize, string? filter, int? filterByScore);
    }

}
