using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class CourseService : ICourseService
    {
        private readonly DrugPreventionDbContext _context;

        public CourseService(DrugPreventionDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IActionResult> GetCoursesByPageAsync()
        {
            return await _context.Courses
            .OrderBy(u => u.Id) // sắp xếp để phân trang ổn định
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        }
        public Task<IActionResult> GetCourseByIdAsync(int courseId)
        {
            throw new NotImplementedException();
        }
    }
    {
    }
}
