using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ConsultantModel;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class AppointmentResponseModel
    {
        public Guid Id { get; set; }
        public DateTime? AppointmentTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public string? Note { get; set; }
        public string? Name { get; set; }

        public ConsultantResponseModel? Consultant { get; set; }
    }
}
