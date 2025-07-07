namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ReviewReqModel
{
    public class CreateReviewReqModel
    {
        public Guid CourseId { get; set; }
        public Guid UserId { get; set; }
        public Guid AppointmentId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}
