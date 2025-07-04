namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyResModel
{
    public class AnswerOptionResponseModel
    {
        public Guid Id { get; set; }
        public Guid? QuestionId { get; set; }
        public string OptionContent { get; set; }
        public int? Score { get; set; }
        public int? PositionOrder { get; set; }
    }
}
