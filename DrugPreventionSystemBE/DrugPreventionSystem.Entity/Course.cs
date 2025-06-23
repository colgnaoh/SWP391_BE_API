using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Entity
{
    public class Course : BaseEnity
    {
        public string? Name { get; set; }
        public Guid? UserId { get; set; }
        public User? User { get; set; } // Assuming a User model exists
        public Guid? CategoryId { get; set; }
        public Category? Category { get; set; } // Assuming a Category model exists
        public string? Content { get; set; }
        public CourseStatus? Status { get; set; }
        public targetAudience? TargetAudience { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? Price { get; set; }
        public decimal? Discount { get; set; }
        public string? Slug { get; set; }

        public ICollection<Session> Sessions { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Cart> Carts { get; set; }
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}
