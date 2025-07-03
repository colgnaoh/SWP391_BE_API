using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyResModel
{
    public class SurveyDetailModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SurveyType? Type { get; set; }
        public List<QuestionModel> Questions { get; set; }
    }

    public class QuestionModel
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public QuestionType? QuestionType { get; set; }
        public int? PositionOrder { get; set; }
        public List<AnswerOptionModel> Options { get; set; }
    }

    public class AnswerOptionModel
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public int? Score { get; set; }
        public int? PositionOrder { get; set; }
    }
}
