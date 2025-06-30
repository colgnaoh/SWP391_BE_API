namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class GetAllSessionResponse
    {
        public bool Success { get; set; }
        public List<SessionViewModel> Data { get; set; }
    }
}
