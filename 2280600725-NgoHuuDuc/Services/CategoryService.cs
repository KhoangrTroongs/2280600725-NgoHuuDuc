using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Responsitories;
using NgoHuuDuc_2280600725.Services.Interfaces;

namespace NgoHuuDuc_2280600725.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            ICategoryRepository categoryRepository,
            ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            return categories.Select(MapToCategoryDTO);
        }

        public async Task<CategoryDTO?> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            return category != null ? MapToCategoryDTO(category) : null;
        }

        public async Task<CategoryDTO> AddCategoryAsync(CreateCategoryDTO categoryDto)
        {
            // Check if category with same name already exists
            var existingCategory = await _categoryRepository.GetCategoryByNameAsync(categoryDto.Name);
            if (existingCategory != null)
            {
                throw new InvalidOperationException($"Danh mục với tên '{categoryDto.Name}' đã tồn tại.");
            }

            var category = new Category
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description
            };

            await _categoryRepository.AddCategoryAsync(category);
            
            // Fetch the newly created category to return complete DTO
            var createdCategory = await _categoryRepository.GetCategoryByIdAsync(category.Id);
            return MapToCategoryDTO(createdCategory);
        }

        public async Task<CategoryDTO?> UpdateCategoryAsync(int id, UpdateCategoryDTO categoryDto)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return null;
            }

            // Check if another category with the same name exists
            var existingCategory = await _categoryRepository.GetCategoryByNameAsync(categoryDto.Name, id);
            if (existingCategory != null)
            {
                throw new InvalidOperationException($"Danh mục khác với tên '{categoryDto.Name}' đã tồn tại.");
            }

            category.Name = categoryDto.Name;
            category.Description = categoryDto.Description;

            await _categoryRepository.UpdateCategoryAsync(category);
            
            // Fetch updated category
            var updatedCategory = await _categoryRepository.GetCategoryByIdAsync(id);
            return MapToCategoryDTO(updatedCategory);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return false;
            }

            await _categoryRepository.DeleteCategoryAsync(id);
            return true;
        }

        public async Task<bool> CategoryExistsAsync(int id)
        {
            return await _categoryRepository.CategoryExistsAsync(id);
        }

        private CategoryDTO MapToCategoryDTO(Category category)
        {
            return new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }
    }
}
