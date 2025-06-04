using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly ICourseService _courseService;

    public UserController(ICourseService courseService)
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
}