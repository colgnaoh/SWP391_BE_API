using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ApiResponse;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.BlogReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controller
{
    [ApiController]
    [Route("/api/blog")]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _blogService;
        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
        }
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateBlogAsync([FromBody] CreateBlogRequest request)
        {
            if (request == null)
            {
                return BadRequest("Yêu cầu không hợp lệ.");
            }
            return await _blogService.CreateBlogAsync(request);
        }

        [HttpGet]
        public async Task<IActionResult> GetBlogsByPageAsync([FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] string? filterByContent)
        {
            return await _blogService.GetBlogsByPageAsync(pageNumber, pageSize, filterByContent);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBlogById(Guid id)
        {
            return await _blogService.GetBlogByIdAsync(id);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetBlogsByUser(
           Guid userId,
           [FromQuery] int pageNumber = 1,
           [FromQuery] int pageSize = 12)
        {
            return await _blogService.GetBlogsByUserAsync(userId, pageNumber, pageSize);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateBlog(Guid id, [FromBody] UpdateBlogRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return await _blogService.UpdateBlogAsync(id, request);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> SoftDeleteBlog(Guid id)
        {
            var result = await _blogService.SoftDeleteBlogAsync(id);
            if (result is NotFoundResult)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Data = null,
                    Message = "Không tìm thấy blog hoặc không thể xóa."
                });
            }
            else
            {
                var blog = await _blogService.GetBlogByIdAsync(id); 
                return Ok(new ApiResponse<IActionResult>
                {
                    Success = true,
                    Data = {}, // Use the awaited result here
                    Message = "Xóa blog thành công."
                });
            }
        }
    }
}
