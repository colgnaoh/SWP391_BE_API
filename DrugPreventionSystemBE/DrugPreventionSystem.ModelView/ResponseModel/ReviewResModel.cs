namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class ReviewResModel
    {
        public Guid Id { get; set; }
        public Guid? CourseId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? AppointmentId { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
