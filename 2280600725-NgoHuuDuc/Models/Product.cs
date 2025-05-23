﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NgoHuuDuc_2280600725.Models
{
    public class Product
    {
        public Product()
        {
            ProductSizes = new List<ProductSize>();
            ProductReviews = new List<ProductReview>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [StringLength(200, ErrorMessage = "Tên sản phẩm không được vượt quá 200 ký tự")]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; } = "";

        [Display(Name = "Mô tả")]
        public string Description { get; set; } = "";

        [Required(ErrorMessage = "Giá sản phẩm không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Giá")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [Display(Name = "Hình ảnh")]
        public string ImageUrl { get; set; } = "";

        [Display(Name = "Mô hình 3D")]
        public string? Model3DUrl { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; } = 0;

        [Display(Name = "Ẩn sản phẩm")]
        public bool IsHidden { get; set; } = false;

        [Required(ErrorMessage = "Danh mục không được để trống")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        // Navigation properties
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        // New navigation properties
        public virtual ICollection<ProductSize> ProductSizes { get; set; }
        public virtual ICollection<ProductReview> ProductReviews { get; set; }

        // Computed properties
        [NotMapped]
        public double AverageRating => ProductReviews.Any() ? ProductReviews.Average(r => r.Rating) : 0;

        [NotMapped]
        public int ReviewCount => ProductReviews.Count;
    }
}