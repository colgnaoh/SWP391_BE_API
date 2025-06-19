namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class OrderResponse
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } 
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } 
        public List<OrderDetailResponse> Items { get; set; }
    }

    public class OrderDetailResponse
    {
        public Guid OrderDetailId { get; set; }
        public string ServiceType { get; set; } // "Course"
        public string ServiceName { get; set; } // Tên khóa học
        public decimal Amount { get; set; }
    }
}
