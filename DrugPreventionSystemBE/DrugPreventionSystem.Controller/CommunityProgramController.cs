using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CommunityProgramReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controllers
{
    [ApiController]
    [Route("api/program")]
    public class CommunityProgramController : ControllerBase
    {
        private readonly ICommunityProgramService _programService;

        public CommunityProgramController(ICommunityProgramService programService)
        {
            _programService = programService;
        }

        // GET: api/program?pageNumber=1&pageSize=10&filterByName=abc
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetProgramsByPage(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? filterByName = null)
        {
            return await _programService.GetCommunityProgramsByPageAsync(pageNumber, pageSize, filterByName);
        }

        // GET: api/program/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProgramById(Guid id)
        {
            return await _programService.GetCommunityProgramByIdAsync(id);
        }

        // POST: api/program/create
        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProgram([FromBody] CommunityProgramCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return await _programService.CreateCommunityProgramAsync(request);
        }

        // PUT: api/program/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProgram(Guid id, [FromBody] CommunityProgramUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return await _programService.UpdateCommunityProgramAsync(id, request);
        }

        // DELETE: api/program/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProgram(Guid id)
        {
            return await _programService.DeleteProgramAsync(id);
        }
    }
}
