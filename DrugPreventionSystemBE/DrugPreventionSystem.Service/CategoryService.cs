using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ApiResponse;
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

        public async Task<IActionResult> ListCategories(string? filterByName = null)
        {
            try
            {
                var query = _context.Categories
                    .Where(c => !c.IsDeleted) 
                    .AsNoTracking()
                    .AsQueryable();
                // Nếu filterByName không null hoặc rỗng, thêm điều kiện tìm kiếm theo tên
                if (!string.IsNullOrEmpty(filterByName))
                {
                    query = query.Where(c => c.Name != null && EF.Functions.Like(c.Name, $"%{filterByName}%"));
                }

                var categories = await query.ToListAsync();

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
                return new ObjectResult(new BaseResponse(false, $"Lỗi khi lấy danh mục: {ex.Message}", null)) { StatusCode = 500 };
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

        public async Task<IActionResult> UpdateCategoryAsync(Guid categoryId, CreateCategoryRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                return new BadRequestObjectResult(new BaseResponse(false, "Yêu cầu cập nhật danh mục không hợp lệ. Tên là bắt buộc.", null));
            }

            try
            {
                var categoryToUpdate = await _context.Categories.FindAsync(categoryId);

                if (categoryToUpdate == null || categoryToUpdate.IsDeleted)
                {
                    return new NotFoundObjectResult(new BaseResponse(false, "Không tìm thấy danh mục để cập nhật hoặc danh mục đã bị xóa.", null));
                }

                var existingCategoryWithSameName = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == request.Name.ToLower() && c.Id != categoryId && !c.IsDeleted);

                if (existingCategoryWithSameName != null)
                {
                    return new ConflictObjectResult(new BaseResponse(false, "Tên danh mục đã tồn tại cho một danh mục khác.", null));
                }

                categoryToUpdate.Name = request.Name;
                categoryToUpdate.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var responseModel = new CategoryResponseModel
                {
                    Id = categoryToUpdate.Id,
                    Name = categoryToUpdate.Name,
                    CreatedAt = categoryToUpdate.CreatedAt,
                    UpdatedAt = categoryToUpdate.UpdatedAt
                };
                return new OkObjectResult(new BaseResponse(true, "Cập nhật danh mục thành công.", responseModel));
            }
            catch (Exception ex)
            {
                return new ObjectResult(new BaseResponse(false, $"Lỗi khi cập nhật danh mục: {ex.Message}", null)) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> GetCategoryByIdAsync(Guid categoryId)
        {
            try
            {
                var category = await _context.Categories
                    .Where(c => c.Id == categoryId && !c.IsDeleted)
                    .AsNoTracking() 
                    .Select(c => new CategoryResponseModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (category == null)
                {
                    return new NotFoundObjectResult(new BaseResponse(false, "Không tìm thấy danh mục.", null));
                }

                return new OkObjectResult(new BaseResponse(true, "Lấy danh mục thành công.", category));
            }
            catch (Exception ex)
            {
                return new ObjectResult(new BaseResponse(false, $"Lỗi khi lấy danh mục: {ex.Message}", null)) { StatusCode = 500 };
            }
        }

    }
}
