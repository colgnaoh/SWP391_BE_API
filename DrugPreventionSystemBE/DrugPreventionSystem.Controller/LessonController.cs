using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.LessonReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controllers
{
    [Route("api/lesson")]
    [ApiController]
    [Authorize]
    public class LessonController : ControllerBase
    {
        private readonly ILessonService _lessonService;

        public LessonController(ILessonService lessonService)
        {
            _lessonService = lessonService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateLesson([FromBody] CreateLessonRequest request)
        {
            return await _lessonService.CreateLessonAsync(request);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetLessonsByPageAsync(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 12,
    [FromQuery] string? filterByName = null,
    [FromQuery] Guid? courseId = null,
    [FromQuery] Guid? sessionId = null,
    [FromQuery] LessonType? lessonType = null)
        {
            return await _lessonService.GetLessonsByPageAsync(pageNumber, pageSize, filterByName, courseId, sessionId, lessonType);
        }


        [HttpGet("{lessonId}")]
        public async Task<IActionResult> GetLessonById(Guid lessonId)
        {
            return await _lessonService.GetLessonByIdAsync(lessonId);
        }

        [HttpGet("session/{sessionId}")]
        public async Task<IActionResult> GetLessonsByUser(Guid sessionId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 12)
        {
            return await _lessonService.GetLessonsByUserAsync(sessionId, pageNumber, pageSize);
        }

        [HttpPut("{lessonId}")]
        public async Task<IActionResult> UpdateLesson(Guid lessonId, [FromBody] UpdateLessonRequest request)
        {
            return await _lessonService.UpdateLessonAsync(lessonId, request);
        }

        [HttpDelete("{lessonId}")]
        public async Task<IActionResult> SoftDeleteLesson(Guid lessonId)
        {
            return await _lessonService.SoftDeleteLessonAsync(lessonId);
        }
    }
}
