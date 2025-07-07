namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class CartListResponse
    {
        public bool Success { get; set; }
        public List<CartItemResponse> Data { get; set; }
    }
}
