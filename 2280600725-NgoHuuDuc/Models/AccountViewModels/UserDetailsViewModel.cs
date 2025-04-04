using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace NgoHuuDuc_2280600725.Models.AccountViewModels
{
    public class UserDetailsViewModel
    {
        public string Id { get; set; } = "";

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = "";

        [Required]
        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Phone]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Địa chỉ")]
        public string Address { get; set; } = "";

        [Display(Name = "Ảnh đại diện")]
        public IFormFile? AvatarFile { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string AvatarUrl { get; set; } = "/images/users/default-avatar.png";

        [Display(Name = "Trạng thái")]
        public bool IsActive { get; set; }

        [Display(Name = "Ngày tạo")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }
    }
}
