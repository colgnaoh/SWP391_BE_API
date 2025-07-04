namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyResModel
{
    public class GetQuestionsByPageResponse
    {
        public bool Success { get; set; }
        public List<QuestionResponseModel> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
