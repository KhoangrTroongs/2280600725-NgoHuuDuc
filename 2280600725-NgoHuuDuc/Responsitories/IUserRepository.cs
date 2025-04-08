using Microsoft.AspNetCore.Identity;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Models.AccountViewModels;

namespace NgoHuuDuc_2280600725.Responsitories
{
    public interface IUserRepository
    {
        Task<IdentityResult> RegisterUserAsync(ApplicationUser identityUser, string password);
        Task AssignRoleAsync(string email, string role);
        Task SignInUserAsync(string email, bool isPersistent);
        Task<SignInResult> PasswordSignInAsync(string email, string password, bool rememberMe);
        Task SignOutAsync();
        Task<List<ApplicationUser>> GetAllUsersAsync();
        Task<ApplicationUser> GetUserByIdAsync(string id);
        Task<IdentityResult> UpdateUserAsync(ApplicationUser user, UserDetailsViewModel model);
        Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
        Task<IdentityResult> DeleteUserAsync(string id);
        Task<ApplicationUser> GetCurrentUserAsync();
        Task<List<UserDetailsViewModel>> GetAllUserDetailsAsync();
        Task<UserDetailsViewModel> GetUserDetailsAsync(string id);
        Task AddUserDetailsAsync(ApplicationUser user, RegisterViewModel model);
        Task<List<string>> GetUserRolesAsync(string userId);
        Task<List<string>> GetAllRolesAsync();
        Task<IdentityResult> UpdateUserRolesAsync(string userId, List<string> roles);
        Task<IdentityResult> RemoveFromRoleAsync(string userId, string role);
        Task<List<ApplicationUser>> GetUsersInRoleAsync(string roleName);
        Task<IdentityResult> LockUserAsync(string userId);
        Task<IdentityResult> UnlockUserAsync(string userId);
    }
}
