using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class CartItemResponse
    {
        public Guid CartId { get; set; }
        public Guid CourseId { get; set; }
        public string CourseName { get; set; }
        public string CourseImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public CartStatus Status { get; set; } // Ví dụ: Active, Pending, Purchased
        public DateTime CreatedAt { get; set; }
    }
}
