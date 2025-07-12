using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyResModel
{
    public class SurveyResultDetailResponseModel
    {
        public Guid? SurveyResultId { get; set; }
        public Guid? SurveyId { get; set; }
        public Guid? UserId { get; set; }
        public int? TotalScore { get; set; }
        public RiskLevel? RiskLevel { get; set; }
        public DateTime CompletedAt { get; set; }
        public List<UserAnswerDetail> Answers { get; set; }
    }

    public class UserAnswerDetail
    {
        public Guid? QuestionId { get; set; }
        public Guid? AnswerOptionId { get; set; }
        public string? QuestionContent { get; set; }
        public string? AnswerOptionContent { get; set; }
        public int? Score { get; set; }
    }

}
