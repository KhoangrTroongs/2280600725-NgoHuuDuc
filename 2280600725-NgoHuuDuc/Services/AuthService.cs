using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

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

        public async Task<AuthResponseDTO> ExternalLoginAsync(ExternalLoginDTO externalLoginDto)
        {
            // Check if user already exists with this external login
            var user = await FindUserByExternalProviderAsync(externalLoginDto.Provider, externalLoginDto.ProviderKey);

            if (user != null)
            {
                // User exists, sign them in
                await _signInManager.SignInAsync(user, isPersistent: true);
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

            // User doesn't exist, create a new one
            if (string.IsNullOrEmpty(externalLoginDto.Email))
            {
                return new AuthResponseDTO
                {
                    IsSuccess = false,
                    Message = "Email không được cung cấp từ nhà cung cấp đăng nhập."
                };
            }

            // Check if user exists with this email
            user = await _userManager.FindByEmailAsync(externalLoginDto.Email);

            if (user == null)
            {
                // Create new user
                user = new ApplicationUser
                {
                    UserName = externalLoginDto.Email,
                    Email = externalLoginDto.Email,
                    FullName = externalLoginDto.Name ?? externalLoginDto.Email,
                    DateOfBirth = DateTime.Now,
                    Address = "",
                    AvatarUrl = externalLoginDto.PhotoUrl ?? "/images/users/default-avatar.png",
                    CreatedAt = DateTime.Now,
                    IsActive = true,
                    ExternalProvider = externalLoginDto.Provider
                };

                // Set provider-specific ID
                if (externalLoginDto.Provider.ToLower() == "facebook")
                {
                    user.FacebookId = externalLoginDto.ProviderKey;
                }
                else if (externalLoginDto.Provider.ToLower() == "google")
                {
                    user.GoogleId = externalLoginDto.ProviderKey;
                }

                // Create user without password
                var result = await _userManager.CreateAsync(user);
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

                // Add external login
                var info = new UserLoginInfo(externalLoginDto.Provider, externalLoginDto.ProviderKey, externalLoginDto.Provider);
                await _userManager.AddLoginAsync(user, info);

                // Add user to Customer role by default
                if (!await _roleManager.RoleExistsAsync("Customer"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Customer"));
                }
                await _userManager.AddToRoleAsync(user, "Customer");
            }
            else
            {
                // User exists with this email, link the external login
                var info = new UserLoginInfo(externalLoginDto.Provider, externalLoginDto.ProviderKey, externalLoginDto.Provider);
                await _userManager.AddLoginAsync(user, info);

                // Update provider-specific ID
                if (externalLoginDto.Provider.ToLower() == "facebook")
                {
                    user.FacebookId = externalLoginDto.ProviderKey;
                }
                else if (externalLoginDto.Provider.ToLower() == "google")
                {
                    user.GoogleId = externalLoginDto.ProviderKey;
                }

                user.ExternalProvider = externalLoginDto.Provider;
                await _userManager.UpdateAsync(user);
            }

            // Sign in the user
            await _signInManager.SignInAsync(user, isPersistent: true);

            var roles = await _userManager.GetRolesAsync(user);
            var jwtToken = await GenerateJwtTokenAsync(user.Id, user.UserName, roles);

            return new AuthResponseDTO
            {
                IsSuccess = true,
                Message = "Đăng nhập thành công.",
                Token = jwtToken,
                Expiration = DateTime.Now.AddDays(7),
                UserId = user.Id,
                UserName = user.UserName,
                Roles = roles.ToList()
            };
        }

        public async Task<string> GetExternalLoginProviderTokenAsync(string provider)
        {
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, "/Account/ExternalLoginCallback");
            return properties.Items.FirstOrDefault(i => i.Key == ".redirect").Value ?? "";
        }

        public async Task<AuthResponseDTO> HandleExternalLoginCallbackAsync(ExternalLoginInfo info)
        {
            if (info == null)
            {
                return new AuthResponseDTO
                {
                    IsSuccess = false,
                    Message = "Lỗi khi đăng nhập bằng tài khoản ngoài."
                };
            }

            // Sign in with external login provider
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                // User already has a login
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                var roles = await _userManager.GetRolesAsync(user);
                var token = await GenerateJwtTokenAsync(user.Id, user.UserName, roles);

                return new AuthResponseDTO
                {
                    IsSuccess = true,
                    Message = "Đăng nhập thành công.",
                    Token = token,
                    Expiration = DateTime.Now.AddDays(7),
                    UserId = user.Id,
                    UserName = user.UserName,
                    Roles = roles.ToList()
                };
            }

            // Get email from external provider
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return new AuthResponseDTO
                {
                    IsSuccess = false,
                    Message = "Email không được cung cấp từ nhà cung cấp đăng nhập."
                };
            }

            // Create external login DTO
            var externalLoginDto = new ExternalLoginDTO
            {
                Provider = info.LoginProvider,
                ProviderKey = info.ProviderKey,
                Email = email,
                Name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email,
                PhotoUrl = info.Principal.FindFirstValue("picture") // This might be different for different providers
            };

            // Process external login
            return await ExternalLoginAsync(externalLoginDto);
        }

        private async Task<ApplicationUser> FindUserByExternalProviderAsync(string provider, string providerKey)
        {
            if (provider.ToLower() == "facebook")
            {
                return await _userManager.Users.FirstOrDefaultAsync(u => u.FacebookId == providerKey);
            }
            else if (provider.ToLower() == "google")
            {
                return await _userManager.Users.FirstOrDefaultAsync(u => u.GoogleId == providerKey);
            }

            return await _userManager.FindByLoginAsync(provider, providerKey);
        }
    }
}
