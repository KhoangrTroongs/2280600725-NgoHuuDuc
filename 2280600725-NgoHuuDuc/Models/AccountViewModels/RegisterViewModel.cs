using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace NgoHuuDuc_2280600725.Models.AccountViewModels
{
    public enum Gender
    {
        [Display(Name = "Nam")]
        Male,
        [Display(Name = "Nữ")]
        Female,
        [Display(Name = "Khác")]
        Other
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, ErrorMessage = "{0} phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Giới tính không được để trống")]
        [Display(Name = "Giới tính")]
        public Gender Gender { get; set; }

        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public IFormFile AvatarFile { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string AvatarUrl { get; set; } = "/images/users/default-avatar.png";
    }
}