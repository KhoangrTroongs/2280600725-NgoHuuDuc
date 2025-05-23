using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Helpers;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Responsitories;
using NgoHuuDuc_2280600725.Services.Interfaces;

namespace NgoHuuDuc_2280600725.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IWebHostEnvironment webHostEnvironment,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductDTO>> GetProductsByCategoryAsync(int? categoryId)
        {
            var products = await _productRepository.GetProductsByCategoryAsync(categoryId);
            return products.Select(MapToProductDTO);
        }

        public async Task<IEnumerable<ProductDTO>> GetProductsByCategoryAsync(int? categoryId, bool includeHidden)
        {
            var products = await _productRepository.GetProductsByCategoryAsync(categoryId);

            // Nếu không bao gồm sản phẩm ẩn, lọc ra các sản phẩm không bị ẩn
            if (!includeHidden)
            {
                products = products.Where(p => !p.IsHidden);
            }

            return products.Select(MapToProductDTO);
        }



        public async Task<PaginatedList<ProductDTO>> GetProductsByCategoryAsync(int? categoryId, int pageIndex, int pageSize)
        {
            var products = await _productRepository.GetProductsByCategoryAsync(categoryId, pageIndex, pageSize);
            var productDtos = products.Select(MapToProductDTO).ToList();

            return new PaginatedList<ProductDTO>(
                productDtos,
                products.TotalItems,
                products.PageIndex,
                products.PageSize);
        }

        public async Task<PaginatedList<ProductDTO>> GetProductsByCategoryAsync(int? categoryId, int pageIndex, int pageSize, bool includeHidden)
        {
            var products = await _productRepository.GetProductsByCategoryAsync(categoryId, pageIndex, pageSize);
            var productList = products.ToList();

            // Nếu không bao gồm sản phẩm ẩn, lọc ra các sản phẩm không bị ẩn
            if (!includeHidden)
            {
                productList = productList.Where(p => !p.IsHidden).ToList();
            }

            var productDtos = productList.Select(MapToProductDTO).ToList();

            return new PaginatedList<ProductDTO>(
                productDtos,
                includeHidden ? products.TotalItems : productList.Count,
                products.PageIndex,
                products.PageSize);
        }



        public async Task<ProductDTO?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            return product != null ? MapToProductDTO(product) : null;
        }

        public async Task<ProductDTO> AddProductAsync(CreateProductDTO productDto, IFormFile? image)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                Quantity = productDto.Quantity,

                CategoryId = productDto.CategoryId
            };

            if (image != null && image.Length > 0)
            {
                product.ImageUrl = await SaveImage(image);
            }

            await _productRepository.AddProductAsync(product);

            // Fetch the product with category to return complete DTO
            var createdProduct = await _productRepository.GetProductByIdAsync(product.Id);
            return MapToProductDTO(createdProduct);
        }

        public async Task<ProductDTO?> UpdateProductAsync(int id, UpdateProductDTO productDto, IFormFile? image)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            {
                return null;
            }

            product.Name = productDto.Name;
            product.Description = productDto.Description;
            product.Price = productDto.Price;
            product.Quantity = productDto.Quantity;

            product.CategoryId = productDto.CategoryId;

            if (image != null && image.Length > 0)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    DeleteImage(product.ImageUrl);
                }

                // Save new image
                product.ImageUrl = await SaveImage(image);
            }

            await _productRepository.UpdateProductAsync(product);

            // Fetch updated product with category
            var updatedProduct = await _productRepository.GetProductByIdAsync(id);
            return MapToProductDTO(updatedProduct);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            {
                return false;
            }

            // Delete image if exists
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                DeleteImage(product.ImageUrl);
            }

            await _productRepository.DeleteProductAsync(id);
            return true;
        }

        public async Task<IEnumerable<ProductDTO>> SearchProductsAsync(string keyword)
        {
            var products = await _productRepository.SearchProductsAsync(keyword);
            return products.Select(MapToProductDTO);
        }

        public async Task<IEnumerable<ProductDTO>> SearchProductsAsync(string keyword, bool includeHidden)
        {
            var products = await _productRepository.SearchProductsAsync(keyword);

            // Nếu không bao gồm sản phẩm ẩn, lọc ra các sản phẩm không bị ẩn
            if (!includeHidden)
            {
                products = products.Where(p => !p.IsHidden);
            }

            return products.Select(MapToProductDTO);
        }



        public async Task<PaginatedList<ProductDTO>> SearchProductsAsync(string keyword, int pageIndex, int pageSize)
        {
            var products = await _productRepository.SearchProductsAsync(keyword, pageIndex, pageSize);
            var productDtos = products.Select(MapToProductDTO).ToList();

            return new PaginatedList<ProductDTO>(
                productDtos,
                products.TotalItems,
                products.PageIndex,
                products.PageSize);
        }

        public async Task<PaginatedList<ProductDTO>> SearchProductsAsync(string keyword, int pageIndex, int pageSize, bool includeHidden)
        {
            var products = await _productRepository.SearchProductsAsync(keyword, pageIndex, pageSize);
            var productList = products.ToList();

            // Nếu không bao gồm sản phẩm ẩn, lọc ra các sản phẩm không bị ẩn
            if (!includeHidden)
            {
                productList = productList.Where(p => !p.IsHidden).ToList();
            }

            var productDtos = productList.Select(MapToProductDTO).ToList();

            return new PaginatedList<ProductDTO>(
                productDtos,
                includeHidden ? products.TotalItems : productList.Count,
                products.PageIndex,
                products.PageSize);
        }



        private async Task<string> SaveImage(IFormFile image)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return "/images/products/" + uniqueFileName;
        }

        private void DeleteImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return;

            var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }
        }

        private ProductDTO MapToProductDTO(Product product)
        {
            var description = product.Description;
            var sizeTag = "[SIZES]";
            var endSizeTag = "[/SIZES]";
            var reviewTag = "[REVIEWS]";
            var endReviewTag = "[/REVIEWS]";

            // Loại bỏ phần kỹ thuật khỏi mô tả cho API
            if (!string.IsNullOrEmpty(description))
            {
                // Loại bỏ phần kích thước
                if (description.Contains(sizeTag) && description.Contains(endSizeTag))
                {
                    var startIndex = description.IndexOf(sizeTag);
                    var endIndex = description.IndexOf(endSizeTag) + endSizeTag.Length;
                    if (startIndex < endIndex)
                    {
                        description = description.Remove(startIndex, endIndex - startIndex);
                    }
                }

                // Loại bỏ phần đánh giá
                if (description.Contains(reviewTag) && description.Contains(endReviewTag))
                {
                    var startIndex = description.IndexOf(reviewTag);
                    var endIndex = description.IndexOf(endReviewTag) + endReviewTag.Length;
                    if (startIndex < endIndex)
                    {
                        description = description.Remove(startIndex, endIndex - startIndex);
                    }
                }

                // Loại bỏ khoảng trắng thừa
                description = description.Trim();
            }

            return new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Description = description, // Sử dụng mô tả đã được xử lý
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                Quantity = product.Quantity,

                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name
            };
        }
    }
}
