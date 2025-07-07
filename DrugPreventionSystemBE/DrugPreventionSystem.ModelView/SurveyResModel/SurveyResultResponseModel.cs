using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyResModel
{
    public class SurveyResultResponseModel
    {
        public int TotalScore { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public Guid? ProgramId { get; set; }
    }
}
