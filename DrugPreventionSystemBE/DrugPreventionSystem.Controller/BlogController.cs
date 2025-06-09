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
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateBlogAsync([FromBody] CreateBlogRequest request)
        {
            if (request == null)
            {
                return BadRequest("Yêu cầu không hợp lệ.");
            }
            return await _blogService.CreateBlogAsync(request);
        }
        [HttpGet("page/{pageNumber}/{pageSize}")]
        public async Task<IActionResult> GetBlogsByPageAsync(int pageNumber, int pageSize)
        {
            return await _blogService.GetBlogsByPageAsync(pageNumber, pageSize);
        }
    }
}
