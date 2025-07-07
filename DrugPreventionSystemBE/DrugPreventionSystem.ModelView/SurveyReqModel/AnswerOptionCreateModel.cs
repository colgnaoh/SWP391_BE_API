namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyReqModel
{
    public class AnswerOptionCreateModel
    {
        public string OptionContent { get; set; }
        public int Score { get; set; }
        public int PositionOrder { get; set; }
    }

    public class MultipleAnswerOptionCreateModel
    {
        public Guid QuestionId { get; set; }
        public List<AnswerOptionCreateModel> Options { get; set; } = new();
    }
}
