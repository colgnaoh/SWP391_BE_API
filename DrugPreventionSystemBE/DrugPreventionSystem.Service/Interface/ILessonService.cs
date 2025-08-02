using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.LessonReqModel;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface ILessonService
    {
        Task<IActionResult> CreateLessonAsync(CreateLessonRequest request);
        Task<IActionResult> GetLessonsByPageAsync(int pageNumber, int pageSize, string? filterByName, Guid? courseId, Guid? sessionId, LessonType? lessonType);
        Task<IActionResult> GetLessonByIdAsync(Guid lessonId);
        Task<IActionResult> GetLessonsByUserAsync(Guid sessionId, int pageNumber = 1, int pageSize = 12);
        Task<IActionResult> UpdateLessonAsync(Guid lessonId, UpdateLessonRequest request);
        Task<IActionResult> SoftDeleteLessonAsync(Guid lessonId);
    }
}
