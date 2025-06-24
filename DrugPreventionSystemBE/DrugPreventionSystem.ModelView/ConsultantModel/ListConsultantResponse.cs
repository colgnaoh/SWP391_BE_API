using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ConsultantModel
{
    public class ListConsultantResponse
    {
        public bool? Success { get; set; }
        public List<ConsultantResponseModel> Data { get; set; }
    }
}
