using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Entity
{
    public class OrderLog : BaseEnity
    {
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }
        public Guid? CartId { get; set; }
        public Cart? Cart { get; set; }
        public string? Action { get; set; } 
        public string? Note { get; set; } 
        public Guid? CourseId { get; set; }
        public Course? Course { get; set; }
        public Guid? UserId { get; set; }
        public User? User { get; set; } 
    }
}