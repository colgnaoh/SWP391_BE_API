using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CourseReqModel;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface ICourseService
    {
        Task<IActionResult> GetCoursesByPageAsync(int pageNumber, int pageSize, string? filterByName, Guid? userId);

        Task<IActionResult> GetCourseByIdAsync(Guid courseId, Guid? userId);

        Task<IActionResult> CreateCourseAsync(CourseCreateModel courseCreateRequest);

        Task<IActionResult> UpdateCourseAsync(Guid id, CourseUpdateModel model);

        Task<IActionResult> SoftDeleteCourseAsync(Guid courseId);

        Task<IActionResult> GetPurchasedCoursesAsync();
    }
}
