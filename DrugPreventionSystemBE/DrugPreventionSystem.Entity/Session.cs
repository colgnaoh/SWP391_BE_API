using DrugPreventionSystemBE.DrugPreventionSystem.Core;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Entity
{
    public class Session : BaseEnity
    {
        public Guid CourseId { get; set; }
        public string? Name { get; set; }
        public Guid? UserId { get; set; }
        public User? User { get; set; } // Assuming a User model exists
        public string? Slug { get; set; }
        public string? Content { get; set; }
        public string? PositionOrder { get; set; }
        public ICollection<Lesson> Lessons { get; set; }
        public Course? Course { get; set; }
    }
}
