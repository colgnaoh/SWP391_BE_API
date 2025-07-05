using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CourseReqModel;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface ICourseService
    {
        Task<IActionResult> GetCoursesByPageAsync(int pageNumber, int pageSize, string? filterByName);

        Task<IActionResult> GetCourseByIdAsync(Guid courseId);

        Task<IActionResult> CreateCourseAsync(CourseCreateModel courseCreateRequest);

        Task<IActionResult> UpdateCourseAsync(Guid id, CourseUpdateModel model);

        Task<IActionResult> SoftDeleteCourseAsync(Guid courseId);

        Task<IActionResult> GetPurchasedCoursesAsync(string? filterByName, int pageNumber = 1, int pageSize = 12);
    }
}
