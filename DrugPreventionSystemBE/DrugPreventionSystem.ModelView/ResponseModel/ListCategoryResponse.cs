namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class ListCategoryResponse
    {
        public bool? Success { get; set; }
        public List<CategoryResponseModel> Data { get; set; }
    }
}
