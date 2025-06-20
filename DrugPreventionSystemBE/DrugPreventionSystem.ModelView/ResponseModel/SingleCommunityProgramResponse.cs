using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CommunityProgramResModel;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class SingleCommunityProgramResponse
    {
        public bool Success { get; set; }
        public CommunityProgramResponseModel Data { get; set; }
    }
}
