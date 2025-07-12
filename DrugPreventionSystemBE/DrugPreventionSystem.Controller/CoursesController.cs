using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CourseReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controllers
{
    [ApiController]
    [Route("api/course")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        // GET: api/Course
        [HttpGet]
        [AllowAnonymous]// Public access
        public async Task<IActionResult> GetCoursesByPage(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? filterByName = null,
            [FromQuery] Guid? userId = null)
        {
            return await _courseService.GetCoursesByPageAsync(pageNumber, pageSize, filterByName, userId);
        }

        // GET: api/Course/{id}
        [HttpGet("{id}")]
        [Authorize] // Public access
        public async Task<IActionResult> GetCourseById(Guid id)
        {
            return await _courseService.GetCourseByIdAsync(id);
        }

        // POST: api/Course
        [HttpPost("create")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateCourse([FromBody] CourseCreateModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return await _courseService.CreateCourseAsync(request);
        }

        // PUT: api/Course
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] CourseUpdateModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return await _courseService.UpdateCourseAsync(id, request);
        }

        // DELETE: api/Course/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> SoftDeleteCourse(Guid id)
        {
            return await _courseService.SoftDeleteCourseAsync(id);
        }
        [HttpGet("myCourses")]
        [Authorize] 
        public async Task<IActionResult> GetPurchasedCourses()
        {
            return await _courseService.GetPurchasedCoursesAsync();
        }
    }
}
