using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CategoryReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controller
{
    [ApiController]
    [Route("/api/category")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateCategoryAsync([FromBody] CreateCategoryRequest request)
        {
            return await _categoryService.CreateCategoryAsync(request);
        }

        [HttpGet]
        public async Task<IActionResult> ListCategories([FromQuery] string? filterByName)
        {
            return await _categoryService.ListCategories(filterByName);
        }
        [HttpGet("{categoryId}")] 
        public async Task<IActionResult> GetCategoryByIdAsync(Guid categoryId)
        {
            return await _categoryService.GetCategoryByIdAsync(categoryId);
        }

        [HttpPut("{categoryId}")] 
        [Authorize(Roles = "Admin,Manager")] 
        public async Task<IActionResult> UpdateCategoryAsync(Guid categoryId, [FromBody] CreateCategoryRequest request)
        {
            return await _categoryService.UpdateCategoryAsync(categoryId, request);
        }

        [HttpDelete("{categoryId}")] 
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> SoftDeleteCategoryAsync(Guid categoryId)
        {
            return await _categoryService.SoftDeleteCategoryAsync(categoryId);
        }
    }
    
}
