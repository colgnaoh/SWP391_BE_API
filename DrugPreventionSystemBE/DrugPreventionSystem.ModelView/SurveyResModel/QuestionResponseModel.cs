namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyResModel
{
    public class QuestionResponseModel
    {
        public Guid Id { get; set; }
        public Guid? SurveyId { get; set; }
        public string QuestionContent { get; set; }
        public QuestionType? QuestionType { get; set; }
        public int? PositionOrder { get; set; }
    }
}
