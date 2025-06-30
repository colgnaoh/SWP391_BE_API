namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CommunityProgramResModel
{
    public class GetProgramsByPageResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<CommunityProgramResponseModel> Data { get; set; }
        public int? TotalCount { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        public int? TotalPages { get; set; }
    }
}
