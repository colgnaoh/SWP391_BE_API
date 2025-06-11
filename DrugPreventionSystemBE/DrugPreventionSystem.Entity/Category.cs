using DrugPreventionSystemBE.DrugPreventionSystem.Core;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Enity
{
    public class Category : BaseEnity
    {
        public string? Name { get; set; }
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
