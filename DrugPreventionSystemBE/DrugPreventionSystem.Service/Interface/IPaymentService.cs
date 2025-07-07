using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ApiResponse;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.PaymentReq;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface IPaymentService
    {
        Task<IActionResult> CreatePaymentForOrderAsync(CreatePaymentRequest request);
        Task<IActionResult> GetPaymentByIdAsync(Guid paymentId);
        Task<IActionResult> GetAllPaymentsAsync();
        Task<IActionResult> GetPaymentsByUserIdAsync(Guid userId);
        Task<IActionResult> UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus newStatus);
        Task<IActionResult> GetPaymentHistoryByUserIdAsync(Guid userId);

    }
}
