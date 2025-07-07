using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controllers
{
    [ApiController]
    [Route("api/answer-options")]
    public class AnswerOptionController : ControllerBase
    {
        private readonly IAnswerOptionService _answerOptionService;

        public AnswerOptionController(IAnswerOptionService answerOptionService)
        {
            _answerOptionService = answerOptionService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> BulkCreate([FromBody] MultipleAnswerOptionCreateModel model)
        {
            return await _answerOptionService.CreateAnswerOptionsAsync(model);
        }

        [HttpPut("update")]
        public async Task<IActionResult> BulkUpdate([FromBody] BulkUpdateAnswerOptionModel model)
        {
            return await _answerOptionService.BulkUpdateAnswerOptionsAsync(model);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return await _answerOptionService.DeleteAnswerOptionAsync(id);
        }

        [HttpGet("by-question/{questionId}")]
        public async Task<IActionResult> GetByQuestionId(Guid questionId)
        {
            var result = await _answerOptionService.GetAnswerOptionsByQuestionIdAsync(questionId);
            return Ok(result);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetByPage(
            [FromQuery] Guid? questionId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filter = null,
            [FromQuery] int? filterByScore = null)
        {
            return await _answerOptionService.GetAnswerOptionsByPageAsync(questionId, pageNumber, pageSize, filter, filterByScore);
        }
    }
}
