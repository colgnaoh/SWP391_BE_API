namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class GetBlogsByPageResponse
    {
        public bool Success { get; set; }
        public List<BlogResponseModel> Data { get; set; }
    }
}
