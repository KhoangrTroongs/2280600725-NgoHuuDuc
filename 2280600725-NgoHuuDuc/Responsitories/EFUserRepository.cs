using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Models.AccountViewModels;

namespace NgoHuuDuc_2280600725.Responsitories
{
    public class EFUserRepository : IUserRepository
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EFUserRepository(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IdentityResult> RegisterUserAsync(IdentityUser identityUser, string password)
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

        public async Task<List<IdentityUser>> GetAllUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<IdentityUser> GetUserByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<IdentityResult> UpdateUserAsync(IdentityUser user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> UpdateUserAsync(IdentityUser user, UserDetailsViewModel model)
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

        private async Task UpdateClaim(IdentityUser user, IList<Claim> existingClaims, string claimType, string claimValue)
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

        public async Task<IdentityUser> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        }

        public async Task AddUserDetailsAsync(IdentityUser user, RegisterViewModel model)
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

        private UserDetailsViewModel ConvertToViewModel(IdentityUser user)
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
    }
}
