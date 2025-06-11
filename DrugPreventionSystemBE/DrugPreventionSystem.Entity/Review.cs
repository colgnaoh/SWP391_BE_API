using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Enity;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Entity
{
    public class Review : BaseEnity
    {
        public Guid CourseId { get; set; }
        public Guid UserId { get; set; }
        public int Rating { get; set; }
        public String Comment { get; set; }
        public Course Course { get; set; }

    }
}
