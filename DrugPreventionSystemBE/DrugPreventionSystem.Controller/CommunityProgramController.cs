using DrugPreventionSystemBE.DrugPreventionSystem.Enity;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommunityProgramController : ControllerBase
    {
        private readonly ICommunityProgramService _programService;

        public CommunityProgramController(ICommunityProgramService programService)
        {
            _programService = programService;
        }

        /// <summary>
        /// Lấy danh sách chương trình có phân trang và lọc theo tên (nếu có).
        /// </summary>
        [HttpGet("paged")]
        public async Task<IActionResult> GetProgramsPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 12, [FromQuery] string? filterByName = null)
        {
            return await _programService.GetCommunityProgramsByPageAsync(pageNumber, pageSize, filterByName);
        }

        /// <summary>
        /// Lấy chi tiết chương trình theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProgram(Guid id)
        {
            return await _programService.GetCommunityProgramByIdAsync(id);
        }

        /// <summary>
        /// Tạo mới chương trình cộng đồng
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCommunityProgramAsync([FromBody] CommunityProgram program)
        {
            var created = await _programService.CreateCommunityProgramAsync(program);
            return CreatedAtAction(nameof(GetProgram), new { id = created.Id }, created);
        }

        /// <summary>
        /// Cập nhật chương trình theo ID
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCommunityProgram(Guid id, [FromBody] CommunityProgram program)
        {
            var result = await _programService.UpdateCommunityProgramAsync(id, program);
            if (!result) return NotFound("Không tìm thấy chương trình cần cập nhật.");
            return Ok("Cập nhật thành công.");
        }

        /// <summary>
        /// Xóa mềm chương trình theo ID
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProgram(Guid id)
        {
            var result = await _programService.DeleteProgramAsync(id);
            if (!result) return NotFound("Không tìm thấy chương trình cần xóa.");
            return Ok("Xóa mềm chương trình thành công.");
        }
    }
}
