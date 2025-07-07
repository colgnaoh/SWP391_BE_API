using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ApiResponse;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.PaymentReq;
using DrugPreventionSystemBE.DrugPreventionSystem.Service;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controller
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [Authorize]
        [HttpPost("createPaymentFromOrder")]
        public async Task<IActionResult>CreatePaymentFromOrder([FromBody]CreatePaymentRequest request)
        {
            var result = await _paymentService.CreatePaymentForOrderAsync(request);
            return result;
        }

        [Authorize]
        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetPaymentHistoryByUserId(Guid userId)
        {
            var result = await _paymentService.GetPaymentHistoryByUserIdAsync(userId);
            return result;
        }

        [Authorize]
        [HttpGet("{paymentId}")]
        public async Task<IActionResult> GetPaymentByIdAsync(Guid paymentId)
        {
            var result = await _paymentService.GetPaymentByIdAsync(paymentId);
            return result;
        }

    }
}
