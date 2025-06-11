using DrugPreventionSystemBE.DrugPreventionSystem.Core;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Enity
{
    public class Session : BaseEnity
    {
        public Guid CourseId { get; set; }
        public String Name { get; set; }
        public Guid UserId { get; set; }
        public String Slug { get; set; }
        public String Content { get; set; }
        public int PositionOrder { get; set; }
        public Course Course { get; set; }
        public ICollection<Lesson> Lessons { get; set; }
    }
}
