using Microsoft.AspNetCore.Identity;
using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Models;

namespace NgoHuuDuc_2280600725.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> LoginAsync(LoginUserDTO loginDto);
        Task<AuthResponseDTO> RegisterAsync(RegisterUserDTO registerDto);
        Task<bool> LogoutAsync();
        Task<string> GenerateJwtTokenAsync(string userId, string userName, IList<string> roles);

        // External login methods
        Task<AuthResponseDTO> ExternalLoginAsync(ExternalLoginDTO externalLoginDto);
        Task<string> GetExternalLoginProviderTokenAsync(string provider);
        Task<AuthResponseDTO> HandleExternalLoginCallbackAsync(ExternalLoginInfo info);
    }
}
