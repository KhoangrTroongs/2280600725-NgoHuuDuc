using NgoHuuDuc_2280600725.DTOs;

namespace NgoHuuDuc_2280600725.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync();
        Task<CategoryDTO?> GetCategoryByIdAsync(int id);
        Task<CategoryDTO> AddCategoryAsync(CreateCategoryDTO categoryDto);
        Task<CategoryDTO?> UpdateCategoryAsync(int id, UpdateCategoryDTO categoryDto);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> CategoryExistsAsync(int id);
    }
}
