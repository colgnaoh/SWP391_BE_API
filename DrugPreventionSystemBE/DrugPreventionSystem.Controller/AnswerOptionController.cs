using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnswerOptionController : ControllerBase
    {
        private readonly IAnswerOptionService _answerOptionService;

        public AnswerOptionController(IAnswerOptionService answerOptionService)
        {
            _answerOptionService = answerOptionService;
        }

        // POST: api/answeroption
        [HttpPost]
        public async Task<IActionResult> CreateAnswerOption([FromBody] AnswerOptionCreateModel model)
        {
            return await _answerOptionService.CreateAnswerOptionAsync(model);
        }

        // PUT: api/answeroption/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAnswerOption(Guid id, [FromBody] AnswerOptionUpdateModel model)
        {
            return await _answerOptionService.UpdateAnswerOptionAsync(id, model);
        }

        // DELETE: api/answeroption/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnswerOption(Guid id)
        {
            return await _answerOptionService.DeleteAnswerOptionAsync(id);
        }

        // GET: api/answeroption/by-question/{questionId}
        [HttpGet("by-question/{questionId}")]
        public async Task<IActionResult> GetAnswerOptionsByQuestionId(Guid questionId)
        {
            var result = await _answerOptionService.GetAnswerOptionsByQuestionIdAsync(questionId);
            return Ok(result);
        }

        // GET: api/answeroption/paged
        [HttpGet("paged")]
        public async Task<IActionResult> GetAnswerOptionsByPage(
            [FromQuery] Guid? questionId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filter = null,
            [FromQuery] int? filterByScore = null
            )
        {
            return await _answerOptionService.GetAnswerOptionsByPageAsync(questionId, pageNumber, pageSize, filter, filterByScore);
        }
    }
}
