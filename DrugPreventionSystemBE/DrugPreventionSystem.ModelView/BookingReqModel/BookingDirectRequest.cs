namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.BookingReqModel
{
    public class BookingDirectRequest
    {
        //public Guid ConsultantId { get; set; }
        public DateTime AppointmentTime { get; set; }
        public string Note { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}