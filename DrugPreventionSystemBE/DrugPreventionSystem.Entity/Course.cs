using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Enity
{
    public class Course : BaseEnity
    {
        public string? Name { get; set; }
        public Guid UserId { get; set; }
        public Guid CategoryId { get; set; }
        public string? Content { get; set; }
        public CourseStatus Status { get; set; }
        public targetAudience TargetAudience { get; set; }
        public Guid ImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public string slug { get; set; }

        public ICollection<Session> Sessions { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Cart> Carts { get; set; }
    }
}
