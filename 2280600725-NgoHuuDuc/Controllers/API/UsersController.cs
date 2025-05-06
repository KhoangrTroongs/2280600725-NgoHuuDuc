using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Services.Interfaces;
using System.Security.Claims;

namespace NgoHuuDuc_2280600725.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // GET: api/Users
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<ResponseDTO<IEnumerable<UserDTO>>>> GetUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(ResponseDTO<IEnumerable<UserDTO>>.Success(users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, ResponseDTO<IEnumerable<UserDTO>>.Fail("An error occurred while retrieving users."));
            }
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDTO<UserDTO>>> GetUser(string id)
        {
            try
            {
                // Check if user is authorized to view this user
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Administrator");
                
                if (currentUserId != id && !isAdmin)
                {
                    return Forbid();
                }

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(ResponseDTO<UserDTO>.Fail("User not found."));
                }
                return Ok(ResponseDTO<UserDTO>.Success(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {Id}", id);
                return StatusCode(500, ResponseDTO<UserDTO>.Fail("An error occurred while retrieving the user."));
            }
        }

        // GET: api/Users/current
        [HttpGet("current")]
        public async Task<ActionResult<ResponseDTO<UserDTO>>> GetCurrentUser()
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                {
                    return NotFound(ResponseDTO<UserDTO>.Fail("User not found."));
                }
                return Ok(ResponseDTO<UserDTO>.Success(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(500, ResponseDTO<UserDTO>.Fail("An error occurred while retrieving the current user."));
            }
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseDTO<UserDTO>>> UpdateUser(string id, [FromForm] UpdateUserDTO userDto, IFormFile? avatarFile)
        {
            try
            {
                // Check if user is authorized to update this user
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Administrator");
                
                if (currentUserId != id && !isAdmin)
                {
                    return Forbid();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ResponseDTO<UserDTO>.Fail("Invalid user data.", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                var user = await _userService.UpdateUserAsync(id, userDto, avatarFile);
                if (user == null)
                {
                    return NotFound(ResponseDTO<UserDTO>.Fail("User not found."));
                }

                return Ok(ResponseDTO<UserDTO>.Success(user));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {Id}", id);
                return StatusCode(500, ResponseDTO<UserDTO>.Fail("An error occurred while updating the user."));
            }
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseDTO<bool>>> DeleteUser(string id)
        {
            try
            {
                // Check if user is authorized to delete this user
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Administrator");
                
                if (currentUserId != id && !isAdmin)
                {
                    return Forbid();
                }

                var result = await _userService.DeleteUserAsync(id);
                if (!result)
                {
                    return NotFound(ResponseDTO<bool>.Fail("User not found."));
                }

                return Ok(ResponseDTO<bool>.Success(true, "User deleted successfully."));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {Id}", id);
                return StatusCode(500, ResponseDTO<bool>.Fail("An error occurred while deleting the user."));
            }
        }
    }
}
