using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class LessonResponseModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Content { get; set; }
        public LessonType? LessonType { get; set; }
        public string? VideoUrl { get; set; }
        public string? ImageUrl { get; set; }
        public int? FullTime { get; set; }
        public int? PositionOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? UserId { get; set; }
        public string? FullName { get; set; }
        public string? UserAvatar { get; set; }
        public Guid SessionId { get; set; }
        public Session? Session { get; set; }
        public Guid CourseId { get; set; }
        public Course Course { get; set; }
    }

}
