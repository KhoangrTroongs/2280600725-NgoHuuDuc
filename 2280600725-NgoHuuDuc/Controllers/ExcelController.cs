using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Data;
using NgoHuuDuc_2280600725.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NgoHuuDuc_2280600725.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ExcelController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ExcelController> _logger;

        public ExcelController(
            ApplicationDbContext context,
            ILogger<ExcelController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Excel/ExportProducts
        public async Task<IActionResult> ExportProducts(string ids)
        {
            try
            {
                List<int> productIds = new List<int>();

                // Parse the ids string if provided
                if (!string.IsNullOrEmpty(ids))
                {
                    productIds = ids.Split(',')
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => int.Parse(s))
                        .ToList();
                }

                // If no ids provided, get all products
                if (productIds.Count == 0)
                {
                    productIds = await _context.Products.Select(p => p.Id).ToListAsync();
                }

                // Get products with their categories
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();

                if (products.Count == 0)
                {
                    return NotFound("Không tìm thấy sản phẩm nào");
                }

                // Tạo file Excel sử dụng EPPlus
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    // Tạo một worksheet
                    var worksheet = package.Workbook.Worksheets.Add("Products");

                    // Thiết lập header
                    worksheet.Cells[1, 1].Value = "ID";
                    worksheet.Cells[1, 2].Value = "Tên sản phẩm";
                    worksheet.Cells[1, 3].Value = "Danh mục";
                    worksheet.Cells[1, 4].Value = "Giá";
                    worksheet.Cells[1, 5].Value = "Tồn kho";
                    worksheet.Cells[1, 6].Value = "Mô tả";

                    // Định dạng header
                    using (var range = worksheet.Cells[1, 1, 1, 6])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        range.Style.Font.Color.SetColor(System.Drawing.Color.Black);
                    }

                    // Thêm dữ liệu
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

                    // Tự động điều chỉnh độ rộng cột
                    worksheet.Cells.AutoFitColumns();

                    // Tạo file Excel
                    var fileName = $"Products_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    var fileBytes = package.GetAsByteArray();

                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting products to Excel");
                return StatusCode(500, "Có lỗi xảy ra khi xuất Excel: " + ex.Message);
            }
        }

        // GET: Excel/DownloadTemplate
        public IActionResult DownloadTemplate()
        {
            try
            {
                // Tạo file Excel mẫu sử dụng EPPlus
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    // Tạo một worksheet
                    var worksheet = package.Workbook.Worksheets.Add("Template");

                    // Thiết lập header
                    worksheet.Cells[1, 1].Value = "Tên sản phẩm";
                    worksheet.Cells[1, 2].Value = "Danh mục";
                    worksheet.Cells[1, 3].Value = "Giá";
                    worksheet.Cells[1, 4].Value = "Số lượng";
                    worksheet.Cells[1, 5].Value = "Mô tả";

                    // Định dạng header
                    using (var range = worksheet.Cells[1, 1, 1, 5])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        range.Style.Font.Color.SetColor(System.Drawing.Color.Black);
                    }

                    // Thêm dữ liệu mẫu
                    worksheet.Cells[2, 1].Value = "Áo sơ mi nam";
                    worksheet.Cells[2, 2].Value = "Áo sơ mi";
                    worksheet.Cells[2, 3].Value = 350000;
                    worksheet.Cells[2, 4].Value = 10;
                    worksheet.Cells[2, 5].Value = "Áo sơ mi nam cao cấp";

                    worksheet.Cells[3, 1].Value = "Quần tây nam";
                    worksheet.Cells[3, 2].Value = "Quần tây";
                    worksheet.Cells[3, 3].Value = 450000;
                    worksheet.Cells[3, 4].Value = 15;
                    worksheet.Cells[3, 5].Value = "Quần tây nam cao cấp";

                    // Tự động điều chỉnh độ rộng cột
                    worksheet.Cells.AutoFitColumns();

                    // Tạo file Excel
                    var fileName = "ProductImportTemplate.xlsx";
                    var fileBytes = package.GetAsByteArray();

                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating template");
                return StatusCode(500, "Có lỗi xảy ra khi tạo template: " + ex.Message);
            }
        }

        // POST: Excel/ImportProducts
        [HttpPost]
        public async Task<IActionResult> ImportProducts(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                return BadRequest("Vui lòng chọn file Excel");
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Chỉ hỗ trợ file Excel (.xlsx)");
            }

            var successCount = 0;
            var errorCount = 0;
            var errors = new List<string>();
            var totalRows = 0;

            try
            {
                // Get all categories
                var categories = await _context.Categories.ToListAsync();

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    using (var package = new OfficeOpenXml.ExcelPackage(stream))
                    {
                        // Lấy worksheet đầu tiên
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            return BadRequest("File Excel không có dữ liệu");
                        }

                        // Đếm số dòng có dữ liệu
                        int rowCount = worksheet.Dimension?.Rows ?? 0;
                        if (rowCount <= 1) // Chỉ có header hoặc không có dữ liệu
                        {
                            return BadRequest("File Excel không có dữ liệu sản phẩm");
                        }

                        totalRows = rowCount - 1; // Trừ đi dòng header

                        // Đọc dữ liệu từ Excel (bắt đầu từ dòng 2, dòng 1 là header)
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

                                // Validate data
                                if (string.IsNullOrWhiteSpace(name))
                                {
                                    errors.Add($"Dòng {row}: Tên sản phẩm không được để trống");
                                    errorCount++;
                                    continue;
                                }

                                // Validate category
                                if (string.IsNullOrWhiteSpace(categoryName))
                                {
                                    errors.Add($"Dòng {row}: Danh mục không được để trống");
                                    errorCount++;
                                    continue;
                                }

                                // Find category
                                var category = categories.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
                                if (category == null)
                                {
                                    errors.Add($"Dòng {row}: Danh mục '{categoryName}' không tồn tại");
                                    errorCount++;
                                    continue;
                                }

                                // Parse price
                                decimal price;
                                if (priceValue == null || !decimal.TryParse(priceValue.ToString(), out price) || price < 0)
                                {
                                    errors.Add($"Dòng {row}: Giá không hợp lệ");
                                    errorCount++;
                                    continue;
                                }

                                // Parse quantity
                                int quantity;
                                if (quantityValue == null || !int.TryParse(quantityValue.ToString(), out quantity) || quantity < 0)
                                {
                                    errors.Add($"Dòng {row}: Số lượng không hợp lệ");
                                    errorCount++;
                                    continue;
                                }

                                // Create new product
                                var product = new Product
                                {
                                    Name = name,
                                    CategoryId = category.Id,
                                    Price = price,
                                    Quantity = quantity,
                                    Description = description ?? ""
                                };

                                _context.Products.Add(product);
                                successCount++;
                            }
                            catch (Exception ex)
                            {
                                errors.Add($"Dòng {row}: {ex.Message}");
                                errorCount++;
                            }
                        }
                    }
                }

                if (successCount > 0)
                {
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    success = true,
                    message = $"Đã nhập {successCount} sản phẩm thành công, {errorCount} lỗi",
                    totalRows,
                    successCount,
                    errorCount,
                    errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing products: {Message}", ex.Message);
                return StatusCode(500, "Có lỗi xảy ra khi nhập Excel: " + ex.Message);
            }
        }


    }
}
