using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class CourseResponseModel
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public Guid? UserId { get; set; }
        public Guid? CategoryId { get; set; }
        public string? Content { get; set; }
        public CourseStatus? Status { get; set; }
        public CourseTargetAudience? TargetAudience { get; set; }
        public List<string> ImageUrls { get; set; }
        public List<string> VideoUrls { get; set; }
        public decimal? Price { get; set; } 
        public decimal? Discount { get; set; }
        public string? Slug { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool IsInCart { get; set; }
        public bool IsPurchased { get; set; }
        public List<SessionResponseModel>? SessionList { get; set; }

    }
}
