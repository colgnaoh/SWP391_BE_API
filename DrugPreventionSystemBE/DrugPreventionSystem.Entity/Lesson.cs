using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using System.ComponentModel.DataAnnotations;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Enity
{
    public class Lesson : BaseEnity
    {
        [Key]
        public int SessionId { get; set; }
        [Key]
        public int CourseId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public LessonType LessonType { get; set; }
        public string VideoUrl { get; set; }
        public string ImageUrl { get; set; }
        public int FullTime { get; set; }
        public int PositionOrder { get; set; }

        public Session Session { get; set; }
        public Course Course { get; set; }
    }
}
