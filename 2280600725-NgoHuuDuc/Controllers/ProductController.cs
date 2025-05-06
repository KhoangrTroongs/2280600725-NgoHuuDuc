using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NgoHuuDuc_2280600725.Data;
using NgoHuuDuc_2280600725.Helpers;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Models.ViewModels;
using NgoHuuDuc_2280600725.Responsitories;

namespace NgoHuuDuc_2280600725.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IWebHostEnvironment webHostEnvironment,
            ILogger<ProductController> logger)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? categoryId, int pageNumber = 1)
        {
            const int pageSize = 12; // 12 sản phẩm mỗi trang

            try
            {
                var products = await _productRepository.GetProductsByCategoryAsync(categoryId, pageNumber, pageSize);
                if (products == null || products.Count == 0)
                {
                    _logger.LogWarning("Không tìm thấy sản phẩm nào.");
                    TempData["Error"] = "Không tìm thấy sản phẩm.";
                }

                ViewBag.Categories = await _categoryRepository.GetAllCategoriesAsync();
                ViewBag.CategoryId = categoryId;
                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = products.TotalPages;
                ViewBag.HasPreviousPage = products.HasPreviousPage;
                ViewBag.HasNextPage = products.HasNextPage;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = products.TotalItems;

                if (categoryId.HasValue)
                {
                    var category = await _categoryRepository.GetCategoryByIdAsync(categoryId.Value);
                    ViewBag.SelectedCategory = category;
                }

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách sản phẩm");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách sản phẩm.";
                return View(null);
                // return RedirectToAction("Index");
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> Search(string keyword, int pageNumber = 1)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return RedirectToAction("Index");
            }

            const int pageSize = 12; // 12 sản phẩm mỗi trang

            try
            {
                var products = await _productRepository.SearchProductsAsync(keyword, pageNumber, pageSize);
                ViewBag.SearchKeyword = keyword;
                ViewBag.ResultCount = products.TotalItems;
                ViewBag.Categories = await _categoryRepository.GetAllCategoriesAsync();
                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = products.TotalPages;
                ViewBag.HasPreviousPage = products.HasPreviousPage;
                ViewBag.HasNextPage = products.HasNextPage;
                ViewBag.PageSize = pageSize;

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm sản phẩm với từ khóa: {Keyword}", keyword);
                TempData["Error"] = "Có lỗi xảy ra khi tìm kiếm. Vui lòng thử lại sau.";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            if (categories == null || !categories.Any())
            {
                TempData["Error"] = "Không có danh mục nào. Vui lòng tạo danh mục trước.";
                return RedirectToAction("Index");
            }

            return View(new ProductViewModel { Categories = categories });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            model.Categories = await _categoryRepository.GetAllCategoriesAsync();

            if (model.CategoryId == 0 || !await _categoryRepository.CategoryExistsAsync(model.CategoryId))
            {
                ModelState.AddModelError("CategoryId", "Danh mục không tồn tại");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                Quantity = model.Quantity,
                CategoryId = model.CategoryId,
                ImageUrl = null
            };

            if (model.Image != null && model.Image.Length > 0)
            {
                if (!model.Image.ContentType.StartsWith("image/"))
                {
                    ModelState.AddModelError("Image", "File phải là hình ảnh");
                    return View(model);
                }
                if (model.Image.Length > 5 * 1024 * 1024) // Increase limit to 5MB
                {
                    ModelState.AddModelError("Image", "Kích thước file không được vượt quá 5MB");
                    return View(model);
                }

                product.ImageUrl = await SaveImage(model.Image);
            }

            try
            {
                await _productRepository.AddProductAsync(product);
                TempData["Success"] = "Sản phẩm đã được thêm thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving the product. Details: {Message}", ex.Message);
                ModelState.AddModelError("", "Đã xảy ra lỗi khi lưu sản phẩm. Vui lòng thử lại.");
                return View(model);
            }
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productRepository.GetProductByIdAsync(id.Value);
            if (product == null)
            {
                _logger.LogWarning("Không tìm thấy sản phẩm với ID: {Id}", id);
                return NotFound();
            }

            var model = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Quantity = product.Quantity,
                CategoryId = product.CategoryId,
                ExistingImageUrl = product.ImageUrl,
                Categories = await _categoryRepository.GetAllCategoriesAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, ProductViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.Categories = await _categoryRepository.GetAllCategoriesAsync();
                return View(model);
            }

            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Không tìm thấy sản phẩm với ID: {Id}", id);
                return NotFound();
            }

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.Quantity = model.Quantity;
            product.CategoryId = model.CategoryId;

            if (model.Image != null && model.Image.Length > 0)
            {
                if (!model.Image.ContentType.StartsWith("image/"))
                {
                    ModelState.AddModelError("Image", "File phải là hình ảnh");
                    model.Categories = await _categoryRepository.GetAllCategoriesAsync();
                    return View(model);
                }
                if (model.Image.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("Image", "Kích thước file không được vượt quá 5MB");
                    model.Categories = await _categoryRepository.GetAllCategoriesAsync();
                    return View(model);
                }

                // Delete the old image if it exists
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Save the new image
                product.ImageUrl = await SaveImage(model.Image);
            }

            try
            {
                await _productRepository.UpdateProductAsync(product);
                TempData["Success"] = "Sản phẩm đã được cập nhật thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _productRepository.ProductExistsAsync(product.Id))
                {
                    return NotFound();
                }
                else
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật sản phẩm.");
                }
            }

            model.Categories = await _categoryRepository.GetAllCategoriesAsync();
            return View(model);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productRepository.GetProductWithCategoryByIdAsync(id.Value);

            if (product == null)
            {
                _logger.LogWarning("Không tìm thấy sản phẩm với ID: {Id}", id);
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Remove associated image file if it exists
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            await _productRepository.DeleteProductAsync(product.Id);
            TempData["Success"] = "Sản phẩm đã được xóa thành công.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productRepository.GetProductWithCategoryByIdAsync(id.Value);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
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
    }
}
