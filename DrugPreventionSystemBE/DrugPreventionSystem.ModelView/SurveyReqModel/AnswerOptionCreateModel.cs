namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyReqModel
{
    public class AnswerOptionCreateModel
    {
        public Guid QuestionId { get; set; }
        public string OptionContent { get; set; }
        public int Score { get; set; }
        public int PositionOrder { get; set; }
    }
}
