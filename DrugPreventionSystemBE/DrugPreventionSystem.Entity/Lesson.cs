using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using System.ComponentModel.DataAnnotations;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Entity
{
    public class Lesson : BaseEnity
    {
        public Guid? SessionId { get; set; }
        public Session? Session { get; set; }
        public Guid? CourseId { get; set; }
        public Course? Course { get; set; }
        public Guid? UserId { get; set; }
        public User User { get; set; } // Assuming a User model exists
        public string? Name { get; set; }
        public string? Content { get; set; }
        public LessonType? LessonType { get; set; }
        public string? VideoUrl { get; set; }
        public string? ImageUrl { get; set; }
        public int? FullTime { get; set; }
        public int? PositionOrder { get; set; }

        
        
    }
}
