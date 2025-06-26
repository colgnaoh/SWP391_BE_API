namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class SessionSingleResponse
    {
        public bool Success { get; set; }
        public SessionViewModel? Data { get; set; }
        //public string? Message { get; set; } // optional
    }

}
