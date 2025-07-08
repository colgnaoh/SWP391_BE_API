namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyReqModel
{
    public class AnswerOptionUpdateModel
    {
        public Guid Id { get; set; }
        public Guid? QuestionId { get; set; }
        public string OptionContent { get; set; } = string.Empty;
        public int Score { get; set; }
        public int PositionOrder { get; set; }
    }

    public class BulkUpdateAnswerOptionModel
    {
        public List<AnswerOptionUpdateModel> Options { get; set; } = new();
    }

}
