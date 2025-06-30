namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.BookingReqModel
{
    public class BookingDirectRequest
    {
        public Guid ConsultantUserId { get; set; }
        public DateTime AppointmentTime { get; set; }
        public string Note { get; set; }
    }
}
