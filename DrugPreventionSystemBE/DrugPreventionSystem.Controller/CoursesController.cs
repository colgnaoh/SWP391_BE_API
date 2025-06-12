using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class CourseController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CourseController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCoursesByPageAsync([FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] string? filterByName)
    {
        // Nếu số trang nhỏ hơn 1 thì dùng trang 1
        var safePageNumber = pageNumber < 1 ? 1 : pageNumber;

        var users = await _courseService.GetCoursesByPageAsync(safePageNumber, pageSize, filterByName);
        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCourseAsync(int courseId)
    {
        return await _courseService.GetCourseByIdAsync(courseId);
    }
}