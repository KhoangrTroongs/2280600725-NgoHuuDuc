﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Data;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Models.AccountViewModels;
using NgoHuuDuc_2280600725.Models.ViewModels;
using NgoHuuDuc_2280600725.Responsitories;
using System.Security.Claims;
using System.Text.Json;

namespace NgoHuuDuc_2280600725.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IUserRepository userRepository,
            ILogger<AccountController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (model == null)
            {
                ModelState.AddModelError(string.Empty, "Dữ liệu đăng nhập không hợp lệ.");
                return View();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError(string.Empty, "Email và mật khẩu không được để trống.");
                return View(model);
            }

            var result = await _userRepository.PasswordSignInAsync(model.Email, model.Password, model.RememberMe);
            if (result == null)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi xử lý đăng nhập.");
                return View(model);
            }

            if (result.Succeeded)
            {
                _logger.LogInformation(1, "Đăng nhập thành công.");
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning(2, "Tài khoản bị khóa.");
                return View("Lockout");
            }

            ModelState.AddModelError(string.Empty, "Đăng nhập không thành công.");
            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // Xử lý upload avatar
                if (model.AvatarFile != null && model.AvatarFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "users");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.AvatarFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.AvatarFile.CopyToAsync(fileStream);
                    }

                    model.AvatarUrl = "/images/users/" + uniqueFileName;
                }
                else
                {
                    // Gán ảnh mặc định nếu không có upload
                    model.AvatarUrl = "/images/users/default-avatar.png";
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    EmailConfirmed = true,
                    FullName = model.FullName,
                    DateOfBirth = model.DateOfBirth,
                    Address = model.Address ?? "",
                    Gender = (Models.Gender)model.Gender,
                    AvatarUrl = model.AvatarUrl
                };

                var result = await _userRepository.RegisterUserAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userRepository.AddUserDetailsAsync(user, model);
                    await _userRepository.AssignRoleAsync(model.Email, "User");
                    await _userRepository.SignInUserAsync(model.Email, isPersistent: false);
                    return RedirectToLocal(returnUrl);
                }
                AddErrors(result);
            }
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _userRepository.SignOutAsync();
            _logger.LogInformation(5, "Đăng xuất thành công.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        // GET: Account
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetAllUserDetailsAsync();
            return View(users);
        }

        // GET: Account/GetAllRoles
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _userRepository.GetAllRolesAsync();
            return Json(roles);
        }

        // GET: Account/GetUserRoles
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var roles = await _userRepository.GetUserRolesAsync(userId);
            return Json(roles);
        }

        // POST: Account/UpdateRoles
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateRoles([FromBody] Dictionary<string, List<string>> userRoles)
        {
            // Log dữ liệu đầu vào để debug
            Console.WriteLine($"Received data: {JsonSerializer.Serialize(userRoles)}");

            if (userRoles == null || userRoles.Count == 0)
            {
                return Json(new { success = false, message = "Không có dữ liệu vai trò để cập nhật" });
            }

            try
            {
                var currentUser = await _userRepository.GetCurrentUserAsync();
                var currentUserId = currentUser?.Id;

                foreach (var entry in userRoles)
                {
                    var userId = entry.Key;
                    var roles = entry.Value;

                    // Đảm bảo roles không null
                    if (roles == null)
                    {
                        roles = new List<string>();
                    }

                    // Kiểm tra nếu người dùng hiện tại là admin và đang cố gắng thay đổi vai trò của chính mình
                    if (userId == currentUserId)
                    {
                        // Đảm bảo vai trò Administrator vẫn được giữ lại
                        if (!roles.Contains("Administrator"))
                        {
                            roles.Add("Administrator");
                        }
                    }

                    Console.WriteLine($"Updating roles for user {userId}: {JsonSerializer.Serialize(roles)}");
                    var result = await _userRepository.UpdateUserRolesAsync(userId, roles);
                    if (!result.Succeeded)
                    {
                        return Json(new { success = false, message = string.Join(", ", result.Errors.Select(e => e.Description)) });
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in UpdateRoles: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details()
        {
            var currentUser = await _userRepository.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return NotFound();
            }
            var userDetails = await _userRepository.GetUserDetailsAsync(currentUser.Id);
            return View(userDetails);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UserDetails(string id)
        {
            var userDetails = await _userRepository.GetUserDetailsAsync(id);
            if (userDetails == null)
            {
                return NotFound();
            }
            return View("Details", userDetails);
        }

        [HttpGet]
        public async Task<IActionResult> Update()
        {
            var currentUser = await _userRepository.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return NotFound();
            }
            var userDetails = await _userRepository.GetUserDetailsAsync(currentUser.Id);
            return View(userDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UserDetailsViewModel model, IFormFile avatarFile)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentUser = await _userRepository.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return NotFound();
            }

            // Get existing user details to preserve data
            var existingUser = await _userRepository.GetUserDetailsAsync(currentUser.Id);

            // Handle avatar upload
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "users");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Delete old avatar if exists and not default
                if (!string.IsNullOrEmpty(existingUser.AvatarUrl) &&
                    !existingUser.AvatarUrl.EndsWith("default-avatar.png"))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                        existingUser.AvatarUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + avatarFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(fileStream);
                }

                model.AvatarUrl = "/images/users/" + uniqueFileName;
            }
            else
            {
                // Keep existing avatar if no new one is uploaded
                model.AvatarUrl = existingUser.AvatarUrl;
            }

            // Update user information
            var result = await _userRepository.UpdateUserAsync(currentUser, model);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Details));
            }

            AddErrors(result);
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userDetails = await _userRepository.GetUserDetailsAsync(id);
            if (userDetails == null)
            {
                return NotFound();
            }
            return View(userDetails);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var result = await _userRepository.DeleteUserAsync(id);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Không thể xóa người dùng này.");
            var userDetails = await _userRepository.GetUserDetailsAsync(id);
            return View(userDetails);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}
