using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgoHuuDuc_2280600725.Models.AccountViewModels;
using NgoHuuDuc_2280600725.Responsitories;

namespace NgoHuuDuc_2280600725.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class RoleManagementController : Controller
    {
        private readonly IUserRepository _userRepository;

        public RoleManagementController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetAllUsersAsync();
            var viewModels = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userRepository.GetUserRolesAsync(user.Id);
                viewModels.Add(new UserRolesViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    CurrentRoles = roles
                });
            }

            return View(viewModels);
        }

        public async Task<IActionResult> EditRoles(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound();

            var currentRoles = await _userRepository.GetUserRolesAsync(userId);
            var allRoles = await _userRepository.GetAllRolesAsync();

            var viewModel = new UserRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                CurrentRoles = currentRoles,
                AvailableRoles = allRoles,
                SelectedRoles = currentRoles
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditRoles(string userId, List<string> roles)
        {
            var result = await _userRepository.UpdateUserRolesAsync(userId, roles);
            if (result.Succeeded)
            {
                TempData["Message"] = "Roles updated successfully";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Error updating roles";
            return RedirectToAction(nameof(EditRoles), new { userId });
        }
    }
}
