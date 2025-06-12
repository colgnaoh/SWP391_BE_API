using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CourseReqModel;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface ICourseService
    {
        Task<IActionResult> GetCoursesByPageAsync([FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] string? filterByName);
        Task<IActionResult> GetCourseByIdAsync(int courseId);

        Task<IActionResult> CreateCourseAsync(CourseCreateRequest courseCreateRequest);

    }
}
