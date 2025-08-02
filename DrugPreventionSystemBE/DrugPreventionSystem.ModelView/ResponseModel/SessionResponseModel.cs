namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class SessionResponseModel
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }
        public string Name { get; set; }
        public string? PositionOrder { get; set; }
        public string Slug { get; set; }
        public string? Content { get; set; }
        public List<LessonResponseModel>? LessonList { get; set; }
    }
}
