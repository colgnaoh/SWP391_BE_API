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
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> CreateProgram([FromBody] CommunityProgramCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _programService.CreateCommunityProgramAsync(request);

            return StatusCode(201, result);
        }

        // PUT: api/program/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> UpdateProgram(Guid id, [FromBody] CommunityProgramUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return await _programService.UpdateCommunityProgramAsync(id, request);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProgram(Guid id)
        {
            var success = await _programService.DeleteProgramAsync(id);

            return StatusCode(204, new { message = "Đã xóa chương trình" });
        }
    }
}
