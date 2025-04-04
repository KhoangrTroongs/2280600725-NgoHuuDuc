using NgoHuuDuc_2280600725.Helpers;
using NgoHuuDuc_2280600725.Models;

namespace NgoHuuDuc_2280600725.Responsitories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int? categoryId);
        Task<PaginatedList<Product>> GetProductsByCategoryAsync(int? categoryId, int pageIndex, int pageSize);
        Task<Product> GetProductByIdAsync(int id);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);
        Task<bool> ProductExistsAsync(int id);
        Task<Product> GetProductWithCategoryByIdAsync(int id);
        Task<IEnumerable<Product>> SearchProductsAsync(string keyword);
        Task<PaginatedList<Product>> SearchProductsAsync(string keyword, int pageIndex, int pageSize);
    }
}
