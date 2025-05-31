using DrugPreventionSystemBE.DrugPreventionSystem.Core;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Enity
{
    public class Course : BaseEnity
    {
        public string Name { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public string Content { get; set; }
        public string Status { get; set; }
        public string TargetAudience { get; set; }
        public string VideoUrl { get; set; }
        public int ImageId { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
    }
}
