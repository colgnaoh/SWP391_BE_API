using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CategoryReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                return new StatusCodeResult(500); 
            }
        }

        public async Task<IActionResult> ListCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .Where(c => !c.IsDeleted)
                    .AsNoTracking() 
                    .ToListAsync();
                return new OkObjectResult(new ListCategoryResponse
                {
                    Success = true,
                    Data = categories.Select(c => new CategoryResponseModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving categories.", ex);
            }
        }

        public async Task<IActionResult> SoftDeleteCategoryAsync(Guid CategoryId)
        {
            var Category = await _context.Categories.FindAsync(CategoryId);
            if (Category == null || Category.IsDeleted)
            {
                return new NotFoundResult();
            }

            Category.IsDeleted = true;
            Category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return new OkObjectResult("Category soft-deleted successfully.");
        }
    }
}
