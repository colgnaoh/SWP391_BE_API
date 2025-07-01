namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class GetAllSessionResponse
    {
        public bool Success { get; set; }
        public List<SessionViewModel> Data { get; set; }
        public int TotalCount { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }
    }
}
