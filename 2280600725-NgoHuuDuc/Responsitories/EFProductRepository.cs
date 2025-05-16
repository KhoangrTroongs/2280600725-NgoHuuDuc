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
            try
            {
                // Lấy sản phẩm hiện tại từ cơ sở dữ liệu
                var existingProduct = await _context.Products.FindAsync(product.Id);
                if (existingProduct == null)
                {
                    throw new Exception($"Product with ID {product.Id} not found");
                }

                // Cập nhật các thuộc tính cơ bản
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.Quantity = product.Quantity;

                existingProduct.CategoryId = product.CategoryId;
                existingProduct.ImageUrl = product.ImageUrl;
                existingProduct.Model3DUrl = product.Model3DUrl;

                // Lưu thay đổi
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log lỗi và ném lại ngoại lệ
                Console.WriteLine($"Error updating product: {ex.Message}");
                throw;
            }
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

        public async Task<PaginatedList<Product>> GetProductsByCategoryAsync(int? categoryId, int pageIndex, int pageSize, bool includeHidden)
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!includeHidden)
            {
                query = query.Where(p => !p.IsHidden);
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

        public async Task<PaginatedList<Product>> SearchProductsAsync(string keyword, int pageIndex, int pageSize, bool includeHidden)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return await GetProductsByCategoryAsync(null, pageIndex, pageSize, includeHidden);

            keyword = keyword.ToLower();
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.Name.ToLower().Contains(keyword) ||
                           p.Description.ToLower().Contains(keyword) ||
                           p.Category.Name.ToLower().Contains(keyword));

            if (!includeHidden)
            {
                query = query.Where(p => !p.IsHidden);
            }

            return await PaginatedList<Product>.CreateAsync(query, pageIndex, pageSize);
        }


    }
}
