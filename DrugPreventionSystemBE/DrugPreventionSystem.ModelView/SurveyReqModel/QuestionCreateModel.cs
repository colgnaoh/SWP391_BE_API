namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyReqModel
{
    public class QuestionCreateModel
    {
        public Guid SurveyId { get; set; }
        public string QuestionContent { get; set; }
        public QuestionType QuestionType { get; set; }
        public int PositionOrder { get; set; }
        //public List<AnswerOptionCreateModel> AnswerOptions { get; set; }
    }
}   