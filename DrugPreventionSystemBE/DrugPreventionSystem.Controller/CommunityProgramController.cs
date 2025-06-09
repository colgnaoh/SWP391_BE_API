using DrugPreventionSystemBE.DrugPreventionSystem.Enity;
using CommunityProgramEntity = DrugPreventionSystemBE.DrugPreventionSystem.Enity.CommunityProgram;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreventionProgramController : ControllerBase
    {
        private readonly ICommunityProgramService _programService;

        public PreventionProgramController(ICommunityProgramService programService)
        {
            _programService = programService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommunityProgram>>> GetPrograms()
        {
            var programs = await _programService.GetAllProgramsAsync();
            return Ok(programs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CommunityProgram>> GetProgram(Guid id)
        {
            var program = await _programService.GetProgramByIdAsync(id);
            if (program == null) return NotFound();
            return Ok(program);
        }

        public async Task<ActionResult<CommunityProgram>> CreateCommunityProgramAsync(CommunityProgram program)
        {
            var created = await _programService.CreateCommunityProgramAsync(program);
            return CreatedAtAction(nameof(GetProgram), new { id = created.Id }, program);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCommunityProgram(Guid id, CommunityProgram program)
        {
            var result = await _programService.UpdateCommunityProgramAsync(id, program);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProgram(Guid id)
        {
            var result = await _programService.DeleteProgramAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
