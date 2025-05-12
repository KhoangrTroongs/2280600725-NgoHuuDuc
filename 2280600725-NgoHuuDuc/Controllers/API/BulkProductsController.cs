using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Data;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Responsitories;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// Cấu hình EPPlus được đặt trong Program.cs

namespace NgoHuuDuc_2280600725.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrator")]
    public class BulkProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BulkProductsController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BulkProductsController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            ApplicationDbContext context,
            ILogger<BulkProductsController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _context = context;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        // POST: api/BulkProducts/delete
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteBulk([FromBody] int[] ids)
        {
            try
            {
                if (ids == null || ids.Length == 0)
                {
                    return BadRequest(new { success = false, message = "Không có sản phẩm nào được chọn" });
                }

                int deletedCount = 0;
                List<string> errors = new List<string>();

                foreach (var id in ids)
                {
                    try
                    {
                        var product = await _context.Products.FindAsync(id);
                        if (product != null)
                        {
                            // Xóa file hình ảnh nếu có
                            if (!string.IsNullOrEmpty(product.ImageUrl))
                            {
                                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                                if (System.IO.File.Exists(imagePath))
                                {
                                    System.IO.File.Delete(imagePath);
                                }
                            }

                            // Xóa file model 3D nếu có
                            if (!string.IsNullOrEmpty(product.Model3DUrl))
                            {
                                var modelPath = Path.Combine(_webHostEnvironment.WebRootPath, product.Model3DUrl.TrimStart('/'));
                                if (System.IO.File.Exists(modelPath))
                                {
                                    System.IO.File.Delete(modelPath);
                                }
                            }

                            _context.Products.Remove(product);
                            deletedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Lỗi khi xóa sản phẩm ID {id}: {ex.Message}");
                        _logger.LogError(ex, "Lỗi khi xóa sản phẩm ID {Id}", id);
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Đã xóa {deletedCount} sản phẩm thành công",
                    errors = errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa hàng loạt sản phẩm");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi xóa sản phẩm" });
            }
        }

        // POST: api/BulkProducts/updateStock
        [HttpPost("updateStock")]
        public async Task<IActionResult> UpdateStock([FromBody] UpdateStockModel model)
        {
            try
            {
                if (model.Ids == null || model.Ids.Length == 0)
                {
                    return BadRequest(new { success = false, message = "Không có sản phẩm nào được chọn" });
                }

                if (model.Quantity < 0)
                {
                    return BadRequest(new { success = false, message = "Số lượng không được nhỏ hơn 0" });
                }

                int updatedCount = 0;
                List<string> errors = new List<string>();

                foreach (var id in model.Ids)
                {
                    try
                    {
                        var product = await _context.Products.FindAsync(id);
                        if (product != null)
                        {
                            product.Quantity = model.Quantity;
                            _context.Products.Update(product);
                            updatedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Lỗi khi cập nhật sản phẩm ID {id}: {ex.Message}");
                        _logger.LogError(ex, "Lỗi khi cập nhật tồn kho sản phẩm ID {Id}", id);
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Đã cập nhật tồn kho cho {updatedCount} sản phẩm thành công",
                    errors = errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật tồn kho hàng loạt");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi cập nhật tồn kho" });
            }
        }

        // GET: api/BulkProducts/export
        [HttpGet("export")]
        public async Task<IActionResult> ExportToExcel([FromQuery] int[] ids)
        {
            try
            {
                if (ids == null || ids.Length == 0)
                {
                    // Lấy tất cả sản phẩm nếu không có ID nào được chọn
                    ids = await _context.Products.Select(p => p.Id).ToArrayAsync();

                    if (ids.Length == 0)
                    {
                        return BadRequest(new { success = false, message = "Không có sản phẩm nào trong hệ thống" });
                    }
                }

                // Lấy danh sách sản phẩm theo ID
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => ids.Contains(p.Id))
                    .ToListAsync();

                if (products == null || products.Count == 0)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy sản phẩm nào" });
                }

                // Tạo file Excel bằng cách đơn giản hóa
                var fileName = $"Products_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var filePath = Path.Combine(Path.GetTempPath(), fileName);

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Products");

                    // Thiết lập header đơn giản
                    string[] headers = new string[] { "ID", "Tên sản phẩm", "Danh mục", "Giá", "Tồn kho", "Mô tả" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = headers[i];
                        worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    }

                    // Điền dữ liệu
                    for (int i = 0; i < products.Count; i++)
                    {
                        var product = products[i];
                        int row = i + 2;

                        worksheet.Cells[row, 1].Value = product.Id;
                        worksheet.Cells[row, 2].Value = product.Name;
                        worksheet.Cells[row, 3].Value = product.Category?.Name;
                        worksheet.Cells[row, 4].Value = product.Price;
                        worksheet.Cells[row, 5].Value = product.Quantity;
                        worksheet.Cells[row, 6].Value = product.Description;
                    }

                    // Lưu file
                    var fileBytes = package.GetAsByteArray();
                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xuất Excel: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = $"Có lỗi xảy ra khi xuất Excel: {ex.Message}" });
            }
        }

        // POST: api/BulkProducts/quickEdit
        [HttpPost("quickEdit")]
        public async Task<IActionResult> QuickEdit([FromBody] QuickEditModel model)
        {
            try
            {
                if (model.Id <= 0)
                {
                    return BadRequest(new { success = false, message = "ID sản phẩm không hợp lệ" });
                }

                var product = await _context.Products.FindAsync(model.Id);
                if (product == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy sản phẩm" });
                }

                // Cập nhật thông tin sản phẩm
                product.Name = model.Name;
                product.Price = model.Price;
                product.Quantity = model.Quantity;

                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật sản phẩm thành công",
                    product = new
                    {
                        product.Id,
                        product.Name,
                        product.Price,
                        product.Quantity
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi sửa nhanh sản phẩm ID {Id}", model.Id);
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi cập nhật sản phẩm" });
            }
        }



        // POST: api/BulkProducts/import
        [HttpPost("import")]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                return BadRequest(new { success = false, message = "Vui lòng chọn file Excel" });
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { success = false, message = "Chỉ hỗ trợ file Excel (.xlsx)" });
            }

            var result = new ImportResult();

            try
            {
                // Lấy danh sách danh mục
                var categories = await _context.Categories.ToListAsync();

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    using (var package = new ExcelPackage(stream))
                    {
                        // Lấy worksheet đầu tiên
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            return BadRequest(new { success = false, message = "File Excel không có dữ liệu" });
                        }

                        // Đếm số dòng có dữ liệu
                        int rowCount = worksheet.Dimension?.Rows ?? 0;
                        if (rowCount <= 1) // Chỉ có header hoặc không có dữ liệu
                        {
                            return BadRequest(new { success = false, message = "File Excel không có dữ liệu sản phẩm" });
                        }

                        result.TotalRows = rowCount - 1; // Trừ đi hàng header

                        // Đọc dữ liệu từ Excel
                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                // Đọc dữ liệu từ các ô
                                var name = worksheet.Cells[row, 1].Value?.ToString();
                                var categoryName = worksheet.Cells[row, 2].Value?.ToString();
                                var priceValue = worksheet.Cells[row, 3].Value;
                                var quantityValue = worksheet.Cells[row, 4].Value;
                                var description = worksheet.Cells[row, 5].Value?.ToString();

                                // Kiểm tra dữ liệu bắt buộc
                                if (string.IsNullOrWhiteSpace(name))
                                {
                                    result.Errors.Add($"Dòng {row}: Tên sản phẩm không được để trống");
                                    result.ErrorCount++;
                                    continue;
                                }

                                // Tìm danh mục
                                if (string.IsNullOrWhiteSpace(categoryName))
                                {
                                    result.Errors.Add($"Dòng {row}: Danh mục không được để trống");
                                    result.ErrorCount++;
                                    continue;
                                }

                                var category = categories.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
                                if (category == null)
                                {
                                    result.Errors.Add($"Dòng {row}: Danh mục '{categoryName}' không tồn tại");
                                    result.ErrorCount++;
                                    continue;
                                }

                                // Chuyển đổi giá
                                decimal price;
                                if (priceValue == null || !decimal.TryParse(priceValue.ToString(), out price) || price < 0)
                                {
                                    result.Errors.Add($"Dòng {row}: Giá không hợp lệ");
                                    result.ErrorCount++;
                                    continue;
                                }

                                // Chuyển đổi số lượng
                                int quantity;
                                if (quantityValue == null || !int.TryParse(quantityValue.ToString(), out quantity) || quantity < 0)
                                {
                                    result.Errors.Add($"Dòng {row}: Số lượng không hợp lệ");
                                    result.ErrorCount++;
                                    continue;
                                }

                                // Tạo sản phẩm mới
                                var product = new Product
                                {
                                    Name = name,
                                    CategoryId = category.Id,
                                    Price = price,
                                    Quantity = quantity,
                                    Description = description ?? ""
                                };

                                _context.Products.Add(product);
                                result.SuccessCount++;
                            }
                            catch (Exception ex)
                            {
                                result.Errors.Add($"Dòng {row}: {ex.Message}");
                                result.ErrorCount++;
                            }
                        }

                        if (result.SuccessCount > 0)
                        {
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = $"Đã nhập {result.SuccessCount} sản phẩm thành công, {result.ErrorCount} lỗi",
                    result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi nhập Excel: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = $"Có lỗi xảy ra khi nhập Excel: {ex.Message}" });
            }
        }

        // GET: api/BulkProducts/template
        [HttpGet("template")]
        public IActionResult DownloadTemplate()
        {
            try
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Template");

                    // Thiết lập header đơn giản
                    string[] headers = new string[] { "Tên sản phẩm", "Danh mục", "Giá", "Số lượng", "Mô tả" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = headers[i];
                        worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    }

                    // Thêm dữ liệu mẫu
                    string[,] sampleData = new string[,]
                    {
                        { "Áo sơ mi nam", "Áo sơ mi", "350000", "10", "Áo sơ mi nam cao cấp" },
                        { "Quần tây nam", "Quần tây", "450000", "15", "Quần tây nam cao cấp" }
                    };

                    for (int row = 0; row < sampleData.GetLength(0); row++)
                    {
                        for (int col = 0; col < sampleData.GetLength(1); col++)
                        {
                            worksheet.Cells[row + 2, col + 1].Value = sampleData[row, col];
                        }
                    }

                    // Lưu file
                    var fileBytes = package.GetAsByteArray();
                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ProductImportTemplate.xlsx");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo template Excel: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = $"Có lỗi xảy ra khi tạo template Excel: {ex.Message}" });
            }
        }
    }

    public class UpdateStockModel
    {
        public int[] Ids { get; set; }
        public int Quantity { get; set; }
    }

    public class QuickEditModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class ImportResult
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
