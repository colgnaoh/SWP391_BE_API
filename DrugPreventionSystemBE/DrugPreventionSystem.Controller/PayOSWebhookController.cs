using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controller
{
    [ApiController]
    [Route("api/payos/webhook")]
    public class PayOSWebhookController : ControllerBase
    {
        private readonly DrugPreventionDbContext _context;

        public PayOSWebhookController(DrugPreventionDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Webhook([FromBody] dynamic payload)
        {
            string orderCode = payload?.orderCode;
            if (string.IsNullOrEmpty(orderCode))
                return BadRequest();

            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentNo == orderCode && !p.IsDeleted);
            if (payment == null || payment.Status == PaymentStatus.Success)
                return Ok(); // tránh xử lý lại

            payment.Status = PaymentStatus.Success;
            payment.UpdatedAt = DateTime.UtcNow;

            if (payment.OrderId.HasValue)
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == payment.OrderId);
                if (order != null)
                {
                    order.Status = OrderStatus.Paid;
                    order.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }

}
