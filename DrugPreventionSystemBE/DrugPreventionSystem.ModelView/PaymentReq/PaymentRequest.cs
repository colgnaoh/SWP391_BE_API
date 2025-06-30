using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.PaymentReq
{
    public class PaymentRequest
    {
        public Guid OrderId { get; set; }
        public PaymentMethod PaymentMethod { get; set; } 
    }
}
