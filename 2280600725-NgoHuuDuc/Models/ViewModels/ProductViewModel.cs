using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace NgoHuuDuc_2280600725.Models.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; } = "";

        [Display(Name = "Mô tả")]
        public string Description { get; set; } = "";

        [Required(ErrorMessage = "Giá sản phẩm không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Giá")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; } = 0;

        [Display(Name = "Ẩn sản phẩm")]
        public bool IsHidden { get; set; } = false;


        [Display(Name = "Hình ảnh")]
        public IFormFile? Image { get; set; }  // Make nullable

        [Display(Name = "Hình ảnh hiện tại")]
        public string? ExistingImageUrl { get; set; }  // Make nullable

        [Display(Name = "Mô hình 3D")]
        public IFormFile? Model3D { get; set; }

        [Display(Name = "Mô hình 3D hiện tại")]
        public string? ExistingModel3DUrl { get; set; }

        [Required(ErrorMessage = "Danh mục không được để trống")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        public IEnumerable<Category> Categories { get; set; } = new List<Category>(); // Ensure initialization
    }
}
