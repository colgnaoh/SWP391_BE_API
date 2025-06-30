using DrugPreventionSystemBE.DrugPreventionSystem.Entity;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class SessionViewModel
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }
        public string? Name { get; set; }
        public Guid? UserId { get; set; }
        public string? Slug { get; set; }
        public string? Content { get; set; }
        public string? PositionOrder { get; set; }
        public Course Course { get; set; } 
    }

}
