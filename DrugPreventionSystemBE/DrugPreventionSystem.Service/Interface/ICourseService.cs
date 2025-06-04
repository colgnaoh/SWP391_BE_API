using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface ICourseService
    {
        Task<IActionResult> GetCoursesByPageAsync(int pageNumber, int pageSize);
        Task<IActionResult> GetCourseByIdAsync(int courseId);
        
    }
}
