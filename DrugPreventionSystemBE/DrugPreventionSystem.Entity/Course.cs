using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

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
        public RiskLevel RiskLevel { get; set; }
        public CourseStatus? Status { get; set; }
        
        public targetAudience? TargetAudience { get; set; }
        public string ImageUrlsJson { get; set; }  // Cột lưu JSON trong DB
        public string VideoUrlsJson { get; set; }  // Cột lưu JSON trong DB
        public decimal? Price { get; set; }
        public decimal? Discount { get; set; }
        public string? Slug { get; set; }

        public ICollection<Session> Sessions { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Cart> Carts { get; set; }
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        [NotMapped]
        public List<string> ImageUrls
        {
            get => string.IsNullOrEmpty(ImageUrlsJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(ImageUrlsJson);
            set => ImageUrlsJson = JsonSerializer.Serialize(value ?? new List<string>());
        }

        [NotMapped]
        public List<string> VideoUrls
        {
            get => string.IsNullOrEmpty(VideoUrlsJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(VideoUrlsJson);
            set => VideoUrlsJson = JsonSerializer.Serialize(value ?? new List<string>());
        }
    }
}
