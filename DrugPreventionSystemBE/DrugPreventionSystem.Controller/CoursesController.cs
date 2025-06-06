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

    [HttpGet("view/{pageNumber}")]
    public async Task<IActionResult> ViewPage(int pageNumber)
    {
        const int pageSize = 12;

        // Nếu số trang nhỏ hơn 1 thì dùng trang 1
        var safePageNumber = pageNumber < 1 ? 1 : pageNumber;

        var users = await _courseService.GetCoursesByPageAsync(safePageNumber, pageSize);
        return Ok(users);
    }

    public async Task<IActionResult> CreateCourseAsync(int courseId)
    {
        return await _courseService.GetCourseByIdAsync(courseId);
    }
}