using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommunityProgramController : ControllerBase
    {
        private readonly ICommunityProgramService _programService;

        public CommunityProgramController(ICommunityProgramService programService)
        {
            _programService = programService;
        }

        // GET: api/CommunityProgram?pageNumber=1&pageSize=10&filterByName=abc
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetProgramsByPage(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? filterByName = null)
        {
            return await _programService.GetCommunityProgramsByPageAsync(pageNumber, pageSize, filterByName);
        }

        // GET: api/CommunityProgram/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProgramById(Guid id)
        {
            return await _programService.GetCommunityProgramByIdAsync(id);
        }

        // POST: api/CommunityProgram
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProgram([FromBody] CommunityProgram program)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return await _programService.CreateCommunityProgramAsync(program);
        }

        // PUT: api/CommunityProgram/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProgram(Guid id, [FromBody] CommunityProgram program)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return await _programService.UpdateCommunityProgramAsync(id, program);
        }

        // DELETE: api/CommunityProgram/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProgram(Guid id)
        {
            return await _programService.DeleteProgramAsync(id);
        }
    }
}