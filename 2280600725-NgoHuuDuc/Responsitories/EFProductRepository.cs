using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Data;
using NgoHuuDuc_2280600725.Helpers;
using NgoHuuDuc_2280600725.Models;

namespace NgoHuuDuc_2280600725.Responsitories
{
    public class EFProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public EFProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int? categoryId)
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }
            return await query.ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProductAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ProductExistsAsync(int id) // Fix: Implement ProductExistsAsync
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }

        public async Task<Product> GetProductWithCategoryByIdAsync(int id) // Fix: Implement GetProductWithCategoryByIdAsync
        {
            return await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return await GetProductsByCategoryAsync(null);

            keyword = keyword.ToLower();
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Name.ToLower().Contains(keyword) ||
                           p.Description.ToLower().Contains(keyword) ||
                           p.Category.Name.ToLower().Contains(keyword))
                .ToListAsync();
        }

        public async Task<PaginatedList<Product>> GetProductsByCategoryAsync(int? categoryId, int pageIndex, int pageSize)
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }
            return await PaginatedList<Product>.CreateAsync(query, pageIndex, pageSize);
        }

        public async Task<PaginatedList<Product>> SearchProductsAsync(string keyword, int pageIndex, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return await GetProductsByCategoryAsync(null, pageIndex, pageSize);

            keyword = keyword.ToLower();
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.Name.ToLower().Contains(keyword) ||
                           p.Description.ToLower().Contains(keyword) ||
                           p.Category.Name.ToLower().Contains(keyword));

            return await PaginatedList<Product>.CreateAsync(query, pageIndex, pageSize);
        }
    }
}
