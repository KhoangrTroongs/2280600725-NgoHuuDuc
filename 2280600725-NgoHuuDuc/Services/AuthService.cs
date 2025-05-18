using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Services.Interfaces;

namespace NgoHuuDuc_2280600725.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginUserDTO loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return new AuthResponseDTO
                {
                    IsSuccess = false,
                    Message = "Email hoặc mật khẩu không đúng."
                };
            }

            if (!user.IsActive)
            {
                return new AuthResponseDTO
                {
                    IsSuccess = false,
                    Message = "Tài khoản của bạn đã bị vô hiệu hóa."
                };
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, loginDto.RememberMe, false);
            if (!result.Succeeded)
            {
                return new AuthResponseDTO
                {
                    IsSuccess = false,
                    Message = "Email hoặc mật khẩu không đúng."
                };
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var token = await GenerateJwtTokenAsync(user.Id, user.UserName, userRoles);

            return new AuthResponseDTO
            {
                IsSuccess = true,
                Message = "Đăng nhập thành công.",
                Token = token,
                Expiration = DateTime.Now.AddDays(7),
                UserId = user.Id,
                UserName = user.UserName,
                Roles = userRoles.ToList()
            };
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterUserDTO registerDto)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return new AuthResponseDTO
                {
                    IsSuccess = false,
                    Message = "Email đã được sử dụng."
                };
            }

            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                DateOfBirth = registerDto.DateOfBirth,
                PhoneNumber = registerDto.PhoneNumber,
                Address = registerDto.Address,
                Gender = registerDto.Gender,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return new AuthResponseDTO
                {
                    IsSuccess = false,
                    Message = "Đăng ký không thành công.",
                    Roles = errors
                };
            }

            // Add user to Customer role by default
            if (!await _roleManager.RoleExistsAsync("Customer"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Customer"));
            }
            await _userManager.AddToRoleAsync(user, "Customer");

            // Sign in the user
            await _signInManager.SignInAsync(user, isPersistent: false);

            var userRoles = await _userManager.GetRolesAsync(user);
            var token = await GenerateJwtTokenAsync(user.Id, user.UserName, userRoles);

            return new AuthResponseDTO
            {
                IsSuccess = true,
                Message = "Đăng ký thành công.",
                Token = token,
                Expiration = DateTime.Now.AddDays(7),
                UserId = user.Id,
                UserName = user.UserName,
                Roles = userRoles.ToList()
            };
        }

        public async Task<bool> LogoutAsync()
        {
            await _signInManager.SignOutAsync();
            return true;
        }

        public async Task<string> GenerateJwtTokenAsync(string userId, string userName, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? "DefaultSecretKeyWithAtLeast32Characters!"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(7);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"] ?? "https://localhost:5001",
                audience: _configuration["JWT:ValidAudience"] ?? "https://localhost:5001",
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
