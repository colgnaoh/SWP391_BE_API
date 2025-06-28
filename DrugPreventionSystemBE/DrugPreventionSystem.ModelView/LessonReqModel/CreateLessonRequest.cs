using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.LessonReqModel
{
    public class CreateLessonRequest
    {
        public string? Name { get; set; }
        public string? Content { get; set; }
        public LessonType? LessonType { get; set; }
        public string? VideoUrl { get; set; }
        public string? ImageUrl { get; set; }
        public int? FullTime { get; set; }
        public int? PositionOrder { get; set; }
        public Guid? SessionId { get; set; }
        public Guid? CourseId { get; set; }
    }

}
