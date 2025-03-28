using Microsoft.AspNetCore.Identity;
using NgoHuuDuc_2280600725.Models.AccountViewModels;

namespace NgoHuuDuc_2280600725.Responsitories
{
    public interface IUserRepository
    {
        Task<IdentityResult> RegisterUserAsync(IdentityUser identityUser, string password);
        Task AssignRoleAsync(string email, string role);
        Task SignInUserAsync(string email, bool isPersistent);
        Task<SignInResult> PasswordSignInAsync(string email, string password, bool rememberMe);
        Task SignOutAsync();
        Task<List<IdentityUser>> GetAllUsersAsync();
        Task<IdentityUser> GetUserByIdAsync(string id);
        Task<IdentityResult> UpdateUserAsync(IdentityUser user, UserDetailsViewModel model);
        Task<IdentityResult> UpdateUserAsync(IdentityUser user);
        Task<IdentityResult> DeleteUserAsync(string id);
        Task<IdentityUser> GetCurrentUserAsync();
        Task<List<UserDetailsViewModel>> GetAllUserDetailsAsync();
        Task<UserDetailsViewModel> GetUserDetailsAsync(string id);
        Task AddUserDetailsAsync(IdentityUser user, RegisterViewModel model);
    }
}
