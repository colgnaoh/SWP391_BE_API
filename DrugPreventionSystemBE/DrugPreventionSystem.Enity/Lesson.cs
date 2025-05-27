using DrugPreventionSystemBE.DrugPreventionSystem.Core;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Enity
{
    public class Lesson : BaseEnity
    {
        public int SessionId { get; set; }
        public int CourseId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public string LessonType { get; set; }
        public string VideoUrl { get; set; }
        public string ImageUrl { get; set; }
        public int FullTime { get; set; }
        public int PositionOrder { get; set; }
    }
}
