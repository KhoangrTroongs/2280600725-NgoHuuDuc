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

        // POST: api/Auth/external-login
        [HttpPost("external-login")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDTO<AuthResponseDTO>>> ExternalLogin(ExternalLoginDTO externalLoginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ResponseDTO<AuthResponseDTO>.Fail("Invalid external login data.", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                var result = await _authService.ExternalLoginAsync(externalLoginDto);
                if (!result.IsSuccess)
                {
                    return BadRequest(ResponseDTO<AuthResponseDTO>.Fail(result.Message ?? "External login failed."));
                }

                return Ok(ResponseDTO<AuthResponseDTO>.Success(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during external login for provider {Provider}", externalLoginDto.Provider);
                return StatusCode(500, ResponseDTO<AuthResponseDTO>.Fail("An error occurred during external login."));
            }
        }

        // GET: api/Auth/external-login-token
        [HttpGet("external-login-token/{provider}")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDTO<string>>> GetExternalLoginToken(string provider)
        {
            try
            {
                if (string.IsNullOrEmpty(provider))
                {
                    return BadRequest(ResponseDTO<string>.Fail("Provider is required."));
                }

                var token = await _authService.GetExternalLoginProviderTokenAsync(provider);
                return Ok(ResponseDTO<string>.Success(token));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting external login token for provider {Provider}", provider);
                return StatusCode(500, ResponseDTO<string>.Fail("An error occurred while getting external login token."));
            }
        }
    }
}
