using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.PaymentReq;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface IPaymentService
    {
        Task<IActionResult> ProcessPaymentAsync(Guid userId, PaymentRequest request);
    }
}
