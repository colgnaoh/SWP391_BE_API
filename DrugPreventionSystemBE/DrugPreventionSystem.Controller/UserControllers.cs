using DrugPreventionSystemBE.DrugPreventionSystem.Enity;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.UserSearchModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET: api/Users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<User>>> SearchUsers([FromBody] UserSearchModel search)
        {
            var users = await _userService.SearchUsersAsync(search);
            return Ok(users);
        }

        // PUT: api/Users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, User updatedUser)
        {
            var success = await _userService.UpdateUserAsync(id, updatedUser);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/Users/{id}
        [HttpPost("delete/{id}")]
        public async Task<IActionResult> SoftDeleteUser(Guid id)
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success)
                return NotFound();

            return Ok(new { message = "User soft-deleted successfully." });
        }

    }
}
