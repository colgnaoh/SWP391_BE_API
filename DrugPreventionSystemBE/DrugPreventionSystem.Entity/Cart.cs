using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Entity
{
    public class Cart : BaseEnity
    {
        public Guid? UserId { get; set; }
        public User User { get; set; } // Assuming a User model exists
        public Guid? CourseId { get; set; }
        public Course Course { get; set; } // Assuming a Course model exists
        public String CartNo { get; set; }
        public CartStatus Status { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
