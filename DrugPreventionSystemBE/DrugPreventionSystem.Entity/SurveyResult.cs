using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Enity
{
    public class SurveyResult : BaseEnity
    {
        public Guid UserId { get; set; }
        public int SurveyId { get; set; }
        public int TotalScore { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public DateTime CompletedAt { get; set; }
        public Guid ProgramId { get; set; }

        public ICollection<UserAnswerLog> UserAnswerLogs { get; set; }
    }
}
