using Microsoft.AspNetCore.Mvc;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.LessonReqModel;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface ILessonService
    {
        Task<IActionResult> CreateLessonAsync(CreateLessonRequest request);
        Task<IActionResult> GetLessonsByPageAsync(int pageNumber, int pageSize, string? filterByName);
        Task<IActionResult> GetLessonByIdAsync(Guid lessonId);
        Task<IActionResult> GetLessonsByUserAsync(Guid sessionId, int pageNumber = 1, int pageSize = 12);
        Task<IActionResult> UpdateLessonAsync(Guid lessonId, UpdateLessonRequest request);
        Task<IActionResult> SoftDeleteLessonAsync(Guid lessonId);
    }
}
