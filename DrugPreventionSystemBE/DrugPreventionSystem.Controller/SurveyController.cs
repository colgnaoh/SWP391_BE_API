namespace DrugPreventionSystemBE.DrugPreventionSystem.Controller
{
    using global::DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyReqModel;
    using global::DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    namespace DrugPreventionSystemBE.DrugPreventionSystem.Controllers
    {
        [ApiController]
        [Route("api/surveys")]
        public class SurveyController : ControllerBase
        {
            private readonly ISurveyService _surveyService;

            public SurveyController(ISurveyService surveyService)
            {
                _surveyService = surveyService;
            }

            /// <summary>
            /// Lấy danh sách tất cả khảo sát (ASSIST, CRAFFT, ...)
            /// </summary>
            [HttpGet]
            public async Task<IActionResult> GetAllSurveys()
            {
                var surveys = await _surveyService.GetAllSurveysAsync();
                return Ok(surveys);
            }

            /// <summary>
            /// Lấy thông tin chi tiết khảo sát (bao gồm câu hỏi và đáp án)
            /// </summary>
            [HttpGet("{id}")]
            public async Task<IActionResult> GetSurveyDetail(Guid id)
            {
                var survey = await _surveyService.GetSurveyDetailAsync(id);
                if (survey == null)
                    return NotFound("Survey not found");

                return Ok(survey);
            }

            /// <summary>
            /// Gửi bài làm khảo sát (người dùng trả lời)
            /// </summary>
            [HttpPost("{id}/submit")]
            public async Task<IActionResult> SubmitSurvey(Guid id, [FromBody] SurveySubmitRequestModel model)
            {
                if (id != model.SurveyId)
                    return BadRequest("Survey ID mismatch");

                var result = await _surveyService.SubmitSurveyAsync(model);
                return Ok(result);
            }

            /// <summary>
            /// Tạo khảo sát mới (Admin)
            /// </summary>
            [HttpPost]
            [Authorize(Roles = "Admin,Manager")]
            public async Task<IActionResult> CreateSurvey([FromBody] SurveyCreateModel model)
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var newSurveyId = await _surveyService.CreateSurveyAsync(model);
                return CreatedAtAction(nameof(GetSurveyDetail), new { id = newSurveyId }, new { id = newSurveyId });
            }
        }
    }

}
