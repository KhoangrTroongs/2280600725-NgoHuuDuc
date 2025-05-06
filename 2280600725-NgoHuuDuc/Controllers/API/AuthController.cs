using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Services.Interfaces;

namespace NgoHuuDuc_2280600725.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDTO<AuthResponseDTO>>> Login(LoginUserDTO loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ResponseDTO<AuthResponseDTO>.Fail("Invalid login data.", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                var result = await _authService.LoginAsync(loginDto);
                if (!result.IsSuccess)
                {
                    return Unauthorized(ResponseDTO<AuthResponseDTO>.Fail(result.Message ?? "Invalid login attempt."));
                }

                return Ok(ResponseDTO<AuthResponseDTO>.Success(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Email}", loginDto.Email);
                return StatusCode(500, ResponseDTO<AuthResponseDTO>.Fail("An error occurred during login."));
            }
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDTO<AuthResponseDTO>>> Register(RegisterUserDTO registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ResponseDTO<AuthResponseDTO>.Fail("Invalid registration data.", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                var result = await _authService.RegisterAsync(registerDto);
                if (!result.IsSuccess)
                {
                    return BadRequest(ResponseDTO<AuthResponseDTO>.Fail(result.Message ?? "Registration failed.", result.Roles));
                }

                return Ok(ResponseDTO<AuthResponseDTO>.Success(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {Email}", registerDto.Email);
                return StatusCode(500, ResponseDTO<AuthResponseDTO>.Fail("An error occurred during registration."));
            }
        }

        // POST: api/Auth/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ResponseDTO<bool>>> Logout()
        {
            try
            {
                await _authService.LogoutAsync();
                return Ok(ResponseDTO<bool>.Success(true, "Logged out successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, ResponseDTO<bool>.Fail("An error occurred during logout."));
            }
        }
    }
}
