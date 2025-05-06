using NgoHuuDuc_2280600725.DTOs;

namespace NgoHuuDuc_2280600725.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> LoginAsync(LoginUserDTO loginDto);
        Task<AuthResponseDTO> RegisterAsync(RegisterUserDTO registerDto);
        Task<bool> LogoutAsync();
        Task<string> GenerateJwtTokenAsync(string userId, string userName, IList<string> roles);
    }
}
