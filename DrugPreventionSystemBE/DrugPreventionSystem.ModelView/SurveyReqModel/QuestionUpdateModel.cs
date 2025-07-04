namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyReqModel
{
    public class QuestionUpdateModel
    {
        public string QuestionContent { get; set; } = string.Empty;
        public QuestionType QuestionType { get; set; }
        public int PositionOrder { get; set; }
    }

}
