using System.ComponentModel.DataAnnotations;

namespace NgoHuuDuc_2280600725.Models
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ và tên")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        [Display(Name = "Tiêu đề")]
        public string Subject { get; set; } = "";

        [Display(Name = "Sản phẩm quan tâm")]
        public string? ProductInterest { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        [Display(Name = "Nội dung")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Nội dung phải từ 10 đến 1000 ký tự")]
        public string Message { get; set; } = "";
    }
}
