using System.ComponentModel.DataAnnotations;

namespace NgoHuuDuc_2280600725.DTOs
{
    public class CategoryDTO
    {
        public int Id { get; set; }
        
        public string Name { get; set; } = "";
        
        public string Description { get; set; } = "";
    }

    public class CreateCategoryDTO
    {
        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        public string Name { get; set; } = "";
        
        public string Description { get; set; } = "";
    }

    public class UpdateCategoryDTO
    {
        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        public string Name { get; set; } = "";
        
        public string Description { get; set; } = "";
    }
}
