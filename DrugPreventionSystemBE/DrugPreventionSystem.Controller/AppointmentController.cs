using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.BookingReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        [HttpPost("booking-direct")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> BookWithSchedule([FromBody] BookingDirectRequest request)
        {
            if (request == null)
                return BadRequest("Yêu cầu không hợp lệ.");

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized("Không thể xác định người dùng.");

            return await _appointmentService.BookWithScheduleAsync(userId, request);
        }

        [HttpPost("booking-withoutTime")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> BookWithoutSchedule([FromBody] BookingRequest request)
        {
            if (request == null)
                return BadRequest("Yêu cầu không hợp lệ.");

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized("Không thể xác định người dùng.");

            return await _appointmentService.BookWithoutScheduleAsync(userId, request);
        }

        [HttpPut("assign")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AssignConsultant([FromBody] AssignConsultantRequest request)
        {
            if (request == null)
                return BadRequest("Yêu cầu không hợp lệ.");

            return await _appointmentService.AssignConsultantAsync(request);
        }

        [HttpPut("complete/{appointmentId}")]
        [Authorize(Roles = "Consultant")]
        public async Task<IActionResult> MarkAsCompleted(Guid appointmentId)
        {
            var consultantIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(consultantIdStr, out var consultantId))
                return Unauthorized("Không thể xác định chuyên viên tư vấn.");

            return await _appointmentService.MarkAsCompletedAsync(consultantId, appointmentId);
        }

        //[HttpPost("review/{appointmentId}")]
        //[Authorize(Roles = "Customer")]
        //public async Task<IActionResult> AddReview(Guid appointmentId, [FromBody] AppointmentReviewRequest request)
        //{
        //    if (request == null)
        //        return BadRequest("Yêu cầu không hợp lệ.");

        //    var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    if (!Guid.TryParse(userIdStr, out var userId))
        //        return Unauthorized("Không thể xác định người dùng.");

        //    return await _appointmentService.AddReviewAsync(userId, appointmentId, request);
        }
    }
