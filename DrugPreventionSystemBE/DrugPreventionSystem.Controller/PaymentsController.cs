using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ApiResponse;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.PaymentReq;
using DrugPreventionSystemBE.DrugPreventionSystem.Service;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Security.Claims;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controller
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;

        public PaymentsController(IPaymentService paymentService, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _configuration = configuration;
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
        [HttpPost("stripe-webhook")] 
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeWebhookSecret = _configuration["Stripe:WebhookSecret"];
                var stripeSignature = Request.Headers["Stripe-Signature"];

                Stripe.Event stripeEvent = EventUtility.ConstructEvent(
                    json,
                    stripeSignature,
                    stripeWebhookSecret
                );

                Console.WriteLine($"Received Stripe event: {stripeEvent.Type}");

                return await _paymentService.HandleStripeWebhookAsync(json);
            }
            catch (StripeException e)
            {
                Console.WriteLine($"Stripe Webhook Error: {e.Message}");
                return BadRequest(new BaseResponse { Success = false, Message = $"Webhook Error: {e.Message}" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal Webhook Error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  new BaseResponse { Success = false, Message = $"Internal Server Error: {ex.Message}" });
            }
        }

    }
}
