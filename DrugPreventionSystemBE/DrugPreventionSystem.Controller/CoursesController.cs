using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CourseReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        // GET: api/Course
        [HttpGet]
        [AllowAnonymous] // Public access
        public async Task<IActionResult> GetCoursesByPage(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? filterByName = null)
        {
            return await _courseService.GetCoursesByPageAsync(pageNumber, pageSize, filterByName);
        }

        // GET: api/Course/{id}
        [HttpGet("{id}")]
        [AllowAnonymous] // Public access
        public async Task<IActionResult> GetCourseById(Guid id)
        {
            return await _courseService.GetCourseByIdAsync(id);
        }

        // POST: api/Course
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCourse([FromBody] CourseCreateModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return await _courseService.CreateCourseAsync(request);
        }

        // PUT: api/Course
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCourse([FromBody] CourseUpdateModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return await _courseService.UpdateCourseAsync(request);
        }

        // DELETE: api/Course/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SoftDeleteCourse(Guid id)
        {
            return await _courseService.SoftDeleteCourseAsync(id);
        }
    }
}
