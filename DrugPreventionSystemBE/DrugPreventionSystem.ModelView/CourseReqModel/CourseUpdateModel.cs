using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CourseReqModel;
using System.ComponentModel.DataAnnotations;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CourseReqModel
{
    public class CourseUpdateModel
    {
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public Guid CategoryId { get; set; }
        public string? Content { get; set; }
        public CourseStatus Status { get; set; }
        public CourseTargetAudience TargetAudience { get; set; }
        public RiskLevel? RiskLevel { get; set; }
        public List<string> ImageUrls { get; set; }
        public List<string> VideoUrls { get; set; }
        public decimal? Price { get; set; }
        public decimal? Discount { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Slug { get; set; }
    }
}
