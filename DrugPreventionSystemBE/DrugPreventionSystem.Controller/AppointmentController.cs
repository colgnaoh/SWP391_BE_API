using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.BookingReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sprache;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controller
{
    [ApiController]
    [Route("api/appointments")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpPost("book")]
        [Authorize]
        public async Task<IActionResult> BookWithSchedule([FromBody] BookingDirectRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized("Không xác thực được người dùng.");

            var result = await _appointmentService.BookWithScheduleAsync(userId, request);

            return StatusCode(201, result);
        }

        [HttpPut("assign")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> AssignConsultant([FromBody] AssignConsultantRequest request)
        {
            
            return await _appointmentService.AssignConsultantAsync(request);
        }

        //[HttpPut("complete")]
        //[Authorize(Roles = "Consultant")]
        //public async Task<IActionResult> MarkAsCompleted([FromQuery] Guid consultantId, [FromQuery] Guid appointmentId)
        //{
        //    return await _appointmentService.MarkAsCompletedAsync(consultantId, appointmentId);
        //}

        [HttpPut("status")]
        [Authorize(Roles = "Admin,Manager,Consultant")]
        public async Task<IActionResult> ChangeStatus([FromQuery] Guid appointmentId, [FromQuery] AppointmentStatus newStatus)
        {
            return await _appointmentService.ChangeAppointmentStatusAsync(appointmentId, newStatus);
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> GetAppointmentsByFilter(
    [FromQuery] AppointmentStatus? status = null,
    [FromQuery] DateTime? fromDate = null,
    [FromQuery] DateTime? toDate = null,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 12)
        {
            var user = HttpContext.User;
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không xác định được người dùng.");
            }

            try
            {
                var result = await _appointmentService.GetAppointmentsByFilterAsync(
                    userId, roles, status, fromDate, toDate, pageNumber, pageSize);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Success = false, Message = "Đã xảy ra lỗi hệ thống." });
            }
        }


        [HttpPut("cancel/{appointmentId}")]
        public async Task<IActionResult> Cancel(Guid appointmentId)
        {
            return await _appointmentService.CancelAppointmentAsync(appointmentId);
        }

    }
}
