using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using System.ComponentModel.DataAnnotations;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CourseReqModel
{
    public class CourseCreateModel
    {
        [Required]
        public string? Name { get; set; }
        [Required]
        public Guid CategoryId { get; set; }
        public string? Content { get; set; }
        [Required]
        public CourseStatus Status { get; set; }
        [Required]
        public targetAudience TargetAudience { get; set; }
        public string? VideoUrl { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? Price { get; set; }
        public decimal? Discount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string? CourseImgUrl { get; set; }
        public string? Slug { get; set; }
    }
}
