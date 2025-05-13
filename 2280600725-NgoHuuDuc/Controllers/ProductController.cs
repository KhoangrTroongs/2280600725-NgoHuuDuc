using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
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
                ImageUrl = "/images/products/no-image.svg",
                Model3DUrl = null
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

            if (model.Model3D != null && model.Model3D.Length > 0)
            {
                // Check if the file is a valid 3D model (glb, gltf, obj)
                var validExtensions = new[] { ".glb", ".gltf", ".obj" };
                var extension = Path.GetExtension(model.Model3D.FileName).ToLowerInvariant();

                if (!validExtensions.Contains(extension))
                {
                    ModelState.AddModelError("Model3D", "File phải là mô hình 3D hợp lệ (.glb, .gltf, .obj)");
                    return View(model);
                }

                if (model.Model3D.Length > 20 * 1024 * 1024) // 20MB limit for 3D models
                {
                    ModelState.AddModelError("Model3D", "Kích thước file không được vượt quá 20MB");
                    return View(model);
                }

                product.Model3DUrl = await SaveModel3D(model.Model3D);
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
                ExistingModel3DUrl = product.Model3DUrl,
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

            if (model.Model3D != null && model.Model3D.Length > 0)
            {
                // Check if the file is a valid 3D model (glb, gltf, obj)
                var validExtensions = new[] { ".glb", ".gltf", ".obj" };
                var extension = Path.GetExtension(model.Model3D.FileName).ToLowerInvariant();

                if (!validExtensions.Contains(extension))
                {
                    ModelState.AddModelError("Model3D", "File phải là mô hình 3D hợp lệ (.glb, .gltf, .obj)");
                    model.Categories = await _categoryRepository.GetAllCategoriesAsync();
                    return View(model);
                }

                if (model.Model3D.Length > 20 * 1024 * 1024) // 20MB limit for 3D models
                {
                    ModelState.AddModelError("Model3D", "Kích thước file không được vượt quá 20MB");
                    model.Categories = await _categoryRepository.GetAllCategoriesAsync();
                    return View(model);
                }

                // Delete the old 3D model if it exists
                if (!string.IsNullOrEmpty(product.Model3DUrl))
                {
                    var oldModelPath = Path.Combine(_webHostEnvironment.WebRootPath, product.Model3DUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldModelPath))
                    {
                        System.IO.File.Delete(oldModelPath);
                    }
                }

                // Save the new 3D model
                product.Model3DUrl = await SaveModel3D(model.Model3D);
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

            // Remove associated 3D model file if it exists
            if (!string.IsNullOrEmpty(product.Model3DUrl))
            {
                var modelPath = Path.Combine(_webHostEnvironment.WebRootPath, product.Model3DUrl.TrimStart('/'));
                if (System.IO.File.Exists(modelPath))
                {
                    System.IO.File.Delete(modelPath);
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

        [HttpPost, ActionName("DeleteImage")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                product.ImageUrl = "/images/products/no-image.svg";
                await _productRepository.UpdateProductAsync(product);
                TempData["Success"] = "Hình ảnh sản phẩm đã được thay thế bằng hình ảnh mặc định.";
            }
            else
            {
                TempData["Error"] = "Sản phẩm không có hình ảnh để xóa.";
            }

            return RedirectToAction(nameof(Edit), new { id });
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteImageGet(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                product.ImageUrl = "/images/products/no-image.svg";
                await _productRepository.UpdateProductAsync(product);
                TempData["Success"] = "Hình ảnh sản phẩm đã được thay thế bằng hình ảnh mặc định.";
            }
            else
            {
                TempData["Error"] = "Sản phẩm không có hình ảnh để xóa.";
            }

            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ManageProductSizes(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Tạo danh sách kích cỡ từ mô tả sản phẩm
            var sizes = new List<ProductSizeViewModel>();

            if (!string.IsNullOrEmpty(product.Description))
            {
                var sizeSection = GetSizeSectionFromDescription(product.Description);
                if (!string.IsNullOrEmpty(sizeSection))
                {
                    var sizePairs = sizeSection.Split(',');
                    foreach (var pair in sizePairs)
                    {
                        var parts = pair.Split(':');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int qty))
                        {
                            sizes.Add(new ProductSizeViewModel
                            {
                                Size = parts[0].Trim(),
                                Quantity = qty
                            });
                        }
                    }
                }
            }

            var viewModel = new ManageProductSizesViewModel
            {
                Product = product,
                Sizes = sizes
            };

            return View(viewModel);
        }

        // ViewModel cho kích cỡ sản phẩm
        public class ProductSizeViewModel
        {
            public string Size { get; set; } = "";
            public int Quantity { get; set; }
        }

        // ViewModel cho trang quản lý kích cỡ
        public class ManageProductSizesViewModel
        {
            public Product Product { get; set; } = null!;
            public List<ProductSizeViewModel> Sizes { get; set; } = new List<ProductSizeViewModel>();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AddProductSize(int productId, string size, int quantity)
        {
            if (string.IsNullOrEmpty(size))
            {
                TempData["Error"] = "Kích cỡ không được để trống.";
                return RedirectToAction(nameof(ManageProductSizes), new { id = productId });
            }

            if (quantity < 0)
            {
                TempData["Error"] = "Số lượng phải lớn hơn hoặc bằng 0.";
                return RedirectToAction(nameof(ManageProductSizes), new { id = productId });
            }

            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            // Lưu thông tin kích cỡ vào một trường tạm thời
            // Format: "S:10,M:20,L:30,XL:40"
            var sizeQuantities = new Dictionary<string, int>();

            // Phân tích chuỗi kích cỡ hiện tại (nếu có)
            if (!string.IsNullOrEmpty(product.Description))
            {
                var sizeSection = GetSizeSectionFromDescription(product.Description);
                if (!string.IsNullOrEmpty(sizeSection))
                {
                    var sizePairs = sizeSection.Split(',');
                    foreach (var pair in sizePairs)
                    {
                        var parts = pair.Split(':');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int qty))
                        {
                            sizeQuantities[parts[0].Trim()] = qty;
                        }
                    }
                }
            }

            // Cập nhật hoặc thêm kích cỡ mới
            sizeQuantities[size] = quantity;

            // Tạo chuỗi kích cỡ mới
            var newSizeSection = string.Join(",", sizeQuantities.Select(kv => $"{kv.Key}:{kv.Value}"));

            // Cập nhật mô tả sản phẩm
            var description = product.Description;
            var sizeTag = "[SIZES]";
            var endSizeTag = "[/SIZES]";

            if (description.Contains(sizeTag) && description.Contains(endSizeTag))
            {
                // Cập nhật phần kích cỡ trong mô tả
                var startIndex = description.IndexOf(sizeTag) + sizeTag.Length;
                var endIndex = description.IndexOf(endSizeTag);
                description = description.Substring(0, startIndex) + newSizeSection + description.Substring(endIndex);
            }
            else
            {
                // Thêm phần kích cỡ vào cuối mô tả
                description += $"\n\n{sizeTag}{newSizeSection}{endSizeTag}";
            }

            product.Description = description;

            // Cập nhật tổng số lượng sản phẩm
            product.Quantity = sizeQuantities.Values.Sum();

            await _productRepository.UpdateProductAsync(product);
            TempData["Success"] = $"Đã cập nhật kích cỡ {size} với số lượng {quantity}.";

            return RedirectToAction(nameof(ManageProductSizes), new { id = productId });
        }

        private string GetSizeSectionFromDescription(string description)
        {
            var sizeTag = "[SIZES]";
            var endSizeTag = "[/SIZES]";

            if (description.Contains(sizeTag) && description.Contains(endSizeTag))
            {
                var startIndex = description.IndexOf(sizeTag) + sizeTag.Length;
                var endIndex = description.IndexOf(endSizeTag);
                if (startIndex < endIndex)
                {
                    return description.Substring(startIndex, endIndex - startIndex);
                }
            }

            return string.Empty;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteProductSize(int productId, string size)
        {
            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(size))
            {
                TempData["Error"] = "Kích cỡ không được để trống.";
                return RedirectToAction(nameof(ManageProductSizes), new { id = productId });
            }

            // Lưu thông tin kích cỡ vào một trường tạm thời
            // Format: "S:10,M:20,L:30,XL:40"
            var sizeQuantities = new Dictionary<string, int>();

            // Phân tích chuỗi kích cỡ hiện tại (nếu có)
            if (!string.IsNullOrEmpty(product.Description))
            {
                var sizeSection = GetSizeSectionFromDescription(product.Description);
                if (!string.IsNullOrEmpty(sizeSection))
                {
                    var sizePairs = sizeSection.Split(',');
                    foreach (var pair in sizePairs)
                    {
                        var parts = pair.Split(':');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int qty))
                        {
                            sizeQuantities[parts[0].Trim()] = qty;
                        }
                    }
                }
            }

            // Xóa kích cỡ
            if (sizeQuantities.ContainsKey(size))
            {
                sizeQuantities.Remove(size);

                // Tạo chuỗi kích cỡ mới
                var newSizeSection = string.Join(",", sizeQuantities.Select(kv => $"{kv.Key}:{kv.Value}"));

                // Cập nhật mô tả sản phẩm
                var description = product.Description;
                var sizeTag = "[SIZES]";
                var endSizeTag = "[/SIZES]";

                if (description.Contains(sizeTag) && description.Contains(endSizeTag))
                {
                    // Cập nhật phần kích cỡ trong mô tả
                    var startIndex = description.IndexOf(sizeTag) + sizeTag.Length;
                    var endIndex = description.IndexOf(endSizeTag);
                    description = description.Substring(0, startIndex) + newSizeSection + description.Substring(endIndex);
                }

                product.Description = description;

                // Cập nhật tổng số lượng sản phẩm
                product.Quantity = sizeQuantities.Values.Sum();

                await _productRepository.UpdateProductAsync(product);
                TempData["Success"] = $"Đã xóa kích cỡ {size}.";
            }
            else
            {
                TempData["Error"] = $"Không tìm thấy kích cỡ {size}.";
            }

            return RedirectToAction(nameof(ManageProductSizes), new { id = productId });
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteModel3DGet(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(product.Model3DUrl))
            {
                var modelPath = Path.Combine(_webHostEnvironment.WebRootPath, product.Model3DUrl.TrimStart('/'));
                if (System.IO.File.Exists(modelPath))
                {
                    System.IO.File.Delete(modelPath);
                }

                product.Model3DUrl = null;
                await _productRepository.UpdateProductAsync(product);
                TempData["Success"] = "Mô hình 3D của sản phẩm đã được xóa thành công.";
            }
            else
            {
                TempData["Error"] = "Sản phẩm không có mô hình 3D để xóa.";
            }

            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddReview(int productId, int Rating, string Comment)
        {
            if (Rating < 1 || Rating > 5)
            {
                TempData["Error"] = "Đánh giá phải từ 1 đến 5 sao.";
                return RedirectToAction(nameof(Details), new { id = productId });
            }

            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity.Name ?? "Người dùng ẩn danh";

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Lưu đánh giá vào mô tả sản phẩm
            // Format: "userId:userName:rating:date:comment,userId2:userName2:rating2:date2:comment2"
            var reviews = new List<ReviewData>();

            // Phân tích chuỗi đánh giá hiện tại (nếu có)
            if (!string.IsNullOrEmpty(product.Description))
            {
                var reviewSection = GetReviewSectionFromDescription(product.Description);
                if (!string.IsNullOrEmpty(reviewSection))
                {
                    var reviewPairs = reviewSection.Split('|');
                    foreach (var pair in reviewPairs)
                    {
                        if (!string.IsNullOrEmpty(pair))
                        {
                            var parts = pair.Split(new[] { "~~" }, StringSplitOptions.None);
                            if (parts.Length >= 5)
                            {
                                reviews.Add(new ReviewData
                                {
                                    UserId = parts[0],
                                    UserName = parts[1],
                                    Rating = int.TryParse(parts[2], out int r) ? r : 5,
                                    Date = DateTime.TryParse(parts[3], out DateTime d) ? d : DateTime.Now,
                                    Comment = parts[4]
                                });
                            }
                        }
                    }
                }
            }

            // Kiểm tra xem người dùng đã đánh giá sản phẩm này chưa
            var existingReview = reviews.FirstOrDefault(r => r.UserId == userId);
            if (existingReview != null)
            {
                // Cập nhật đánh giá hiện có
                existingReview.Rating = Rating;
                existingReview.Comment = Comment;
                existingReview.Date = DateTime.Now;
            }
            else
            {
                // Tạo đánh giá mới
                reviews.Add(new ReviewData
                {
                    UserId = userId,
                    UserName = userName,
                    Rating = Rating,
                    Comment = Comment,
                    Date = DateTime.Now
                });
            }

            // Tạo chuỗi đánh giá mới
            var newReviewSection = string.Join("|", reviews.Select(r =>
                $"{r.UserId}~~{r.UserName}~~{r.Rating}~~{r.Date:yyyy-MM-dd HH:mm:ss}~~{r.Comment}"));

            // Cập nhật mô tả sản phẩm
            var description = product.Description;
            var reviewTag = "[REVIEWS]";
            var endReviewTag = "[/REVIEWS]";

            if (description.Contains(reviewTag) && description.Contains(endReviewTag))
            {
                // Cập nhật phần đánh giá trong mô tả
                var startIndex = description.IndexOf(reviewTag) + reviewTag.Length;
                var endIndex = description.IndexOf(endReviewTag);
                description = description.Substring(0, startIndex) + newReviewSection + description.Substring(endIndex);
            }
            else
            {
                // Thêm phần đánh giá vào cuối mô tả
                description += $"\n\n{reviewTag}{newReviewSection}{endReviewTag}";
            }

            product.Description = description;

            await _productRepository.UpdateProductAsync(product);
            TempData["Success"] = "Cảm ơn bạn đã đánh giá sản phẩm!";
            return RedirectToAction(nameof(Details), new { id = productId });
        }

        private string GetReviewSectionFromDescription(string description)
        {
            var reviewTag = "[REVIEWS]";
            var endReviewTag = "[/REVIEWS]";

            if (description.Contains(reviewTag) && description.Contains(endReviewTag))
            {
                var startIndex = description.IndexOf(reviewTag) + reviewTag.Length;
                var endIndex = description.IndexOf(endReviewTag);
                if (startIndex < endIndex)
                {
                    return description.Substring(startIndex, endIndex - startIndex);
                }
            }

            return string.Empty;
        }

        // Lớp để lưu trữ thông tin đánh giá
        private class ReviewData
        {
            public string UserId { get; set; } = "";
            public string UserName { get; set; } = "";
            public int Rating { get; set; }
            public DateTime Date { get; set; }
            public string Comment { get; set; } = "";
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteReview(int productId, string userId)
        {
            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            // Lấy danh sách đánh giá từ mô tả sản phẩm
            var reviews = new List<ReviewData>();

            if (!string.IsNullOrEmpty(product.Description))
            {
                var reviewSection = GetReviewSectionFromDescription(product.Description);
                if (!string.IsNullOrEmpty(reviewSection))
                {
                    var reviewPairs = reviewSection.Split('|');
                    foreach (var pair in reviewPairs)
                    {
                        if (!string.IsNullOrEmpty(pair))
                        {
                            var parts = pair.Split(new[] { "~~" }, StringSplitOptions.None);
                            if (parts.Length >= 5)
                            {
                                reviews.Add(new ReviewData
                                {
                                    UserId = parts[0],
                                    UserName = parts[1],
                                    Rating = int.TryParse(parts[2], out int r) ? r : 5,
                                    Date = DateTime.TryParse(parts[3], out DateTime d) ? d : DateTime.Now,
                                    Comment = parts[4]
                                });
                            }
                        }
                    }
                }
            }

            // Xóa đánh giá của người dùng
            var reviewToRemove = reviews.FirstOrDefault(r => r.UserId == userId);
            if (reviewToRemove != null)
            {
                reviews.Remove(reviewToRemove);

                // Tạo chuỗi đánh giá mới
                var newReviewSection = string.Join("|", reviews.Select(r =>
                    $"{r.UserId}~~{r.UserName}~~{r.Rating}~~{r.Date:yyyy-MM-dd HH:mm:ss}~~{r.Comment}"));

                // Cập nhật mô tả sản phẩm
                var description = product.Description;
                var reviewTag = "[REVIEWS]";
                var endReviewTag = "[/REVIEWS]";

                if (description.Contains(reviewTag) && description.Contains(endReviewTag))
                {
                    // Cập nhật phần đánh giá trong mô tả
                    var startIndex = description.IndexOf(reviewTag) + reviewTag.Length;
                    var endIndex = description.IndexOf(endReviewTag);
                    description = description.Substring(0, startIndex) + newReviewSection + description.Substring(endIndex);

                    product.Description = description;
                    await _productRepository.UpdateProductAsync(product);

                    TempData["Success"] = "Đã xóa đánh giá thành công.";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy phần đánh giá trong mô tả sản phẩm.";
                }
            }
            else
            {
                TempData["Error"] = "Không tìm thấy đánh giá cần xóa.";
            }

            return RedirectToAction(nameof(Details), new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteModel3D(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(product.Model3DUrl))
            {
                var modelPath = Path.Combine(_webHostEnvironment.WebRootPath, product.Model3DUrl.TrimStart('/'));
                if (System.IO.File.Exists(modelPath))
                {
                    System.IO.File.Delete(modelPath);
                }

                product.Model3DUrl = null;
                await _productRepository.UpdateProductAsync(product);
                TempData["Success"] = "Mô hình 3D của sản phẩm đã được xóa thành công.";
            }
            else
            {
                TempData["Error"] = "Sản phẩm không có mô hình 3D để xóa.";
            }

            return RedirectToAction(nameof(Edit), new { id });
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

        private async Task<string> SaveModel3D(IFormFile model)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "models/products");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.CopyToAsync(fileStream);
            }

            return "/models/products/" + uniqueFileName;
        }
    }
}
