using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controller
{
    [ApiController]
    [Route("api/question")]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;

        public QuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        // POST: api/question
        [HttpPost]
        public async Task<IActionResult> CreateQuestion([FromBody] QuestionCreateModel model)
        {
            return await _questionService.CreateQuestionAsync(model);
        }

        // PUT: api/question/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuestion(Guid id, [FromBody] QuestionUpdateModel model)
        {
            return await _questionService.UpdateQuestionAsync(id, model);
        }

        // DELETE: api/question/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(Guid id)
        {
            return await _questionService.DeleteQuestionAsync(id);
        }

        // GET: api/question/by-survey/{surveyId}
        [HttpGet("by-survey/{surveyId}")]
        public async Task<IActionResult> GetQuestionsBySurveyId(Guid surveyId)
        {
            var result = await _questionService.GetQuestionsBySurveyIdAsync(surveyId);
            return Ok(result);
        }

        // GET: api/question/paged
        [HttpGet("paged")]
        public async Task<IActionResult> GetQuestionsByPage(
            [FromQuery] Guid? surveyId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filter = null)
        {
            return await _questionService.GetQuestionsByPageAsync(surveyId, pageNumber, pageSize, filter);
        }
    }
}
