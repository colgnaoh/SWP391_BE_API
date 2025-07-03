using DrugPreventionSystemBE.DrugPreventionSystem.Helpers;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ApiResponse;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.PaymentReq;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.VnPayPayment;
using DrugPreventionSystemBE.DrugPreventionSystem.Service;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using DrugPreventionSystemBE.DrugPreventionSystem.Services;
using Microsoft.AspNetCore.Authorization;
using System; 
using System.Threading.Tasks;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using DrugPreventionSystemBE.DrugPreventionSystem;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace DrugPreventionSystemBE.DrugPreventionSystem.Controller
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IVnPayService _vnpayService;
        public PaymentsController(IPaymentService paymentService, IVnPayService vnPayService)
        {
            _paymentService = paymentService;
            _vnpayService = vnPayService;
        }

        [Authorize]
        [HttpPost("createPaymentFromOrder")]
        public async Task<IActionResult>CreatePaymentFromOrder([FromBody]CreatePaymentRequest request)
        {
            var result = await _paymentService.CreatePaymentForOrderAsync(request);
            return result;
        }

        [Authorize]
        [HttpGet("{paymentId}")]
        public async Task<IActionResult> GetPaymentByIdAsync(Guid paymentId)
        {
            var result = await _paymentService.GetPaymentByIdAsync(paymentId);
            return result;
        }
        [HttpGet("CreatePaymentUrl")]
        public ActionResult<string> CreatePaymentUrl([FromQuery] double moneyToPay, [FromQuery] string description)
        {
            try
            {
                var ipAddress = NetworkHelper.GetIpAddress(HttpContext);

                long orderCode = DateTime.Now.Ticks; // Replace with your actual unique order ID from DB

                var request = new PaymentRequestVnPay
                {
                    PaymentId = orderCode,
                    Money = moneyToPay,
                    Description = description,
                    IpAddress = ipAddress,
                    // BankCode, CreatedDate, Currency, Language will use defaults from PaymentRequest model
                };

                var paymentUrl = _vnpayService.GetPaymentUrl(request);

                if (string.IsNullOrEmpty(paymentUrl))
                {
                    return BadRequest("Failed to create VNPAY payment URL.");
                }

                return Created(paymentUrl, paymentUrl);
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using ILogger)
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("VnPayReturn")]
        public IActionResult VnPayReturn([FromQuery] VnPayReturnResponse response)
        {
            bool success = _vnpayService.ProcessVnPayReturn(response);

            if (success)
            {
                // Redirect to your frontend success page, or display success message
                return Ok("Payment successful!");
            }
            else
            {
                // Redirect to your frontend failure page, or display error message
                return BadRequest("Payment failed or invalid signature.");
            }
        }

        [HttpPost("VnPayIpn")]
        public ActionResult<VnPayIpnResult> VnPayIpn([FromForm] VnPayIpnResponse response) // VNPAY typically sends IPN as Form data
        {
            var result = _vnpayService.ProcessVnPayIpn(response);
            return Ok(result); // VNPAY expects a JSON response
        }

    }
}
