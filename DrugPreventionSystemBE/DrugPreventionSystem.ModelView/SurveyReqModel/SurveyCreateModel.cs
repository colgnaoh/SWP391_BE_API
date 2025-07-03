using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyReqModel
{
    public class SurveyCreateModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public SurveyType SurveyType { get; set; } // enum
        public List<QuestionCreateModel> Questions { get; set; }
    }

    public class QuestionCreateModel
    {
        public string QuestionContent { get; set; }
        public QuestionType QuestionType { get; set; }
        public int PositionOrder { get; set; }
        public List<AnswerOptionCreateModel> AnswerOptions { get; set; }
    }

    public class AnswerOptionCreateModel
    {
        public string OptionContent { get; set; }
        public int Score { get; set; }
        public int PositionOrder { get; set; }
    }

}
