namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyReqModel
{
    public class SurveySubmitRequestModel
    {
        public Guid UserId { get; set; }
        public Guid SurveyId { get; set; }
        public Guid ProgamId { get; set; }
        public List<AnswerRequestModel> Answers { get; set; }
    }


}
