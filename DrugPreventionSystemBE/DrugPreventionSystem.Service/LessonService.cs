using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.LessonReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class LessonService : ILessonService

    {
        public Task<IActionResult> CreateLessonAsync(CreateLessonRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> GetLessonByIdAsync(Guid lessonId)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> GetLessonsByPageAsync(int pageNumber, int pageSize, string? filterByName)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> GetLessonsByUserAsync(Guid userId, int pageNumber = 1, int pageSize = 12)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> SoftDeleteLessonAsync(Guid lessonId)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> UpdateLessonAsync(Guid lessonId, UpdateLessonRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
