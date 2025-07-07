namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyReqModel
{
    public class AnswerRequestModel
    {
        public Guid QuestionId { get; set; }
        public Guid? AnswerOptionId { get; set; } // Optional nếu là câu hỏi tự luận
    }
}
