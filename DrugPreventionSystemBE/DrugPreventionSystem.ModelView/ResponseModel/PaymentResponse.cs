using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class PaymentResponse
    {
        public Guid PaymentId { get; set; }
        public string PaymentNo { get; set; }
        public Guid? UserId { get; set; }
        public object UserName { get; set; }
        public Guid? OrderId { get; set; }
        public decimal? Amount { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public PaymentStatus? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ExternalTransactionId { get; set; } 
        public string? StripeCheckoutUrl { get; set; }
    }
}
