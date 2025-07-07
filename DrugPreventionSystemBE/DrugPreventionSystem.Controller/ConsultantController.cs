using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ConsultantModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controller
{
    [ApiController]
    [Route("api/consultant")]
    public class ConsultantController : ControllerBase
    {
        private readonly IConsultantService _consultantService;
        public ConsultantController(IConsultantService consultantService)
        {
            _consultantService = consultantService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllConsultants()
        {
            var response = await _consultantService.GetAllConsultantsAsync();
            if (response.Success == true)
            {
                return Ok(response);
            }
            return StatusCode(500, new { Message = "An error occurred while retrieving consultants." });
        }

        // GET: api/consultant/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GetConsultantByIdResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetConsultantById(Guid id)
        {
            var result = await _consultantService.GetConsultantByIdAsync(id);
            return result;
        }

        // POST: api/consultant/create
        [HttpPost("create")]
        [Authorize(Roles = "Admin,Manager")] 
        public async Task<IActionResult> CreateConsultant([FromBody] ConsultantCreateRequest request)
        {
            var result = await _consultantService.CreateConsultantAsync(request);
            return result;
        }

        // PUT: api/consultant/update/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")] 
        public async Task<IActionResult> UpdateConsultant(Guid id, [FromBody] ConsultantUpdateRequest request)
        {
            var result = await _consultantService.UpdateConsultantAsync(id, request);
            return result;
        }

        // DELETE: api/consultant/delete/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")] 
        public async Task<IActionResult> DeleteConsultant(Guid id)
        {
            var result = await _consultantService.DeleteConsultantAsync(id);
            return result;
        }
    }
}

