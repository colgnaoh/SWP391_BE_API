using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Enity
{
    public class SurveyResult : BaseEnity
    {
        public int UserId { get; set; }
        public int SurveyId { get; set; }
        public int TotalScore { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public DateTime CompletedAt { get; set; }
        public int ProgramId { get; set; }
    }
}
