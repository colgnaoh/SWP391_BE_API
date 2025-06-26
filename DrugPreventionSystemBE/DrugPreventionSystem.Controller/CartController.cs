using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CourseReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controller
{
    [ApiController]
    [Route("/api/cart")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [Authorize]
        [HttpPost("addCourse")]
        public async Task<IActionResult> AddCourseToCartAsync([FromBody] AddToCartRequest request)
        {
            if (request == null)
            {
                return BadRequest("Yêu cầu không hợp lệ.");
            }
            var userId = _cartService.GetCurrentUserId();
            return await _cartService.AddCourseToCartAsync(userId, request);
        }

        [Authorize]
        [HttpGet("myCart")]
        public async Task<IActionResult> GetUserCartAsync([FromQuery] Guid userId)
        {
            return await _cartService.GetUserCartAsync(userId);
        }
    }
}
