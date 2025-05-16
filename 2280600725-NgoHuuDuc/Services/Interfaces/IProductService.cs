using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Helpers;

namespace NgoHuuDuc_2280600725.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDTO>> GetProductsByCategoryAsync(int? categoryId);
        Task<IEnumerable<ProductDTO>> GetProductsByCategoryAsync(int? categoryId, bool includeHidden);

        Task<PaginatedList<ProductDTO>> GetProductsByCategoryAsync(int? categoryId, int pageIndex, int pageSize);
        Task<PaginatedList<ProductDTO>> GetProductsByCategoryAsync(int? categoryId, int pageIndex, int pageSize, bool includeHidden);

        Task<ProductDTO?> GetProductByIdAsync(int id);
        Task<ProductDTO> AddProductAsync(CreateProductDTO productDto, IFormFile? image);
        Task<ProductDTO?> UpdateProductAsync(int id, UpdateProductDTO productDto, IFormFile? image);
        Task<bool> DeleteProductAsync(int id);
        Task<IEnumerable<ProductDTO>> SearchProductsAsync(string keyword);
        Task<IEnumerable<ProductDTO>> SearchProductsAsync(string keyword, bool includeHidden);

        Task<PaginatedList<ProductDTO>> SearchProductsAsync(string keyword, int pageIndex, int pageSize);
        Task<PaginatedList<ProductDTO>> SearchProductsAsync(string keyword, int pageIndex, int pageSize, bool includeHidden);

    }
}
