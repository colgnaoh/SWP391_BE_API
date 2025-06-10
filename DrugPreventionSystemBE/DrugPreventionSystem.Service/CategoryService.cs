using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Enity;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CategoryReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly IdServices _idServices;
        private readonly DrugPreventionDbContext _context;

        public CategoryService(IdServices idServices, DrugPreventionDbContext context)
        {
            _idServices = idServices;
            _context = context;
        }

        public async Task<IActionResult> CreateCategoryAsync(CreateCategoryRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                return new BadRequestObjectResult("Invalid category request. Name is required.");
            }
            var categoryId = _idServices.GenerateNextId();
            var newCategory = new Category
            {
                Id = categoryId,
                Name = request.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            try
            {
                _context.Categories.AddAsync(newCategory);
                await _context.SaveChangesAsync();
                return new OkObjectResult(newCategory);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500); // try catch bắt lỗi và trả về lỗi 500 nếu có lỗi xảy ra
            }
        }
    }
}
