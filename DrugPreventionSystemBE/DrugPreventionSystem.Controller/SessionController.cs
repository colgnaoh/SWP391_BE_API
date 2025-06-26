using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.SessionReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly ISessionService _sessionService;

        public SessionController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        // GET: api/session
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sessions = await _sessionService.GetAllAsync();
            return Ok(sessions);
        }

        // GET: api/session/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _sessionService.GetByIdAsync(id);
            if (result == null) return NotFound();

            return Ok(result);
        }

        // POST: api/session
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SessionCreateModelView request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _sessionService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT: api/session/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SessionUpdateModelView request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _sessionService.UpdateAsync(id, request);
            if (!updated) return NotFound();

            return NoContent();
        }

        // DELETE: api/session/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _sessionService.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}
