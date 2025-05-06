using NgoHuuDuc_2280600725.DTOs;

namespace NgoHuuDuc_2280600725.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<UserDTO?> GetUserByIdAsync(string id);
        Task<UserDTO?> GetCurrentUserAsync();
        Task<UserDTO?> UpdateUserAsync(string id, UpdateUserDTO userDto, IFormFile? avatarFile);
        Task<bool> DeleteUserAsync(string id);
    }
}
