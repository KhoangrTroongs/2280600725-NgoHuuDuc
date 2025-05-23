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
    [Route("[controller]")]
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

        // GET: Excel/ExportProductsSimple
        [HttpGet("ExportProductsSimple")]
        public async Task<IActionResult> ExportProductsSimple(
            string ids,
            int? categoryId = null,
            string stockFilter = null,
            string sortBy = null,
            decimal? priceMin = null,
            decimal? priceMax = null)
        {
            try
            {
                // Tạo query cơ bản
                var query = _context.Products.Include(p => p.Category).AsQueryable();

                // Áp dụng lọc theo ID nếu có
                if (!string.IsNullOrEmpty(ids))
                {
                    var productIds = ids.Split(',')
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => int.Parse(s))
                        .ToList();

                    if (productIds.Count > 0)
                    {
                        query = query.Where(p => productIds.Contains(p.Id));
                    }
                }

                // Áp dụng lọc theo danh mục
                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    query = query.Where(p => p.CategoryId == categoryId.Value);
                }

                // Áp dụng lọc theo tình trạng tồn kho
                if (!string.IsNullOrEmpty(stockFilter))
                {
                    if (stockFilter == "in-stock")
                    {
                        query = query.Where(p => p.Quantity > 0);
                    }
                    else if (stockFilter == "out-of-stock")
                    {
                        query = query.Where(p => p.Quantity <= 0);
                    }
                }

                // Áp dụng lọc theo giá
                if (priceMin.HasValue)
                {
                    query = query.Where(p => p.Price >= priceMin.Value);
                }

                if (priceMax.HasValue)
                {
                    query = query.Where(p => p.Price <= priceMax.Value);
                }

                // Áp dụng sắp xếp
                if (!string.IsNullOrEmpty(sortBy))
                {
                    switch (sortBy)
                    {
                        case "id-asc":
                            query = query.OrderBy(p => p.Id);
                            break;
                        case "id-desc":
                            query = query.OrderByDescending(p => p.Id);
                            break;
                        case "name-asc":
                            query = query.OrderBy(p => p.Name);
                            break;
                        case "name-desc":
                            query = query.OrderByDescending(p => p.Name);
                            break;
                        case "price-asc":
                            query = query.OrderBy(p => p.Price);
                            break;
                        case "price-desc":
                            query = query.OrderByDescending(p => p.Price);
                            break;
                        case "stock-asc":
                            query = query.OrderBy(p => p.Quantity);
                            break;
                        case "stock-desc":
                            query = query.OrderByDescending(p => p.Quantity);
                            break;
                        default:
                            query = query.OrderBy(p => p.Id);
                            break;
                    }
                }
                else
                {
                    // Mặc định sắp xếp theo ID
                    query = query.OrderBy(p => p.Id);
                }

                // Thực thi query để lấy danh sách sản phẩm
                var products = await query.ToListAsync();

                if (products.Count == 0)
                {
                    // Trả về thông báo lỗi dưới dạng JSON để JavaScript có thể xử lý
                    return Json(new {
                        success = false,
                        message = "Không tìm thấy sản phẩm nào thỏa mãn điều kiện lọc"
                    });
                }

                // Tạo file Excel sử dụng EPPlus
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    // Tạo một worksheet
                    var worksheet = package.Workbook.Worksheets.Add("Products");

                    // Thiết lập header
                    int col = 1;
                    worksheet.Cells[1, col++].Value = "ID";
                    worksheet.Cells[1, col++].Value = "Tên sản phẩm";
                    worksheet.Cells[1, col++].Value = "Danh mục";
                    worksheet.Cells[1, col++].Value = "Giá";
                    worksheet.Cells[1, col++].Value = "Tồn kho";

                    // Thêm cột kích thước
                    worksheet.Cells[1, col++].Value = "Kích thước";

                    // Định dạng header
                    using (var range = worksheet.Cells[1, 1, 1, col - 1])
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
                        col = 1;

                        worksheet.Cells[row, col++].Value = product.Id;
                        worksheet.Cells[row, col++].Value = product.Name;
                        worksheet.Cells[row, col++].Value = product.Category?.Name;
                        worksheet.Cells[row, col++].Value = product.Price;
                        worksheet.Cells[row, col++].Value = product.Quantity;

                        // Trích xuất thông tin kích thước
                        string sizeInfo = "Không có thông tin";
                        if (!string.IsNullOrEmpty(product.Description))
                        {
                            var sizeTag = "[SIZES]";
                            var endSizeTag = "[/SIZES]";

                            if (product.Description.Contains(sizeTag) && product.Description.Contains(endSizeTag))
                            {
                                var startIndex = product.Description.IndexOf(sizeTag) + sizeTag.Length;
                                var endIndex = product.Description.IndexOf(endSizeTag);
                                if (startIndex < endIndex)
                                {
                                    sizeInfo = product.Description.Substring(startIndex, endIndex - startIndex);
                                }
                            }
                        }

                        worksheet.Cells[row, col++].Value = sizeInfo;
                    }

                    // Tự động điều chỉnh độ rộng cột
                    worksheet.Cells.AutoFitColumns();

                    // Tạo tên file Excel với thông tin về bộ lọc
                    string fileNamePrefix = "Products";

                    // Thêm thông tin về danh mục vào tên file
                    if (categoryId.HasValue && categoryId.Value > 0)
                    {
                        var category = await _context.Categories.FindAsync(categoryId.Value);
                        if (category != null)
                        {
                            fileNamePrefix += $"_{category.Name.Replace(" ", "")}";
                        }
                    }

                    // Thêm thông tin về tình trạng tồn kho vào tên file
                    if (!string.IsNullOrEmpty(stockFilter))
                    {
                        fileNamePrefix += $"_{stockFilter}";
                    }

                    // Thêm thông tin về sắp xếp vào tên file
                    if (!string.IsNullOrEmpty(sortBy))
                    {
                        fileNamePrefix += $"_{sortBy}";
                    }

                    // Tạo file Excel
                    var fileName = $"{fileNamePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
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
        [HttpGet("DownloadTemplate")]
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
                    int col = 1;
                    worksheet.Cells[1, col++].Value = "Tên sản phẩm";
                    worksheet.Cells[1, col++].Value = "Danh mục";
                    worksheet.Cells[1, col++].Value = "Giá";

                    // Thêm cột kích thước
                    worksheet.Cells[1, col++].Value = "Size S";
                    worksheet.Cells[1, col++].Value = "Size M";
                    worksheet.Cells[1, col++].Value = "Size L";
                    worksheet.Cells[1, col++].Value = "Size XL";
                    worksheet.Cells[1, col++].Value = "Size 2XL";

                    worksheet.Cells[1, col++].Value = "Mô tả";

                    // Định dạng header
                    using (var range = worksheet.Cells[1, 1, 1, col - 1])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        range.Style.Font.Color.SetColor(System.Drawing.Color.Black);
                    }

                    // Thêm dữ liệu mẫu
                    // Sản phẩm 1
                    col = 1;
                    worksheet.Cells[2, col++].Value = "Áo sơ mi nam";
                    worksheet.Cells[2, col++].Value = "Áo sơ mi";
                    worksheet.Cells[2, col++].Value = 350000;
                    worksheet.Cells[2, col++].Value = 5;  // Size S
                    worksheet.Cells[2, col++].Value = 10; // Size M
                    worksheet.Cells[2, col++].Value = 15; // Size L
                    worksheet.Cells[2, col++].Value = 8;  // Size XL
                    worksheet.Cells[2, col++].Value = 3;  // Size 2XL
                    worksheet.Cells[2, col++].Value = "Áo sơ mi nam cao cấp, chất liệu cotton, phù hợp cho công sở";

                    // Sản phẩm 2
                    col = 1;
                    worksheet.Cells[3, col++].Value = "Quần tây nam";
                    worksheet.Cells[3, col++].Value = "Quần tây";
                    worksheet.Cells[3, col++].Value = 450000;
                    worksheet.Cells[3, col++].Value = 8;  // Size S
                    worksheet.Cells[3, col++].Value = 12; // Size M
                    worksheet.Cells[3, col++].Value = 15; // Size L
                    worksheet.Cells[3, col++].Value = 10; // Size XL
                    worksheet.Cells[3, col++].Value = 5;  // Size 2XL
                    worksheet.Cells[3, col++].Value = "Quần tây nam cao cấp, chất liệu kaki, phù hợp cho công sở và dự tiệc";

                    // Sản phẩm 3
                    col = 1;
                    worksheet.Cells[4, col++].Value = "Veston nam";
                    worksheet.Cells[4, col++].Value = "Veston";
                    worksheet.Cells[4, col++].Value = 1500000;
                    worksheet.Cells[4, col++].Value = 3;  // Size S
                    worksheet.Cells[4, col++].Value = 7;  // Size M
                    worksheet.Cells[4, col++].Value = 10; // Size L
                    worksheet.Cells[4, col++].Value = 5;  // Size XL
                    worksheet.Cells[4, col++].Value = 2;  // Size 2XL
                    worksheet.Cells[4, col++].Value = "Veston nam cao cấp, chất liệu wool, phù hợp cho các dịp trang trọng";

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
        [HttpPost("ImportProducts")]
        public async Task<IActionResult> ImportProducts(IFormFile file, bool updateExisting = true, bool skipErrors = true)
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
            var updatedCount = 0;
            var newCount = 0;

            try
            {
                // Get all categories
                var categories = await _context.Categories.ToListAsync();

                // Get all existing products for update check
                var existingProducts = new Dictionary<string, Product>();
                if (updateExisting)
                {
                    var products = await _context.Products.ToListAsync();
                    foreach (var product in products)
                    {
                        existingProducts[product.Name.ToLower()] = product;
                    }
                }

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

                        // Kiểm tra định dạng file Excel
                        bool hasSizeColumns = false;
                        int descriptionColumnIndex = 5; // Mặc định là cột thứ 5

                        // Kiểm tra xem có cột kích thước không
                        var headers = new List<string>();
                        for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                        {
                            var header = worksheet.Cells[1, col].Value?.ToString();
                            if (!string.IsNullOrEmpty(header))
                            {
                                headers.Add(header);
                            }
                        }

                        // Kiểm tra xem có cột kích thước không
                        hasSizeColumns = headers.Any(h => h.StartsWith("Size "));

                        // Nếu có cột kích thước, mô tả sẽ ở cột cuối cùng
                        if (hasSizeColumns)
                        {
                            descriptionColumnIndex = headers.Count;
                        }

                        // Đọc dữ liệu từ Excel (bắt đầu từ dòng 2, dòng 1 là header)
                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                // Đọc dữ liệu từ các ô
                                var name = worksheet.Cells[row, 1].Value?.ToString();
                                var categoryName = worksheet.Cells[row, 2].Value?.ToString();
                                var priceValue = worksheet.Cells[row, 3].Value;
                                var description = worksheet.Cells[row, descriptionColumnIndex].Value?.ToString();

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

                                // Xử lý kích thước và số lượng
                                int totalQuantity = 0;
                                string sizeInfo = "";

                                if (hasSizeColumns)
                                {
                                    // Đọc thông tin kích thước từ các cột
                                    var sizeQuantities = new Dictionary<string, int>();

                                    // Tìm các cột kích thước
                                    for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                                    {
                                        var header = worksheet.Cells[1, col].Value?.ToString();
                                        if (!string.IsNullOrEmpty(header) && header.StartsWith("Size "))
                                        {
                                            var size = header.Substring(5); // Lấy phần sau "Size "
                                            var quantityValue = worksheet.Cells[row, col].Value;

                                            if (quantityValue != null && int.TryParse(quantityValue.ToString(), out int sizeQuantity) && sizeQuantity >= 0)
                                            {
                                                sizeQuantities[size] = sizeQuantity;
                                                totalQuantity += sizeQuantity;
                                            }
                                        }
                                    }

                                    // Tạo chuỗi thông tin kích thước
                                    if (sizeQuantities.Count > 0)
                                    {
                                        sizeInfo = string.Join(",", sizeQuantities.Select(kv => $"{kv.Key}:{kv.Value}"));
                                    }
                                }
                                else
                                {
                                    // Nếu không có cột kích thước, đọc số lượng từ cột thứ 4
                                    var quantityValue = worksheet.Cells[row, 4].Value;
                                    if (quantityValue == null || !int.TryParse(quantityValue.ToString(), out totalQuantity) || totalQuantity < 0)
                                    {
                                        errors.Add($"Dòng {row}: Số lượng không hợp lệ");
                                        errorCount++;
                                        continue;
                                    }
                                }

                                // Kiểm tra sản phẩm đã tồn tại chưa
                                bool isUpdate = false;
                                Product product;

                                if (updateExisting && existingProducts.TryGetValue(name.ToLower(), out product))
                                {
                                    // Cập nhật sản phẩm đã tồn tại
                                    product.CategoryId = category.Id;
                                    product.Price = price;
                                    product.Quantity = totalQuantity;

                                    // Cập nhật mô tả và thông tin kích thước
                                    if (!string.IsNullOrEmpty(description))
                                    {
                                        // Kiểm tra xem mô tả đã có thông tin kích thước chưa
                                        if (hasSizeColumns && !string.IsNullOrEmpty(sizeInfo))
                                        {
                                            var sizeTag = "[SIZES]";
                                            var endSizeTag = "[/SIZES]";

                                            if (description.Contains(sizeTag) && description.Contains(endSizeTag))
                                            {
                                                // Cập nhật phần kích cỡ trong mô tả
                                                var startIndex = description.IndexOf(sizeTag) + sizeTag.Length;
                                                var endIndex = description.IndexOf(endSizeTag);
                                                description = description.Substring(0, startIndex) + sizeInfo + description.Substring(endIndex);
                                            }
                                            else
                                            {
                                                // Thêm phần kích cỡ vào cuối mô tả
                                                description += $"\n\n{sizeTag}{sizeInfo}{endSizeTag}";
                                            }
                                        }

                                        product.Description = description;
                                    }

                                    isUpdate = true;
                                    updatedCount++;
                                }
                                else
                                {
                                    // Tạo sản phẩm mới
                                    string finalDescription = description ?? "";

                                    // Thêm thông tin kích thước vào mô tả nếu có
                                    if (hasSizeColumns && !string.IsNullOrEmpty(sizeInfo))
                                    {
                                        finalDescription += $"\n\n[SIZES]{sizeInfo}[/SIZES]";
                                    }

                                    product = new Product
                                    {
                                        Name = name,
                                        CategoryId = category.Id,
                                        Price = price,
                                        Quantity = totalQuantity,
                                        Description = finalDescription
                                    };

                                    _context.Products.Add(product);
                                    newCount++;
                                }

                                successCount++;
                            }
                            catch (Exception ex)
                            {
                                errors.Add($"Dòng {row}: {ex.Message}");
                                errorCount++;

                                // Nếu không bỏ qua lỗi, dừng quá trình nhập
                                if (!skipErrors)
                                {
                                    break;
                                }
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
                    message = $"Đã nhập {successCount} sản phẩm thành công ({newCount} mới, {updatedCount} cập nhật), {errorCount} lỗi",
                    totalRows,
                    successCount,
                    newCount,
                    updatedCount,
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
