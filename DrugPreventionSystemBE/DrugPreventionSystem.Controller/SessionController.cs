using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SessionReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controllers
{
    [Route("api/session")]
    [ApiController]
    [Authorize]
    public class SessionController : ControllerBase
    {
        private readonly ISessionService _sessionService;

        public SessionController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        /// <summary>
        /// Tạo mới buổi học (Session)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateSession([FromBody] SessionCreateModelView request)
        {
            return await _sessionService.CreateAsync(request);
        }

        /// <summary>
        /// Lấy tất cả buổi học (không phân trang, lọc)
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] string? name, [FromQuery] Guid? courseId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 12)
        {
            return await _sessionService.GetAllAsync(name, courseId, pageNumber, pageSize);
        }


        /// <summary>
        /// Lấy thông tin 1 buổi học theo Id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSessionById(Guid id)
        {
            return await _sessionService.GetByIdAsync(id);
        }

        /// <summary>
        /// Lấy danh sách buổi học theo khóa học (CourseId) có phân trang
        /// </summary>
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetSessionByPageAsync(Guid courseId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 12)
        {
            return await _sessionService.GetSessionByPageAsync(courseId, pageNumber, pageSize);
        }

        /// <summary>
        /// Cập nhật buổi học
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSession(Guid id, [FromBody] SessionUpdateModelView request)
        {
            return await _sessionService.UpdateAsync(id, request);
        }

        /// <summary>
        /// Xóa mềm buổi học
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteSession(Guid id)
        {
            return await _sessionService.SoftDeleteAsync(id);
        }
    }
}
