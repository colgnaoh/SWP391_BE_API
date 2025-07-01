using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Entity
{
    public class CommunityProgram : BaseEnity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public Programtype? Type { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ProgramImgUrl { get; set; }

        public string? ProgramVidUrl { get; set; }

        public RiskLevel RiskLevel { get; set; }

        public ICollection<SurveyResult> SurveyResults { get; set; } = new List<SurveyResult>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<UserAnswerLog> UserAnswerLogs { get; set; } = new List<UserAnswerLog>();

        public ICollection<Course> Courses { get; set; } = new List<Course>();

    }
}
