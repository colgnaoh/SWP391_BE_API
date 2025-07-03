using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyResModel;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface ISurveyService
    {
        Task<List<Survey>> GetAllSurveysAsync();
        Task<SurveyDetailModel> GetSurveyDetailAsync(Guid surveyId);
        Task<SurveyResultResponseModel> SubmitSurveyAsync(SurveySubmitRequestModel model);
        Task<Guid> CreateSurveyAsync(SurveyCreateModel model);

    }

}
