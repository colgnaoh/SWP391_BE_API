using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Entity
{
    public class SurveyResult : BaseEnity
    {
        public Guid? UserId { get; set; }
        public User? User { get; set; } // Assuming a User model exists
        public Guid SurveyId { get; set; }
        public Survey? Survey { get; set; } // Assuming a Survey model exists
        public int? TotalScore { get; set; }
        public RiskLevel? RiskLevel { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Guid? ProgramId { get; set; }
        public CommunityProgram? Program { get; set; } // Assuming a Program model exists

        public ICollection<UserAnswerLog> UserAnswerLogs { get; set; }
    }
}
