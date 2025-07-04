namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyResModel
{
    public class SurveyPagedResultModel
    {
        public bool Success { get; set; }
        public List<SurveyResponseModel> Data { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }

}
