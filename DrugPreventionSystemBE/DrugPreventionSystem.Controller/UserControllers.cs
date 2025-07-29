using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.AuthModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.UserSearchModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using DrugPreventionSystemBE.DrugPreventionSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] UserRegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _userService.CreateUserAsync(request);
            return result;
        }
        
        

        // GET: api/User        
        [HttpGet]
        public async Task<IActionResult> GetUsersPaged(int pageNumber = 1, int pageSize = 10)
        {
            var result = await _userService.GetUsersByPageAsync(pageNumber, pageSize);
            return Ok(result);
        }

        // GET: api/User/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }

            return Ok(user);
        }

        // PUT: api/User/profile
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UserProfileUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.UpdateUserProfileAsync(request);
            return result;
        }

        // DELETE: api/User/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success)
            {
                return NotFound("Người dùng không tồn tại.");
            }

            return Ok("Người dùng đã được xóa (soft delete).");
        }

        // GET: api/User/exists/{id}
        //[HttpGet("exists/{id}")]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> UserExists(Guid id)
        //{
        //    var exists = await _userService.UserExists(id);
        //    return Ok(new { Exists = exists });
        //}

        // POST: api/User/search
        [HttpPost("search")]
        public async Task<IActionResult> SearchUsers([FromBody] UserSearchModel search)
        {
            var result = await _userService.SearchUsersAsync(search);
            return Ok(result);
        }
        // POST: api/user/change-password
        [HttpPut("changePassword")]
        [Authorize] 
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.ChangePasswordAsync(request.CurrentPassword, request.NewPassword);
            return result;
        }
    }
}
