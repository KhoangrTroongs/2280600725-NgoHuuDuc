using System.ComponentModel.DataAnnotations;
using NgoHuuDuc_2280600725.Models;

namespace NgoHuuDuc_2280600725.DTOs
{
    public class UserDTO
    {
        public string Id { get; set; } = "";
        
        public string Email { get; set; } = "";
        
        public string FullName { get; set; } = "";
        
        public DateTime DateOfBirth { get; set; }
        
        public string? PhoneNumber { get; set; }
        
        public string Address { get; set; } = "";
        
        public string AvatarUrl { get; set; } = "";
        
        public Gender Gender { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }

    public class RegisterUserDTO
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";
        
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
        
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = "";
        
        [Required(ErrorMessage = "Họ và tên không được để trống")]
        public string FullName { get; set; } = "";
        
        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        
        public string? PhoneNumber { get; set; }
        
        public string Address { get; set; } = "";
        
        [Required(ErrorMessage = "Giới tính không được để trống")]
        public Gender Gender { get; set; } = Gender.Male;
    }

    public class LoginUserDTO
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";
        
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
        
        public bool RememberMe { get; set; }
    }

    public class UpdateUserDTO
    {
        [Required(ErrorMessage = "Họ và tên không được để trống")]
        public string FullName { get; set; } = "";
        
        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        
        public string? PhoneNumber { get; set; }
        
        public string Address { get; set; } = "";
        
        [Required(ErrorMessage = "Giới tính không được để trống")]
        public Gender Gender { get; set; }
    }
}
