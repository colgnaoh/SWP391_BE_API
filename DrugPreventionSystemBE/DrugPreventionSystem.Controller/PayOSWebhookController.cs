using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.PayOS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.Extensions.Logging;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controller
{
    [ApiController]
    [Route("api/payos/webhook")]
    public class PayOSWebhookController : ControllerBase
    {
        private readonly DrugPreventionDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IPayOSSignatureService _payOSSignatureService;
        private readonly ILogger<PayOSWebhookController> _logger;

        public PayOSWebhookController(
            DrugPreventionDbContext context,
            IConfiguration configuration,
            IPayOSSignatureService payOSSignatureService,
            ILogger<PayOSWebhookController> logger)
        {
            _context = context;
            _configuration = configuration;
            _payOSSignatureService = payOSSignatureService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Webhook([FromBody] PayOSWebhookPayload payload)
        {
            string rawBody;
            using (var reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8))
            {
                Request.Body.Seek(0, SeekOrigin.Begin);
                rawBody = await reader.ReadToEndAsync();
                Request.Body.Seek(0, SeekOrigin.Begin);
            }

            if (payload == null || payload.data == null)
            {
                _logger.LogWarning("Invalid webhook payload structure received.");
                return BadRequest();
            }

            var checksumKey = _configuration["PayOS:ChecksumKey"];
            if (string.IsNullOrEmpty(checksumKey))
            {
                _logger.LogError("Webhook checksum key not configured.");
                return StatusCode(500);
            }

            if (!_payOSSignatureService.VerifyPayOSSignature(rawBody, payload.signature, checksumKey))
            {
                _logger.LogWarning("Webhook signature mismatch for orderCode: {OrderCode}", payload.data.orderCode);
                return Unauthorized();
            }

            long orderCode = payload.data.orderCode;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentNo == orderCode.ToString() && !p.IsDeleted);

                if (payment == null)
                {
                    _logger.LogInformation("Payment not found for orderCode: {OrderCode}, ignoring webhook.", orderCode);
                    await transaction.CommitAsync();
                    return Ok();
                }

                if (payment.Status == PaymentStatus.Success)
                {
                    _logger.LogInformation("Payment for orderCode: {OrderCode} already success, ignoring webhook.", orderCode);
                    await transaction.CommitAsync();
                    return Ok();
                }

                bool statusChanged = false;
                if (payload.data.status == 1)
                {
                    if (payment.Status != PaymentStatus.Success)
                    {
                        payment.Status = PaymentStatus.Success;
                        statusChanged = true;
                    }
                }
                else if (payload.data.status == -1)
                {
                    if (payment.Status != PaymentStatus.Failed)
                    {
                        payment.Status = PaymentStatus.Failed;
                        statusChanged = true;
                    }
                }
                else if (payload.data.status == 0)
                {
                    _logger.LogInformation("Webhook received, payment for orderCode: {OrderCode} is still pending on PayOS.", orderCode);
                    await transaction.CommitAsync();
                    return Ok();
                }
                else
                {
                    _logger.LogWarning("Unknown PayOS status '{PayOSStatus}' for orderCode: {OrderCode}", payload.data.status, orderCode);
                    await transaction.CommitAsync();
                    return Ok();
                }

                payment.UpdatedAt = DateTime.UtcNow;

                if (payment.OrderId.HasValue && statusChanged)
                {
                    var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == payment.OrderId);
                    if (order != null)
                    {
                        if (payment.Status == PaymentStatus.Success && order.Status != OrderStatus.Paid)
                        {
                            order.Status = OrderStatus.Paid;
                            order.UpdatedAt = DateTime.UtcNow;
                            _logger.LogInformation("Order {OrderId} status updated to Paid.", order.Id);
                        }
                        else if (payment.Status == PaymentStatus.Failed && order.Status != OrderStatus.Failed)
                        {
                            order.Status = OrderStatus.Failed;
                            order.UpdatedAt = DateTime.UtcNow;
                            _logger.LogWarning("Order {OrderId} status updated to Failed.", order.Id);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Associated order not found for payment {PaymentId} (orderId: {OrderId}).", payment.Id, payment.OrderId.Value);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("Successfully processed webhook for orderCode: {OrderCode} with status: {Status}", orderCode, payment.Status);
                return Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing PayOS webhook for orderCode: {OrderCode}", payload.data.orderCode);
                return StatusCode(500);
            }
        }
    }
}