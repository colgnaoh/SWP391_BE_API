using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.PaymentReq
{
    public class PaymentRequest
    {
        public Guid OrderId { get; set; }
        public paymentMethod PaymentMethod { get; set; } 
    }
}
