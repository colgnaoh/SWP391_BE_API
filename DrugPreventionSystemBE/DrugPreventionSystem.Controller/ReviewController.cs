using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ReviewReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controller
{
    [Authorize]
    [Route("api/review")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return await _reviewService.GetAllReviewsAsync();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            return await _reviewService.GetReviewByIdAsync(id);
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourseId(Guid courseId)
        {
            return await _reviewService.GetReviewsByCourseIdAsync(courseId);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            return await _reviewService.GetReviewsByUserIdAsync(userId);
        }

        [HttpGet("appointment/{appointmentId}")]
        public async Task<IActionResult> GetByAppointmentId(Guid appointmentId)
        {
            return await _reviewService.GetReviewsByAppointmentIdAsync(appointmentId);
        }

        [HttpGet("consultant/{consultantId}")]
        public async Task<IActionResult> GetReviewsByConsultantId(Guid consultantId)
        {
            return await _reviewService.GetReviewsByConsultantIdAsync(consultantId);
        }

        [HttpPost("course")]
        public async Task<IActionResult> CreateCourseReview([FromBody] CreateReviewCourseReqModel request)
        {
            return await _reviewService.CreateReviewAsync(request);
        }

        [HttpPost("appointment")]
        public async Task<IActionResult> CreateAppointmentReview([FromBody] CreateAppointmentReviewReqModel request)
        {
            return await _reviewService.CreateAppointmentReviewAsync(request);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReviewReqModel request)
        {
            return await _reviewService.UpdateReviewAsync(id, request);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return await _reviewService.DeleteReviewAsync(id);
        }
    }
}