namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.BookingReqModel
{
    public class AssignConsultantRequest
    {
        public Guid AppointmentId { get; set; }
        public Guid ConsultantUserId { get; set; }

    }
}
