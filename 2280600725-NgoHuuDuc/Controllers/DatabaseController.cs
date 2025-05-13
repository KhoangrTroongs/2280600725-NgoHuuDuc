using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Data;
using System.IO;
using System.Threading.Tasks;

namespace NgoHuuDuc_2280600725.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class DatabaseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<DatabaseController> _logger;

        public DatabaseController(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            ILogger<DatabaseController> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateTables()
        {
            try
            {
                // Kiểm tra xem bảng ProductSizes đã tồn tại chưa
                var productSizesExists = false;
                try
                {
                    var result = await _context.Database.ExecuteSqlRawAsync(
                        "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ProductSizes'");
                    productSizesExists = result > 0;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking if ProductSizes table exists");
                    productSizesExists = false;
                }

                if (!productSizesExists)
                {
                    try
                    {
                        // Tạo bảng ProductSizes
                        await _context.Database.ExecuteSqlRawAsync(@"
                            CREATE TABLE ProductSizes (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                ProductId INT NOT NULL,
                                Size NVARCHAR(50) NOT NULL,
                                Quantity INT NOT NULL DEFAULT 0,
                                CONSTRAINT FK_ProductSizes_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
                            )");

                        _logger.LogInformation("ProductSizes table created successfully.");
                        TempData["Success"] = "ProductSizes table created successfully.";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating ProductSizes table");
                        TempData["Error"] = "Error creating ProductSizes table: " + ex.Message;
                    }
                }
                else
                {
                    _logger.LogInformation("ProductSizes table already exists.");
                    TempData["Info"] = "ProductSizes table already exists.";
                }

                // Kiểm tra xem bảng ProductReviews đã tồn tại chưa
                var productReviewsExists = false;
                try
                {
                    var result = await _context.Database.ExecuteSqlRawAsync(
                        "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ProductReviews'");
                    productReviewsExists = result > 0;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking if ProductReviews table exists");
                    productReviewsExists = false;
                }

                if (!productReviewsExists)
                {
                    try
                    {
                        // Tạo bảng ProductReviews
                        await _context.Database.ExecuteSqlRawAsync(@"
                            CREATE TABLE ProductReviews (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                ProductId INT NOT NULL,
                                UserId NVARCHAR(450) NOT NULL,
                                Rating INT NOT NULL,
                                Comment NVARCHAR(MAX) NULL,
                                CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                                CONSTRAINT FK_ProductReviews_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
                                CONSTRAINT FK_ProductReviews_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                            )");

                        _logger.LogInformation("ProductReviews table created successfully.");
                        TempData["Success"] = (TempData["Success"] ?? "") + " ProductReviews table created successfully.";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating ProductReviews table");
                        TempData["Error"] = (TempData["Error"] ?? "") + " Error creating ProductReviews table: " + ex.Message;
                    }
                }
                else
                {
                    _logger.LogInformation("ProductReviews table already exists.");
                    TempData["Info"] = (TempData["Info"] ?? "") + " ProductReviews table already exists.";
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tables");
                TempData["Error"] = "Error creating tables: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
