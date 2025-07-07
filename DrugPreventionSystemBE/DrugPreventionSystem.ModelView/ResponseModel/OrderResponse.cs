using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ApiResponse;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class OrderResponse
    {
        public Guid? OrderId { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? OrderDate { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public Guid? PaymentId { get; set; }
        public OrderStatus? OrderStatus { get; set; }
        public List<OrderDetailResponse> OrderDetails { get; set; } = new List<OrderDetailResponse>();
    }
    public class OrderDetailResponse
    {
        public Guid? OrderDetailId { get; set; }
        public Guid? CourseId { get; set; }
        public string? CourseName { get; set; }
        public decimal? Amount { get; set; }
    }

    public class OrderListResponse : BaseResponse
    {
        public List<OrderResponse> Data { get; set; }
    }

}
