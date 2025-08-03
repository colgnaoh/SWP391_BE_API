using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CommunityProgramReqModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.CommunityProgramsResModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System;
using System.Security.Claims;
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
     [FromQuery] string? filterByName = null,
     [FromQuery] RiskLevel? riskLevel = null,
     [FromQuery] Programtype? programType = null)
        {
            return await _programService.GetCommunityProgramsByPageAsync(
                pageNumber, pageSize, filterByName, riskLevel, programType);
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


        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll([FromBody] EnrollProgramRequest request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(new EnrollProgramResponse
                {
                    Success = false,
                    Message = "Token không hợp lệ hoặc thiếu thông tin người dùng."
                });
            }

            var result = await _programService.EnrollToProgramAsync(request, userId);
            return result.Success
                ? Ok(result)
                : BadRequest(result);
        }


        [HttpGet("history")]
        [Authorize]
        public async Task<IActionResult> GetHistory()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "Token không hợp lệ hoặc thiếu thông tin người dùng."
                });
            }

            var history = await _programService.GetEnrollmentHistoryAsync(userId, User);

            return Ok(new
            {
                Success = true,
                Message = "Lấy dữ liệu chương trình thành công.",
                Data = history
            });
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProgram(Guid id)
        {
            var success = await _programService.DeleteProgramAsync(id);

            return StatusCode(204, new { message = "Đã xóa chương trình" });
        }
    }
}
