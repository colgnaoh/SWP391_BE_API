namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ReviewReqModel
{
    public class CreateAppointmentReviewReqModel
    {
        public Guid AppointmentId { get; set; }
        public Guid UserId { get; set; } // Lấy từ context nếu cần
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }

}
