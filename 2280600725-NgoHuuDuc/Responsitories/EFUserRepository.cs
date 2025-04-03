using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Models.AccountViewModels;

namespace NgoHuuDuc_2280600725.Responsitories
{
    public class EFUserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EFUserRepository(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IdentityResult> RegisterUserAsync(ApplicationUser identityUser, string password)
        {
            return await _userManager.CreateAsync(identityUser, password);
        }

        public async Task AssignRoleAsync(string email, string role)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
                await _userManager.AddToRoleAsync(user, role);
            }
        }

        public async Task SignInUserAsync(string email, bool isPersistent)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                await _signInManager.SignInAsync(user, isPersistent);
            }
        }

        public async Task<SignInResult> PasswordSignInAsync(string email, string password, bool rememberMe)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                return await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: false);
            }
            return SignInResult.Failed;
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user, UserDetailsViewModel model)
        {
            user.PhoneNumber = model.PhoneNumber;
            user.UserName = model.Email;
            user.Email = model.Email;

            // Lưu thông tin bổ sung vào UserClaims
            var claims = await _userManager.GetClaimsAsync(user);

            await UpdateClaim(user, claims, "FullName", model.FullName);
            await UpdateClaim(user, claims, "Address", model.Address);
            await UpdateClaim(user, claims, "DateOfBirth", model.DateOfBirth.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(model.AvatarUrl))
            {
                await UpdateClaim(user, claims, "AvatarUrl", model.AvatarUrl);
            }

            return await _userManager.UpdateAsync(user);
        }

        private async Task UpdateClaim(ApplicationUser user, IList<Claim> existingClaims, string claimType, string claimValue)
        {
            var claim = existingClaims.FirstOrDefault(c => c.Type == claimType);
            if (claim != null)
            {
                await _userManager.RemoveClaimAsync(user, claim);
            }
            await _userManager.AddClaimAsync(user, new Claim(claimType, claimValue));
        }

        public async Task<IdentityResult> DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                return await _userManager.DeleteAsync(user);
            }
            return IdentityResult.Failed();
        }

        public async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        }

        public async Task AddUserDetailsAsync(ApplicationUser user, RegisterViewModel model)
        {
            var claims = new List<Claim>
            {
                new Claim("FullName", model.FullName),
                new Claim("DateOfBirth", model.DateOfBirth.ToString("yyyy-MM-dd")),
                new Claim("Address", model.Address ?? string.Empty)
            };

            if (!string.IsNullOrEmpty(model.AvatarUrl))
            {
                claims.Add(new Claim("AvatarUrl", model.AvatarUrl));
            }

            foreach (var claim in claims)
            {
                await _userManager.AddClaimAsync(user, claim);
            }
        }

        private UserDetailsViewModel ConvertToViewModel(ApplicationUser user)
        {
            var claims = _userManager.GetClaimsAsync(user).Result;
            return new UserDetailsViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? user.UserName,
                PhoneNumber = user.PhoneNumber,
                Address = claims.FirstOrDefault(c => c.Type == "Address")?.Value,
                DateOfBirth = DateTime.TryParse(claims.FirstOrDefault(c => c.Type == "DateOfBirth")?.Value, out var dob) ? dob : DateTime.Now,
                AvatarUrl = claims.FirstOrDefault(c => c.Type == "AvatarUrl")?.Value ?? "/images/users/default-avatar.png",
                IsActive = !user.LockoutEnabled,
                CreatedAt = user.LockoutEnd?.DateTime ?? DateTime.Now
            };
        }

        public async Task<List<UserDetailsViewModel>> GetAllUserDetailsAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            return users.Select(u => ConvertToViewModel(u)).ToList();
        }

        public async Task<UserDetailsViewModel> GetUserDetailsAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            return user != null ? ConvertToViewModel(user) : null;
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                return (await _userManager.GetRolesAsync(user)).ToList();
            }
            return new List<string>();
        }

        public async Task<List<string>> GetAllRolesAsync()
        {
            return await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        }

        public async Task<IdentityResult> UpdateUserRolesAsync(string userId, List<string> newRoles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles.Except(newRoles).ToList();
            var rolesToAdd = newRoles.Except(currentRoles).ToList();

            var result = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!result.Succeeded) return result;

            result = await _userManager.AddToRolesAsync(user, rolesToAdd);
            return result;
        }

        public async Task<IdentityResult> RemoveFromRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                return await _userManager.RemoveFromRoleAsync(user, role);
            }
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });
        }
    }
}
