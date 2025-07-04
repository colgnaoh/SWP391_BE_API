using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SurveyReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controller
{
    [ApiController]
    [Route("api/survey")]
    public class SurveyController : ControllerBase
    {
        private readonly ISurveyService _surveyService;

        public SurveyController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
        }


        //GET: api/survey/page
        [HttpGet("paged")]
        public async Task<IActionResult> GetSurveysByPage(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? filterByName = null)
        {
            return await _surveyService.GetSurveysByPageAsync(pageNumber, pageSize, filterByName);
        }


        // GET: api/survey/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSurveyDetail(Guid id)
        {
            var detail = await _surveyService.GetSurveyDetailAsync(id);
            if (detail == null)
                return NotFound("Survey không tồn tại.");

            return Ok(detail);
        }

        // POST: api/survey
        [HttpPost]
        public async Task<IActionResult> CreateSurvey([FromBody] SurveyCreateModel model)
        {
            return await _surveyService.CreateSurveyAsync(model);
        }

        // PUT: api/survey/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSurvey(Guid id, [FromBody] SurveyUpdateModel model)
        {
            return await _surveyService.UpdateSurveyAsync(id, model);
        }

        // DELETE: api/survey/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSurvey(Guid id)
        {
            return await _surveyService.DeleteSurveyAsync(id);
        }

        // POST: api/survey/submit
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitSurvey([FromBody] SurveySubmitRequestModel model)
        {
            var result = await _surveyService.SubmitSurveyAsync(model);
            return Ok(result);
        }
    }
}
